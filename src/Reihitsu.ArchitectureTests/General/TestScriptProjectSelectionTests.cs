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
    #region Constants

    /// <summary>
    /// The self-hosting test project, which scans the repository's own source; CI runs it, the default local run leaves it out
    /// </summary>
    private const string SelfHostingTestProject = "src/Reihitsu.SelfHosting.Test/Reihitsu.SelfHosting.Test.csproj";

    /// <summary>
    /// The architecture test project, which the build workflow runs on its own instead of the SonarCloud test loop
    /// </summary>
    private const string ArchitectureTestProject = "src/Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Test projects in the order used by a default repository test run
    /// </summary>
    private static readonly string[] _expectedTestProjects = [
                                                                 "src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj",
                                                                 "src/Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj",
                                                                 "src/Reihitsu.Core.Test/Reihitsu.Core.Test.csproj",
                                                                 "src/Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj",
                                                                 "src/Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj",
                                                                 "src/Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj",
                                                                 "src/Reihitsu.Playground.Test/Reihitsu.Playground.Test.csproj"
                                                             ];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that the shell all branch selects every test project in the solution except the self-hosting project
    /// </summary>
    [TestMethod]
    public void ShellAllIncludesEverySolutionTestProjectExceptSelfHosting()
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
    /// Verifies that the architecture and tooling aliases remain single-project selections
    /// </summary>
    [TestMethod]
    public void NamedArchitectureAndToolingSelectionsRemainNarrow()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var shell = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "test.sh"));

        // Act and assert
        AssertNamedShellSelection(shell,
                                  "architecture",
                                  "src/Reihitsu.ArchitectureTests/Reihitsu.ArchitectureTests.csproj");
        AssertNamedShellSelection(shell,
                                  "tooling",
                                  "src/Reihitsu.Tooling.Test/Reihitsu.Tooling.Test.csproj");
    }

    /// <summary>
    /// Verifies that the default run leaves the self-hosting project out
    /// </summary>
    [TestMethod]
    public void ShellAllLeavesOutTheSelfHostingProject()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var shell = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "test.sh"));

        // Act
        var selected = ProjectPaths(AllBranchBody(shell));

        // Assert
        Assert.DoesNotContain(SelfHostingTestProject, selected);
    }

    /// <summary>
    /// Verifies that the selfhosting alias selects the self-hosting project and nothing else
    /// </summary>
    [TestMethod]
    public void NamedSelfHostingSelectionSelectsOnlyTheSelfHostingProject()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var shell = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "test.sh"));

        // Act and assert
        AssertNamedShellSelection(shell, "selfhosting", SelfHostingTestProject);
    }

    /// <summary>
    /// Verifies that the solution lists the self-hosting project as a test project, so a whole-solution build compiles it
    /// and a whole-solution test run executes it
    /// </summary>
    [TestMethod]
    public void SolutionListsTheSelfHostingTestProject()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var solutionProjects = SolutionTestProjects(repositoryRoot);

        // Assert
        Assert.Contains(SelfHostingTestProject, solutionProjects);
    }

    /// <summary>
    /// Verifies that the SonarCloud test loop runs every test project of the solution except the architecture tests,
    /// which the build workflow runs on its own, so a pull request runs the self-hosting scans in CI
    /// </summary>
    [TestMethod]
    public void SonarCloudTestLoopRunsEverySolutionTestProjectExceptArchitecture()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var expected = SolutionTestProjects(repositoryRoot).Where(path => path != ArchitectureTestProject)
                                                           .ToArray();

        // Act
        var looped = SonarCloudLoopProjects(repositoryRoot);

        // Assert
        Assert.AreSequenceEqual(expected, looped, SequenceOrder.InAnyOrder, "The SonarCloud test loop does not match the solution's test projects.");
    }

    /// <summary>
    /// Verifies that the SonarCloud test loop names the self-hosting project
    /// </summary>
    [TestMethod]
    public void SonarCloudTestLoopRunsTheSelfHostingProject()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var looped = SonarCloudLoopProjects(repositoryRoot);

        // Assert
        Assert.Contains(SelfHostingTestProject, looped);
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

        Assert.AreSequenceEqual(_expectedTestProjects, selected, "The all-project order changed.");
        Assert.AreSequenceEqual(_expectedTestProjects.Append(SelfHostingTestProject).ToArray(), solutionProjects, SequenceOrder.InAnyOrder, "The all branch and the self-hosting project do not match the solution's test projects.");
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
    /// Extracts the body of the shell all branch
    /// </summary>
    /// <param name="script">Script text</param>
    /// <returns>The branch body</returns>
    private static string AllBranchBody(string script)
    {
        var branch = Regex.Match(script, @"(?ms)^\s*all\)\s*\r?\n(?<body>.*?)^\s*;;\s*$");

        Assert.IsTrue(branch.Success, "Could not locate the all-project branch.");

        return branch.Groups["body"].Value;
    }

    /// <summary>
    /// Extracts the projects of the SonarCloud workflow's test loop as solution-relative project paths
    /// </summary>
    /// <param name="repositoryRoot">Repository root</param>
    /// <returns>Project paths in loop order</returns>
    private static string[] SonarCloudLoopProjects(string repositoryRoot)
    {
        var workflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "SonarCloud.yml"));
        var loop = Regex.Match(workflow, @"(?m)^\s*for\s+project\s+in\s+(?<names>[^;\r\n]+);\s*do\s*$");

        Assert.IsTrue(loop.Success, "Could not locate the SonarCloud test loop.");

        return loop.Groups["names"].Value
                   .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                   .Select(name => $"src/{name}/{name}.csproj")
                   .ToArray();
    }

    /// <summary>
    /// Extracts project paths from a script branch
    /// </summary>
    /// <param name="text">Script branch text</param>
    /// <returns>Project paths in source order</returns>
    private static string[] ProjectPaths(string text)
    {
        return text.Split(['\'', '"'], StringSplitOptions.RemoveEmptyEntries)
                   .Where(segment => segment.StartsWith("src/Reihitsu.", StringComparison.Ordinal)
                                     && segment.EndsWith(".csproj", StringComparison.Ordinal))
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

        return solution.Split('"')
                       .Where(segment => segment.EndsWith(".csproj", StringComparison.Ordinal))
                       .Select(path => path.Replace('\\', '/'))
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