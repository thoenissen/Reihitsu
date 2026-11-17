using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.EndToEnd;

/// <summary>
/// End-to-end tests for files whose text another build compiles as code (CLI-021): the CLI leaves them unchanged in every mode,
/// names them as skipped, and they do not affect the exit code; and for a file with a comment between a conditional operator
/// and its branch, which the formatter refuses as unsupported (FMT-UNSUP-008); and for files with directives inside and in front of
/// combined field declarations (FMT-UNSUP-010, FMT-STRUCT-022)
/// </summary>
[TestClass]
[DoNotParallelize]
[TestCategory("E2E")]
public class CliHiddenCodeEndToEndTests
{
    #region Constants

    /// <summary>
    /// The file of the CLI-021 example: the comment starts inside the <c>#if !A</c> group and holds the line <c>#else</c>, and the
    /// formatter would otherwise correct <c>int  x</c>
    /// </summary>
    private const string HiddenCodeSource = "class C\n{\n    void M()\n    {\n#if !A\n        /*\n#else\n        var s = @\"\n        */\n        int  x = 1; //\";\n#endif\n    }\n}\n";

    /// <summary>
    /// A file that holds a hiding carrier and that the formatter, ignoring the carrier, would leave as it is apart from its
    /// encoding
    /// </summary>
    private const string FormattedHiddenCodeSource = "class C\n{\n    void M()\n    {\n#if !A\n        /*\n#else\n        var s = @\"\n        */\n        int x = 1; //\";\n#endif\n    }\n}";

    /// <summary>
    /// A file that holds a hiding carrier and a comment inside a variable declaration, which the formatter refuses as unsupported
    /// </summary>
    private const string HiddenCodeWithUnsupportedShapeSource = "class C\n{\n    void M()\n    {\n#if !A\n        /*\n#else\n        var s = @\"\n        */\n        int  x = 1; //\";\n#endif\n        int /* c */ y = 1;\n    }\n}\n";

    /// <summary>
    /// The file of the FMT-UNSUP-008 example with inline comments behind both operators, whose first comment starts in line 5,
    /// column 21
    /// </summary>
    private const string ConditionalCommentSource = "class C\n{\n    int M(bool a)\n    {\n        var x = a ? /* yes */ 1 : /* no */ 2;\n        var y = a\n                    ? /* yes */ 1\n                    : /* no */ 2;\n\n        return x + y;\n    }\n}\n";

    /// <summary>
    /// The file of the FMT-UNSUP-009 change's visible output example: comments and directives between list elements and the
    /// commas behind them, whose first comment starts in line 3, column 21
    /// </summary>
    private const string ListSeparatorCommentSource = "class C\n{\n    void M<T>(int a // first\n              , int b)\n        where T : class // k\n            , new()\n    {\n        Foo(1 // first\n            , 2);\n        var e = new D { X = 3 // k\n            , Y = 4 };\n        Foo(a, b\n#if DEBUG\n            , c\n#else\n            , d\n#endif\n            );\n        int[] g = new[] { 1,\n            2 /* two */\n            , };\n    }\n}\n";

    /// <summary>
    /// A formatted file
    /// </summary>
    private const string FormattedSource = "class B\n{\n    void M()\n    {\n    }\n}";

    /// <summary>
    /// A file the formatter changes
    /// </summary>
    private const string UnformattedSource = "class A{ void M(){ } }\n";

    /// <summary>
    /// The summary line for one skipped file
    /// </summary>
    private const string SkippedSummaryLine = "Skipped 1 file(s) with code hidden in another build configuration.";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the specification example: <c>--check --verbose</c> names the file as skipped, counts it in the total but not as
    /// needing formatting, prints the skip summary line, and exits with code 0
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-021")]
    public async Task SpecExampleCheckVerboseNamesTheFileAsSkippedAndSucceeds()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", HiddenCodeSource);

