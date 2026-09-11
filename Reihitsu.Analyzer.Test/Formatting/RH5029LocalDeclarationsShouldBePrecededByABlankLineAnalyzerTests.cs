using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer, RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a local declaration directly follows an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenLocalDeclarationIsNotPrecededByBlankLine()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        Consume();
                                        {|#0:var|} value = GetValue();
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     public void Execute()
                                     {
                                         Consume();

                                         var value = GetValue();
                                     }

                                     private string GetValue()
                                     {
                                         return string.Empty;
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that the inserted blank line's indentation matches the preceding statement's own column when that
    /// column is anchored to an object initializer rather than derived from brace-scope nesting depth, which
    /// understates an anchor-derived column by not accounting for the initializer's own alignment (issue #748)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenLocalDeclarationInsideObjectInitializerIsNotPrecededByBlankLine()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = () =>
                                        {
                                                                          Consume(); {|#0:var|} value = GetValue();
                                        }
                                    };

                                    private static string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private static void Consume()
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action Handler { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = () =>
                                         {
                                                                           Consume();

                                                                           var value = GetValue();
                                         }
                                     };

                                     private static string GetValue()
                                     {
                                         return string.Empty;
                                     }

                                     private static void Consume()
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action Handler { get; set; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that the inserted blank line's indentation is read from the preceding statement's own start line
    /// rather than from whichever line its last token happens to sit on. A multi-line preceding statement's own
    /// column is anchored, inside an object initializer, to the same anchor as the moved statement, but its last
    /// token sits on an unrelated continuation line whose indentation carries no such meaning (issue #748)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAnchorsOnPrecedingStatementsOwnLineWhenItSpansMultipleLines()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = () =>
                                        {
                                                                          Baz(
                                                                              1,
                                                                              2); {|#0:var|} y = 1;
                                        }
                                    };

                                    private static void Baz(int a, int b)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action Handler { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = () =>
                                         {
                                                                           Baz(
                                                                               1,
                                                                               2);

                                                                           var y = 1;
                                         }
                                     };

                                     private static void Baz(int a, int b)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action Handler { get; set; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that the anchor is the target's actual preceding sibling statement, not merely the innermost
    /// statement enclosing the previous token. When the preceding sibling has an unbraced embedded body, the
    /// previous token sits inside that embedded statement - one indentation level deeper than the sibling whose
    /// column the target must actually match (issue #748)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAnchorsOnPrecedingSiblingRatherThanItsUnbracedEmbeddedStatement()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = () =>
                                        {
                                                                          if (Check())
                                                                              Baz(1, 2); {|#0:var|} y = 1;
                                        }
                                    };

                                    private static bool Check()
                                    {
                                        return true;
                                    }

                                    private static void Baz(int a, int b)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action Handler { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = () =>
                                         {
                                                                           if (Check())
                                                                               Baz(1, 2);

                                                                           var y = 1;
                                         }
                                     };

                                     private static bool Check()
                                     {
                                         return true;
                                     }

                                     private static void Baz(int a, int b)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action Handler { get; set; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that, when the target statement's anchor is inside an object initializer and the preceding
    /// statement shares its switch-section <c>case</c> label's own line, the inserted blank line's indentation is
    /// the label's column plus one <see cref="Reihitsu.Core.SyntaxIndentationUtilities.IndentSize"/> - matching
    /// where the section's own <c>break;</c> already sits - rather than the label's own column (issue #786)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAnchorsOnLabelColumnPlusIndentSizeWhenPrecedingStatementSharesSwitchLabelLine()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                                          case 0: Method(1); {|#0:var|} y = 1;
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

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                                           case 0: Method(1);

                                                                               var y = 1;
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

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that no extra indentation level is added when the anchor line's own leading content is an earlier
    /// sibling statement rather than the section's own label. That sibling's column is already correct, so adding
    /// a level - the same mistake the issue's own suggested predicate would make - would place the target one
    /// level too deep (issue #786)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticKeepsSiblingColumnWhenPrecedingLineStartsWithEarlierSibling()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 0:
                                                    Method(1); Method(2); {|#0:var|} y = 1;
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

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 0:
                                                     Method(1); Method(2);

                                                     var y = 1;
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

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that no extra indentation level is added when the anchor line's own leading content is a comment
    /// rather than the section's own label (issue #786)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticKeepsSiblingColumnWhenPrecedingLineStartsWithComment()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 0:
                                                    /* note */ Method(1); {|#0:var|} y = 1;
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

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 0:
                                                     /* note */ Method(1);

                                                     var y = 1;
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

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that an explicitly braced switch section is unaffected: the target's direct parent is the block,
    /// not the section itself, so no label relationship applies (issue #786)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticUsesBlockIndentationForExplicitlyBracedSwitchSection()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 0:
                                                {
                                                    Method(1); {|#0:var|} y = 1;
                                                    break;
                                                }
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

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 0:
                                                 {
                                                     Method(1);

                                                     var y = 1;
                                                     break;
                                                 }
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

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that, outside any anchor scope, a switch label's own mis-indented column is never propagated: the
    /// canonical, level-derived column self-corrects it instead (issue #786)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticUsesCanonicalIndentationWhenLabelSharesLineWithoutAnchorScope()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                                          case 0: Method(1); {|#0:var|} y = 1;
                                                              break;
                                        }
                                    }

                                    private void Method(int value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                                           case 0: Method(1);

                                                 var y = 1;
                                                               break;
                                         }
                                     }

                                     private void Method(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that, when the switch-section label itself is not first on its own line - the whole
    /// <c>switch</c> statement written on one physical line - the shared base cannot compensate: no whitespace
    /// run on that line equals the label's column, so the line's own leading whitespace is used unchanged. This
    /// is a documented limitation rather than a defect (issue #786)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticDoesNotCompensateWhenSwitchLabelItselfIsNotFirstOnItsLine()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value) { case 0: Method(1); {|#0:var|} y = 1; break; }
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

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value) { case 0: Method(1);

                                             var y = 1; break; }
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

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab-indented anchor line is compensated by appending four spaces, matching
    /// <see cref="Reihitsu.Analyzer.CodeFixes.Rules.Layout.RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider"/>'s
    /// own behavior for the identical shape, rather than repeating the line's own indentation character
    /// (issue #786)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticCompensatesTabIndentedLabelLineByAppendingFourSpaces()
    {
        const string testCode = "internal class RH5029\n{\n\tprivate readonly Config _config = new Config\n\t{\n\t\tHandler = value =>\n\t\t{\n\t\t\tswitch (value)\n\t\t\t{\n\t\t\t\tcase 0: Method(1); {|#0:var|} y = 1;\n\t\t\t\t\tbreak;\n\t\t\t}\n\t\t}\n\t};\n\n\tprivate static void Method(int value)\n\t{\n\t}\n}\ninternal sealed class Config\n{\n\tpublic System.Action<int> Handler { get; set; }\n}";
        const string fixedCode = "internal class RH5029\n{\n\tprivate readonly Config _config = new Config\n\t{\n\t\tHandler = value =>\n\t\t{\n\t\t\tswitch (value)\n\t\t\t{\n\t\t\t\tcase 0: Method(1);\n\n\t\t\t\t    var y = 1;\n\t\t\t\t\tbreak;\n\t\t\t}\n\t\t}\n\t};\n\n\tprivate static void Method(int value)\n\t{\n\t}\n}\ninternal sealed class Config\n{\n\tpublic System.Action<int> Handler { get; set; }\n}";

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All computes each target's indentation against the unmodified document, so a second
    /// diagnostic sharing the same label line is not compensated twice once the first has conceptually moved to
    /// its own line (issue #786)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixAllCompensatesBothDiagnosticsOnSharedLabelLineWithoutDoubleCompensation()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 0: Method(1); {|#0:var|} y = 1; Method(2); {|#1:var|} z = 2;
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

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 0: Method(1);

                                                     var y = 1; Method(2);

                                                     var z = 2;
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

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat, 2));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a local declaration in a switch section directly follows a statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenLocalDeclarationInSwitchSectionIsNotPrecededByBlankLine()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                Consume();
                                                {|#0:var|} text = GetValue();
                                                break;
                                        }
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 Consume();

                                                 var text = GetValue();
                                                 break;
                                         }
                                     }

                                     private string GetValue()
                                     {
                                         return string.Empty;
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a local declaration is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenLocalDeclarationIsFirstStatementInBlock()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        var value = GetValue();
                                        Consume(value);
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a local declaration is the first statement in a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenLocalDeclarationIsFirstStatementInSwitchSection()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                var text = GetValue();
                                                Consume(text);
                                                break;
                                        }
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for consecutive local declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForConsecutiveLocalDeclarations()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        Consume();

                                        var first = GetValue();
                                        var second = GetValue();
                                        Consume(first + second);
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }

                                    private void Consume(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a local declaration already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenLocalDeclarationAlreadyHasPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        Consume();

                                        var value = GetValue();
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when the line immediately preceding the local declaration is a
    /// preprocessor directive, which acts as a transparent boundary rather than ordinary preceding content
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenLocalDeclarationDirectlyFollowsPreprocessorDirective()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        Consume();
                                #pragma warning disable CS0219
                                        var value = GetValue();
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when the local declaration directly follows an <c>#endif</c> directive,
    /// matching the representative case from issue #350
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenLocalDeclarationDirectlyFollowsEndIfDirective()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        Consume();
                                #if true
                                        Consume();
                                #endif
                                        var value = GetValue();
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        Consume();
                                        {|#0:var|} first = GetValue();
                                        Consume(first);
                                        {|#1:var|} second = GetValue();
                                        Consume(second);
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }

                                    private void Consume(string value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     public void Execute()
                                     {
                                         Consume();

                                         var first = GetValue();
                                         Consume(first);

                                         var second = GetValue();
                                         Consume(second);
                                     }

                                     private string GetValue()
                                     {
                                         return string.Empty;
                                     }

                                     private void Consume()
                                     {
                                     }

                                     private void Consume(string value)
                                     {
                                     }
                                 }
                                 """;

        // Two independent consumption/declaration pairs, each missing its separating blank line, so the second
        // fix's insertion point is unaffected by the first occurrence's own blank-line insertion
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}