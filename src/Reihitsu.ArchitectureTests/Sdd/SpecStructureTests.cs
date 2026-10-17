using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// Guards the structure rules of <c>sdd/README.md</c> that keep many specs consistent with each other and with
/// the analyzer rules they describe: requirement IDs carry their spec's prefix, analyzer specs are named after an
/// existing rule and state its metadata, the index lists every spec, <c>Uses</c> lists exactly the referenced
/// requirements, the ledger stays sorted, and a type is a code owner of at most one spec
/// </summary>
[TestClass]
public sealed class SpecStructureTests
{
    #region Constants

    /// <summary>
    /// Spec directory of the analyzer rules, relative to <c>sdd/specs</c>
    /// </summary>
    private const string AnalyzerDirectory = "analyzer/";

    /// <summary>
    /// <c>Code fix</c> header value of a rule without a code fix
    /// </summary>
    private const string NoCodeFix = "None";

    /// <summary>
    /// <c>Code fix</c> header value of a rule whose code fix offers Fix All
    /// </summary>
    private const string CodeFixWithFixAll = "Fix All";

    /// <summary>
    /// <c>Code fix</c> header value of a rule whose code fix offers no Fix All
    /// </summary>
    private const string CodeFixWithoutFixAll = "Without Fix All";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Matches the file name of an analyzer spec, which is the rule ID
    /// </summary>
    private static readonly Regex _ruleIdPattern = new(@"^RH[0-9]{4}[A-Z]?$");

    #endregion // Fields

    #region Tests

    /// <summary>
    /// Verifies that every requirement ID is its spec's ID prefix followed by a three-digit number
    /// </summary>
    [TestMethod]
    public void EveryRequirementIdCarriesItsSpecPrefix()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindIdPrefixProblems(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Every requirement ID must be its spec's ID prefix followed by -NNN", findings));
    }

