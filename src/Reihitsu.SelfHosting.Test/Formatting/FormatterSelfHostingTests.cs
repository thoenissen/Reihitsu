using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter;
using Reihitsu.SelfHosting.Test.Utilities;

namespace Reihitsu.SelfHosting.Test.Formatting;

/// <summary>
/// Self-hosting scans that run the formatter over the solution's own C# source files, one test row per file. They
/// verify that the formatter changes nothing in correctly formatted code and that formatting is idempotent, each under
/// both LF and CRLF line endings
/// </summary>
[TestClass]
public sealed class FormatterSelfHostingTests
{
    #region Fields

    /// <summary>
    /// The line endings every file is checked under
    /// </summary>
    private static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// One row per scanned source file, named by its solution-relative path
    /// </summary>
    public static IEnumerable<string> SourceFiles => FormatterSourceCatalog.EnumerateRelativeSourceFiles(RepositoryRoot.Find());

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that formatting the file twice produces the same result as formatting it once
    /// </summary>
    /// <param name="relativePath">Solution-relative path of the source file</param>
    [TestMethod]
    [DynamicData(nameof(SourceFiles))]
    [Requirement("FMT-PIPE-004")]
    public void FormatterIsIdempotentOnSourceFile(string relativePath)
    {
        var failures = new List<string>();

        foreach (var (endOfLine, syntaxTree) in ParseUnderEachLineEnding(relativePath))
        {
            var firstPass = ReihitsuFormatter.FormatSyntaxTree(syntaxTree, TestContext.CancellationToken);
            var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationToken);
            var firstResult = firstPass.GetRoot(TestContext.CancellationToken).ToFullString();
            var secondResult = secondPass.GetRoot(TestContext.CancellationToken).ToFullString();

            if (firstResult != secondResult)
            {
                failures.Add(BuildDiffSummary(firstResult, secondResult, $"{relativePath} [{DescribeLineEnding(endOfLine)}], second pass against first pass"));
            }
        }

