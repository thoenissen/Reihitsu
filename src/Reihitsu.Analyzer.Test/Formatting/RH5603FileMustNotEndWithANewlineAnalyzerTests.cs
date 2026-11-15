using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5603FileMustNotEndWithANewlineAnalyzer"/> and <see cref="RH5603FileMustNotEndWithANewlineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5603FileMustNotEndWithANewlineAnalyzerTests : BatchCodeFixTestsBase<RH5603FileMustNotEndWithANewlineAnalyzer, RH5603FileMustNotEndWithANewlineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that files without a trailing newline do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-002")]
    public async Task VerifyNoDiagnosticsWhenFileDoesNotEndWithNewline()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a trailing newline is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-003")]
    [Requirement("RH5603-004")]
    public async Task VerifyTrailingNewlineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostic(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId).WithSpan(3, 2, 4, 1).WithMessage(AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that several line breaks at the end of the file are one diagnostic and are all deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-003")]
    [Requirement("RH5603-004")]
    public async Task VerifySeveralTrailingLineBreaksAreOneDiagnosticAndAreDeleted()
    {
        const string testData = "internal class TestClass\n{\n}{|#0:\n\n|}";
        const string fixedData = "internal class TestClass\n{\n}";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that trailing CRLF line breaks are detected and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-003")]
    [Requirement("RH5603-004")]
    public async Task VerifyTrailingCarriageReturnLineFeedsAreDetectedAndFixed()
    {
        const string testData = "internal class TestClass\r\n{\r\n}{|#0:\r\n\r\n|}";
        const string fixedData = "internal class TestClass\r\n{\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that a mix of CRLF and LF line breaks at the end of the file is detected and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-004")]
    public async Task VerifyMixedTrailingLineBreaksAreDetectedAndFixed()
    {
        const string testData = "internal class TestClass\r\n{\r\n}{|#0:\r\n\n|}";
        const string fixedData = "internal class TestClass\r\n{\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that a line break after a comment on the last line is detected and deleted like one after code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-004")]
    public async Task VerifyTrailingLineBreakAfterFinalCommentIsDetectedAndFixed()
    {
        const string testData = "internal class TestClass\n{\n}\n\n// end{|#0:\n|}";
        const string fixedData = "internal class TestClass\n{\n}\n\n// end";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that a file ending with a comment is not reported, and neither are the blank lines before its last line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-002")]
    public async Task VerifyNoDiagnosticsWhenFileEndsWithCommentAfterBlankLines()
    {
        const string testData = "internal class TestClass\n{\n}\n\n// end";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a line of whitespace only between the trailing line breaks belongs to them: the diagnostic covers
    /// every line break after the last code character, and one application of the fix deletes all of them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-003")]
    [Requirement("RH5603-004")]
    public async Task VerifyWhitespaceOnlyLineInTheTrailingLineBreaksIsPartOfTheDiagnosticAndIsDeleted()
    {
        const string testData = "class C\n{\n}{|#0:\n   \n|}";
        const string fixedData = "class C\n{\n}";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that a line of whitespace only between trailing CRLF line breaks belongs to them and is deleted with them
    /// in one application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-003")]
    [Requirement("RH5603-004")]
    public async Task VerifyWhitespaceOnlyLineInTheTrailingCarriageReturnLineFeedsIsPartOfTheDiagnosticAndIsDeleted()
    {
        const string testData = "class C\r\n{\r\n}{|#0:\r\n   \r\n|}";
        const string fixedData = "class C\r\n{\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix deletes a trailing whitespace-only line in one application and returns exactly the expected
    /// text, line endings included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-004")]
    public async Task VerifyOneApplicationOfTheFixDeletesTheWholeTail()
    {
        Assert.AreEqual("class C\n{\n}", await ApplyCodeFixAsync("class C\n{\n}\n   \n"));
        Assert.AreEqual("class C\r\n{\r\n}", await ApplyCodeFixAsync("class C\r\n{\r\n}\r\n   \r\n"));
    }

    /// <summary>
    /// Verifies that a line of whitespace only between trailing lone carriage returns belongs to them and is deleted with
    /// them in one application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWhitespaceOnlyLineInTheTrailingCarriageReturnsIsPartOfTheDiagnosticAndIsDeleted()
    {
        const string testData = "class C\r{\r}{|#0:\r   \r|}";
        const string fixedData = "class C\r{\r}";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that several whitespace-only lines of spaces and tabs between the trailing line breaks all belong to them
    /// and are deleted in one application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-003")]
    [Requirement("RH5603-004")]
    public async Task VerifySeveralWhitespaceOnlyLinesInTheTrailingLineBreaksArePartOfTheDiagnosticAndAreDeleted()
    {
        const string testData = "class C\n{\n}{|#0:\n  \n\t\n\n|}";
        const string fixedData = "class C\n{\n}";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace behind the last code character on its line is not part of the diagnostic and stays after
    /// the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-003")]
    [Requirement("RH5603-004")]
    public async Task VerifyWhitespaceBehindTheLastCharacterIsNotPartOfTheDiagnosticAndStays()
    {
        const string testData = "class C\n{\n}  {|#0:\n|}";
        const string fixedData = "class C\n{\n}  ";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that with whitespace behind the last code character and a whitespace-only line in the tail, the diagnostic
    /// starts at the first line break behind that character and the fix keeps the whitespace on the last code line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-003")]
    [Requirement("RH5603-004")]
    public async Task VerifyWhitespaceBehindTheLastCharacterStaysAndTheWhitespaceOnlyLineIsDeleted()
    {
        const string testData = "class C\n{\n}  {|#0:\n   \n|}";
        const string fixedData = "class C\n{\n}  ";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that a whitespace-only line in the tail behind a final comment belongs to the trailing line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5603-001")]
    [Requirement("RH5603-004")]
    public async Task VerifyWhitespaceOnlyLineBehindAFinalCommentIsPartOfTheDiagnosticAndIsDeleted()
    {
        const string testData = "class C\n{\n}\n\n// end{|#0:\n   \n|}";
        const string fixedData = "class C\n{\n}\n\n// end";

        await Verify(testData, fixedData, Diagnostics(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId, AnalyzerResources.RH5603MessageFormat));
    }

    /// <summary>
    /// Verifies that a file whose last line holds whitespace only and ends without a line break is not reported; that
    /// whitespace is RH5602's
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenTheLastLineHoldsWhitespaceOnlyWithoutALineBreak()
    {
        const string testData = "class C\n{\n}\n   ";

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                }

                                """;
        const string secondSource = """
                                    internal class OtherClass
                                    {
                                    }

                                    """;
        const string fixedCode = """
                                 internal class TestClass
                                 {
                                 }
                                 """;
        const string secondFixedSource = """
                                         internal class OtherClass
                                         {
                                         }
                                         """;

        // This rule reports at most one diagnostic per syntax tree, so the Fix All scenario needs two documents to
        // report two diagnostics at all. Each document's own trailing newline is removed independently, proving
        // per-document independence rather than within-document interference
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  [
                                      Diagnostic(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId).WithSpan("/0/Test0.cs", 3, 2, 4, 1).WithMessage(AnalyzerResources.RH5603MessageFormat),
                                      Diagnostic(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId).WithSpan("/0/Test1.cs", 3, 2, 4, 1).WithMessage(AnalyzerResources.RH5603MessageFormat)
                                  ],
                                  config =>
                                  {
                                      config.TestState.Sources.Add(("/0/Test1.cs", secondSource));
                                      config.FixedState.Sources.Add(("/0/Test1.cs", secondFixedSource));
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}