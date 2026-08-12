using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer, RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider>
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
                                [System.Obsolete]
                                internal class TestClass
                                {
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
                                [System.Obsolete{|#0: |}]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab before a closing attribute bracket is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabBeforeClosingAttributeBracketIsDetectedAndFixed()
    {
        const string testData = """
                                [System.Obsolete{|#0:	|}]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix preserves a comment before a closing attribute bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeClosingAttributeBracketIsPreservedByCodeFix()
    {
        const string testData = """
                                [System.Obsolete /* Keep. */{|#0: |}]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete /* Keep. */]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));
    }

    /// <summary>
    /// Verifies that a closing attribute bracket on a continuation line does not produce a diagnostic with LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingAttributeBracketDoesNotProduceDiagnostic()
    {
        const string testData = """
                                [
                                    System.Obsolete
                                    ]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a closing attribute bracket on a continuation line does not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingAttributeBracketDoesNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                [
                                    System.Obsolete
                                    ]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that disabled text and a directive boundary before a continuation-line closing attribute bracket do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineClosingAttributeBracketDoNotProduceDiagnostic()
    {
        const string testData = """
                                #if false
                                [System.Obsolete ]
                                internal class Disabled;
                                #endif
                                [
                                    System.Obsolete
                                    ]
                                internal class TestClass;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that disabled text and a directive boundary before a continuation-line closing attribute bracket do not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineClosingAttributeBracketDoNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                #if false
                                [System.Obsolete ]
                                internal class Disabled;
                                #endif
                                [
                                    System.Obsolete
                                    ]
                                internal class TestClass;
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that Fix All removes multiple same-line whitespace runs in one iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleSameLineDiagnosticsAreFixedInOneFixAllIteration()
    {
        const string testData = """
                                [System.Obsolete{|#0: |}]
                                [System.CLSCompliant(true){|#1:	|}]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 [System.CLSCompliant(true)]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat, 2));
    }

    #endregion // Tests
}