    /// <summary>
    /// Verifies that the spec index lists every spec once, with the prefix and code owners of its header
    /// </summary>
    [TestMethod]
    public void SpecIndexListsEverySpecWithItsHeader()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindIndexProblems(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings($"sdd/specs/{SpecCatalog.IndexFileName} must list every spec with its header's prefix and code owners", findings));
    }

    /// <summary>
    /// Verifies that the <c>Uses</c> row of every spec lists exactly the active requirements of other specs that
    /// the spec references outside its non-goals
    /// </summary>
    [TestMethod]
    public void UsesListsExactlyTheReferencedRequirements()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindUsesProblems(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("The Uses row must list exactly the active requirements of other specs that the spec references outside its non-goals", findings));
    }

    /// <summary>
    /// Verifies that the requirement-ID ledger is in ordinal sort order
    /// </summary>
    [TestMethod]
    public void LedgerIsSorted()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindLedgerOrderProblems(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings($"sdd/specs/{SpecCatalog.LedgerFileName} must be sorted; insert a new ID at its sorted position", findings));
    }

    /// <summary>
    /// Verifies that no type is a code owner of more than one spec
    /// </summary>
    [TestMethod]
    public void EveryTypeIsCodeOwnerOfAtMostOneSpec()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindSharedCodeOwners(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("A type is a code owner of at most one spec", findings));
    }

    /// <summary>
    /// Verifies that every analyzer spec is named after an existing rule, states the rule's descriptor and code
    /// fix in its header, and names the rule's analyzer and code-fix providers as code owners
    /// </summary>
    [TestMethod]
    public void AnalyzerSpecHeaderMatchesItsRule()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var rules = DiscoverRules();

        // Act
        var findings = FindAnalyzerHeaderProblems(repositoryRoot, rules);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Every analyzer spec must be named after its rule and match the rule's descriptor and code fix", findings));
    }

    /// <summary>
    /// Verifies that an ID with another prefix and an ID without a three-digit number are both reported
    /// </summary>
    [TestMethod]
    public void IdPrefixCheckReportsForeignPrefixAndMalformedNumber()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteFile("sdd/specs/formatter/a.md", CreateSpec("FMT-A", "—", "FMT-A-001", "FMT-B-002", "FMT-A-03"));

            // Act
            var findings = FindIdPrefixProblems(fixture.Path);

            // Assert
            Assert.HasCount(2, findings);
            Assert.Contains(finding => finding.Contains("FMT-B-002", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.Contains("FMT-A-03", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that a missing and an unused <c>Uses</c> entry are reported, and that a reference in the non-goals
    /// is not a use
    /// </summary>
    [TestMethod]
    public void UsesCheckReportsMissingAndUnusedEntries()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteFile("sdd/specs/shared/s.md", CreateSpec("SHARED-S", "—", "SHARED-S-001", "SHARED-S-002", "SHARED-S-003"));
            fixture.WriteFile("sdd/specs/formatter/a.md",
                              "# A\n\n| | |\n|---|---|\n| ID prefix | FMT-A |\n| Uses | SHARED-S-002 |\n\nIntro uses SHARED-S-001.\n\n"
                              + "## Non-goals\n\n- SHARED-S-003 decides that.\n\n## Requirements\n\n### FMT-A-001 — Title\n\nThe formatter MUST do it.\n");

            // Act
            var findings = FindUsesProblems(fixture.Path);

            // Assert
            Assert.HasCount(2, findings);
            Assert.Contains(finding => finding.Contains("SHARED-S-001", StringComparison.Ordinal) && finding.Contains("missing", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.Contains("SHARED-S-002", StringComparison.Ordinal) && finding.Contains("not referenced", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that a <c>Uses</c> entry naming a retired requirement is reported
    /// </summary>
    [TestMethod]
    public void UsesCheckReportsRetiredRequirement()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteFile("sdd/specs/shared/s.md",
                              "# S\n\n| | |\n|---|---|\n| ID prefix | SHARED-S |\n| Uses | — |\n\n## Requirements\n\n### SHARED-S-001 — Title\n\nStatus: Retired — reason.\n");
            fixture.WriteFile("sdd/specs/formatter/a.md",
                              "# A\n\n| | |\n|---|---|\n| ID prefix | FMT-A |\n| Uses | SHARED-S-001 |\n\n## Requirements\n\n### FMT-A-001 — Title\n\nThe formatter MUST follow SHARED-S-001.\n");

            // Act
            var findings = FindUsesProblems(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("retired", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that an ID out of sort order is reported
    /// </summary>
    [TestMethod]
    public void LedgerOrderCheckReportsUnsortedId()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteFile("sdd/specs/requirement-ids.txt", "CLI-001\nCLI-003\nCLI-002\n");

            // Act
            var findings = FindLedgerOrderProblems(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("CLI-002", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a type named by two specs is reported with both specs
    /// </summary>
    [TestMethod]
    public void CodeOwnerOwnershipCheckReportsTypeOfTwoSpecs()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteSpec("formatter/a.md", "`Shared`, `OnlyA`");
            fixture.WriteSpec("shared/b.md", "`Shared`");

            // Act
            var findings = FindSharedCodeOwners(fixture.Path);

            // Assert
            Assert.HasCount(1, findings);
            Assert.Contains("`Shared`", findings[0]);
            Assert.Contains("formatter/a.md", findings[0]);
            Assert.Contains("shared/b.md", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a spec missing from the index and an index row whose owners differ from the header are both
    /// reported
    /// </summary>
    [TestMethod]
    public void IndexCheckReportsMissingSpecAndStaleRow()
    {
        // Arrange
        using (var fixture = new SpecFixtureDirectory())
        {
            fixture.WriteFile("sdd/specs/formatter/a.md", CreateSpec("FMT-A", "`A`", "FMT-A-001"));
            fixture.WriteFile("sdd/specs/formatter/b.md", CreateSpec("FMT-B", "`B`", "FMT-B-001"));
            fixture.WriteFile("sdd/specs/README.md", "| Spec | ID prefix | Code owners |\n|---|---|---|\n| [formatter/a.md](formatter/a.md) | FMT-A | `Old` |\n");

            // Act
            var findings = FindIndexProblems(fixture.Path);

            // Assert
            Assert.HasCount(2, findings);
            Assert.Contains(finding => finding.Contains("formatter/a.md", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.Contains("formatter/b.md", StringComparison.Ordinal), findings);
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Finds every requirement ID that is not its spec's prefix followed by a three-digit number
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <returns>One message per malformed ID</returns>
    private static List<string> FindIdPrefixProblems(string repositoryRoot)
    {
        var specsDirectory = Path.Combine(repositoryRoot, "sdd", "specs");
        var prefixes = SpecCatalog.ReadHeaders(specsDirectory)
                                  .ToDictionary(header => header.SpecPath, header => header.GetRow("ID prefix"), StringComparer.Ordinal);
        var problems = new List<string>();

        foreach (var requirement in SpecCatalog.ReadRequirements(specsDirectory))
        {
            var prefix = prefixes.GetValueOrDefault(requirement.SpecPath);

            if (prefix == null
                || Regex.IsMatch(requirement.Id, $"^{Regex.Escape(prefix)}-[0-9]{{3}}$") == false)
            {
                problems.Add($"{requirement.SpecPath}:{requirement.Line}: {requirement.Id} is not '{prefix}-NNN'");
            }
        }

        return problems;
    }

    /// <summary>
    /// Compares the spec index with the spec headers
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <returns>One message per spec missing from the index, per row without a spec, and per row that differs from its header</returns>
    private static List<string> FindIndexProblems(string repositoryRoot)
    {
        var specsDirectory = Path.Combine(repositoryRoot, "sdd", "specs");
        var headers = SpecCatalog.ReadHeaders(specsDirectory)
                                 .ToDictionary(header => header.SpecPath, StringComparer.Ordinal);
        var rows = SpecCatalog.ReadIndex(specsDirectory);
        var problems = new List<string>();

        problems.AddRange(rows.GroupBy(row => row.SpecPath, StringComparer.Ordinal)
                              .Where(group => group.Count() > 1)
                              .Select(group => $"{group.Key}: listed more than once"));

        foreach (var row in rows)
        {
            if (headers.TryGetValue(row.SpecPath, out var header) == false)
            {
                problems.Add($"{row.SpecPath}: listed in the index, but no such spec exists");
            }
            else if (row.Prefix != header.GetRow("ID prefix")
                     || row.CodeOwners != header.GetRow("Code owners"))
            {
                problems.Add($"{row.SpecPath}: the index row differs from the spec header's ID prefix or code owners");
            }
        }

        var listed = rows.Select(row => row.SpecPath)
                         .ToHashSet(StringComparer.Ordinal);

        problems.AddRange(headers.Keys.Where(specPath => listed.Contains(specPath) == false)
                                      .OrderBy(specPath => specPath, StringComparer.Ordinal)
                                      .Select(specPath => $"{specPath}: not listed in the index"));

        return problems;
    }

    /// <summary>
    /// Compares the <c>Uses</c> row of every spec with the requirements of other specs it references outside its
    /// non-goals
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <returns>One message per missing, unused, unknown, or retired entry</returns>
    private static List<string> FindUsesProblems(string repositoryRoot)
    {
        var specsDirectory = Path.Combine(repositoryRoot, "sdd", "specs");
        var requirements = SpecCatalog.ReadRequirements(specsDirectory)
                                      .GroupBy(requirement => requirement.Id, StringComparer.Ordinal)
                                      .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var mentions = SpecCatalog.ReadMentionedRequirementIds(specsDirectory);
        var problems = new List<string>();

        foreach (var header in SpecCatalog.ReadHeaders(specsDirectory))
        {
            var prefix = header.GetRow("ID prefix") ?? string.Empty;
            var uses = SpecCatalog.SplitCell(header.GetRow("Uses"))
                                  .ToHashSet(StringComparer.Ordinal);
            var referenced = mentions.GetValueOrDefault(header.SpecPath, [])
                                     .Where(id => id.StartsWith($"{prefix}-", StringComparison.Ordinal) == false)
                                     .ToHashSet(StringComparer.Ordinal);

            foreach (var id in referenced.Where(id => uses.Contains(id) == false).OrderBy(id => id, StringComparer.Ordinal))
            {
                problems.Add($"{header.SpecPath}: {id} is referenced but missing from Uses");
            }

            foreach (var id in uses.Where(id => referenced.Contains(id) == false).OrderBy(id => id, StringComparer.Ordinal))
            {
                problems.Add($"{header.SpecPath}: {id} is in Uses but not referenced outside the non-goals");
            }

            foreach (var id in uses.Union(referenced).OrderBy(id => id, StringComparer.Ordinal))
            {
                if (requirements.TryGetValue(id, out var requirement) == false)
                {
                    problems.Add($"{header.SpecPath}: {id} heads no requirement");
                }
                else if (requirement.IsRetired)
                {
                    problems.Add($"{header.SpecPath}: {id} is retired ({requirement.SpecPath}:{requirement.Line})");
                }
            }
        }

        return problems;
    }

    /// <summary>
    /// Finds every ledger entry that sorts before the entry above it
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <returns>One message per entry out of order</returns>
    private static List<string> FindLedgerOrderProblems(string repositoryRoot)
    {
        var ledger = SpecCatalog.ReadLedger(Path.Combine(repositoryRoot, "sdd", "specs"));

        return ledger.Zip(ledger.Skip(1))
                     .Where(pair => string.CompareOrdinal(pair.First.Id, pair.Second.Id) > 0)
                     .Select(pair => $"line {pair.Second.Line}: {pair.Second.Id} sorts before {pair.First.Id}")
                     .ToList();
    }

    /// <summary>
    /// Finds every type that more than one spec names as a code owner
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <returns>One message per type, naming every spec that owns it</returns>
    private static List<string> FindSharedCodeOwners(string repositoryRoot)
    {
        return SpecCatalog.ReadCodeOwners(Path.Combine(repositoryRoot, "sdd", "specs"), [])
                          .GroupBy(owner => owner.Text, StringComparer.Ordinal)
                          .Where(group => group.Select(owner => owner.SpecPath).Distinct(StringComparer.Ordinal).Count() > 1)
                          .Select(group => $"`{group.Key}`: {string.Join(", ", group.Select(owner => owner.SpecPath).Distinct(StringComparer.Ordinal))}")
                          .ToList();
    }

    /// <summary>
    /// Compares the header of every analyzer spec with the rule it is named after
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>sdd/specs</c></param>
    /// <param name="rules">The shipped rules by diagnostic ID</param>
    /// <returns>One message per mismatch</returns>
    private static List<string> FindAnalyzerHeaderProblems(string repositoryRoot, IReadOnlyDictionary<string, DiscoveredRule> rules)
    {
        var problems = new List<string>();

        foreach (var header in SpecCatalog.ReadHeaders(Path.Combine(repositoryRoot, "sdd", "specs"))
                                          .Where(header => header.SpecPath.StartsWith(AnalyzerDirectory, StringComparison.Ordinal)))
        {
            var ruleId = Path.GetFileNameWithoutExtension(header.SpecPath);

            if (_ruleIdPattern.IsMatch(ruleId) == false
                || rules.TryGetValue(ruleId, out var rule) == false)
            {
                problems.Add($"{header.SpecPath}: an analyzer spec must be named after an existing rule ID");

                continue;
            }

            CompareRow(problems, header, "ID prefix", ruleId);
            CompareRow(problems, header, "Category", rule.Category);
            CompareRow(problems, header, "Severity", rule.Severity);
            CompareRow(problems, header, "Enabled by default", rule.IsEnabledByDefault ? "Yes" : "No");
            CompareRow(problems, header, "Code fix", rule.CodeFix);

            var owners = SpecCatalog.SplitCell(header.GetRow("Code owners"))
                                    .Select(owner => owner.Trim('`'))
                                    .Select(owner => owner.Split('<')[0])
                                    .ToHashSet(StringComparer.Ordinal);

            foreach (var typeName in rule.TypeNames.Where(typeName => owners.Contains(typeName) == false))
            {
                problems.Add($"{header.SpecPath}: `{typeName}` implements the rule but is not a code owner");
            }
        }

        return problems;
    }

    /// <summary>
    /// Adds a message when a header row differs from the expected value
    /// </summary>
    /// <param name="problems">Receives the message</param>
    /// <param name="header">Spec header</param>
    /// <param name="label">Row label</param>
    /// <param name="expected">Expected cell text</param>
    private static void CompareRow(List<string> problems, SpecHeader header, string label, string expected)
    {
        var actual = header.GetRow(label);

        if (actual != expected)
        {
            problems.Add($"{header.SpecPath}: '{label}' is '{actual ?? "missing"}', the rule says '{expected}'");
        }
    }

    /// <summary>
    /// Discovers the shipped rules from the analyzer and code-fix assemblies
    /// </summary>
    /// <returns>The rules by diagnostic ID</returns>
    private static Dictionary<string, DiscoveredRule> DiscoverRules()
    {
        var codeFixProviders = CreateInstances<CodeFixProvider>(Assembly.Load("Reihitsu.Analyzer.CodeFixes"));
        var codeFixes = codeFixProviders.SelectMany(provider => provider.FixableDiagnosticIds.Select(id => (Id: id, Provider: provider)))
                                        .GroupBy(entry => entry.Id, StringComparer.Ordinal)
                                        .ToDictionary(group => group.Key, group => group.Select(entry => entry.Provider).ToList(), StringComparer.Ordinal);
        var rules = new Dictionary<string, DiscoveredRule>(StringComparer.Ordinal);

        foreach (var analyzer in CreateInstances<DiagnosticAnalyzer>(Assembly.Load("Reihitsu.Analyzer")))
        {
            foreach (var descriptor in analyzer.SupportedDiagnostics)
            {
                var providers = codeFixes.GetValueOrDefault(descriptor.Id, []);
                var codeFix = GetCodeFixKind(providers);
                var typeNames = providers.Select(provider => provider.GetType().Name)
                                         .Prepend(analyzer.GetType().Name)
                                         .ToList();

                rules[descriptor.Id] = new DiscoveredRule(descriptor.Category, descriptor.DefaultSeverity.ToString(), descriptor.IsEnabledByDefault, codeFix, typeNames);
            }
        }

        return rules;
    }

    /// <summary>
    /// Gets the <c>Code fix</c> header value that the code-fix providers of a rule call for
    /// </summary>
    /// <param name="providers">The rule's code-fix providers</param>
    /// <returns><see cref="NoCodeFix"/>, <see cref="CodeFixWithFixAll"/>, or <see cref="CodeFixWithoutFixAll"/></returns>
    private static string GetCodeFixKind(List<CodeFixProvider> providers)
    {
        if (providers.Count == 0)
        {
            return NoCodeFix;
        }

        return providers.Any(provider => provider.GetFixAllProvider() != null)
                   ? CodeFixWithFixAll
                   : CodeFixWithoutFixAll;
    }

    /// <summary>
    /// Creates one instance of every concrete type of an assembly that derives from the given base type and has a
    /// parameterless constructor
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    /// <param name="assembly">Assembly to search</param>
    /// <returns>The instances</returns>
    private static IEnumerable<T> CreateInstances<T>(Assembly assembly)
        where T : class
    {
        return assembly.GetTypes()
                       .Where(type => typeof(T).IsAssignableFrom(type)
                                      && type.IsAbstract == false
                                      && type.ContainsGenericParameters == false
                                      && type.GetConstructor(Type.EmptyTypes) != null)
                       .Select(type => (T)Activator.CreateInstance(type));
    }

    /// <summary>
    /// Creates the source of a minimal spec
    /// </summary>
    /// <param name="prefix">ID prefix</param>
    /// <param name="codeOwners">Content of the <c>Code owners</c> cell</param>
    /// <param name="requirementIds">Requirement IDs, one heading each</param>
    /// <returns>The spec source</returns>
    private static string CreateSpec(string prefix, string codeOwners, params string[] requirementIds)
    {
        return $"# Spec\n\n| | |\n|---|---|\n| ID prefix | {prefix} |\n| Status | Active |\n| Code owners | {codeOwners} |\n| Uses | — |\n\n## Requirements\n"
               + string.Concat(requirementIds.Select(id => $"\n### {id} — Title\n\nThe formatter MUST do it.\n"));
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