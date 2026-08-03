using System.Globalization;
using System.Xml.Linq;

namespace FiveECharacterGenerator.Architecture.Tests;

/// <summary>
/// Verifies the closed production-project inventory and dependency policy.
/// </summary>
public sealed class ProjectDependencyPolicyTests
{
    private const string SolutionFileName = "FiveECharacterGenerator.sln";

    /// <summary>
    /// Verifies the repository's complete production-project inventory,
    /// solution membership, and direct project references.
    /// </summary>
    [Fact]
    public void ProductionProjectsFollowTheClosedPolicy()
    {
        string repositoryRoot = FindRepositoryRoot();
        string[] violations = FindPolicyViolations(repositoryRoot);

        Assert.True(
            violations.Length == 0,
            CreateFailureMessage(violations));
    }

    /// <summary>
    /// Verifies that an unknown production project is rejected.
    /// </summary>
    [Fact]
    public void DetectorReportsAnUnknownProductionProject()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.WriteProject(
            "FiveECharacterGenerator.Extension",
            "FiveECharacterGenerator.CharacterState");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Unknown production project under src: " +
            "FiveECharacterGenerator.Extension " +
            "(src/FiveECharacterGenerator.Extension/" +
            "FiveECharacterGenerator.Extension.csproj).",
            violations);
    }

    /// <summary>
    /// Verifies that a missing approved production project is rejected.
    /// </summary>
    [Fact]
    public void DetectorReportsAMissingApprovedProductionProject()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.DeleteProject(
            "FiveECharacterGenerator.Generation");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Approved production project missing from src: " +
            "FiveECharacterGenerator.Generation.",
            violations);
    }

    /// <summary>
    /// Verifies that a forbidden direct project reference is rejected.
    /// </summary>
    [Fact]
    public void DetectorReportsAForbiddenProjectReference()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.WriteProject(
            "FiveECharacterGenerator.CharacterState",
            "FiveECharacterGenerator.Persistence");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Forbidden project reference: " +
            "FiveECharacterGenerator.CharacterState -> " +
            "FiveECharacterGenerator.Persistence.",
            violations);
    }

    /// <summary>
    /// Verifies that an unknown project cannot evade enforcement by having no
    /// direct references.
    /// </summary>
    [Fact]
    public void DetectorReportsAnUnknownProjectWithNoReferences()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.WriteProject(
            "FiveECharacterGenerator.EmptyExtension");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Unknown production project under src: " +
            "FiveECharacterGenerator.EmptyExtension " +
            "(src/FiveECharacterGenerator.EmptyExtension/" +
            "FiveECharacterGenerator.EmptyExtension.csproj).",
            violations);
    }

    /// <summary>
    /// Verifies that duplicate production-project identities are rejected.
    /// </summary>
    [Fact]
    public void DetectorReportsADuplicateProductionProjectIdentity()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.WriteProjectAt(
            "src/Duplicate/FiveECharacterGenerator.Rules.csproj",
            "FiveECharacterGenerator.CharacterState");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Duplicate production-project identity under src: " +
            "FiveECharacterGenerator.Rules " +
            "(FiveECharacterGenerator.Rules at " +
            "src/Duplicate/FiveECharacterGenerator.Rules.csproj, " +
            "FiveECharacterGenerator.Rules at " +
            "src/FiveECharacterGenerator.Rules/" +
            "FiveECharacterGenerator.Rules.csproj).",
            violations);
    }

    /// <summary>
    /// Verifies that duplicate production-project solution entries are
    /// rejected.
    /// </summary>
    [Fact]
    public void DetectorReportsADuplicateSolutionEntry()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.AddSolutionEntry(
            "FiveECharacterGenerator.Rules",
            "src/FiveECharacterGenerator.Rules/" +
            "FiveECharacterGenerator.Rules.csproj");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Duplicate production-project entries in solution: " +
            "FiveECharacterGenerator.Rules " +
            "(src/FiveECharacterGenerator.Rules/" +
            "FiveECharacterGenerator.Rules.csproj, " +
            "src/FiveECharacterGenerator.Rules/" +
            "FiveECharacterGenerator.Rules.csproj).",
            violations);
    }

    /// <summary>
    /// Verifies that a missing required direct project reference is rejected.
    /// </summary>
    [Fact]
    public void DetectorReportsAMissingRequiredProjectReference()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.WriteProject(
            "FiveECharacterGenerator.Persistence");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Missing required project reference: " +
            "FiveECharacterGenerator.Persistence -> " +
            "FiveECharacterGenerator.Application.",
            violations);
    }

    /// <summary>
    /// Verifies that a direct reference to an unknown production project is
    /// rejected.
    /// </summary>
    [Fact]
    public void DetectorReportsAReferenceToAnUnknownProductionProject()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.WriteProject(
            "FiveECharacterGenerator.Extension");
        repository.WriteProject(
            "FiveECharacterGenerator.CharacterState",
            "FiveECharacterGenerator.Extension");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Reference to unknown production project: " +
            "FiveECharacterGenerator.CharacterState -> " +
            "FiveECharacterGenerator.Extension.",
            violations);
    }

    /// <summary>
    /// Verifies that an approved production project missing from the solution
    /// is rejected.
    /// </summary>
    [Fact]
    public void DetectorReportsAnApprovedProjectMissingFromTheSolution()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.RemoveSolutionEntries(
            "FiveECharacterGenerator.Persistence");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Approved production project missing from solution: " +
            "FiveECharacterGenerator.Persistence " +
            "(src/FiveECharacterGenerator.Persistence/" +
            "FiveECharacterGenerator.Persistence.csproj).",
            violations);
    }

    /// <summary>
    /// Verifies that an unknown source project represented in the solution is
    /// rejected.
    /// </summary>
    [Fact]
    public void DetectorReportsAnUnknownSourceProjectInTheSolution()
    {
        using TemporaryRepository repository =
            TemporaryRepository.CreateValid();

        repository.WriteProject(
            "FiveECharacterGenerator.Extension");
        repository.AddSolutionEntry(
            "FiveECharacterGenerator.Extension",
            "src/FiveECharacterGenerator.Extension/" +
            "FiveECharacterGenerator.Extension.csproj");

        string[] violations = FindPolicyViolations(repository.RootPath);

        Assert.Contains(
            "Unknown production project from src is represented in solution: " +
            "FiveECharacterGenerator.Extension " +
            "(src/FiveECharacterGenerator.Extension/" +
            "FiveECharacterGenerator.Extension.csproj).",
            violations);
    }

    private static string[] FindPolicyViolations(
        string repositoryRoot)
    {
        Dictionary<string, HashSet<string>> approvedReferences =
            CreateApprovedReferences();

        ProductionProject[] productionProjects =
            DiscoverProductionProjects(repositoryRoot);

        SolutionProjectEntry[] solutionProjects =
            ReadSolutionProductionProjects(repositoryRoot);

        var violations = new HashSet<string>(StringComparer.Ordinal);

        AddInventoryViolations(
            approvedReferences,
            productionProjects,
            violations);

        AddSolutionViolations(
            approvedReferences,
            productionProjects,
            solutionProjects,
            violations);

        AddDependencyViolations(
            approvedReferences,
            productionProjects,
            violations);

        return violations
            .OrderBy(
                static violation => violation,
                StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddInventoryViolations(
        Dictionary<string, HashSet<string>> approvedReferences,
        ProductionProject[] productionProjects,
        HashSet<string> violations)
    {
        foreach (string approvedProjectName in approvedReferences.Keys)
        {
            if (!productionProjects.Any(
                    project => string.Equals(
                        project.Name,
                        approvedProjectName,
                        StringComparison.OrdinalIgnoreCase)))
            {
                violations.Add(
                    "Approved production project missing from src: " +
                    $"{approvedProjectName}.");
            }
        }

        foreach (ProductionProject productionProject in productionProjects)
        {
            if (!approvedReferences.ContainsKey(productionProject.Name))
            {
                violations.Add(
                    "Unknown production project under src: " +
                    $"{productionProject.Name} " +
                    $"({productionProject.RelativePath}).");
            }
        }

        foreach (
            IGrouping<string, ProductionProject> duplicateIdentity
            in productionProjects
                .GroupBy(
                    static project => project.Name,
                    StringComparer.OrdinalIgnoreCase)
                .Where(static group => group.Count() > 1)
                .OrderBy(
                    static group => group.Key,
                    StringComparer.Ordinal))
        {
            string projects = string.Join(
                ", ",
                duplicateIdentity
                    .OrderBy(
                        static project => project.RelativePath,
                        StringComparer.Ordinal)
                    .Select(
                        static project =>
                            $"{project.Name} at {project.RelativePath}"));

            violations.Add(
                "Duplicate production-project identity under src: " +
                $"{duplicateIdentity.Key} ({projects}).");
        }
    }

    private static void AddSolutionViolations(
        Dictionary<string, HashSet<string>> approvedReferences,
        ProductionProject[] productionProjects,
        SolutionProjectEntry[] solutionProjects,
        HashSet<string> violations)
    {
        foreach (
            IGrouping<string, SolutionProjectEntry> duplicateIdentity
            in solutionProjects
                .GroupBy(
                    static project => project.ProjectIdentity,
                    StringComparer.OrdinalIgnoreCase)
                .Where(static group => group.Count() > 1)
                .OrderBy(
                    static group => group.Key,
                    StringComparer.Ordinal))
        {
            string paths = string.Join(
                ", ",
                duplicateIdentity
                    .OrderBy(
                        static project => project.RelativePath,
                        StringComparer.Ordinal)
                    .Select(static project => project.RelativePath));

            violations.Add(
                "Duplicate production-project entries in solution: " +
                $"{duplicateIdentity.Key} ({paths}).");
        }

        foreach (ProductionProject productionProject in productionProjects)
        {
            bool isApproved =
                approvedReferences.ContainsKey(productionProject.Name);

            bool hasMatchingSolutionEntry = solutionProjects.Any(
                solutionProject => string.Equals(
                    solutionProject.RelativePath,
                    productionProject.RelativePath,
                    StringComparison.OrdinalIgnoreCase));

            if (isApproved && !hasMatchingSolutionEntry)
            {
                violations.Add(
                    "Approved production project missing from solution: " +
                    $"{productionProject.Name} " +
                    $"({productionProject.RelativePath}).");
            }

            if (!isApproved && hasMatchingSolutionEntry)
            {
                violations.Add(
                    "Unknown production project from src is represented " +
                    $"in solution: {productionProject.Name} " +
                    $"({productionProject.RelativePath}).");
            }
        }

        foreach (SolutionProjectEntry solutionProject in solutionProjects)
        {
            bool matchesDiscoveredProject = productionProjects.Any(
                productionProject => string.Equals(
                    productionProject.RelativePath,
                    solutionProject.RelativePath,
                    StringComparison.OrdinalIgnoreCase));

            if (!matchesDiscoveredProject)
            {
                violations.Add(
                    "Solution production-project entry does not match a " +
                    "discovered project under src: " +
                    $"{solutionProject.ProjectIdentity} " +
                    $"({solutionProject.RelativePath}).");
            }
        }

        foreach (string approvedProjectName in approvedReferences.Keys)
        {
            bool isPresent = solutionProjects.Any(
                solutionProject => string.Equals(
                    solutionProject.ProjectIdentity,
                    approvedProjectName,
                    StringComparison.OrdinalIgnoreCase));

            bool isDiscovered = productionProjects.Any(
                productionProject => string.Equals(
                    productionProject.Name,
                    approvedProjectName,
                    StringComparison.OrdinalIgnoreCase));

            if (!isPresent && !isDiscovered)
            {
                violations.Add(
                    "Approved production project missing from solution: " +
                    $"{approvedProjectName}.");
            }
        }
    }

    private static void AddDependencyViolations(
        Dictionary<string, HashSet<string>> approvedReferences,
        ProductionProject[] productionProjects,
        HashSet<string> violations)
    {
        var projectsByPath = productionProjects.ToDictionary(
            static project => project.FullPath,
            StringComparer.OrdinalIgnoreCase);

        foreach (ProductionProject productionProject in productionProjects)
        {
            ProjectReference[] actualReferences =
                ReadProjectReferences(productionProject.FullPath);

            var actualApprovedReferenceNames = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            bool sourceIsApproved = approvedReferences.TryGetValue(
                productionProject.Name,
                out HashSet<string>? expectedReferences);

            foreach (ProjectReference actualReference in actualReferences)
            {
                bool targetWasDiscovered = projectsByPath.TryGetValue(
                    actualReference.TargetFullPath,
                    out ProductionProject? targetProject);

                string targetName = targetProject?.Name ??
                    actualReference.Name;

                bool targetIsApproved = targetWasDiscovered &&
                    approvedReferences.ContainsKey(targetName);

                if (targetIsApproved)
                {
                    actualApprovedReferenceNames.Add(targetName);
                }
                else
                {
                    violations.Add(
                        "Reference to unknown production project: " +
                        $"{productionProject.Name} -> {targetName}.");
                }

                if (sourceIsApproved &&
                    expectedReferences is not null &&
                    (!targetIsApproved ||
                        !expectedReferences.Contains(targetName)))
                {
                    violations.Add(
                        "Forbidden project reference: " +
                        $"{productionProject.Name} -> {targetName}.");
                }
            }

            if (!sourceIsApproved || expectedReferences is null)
            {
                continue;
            }

            foreach (string expectedReference in expectedReferences)
            {
                if (!actualApprovedReferenceNames.Contains(expectedReference))
                {
                    violations.Add(
                        "Missing required project reference: " +
                        $"{productionProject.Name} -> " +
                        $"{expectedReference}.");
                }
            }
        }
    }

    private static Dictionary<string, HashSet<string>>
        CreateApprovedReferences()
    {
        return new Dictionary<string, HashSet<string>>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["FiveECharacterGenerator.CharacterState"] = new(
                StringComparer.OrdinalIgnoreCase),

            ["FiveECharacterGenerator.Rules"] = new(
                StringComparer.OrdinalIgnoreCase)
            {
                "FiveECharacterGenerator.CharacterState",
            },

            ["FiveECharacterGenerator.Generation"] = new(
                StringComparer.OrdinalIgnoreCase)
            {
                "FiveECharacterGenerator.CharacterState",
                "FiveECharacterGenerator.Rules",
            },

            ["FiveECharacterGenerator.Application"] = new(
                StringComparer.OrdinalIgnoreCase)
            {
                "FiveECharacterGenerator.CharacterState",
                "FiveECharacterGenerator.Generation",
                "FiveECharacterGenerator.Rules",
            },

            ["FiveECharacterGenerator.Persistence"] = new(
                StringComparer.OrdinalIgnoreCase)
            {
                "FiveECharacterGenerator.Application",
            },

            ["FiveECharacterGenerator.Desktop"] = new(
                StringComparer.OrdinalIgnoreCase)
            {
                "FiveECharacterGenerator.Application",
                "FiveECharacterGenerator.Persistence",
            },
        };
    }

    private static ProductionProject[] DiscoverProductionProjects(
        string repositoryRoot)
    {
        string sourceRoot = Path.Combine(repositoryRoot, "src");

        return Directory
            .EnumerateFiles(
                sourceRoot,
                "*",
                SearchOption.AllDirectories)
            .Where(
                static projectPath => string.Equals(
                    Path.GetExtension(projectPath),
                    ".csproj",
                    StringComparison.OrdinalIgnoreCase))
            .Select(
                projectPath => new ProductionProject(
                    Path.GetFileNameWithoutExtension(projectPath),
                    Path.GetFullPath(projectPath),
                    NormalizeRelativePath(repositoryRoot, projectPath)))
            .OrderBy(
                static project => project.RelativePath,
                StringComparer.Ordinal)
            .ToArray();
    }

    private static SolutionProjectEntry[] ReadSolutionProductionProjects(
        string repositoryRoot)
    {
        string solutionPath = Path.Combine(
            repositoryRoot,
            SolutionFileName);

        var entries = new List<SolutionProjectEntry>();

        foreach (string line in File.ReadLines(solutionPath))
        {
            if (!TryReadSolutionProjectEntry(
                    line,
                    out _,
                    out string projectPath))
            {
                continue;
            }

            string normalizedPath = NormalizeSolutionPath(
                repositoryRoot,
                projectPath);

            if (!normalizedPath.StartsWith(
                    "src/",
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(
                    Path.GetExtension(normalizedPath),
                    ".csproj",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            entries.Add(
                new SolutionProjectEntry(
                    Path.GetFileNameWithoutExtension(normalizedPath),
                    normalizedPath));
        }

        return entries
            .OrderBy(
                static entry => entry.RelativePath,
                StringComparer.Ordinal)
            .ThenBy(
                static entry => entry.ProjectIdentity,
                StringComparer.Ordinal)
            .ToArray();
    }

    private static bool TryReadSolutionProjectEntry(
        string line,
        out string projectName,
        out string projectPath)
    {
        projectName = string.Empty;
        projectPath = string.Empty;

        if (!line.StartsWith(
                "Project(",
                StringComparison.Ordinal))
        {
            return false;
        }

        int assignmentIndex = line.IndexOf('=', StringComparison.Ordinal);

        if (assignmentIndex < 0)
        {
            return false;
        }

        int currentIndex = assignmentIndex + 1;

        if (!TryReadQuotedField(
                line,
                ref currentIndex,
                out projectName) ||
            !TryReadComma(line, ref currentIndex) ||
            !TryReadQuotedField(
                line,
                ref currentIndex,
                out projectPath) ||
            !TryReadComma(line, ref currentIndex) ||
            !TryReadQuotedField(
                line,
                ref currentIndex,
                out _))
        {
            projectName = string.Empty;
            projectPath = string.Empty;
            return false;
        }

        SkipWhiteSpace(line, ref currentIndex);
        return currentIndex == line.Length;
    }

    private static bool TryReadQuotedField(
        string text,
        ref int currentIndex,
        out string value)
    {
        value = string.Empty;
        SkipWhiteSpace(text, ref currentIndex);

        if (currentIndex >= text.Length || text[currentIndex] != '"')
        {
            return false;
        }

        int closingQuoteIndex = text.IndexOf(
            '"',
            currentIndex + 1);

        if (closingQuoteIndex < 0)
        {
            return false;
        }

        value = text[(currentIndex + 1)..closingQuoteIndex];
        currentIndex = closingQuoteIndex + 1;
        return true;
    }

    private static bool TryReadComma(
        string text,
        ref int currentIndex)
    {
        SkipWhiteSpace(text, ref currentIndex);

        if (currentIndex >= text.Length || text[currentIndex] != ',')
        {
            return false;
        }

        currentIndex++;
        return true;
    }

    private static void SkipWhiteSpace(
        string text,
        ref int currentIndex)
    {
        while (currentIndex < text.Length &&
            char.IsWhiteSpace(text[currentIndex]))
        {
            currentIndex++;
        }
    }

    private static ProjectReference[] ReadProjectReferences(
        string projectPath)
    {
        XDocument projectDocument = XDocument.Load(
            projectPath,
            LoadOptions.None);

        string? projectDirectory = Path.GetDirectoryName(projectPath);

        if (projectDirectory is null)
        {
            throw new InvalidOperationException(
                $"The project directory could not be determined: {projectPath}");
        }

        return projectDocument
            .Descendants()
            .Where(
                static element =>
                    element.Name.LocalName == "ProjectReference")
            .Select(
                static projectReference =>
                    projectReference.Attribute("Include")?.Value)
            .Where(
                static includePath =>
                    !string.IsNullOrWhiteSpace(includePath))
            .Select(
                includePath =>
                {
                    string normalizedIncludePath =
                        NormalizePathSeparators(includePath!);

                    string targetFullPath = Path.GetFullPath(
                        Path.Combine(
                            projectDirectory,
                            normalizedIncludePath.Replace(
                                '/',
                                Path.DirectorySeparatorChar)));

                    return new ProjectReference(
                        Path.GetFileNameWithoutExtension(targetFullPath),
                        targetFullPath);
                })
            .OrderBy(
                static projectReference => projectReference.Name,
                StringComparer.Ordinal)
            .ThenBy(
                static projectReference => projectReference.TargetFullPath,
                StringComparer.Ordinal)
            .ToArray();
    }

    private static string NormalizeRelativePath(
        string repositoryRoot,
        string path)
    {
        return Path.GetRelativePath(
                repositoryRoot,
                Path.GetFullPath(path))
            .Replace('\\', '/')
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private static string NormalizeSolutionPath(
        string repositoryRoot,
        string path)
    {
        string localPath = NormalizePathSeparators(path).Replace(
            '/',
            Path.DirectorySeparatorChar);

        return NormalizeRelativePath(
            repositoryRoot,
            Path.Combine(repositoryRoot, localPath));
    }

    private static string NormalizePathSeparators(string path)
    {
        return path
            .Trim()
            .Replace('\\', '/')
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private static string CreateFailureMessage(string[] violations)
    {
        if (violations.Length == 0)
        {
            return string.Empty;
        }

        return "Production-project policy violations:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                violations.Select(
                    static violation => $"- {violation}"));
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

            string sourceDirectory = Path.Combine(
                currentDirectory.FullName,
                "src");

            if (File.Exists(solutionPath) &&
                Directory.Exists(sourceDirectory))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException(
            "The repository root could not be located using " +
            $"{SolutionFileName} and the src directory.");
    }

    private sealed class ProductionProject
    {
        internal ProductionProject(
            string name,
            string fullPath,
            string relativePath)
        {
            Name = name;
            FullPath = fullPath;
            RelativePath = relativePath;
        }

        internal string Name { get; }

        internal string FullPath { get; }

        internal string RelativePath { get; }
    }

    private sealed class SolutionProjectEntry
    {
        internal SolutionProjectEntry(
            string projectIdentity,
            string relativePath)
        {
            ProjectIdentity = projectIdentity;
            RelativePath = relativePath;
        }

        internal string ProjectIdentity { get; }

        internal string RelativePath { get; }
    }

    private sealed class ProjectReference
    {
        internal ProjectReference(
            string name,
            string targetFullPath)
        {
            Name = name;
            TargetFullPath = targetFullPath;
        }

        internal string Name { get; }

        internal string TargetFullPath { get; }
    }

    private sealed class TemporaryRepository : IDisposable
    {
        private readonly List<SolutionFixtureEntry> solutionEntries = new();

        private TemporaryRepository(string rootPath)
        {
            RootPath = rootPath;
        }

        internal string RootPath { get; }

        internal static TemporaryRepository CreateValid()
        {
            string rootPath = Path.Combine(
                Path.GetTempPath(),
                "FiveECharacterGenerator.Architecture.Tests",
                Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));

            Directory.CreateDirectory(rootPath);

            var repository = new TemporaryRepository(rootPath);

            foreach (
                KeyValuePair<string, HashSet<string>> projectPolicy
                in CreateApprovedReferences()
                    .OrderBy(
                        static project => project.Key,
                        StringComparer.Ordinal))
            {
                repository.WriteProject(
                    projectPolicy.Key,
                    projectPolicy.Value
                        .OrderBy(
                            static reference => reference,
                            StringComparer.Ordinal)
                        .ToArray());

                repository.solutionEntries.Add(
                    new SolutionFixtureEntry(
                        projectPolicy.Key,
                        $"src/{projectPolicy.Key}/" +
                        $"{projectPolicy.Key}.csproj"));
            }

            repository.WriteSolution();
            return repository;
        }

        internal void WriteProject(
            string projectName,
            params string[] projectReferences)
        {
            WriteProjectAt(
                $"src/{projectName}/{projectName}.csproj",
                projectReferences);
        }

        internal void WriteProjectAt(
            string relativeProjectPath,
            params string[] projectReferences)
        {
            string normalizedRelativePath =
                NormalizePathSeparators(relativeProjectPath);

            string projectPath = Path.Combine(
                RootPath,
                normalizedRelativePath.Replace(
                    '/',
                    Path.DirectorySeparatorChar));

            string? projectDirectory = Path.GetDirectoryName(projectPath);

            if (projectDirectory is null)
            {
                throw new InvalidOperationException(
                    "The fixture project directory could not be determined.");
            }

            Directory.CreateDirectory(projectDirectory);

            var projectLines = new List<string>
            {
                "<Project Sdk=\"Microsoft.NET.Sdk\">",
            };

            if (projectReferences.Length > 0)
            {
                projectLines.Add("  <ItemGroup>");

                foreach (
                    string projectReference
                    in projectReferences.OrderBy(
                        static reference => reference,
                        StringComparer.Ordinal))
                {
                    string targetPath = Path.Combine(
                        RootPath,
                        "src",
                        projectReference,
                        $"{projectReference}.csproj");

                    string includePath = Path.GetRelativePath(
                            projectDirectory,
                            targetPath)
                        .Replace('/', '\\')
                        .Replace(Path.DirectorySeparatorChar, '\\')
                        .Replace(Path.AltDirectorySeparatorChar, '\\');

                    projectLines.Add(
                        "    <ProjectReference Include=\"" +
                        $"{includePath}\" />");
                }

                projectLines.Add("  </ItemGroup>");
            }

            projectLines.Add("</Project>");

            File.WriteAllText(
                projectPath,
                string.Join(Environment.NewLine, projectLines) +
                Environment.NewLine);
        }

        internal void DeleteProject(string projectName)
        {
            File.Delete(
                Path.Combine(
                    RootPath,
                    "src",
                    projectName,
                    $"{projectName}.csproj"));
        }

        internal void AddSolutionEntry(
            string projectName,
            string relativeProjectPath)
        {
            solutionEntries.Add(
                new SolutionFixtureEntry(
                    projectName,
                    NormalizePathSeparators(relativeProjectPath)));

            WriteSolution();
        }

        internal void RemoveSolutionEntries(string projectName)
        {
            solutionEntries.RemoveAll(
                entry => string.Equals(
                    entry.Name,
                    projectName,
                    StringComparison.OrdinalIgnoreCase));

            WriteSolution();
        }

        public void Dispose()
        {
            Directory.Delete(RootPath, recursive: true);
            GC.SuppressFinalize(this);
        }

        private void WriteSolution()
        {
            var solutionLines = new List<string>
            {
                "Microsoft Visual Studio Solution File, Format Version 12.00",
            };

            foreach (
                SolutionFixtureEntry entry
                in solutionEntries.OrderBy(
                    static solutionEntry => solutionEntry.RelativePath,
                    StringComparer.Ordinal))
            {
                solutionLines.Add(
                    "Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") " +
                    $"= \"{entry.Name}\", " +
                    $"\"{entry.RelativePath.Replace('/', '\\')}\", " +
                    "\"{00000000-0000-0000-0000-000000000000}\"");
                solutionLines.Add("EndProject");
            }

            solutionLines.Add("Global");
            solutionLines.Add("EndGlobal");

            File.WriteAllText(
                Path.Combine(RootPath, SolutionFileName),
                string.Join(Environment.NewLine, solutionLines) +
                Environment.NewLine);
        }
    }

    private sealed class SolutionFixtureEntry
    {
        internal SolutionFixtureEntry(
            string name,
            string relativePath)
        {
            Name = name;
            RelativePath = relativePath;
        }

        internal string Name { get; }

        internal string RelativePath { get; }
    }
}
