using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string testData = """
                                [System.Obsolete{|#0: |}]
                                internal class TestClass;
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 internal class TestClass;
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));
    }

    /// <summary>
    /// Verifies that the space in front of the closing bracket is still removed when a documentation comment sits
    /// between it and the attribute. Roslyn files that comment as the bracket's leading trivia, which puts the space
    /// on the far side of it from the spacing rules, so the comment-gap exemption has to trim it explicitly or the
    /// formatter emits output this analyzer reports (issue #591)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentBeforeClosingBracketStaysAnalyzerClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    [System.Obsolete /** why */{|#0: |}]
                                    public void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     [System.Obsolete /** why */]
                                     public void Method()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6014MessageFormat));
    }

    /// <summary>
    /// Verifies that a continuation-line closing attribute bracket remains analyzer-clean with LF and CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingAttributeBracketStaysAnalyzerClean()
    {
        const string testData = """
                                [
                                System.Obsolete
                                ]
                                internal class TestClass;
                                """;

        await VerifyFormatter(testData);
    }

    #endregion // Tests
}