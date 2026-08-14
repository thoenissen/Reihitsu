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

    /// <summary>
    /// Verifies a three-statement same-line chain is rewritten with block indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task Issue469ThreeStatementChainFixAllKeepsBlockIndentation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1; {|#0:int second = 2;|} {|#1:int third = 3;|}
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
                                         int third = 3;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a block statement chain preserves the source line's exact tab indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TabIndentedBlockChainFixAllPreservesExactIndentation()
    {
        const string testData = "internal class TestClass\n{\n\tvoid Method()\n\t{\n\t\tint first = 1; {|#0:int second = 2;|} {|#1:int third = 3;|}\n\t}\n}";
        const string fixedData = "internal class TestClass\n{\n\tvoid Method()\n\t{\n\t\tint first = 1;\n\t\tint second = 2;\n\t\tint third = 3;\n\t}\n}";

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a switch-section statement chain is rewritten with the section statement indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ThreeStatementSwitchSectionFixAllKeepsSectionIndentation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int value)
                                    {
                                        switch (value)
                                        {
                                            case 0: Method(1); {|#0:Method(2);|} {|#1:break;|}
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int value)
                                     {
                                         switch (value)
                                         {
                                             case 0: Method(1);
                                                 Method(2);
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a fix is withheld when an empty statement lies between the analyzed non-empty siblings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixIsNotOfferedAcrossEmptyStatementSyntax()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Method(); ; Method();
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ExpressionStatementSyntax>()
                                                               .Last()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies a fix is withheld when a directive and skipped syntax occupy the selected statement's leading gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixIsNotOfferedAcrossDirectiveAndSkippedSyntax()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Method(); #if FEATURE
                                        Method();
                                        #endif
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ExpressionStatementSyntax>()
                                                               .Last()
                                                               .GetLocation(),
                                                   "FEATURE");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a stale diagnostic targeting the first statement does not offer a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixIsNotOfferedWhenDiagnosticTargetsFirstStatement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Method();
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ExpressionStatementSyntax>()
                                                               .Single()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}