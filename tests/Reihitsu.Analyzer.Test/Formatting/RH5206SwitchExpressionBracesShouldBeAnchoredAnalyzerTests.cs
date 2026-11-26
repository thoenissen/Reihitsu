using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer"/> and <see cref="RH5206SwitchExpressionBracesShouldBeAnchoredCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests : BatchCodeFixTestsBase<RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer, RH5206SwitchExpressionBracesShouldBeAnchoredCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a misaligned switch expression in a variable assignment reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    [Requirement("RH5206-002")]
    [Requirement("RH5206-003")]
    public async Task VerifyDiagnosticAndCodeFixForVariableAssignment()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        var state = {|#0:status switch
                                            {
                                              0 => "Idle",
                                                1 => "Running",
                                                _ => "Unknown"
                                            }|};

                                        return state;
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class Example
                                  {
                                      private static string GetState(int status)
                                      {
                                          var state = status switch
                                                      {
                                                          0 => "Idle",
                                                          1 => "Running",
                                                          _ => "Unknown"
                                                      };

                                          return state;
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned switch expression in a return statement reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    [Requirement("RH5206-002")]
    [Requirement("RH5206-003")]
    public async Task VerifyDiagnosticAndCodeFixForReturnStatement()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        return {|#0:status switch
                                                {
                                                    0 => "Idle",
                                                  1 => "Running",
                                                _ => "Unknown"
                                            }|};
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class Example
                                  {
                                      private static string GetState(int status)
                                      {
                                          return status switch
                                                 {
                                                     0 => "Idle",
                                                     1 => "Running",
                                                     _ => "Unknown"
                                                 };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a correctly anchored switch expression does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    public async Task VerifyNoDiagnosticForCorrectSwitchExpression()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        return status switch
                                               {
                                                   0 => "Idle",
                                                   1 => "Running",
                                                   _ => "Unknown"
                                               };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a same-line opening brace reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    [Requirement("RH5206-002")]
    [Requirement("RH5206-003")]
    public async Task VerifyDiagnosticAndCodeFixForSameLineOpeningBrace()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        return {|#0:status switch {
                                                   0 => "Idle",
                                                   1 => "Running",
                                                   _ => "Unknown"
                                               }|};
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class Example
                                  {
                                      private static string GetState(int status)
                                      {
                                          return status switch
                                                 {
                                                     0 => "Idle",
                                                     1 => "Running",
                                                     _ => "Unknown"
                                                 };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that arms sharing a line with a previous arm are not flagged, because no formatter phase splits
    /// switch-expression arms onto separate lines.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    public async Task VerifyNoDiagnosticForArmsSharingALine()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        return status switch
                                               {
                                                   0 => "Idle", 1 => "Running",
                                                   _ => "Unknown"
                                               };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a switch expression written on one line is reported on the whole expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    [Requirement("RH5206-002")]
    public async Task VerifyDiagnosticForSwitchExpressionOnOneLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal string Method(int status)
                                    {
                                        var state = {|#0:status switch { 0 => "Idle", _ => "Other" }|};

                                        return state;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a switch expression whose braces are anchored is reported once when one of its arms is not one
    /// level deeper than the governing expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    [Requirement("RH5206-002")]
    public async Task VerifyDiagnosticForMisindentedArm()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal string Method(int status)
                                    {
                                        var state = {|#0:status switch
                                                    {
                                                      0 => "Idle",
                                                        _ => "Other"
                                                    }|};

                                        return state;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a switch expression is reported when only its closing brace is off the anchor column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    public async Task VerifyDiagnosticForMisplacedClosingBrace()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal string Method(int status)
                                    {
                                        var state = {|#0:status switch
                                                    {
                                                        0 => "Idle",
                                                        _ => "Other"
                                                  }|};

                                        return state;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix anchors the braces and indents the arms under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-003")]
    public async Task VerifySwitchExpressionIsFixedWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal string Method(int status)
                                    {
                                        var state = {|#0:status switch
                                        {
                                            0 => "Idle",
                                            _ => "Other"
                                        }|};

                                        return state;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     internal string Method(int status)
                                     {
                                         var state = status switch
                                                     {
                                                         0 => "Idle",
                                                         _ => "Other"
                                                     };

                                         return state;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              internal class Example
                              {
                                  private static string Outer(int a, int b)
                                  {
                                      var value = a switch
                                      {
                                          0 => b switch
                                          {
                                              0 => "Idle",
                                                  _ => "Unknown"
                                          },
                                              _ => "Fallback"
                                      };

                                      return value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that a switch expression whose arm stands behind a comment that starts its line outside the arm column is reported, although the arm behind the comment reaches the arm column, and that the code fix is withheld or lays the line out in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    [Requirement("SHARED-COMMENT-011")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyArmBehindMisplacedLeadingCommentIsReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    string M(int status)
                                    {
                                        return {|RH5206:status switch
                                               {
                                        /* idle */ 0 => "Idle",
                                                   _ => "Other"
                                               }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string M(int status)
                                     {
                                         return status switch
                                                {
                                                    /* idle */ 0 => "Idle",
                                                    _ => "Other"
                                                };
                                     }
                                 }
                                 """;

        await VerifyCodeFixIsWithheldOrProducesOneOf(ConvertEndOfLine(testData, endOfLine), true, ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that a switch expression whose opening brace stands behind a comment that starts its line outside the column of the governing expression is reported, although the brace behind the comment reaches that column, and that the code fix is withheld or lays the line out in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    [Requirement("SHARED-COMMENT-011")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyOpeningBraceBehindMisplacedLeadingCommentIsReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    string M(int status)
                                    {
                                        return {|RH5206:status switch
                                       /* m */ {
                                                   0 => "Idle",
                                                   _ => "Other"
                                               }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string M(int status)
                                     {
                                         return status switch
                                                /* m */ {
                                                    0 => "Idle",
                                                    _ => "Other"
                                                };
                                     }
                                 }
                                 """;

        await VerifyCodeFixIsWithheldOrProducesOneOf(ConvertEndOfLine(testData, endOfLine), true, ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies the spec example: no blank line is inserted in front of the statement that holds the switch expression
    /// the fix lays out, where formatting the document would insert one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5206-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixInsertsNoBlankLineInFrontOfTheStatementThatHoldsTheSwitchExpression(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    string M(int status)
                                    {
                                        status++;
                                        return {|RH5206:status switch
                                            {
                                              0 => "Idle",
                                                _ => "Other"
                                            }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string M(int status)
                                     {
                                         status++;
                                         return status switch
                                                {
                                                    0 => "Idle",
                                                    _ => "Other"
                                                };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the fix of a switch expression in the condition of an if statement lays out only the
    /// switch expression, from the column of its first token, while the body of the if statement stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5206-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfSwitchExpressionInIfConditionLeavesTheBodyAsWritten(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    bool Check(string s) => true;

                                    void M(int status)
                                    {
                                        if (Check({|RH5206:status switch
                                            {
                                              0 => "Idle",
                                                _ => "Other"
                                            }|}))
                                        {
                                            int  y = 1;
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     bool Check(string s) => true;

                                     void M(int status)
                                     {
                                         if (Check(status switch
                                                   {
                                                       0 => "Idle",
                                                       _ => "Other"
                                                   }))
                                         {
                                             int  y = 1;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix lays out the switch expression from the column its first token keeps, while the misspaced
    /// declaration in front of it stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5206-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixLeavesTheCodeInFrontOfTheSwitchExpressionAsWritten(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    string M(int status)
                                    {
                                        var  r={|RH5206:status switch
                                            {
                                              0 => "Idle",
                                                _ => "Other"
                                            }|};

                                        return r;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string M(int status)
                                     {
                                         var  r=status switch
                                                {
                                                    0 => "Idle",
                                                    _ => "Other"
                                                };

                                         return r;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a switch expression whose arms hold a multi-line property pattern behind <c>and</c> or
    /// <c>not</c> puts the braces of that pattern under the arm's first token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-003")]
    [Requirement("FMT-INDENT-026")]
    public async Task VerifyFixPutsBracesOfPropertyPatternBehindAndOrNotUnderTheArmsFirstToken()
    {
        const string testData = """
                                internal class P
                                {
                                    public int A { get; set; }

                                    public int B { get; set; }
                                }

                                internal class Example
                                {
                                    private static int M(P o)
                                    {
                                        return {|#0:o switch
                                            {
                                                P and { A: 1,
                                                    B: 2 } => 1,
                                                not { A: 3,
                                                    B: 4 } => 2,
                                                _ => 0
                                            }|};
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class P
                                  {
                                      public int A { get; set; }

                                      public int B { get; set; }
                                  }

                                  internal class Example
                                  {
                                      private static int M(P o)
                                      {
                                          return o switch
                                                 {
                                                     P
                                                     and
                                                     {
                                                         A: 1,
                                                         B: 2
                                                     } => 1,
                                                     not
                                                     {
                                                         A: 3,
                                                         B: 4
                                                     } => 2,
                                                     _ => 0
                                                 };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a switch expression whose arm holds a multi-line property pattern with a type behind
    /// <c>not</c> puts the braces of that pattern under the <c>not</c>, the arm's first token, and not under the type
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5206-003")]
    [Requirement("FMT-INDENT-026")]
    public async Task VerifyFixPutsBracesOfTypedPropertyPatternBehindNotUnderTheArmsFirstToken(string endOfLine)
    {
        const string testData = """
                                class P
                                {
                                    public int A { get; set; }

                                    public int B { get; set; }
                                }

                                class Q
                                {
                                    public int A { get; set; }

                                    public int B { get; set; }
                                }

                                class C
                                {
                                    int M(object o)
                                    {
                                        return {|RH5206:o switch
                                            {
                                                P { A: 1,
                                                    B: 2 } or Q { A: 1 } => 1,
                                                not Q { A: 3,
                                                    B: 4 } => 2,
                                                _ => 0
                                            }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class P
                                 {
                                     public int A { get; set; }

                                     public int B { get; set; }
                                 }

                                 class Q
                                 {
                                     public int A { get; set; }

                                     public int B { get; set; }
                                 }

                                 class C
                                 {
                                     int M(object o)
                                     {
                                         return o switch
                                                {
                                                    P
                                                    {
                                                        A: 1,
                                                        B: 2
                                                    }
                                                    or Q { A: 1 } => 1,
                                                    not Q
                                                    {
                                                        A: 3,
                                                        B: 4
                                                    } => 2,
                                                    _ => 0
                                                };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the change doc's example: the switch expression the fix formats holds a comment behind the <c>=&gt;</c> of an
    /// arm, which the formatter refuses, so no fix is offered and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-021")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBehindTheArrowOfAnArm(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int M(int v)
                                  {
                                      var r = {|RH5206:v switch {
                              1 => // one
                              2,
                              _ => 0
                              }|};

                                      return r;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    private static string Outer(int a, int b)
                                    {
                                        var value = {|#0:a switch
                                        {
                                            0 => {|#1:b switch
                                            {
                                                0 => "Idle",
                                                    _ => "Unknown"
                                            }|},
                                                _ => "Fallback"
                                        }|};

                                        return value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     private static string Outer(int a, int b)
                                     {
                                         var value = a switch
                                                     {
                                                         0 => b switch
                                                              {
                                                                  0 => "Idle",
                                                                  _ => "Unknown"
                                                              },
                                                         _ => "Fallback"
                                                     };

                                         return value;
                                     }
                                 }
                                 """;

        // Both switch expressions resolve to the same fix target: the enclosing equals-value clause of the
        // "var value = ..." declaration, since walking up from the nested switch expression passes straight
        // through the outer one without matching a statement, initializer, or arrow clause of its own. The batch
        // fixer discards the second, overlapping action, and the surviving single fix already anchors both the
        // outer and the nested switch expression's braces and arms in one pass
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}