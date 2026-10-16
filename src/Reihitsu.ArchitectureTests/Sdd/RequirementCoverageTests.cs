using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// Links the tests to the spec requirements they cover, as <c>sdd/README.md</c> describes: a test names each
/// requirement it covers with a <c>[Requirement("&lt;ID&gt;")]</c> attribute. Every such reference must name an
/// active requirement, and every active requirement of an enforced spec must be covered by at least one test.
/// Specs outside <see cref="EnforcedSpecDirectories"/> are reported but do not fail
/// </summary>
[TestClass]
public sealed class RequirementCoverageTests
{
    #region Constants

    /// <summary>
    /// Spec directories, relative to <c>sdd/specs</c>, whose active requirements must each be covered by a test.
    /// A directory stays on this list once it is added
    /// </summary>
    internal static readonly IReadOnlyList<string> EnforcedSpecDirectories = ["analyzer/", "cli/"];

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context, used to report the coverage of specs that are not enforced yet
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that every requirement attribute in a test source names an active requirement with a string literal
    /// and sits on a test method
    /// </summary>
    [TestMethod]
    public void EveryRequirementReferenceNamesAnActiveRequirement()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindReferenceProblems(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Every [Requirement] attribute must name an active requirement on a test method", findings));
    }

    /// <summary>
    /// Verifies that every active requirement of an enforced spec is covered by at least one test
    /// </summary>
    [TestMethod]
    public void EveryRequirementOfAnEnforcedSpecIsCovered()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindUncoveredRequirements(repositoryRoot, EnforcedSpecDirectories);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Every active requirement of an enforced spec must be covered by a test tagged [Requirement(\"<ID>\")]", findings));
    }

    /// <summary>
    /// Reports, per spec that is not enforced yet, how many active requirements no test covers. The report is
    /// written to the test output and never fails
    /// </summary>
    [TestMethod]
    public void ReportCoverageOfSpecsNotYetEnforced()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var report = DescribeCoverageOfSpecsNotYetEnforced(repositoryRoot);

        // Assert
        TestContext.WriteLine(report);
        Assert.IsNotNull(report);
    }

    /// <summary>
    /// Verifies that a reference to an ID that heads no requirement is reported with its location
    /// </summary>
    [TestMethod]
    public void ReferenceCheckReportsUnknownId()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001");
            fixture.WriteFile("src/Reihitsu.Cli.Test/ProgramTests.cs", CreateTestSource("[Requirement(\"CLI-099\")]"));

            // Act
            var findings = FindReferenceProblems(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("CLI-099", findings[0]);
            Assert.Contains("src/Reihitsu.Cli.Test/ProgramTests.cs:", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a reference to a retired requirement is reported
    /// </summary>
    [TestMethod]
    public void ReferenceCheckReportsRetiredId()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteFile("sdd/specs/cli/reihitsu-format.md", "### CLI-001 — Title\n\nStatus: Retired — superseded.\n");
            fixture.WriteFile("src/Reihitsu.Cli.Test/ProgramTests.cs", CreateTestSource("[Requirement(\"CLI-001\")]"));

            // Act
            var findings = FindReferenceProblems(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("CLI-001", findings[0]);
            Assert.Contains("retired", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that an argument other than one string literal is reported, because the check reads the sources
    /// </summary>
    [TestMethod]
    public void ReferenceCheckReportsNonLiteralArgument()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001");
            fixture.WriteFile("src/Reihitsu.Cli.Test/ProgramTests.cs", CreateTestSource("[Requirement(Ids.First)]"));

            // Act
            var findings = FindReferenceProblems(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("string literal", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a requirement attribute on a method that is not a test method is reported
    /// </summary>
    [TestMethod]
    public void ReferenceCheckReportsAttributeOutsideTestMethod()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001");
            fixture.WriteFile("src/Reihitsu.Cli.Test/ProgramTests.cs", "internal class ProgramTests\n{\n    [Requirement(\"CLI-001\")]\n    private void Helper() { }\n}\n");

            // Act
            var findings = FindReferenceProblems(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("[TestMethod]", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that the attribute is recognized with the <c>Attribute</c> suffix, with a qualifier, and in an
    /// attribute list it shares with <c>[TestMethod]</c>
    /// </summary>
    [TestMethod]
    public void ReferenceCheckRecognizesEveryAttributeSpelling()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001", "CLI-002", "CLI-003");
            fixture.WriteFile("src/Reihitsu.Cli.Test/ProgramTests.cs",
                              "internal class ProgramTests\n{\n"
                              + "    [TestMethod]\n    [RequirementAttribute(\"CLI-001\")]\n    public void A() { }\n"
                              + "    [TestMethod]\n    [Helpers.Requirement(\"CLI-002\")]\n    public void B() { }\n"
                              + "    [TestMethod, Requirement(\"CLI-003\")]\n    public void C() { }\n"
                              + "}\n");

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, ["cli/"]);

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Verifies that an uncovered active requirement of an enforced spec is reported with its spec location
    /// </summary>
    [TestMethod]
    public void CoverageCheckReportsUncoveredRequirementOfEnforcedSpec()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001", "CLI-002");
            fixture.WriteFile("src/Reihitsu.Cli.Test/ProgramTests.cs", CreateTestSource("[Requirement(\"CLI-001\")]"));

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, ["cli/"]);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("CLI-002", findings[0]);
            Assert.Contains("cli/reihitsu-format.md:", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that an uncovered requirement of a spec outside the enforced directories is not reported as a failure,
    /// and that a retired requirement never needs a covering test
    /// </summary>
    [TestMethod]
    public void CoverageCheckIgnoresSpecsNotEnforcedAndRetiredRequirements()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("formatter/cleanup.md", "`CleanupPhase`", "FMT-CLEAN-001");
            fixture.WriteFile("sdd/specs/cli/reihitsu-format.md", "### CLI-001 — Title\n\nStatus: Retired — superseded.\n");

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, ["cli/"]);

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Verifies that a reference counts only from a test project's own sources, not from a production project or
    /// from build output, and only when it sits on a test method
    /// </summary>
    [TestMethod]
    public void CoverageCheckCountsOnlyTestMethodsInTestProjects()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001", "CLI-002", "CLI-003", "CLI-004");
            fixture.WriteFile("src/Reihitsu.Cli/Program.cs", CreateTestSource("[Requirement(\"CLI-001\")]"));
            fixture.WriteFile("src/Reihitsu.Cli.Test/obj/Generated.cs", CreateTestSource("[Requirement(\"CLI-002\")]"));
            fixture.WriteFile("src/Reihitsu.Cli.Test/Helper.cs", "internal class Helper\n{\n    [Requirement(\"CLI-003\")]\n    private void Run() { }\n}\n");
            fixture.WriteFile("src/Reihitsu.ArchitectureTests/CliTests.cs", CreateTestSource("[Requirement(\"CLI-004\")]"));

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, ["cli/"]);

            // Assert
            Assert.HasCount(3, findings);
            Assert.Contains(finding => finding.StartsWith("CLI-001", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("CLI-002", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("CLI-003", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that the report of specs not enforced yet counts the uncovered requirements per spec and leaves
    /// enforced specs out
    /// </summary>
    [TestMethod]
    public void CoverageReportCountsUncoveredRequirementsPerSpecNotYetEnforced()
    {
        // Arrange
        string report;

        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("formatter/cleanup.md", "`CleanupPhase`", "FMT-CLEAN-001", "FMT-CLEAN-002");
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001");
            fixture.WriteFile("src/Reihitsu.Formatter.Test/CleanupTests.cs", CreateTestSource("[Requirement(\"FMT-CLEAN-001\")]"));

            // Act
            report = DescribeCoverageOfSpecsNotYetEnforced(fixture.Path);
        }

        // Assert
        Assert.Contains("formatter/cleanup.md: 1 of 2 uncovered (FMT-CLEAN-002)", report);
        Assert.DoesNotContain("cli/reihitsu-format.md", report);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Finds every requirement attribute that does not name an active requirement with a string literal, or that
    /// is not on a test method
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c> and <c>src</c></param>
    /// <returns>One message per problem, naming the source location</returns>
    private static List<string> FindReferenceProblems(string repositoryRoot)
    {
        var requirements = ReadRequirementsById(repositoryRoot);
        var problems = new List<string>();

        foreach (var reference in RequirementReferenceCatalog.ReadReferences(repositoryRoot))
        {
            var location = $"{reference.FilePath}:{reference.Line}";

            if (reference.Id == null)
            {
                problems.Add($"{location}: the requirement ID must be a single string literal");
            }
            else if (requirements.TryGetValue(reference.Id, out var requirement) == false)
            {
                problems.Add($"{location}: {reference.Id} heads no requirement in any spec");
            }
            else if (requirement.IsRetired)
            {
                problems.Add($"{location}: {reference.Id} is retired ({requirement.SpecPath}:{requirement.Line}); tag the requirement that replaced it");
            }

            if (reference.IsOnTestMethod == false)
            {
                problems.Add($"{location}: [Requirement] must be on a method marked [TestMethod]");
            }
        }

        return problems;
    }

    /// <summary>
    /// Finds every active requirement of the given spec directories that no test method covers
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c> and <c>src</c></param>
    /// <param name="specDirectories">Spec directories relative to <c>sdd/specs</c>, each ending with a slash</param>
    /// <returns>One message per uncovered requirement, starting with its ID</returns>
    private static List<string> FindUncoveredRequirements(string repositoryRoot, IReadOnlyList<string> specDirectories)
    {
        var coveredIds = ReadCoveredIds(repositoryRoot);

        return ReadActiveRequirements(repositoryRoot).Where(requirement => IsInDirectories(requirement, specDirectories)
                                                                           && coveredIds.Contains(requirement.Id) == false)
                                                     .Select(requirement => $"{requirement.Id}: no test covers it ({requirement.SpecPath}:{requirement.Line})")
                                                     .ToList();
    }

    /// <summary>
    /// Describes, per spec outside <see cref="EnforcedSpecDirectories"/>, the active requirements no test covers
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c> and <c>src</c></param>
    /// <returns>The report, one line per spec</returns>
    private static string DescribeCoverageOfSpecsNotYetEnforced(string repositoryRoot)
    {
        var coveredIds = ReadCoveredIds(repositoryRoot);
        var lines = ReadActiveRequirements(repositoryRoot).Where(requirement => IsInDirectories(requirement, EnforcedSpecDirectories) == false)
                                                          .GroupBy(requirement => requirement.SpecPath, StringComparer.Ordinal)
                                                          .Select(group =>
                                                                  {
                                                                      var uncovered = group.Where(requirement => coveredIds.Contains(requirement.Id) == false)
                                                                                           .Select(requirement => requirement.Id)
                                                                                           .ToList();
                                                                      var detail = uncovered.Count == 0 ? string.Empty : $" ({string.Join(", ", uncovered)})";

                                                                      return $"{group.Key}: {uncovered.Count} of {group.Count()} uncovered{detail}";
                                                                  })
                                                          .ToList();

        return $"Coverage of specs not yet enforced:{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
    }

    /// <summary>
    /// Reads the IDs that at least one test method references
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>src</c></param>
    /// <returns>The covered IDs</returns>
    private static HashSet<string> ReadCoveredIds(string repositoryRoot)
    {
        return RequirementReferenceCatalog.ReadReferences(repositoryRoot)
                                          .Where(reference => reference.IsOnTestMethod && reference.Id != null)
                                          .Select(reference => reference.Id)
                                          .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Reads the requirements of all specs that are not retired
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <returns>The active requirements</returns>
    private static IEnumerable<SpecRequirement> ReadActiveRequirements(string repositoryRoot)
    {
        return SpecCatalog.ReadRequirements(Path.Combine(repositoryRoot, "sdd", "specs"))
                          .Where(requirement => requirement.IsRetired == false);
    }

    /// <summary>
    /// Reads the requirements of all specs keyed by ID; a duplicated ID, which <see cref="SpecConsistencyTests"/>
    /// reports, keeps its first heading
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <returns>The requirements by ID</returns>
    private static Dictionary<string, SpecRequirement> ReadRequirementsById(string repositoryRoot)
    {
        return SpecCatalog.ReadRequirements(Path.Combine(repositoryRoot, "sdd", "specs"))
                          .GroupBy(requirement => requirement.Id, StringComparer.Ordinal)
                          .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    /// <summary>
    /// Determines whether a requirement's spec lies in one of the given spec directories
    /// </summary>
    /// <param name="requirement">Requirement</param>
    /// <param name="specDirectories">Spec directories relative to <c>sdd/specs</c>, each ending with a slash</param>
    /// <returns><see langword="true"/> when the spec lies in one of the directories</returns>
    private static bool IsInDirectories(SpecRequirement requirement, IReadOnlyList<string> specDirectories)
    {
        return specDirectories.Any(directory => requirement.SpecPath.StartsWith(directory, StringComparison.Ordinal));
    }

    /// <summary>
    /// Creates a test class whose one test method carries the given attribute
    /// </summary>
    /// <param name="requirementAttribute">Requirement attribute source, such as <c>[Requirement("CLI-001")]</c></param>
    /// <returns>The test source</returns>
    private static string CreateTestSource(string requirementAttribute)
    {
        return $"internal class Tests\n{{\n    [TestMethod]\n    {requirementAttribute}\n    public void Test() {{ }}\n}}\n";
    }

    /// <summary>
    /// Formats findings into an assertion message
    /// </summary>
    /// <param name="title">Message title</param>
    /// <param name="findings">Findings to list</param>
    /// <returns>The assertion message</returns>
    private static string DescribeFindings(string title, IEnumerable<string> findings)
    {
        return $"{title}:{Environment.NewLine}{string.Join(Environment.NewLine, findings.Select(finding => $"  {finding}"))}";
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