        if (failures.Count > 0)
        {
            Assert.Fail($"Formatter is not idempotent on {relativePath}:\n\n{string.Join("\n\n", failures)}");
        }
    }

    /// <summary>
    /// Verifies that the formatter produces no change on the file, which is assumed to be correctly formatted; a
    /// failure prints a diff summary per line ending
    /// </summary>
    /// <param name="relativePath">Solution-relative path of the source file</param>
    [TestMethod]
    [DynamicData(nameof(SourceFiles))]
    [Requirement("FMT-PIPE-004")]
    public void FormatterProducesNoChangesOnSourceFile(string relativePath)
    {
        var failures = new List<string>();

        foreach (var (endOfLine, syntaxTree) in ParseUnderEachLineEnding(relativePath))
        {
            var normalized = syntaxTree.GetRoot(TestContext.CancellationToken).ToFullString();
            var formatted = ReihitsuFormatter.FormatSyntaxTree(syntaxTree, TestContext.CancellationToken);
            var result = formatted.GetRoot(TestContext.CancellationToken).ToFullString();

            if (normalized != result)
            {
                failures.Add(BuildDiffSummary(normalized, result, $"{relativePath} [{DescribeLineEnding(endOfLine)}]"));
            }
        }

        if (failures.Count > 0)
        {
            Assert.Fail($"Formatter changed {relativePath}:\n\n{string.Join("\n\n", failures)}");
        }
    }

    /// <summary>
    /// Verifies that the rows cover every scanned directory, name each file once with a forward-slash path, and skip
    /// build output and generated files
    /// </summary>
    [TestMethod]
    public void SourceFileRowsCoverEveryScannedDirectoryAndSkipBuildOutput()
    {
        // Act
        var files = SourceFiles.ToList();

        // Assert
        Assert.HasCount(files.Count, files.Distinct(StringComparer.Ordinal).ToList(), "Two source-file rows share a path.");

        foreach (var directory in FormatterSourceCatalog.SourceDirectories)
        {
            Assert.Contains(file => file.StartsWith(directory + "/", StringComparison.Ordinal), files, $"No source-file row lies under '{directory}'.");
        }

        Assert.Contains("src/Reihitsu.SelfHosting.Test/Formatting/FormatterSelfHostingTests.cs", files);
        Assert.DoesNotContain(file => file.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
                                      || file.Contains("/obj/", StringComparison.OrdinalIgnoreCase),
                              files,
                              "A source-file row lies in build output.");
        Assert.DoesNotContain(RepositoryRoot.IsGeneratedSourceFile, files, "A source-file row is a generated file.");
        Assert.DoesNotContain(file => file.Contains('\\', StringComparison.Ordinal), files, "A source-file row uses a backslash.");
    }

    /// <summary>
    /// Rewrites every line break in the given text to the requested end-of-line sequence
    /// </summary>
    /// <param name="text">The text to normalize</param>
    /// <param name="endOfLine">The target end-of-line sequence</param>
    /// <returns>The text using the requested line endings</returns>
    private static string NormalizeLineEndings(string text, string endOfLine)
    {
        var lineFeedOnly = text.Replace("\r\n", "\n");

        return endOfLine == "\n" ? lineFeedOnly : lineFeedOnly.Replace("\n", endOfLine);
    }

    /// <summary>
    /// Returns a short, human-readable name for the given end-of-line sequence
    /// </summary>
    /// <param name="endOfLine">The end-of-line sequence</param>
    /// <returns><c>LF</c> or <c>CRLF</c></returns>
    private static string DescribeLineEnding(string endOfLine)
    {
        return endOfLine == "\n" ? "LF" : "CRLF";
    }

    /// <summary>
    /// Builds a compact diff summary showing the first few changed lines
    /// </summary>
    /// <param name="original">The original file content</param>
    /// <param name="formatted">The formatted file content</param>
    /// <param name="label">The label naming the file and line ending</param>
    /// <returns>A string containing the diff summary</returns>
    private static string BuildDiffSummary(string original, string formatted, string label)
    {
        var originalLines = original.Split('\n');
        var formattedLines = formatted.Split('\n');
        var sb = new StringBuilder();
        var changedLineCount = 0;

        sb.AppendLine($"--- {label}");

        var maxLines = Math.Max(originalLines.Length, formattedLines.Length);

        for (var lineIndex = 0; lineIndex < maxLines; lineIndex++)
        {
            var origLine = lineIndex < originalLines.Length ? originalLines[lineIndex] : "<EOF>";
            var fmtLine = lineIndex < formattedLines.Length ? formattedLines[lineIndex] : "<EOF>";

            if (origLine != fmtLine)
            {
                changedLineCount++;

                if (changedLineCount <= 10)
                {
                    sb.AppendLine($"  Line {lineIndex + 1}:");
                    sb.AppendLine($"    - {origLine.TrimEnd().Replace("\r", "\\r")}");
                    sb.AppendLine($"    + {fmtLine.TrimEnd().Replace("\r", "\\r")}");
                }
            }
        }

        if (changedLineCount > 10)
        {
            sb.AppendLine($"  ... and {changedLineCount - 10} more changed line(s)");
        }

        sb.AppendLine($"  Total: {changedLineCount} changed line(s)");

        return sb.ToString();
    }

    /// <summary>
    /// Reads a source file and parses it once per line ending, skipping a variant that has syntax errors
    /// </summary>
    /// <param name="relativePath">Solution-relative path of the source file</param>
    /// <returns>Each line ending with the parsed tree of the file under it</returns>
    private IEnumerable<(string EndOfLine, SyntaxTree SyntaxTree)> ParseUnderEachLineEnding(string relativePath)
    {
        var content = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), relativePath.Replace('/', Path.DirectorySeparatorChar)), Encoding.UTF8);

        foreach (var endOfLine in _lineEndings)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(content, endOfLine), cancellationToken: TestContext.CancellationToken);

            if (syntaxTree.GetDiagnostics(TestContext.CancellationToken).Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            {
                continue;
            }

            yield return (endOfLine, syntaxTree);
        }
    }

    #endregion // Methods
}