using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// Guards the specs under <c>sdd/specs/</c> against drifting from the code and from each other: every type a
/// spec names as a code owner must exist in a production project, and every requirement ID must be unique
/// across all specs, as <c>sdd/README.md</c> requires
/// </summary>
[TestClass]
public sealed class SpecConsistencyTests
{
    #region Tests

    /// <summary>
    /// Verifies that every type named in the <c>Code owners</c> header of a spec is declared in a production
    /// project
    /// </summary>
    [TestMethod]
    public void EveryCodeOwnerNamesAnExistingType()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindCodeOwnerProblems(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Spec code owners must name existing production types", findings));
    }

    /// <summary>
    /// Verifies that no requirement ID is used by more than one requirement heading across all specs
    /// </summary>
    [TestMethod]
    public void EveryRequirementIdIsUniqueAcrossSpecs()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindDuplicateRequirementIds(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Requirement IDs must be unique across all specs", findings));
    }

    /// <summary>
    /// Verifies that a code owner without a declaration is reported with its spec and its name
    /// </summary>
    [TestMethod]
    public void CodeOwnerCheckReportsMissingTypeWithSpecAndName()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("formatter/cleanup.md", "`CleanupPhase`, `RemovedRewriter`");
            fixture.WriteFile("src/Reihitsu.Formatter/CleanupPhase.cs", "internal sealed class CleanupPhase { }\n");

            // Act
            var findings = FindCodeOwnerProblems(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("formatter/cleanup.md", findings[0]);
            Assert.Contains("`RemovedRewriter`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a generic code owner matches only a declaration with the same number of type parameters
    /// </summary>
    [TestMethod]
    public void CodeOwnerCheckMatchesGenericArity()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("analyzer/RH0001.md", "`Base<T>`, `Other<T>`");
            fixture.WriteFile("src/Reihitsu.Analyzer/Types.cs", "internal class Base<T> { }\ninternal class Other { }\n");

            // Act
            var findings = FindCodeOwnerProblems(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("`Other<T>`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that records, structs, interfaces, enums, delegates, nested types, and Razor components all
    /// count as declarations
    /// </summary>
    [TestMethod]
    public void CodeOwnerCheckAcceptsEveryKindOfTypeDeclaration()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("playground/playground.md", "`R`, `S`, `I`, `E`, `D`, `Nested`, `Home`");
            fixture.WriteFile("src/Reihitsu.Playground/Types.cs",
                              "internal record R;\ninternal struct S { }\ninternal interface I { }\ninternal enum E { }\ninternal delegate void D();\ninternal class Outer { private class Nested { } }\n");
            fixture.WriteFile("src/Reihitsu.Playground/Pages/Home.razor", "<h1>Home</h1>\n");

            // Act
            var findings = FindCodeOwnerProblems(fixture.Path);

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Verifies that types declared only in test projects or in build output do not satisfy a code owner
    /// </summary>
    [TestMethod]
    public void CodeOwnerCheckIgnoresTestProjectsAndBuildOutput()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("cli/reihitsu-format.md", "`FromTest`, `FromArchitectureTests`, `FromObj`, `FromBin`");
            fixture.WriteFile("src/Reihitsu.Cli.Test/FromTest.cs", "internal class FromTest { }\n");
            fixture.WriteFile("src/Reihitsu.ArchitectureTests/FromArchitectureTests.cs", "internal class FromArchitectureTests { }\n");
            fixture.WriteFile("src/Reihitsu.Cli/obj/Release/FromObj.cs", "internal class FromObj { }\n");
            fixture.WriteFile("src/Reihitsu.Cli/bin/Debug/FromBin.cs", "internal class FromBin { }\n");

            // Act
            var findings = FindCodeOwnerProblems(fixture.Path);

            // Assert
            Assert.HasCount(4, findings);
        }
    }

    /// <summary>
    /// Verifies that a spec without a <c>Code owners</c> header row is reported, and that the spec index is not
    /// read as a spec
    /// </summary>
    [TestMethod]
    public void CodeOwnerCheckReportsSpecWithoutCodeOwnersRow()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteFile("sdd/specs/README.md", "# Spec index\n");
            fixture.WriteFile("sdd/specs/shared/policy.md", "# Policy\n\n| | |\n|---|---|\n| ID prefix | SHARED-X |\n");
            fixture.WriteFile("src/Reihitsu.Core/Placeholder.cs", "internal class Placeholder { }\n");

            // Act
            var findings = FindCodeOwnerProblems(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("shared/policy.md", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that an ID used in two specs is reported with both locations
    /// </summary>
    [TestMethod]
    public void RequirementIdCheckReportsDuplicateAcrossSpecs()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("formatter/a.md", "`A`", "FMT-X-001", "FMT-X-002");
            fixture.WriteSpec("formatter/b.md", "`B`", "FMT-X-002", "FMT-Y-001");

            // Act
            var findings = FindDuplicateRequirementIds(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("FMT-X-002", findings[0]);
            Assert.Contains("formatter/a.md", findings[0]);
            Assert.Contains("formatter/b.md", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that an ID used twice within one spec is reported
    /// </summary>
    [TestMethod]
    public void RequirementIdCheckReportsDuplicateWithinSpec()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("cli/reihitsu-format.md", "`Program`", "CLI-001", "CLI-001");

            // Act
            var findings = FindDuplicateRequirementIds(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("CLI-001", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a heading-shaped line inside a fenced code block is not read as a requirement
    /// </summary>
    [TestMethod]
    public void RequirementScanSkipsFencedCodeBlocks()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteFile("sdd/specs/formatter/a.md", "### FMT-X-001 — Title\n\n```markdown\n### FMT-X-001 — Title\n```\n");

            // Act
            var requirements = SpecCatalog.ReadRequirements(Path.Combine(fixture.Path, "sdd", "specs"));

            // Assert
            Assert.HasCount(1, requirements);
            Assert.AreEqual(1, requirements[0].Line);
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Finds every spec header that has no readable code owners and every code owner that names no type
    /// declared in a production project
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c> and <c>src</c></param>
    /// <returns>One message per problem</returns>
    private static List<string> FindCodeOwnerProblems(string repositoryRoot)
    {
        var problems = new List<string>();
        var owners = SpecCatalog.ReadCodeOwners(Path.Combine(repositoryRoot, "sdd", "specs"), problems);
        var declaredTypes = SpecCatalog.CollectDeclaredTypes(Path.Combine(repositoryRoot, "src"));

        foreach (var owner in owners)
        {
            if (declaredTypes.Contains(SpecCatalog.GetTypeKey(owner.Name, owner.Arity)) == false)
            {
                problems.Add($"{owner.SpecPath}: code owner `{owner.Text}` is not declared in any production project under src/");
            }
        }

        return problems;
    }

    /// <summary>
    /// Finds every requirement ID that heads more than one requirement
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <returns>One message per duplicated ID, naming every location</returns>
    private static List<string> FindDuplicateRequirementIds(string repositoryRoot)
    {
        return SpecCatalog.ReadRequirements(Path.Combine(repositoryRoot, "sdd", "specs"))
                          .GroupBy(requirement => requirement.Id, StringComparer.Ordinal)
                          .Where(group => group.Count() > 1)
                          .Select(group => $"{group.Key}: {string.Join(", ", group.Select(requirement => $"{requirement.SpecPath}:{requirement.Line}"))}")
                          .ToList();
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