            var (exitCode, output, error) = await Run(["--check", "--verbose", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join($"Skipped (code hidden in another build configuration): {Path.GetFullPath(filePath)}", string.Empty, "0 of 1 file(s) need formatting.", SkippedSummaryLine), output);
        }
    }

    /// <summary>
    /// Verifies that without <c>--verbose</c> the per-file line is not written, and the summary still names the skipped file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-021")]
    public async Task CheckWithoutVerbosePrintsOnlyTheSummary()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", HiddenCodeSource);

            var (exitCode, output, _) = await Run(["--check", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(Join(string.Empty, "0 of 1 file(s) need formatting.", SkippedSummaryLine), output);
        }
    }

    /// <summary>
    /// Verifies the specification example: write mode leaves the file unchanged and ends with the formatted count and the skip
    /// summary line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-021")]
    public async Task SpecExampleWriteModeLeavesTheFileUnchanged()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", HiddenCodeSource);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            var (exitCode, output, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join(string.Empty, "Formatted 0 of 1 file(s).", SkippedSummaryLine), output);
            Assert.AreSequenceEqual(originalBytes, await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that <c>--utf8-bom</c> does not rewrite the encoding of the skipped file, whose content the formatter would not
    /// change either
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-021")]
    public async Task Utf8BomDoesNotRewriteTheSkippedFile()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", FormattedHiddenCodeSource, new UTF8Encoding(false));
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            var (exitCode, _, _) = await Run(["--utf8-bom", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreSequenceEqual(originalBytes, await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that <c>--dry-run</c> prints no <c>Would format:</c> line and no diff for the skipped file, and does not count it
    /// as a file that would be formatted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-021")]
    public async Task DryRunDoesNotCountTheSkippedFileAsWouldBeFormatted()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", HiddenCodeSource);
            var formattedPath = tempDir.CreateFile("B.cs", FormattedSource);

            var (exitCode, output, error) = await Run(["--dry-run", filePath, formattedPath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join(string.Empty, "0 of 2 file(s) would be formatted.", SkippedSummaryLine), output);
            Assert.AreEqual(HiddenCodeSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that with an unformatted file in the run the exit code follows that file, and the skipped file does not count as
    /// needing formatting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-021")]
    public async Task CheckExitCodeFollowsTheOtherFiles()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", HiddenCodeSource);
            var unformattedPath = tempDir.CreateFile("A.cs", UnformattedSource);

            var (exitCode, output, _) = await Run(["--check", filePath, unformattedPath]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.AreEqual(Join($"Not formatted: {Path.GetFullPath(unformattedPath)}", string.Empty, "1 of 2 file(s) need formatting.", SkippedSummaryLine), output);
        }
    }

    /// <summary>
    /// Verifies that a file that holds a hiding carrier and a shape the formatter refuses as unsupported is skipped as a file
    /// whose code another build hides: no error line, the skip summary line, and exit code 0
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-021")]
    public async Task CarrierBesideAnUnsupportedShapeIsSkippedWithoutAnError()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", HiddenCodeWithUnsupportedShapeSource);

            var (exitCode, output, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join(string.Empty, "Formatted 0 of 1 file(s).", SkippedSummaryLine), output);
            Assert.AreEqual(HiddenCodeWithUnsupportedShapeSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that a file with a syntax error that also holds a hiding carrier is skipped as a file with syntax errors
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-021")]
    public async Task CarrierBesideASyntaxErrorIsSkippedForTheSyntaxError()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var source = HiddenCodeSource.Replace("    }\n}\n", "    }\n\n    void N(\n}\n", System.StringComparison.Ordinal);
            var filePath = tempDir.CreateFile("C.cs", source);

            var (exitCode, output, _) = await Run(["--check", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(Join(string.Empty, "0 of 1 file(s) need formatting.", "Skipped 1 file(s) with syntax errors."), output);
        }
    }

    /// <summary>
    /// Verifies the specification example of FMT-UNSUP-008: a file with inline comments behind a conditional's operators is left
    /// unchanged, reported with the reason and the first comment's line and column, and the run exits with code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public async Task CommentBehindAConditionalOperatorIsReportedAsUnsupported()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("u.cs", ConditionalCommentSource);

            var (exitCode, _, error) = await Run(["--check", filePath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { $"error: {Path.GetFullPath(filePath)}(5,21): unsupported syntax: a comment or directive between a conditional operator and its branch. The file was left unchanged." }, Lines(error));
            Assert.AreEqual(ConditionalCommentSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies the visible output example of FMT-UNSUP-009: a file with comments and a directive between list elements and the
    /// commas behind them is left unchanged, reported with the reason and the first comment's line and column, and the run exits
    /// with code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    public async Task CommentInFrontOfACommaOnALaterLineIsReportedAsUnsupported()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("l.cs", ListSeparatorCommentSource);

            var (exitCode, _, error) = await Run(["--check", filePath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { $"error: {Path.GetFullPath(filePath)}(3,21): unsupported syntax: a comment or directive between a list element and the comma behind it. The file was left unchanged." }, Lines(error));
            Assert.AreEqual(ListSeparatorCommentSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that the CLI formats a file in place whose only comma-first list keeps its comments behind the commas, which
    /// FMT-UNSUP-009 does not refuse, and reports nothing on standard error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    public async Task CommentBehindACommaOnALaterLineIsFormatted()
    {
        const string source = "class C\n{\n    void M()\n    {\n        Foo(1\n            , /* c */ 2);\n    }\n}\n";
        const string expected = "class C\n{\n    void M()\n    {\n        Foo(1, /* c */\n            2);\n    }\n}";

        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("f.cs", source);

            var (exitCode, _, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(expected, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies the visible output example of FMT-UNSUP-010: a file with a conditional group between the declarators of a field
    /// declaration is left unchanged, reported with the reason and the line and column of the <c>#if</c> directive, and the run
    /// exits with code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    public async Task DirectiveBetweenTheDeclaratorsOfAFieldIsReportedAsUnsupported()
    {
        const string source = "class C\n{\n    int a,\n#if X\n        b,\n#endif\n        c;\n\n    void M(){ }\n}\n";

        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("f.cs", source);

            var (exitCode, _, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { $"error: {Path.GetFullPath(filePath)}(4,1): unsupported syntax: a directive inside a field declaration with several declarators. The file was left unchanged." }, Lines(error));
            Assert.AreEqual(source, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies the visible output example of FMT-UNSUP-010: a file with a conditional group inside the lambda value of a field
    /// declaration with several declarators is left unchanged and reported with the line and column of the <c>#if</c> directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    public async Task DirectiveInsideAValueOfACombinedFieldIsReportedAsUnsupported()
    {
        const string source = "using System;\n\nclass C\n{\n    private Func<int> _a = () =>\n    {\n#if DEBUG\n        return 1;\n#else\n        return 2;\n#endif\n    }, _b;\n}\n";

        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("v.cs", source);

            var (exitCode, _, error) = await Run(["--check", filePath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { $"error: {Path.GetFullPath(filePath)}(7,1): unsupported syntax: a directive inside a field declaration with several declarators. The file was left unchanged." }, Lines(error));
            Assert.AreEqual(source, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies the visible output example of FMT-STRUCT-022: the CLI splits a combined field behind a region and one inside a
    /// conditional group that is active without preprocessor symbols, keeps the directives in front of the first fields, and
    /// reports nothing on standard error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public async Task CombinedFieldsBehindDirectivesAreSplit()
    {
        const string source = "class C\n{\n    #region Fields\n\n    private int _x, _y;\n\n    #endregion // Fields\n\n#if !RELEASE\n    private int _p, _q = 1;\n#endif\n}\n";
        const string expected = "class C\n{\n    #region Fields\n\n    private int _x;\n    private int _y;\n\n    #endregion // Fields\n\n#if !RELEASE\n    private int _p;\n    private int _q = 1;\n#endif\n}";

        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("s.cs", source);

            var (exitCode, _, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(expected, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies the visible output example of FMT-STRUCT-022: the CLI, which defines no preprocessor symbols, keeps a combined field
    /// behind an inactive attribute list whole, does not refuse the file, and reports nothing on standard error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public async Task CombinedFieldBehindAnInactiveAttributeListStaysWhole()
    {
        const string source = "using System.Diagnostics;\n\nclass C\n{\n#if DEBUG\n    [DebuggerBrowsable(DebuggerBrowsableState.Never)]\n#endif\n    private int _a, _b;\n}\n";
        const string expected = "using System.Diagnostics;\n\nclass C\n{\n#if DEBUG\n    [DebuggerBrowsable(DebuggerBrowsableState.Never)]\n#endif\n    private int _a, _b;\n}";

        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("d.cs", source);

            var (exitCode, _, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(expected, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Runs the CLI with the given arguments and captures its exit code and both output streams
    /// </summary>
    /// <param name="arguments">The command-line arguments</param>
    /// <returns>The exit code, the standard output, and the standard error</returns>
    private static async Task<(int ExitCode, string Output, string Error)> Run(string[] arguments)
    {
        using (var capture = new ConsoleCapture())
        {
            var exitCode = await Program.Main(arguments);

            return (exitCode, capture.StandardOutput, capture.StandardError);
        }
    }

    /// <summary>
    /// Splits captured output into its lines, without the empty line behind the last line break
    /// </summary>
    /// <param name="text">The captured output</param>
    /// <returns>The lines</returns>
    private static string[] Lines(string text)
    {
        return text.Split(Environment.NewLine).SkipLast(1).ToArray();
    }

    /// <summary>
    /// Joins lines with the console's line break and ends the last one with it
    /// </summary>
    /// <param name="lines">The lines</param>
    /// <returns>The text the console writes for the lines</returns>
    private static string Join(params string[] lines)
    {
        return string.Concat(lines.Select(line => line + Environment.NewLine));
    }

    #endregion // Methods
}