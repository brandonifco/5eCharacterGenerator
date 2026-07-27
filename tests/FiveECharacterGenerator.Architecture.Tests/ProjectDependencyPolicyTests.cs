using System.Xml.Linq;

namespace FiveECharacterGenerator.Architecture.Tests;

/// <summary>
/// Verifies the approved production-project dependency policy.
/// </summary>
public sealed class ProjectDependencyPolicyTests
{
    private const string SolutionFileName = "FiveECharacterGenerator.sln";

    /// <summary>
    /// Verifies that the solution contains every approved production project.
    /// </summary>
    [Fact]
    public void SolutionContainsEveryExpectedProductionProject()
    {
        string repositoryRoot = FindRepositoryRoot();
        string solutionPath = Path.Combine(repositoryRoot, SolutionFileName);
        string solutionText = File.ReadAllText(solutionPath);

        foreach (string projectName in CreateExpectedReferences().Keys)
        {
            string expectedProjectPath =
                $@"src\{projectName}\{projectName}.csproj";

            Assert.True(
                solutionText.Contains(
                    expectedProjectPath,
                    StringComparison.OrdinalIgnoreCase),
                $"The solution does not contain {expectedProjectPath}.");
        }
    }

    /// <summary>
    /// Verifies that each production project has exactly its approved references.
    /// </summary>
    [Fact]
    public void ProductionProjectsHaveExactlyTheApprovedReferences()
    {
        string repositoryRoot = FindRepositoryRoot();

        foreach (
            KeyValuePair<string, HashSet<string>> projectPolicy
            in CreateExpectedReferences())
        {
            string projectPath = Path.Combine(
                repositoryRoot,
                "src",
                projectPolicy.Key,
                $"{projectPolicy.Key}.csproj");

            Assert.True(
                File.Exists(projectPath),
                $"Expected production project was not found: {projectPath}");

            string[] expectedReferences = projectPolicy.Value
                .OrderBy(
                    static projectName => projectName,
                    StringComparer.Ordinal)
                .ToArray();

            string[] actualReferences = ReadProjectReferenceNames(projectPath)
                .OrderBy(
                    static projectName => projectName,
                    StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(expectedReferences, actualReferences);
        }
    }

    /// <summary>
    /// Verifies that the dependency-policy checker identifies a forbidden reference.
    /// </summary>
    [Fact]
    public void DependencyPolicyReportsAForbiddenReference()
    {
        Dictionary<string, HashSet<string>> expectedReferences =
            CreateExpectedReferences();

        Dictionary<string, HashSet<string>> actualReferences =
            CreateExpectedReferences();

        actualReferences["FiveECharacterGenerator.CharacterState"].Add(
            "FiveECharacterGenerator.Persistence");

        string[] violations = FindForbiddenReferences(
            expectedReferences,
            actualReferences);

        Assert.Contains(
            "FiveECharacterGenerator.CharacterState -> " +
            "FiveECharacterGenerator.Persistence",
            violations);
    }

    private static Dictionary<string, HashSet<string>>
        CreateExpectedReferences()
    {
        return new Dictionary<string, HashSet<string>>(
            StringComparer.Ordinal)
        {
            ["FiveECharacterGenerator.CharacterState"] = new(
                StringComparer.Ordinal),

            ["FiveECharacterGenerator.Rules"] = new(
                StringComparer.Ordinal)
            {
                "FiveECharacterGenerator.CharacterState",
            },

            ["FiveECharacterGenerator.Generation"] = new(
                StringComparer.Ordinal)
            {
                "FiveECharacterGenerator.CharacterState",
                "FiveECharacterGenerator.Rules",
            },

            ["FiveECharacterGenerator.Application"] = new(
                StringComparer.Ordinal)
            {
                "FiveECharacterGenerator.CharacterState",
                "FiveECharacterGenerator.Generation",
                "FiveECharacterGenerator.Rules",
            },

            ["FiveECharacterGenerator.Persistence"] = new(
                StringComparer.Ordinal)
            {
                "FiveECharacterGenerator.Application",
            },

            ["FiveECharacterGenerator.Desktop"] = new(
                StringComparer.Ordinal)
            {
                "FiveECharacterGenerator.Application",
                "FiveECharacterGenerator.Persistence",
            },
        };
    }

    private static string[] FindForbiddenReferences(
        Dictionary<string, HashSet<string>> expectedReferences,
        Dictionary<string, HashSet<string>> actualReferences)
    {
        var violations = new List<string>();

        foreach (
            KeyValuePair<string, HashSet<string>> actualProject
            in actualReferences)
        {
            if (!expectedReferences.TryGetValue(
                    actualProject.Key,
                    out HashSet<string>? allowedReferences))
            {
                foreach (string actualReference in actualProject.Value)
                {
                    violations.Add(
                        $"{actualProject.Key} -> {actualReference}");
                }

                continue;
            }

            foreach (string actualReference in actualProject.Value)
            {
                if (!allowedReferences.Contains(actualReference))
                {
                    violations.Add(
                        $"{actualProject.Key} -> {actualReference}");
                }
            }
        }

        return violations
            .OrderBy(
                static violation => violation,
                StringComparer.Ordinal)
            .ToArray();
    }

    private static HashSet<string> ReadProjectReferenceNames(
        string projectPath)
    {
        XDocument projectDocument = XDocument.Load(
            projectPath,
            LoadOptions.None);

        var referenceNames = new HashSet<string>(
            StringComparer.Ordinal);

        foreach (
            XElement projectReference
            in projectDocument
                .Descendants()
                .Where(
                    static element =>
                        element.Name.LocalName == "ProjectReference"))
        {
            string? includePath =
                projectReference.Attribute("Include")?.Value;

            if (string.IsNullOrWhiteSpace(includePath))
            {
                continue;
            }

            string? projectName =
                Path.GetFileNameWithoutExtension(includePath);

            if (!string.IsNullOrWhiteSpace(projectName))
            {
                referenceNames.Add(projectName);
            }
        }

        return referenceNames;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? currentDirectory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string solutionPath = Path.Combine(
                currentDirectory.FullName,
                SolutionFileName);

            string gitDirectory = Path.Combine(
                currentDirectory.FullName,
                ".git");

            if (File.Exists(solutionPath) &&
                Directory.Exists(gitDirectory))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException(
            "The repository root could not be located.");
    }
}
