using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Sdd;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.ArchitectureTests.Formatter;

/// <summary>
/// Guards the formatter phase ownership map in the repository instructions: it lists every runtime phase once and in
/// order, and every name in its guard column resolves to a declaration a formatter phase can call — in
/// <c>Reihitsu.Formatter</c> or a project it references — or to a Roslyn <see cref="SyntaxKind"/> member. A
/// backticked span of the guard column with no letter, or with a <c>/</c>, such as a token or a path, is no name and
/// is skipped
/// </summary>
[TestClass]
public sealed class PhaseOwnershipDocumentationTests
{
    #region Constants

    /// <summary>
    /// Marker before the ownership-map block
    /// </summary>
    private const string StartMarker = "<!-- formatter-phase-ownership:start -->";

    /// <summary>
    /// Marker after the ownership-map block
    /// </summary>
    private const string EndMarker = "<!-- formatter-phase-ownership:end -->";

    /// <summary>
    /// Project file of the formatter, relative to <c>src</c>, from which the projects whose declarations a guard
    /// name may resolve to are collected
    /// </summary>
    private const string FormatterProjectFile = "Reihitsu.Formatter/Reihitsu.Formatter.csproj";

    /// <summary>
    /// Zero-based index of the guard column in a row of the ownership table
    /// </summary>
    private const int GuardColumnIndex = 2;

    /// <summary>
    /// Matches one backtick-quoted span
    /// </summary>
    private static readonly Regex _quotedSpanPattern = new("`(?<span>[^`]+)`", RegexOptions.None, Regex.InfiniteMatchTimeout);

    /// <summary>
    /// Matches a plain or dotted name, such as <c>IsMultiLineSection</c> or <c>BlankLineEditor.Apply</c>
    /// </summary>
    private static readonly Regex _namePattern = new(@"^[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*$", RegexOptions.None, Regex.InfiniteMatchTimeout);

    /// <summary>
    /// Matches the separator row below the header of a Markdown table
    /// </summary>
    private static readonly Regex _separatorRowPattern = new(@"^\|[\s:|-]+\|$", RegexOptions.None, Regex.InfiniteMatchTimeout);

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that the map in <c>AGENTS.md</c> lists every runtime phase exactly once in order
    /// </summary>
    [TestMethod]
    public void OwnershipMapMatchesRuntimePhaseOrder()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var expectedBlock = ExtractOwnershipMap(Path.Combine(repositoryRoot, "AGENTS.md"));

        // Act
        var phaseNames = GetRuntimePhaseNames();

        // Assert
        Assert.HasCount(13, phaseNames);

        var previousPosition = -1;

