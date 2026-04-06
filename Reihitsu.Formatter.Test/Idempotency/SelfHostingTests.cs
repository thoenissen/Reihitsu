using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Idempotency;

/// <summary>
/// Self-hosting tests that run the formatter over every C# file in the Reihitsu solution.
/// These tests verify that the formatter produces no unwanted changes on correctly-formatted code
/// and that formatting is idempotent (formatting twice yields the same result).
/// </summary>
[TestClass]
public class SelfHostingTests
{
    #region Constants

    /// <summary>
    /// Directories to scan for C# files (relative to the solution root).
    /// </summary>
    private static readonly string[] _sourceDirectories = ["Reihitsu.Analyzer", "Reihitsu.Analyzer.CodeFixes", "Reihitsu.Formatter", "Reihitsu.Cli"];

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that formatting every source file in the solution is idempotent:
    /// formatting a file twice produces the same result as formatting it once.
    /// </summary>
    [TestMethod]
    public void FormatterIsIdempotentOnAllSourceFiles()
    {
#if DEBUG
        return;
#endif
        var solutionRoot = FindSolutionRoot();
        var failures = new List<string>();

        foreach (var file in EnumerateSourceFiles(solutionRoot))
        {
            TestContext.CancellationTokenSource.Token.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(solutionRoot, file);
            var content = File.ReadAllText(file, Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(content, cancellationToken: TestContext.CancellationTokenSource.Token);

            if (syntaxTree.GetDiagnostics(TestContext.CancellationTokenSource.Token).Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                continue;
            }

            var firstPass = ReihitsuFormatter.FormatSyntaxTree(syntaxTree, TestContext.CancellationTokenSource.Token);
            var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

            var firstResult = firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();
            var secondResult = secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

            if (firstResult != secondResult)
            {
                failures.Add($"NOT IDEMPOTENT: {relativePath}");
            }
        }

        if (failures.Count > 0)
        {
            Assert.Fail($"Formatter is not idempotent for {failures.Count} file(s):\n{string.Join("\n", failures)}");
        }
    }

    /// <summary>
    /// Verifies that the formatter produces no changes on source files that are assumed
    /// to be correctly formatted. Files where the formatter produces changes are reported
    /// with a diff summary.
    /// </summary>
    [TestMethod]
    public void FormatterProducesNoChangesOnSourceFiles()
    {
#if DEBUG
        return;
#endif

        var solutionRoot = FindSolutionRoot();
        var failures = new List<string>();

        foreach (var file in EnumerateSourceFiles(solutionRoot))
        {
            TestContext.CancellationTokenSource.Token.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(solutionRoot, file);
            var content = File.ReadAllText(file, Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(content, cancellationToken: TestContext.CancellationTokenSource.Token);

            if (syntaxTree.GetDiagnostics(TestContext.CancellationTokenSource.Token).Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                continue;
            }

            var formatted = ReihitsuFormatter.FormatSyntaxTree(syntaxTree, TestContext.CancellationTokenSource.Token);
            var result = formatted.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

            if (content != result)
            {
                var diffSummary = BuildDiffSummary(content, result, relativePath);

                failures.Add(diffSummary);
            }
        }

        if (failures.Count > 0)
        {
            Assert.Fail($"Formatter changed {failures.Count} file(s):\n\n{string.Join("\n\n", failures)}");
        }
    }

    /// <summary>
    /// Finds the solution root directory by walking up from the test assembly's location.
    /// </summary>
    /// <returns>The absolute path to the solution root directory.</returns>
    private static string FindSolutionRoot()
    {
        var directory = AppContext.BaseDirectory;

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "Reihitsu.sln")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new InvalidOperationException("Could not find solution root (Reihitsu.sln).");
    }

    /// <summary>
    /// Enumerates all C# source files in the configured source directories,
    /// excluding auto-generated files and bin/obj directories.
    /// </summary>
    /// <param name="solutionRoot">The absolute path to the solution root.</param>
    /// <returns>An enumerable of absolute file paths.</returns>
    private static IEnumerable<string> EnumerateSourceFiles(string solutionRoot)
    {
        foreach (var dir in _sourceDirectories)
        {
            var fullPath = Path.Combine(solutionRoot, dir);

            if (Directory.Exists(fullPath) == false)
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(fullPath, "*.cs", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(solutionRoot, file);

                if (relativePath.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                    || relativePath.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                {
                    continue;
                }

                if (relativePath.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (relativePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return file;
            }
        }
    }

    /// <summary>
    /// Builds a compact diff summary showing the first few changed lines.
    /// </summary>
    /// <param name="original">The original file content.</param>
    /// <param name="formatted">The formatted file content.</param>
    /// <param name="relativePath">The relative file path for display.</param>
    /// <returns>A string containing the diff summary.</returns>
    private static string BuildDiffSummary(string original, string formatted, string relativePath)
    {
        var originalLines = original.Split('\n');
        var formattedLines = formatted.Split('\n');
        var sb = new StringBuilder();
        var changedLineCount = 0;

        sb.AppendLine($"--- {relativePath}");

        var maxLines = Math.Max(originalLines.Length, formattedLines.Length);

        for (var i = 0; i < maxLines; i++)
        {
            var origLine = i < originalLines.Length ? originalLines[i] : "<EOF>";
            var fmtLine = i < formattedLines.Length ? formattedLines[i] : "<EOF>";

            if (origLine != fmtLine)
            {
                changedLineCount++;

                if (changedLineCount <= 10)
                {
                    sb.AppendLine($"  Line {i + 1}:");
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

    #endregion // Methods
}