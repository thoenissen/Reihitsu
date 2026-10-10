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
public class RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzerTests : BatchCodeFixTestsBase<RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer, RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider>
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
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings.
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
    public async Task ThreeStatementChainFixAllKeepsBlockIndentation()
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
    /// Verifies that a switch-section statement chain sharing its case label's line lands on the label's own
    /// column plus one indentation level, even when that label's column is anchored to an object initializer
    /// rather than derived from brace-scope nesting depth. Under Roslyn's batch fix-all provider both actions are
    /// computed against the unmodified document, so neither previous statement is first on its own line and the
    /// anchor-derived fallback this test exercises cannot be bypassed by iterative reformatting.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SwitchSectionChainInsideObjectInitializerFixAllKeepsLabelRelativeIndentation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                                          case 0: Method(1); {|#0:Method(2);|} {|#1:break;|}
                                            }
                                        }
                                    };

                                    private static void Method(int value)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action<int> Handler { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                                           case 0: Method(1);
                                                                               Method(2);
                                                                               break;
                                             }
                                         }
                                     };

                                     private static void Method(int value)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action<int> Handler { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a switch-section statement chain sharing a mis-indented case label's line still lands at the
    /// canonical, level-derived column rather than propagating the label's own miskeyed indentation, because no
    /// object initializer or anonymous object sits between the section and its nearest brace scope here. Only when
    /// such an anchor scope is present does the label's own column become the correct source of truth.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SwitchSectionChainWithoutAnchorScopeUsesCanonicalIndentationRegardlessOfLabelsOwnColumn()
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
    /// Verifies that no extra indentation level is added when the anchor line's own leading content is a comment
    /// rather than the section's own label: the moved statement keeps its sibling's column instead of gaining a
    /// spurious level.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySplitStatementKeepsSiblingColumnWhenPrecedingLineStartsWithComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 0:
                                                    /* note */ Method(1); {|#0:Method(2);|}
                                                    break;
                                            }
                                        }
                                    };

                                    private static void Method(int value)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action<int> Handler { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 0:
                                                     /* note */ Method(1);
                                                     Method(2);
                                                     break;
                                             }
                                         }
                                     };

                                     private static void Method(int value)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action<int> Handler { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat));
    }

    /// <summary>
    /// Verifies the same non-compensation under Fix All, where every split statement's indentation is computed
    /// against the unmodified document.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixAllKeepsSiblingColumnWhenPrecedingLineStartsWithComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 0:
                                                    /* note */ Method(1); {|#0:Method(2);|} {|#1:Method(3);|}
                                                    break;
                                            }
                                        }
                                    };

                                    private static void Method(int value)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action<int> Handler { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 0:
                                                     /* note */ Method(1);
                                                     Method(2);
                                                     Method(3);
                                                     break;
                                             }
                                         }
                                     };

                                     private static void Method(int value)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action<int> Handler { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that, when the switch-section label itself is not first on its own line - the whole
    /// <c>switch</c> statement written on one physical line - the split statement is not compensated: no
    /// whitespace run on that line equals the label's column, so the line's own leading whitespace is used
    /// unchanged. This is a deliberate behavior change from the label-relative column this provider used to add
    /// unconditionally for any anchor-scope target that was not first on its own line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySplitStatementDoesNotCompensateWhenSwitchLabelIsNotFirstOnItsLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value) { case 0: Method(1); {|#0:Method(2);|}
                                                break; }
                                        }
                                    };

                                    private static void Method(int value)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action<int> Handler { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value) { case 0: Method(1);
                                             Method(2);
                                                 break; }
                                         }
                                     };

                                     private static void Method(int value)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action<int> Handler { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat));
    }

    /// <summary>
    /// Verifies that, when the physical line's own leading content is an earlier sibling switch section's label,
    /// the split statement is compensated exactly like a line led by the target's own section's label: sibling
    /// sections of one <c>switch</c> statement share the same nesting depth, so their labels share the same
    /// one-level relationship to this section's statements.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySplitStatementCompensatesWhenLineStartsWithAnotherSectionsLabel()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 1: break; case 0: Method(1); {|#0:Method(2);|}
                                                    break;
                                            }
                                        }
                                    };

                                    private static void Method(int value)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action<int> Handler { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 1: break; case 0: Method(1);
                                                     Method(2);
                                                     break;
                                             }
                                         }
                                     };

                                     private static void Method(int value)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action<int> Handler { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat));
    }

    /// <summary>
    /// Verifies that, when a comment precedes the target's own section's label on that label's shared line, the
    /// split statement is still compensated: the comment is attached to the label, not to a statement, so the
    /// line is still a label line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySplitStatementCompensatesWhenCommentPrecedesTheLabelOnItsLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                /* note */ case 0: Method(1); {|#0:Method(2);|}
                                                    break;
                                            }
                                        }
                                    };

                                    private static void Method(int value)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action<int> Handler { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 /* note */ case 0: Method(1);
                                                     Method(2);
                                                     break;
                                             }
                                         }
                                     };

                                     private static void Method(int value)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action<int> Handler { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat));
    }

    /// <summary>
    /// Verifies that, when the physical line's own leading content is an earlier sibling section's own
    /// statement rather than that section's label, the split statement is not compensated: a statement sharing
    /// the textual stretch between two labels is not itself a label, so it carries no one-level relationship,
    /// even though it lies between the enclosing switch statement's opening brace and this section's own last
    /// label.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySplitStatementDoesNotCompensateWhenLineStartsWithAnEarlierSiblingSectionsStatement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 1:
                                                    break; case 0: Method(1); {|#0:Method(2);|}
                                                    break;
                                            }
                                        }
                                    };

                                    private static void Method(int value)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action<int> Handler { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 1:
                                                     break; case 0: Method(1);
                                                     Method(2);
                                                     break;
                                             }
                                         }
                                     };

                                     private static void Method(int value)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action<int> Handler { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat));
    }

    /// <summary>
    /// Characterizes the documented limitation for a switch label that itself spans multiple physical lines,
    /// such as a <c>case</c> pattern with a <c>when</c> clause: the split statement shares a physical line with
    /// the label's own continuation line rather than with a preceding sibling statement, so the anchor column
    /// read from that line is the continuation's own column, not the label's first-line column, and the fix adds
    /// one indentation level too many. This is a known, documented limitation (see RH5103.md), not a defect this
    /// test expects to be fixed - it pins today's accepted behavior so a future change to it is deliberate.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySplitStatementOverIndentsWhenLabelSpansMultipleLinesAndStatementSharesItsContinuationLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 1 when
                                                    Check(value): Method(1); {|#0:Method(2);|}
                                                    break;
                                            }
                                        }
                                    };

                                    private static bool Check(int value)
                                    {
                                        return true;
                                    }

                                    private static void Method(int value)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action<int> Handler { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 1 when
                                                     Check(value): Method(1);
                                                         Method(2);
                                                     break;
                                             }
                                         }
                                     };

                                     private static bool Check(int value)
                                     {
                                         return true;
                                     }

                                     private static void Method(int value)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action<int> Handler { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat));
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

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
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

        // Verifies a switch-section statement chain is rewritten with the section statement indentation
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}