using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards that no C# comment or declared name in the repository references this repository's own tracker
/// numbers. A tracker number pins a comment or a name to an entry that can be renumbered, closed, or deleted,
/// and it carries no information a reader can act on without leaving the editor; the durable behavior,
/// invariant, or constraint belongs in the text itself instead. Comments are read as Roslyn trivia so a
/// tracker-shaped fixture inside a string literal is never mistaken for a live reference, and declared names
/// are read from the syntax tree for the same reason. A link to another project's tracker, written as a full
/// GitHub URL, stays legitimate and is exempt
/// </summary>
[TestClass]
public sealed class SelfReferentialTrackerReferenceTests
{
    #region Constants

    /// <summary>
    /// Owner segment of this repository's GitHub identity
    /// </summary>
    private const string RepositoryOwner = "thoenissen";

    /// <summary>
    /// Repository-name segment of this repository's GitHub identity
    /// </summary>
    private const string RepositoryName = "Reihitsu";

    /// <summary>
    /// File-name suffixes that mark generated source excluded from the scan
    /// </summary>
    private static readonly string[] _generatedFileSuffixes = [".Designer.cs", ".g.cs", ".g.i.cs"];

    /// <summary>
    /// Matches a GitHub issue or pull-request URL and captures its owner, repository, and number
    /// </summary>
    private static readonly Regex _trackerUrlPattern = new(@"https://github\.com/([A-Za-z0-9._-]+)/([A-Za-z0-9._-]+)/(?:issues|pull)/(\d+)");

    /// <summary>
    /// Matches a bare tracker number — a hash mark directly followed by digits — which carries no
    /// repository qualifier and is therefore never exempt
    /// </summary>
    private static readonly Regex _bareTrackerNumberPattern = new(@"#\d+");

