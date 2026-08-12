using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6003SemicolonsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6003SemicolonsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer, RH6003SemicolonsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0{|#0: |};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab immediately before a semicolon is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabBeforeSemicolonIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0{|#0:	|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));
    }

    /// <summary>
    /// Verifies that continuation-line indentation before a semicolon does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineIndentationDoesNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1
                                            ;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that continuation-line indentation before a semicolon does not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineIndentationDoesNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1
                                            ;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that disabled text and a directive boundary before a continuation-line semicolon do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineSemicolonDoNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                #if false
                                        int disabled = 0 ;
                                #endif
                                        ;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that disabled text and a directive boundary before a continuation-line semicolon do not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineSemicolonDoNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                #if false
                                        int disabled = 0 ;
                                #endif
                                        ;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the shared whitespace-span policy reports only same-line runs and rejects continuation indentation
    /// </summary>
    [TestMethod]
    public void VerifySharedWhitespaceSpanPolicyIsLineBounded()
    {
        const string sameLine = "value \t;";
        const string continuation = "#if false\ndisabled ;\n#endif\n    ;";
        var sameLineText = SourceText.From(sameLine);
        var continuationText = SourceText.From(continuation);
        var carriageReturnLineFeedText = SourceText.From(NormalizeToCarriageReturnLineFeed(continuation));

        Assert.AreEqual(TextSpan.FromBounds(5, 7), SameLinePrecedingWhitespaceAnalysis.GetSpan(sameLineText, sameLine.Length - 1));
        Assert.IsNull(SameLinePrecedingWhitespaceAnalysis.GetSpan(continuationText, continuation.LastIndexOf(';')));
        Assert.IsNull(SameLinePrecedingWhitespaceAnalysis.GetSpan(carriageReturnLineFeedText, carriageReturnLineFeedText.ToString().LastIndexOf(';')));
    }

    /// <summary>
    /// Verifies that a comment before a semicolon is preserved while only the adjacent whitespace run is removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeSemicolonIsPreservedByCodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1 /* Keep. */{|#0: |};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 1 /* Keep. */;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All removes multiple same-line whitespace runs in one iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleSameLineDiagnosticsAreFixedInOneFixAllIteration()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1{|#0: |};
                                        int second = 2{|#1:	|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = 1;
                                         int second = 2;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat, 2));
    }

    #endregion // Tests
}