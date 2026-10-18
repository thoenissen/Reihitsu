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