    /// <summary>
    /// Matches a tracker number embedded in a declared name or file name
    /// </summary>
    private static readonly Regex _trackerNamePattern = new(@"(?:Issue|Bug|Ticket|PR)\d+");

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that no comment or documentation comment in the repository references this repository's own
    /// tracker numbers
    /// </summary>
    [TestMethod]
    public void RepositoryCommentsContainNoSelfReferentialTrackerReferences()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = ScanForCommentReferences(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("comment", findings));
    }

    /// <summary>
    /// Verifies that no declared type, member, or file name in the repository references this repository's
    /// own tracker numbers
    /// </summary>
    [TestMethod]
    public void RepositoryDeclaredNamesContainNoSelfReferentialTrackerReferences()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = ScanForNameReferences(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings("name", findings));
    }

    /// <summary>
    /// Verifies that the comment scan reports the <c>issue #N</c> keyword form, the <c>PR #N</c> form, and a
    /// bare continuation reference, each at its own line
    /// </summary>
    [TestMethod]
    public void CommentScanDetectsIssueKeywordPrTagAndBareContinuation()
    {
        // Arrange and act
        IReadOnlyList<TrackerReference> findings;

        using (var fixture = new TrackerReferenceFixtureDirectory())
        {
            fixture.WriteFile("Sample.cs", "internal class Sample\n{\n    // issue #123\n    // PR #721\n    // #636).\n}\n");

            findings = ScanForCommentReferences(fixture.Path);
        }

        // Assert
        Assert.HasCount(3, findings);
        Assert.Contains(finding => finding.Line == 3 && finding.MatchedText == "#123", findings);
        Assert.Contains(finding => finding.Line == 4 && finding.MatchedText == "#721", findings);
        Assert.Contains(finding => finding.Line == 5 && finding.MatchedText == "#636", findings);
    }

    /// <summary>
    /// Verifies that the comment scan does not mistake an analyzer rule identifier, a compiler diagnostic
    /// identifier, a target-framework moniker, or a directive for a tracker reference
    /// </summary>
    [TestMethod]
    public void CommentScanIgnoresRuleIdentifiersDirectivesAndFrameworkVersion()
    {
        // Arrange and act
        IReadOnlyList<TrackerReference> findings;

        using (var fixture = new TrackerReferenceFixtureDirectory())
        {
            fixture.WriteFile("Sample.cs",
                              "#nullable enable\n"
                              + "#region Constants\n"
                              + "// RH5201 measures the reference column; CS8632 governs nullable annotations; targets net10.0\n"
                              + "#endregion // Constants\n"
                              + "internal class Sample\n{\n}\n");

            findings = ScanForCommentReferences(fixture.Path);
        }

        // Assert
        Assert.IsEmpty(findings);
    }

    /// <summary>
    /// Verifies that a tracker-shaped sequence living inside a string literal is not mistaken for a real
    /// comment, while the byte-identical text as an actual comment is still reported
    /// </summary>
    [TestMethod]
    public void CommentScanIgnoresRawStringFixtureContent()
    {
        // Arrange and act
        IReadOnlyList<TrackerReference> findings;

        using (var fixture = new TrackerReferenceFixtureDirectory())
        {
            fixture.WriteFile("Sample.cs",
                              "internal class Sample\n"
                              + "{\n"
                              + "    // issue #123\n"
                              + "    private const string _fixture = \"\"\"\n"
                              + "        // issue #123\n"
                              + "        \"\"\";\n"
                              + "}\n");

            findings = ScanForCommentReferences(fixture.Path);
        }

        // Assert
        Assert.HasCount(1, findings);
        Assert.Contains(finding => finding.Line == 3, findings);
    }

    /// <summary>
    /// Verifies that build output and generated files are excluded from the comment scan regardless of
    /// their content
    /// </summary>
    [TestMethod]
    public void CommentScanExcludesBuildOutputAndGeneratedFiles()
    {
        // Arrange and act
        IReadOnlyList<TrackerReference> findings;
        string trackedRelativePath;

        using (var fixture = new TrackerReferenceFixtureDirectory())
        {
            fixture.WriteFile(Path.Combine("obj", "Release", "Sample.cs"), "internal class Sample\n{\n    // issue #123\n}\n");
            fixture.WriteFile(Path.Combine("bin", "Debug", "Sample.cs"), "internal class Sample\n{\n    // issue #123\n}\n");
            fixture.WriteFile("Sample.Designer.cs", "internal class Sample\n{\n    // issue #123\n}\n");

            var trackedPath = fixture.WriteFile(Path.Combine("Project", "Sample.cs"), "internal class Sample\n{\n    // issue #123\n}\n");

            trackedRelativePath = Path.GetRelativePath(fixture.Path, trackedPath);
            findings = ScanForCommentReferences(fixture.Path);
        }

        // Assert
        Assert.HasCount(1, findings);
        Assert.Contains(finding => finding.FilePath == trackedRelativePath, findings);
    }

    /// <summary>
    /// Verifies that a link to another project's tracker is exempt while the same URL shape pointing at this
    /// repository is reported
    /// </summary>
    [TestMethod]
    public void CommentScanExemptsForeignRepositoryUrlButFlagsOwnRepositoryUrl()
    {
        // Arrange and act
        IReadOnlyList<TrackerReference> findings;

        using (var fixture = new TrackerReferenceFixtureDirectory())
        {
            fixture.WriteFile("Sample.cs",
                              "internal class Sample\n"
                              + "{\n"
                              + "    // Workaround for https://github.com/dotnet/roslyn/issues/41610\n"
                              + "    // see https://github.com/thoenissen/Reihitsu/issues/720\n"
                              + "}\n");

            findings = ScanForCommentReferences(fixture.Path);
        }

        // Assert
        Assert.HasCount(1, findings);
        Assert.Contains(finding => finding.Line == 4 && finding.MatchedText.EndsWith("/issues/720", StringComparison.Ordinal), findings);
    }

    /// <summary>
    /// Verifies that a bare tracker number in prose about a foreign project is still reported, because a
    /// bare number carries no repository qualifier
    /// </summary>
    [TestMethod]
    public void CommentScanFlagsBareTrackerNumberInProseAboutForeignProject()
    {
        // Arrange and act
        IReadOnlyList<TrackerReference> findings;

        using (var fixture = new TrackerReferenceFixtureDirectory())
        {
            fixture.WriteFile("Sample.cs", "internal class Sample\n{\n    // Roslyn issue #41610\n}\n");

            findings = ScanForCommentReferences(fixture.Path);
        }

        // Assert
        Assert.HasCount(1, findings);
        Assert.Contains(finding => finding.Line == 3 && finding.MatchedText == "#41610", findings);
    }

    /// <summary>
    /// Verifies that the name scan reports a tracker number embedded in a class name, a method name, and a
    /// file name
    /// </summary>
    [TestMethod]
    public void NameScanDetectsTrackerNumberInClassMethodAndFileName()
    {
        // Arrange and act
        IReadOnlyList<TrackerReference> findings;

        using (var fixture = new TrackerReferenceFixtureDirectory())
        {
            fixture.WriteFile("Issue999Tests.cs", "internal class Issue999Tests\n{\n    public void VerifyIssue999Works()\n    {\n    }\n}\n");

            findings = ScanForNameReferences(fixture.Path);
        }

        // Assert
        Assert.HasCount(3, findings);
        Assert.Contains(finding => finding.MatchedText == "Issue999" && finding.Line == 1, findings);
        Assert.Contains(finding => finding.MatchedText == "Issue999" && finding.Line == 3, findings);
    }

    /// <summary>
    /// Verifies that the name scan does not mistake a rule-identifier-named class, a framework-named class, or
    /// a tracker-shaped string literal for a declared tracker reference
    /// </summary>
    [TestMethod]
    public void NameScanIgnoresRuleNamedClassesAndFixtureStringContent()
    {
        // Arrange and act
        IReadOnlyList<TrackerReference> findings;

        using (var fixture = new TrackerReferenceFixtureDirectory())
        {
            fixture.WriteFile("Sample.cs",
                              "internal class RH5201MethodChainsShouldBeAlignedAnalyzerTests\n"
                              + "{\n"
                              + "    private const string _fixture = \"Issue764\";\n"
                              + "}\n"
                              + "internal class Utf8EncodingTests\n{\n}\n");

            findings = ScanForNameReferences(fixture.Path);
        }

        // Assert
        Assert.IsEmpty(findings);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Scans every non-generated C# file under a root directory for a self-referential tracker number in a
    /// comment or documentation comment
    /// </summary>
    /// <param name="rootDirectory">Directory to scan</param>
    /// <returns>Every self-referential tracker reference found</returns>
    internal static IReadOnlyList<TrackerReference> ScanForCommentReferences(string rootDirectory)
    {
        var findings = new List<TrackerReference>();

        foreach (var filePath in EnumerateScannedFiles(rootDirectory))
        {
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath)).GetRoot();
            var relativePath = Path.GetRelativePath(rootDirectory, filePath);

            foreach (var trivia in root.DescendantTrivia())
            {
                if (SyntaxTriviaUtilities.IsCommentTrivia(trivia) == false)
                {
                    continue;
                }

                findings.AddRange(FindTrackerReferencesInComment(trivia, relativePath));
            }
        }

        return findings;
    }

    /// <summary>
    /// Scans every non-generated C# file under a root directory for a self-referential tracker number in a
    /// declared type, member, or file name
    /// </summary>
    /// <param name="rootDirectory">Directory to scan</param>
    /// <returns>Every self-referential tracker reference found</returns>
    internal static IReadOnlyList<TrackerReference> ScanForNameReferences(string rootDirectory)
    {
        var findings = new List<TrackerReference>();

        foreach (var filePath in EnumerateScannedFiles(rootDirectory))
        {
            var relativePath = Path.GetRelativePath(rootDirectory, filePath);
            var fileNameMatch = _trackerNamePattern.Match(Path.GetFileNameWithoutExtension(filePath));

            if (fileNameMatch.Success)
            {
                findings.Add(new TrackerReference(relativePath, 1, fileNameMatch.Value));
            }

            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath)).GetRoot();

            foreach (var node in root.DescendantNodes())
            {
                var identifier = GetDeclaredIdentifier(node);

                if (identifier is not { } token)
                {
                    continue;
                }

                var nameMatch = _trackerNamePattern.Match(token.ValueText);

                if (nameMatch.Success)
                {
                    findings.Add(new TrackerReference(relativePath, token.GetLocation().GetLineSpan().StartLinePosition.Line + 1, nameMatch.Value));
                }
            }
        }

        return findings;
    }

    /// <summary>
    /// Finds every self-referential tracker reference inside one comment trivia
    /// </summary>
    /// <param name="trivia">Comment trivia to inspect</param>
    /// <param name="relativePath">File path reported with each finding</param>
    /// <returns>Every self-referential tracker reference found in the trivia</returns>
    private static IEnumerable<TrackerReference> FindTrackerReferencesInComment(SyntaxTrivia trivia, string relativePath)
    {
        var text = trivia.ToFullString();
        var startLine = trivia.GetLocation().GetLineSpan().StartLinePosition.Line;

        foreach (var match in _trackerUrlPattern.Matches(text).Cast<Match>())
        {
            if (string.Equals(match.Groups[1].Value, RepositoryOwner, StringComparison.OrdinalIgnoreCase) == false
                || string.Equals(match.Groups[2].Value, RepositoryName, StringComparison.OrdinalIgnoreCase) == false)
            {
                continue;
            }

            yield return new TrackerReference(relativePath, startLine + CountLineBreaksBefore(text, match.Index) + 1, match.Value);
        }

        foreach (var match in _bareTrackerNumberPattern.Matches(text).Cast<Match>())
        {
            yield return new TrackerReference(relativePath, startLine + CountLineBreaksBefore(text, match.Index) + 1, match.Value);
        }
    }

    /// <summary>
    /// Returns the declared-name identifier of a syntax node, when the node is a type, method, delegate,
    /// property, or enum-member declaration
    /// </summary>
    /// <param name="node">Node to inspect</param>
    /// <returns>The declared identifier, or <see langword="null"/> when the node declares no name</returns>
    private static SyntaxToken? GetDeclaredIdentifier(SyntaxNode node)
    {
        return node switch
               {
                   BaseTypeDeclarationSyntax typeDeclaration => typeDeclaration.Identifier,
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.Identifier,
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Identifier,
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier,
                   EnumMemberDeclarationSyntax enumMemberDeclaration => enumMemberDeclaration.Identifier,
                   _ => null
               };
    }

    /// <summary>
    /// Enumerates every C# file under a root directory that is neither build output nor generated source
    /// </summary>
    /// <param name="rootDirectory">Directory to scan</param>
    /// <returns>Every scanned file path</returns>
    private static IEnumerable<string> EnumerateScannedFiles(string rootDirectory)
    {
        foreach (var filePath in Directory.EnumerateFiles(rootDirectory, "*.cs", SearchOption.AllDirectories))
        {
            if (IsExcludedFromScan(filePath))
            {
                continue;
            }

            yield return filePath;
        }
    }

    /// <summary>
    /// Determines whether a file sits under a build-output directory or matches a generated-file suffix
    /// </summary>
    /// <param name="filePath">File path to check</param>
    /// <returns><see langword="true"/> if the file is excluded from the scan; otherwise, <see langword="false"/></returns>
    private static bool IsExcludedFromScan(string filePath)
    {
        var segments = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (segments.Any(segment => string.Equals(segment, "bin", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(segment, "obj", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return _generatedFileSuffixes.Any(suffix => filePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Counts the line breaks in a text before a character index, used to translate a match offset inside a
    /// multi-line trivia into a line-number delta
    /// </summary>
    /// <param name="text">Text to scan</param>
    /// <param name="index">Exclusive upper bound of the scan</param>
    /// <returns>The number of line breaks before the index</returns>
    private static int CountLineBreaksBefore(string text, int index)
    {
        var count = 0;

        for (var position = 0; position < index; position++)
        {
            if (text[position] == '\n')
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Formats every finding into a single assertion message
    /// </summary>
    /// <param name="kind">Either <c>"comment"</c> or <c>"name"</c>, describing what the finding references</param>
    /// <param name="findings">Findings to describe</param>
    /// <returns>One line per finding, or an empty string when there are none</returns>
    private static string DescribeFindings(string kind, IReadOnlyList<TrackerReference> findings)
    {
        if (findings.Count == 0)
        {
            return string.Empty;
        }

        var lines = findings.Select(finding => $"{finding.FilePath}({finding.Line}): {kind} references this repository's tracker number '{finding.MatchedText}'. "
                                               + "State the invariant it protects, or the behavior it names, instead; link another project's tracker by full URL.");

        return $"Found {findings.Count} self-referential tracker {kind} reference(s):\n{string.Join("\n", lines)}";
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