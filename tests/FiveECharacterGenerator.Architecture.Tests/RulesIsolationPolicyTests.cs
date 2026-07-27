using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using FiveECharacterGenerator.Rules;

namespace FiveECharacterGenerator.Architecture.Tests;

/// <summary>
/// Verifies that the Rules assembly remains isolated from infrastructure,
/// ambient state, and mutable global state.
/// </summary>
public sealed class RulesIsolationPolicyTests
{
    /// <summary>
    /// Verifies that Rules does not reference prohibited infrastructure types.
    /// </summary>
    [Fact]
    public void RulesAssemblyDoesNotReferenceForbiddenInfrastructureTypes()
    {
        using FileStream assemblyStream =
            File.OpenRead(GetRulesAssemblyPath());

        using var peReader = new PEReader(assemblyStream);
        MetadataReader metadataReader = peReader.GetMetadataReader();
        var violations = new List<string>();

        foreach (
            TypeReferenceHandle typeReferenceHandle
            in metadataReader.TypeReferences)
        {
            TypeReference typeReference =
                metadataReader.GetTypeReference(typeReferenceHandle);

            string fullName = CreateFullTypeName(
                metadataReader.GetString(typeReference.Namespace),
                metadataReader.GetString(typeReference.Name));

            if (IsForbiddenTypeReference(fullName))
            {
                violations.Add(fullName);
            }
        }

        string[] distinctViolations = violations
            .Distinct(StringComparer.Ordinal)
            .OrderBy(
                static violation => violation,
                StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(distinctViolations);
    }

    /// <summary>
    /// Verifies that Rules does not read ambient system time.
    /// </summary>
    [Fact]
    public void RulesAssemblyDoesNotReadAmbientSystemTime()
    {
        using FileStream assemblyStream =
            File.OpenRead(GetRulesAssemblyPath());

        using var peReader = new PEReader(assemblyStream);
        MetadataReader metadataReader = peReader.GetMetadataReader();
        var violations = new List<string>();

        foreach (
            MemberReferenceHandle memberReferenceHandle
            in metadataReader.MemberReferences)
        {
            MemberReference memberReference =
                metadataReader.GetMemberReference(memberReferenceHandle);

            string? declaringTypeName = TryGetTypeFullName(
                metadataReader,
                memberReference.Parent);

            if (declaringTypeName is null)
            {
                continue;
            }

            string memberName =
                metadataReader.GetString(memberReference.Name);

            if (IsForbiddenAmbientTimeMember(
                    declaringTypeName,
                    memberName))
            {
                violations.Add(
                    $"{declaringTypeName}.{memberName}");
            }
        }

        string[] distinctViolations = violations
            .Distinct(StringComparer.Ordinal)
            .OrderBy(
                static violation => violation,
                StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(distinctViolations);
    }

    /// <summary>
    /// Verifies that Rules declares no user-authored mutable static fields.
    /// </summary>
    [Fact]
    public void RulesAssemblyHasNoUserAuthoredMutableStaticFields()
    {
        Assembly rulesAssembly = typeof(AbilityModifierRules).Assembly;

        string[] violations = rulesAssembly
            .GetTypes()
            .Where(
                static type =>
                    !IsCompilerGenerated(type))
            .SelectMany(
                static type => type.GetFields(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static |
                    BindingFlags.DeclaredOnly))
            .Where(
                static field =>
                    !field.IsLiteral &&
                    !field.IsInitOnly &&
                    !IsCompilerGenerated(field))
            .Select(
                static field =>
                    $"{field.DeclaringType!.FullName}.{field.Name}")
            .OrderBy(
                static violation => violation,
                StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verifies that the infrastructure detector recognizes representative
    /// prohibited dependencies.
    /// </summary>
    /// <param name="fullTypeName">The prohibited type name.</param>
    [Theory]
    [InlineData("System.IO.File")]
    [InlineData("System.Net.Http.HttpClient")]
    [InlineData("System.Data.DataSet")]
    [InlineData("System.Environment")]
    [InlineData("System.Random")]
    [InlineData("System.TimeProvider")]
    [InlineData("Microsoft.EntityFrameworkCore.DbContext")]
    [InlineData("Microsoft.Extensions.Configuration.IConfiguration")]
    [InlineData("System.Windows.Window")]
    [InlineData("Avalonia.Application")]
    [InlineData("Godot.Node")]
    public void InfrastructureDetectorRecognizesForbiddenTypes(
        string fullTypeName)
    {
        Assert.True(IsForbiddenTypeReference(fullTypeName));
    }

    /// <summary>
    /// Verifies that the infrastructure detector permits representative
    /// rules-safe framework types.
    /// </summary>
    /// <param name="fullTypeName">The permitted type name.</param>
    [Theory]
    [InlineData("System.ArgumentException")]
    [InlineData("System.Collections.Generic.IReadOnlyList")]
    [InlineData("System.Linq.Enumerable")]
    [InlineData("System.String")]
    public void InfrastructureDetectorPermitsRulesSafeTypes(
        string fullTypeName)
    {
        Assert.False(IsForbiddenTypeReference(fullTypeName));
    }

    /// <summary>
    /// Verifies that the ambient-time detector recognizes prohibited clock
    /// access while permitting ordinary date-time members.
    /// </summary>
    [Fact]
    public void AmbientTimeDetectorRecognizesClockAccess()
    {
        Assert.True(
            IsForbiddenAmbientTimeMember(
                "System.DateTime",
                "get_Now"));

        Assert.True(
            IsForbiddenAmbientTimeMember(
                "System.DateTime",
                "get_UtcNow"));

        Assert.True(
            IsForbiddenAmbientTimeMember(
                "System.DateTimeOffset",
                "get_Now"));

        Assert.False(
            IsForbiddenAmbientTimeMember(
                "System.DateTime",
                "get_Year"));
    }

    private static string GetRulesAssemblyPath()
    {
        return typeof(AbilityModifierRules).Assembly.Location;
    }

    private static bool IsForbiddenTypeReference(string fullTypeName)
    {
        string[] forbiddenNamespacePrefixes =
        {
            "System.IO",
            "System.Net",
            "System.Data",
            "Microsoft.Data",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.Extensions.Configuration",
            "Microsoft.Extensions.Hosting",
            "System.Windows",
            "Microsoft.Maui",
            "Avalonia",
            "Godot",
        };

        string[] forbiddenExactTypes =
        {
            "System.Environment",
            "System.Random",
            "System.TimeProvider",
            "System.Diagnostics.Stopwatch",
            "System.Security.Cryptography.RandomNumberGenerator",
        };

        if (forbiddenExactTypes.Contains(
                fullTypeName,
                StringComparer.Ordinal))
        {
            return true;
        }

        return forbiddenNamespacePrefixes.Any(
            prefix =>
                fullTypeName.Equals(
                    prefix,
                    StringComparison.Ordinal) ||
                fullTypeName.StartsWith(
                    prefix + ".",
                    StringComparison.Ordinal));
    }

    private static bool IsForbiddenAmbientTimeMember(
        string declaringTypeName,
        string memberName)
    {
        if (declaringTypeName == "System.DateTime")
        {
            return memberName is
                "get_Now" or
                "get_Today" or
                "get_UtcNow";
        }

        if (declaringTypeName == "System.DateTimeOffset")
        {
            return memberName is
                "get_Now" or
                "get_UtcNow";
        }

        return false;
    }

    private static string? TryGetTypeFullName(
        MetadataReader metadataReader,
        EntityHandle entityHandle)
    {
        return entityHandle.Kind switch
        {
            HandleKind.TypeReference =>
                CreateTypeReferenceFullName(
                    metadataReader,
                    (TypeReferenceHandle)entityHandle),

            HandleKind.TypeDefinition =>
                CreateTypeDefinitionFullName(
                    metadataReader,
                    (TypeDefinitionHandle)entityHandle),

            _ => null,
        };
    }

    private static string CreateTypeReferenceFullName(
        MetadataReader metadataReader,
        TypeReferenceHandle typeReferenceHandle)
    {
        TypeReference typeReference =
            metadataReader.GetTypeReference(typeReferenceHandle);

        return CreateFullTypeName(
            metadataReader.GetString(typeReference.Namespace),
            metadataReader.GetString(typeReference.Name));
    }

    private static string CreateTypeDefinitionFullName(
        MetadataReader metadataReader,
        TypeDefinitionHandle typeDefinitionHandle)
    {
        TypeDefinition typeDefinition =
            metadataReader.GetTypeDefinition(typeDefinitionHandle);

        return CreateFullTypeName(
            metadataReader.GetString(typeDefinition.Namespace),
            metadataReader.GetString(typeDefinition.Name));
    }

    private static string CreateFullTypeName(
        string typeNamespace,
        string typeName)
    {
        return string.IsNullOrEmpty(typeNamespace)
            ? typeName
            : $"{typeNamespace}.{typeName}";
    }

    private static bool IsCompilerGenerated(MemberInfo member)
    {
        return member.IsDefined(
            typeof(CompilerGeneratedAttribute),
            inherit: false);
    }
}
