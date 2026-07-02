using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer"/> and <see cref="RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzerTests : AnalyzerTestsBase<RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer, RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that separate-line statements do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenStatementsAreOnSeparateLines()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1;
                                        int second = 2;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiple statements on one line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleStatementsOnOneLineAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1; {|#0:int second = 2;|}
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

        await Verify(testData, fixedData, Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is not offered when a comment sits between the joined statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenCommentIsBetweenStatements()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int a = 1; /* note */ int b = 2;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<LocalDeclarationStatementSyntax>()
                                                               .Last()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the inserted line break matches the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1; int second = 2;
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.Contains("int first = 1;\r\n", fixedSource);
        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    #endregion // Tests
}