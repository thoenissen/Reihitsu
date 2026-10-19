using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer"/> and <see cref="RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzerTests : BatchCodeFixTestsBase<RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer, RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that files starting with content do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5028-002")]
    public async Task VerifyNoDiagnosticsWhenFileStartsWithContent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that leading blank lines are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5028-001")]
    [Requirement("RH5028-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyLeadingBlankLinesAreDetectedAndFixed()
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
                     Diagnostic(RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.DiagnosticId).WithSpan(1, 1, 3, 1).WithMessage(AnalyzerResources.RH5028MessageFormat));
    }

    /// <summary>
    /// Verifies the blank lines in front of the first content line are reported with one diagnostic and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5028-001")]
    [Requirement("RH5028-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyBlankLinesInFrontOfFirstContentLineAreReportedAsOneSpan()
    {
        const string testCode = """
                                {|RH5028:


                                |}class C
                                {
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a file that holds nothing but whitespace and line breaks is reported over the whole file and emptied
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5028-001")]
    [Requirement("RH5028-003")]
    public async Task VerifyWhitespaceOnlyFileIsReportedOverWholeFileAndEmptied()
    {
        const string testCode = "{|RH5028:  \n\t\n    |}";

        await Verify(testCode, string.Empty);
    }

    /// <summary>
    /// Verifies an empty file is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5028-002")]
    public async Task VerifyEmptyFileIsNotReported()
    {
        await Verify(string.Empty);
    }

    /// <summary>
    /// Verifies a file whose first line holds a comment is not reported, even when a blank line follows it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5028-002")]
    public async Task VerifyFileStartingWithCommentFollowedByBlankLineIsNotReported()
    {
        const string testCode = """
                                // header

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies blank lines in front of a first line that holds a directive are reported and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5028-001")]
    [Requirement("RH5028-003")]
    public async Task VerifyBlankLinesInFrontOfDirectiveAreReportedAndDeleted()
    {
        const string testCode = """
                                {|RH5028:

                                |}#nullable enable
                                class C
                                {
                                }
                                """;
        const string fixedCode = """
                                 #nullable enable
                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies leading whitespace-only lines with CRLF line breaks are deleted up to the first comment line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5028-001")]
    [Requirement("RH5028-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesLeadingBlankLinesWithCarriageReturnLineFeed()
    {
        const string testCode = "  \n\n// header\nclass C\n{\n}\n";
        const string fixedCode = "// header\nclass C\n{\n}\n";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode), actual);
    }

    /// <summary>
    /// Verifies one application of the fix removes the whole run of blank lines its one diagnostic reports, without
    /// relying on the fix of another rule that reports part of the run
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-001")]
    public async Task OneApplicationRemovesTheWholeReportedRun()
    {
        const string testCode = "{|RH5028:\n\n\n|}class C\n{\n}\n";
        const string fixedCode = "class C\n{\n}\n";

        await Verify(testCode, fixedCode, test => test.NumberOfIncrementalIterations = 1);
    }

    /// <summary>
    /// Verifies one application of the fix removes the whole reported run of blank lines under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-001")]
    public async Task OneApplicationRemovesTheWholeReportedRunUnderCarriageReturnLineFeed()
    {
        const string testCode = "{|RH5028:\r\n\r\n\r\n|}class C\r\n{\r\n}\r\n";
        const string fixedCode = "class C\r\n{\r\n}\r\n";

        await Verify(testCode, fixedCode, test => test.NumberOfIncrementalIterations = 1);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
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

        const string secondSource = """

                                    namespace TestNamespace
                                    {
                                    }
                                    """;
        const string secondFixedSource = """
                                         namespace TestNamespace
                                         {
                                         }
                                         """;

        // RH5028's analyzer (Reihitsu.Analyzer/Rules/Layout/RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.cs)
        // reports at most one diagnostic per syntax tree, so the Fix All scenario needs two documents to report
        // two diagnostics at all
        return new FixAllScenario(testData,
                                  fixedData,
                                  [
                                      Diagnostic(RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.DiagnosticId).WithSpan("/0/Test0.cs", 1, 1, 3, 1).WithMessage(AnalyzerResources.RH5028MessageFormat),
                                      Diagnostic(RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.DiagnosticId).WithSpan("/0/Test1.cs", 1, 1, 2, 1).WithMessage(AnalyzerResources.RH5028MessageFormat)
                                  ],
                                  config =>
                                  {
                                      config.TestState.Sources.Add(("/0/Test1.cs", secondSource));
                                      config.FixedState.Sources.Add(("/0/Test1.cs", secondFixedSource));

                                      // Fix All in document scope corrects one document per iteration, and this rule reports at most one
                                      // diagnostic per file, so two documents need at most two iterations
                                      config.NumberOfFixAllIterations = -2;
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}