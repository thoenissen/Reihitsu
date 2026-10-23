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
/// Every spec is enforced unless <see cref="ExemptedSpecs"/> names it, and every exemption must state its reason; a
/// requirement of an exempted <c>shared/</c> spec stays enforced when a spec that is not exempted lists it under
/// <c>Uses</c>. The uncovered requirements of exempted specs are reported but do not fail
/// </summary>
[TestClass]
public sealed class RequirementCoverageTests
{
    #region Constants

    /// <summary>
    /// Specs left out of the coverage check, each a single spec file relative to <c>sdd/specs</c> mapped to the reason
    /// it is exempted. Every spec not on this list is enforced; the goal is an empty list
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, string> ExemptedSpecs = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Spec directory of the shared specs, whose requirements of an exempted spec an enforced spec keeps enforced by
    /// using them
    /// </summary>
    private const string SharedDirectory = "shared/";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context, used to report the coverage of exempted specs
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
    /// Verifies that every active requirement of every spec that is not exempted is covered by at least one test
    /// </summary>
    [TestMethod]
    public void EveryRequirementOfAnEnforcedSpecIsCovered()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindUncoveredRequirements(repositoryRoot, ExemptedSpecs);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Every active requirement of an enforced spec must be covered by a test tagged [Requirement(\"<ID>\")]", findings));
    }

    /// <summary>
    /// Verifies that every exemption from the coverage check states why its spec is exempted
    /// </summary>
    [TestMethod]
    public void EveryExemptionStatesAReason()
    {
        // Act
        var findings = FindExemptionsWithoutReason(ExemptedSpecs);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Every exemption from the requirement coverage check must state its reason", findings));
    }

    /// <summary>
    /// Reports, per exempted spec, how many active requirements no test covers. The report is written to the test
    /// output and never fails
    /// </summary>
    [TestMethod]
    public void ReportCoverageOfExemptedSpecs()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var report = DescribeCoverageOfExemptedSpecs(repositoryRoot, ExemptedSpecs);

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
            var findings = FindUncoveredRequirements(fixture.Path, CreateExemptions());

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Verifies that an uncovered active requirement of a spec that is not exempted is reported with its spec location
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
            var findings = FindUncoveredRequirements(fixture.Path, CreateExemptions());

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("CLI-002", findings[0]);
            Assert.Contains("cli/reihitsu-format.md:", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that, without exemptions, an uncovered requirement is reported in every spec area: the areas the
    /// former opt-in list named, a <c>shared/</c> spec it did not name, and an area that no list names at all
    /// </summary>
    [TestMethod]
    public void CoverageCheckEnforcesEverySpecByDefault()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("analyzer/RH3202.md", "`Analyzer`", "RH3202-001");
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001");
            fixture.WriteSpec("formatter/cleanup.md", "`CleanupPhase`", "FMT-CLEAN-001");
            fixture.WriteSpec("playground/editor.md", "`Editor`", "PLAY-001");
            fixture.WriteSpec("shared/casing.md", "`CasingUtilities`", "SHARED-CASING-001");
            fixture.WriteSpec("new-area/surface.md", "`Surface`", "NEW-001");
            fixture.WriteSpec("top-level.md", "`TopLevel`", "TOP-001");

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, CreateExemptions());

            // Assert
            Assert.HasCount(7, findings, DescribeFindings("Findings", findings));
            Assert.Contains(finding => finding.StartsWith("RH3202-001", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("CLI-001", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("FMT-CLEAN-001", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("PLAY-001", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("SHARED-CASING-001", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("NEW-001", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("TOP-001", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that a requirement of a <c>shared/</c> spec is enforced although no spec lists it under <c>Uses</c>
    /// </summary>
    [TestMethod]
    public void CoverageCheckEnforcesSharedRequirementThatNoSpecUses()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("shared/policy.md", "`Policy`", "SHARED-P-001", "SHARED-P-002");
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001");
            fixture.WriteFile("src/Reihitsu.Cli.Test/ProgramTests.cs", CreateTestSource("[Requirement(\"CLI-001\")]"));
            fixture.WriteFile("src/Reihitsu.Core.Test/PolicyTests.cs", CreateTestSource("[Requirement(\"SHARED-P-001\")]"));

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, CreateExemptions());

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("SHARED-P-002", findings[0]);
            Assert.Contains("shared/policy.md:", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a retired requirement never needs a covering test, while an active requirement of the same spec
    /// does
    /// </summary>
    [TestMethod]
    public void CoverageCheckIgnoresRetiredRequirements()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteFile("sdd/specs/cli/reihitsu-format.md", "### CLI-001 — Title\n\nStatus: Retired — superseded.\n\n### CLI-002 — Title\n\nThe CLI MUST do it.\n");

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, CreateExemptions());

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("CLI-002", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that an uncovered requirement of an exempted spec does not fail the check, while an uncovered
    /// requirement of a sibling spec in the same directory that is not exempted still does
    /// </summary>
    [TestMethod]
    public void CoverageCheckSkipsOnlyTheExemptedSpec()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("formatter/cleanup.md", "`CleanupPhase`", "FMT-CLEAN-001");
            fixture.WriteSpec("formatter/indentation.md", "`IndentationPhase`", "FMT-INDENT-001");

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, CreateExemptions(("formatter/cleanup.md", "Its tests are written in a follow-up.")));

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("FMT-INDENT-001", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that an exemption keyed by a directory names no spec and therefore exempts none of the specs in that
    /// directory, so their uncovered requirements still fail the check
    /// </summary>
    [TestMethod]
    public void CoverageCheckDoesNotExemptTheSpecsOfAnExemptedDirectory()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("formatter/cleanup.md", "`CleanupPhase`", "FMT-CLEAN-001");
            fixture.WriteSpec("formatter/indentation.md", "`IndentationPhase`", "FMT-INDENT-001");

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, CreateExemptions(("formatter/", "Its tests are written in a follow-up.")));

            // Assert
            Assert.HasCount(2, findings, DescribeFindings("Findings", findings));
            Assert.Contains(finding => finding.StartsWith("FMT-CLEAN-001", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("FMT-INDENT-001", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that a requirement of an exempted <c>shared/</c> spec stays enforced when a spec that is not exempted
    /// lists it under <c>Uses</c>, even in an area no list names, while an unused requirement of the same shared spec
    /// is not enforced
    /// </summary>
    [TestMethod]
    public void CoverageCheckEnforcesExemptedSharedRequirementsThatAnEnforcedSpecUses()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("shared/policy.md", "`Policy`", "SHARED-P-001", "SHARED-P-002");
            fixture.WriteFile("sdd/specs/new-area/surface.md",
                              "# Surface\n\n| | |\n|---|---|\n| ID prefix | NEW |\n| Uses | SHARED-P-001 |\n\n## Requirements\n\n### NEW-001 — Title\n\nThe surface MUST follow SHARED-P-001.\n");
            fixture.WriteFile("src/Reihitsu.Cli.Test/SurfaceTests.cs", CreateTestSource("[Requirement(\"NEW-001\")]"));

            // Act
            var findings = FindUncoveredRequirements(fixture.Path, CreateExemptions(("shared/policy.md", "Its tests are written in a follow-up.")));

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("SHARED-P-001", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a requirement of an exempted <c>shared/</c> spec is not enforced when only an exempted spec lists
    /// it under <c>Uses</c>
    /// </summary>
    [TestMethod]
    public void CoverageCheckDoesNotEnforceExemptedSharedRequirementsThatOnlyAnExemptedSpecUses()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("shared/policy.md", "`Policy`", "SHARED-P-001");
            fixture.WriteFile("sdd/specs/cli/reihitsu-format.md",
                              "# CLI\n\n| | |\n|---|---|\n| ID prefix | CLI |\n| Uses | SHARED-P-001 |\n\n## Requirements\n\n### CLI-001 — Title\n\nThe CLI MUST follow SHARED-P-001.\n");

            // Act
            var findings = FindUncoveredRequirements(fixture.Path,
                                                     CreateExemptions(("shared/policy.md", "Its tests are written in a follow-up."),
                                                                      ("cli/reihitsu-format.md", "Its tests are written in a follow-up.")));

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
            var findings = FindUncoveredRequirements(fixture.Path, CreateExemptions());

            // Assert
            Assert.HasCount(3, findings);
            Assert.Contains(finding => finding.StartsWith("CLI-001", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("CLI-002", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("CLI-003", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that an exemption whose reason is empty is reported with the spec it names
    /// </summary>
    [TestMethod]
    public void ExemptionCheckReportsEmptyReason()
    {
        // Act
        var findings = FindExemptionsWithoutReason(CreateExemptions(("formatter/cleanup.md", string.Empty)));

        // Assert
        Assert.HasCount(1, findings);
        Assert.Contains("formatter/cleanup.md", findings[0]);
    }

    /// <summary>
    /// Verifies that an exemption whose reason is only whitespace is reported with the spec it names
    /// </summary>
    [TestMethod]
    public void ExemptionCheckReportsWhitespaceReason()
    {
        // Act
        var findings = FindExemptionsWithoutReason(CreateExemptions(("formatter/cleanup.md", " \t ")));

        // Assert
        Assert.HasCount(1, findings);
        Assert.Contains("formatter/cleanup.md", findings[0]);
    }

    /// <summary>
    /// Verifies that an exemption without any reason is reported with the spec it names
    /// </summary>
    [TestMethod]
    public void ExemptionCheckReportsMissingReason()
    {
        // Act
        var findings = FindExemptionsWithoutReason(CreateExemptions(("formatter/cleanup.md", null)));

        // Assert
        Assert.HasCount(1, findings);
        Assert.Contains("formatter/cleanup.md", findings[0]);
    }

    /// <summary>
    /// Verifies that only the exemptions without a reason are reported, and an exemption that states one is not
    /// </summary>
    [TestMethod]
    public void ExemptionCheckReportsOnlyExemptionsWithoutReason()
    {
        // Act
        var findings = FindExemptionsWithoutReason(CreateExemptions(("formatter/cleanup.md", "Its tests are written in a follow-up."),
                                                                    ("formatter/indentation.md", string.Empty)));

        // Assert
        Assert.HasCount(1, findings);
        Assert.Contains("formatter/indentation.md", findings[0]);
        Assert.DoesNotContain("formatter/cleanup.md", findings[0]);
    }

    /// <summary>
    /// Verifies that an empty exemption list has no exemption without a reason
    /// </summary>
    [TestMethod]
    public void ExemptionCheckAcceptsEmptyList()
    {
        // Act
        var findings = FindExemptionsWithoutReason(CreateExemptions());

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
    }

    /// <summary>
    /// Verifies that the report of exempted specs counts the uncovered requirements per exempted spec and leaves
    /// specs that are not exempted out, a <c>shared/</c> spec among them
    /// </summary>
    [TestMethod]
    public void CoverageReportCountsUncoveredRequirementsPerExemptedSpec()
    {
        // Arrange
        string report;

        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("shared/casing.md", "`CasingUtilities`", "SHARED-CASING-001", "SHARED-CASING-002");
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001");
            fixture.WriteSpec("shared/policy.md", "`Policy`", "SHARED-P-001");
            fixture.WriteFile("src/Reihitsu.Core.Test/CasingTests.cs", CreateTestSource("[Requirement(\"SHARED-CASING-001\")]"));

            // Act
            report = DescribeCoverageOfExemptedSpecs(fixture.Path, CreateExemptions(("shared/casing.md", "Its tests are written in a follow-up.")));
        }

        // Assert
        Assert.Contains("shared/casing.md: 1 of 2 uncovered (SHARED-CASING-002)", report);
        Assert.DoesNotContain("cli/reihitsu-format.md", report);
        Assert.DoesNotContain("shared/policy.md", report);
    }

    /// <summary>
    /// Verifies that the report of exempted specs names no spec when nothing is exempted, although a spec has an
    /// uncovered requirement
    /// </summary>
    [TestMethod]
    public void CoverageReportNamesNoSpecWhenNothingIsExempted()
    {
        // Arrange
        string report;

        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("shared/casing.md", "`CasingUtilities`", "SHARED-CASING-001");
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001");

            // Act
            report = DescribeCoverageOfExemptedSpecs(fixture.Path, CreateExemptions());
        }

        // Assert
        Assert.DoesNotContain("shared/casing.md", report);
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
    /// Finds every active requirement that no test method covers and that is enforced: its spec is not exempted, or it
    /// belongs to an exempted <c>shared/</c> spec and a spec that is not exempted lists it under <c>Uses</c>
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c> and <c>src</c></param>
    /// <param name="exemptedSpecs">Exempted spec files, relative to <c>sdd/specs</c>, mapped to their reasons</param>
    /// <returns>One message per uncovered requirement, starting with its ID</returns>
    private static List<string> FindUncoveredRequirements(string repositoryRoot, IReadOnlyDictionary<string, string> exemptedSpecs)
    {
        var coveredIds = ReadCoveredIds(repositoryRoot);
        var usedSharedIds = ReadSharedIdsUsedByEnforcedSpecs(repositoryRoot, exemptedSpecs);

        return ReadActiveRequirements(repositoryRoot).Where(requirement => IsEnforced(requirement, exemptedSpecs, usedSharedIds)
                                                                           && coveredIds.Contains(requirement.Id) == false)
                                                     .Select(requirement => $"{requirement.Id}: no test covers it ({requirement.SpecPath}:{requirement.Line})")
                                                     .ToList();
    }

    /// <summary>
    /// Finds every exemption that states no reason
    /// </summary>
    /// <param name="exemptedSpecs">Exempted spec files, relative to <c>sdd/specs</c>, mapped to their reasons</param>
    /// <returns>One message per exemption whose reason is missing, empty, or only whitespace, naming its spec</returns>
    private static List<string> FindExemptionsWithoutReason(IReadOnlyDictionary<string, string> exemptedSpecs)
    {
        return exemptedSpecs.Where(exemption => string.IsNullOrWhiteSpace(exemption.Value))
                            .Select(exemption => $"{exemption.Key}: the exemption states no reason")
                            .ToList();
    }

    /// <summary>
    /// Describes, per exempted spec, the active requirements no test covers
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c> and <c>src</c></param>
    /// <param name="exemptedSpecs">Exempted spec files, relative to <c>sdd/specs</c>, mapped to their reasons</param>
    /// <returns>The report, one line per exempted spec</returns>
    private static string DescribeCoverageOfExemptedSpecs(string repositoryRoot, IReadOnlyDictionary<string, string> exemptedSpecs)
    {
        var coveredIds = ReadCoveredIds(repositoryRoot);
        var lines = ReadActiveRequirements(repositoryRoot).Where(requirement => exemptedSpecs.ContainsKey(requirement.SpecPath))
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

        return lines.Count == 0
                   ? "Coverage of exempted specs: no spec is exempted"
                   : $"Coverage of exempted specs:{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
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
    /// Determines whether a requirement is enforced: its spec is not exempted, or it belongs to a <c>shared/</c> spec
    /// and a spec that is not exempted uses it
    /// </summary>
    /// <param name="requirement">Requirement</param>
    /// <param name="exemptedSpecs">Exempted spec files, relative to <c>sdd/specs</c>, mapped to their reasons</param>
    /// <param name="usedSharedIds">IDs that a spec that is not exempted lists under <c>Uses</c></param>
    /// <returns><see langword="true"/> when the requirement is enforced</returns>
    private static bool IsEnforced(SpecRequirement requirement, IReadOnlyDictionary<string, string> exemptedSpecs, HashSet<string> usedSharedIds)
    {
        return exemptedSpecs.ContainsKey(requirement.SpecPath) == false
               || (requirement.SpecPath.StartsWith(SharedDirectory, StringComparison.Ordinal)
                   && usedSharedIds.Contains(requirement.Id));
    }

    /// <summary>
    /// Reads the IDs that the specs that are not exempted list under <c>Uses</c>
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <param name="exemptedSpecs">Exempted spec files, relative to <c>sdd/specs</c>, mapped to their reasons</param>
    /// <returns>The used IDs</returns>
    private static HashSet<string> ReadSharedIdsUsedByEnforcedSpecs(string repositoryRoot, IReadOnlyDictionary<string, string> exemptedSpecs)
    {
        return SpecCatalog.ReadHeaders(Path.Combine(repositoryRoot, "sdd", "specs"))
                          .Where(header => exemptedSpecs.ContainsKey(header.SpecPath) == false)
                          .SelectMany(header => SpecCatalog.SplitCell(header.GetRow("Uses")))
                          .ToHashSet(StringComparer.Ordinal);
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
    /// Creates an exemption list for a fixture test
    /// </summary>
    /// <param name="exemptions">Exempted spec paths, relative to <c>sdd/specs</c>, each with its reason</param>
    /// <returns>The exemptions keyed by spec path</returns>
    private static IReadOnlyDictionary<string, string> CreateExemptions(params (string SpecPath, string Reason)[] exemptions)
    {
        return exemptions.ToDictionary(exemption => exemption.SpecPath, exemption => exemption.Reason, StringComparer.Ordinal);
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