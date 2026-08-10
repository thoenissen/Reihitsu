using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards the test-project selection shared by the repository test scripts
/// </summary>
[TestClass]
public sealed class TestScriptProjectSelectionTests
{
    #region Fields

    /// <summary>
    /// Test projects in the order used by a full repository test run
    /// </summary>
    private static readonly string[] _expectedTestProjects = [
                                                                 "Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj",
                                                                 "Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj",
                                                                 "Reihitsu.Core.Test/Reihitsu.Core.Test.csproj",
                                                                 "Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj",
                                                                 "Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj",
                                                                 "Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj"
                                                             ];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that the PowerShell all branch selects every test project in the solution
    /// </summary>
    [TestMethod]
    public void PowerShellAllIncludesEverySolutionTestProject()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "test.ps1"));

        // Act and assert
        AssertAllSelection(script,
                           @"(?ms)^\s*'all'\s*\r?\n\s*\{\s*\r?\n(?<body>.*?)^\s*\}\s*\r?\n\}",
                           repositoryRoot);
    }

    /// <summary>
    /// Verifies that the shell all branch selects every test project in the solution
    /// </summary>
    [TestMethod]
    public void ShellAllIncludesEverySolutionTestProject()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "test.sh"));

        // Act and assert
        AssertAllSelection(script,
                           @"(?ms)^\s*all\)\s*\r?\n(?<body>.*?)^\s*;;\s*$",
                           repositoryRoot);
    }

    /// <summary>
    /// Verifies that the architecture and tooling aliases remain single-project selections in both scripts
    /// </summary>
    [TestMethod]
    public void NamedArchitectureAndToolingSelectionsRemainNarrow()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var powerShell = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "test.ps1"));
        var shell = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "test.sh"));

        // Act and assert
        AssertNamedPowerShellSelection(powerShell,
                                       "architecture",
                                       "Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj");
        AssertNamedPowerShellSelection(powerShell,
                                       "tooling",
                                       "Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj");
        AssertNamedShellSelection(shell,
                                  "architecture",
                                  "Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj");
        AssertNamedShellSelection(shell,
                                  "tooling",
                                  "Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj");
    }

    /// <summary>
    /// Verifies an all branch against the expected ordered set and the solution's current test projects
    /// </summary>
    /// <param name="script">Script text</param>
    /// <param name="allPattern">Pattern selecting the all branch</param>
    /// <param name="repositoryRoot">Repository root</param>
    private static void AssertAllSelection(string script, string allPattern, string repositoryRoot)
    {
        var branch = Regex.Match(script, allPattern);

        Assert.IsTrue(branch.Success, "Could not locate the all-project branch.");

        var selected = ProjectPaths(branch.Groups["body"].Value);
        var solutionProjects = SolutionTestProjects(repositoryRoot);

        CollectionAssert.AreEqual(_expectedTestProjects, selected, "The all-project order changed.");
        CollectionAssert.AreEquivalent(_expectedTestProjects,
                                       solutionProjects,
                                       "The all branch does not match the solution's test projects.");
    }

    /// <summary>
    /// Verifies one named PowerShell selection
    /// </summary>
    /// <param name="script">Script text</param>
    /// <param name="name">Selection name</param>
    /// <param name="expectedProject">Expected project path</param>
    private static void AssertNamedPowerShellSelection(string script, string name, string expectedProject)
    {
        var match = Regex.Match(script,
                                $@"(?m)^\s*'{Regex.Escape(name)}'\s*\{{\s*@\('(?<path>[^']+\.csproj)'\)\s*\}}");

        Assert.IsTrue(match.Success, $"Could not locate the PowerShell '{name}' selection.");
        Assert.AreEqual(expectedProject, match.Groups["path"].Value);
    }

    /// <summary>
    /// Verifies one named shell selection
    /// </summary>
    /// <param name="script">Script text</param>
    /// <param name="name">Selection name</param>
    /// <param name="expectedProject">Expected project path</param>
    private static void AssertNamedShellSelection(string script, string name, string expectedProject)
    {
        var match = Regex.Match(script,
                                $@"(?m)^\s*{Regex.Escape(name)}\)\s*projects=\(""(?<path>[^""]+\.csproj)""\)\s*;;");

        Assert.IsTrue(match.Success, $"Could not locate the shell '{name}' selection.");
        Assert.AreEqual(expectedProject, match.Groups["path"].Value);
    }

    /// <summary>
    /// Extracts project paths from a script branch
    /// </summary>
    /// <param name="text">Script branch text</param>
    /// <returns>Project paths in source order</returns>
    private static string[] ProjectPaths(string text)
    {
        return Regex.Matches(text, @"[""'](?<path>Reihitsu\.[^""']+\.csproj)[""']")
                    .Select(match => match.Groups["path"].Value)
                    .ToArray();
    }

    /// <summary>
    /// Finds every test project listed in the solution by its test SDK reference
    /// </summary>
    /// <param name="repositoryRoot">Repository root</param>
    /// <returns>Solution-relative test-project paths</returns>
    private static string[] SolutionTestProjects(string repositoryRoot)
    {
        var solution = File.ReadAllText(Path.Combine(repositoryRoot, "Reihitsu.sln"));

        return Regex.Matches(solution, @"""(?<path>[^""]+\.csproj)""")
                    .Select(match => match.Groups["path"].Value.Replace('\\', '/'))
                    .Where(path => File.ReadAllText(Path.Combine(repositoryRoot, path.Replace('/', Path.DirectorySeparatorChar)))
                                       .Contains("Microsoft.NET.Test.Sdk", StringComparison.Ordinal))
                    .ToArray();
    }

    /// <summary>
    /// Finds the repository root from the test output directory
    /// </summary>
    /// <returns>Absolute repository-root path</returns>
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Reihitsu.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Reihitsu repository root.");
    }

    #endregion // Methods
}