        foreach (var phaseName in phaseNames)
        {
            var tableEntry = $"`{phaseName}`";
            var position = expectedBlock.IndexOf(tableEntry, StringComparison.Ordinal);

            Assert.AreEqual(1,
                            CountOccurrences(expectedBlock, tableEntry),
                            $"Expected exactly one ownership row for {phaseName}.");
            Assert.IsGreaterThan(previousPosition,
                                 position,
                                 $"Ownership row for {phaseName} is out of runtime order.");

            previousPosition = position;
        }
    }

    /// <summary>
    /// Verifies that every name in the guard column of the map in <c>AGENTS.md</c> resolves to a declaration in
    /// <c>Reihitsu.Formatter</c> or a project it references, or to a <see cref="SyntaxKind"/> member
    /// </summary>
    [TestMethod]
    public void EveryGuardNameInOwnershipMapResolves()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = FindUnresolvedGuardNames(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("Every name in the guard column of the phase ownership map must resolve", findings));
    }

    /// <summary>
    /// Verifies that names declared in the formatter, as a type, as a member, or as a member of a named type, resolve
    /// </summary>
    [TestMethod]
    public void GuardCheckAcceptsNamesDeclaredInFormatter()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`GuardUtilities`; `IsGuarded`; `GuardUtilities.IsGuarded`; `GuardUtilities.Threshold`; `GuardKind.Blocking`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Verifies that a member declared in another part of a partial type resolves as a member of that type
    /// </summary>
    [TestMethod]
    public void GuardCheckAcceptsMemberOfAnotherPartialPart()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`GuardUtilities.IsPartialGuard`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Verifies that names declared only in a project the formatter references resolve, as a plain and as a qualified
    /// name
    /// </summary>
    [TestMethod]
    public void GuardCheckAcceptsNamesDeclaredInReferencedProject()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`AreSeparatedByEndOfLine`; `CoreUtilities.AreSeparatedByEndOfLine`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Verifies that a name declared only in a project that a referenced project references resolves
    /// </summary>
    [TestMethod]
    public void GuardCheckAcceptsNameDeclaredInTransitivelyReferencedProject()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`IsBaseGuard`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Verifies that a name declared only in a production project the formatter does not reference fails, although
    /// that project is under <c>src</c>
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsNameDeclaredOnlyInUnreferencedProject()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`CanRewrite`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`CanRewrite`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a name declared in a project the formatter no longer references fails
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsNameOfProjectTheFormatterDoesNotReference()
    {
        // Arrange
        using (var fixture = CreateGuardFixtureWithoutCoreReference("`AreSeparatedByEndOfLine`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`AreSeparatedByEndOfLine`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a name no project declares fails, and the message names it and its phase
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsUndeclaredName()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`IsGuarded`; `IsAfterSourceOnSameLine`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`IsAfterSourceOnSameLine`", findings[0]);
            Assert.Contains("SamplePhase", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a name that the formatter's sources hold only in a comment or a string literal fails, because
    /// only declarations count
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsNamesOnlyInCommentOrString()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`IsMentionedOnly`; `IsQuotedOnly`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(2, findings, DescribeFindings("Findings", findings));
            Assert.Contains(finding => finding.Contains("`IsMentionedOnly`", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.Contains("`IsQuotedOnly`", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that a name declared only in a test project fails
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsNameDeclaredOnlyInTestProject()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`IsTestOnlyGuard`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`IsTestOnlyGuard`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a qualified name fails when the type exists but the member is declared on another type
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsMemberOfAnotherType()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`GuardUtilities.AreSeparatedByEndOfLine`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`GuardUtilities.AreSeparatedByEndOfLine`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a qualified name fails when its type is declared only outside the formatter and the projects it
    /// references, although the type declares the member
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsMemberOfTypeOutsideScope()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`CodeFixUtilities.CanRewrite`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`CodeFixUtilities.CanRewrite`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a <see cref="SyntaxKind"/> member resolves while a misspelled one fails
    /// </summary>
    [TestMethod]
    public void GuardCheckResolvesSyntaxKindMembers()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`EndOfLineTrivia`/`XmlTextLiteralNewLineToken`; `EndOfLineTrivai`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`EndOfLineTrivai`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that spans without a letter or with a slash, such as tokens and paths, are skipped
    /// </summary>
    [TestMethod]
    public void GuardCheckSkipsTokensAndPaths()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("between `?` or `:` and `sdd/specs/formatter/unsupported-syntax.md`; `IsGuarded`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Verifies that a span with letters that is no plain or dotted name, such as a call, fails rather than being
    /// skipped
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsDecoratedName()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`IsGuarded()`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`IsGuarded()`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a name continued behind a slash is checked as a plain name: it resolves when declared, and fails
    /// when not
    /// </summary>
    [TestMethod]
    public void GuardCheckChecksSlashContinuedNamesAsPlainNames()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`GuardUtilities.IsGuarded`/`AreSeparatedByEndOfLine`; `GuardUtilities.IsGuarded`/`IsRemovedGuard`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`IsRemovedGuard`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that a name declared only in the formatter's build output fails
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsNameDeclaredOnlyInBuildOutput()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`IsGeneratedGuard`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(1, findings, DescribeFindings("Findings", findings));
            Assert.Contains("`IsGeneratedGuard`", findings[0]);
        }
    }

    /// <summary>
    /// Verifies that every name that does not resolve is reported in one run, over several rows
    /// </summary>
    [TestMethod]
    public void GuardCheckReportsEveryUnresolvedName()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`CanRewrite`; `IsGuarded`; `IsAfterSourceOnSameLine`", "`IsRemovedGuard`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.HasCount(3, findings, DescribeFindings("Findings", findings));
            Assert.Contains(finding => finding.Contains("`CanRewrite`", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.Contains("`IsAfterSourceOnSameLine`", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.Contains("`IsRemovedGuard`", StringComparison.Ordinal) && finding.Contains("OtherPhase", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that only the guard column is checked: an undeclared name in the phase or decision column, or outside
    /// the marked map, is not reported
    /// </summary>
    [TestMethod]
    public void GuardCheckReadsOnlyTheGuardColumn()
    {
        // Arrange
        using (var fixture = CreateGuardFixture("`IsGuarded`"))
        {
            // Act
            var findings = FindUnresolvedGuardNames(fixture.Path);

            // Assert
            Assert.IsEmpty(findings, DescribeFindings("Unexpected findings", findings));
        }
    }

    /// <summary>
    /// Counts non-overlapping occurrences of a value in text
    /// </summary>
    /// <param name="text">Text to search</param>
    /// <param name="value">Value to count</param>
    /// <returns>Number of occurrences</returns>
    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var searchPosition = 0;

        while ((searchPosition = text.IndexOf(value, searchPosition, StringComparison.Ordinal)) >= 0)
        {
            count++;
            searchPosition += value.Length;
        }

        return count;
    }

    /// <summary>
    /// Extracts and normalizes the marked phase ownership block from an instruction file
    /// </summary>
    /// <param name="path">Instruction file path</param>
    /// <returns>The marked ownership block with LF line endings</returns>
    private static string ExtractOwnershipMap(string path)
    {
        var text = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
        var start = text.IndexOf(StartMarker, StringComparison.Ordinal);
        var end = text.IndexOf(EndMarker, StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, start, $"Missing start marker in {path}.");
        Assert.IsGreaterThan(start, end, $"Missing or misplaced end marker in {path}.");

        return text[start..(end + EndMarker.Length)];
    }

    /// <summary>
    /// Finds every backticked span in the guard column of the ownership map that names no declaration in
    /// <c>Reihitsu.Formatter</c> or a project it references and no <see cref="SyntaxKind"/> member. A span with no
    /// letter, or with a <c>/</c>, is skipped; any other span that is no plain or dotted name is reported
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>AGENTS.md</c> and <c>src</c></param>
    /// <returns>One message per span that does not resolve, naming its phase and the span</returns>
    private static List<string> FindUnresolvedGuardNames(string repositoryRoot)
    {
        var ownershipMap = ExtractOwnershipMap(Path.Combine(repositoryRoot, "AGENTS.md"));
        var declarations = CollectFormatterDeclarations(Path.Combine(repositoryRoot, "src"));
        var syntaxKinds = Enum.GetNames<SyntaxKind>().ToHashSet(StringComparer.Ordinal);
        var findings = new List<string>();

        foreach (var cells in ReadTableRows(ownershipMap))
        {
            var phase = cells[0];

            foreach (Match match in _quotedSpanPattern.Matches(cells[GuardColumnIndex]))
            {
                var span = match.Groups["span"].Value;

                if (span.Any(char.IsLetter) == false
                    || span.Contains('/', StringComparison.Ordinal))
                {
                    continue;
                }

                if (_namePattern.IsMatch(span) == false)
                {
                    findings.Add($"{phase}: `{span}` is no type or member name");
                }
                else if (Resolves(span, declarations, syntaxKinds) == false)
                {
                    findings.Add($"{phase}: `{span}` resolves to no declaration in Reihitsu.Formatter or a project it references, and to no SyntaxKind member");
                }
            }
        }

        return findings;
    }

    /// <summary>
    /// Reads the cells of every body row of the Markdown table in the ownership map, skipping its header and
    /// separator rows. A <c>|</c> inside a backticked span does not split a cell
    /// </summary>
    /// <param name="ownershipMap">The marked ownership block</param>
    /// <returns>The trimmed cells of each body row</returns>
    private static List<List<string>> ReadTableRows(string ownershipMap)
    {
        var rows = new List<List<string>>();
        var isHeader = true;

        foreach (var line in ownershipMap.Split('\n').Select(line => line.Trim()))
        {
            if (line.StartsWith('|') == false)
            {
                isHeader = true;

                continue;
            }

            if (isHeader)
            {
                isHeader = false;

                continue;
            }

            if (_separatorRowPattern.IsMatch(line))
            {
                continue;
            }

            var cells = SplitRow(line);

            Assert.IsGreaterThan(GuardColumnIndex, cells.Count, $"Ownership row has no guard column: {line}");

            rows.Add(cells);
        }

        return rows;
    }

    /// <summary>
    /// Splits a Markdown table row into its trimmed cells, ignoring a <c>|</c> inside a backticked span
    /// </summary>
    /// <param name="line">Table row, starting with <c>|</c></param>
    /// <returns>The cells between the outer pipes</returns>
    private static List<string> SplitRow(string line)
    {
        var cells = new List<string>();
        var cell = new StringBuilder();
        var isInSpan = false;

        foreach (var character in line.Trim('|'))
        {
            if (character == '`')
            {
                isInSpan = isInSpan == false;
            }

            if (character == '|'
                && isInSpan == false)
            {
                cells.Add(cell.ToString().Trim());
                cell.Clear();

                continue;
            }

            cell.Append(character);
        }

        cells.Add(cell.ToString().Trim());

        return cells;
    }

    /// <summary>
    /// Determines whether a plain or dotted name resolves. A plain name resolves when it is a declared type or member
    /// or a <see cref="SyntaxKind"/> member; in a dotted name, the first segment is a declared type and each further
    /// segment is a member that the type named by the segment before it declares
    /// </summary>
    /// <param name="name">Plain or dotted name</param>
    /// <param name="declarations">Declared members, keyed by the simple name of their declaring type</param>
    /// <param name="syntaxKinds">Names of the <see cref="SyntaxKind"/> members</param>
    /// <returns><see langword="true"/> when the name resolves</returns>
    private static bool Resolves(string name, Dictionary<string, HashSet<string>> declarations, HashSet<string> syntaxKinds)
    {
        var segments = name.Split('.');

        if (segments.Length == 1)
        {
            return syntaxKinds.Contains(name)
                   || declarations.ContainsKey(name)
                   || declarations.Values.Any(members => members.Contains(name));
        }

        for (var index = 1; index < segments.Length; index++)
        {
            if (declarations.TryGetValue(segments[index - 1], out var members) == false
                || members.Contains(segments[index]) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Collects the types and members declared in <c>Reihitsu.Formatter</c> and every project it references, directly
    /// or through another referenced project, read from their sources outside <c>bin</c> and <c>obj</c>. Comments and
    /// literals declare nothing
    /// </summary>
    /// <param name="sourceDirectory">The <c>src</c> directory</param>
    /// <returns>The member names of every declared type, keyed by the type's simple name; partial parts are merged</returns>
    private static Dictionary<string, HashSet<string>> CollectFormatterDeclarations(string sourceDirectory)
    {
        var declarations = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var projectDirectory in CollectFormatterProjectDirectories(sourceDirectory))
        {
            foreach (var file in Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories))
            {
                var segments = Path.GetRelativePath(projectDirectory, file).Replace('\\', '/').Split('/');

                if (segments.Contains("bin", StringComparer.Ordinal)
                    || segments.Contains("obj", StringComparer.Ordinal))
                {
                    continue;
                }

                AddDeclarations(CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot(), declarations);
            }
        }

        return declarations;
    }

    /// <summary>
    /// Adds the types and members declared in one source file. Comments and literals declare nothing
    /// </summary>
    /// <param name="root">Syntax root of the source file</param>
    /// <param name="declarations">The member names of every type declared so far, keyed by the type's simple name; partial parts are merged</param>
    private static void AddDeclarations(SyntaxNode root, Dictionary<string, HashSet<string>> declarations)
    {
        foreach (var type in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
            if (declarations.TryGetValue(type.Identifier.ValueText, out var members) == false)
            {
                members = new HashSet<string>(StringComparer.Ordinal);
                declarations.Add(type.Identifier.ValueText, members);
            }

            members.UnionWith(GetMemberNames(type));
        }

        foreach (var declaration in root.DescendantNodes().OfType<DelegateDeclarationSyntax>())
        {
            declarations.TryAdd(declaration.Identifier.ValueText, new HashSet<string>(StringComparer.Ordinal));
        }
    }

    /// <summary>
    /// Reads the names of the members a type declares directly: methods, properties, fields, events, enum members,
    /// and nested types and delegates
    /// </summary>
    /// <param name="type">Type declaration</param>
    /// <returns>The member names</returns>
    private static IEnumerable<string> GetMemberNames(BaseTypeDeclarationSyntax type)
    {
        if (type is EnumDeclarationSyntax enumDeclaration)
        {
            foreach (var member in enumDeclaration.Members)
            {
                yield return member.Identifier.ValueText;
            }

            yield break;
        }

        if (type is not TypeDeclarationSyntax typeDeclaration)
        {
            yield break;
        }

        foreach (var member in typeDeclaration.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    {
                        yield return method.Identifier.ValueText;
                    }
                    break;

                case PropertyDeclarationSyntax property:
                    {
                        yield return property.Identifier.ValueText;
                    }
                    break;

                case EventDeclarationSyntax eventDeclaration:
                    {
                        yield return eventDeclaration.Identifier.ValueText;
                    }
                    break;

                case BaseFieldDeclarationSyntax field:
                    {
                        foreach (var variable in field.Declaration.Variables)
                        {
                            yield return variable.Identifier.ValueText;
                        }
                    }
                    break;

                case BaseTypeDeclarationSyntax nestedType:
                    {
                        yield return nestedType.Identifier.ValueText;
                    }
                    break;

                case DelegateDeclarationSyntax nestedDelegate:
                    {
                        yield return nestedDelegate.Identifier.ValueText;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Collects the directories of <c>Reihitsu.Formatter</c> and of every project it references through
    /// <c>ProjectReference</c> items, directly or transitively
    /// </summary>
    /// <param name="sourceDirectory">The <c>src</c> directory</param>
    /// <returns>Full paths of the project directories</returns>
    private static List<string> CollectFormatterProjectDirectories(string sourceDirectory)
    {
        var pending = new Queue<string>([Path.GetFullPath(Path.Combine(sourceDirectory, FormatterProjectFile))]);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        while (pending.Count > 0)
        {
            var projectFile = pending.Dequeue();

            if (visited.Add(projectFile) == false)
            {
                continue;
            }

            Assert.IsTrue(File.Exists(projectFile), $"Missing project file {projectFile}.");

            var projectDirectory = Path.GetDirectoryName(projectFile);

            foreach (var reference in XDocument.Load(projectFile).Descendants("ProjectReference"))
            {
                var include = reference.Attribute("Include")?.Value;

                if (string.IsNullOrEmpty(include) == false)
                {
                    pending.Enqueue(Path.GetFullPath(Path.Combine(projectDirectory, include.Replace('\\', '/'))));
                }
            }
        }

        return visited.Select(Path.GetDirectoryName).ToList();
    }

    /// <summary>
    /// Creates a repository-shaped fixture whose <c>AGENTS.md</c> holds an ownership map with one row per guard cell,
    /// and whose <c>src</c> holds a formatter that references a core project, which references a base project, next to
    /// a code-fix project and a formatter test project that the formatter does not reference
    /// </summary>
    /// <param name="guardCells">
    /// Guard cell of each row; the first row belongs to <c>SamplePhase</c>, the second to
    /// <c>OtherPhase</c>
    /// </param>
    /// <returns>The fixture directory</returns>
    private static SpecFixtureDirectory CreateGuardFixture(params string[] guardCells)
    {
        return CreateGuardFixture(true, guardCells);
    }

    /// <summary>
    /// Creates the repository-shaped fixture of the guard-check tests
    /// </summary>
    /// <param name="formatterReferencesCore">Whether the formatter project references the core project</param>
    /// <param name="guardCells">Guard cell of each row</param>
    /// <returns>The fixture directory</returns>
    private static SpecFixtureDirectory CreateGuardFixture(bool formatterReferencesCore, string[] guardCells)
    {
        string[] phases = ["SamplePhase", "OtherPhase"];
        var map = new StringBuilder();

        map.Append("# Instructions\n\n| Phase | Decisions | Guards |\n|---|---|---|\n| `OutsidePhase` | none | `IsOutsideGuard` |\n\n")
           .Append(StartMarker)
           .Append("\n### Formatter phase ownership\n\nProse naming `IsProseGuard`.\n\n| Phase | Decisions it owns | Primary policy/guards |\n|---|---|---|\n");

        for (var index = 0; index < guardCells.Length; index++)
        {
            map.Append($"| `{phases[index]}` | Decides via `IsDecisionGuard` | {guardCells[index]} |\n");
        }

        map.Append(EndMarker)
           .Append('\n');

        var fixture = new SpecFixtureDirectory();

        fixture.WriteFile("AGENTS.md", map.ToString());
        fixture.WriteFile($"src/{FormatterProjectFile}",
                          formatterReferencesCore
                              ? "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <ProjectReference Include=\"..\\Reihitsu.Core\\Reihitsu.Core.csproj\" />\n  </ItemGroup>\n</Project>\n"
                              : "<Project Sdk=\"Microsoft.NET.Sdk\">\n</Project>\n");
        fixture.WriteFile("src/Reihitsu.Formatter/GuardUtilities.cs",
                          "// IsMentionedOnly is no declaration\ninternal static partial class GuardUtilities\n{\n    private const string Text = \"IsQuotedOnly\";\n    internal static int Threshold;\n    internal static bool IsGuarded() => true;\n}\n\ninternal enum GuardKind\n{\n    Blocking\n}\n");
        fixture.WriteFile("src/Reihitsu.Formatter/Partial/GuardUtilities.Partial.cs",
                          "internal static partial class GuardUtilities\n{\n    internal static bool IsPartialGuard() => true;\n}\n");
        fixture.WriteFile("src/Reihitsu.Formatter/obj/Generated.cs", "internal static class Generated\n{\n    internal static bool IsGeneratedGuard() => true;\n}\n");
        fixture.WriteFile("src/Reihitsu.Core/Reihitsu.Core.csproj",
                          "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <ProjectReference Include=\"..\\Reihitsu.Base\\Reihitsu.Base.csproj\" />\n  </ItemGroup>\n</Project>\n");
        fixture.WriteFile("src/Reihitsu.Core/CoreUtilities.cs", "internal static class CoreUtilities\n{\n    internal static bool AreSeparatedByEndOfLine() => true;\n}\n");
        fixture.WriteFile("src/Reihitsu.Base/Reihitsu.Base.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\">\n</Project>\n");
        fixture.WriteFile("src/Reihitsu.Base/BaseUtilities.cs", "internal static class BaseUtilities\n{\n    internal static bool IsBaseGuard() => true;\n}\n");
        fixture.WriteFile("src/Reihitsu.Analyzer.CodeFixes/Reihitsu.Analyzer.CodeFixes.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\">\n</Project>\n");
        fixture.WriteFile("src/Reihitsu.Analyzer.CodeFixes/CodeFixUtilities.cs", "internal static class CodeFixUtilities\n{\n    private static bool CanRewrite() => true;\n}\n");
        fixture.WriteFile("src/Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\">\n</Project>\n");
        fixture.WriteFile("src/Reihitsu.Formatter.Test/TestUtilities.cs", "internal static class TestUtilities\n{\n    internal static bool IsTestOnlyGuard() => true;\n}\n");

        return fixture;
    }

    /// <summary>
    /// Creates a repository-shaped fixture as <see cref="CreateGuardFixture(string[])"/> does, except that the formatter
    /// project references no project
    /// </summary>
    /// <param name="guardCell">Guard cell of the only row, which belongs to <c>SamplePhase</c></param>
    /// <returns>The fixture directory</returns>
    private static SpecFixtureDirectory CreateGuardFixtureWithoutCoreReference(string guardCell)
    {
        return CreateGuardFixture(false, [guardCell]);
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

    /// <summary>
    /// Obtains the top-level runtime phase names through the pipeline observer
    /// </summary>
    /// <returns>Phase names in execution order</returns>
    private static List<string> GetRuntimePhaseNames()
    {
        var tree = CSharpSyntaxTree.ParseText("internal class Sample { }");
        var phaseNames = new List<string>();

        FormattingPipeline.Execute(tree.GetRoot(),
                                   new FormattingContext("\n"),
                                   (phaseName, _, _) => phaseNames.Add(phaseName),
                                   CancellationToken.None);

        return phaseNames;
    }

    #endregion // Methods
}