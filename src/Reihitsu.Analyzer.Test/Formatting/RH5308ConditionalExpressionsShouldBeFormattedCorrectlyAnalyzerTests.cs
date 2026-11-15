using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5308ConditionalExpressionsShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer, RH5308ConditionalExpressionsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that a conditional expression on a single line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-001")]
    public async Task VerifySingleLineConditionalIsNotReported()
    {
        const string testData = """
                                internal class RH5308
                                {
                                    string Method(bool condition)
                                    {
                                        return condition ? "1" : "2";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a correctly aligned multi-line conditional expression is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-002")]
    public async Task VerifyAlignedMultiLineConditionalIsNotReported()
    {
        const string testData = """
                                internal class RH5308
                                {
                                    string Method(bool condition)
                                    {
                                        var value = condition
                                                        ? "1"
                                                        : "2";

                                        return value;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a misaligned multi-line conditional expression is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-002")]
    [Requirement("RH5308-003")]
    [Requirement("RH5308-004")]
    public async Task VerifyMisalignedConditionalIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5308
                                {
                                    string Method(bool condition)
                                    {
                                        var value = condition
                                {|#0:?|} "1"
                                : "2";

                                        return value;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH5308
                                  {
                                      string Method(bool condition)
                                      {
                                          var value = condition
                                                          ? "1"
                                                          : "2";

                                          return value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5308MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that a <c>?</c> token left on the condition line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-001")]
    [Requirement("RH5308-003")]
    [Requirement("RH5308-004")]
    public async Task VerifyQuestionTokenOnConditionLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5308
                                {
                                    string Method(bool condition)
                                    {
                                        var value = condition {|#0:?|} "1"
                                                        : "2";

                                        return value;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH5308
                                  {
                                      string Method(bool condition)
                                      {
                                          var value = condition
                                                          ? "1"
                                                          : "2";

                                          return value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5308MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that two separate misaligned conditional expressions are each detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-002")]
    [Requirement("RH5308-003")]
    [Requirement("RH5308-004")]
    public async Task VerifyMultipleConditionalsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5308
                                {
                                    string First(bool condition)
                                    {
                                        return condition
                                {|#0:?|} "1"
                                : "2";
                                    }

                                    string Second(bool condition)
                                    {
                                        return condition
                                {|#1:?|} "3"
                                : "4";
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH5308
                                  {
                                      string First(bool condition)
                                      {
                                          return condition
                                                     ? "1"
                                                     : "2";
                                      }

                                      string Second(bool condition)
                                      {
                                          return condition
                                                     ? "3"
                                                     : "4";
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5308MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a nested conditional whose operators break the indentation stair is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-002")]
    [Requirement("RH5308-003")]
    [Requirement("RH5308-004")]
    public async Task VerifyNestedConditionalStairIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5308
                                {
                                    string Method(bool a, bool b)
                                    {
                                        var value = a
                                                        ? "1"
                                                        : b
                                {|#0:?|} "2"
                                : "3";

                                        return value;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH5308
                                  {
                                      string Method(bool a, bool b)
                                      {
                                          var value = a
                                                          ? "1"
                                                          : b
                                                              ? "2"
                                                              : "3";

                                          return value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5308MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that two sibling conditionals in the same statement are each detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-002")]
    [Requirement("RH5308-003")]
    [Requirement("RH5308-004")]
    public async Task VerifySiblingConditionalsInOneStatementAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5308
                                {
                                    string Method(bool a, bool b)
                                    {
                                        return Combine(a
                                {|#0:?|} "1"
                                : "2", b
                                {|#1:?|} "3"
                                : "4");
                                    }

                                    string Combine(string x, string y) => x + y;
                                }
                                """;

        const string resultData = """
                                  internal class RH5308
                                  {
                                      string Method(bool a, bool b)
                                      {
                                          return Combine(a
                                                             ? "1"
                                                             : "2",
                                                         b
                                                             ? "3"
                                                             : "4");
                                      }

                                      string Combine(string x, string y) => x + y;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5308MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a wrapped conditional whose question mark is on the line of its condition, or whose colon is on
    /// the line of its true branch, is reported, while a conditional on one line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-001")]
    [Requirement("RH5308-003")]
    public async Task VerifyOperatorThatDoesNotStartItsLineIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(bool a)
                                    {
                                        var x = a {|RH5308:?|}
                                                1 :
                                                2;
                                        var y = a
                                                    ? 1 {|RH5308::|} 2;
                                        var z = a ? 1 : 2;

                                        return x;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a conditional nested in the false branch of a wrapped conditional is reported when its colon is on
    /// the line of its true branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-001")]
    [Requirement("RH5308-003")]
    public async Task VerifyNestedConditionalInBranchIsCheckedTheSameWay()
    {
        const string testCode = """
                                class C
                                {
                                    int M(bool a, bool b)
                                    {
                                        var x = a
                                                    ? 1
                                                    : b
                                                        ? 2 {|RH5308::|}
                                                        3;

                                        return x;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an operator which starts its line outside the column one level deeper than the condition, and one
    /// further level per nesting, is reported, while operators in those columns are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-002")]
    [Requirement("RH5308-003")]
    public async Task VerifyOperatorThatStartsItsLineOutsideItsColumnIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(bool a, bool b)
                                    {
                                        var x = a
                                                {|RH5308:?|} 1
                                                : 2;
                                        var y = a
                                                    ? 1
                                                    : b
                                                    {|RH5308:?|} 2
                                                    : 3;
                                        var z = a
                                                    ? 1
                                                    : b
                                                        ? 2
                                                        : 3;

                                        return x;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that one diagnostic per outermost conditional is reported, on the question mark when both the question
    /// mark and the colon are misplaced
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-003")]
    public async Task VerifyOnlyFirstMisplacedOperatorIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(bool a)
                                    {
                                        var x = a {|RH5308:?|}
                                                1 :
                                                2;

                                        return x;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix puts every question mark and colon on its own line followed by its branch, one level
    /// deeper than the condition and one further level for each nested conditional
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-004")]
    public async Task VerifyFixLaysOutConditionalAsStair()
    {
        const string testCode = """
                                class C
                                {
                                    int M(bool a, bool b)
                                    {
                                        var x = a {|RH5308:?|}
                                                1 :
                                                b ? 2 : 3;

                                        return x;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int M(bool a, bool b)
                                     {
                                         var x = a
                                                     ? 1
                                                     : b
                                                         ? 2
                                                         : 3;

                                         return x;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix puts every question mark and colon on its own line followed by its branch, one level
    /// deeper than the condition and one further level for each nested conditional, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-004")]
    public async Task VerifyFixLaysOutConditionalAsStairWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    int M(bool a, bool b)
                                    {
                                        var x = a {|RH5308:?|}
                                                1 :
                                                b ? 2 : 3;

                                        return x;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int M(bool a, bool b)
                                     {
                                         var x = a
                                                     ? 1
                                                     : b
                                                         ? 2
                                                         : 3;

                                         return x;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that a refused comment between the type and the name of the declaration that holds the conditional
    /// expression withholds the fix: the fix formats that statement, which the formatter refuses, although the
    /// conditional expression holds no refused shape
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task NoFixIsOfferedWhenTheStatementHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              internal class C
                              {
                                  private string M(bool condition)
                                  {
                                      string /* c */ value = condition {|RH5308:?|} "1"
                                                                 : "2";

                                      return value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a comment behind the <c>?</c> of one conditional and behind the <c>:</c> of another
    /// is reported, and no code fix is offered for either, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-005")]
    public async Task SpecExampleNoFixIsOfferedForACommentBehindAnOperator()
    {
        const string source = """
                              class C
                              {
                                  int M(bool a)
                                  {
                                      var x = a {|RH5308:?|} // c
                                          1
                                          : 2;
                                      var y = a
                                                  ? 1 {|RH5308::|} // c
                                                  2;

                                      return x;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(source));
    }

    /// <summary>
    /// Verifies the specification example: the conditional whose <c>?</c> a comment follows is reported, although the formatter
    /// leaves the input unchanged, because the formatter refuses the shape as unsupported and does not keep it; the fix is
    /// withheld
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-010")]
    [Requirement("RH5308-005")]
    public async Task SpecExampleConditionalTheFormatterRefusesIsReported()
    {
        const string source = """
                              class C
                              {
                                  int M(bool a)
                                  {
                                      var x = a {|RH5308:?|} // c
                                          1
                                          : 2;

                                      return x;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that an inline block comment behind the <c>?</c> of a conditional that is not laid out yet withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-005")]
    public async Task NoFixIsOfferedForAnInlineCommentBehindTheQuestionMark()
    {
        const string source = """
                              class C
                              {
                                  int M(bool a)
                                  {
                                      var z = a {|RH5308:?|} /* c */ 1
                                              : 2;

                                      return z;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a directive and disabled text behind the <c>?</c> withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-005")]
    public async Task NoFixIsOfferedForADirectiveBehindTheQuestionMark()
    {
        const string source = """
                              class C
                              {
                                  int M(bool a)
                                  {
                                      return a {|RH5308:?|}
                              #if DEBUG
                                          1
                              #else
                                          2
                              #endif
                                          : 3;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a reported conditional without a comment withholds its fix when the statement the fix formats holds another
    /// conditional with a comment behind its operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-005")]
    public async Task NoFixIsOfferedWhenTheStatementHoldsAnotherConditionalWithACommentBehindItsOperator()
    {
        const string source = """
                              class C
                              {
                                  int M(bool a)
                                  {
                                      return N(a {|RH5308:?|} 1
                                               : 2, a ? /* c */ 3 : 4);
                                  }

                                  int N(int x, int y) => x + y;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment on the line above the operator's line does not withhold
    /// the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-005")]
    public async Task SpecExampleCommentAboveTheOperatorsLineDoesNotWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  int M(bool a)
                                  {
                                      var x = a
                                              // c
                                              {|RH5308:?|} 1
                                              : 2;

                                      return x;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   class C
                                   {
                                       int M(bool a)
                                       {
                                           var x = a

                                                       // c
                                                       ? 1
                                                       : 2;

                                           return x;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
        await VerifyCodeFixWithEndOfLine(source, fixedSource, "\r\n");
    }

    /// <summary>
    /// Verifies the specification example: a conditional in an argument of an <c>[assembly:]</c> attribute stands in no statement
    /// and no member, so it is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-006")]
    [Requirement("SHARED-FIX-005")]
    public async Task SpecExampleNoFixIsOfferedForAConditionalInAnAssemblyAttribute()
    {
        const string source = """
                              [assembly: System.CLSCompliant(true {|RH5308:?|}
                                  true :
                                  false)]
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a conditional in an argument of a <c>[module:]</c> attribute is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-006")]
    [Requirement("SHARED-FIX-005")]
    public async Task NoFixIsOfferedForAConditionalInAModuleAttribute()
    {
        const string source = """
                              [module: System.CLSCompliant(true {|RH5308:?|}
                                  true :
                                  false)]
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a conditional in an argument of an attribute on a type stands in a member declaration, so the fix is still
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-006")]
    public async Task FixIsOfferedForAConditionalInAnAttributeOnAType()
    {
        const string source = """
                              [System.CLSCompliant(true {|RH5308:?|}
                                  true :
                                  false)]
                              public class C
                              {
                              }
                              """;

        Assert.IsNotNull(await TryApplyCodeFixAsync(RemoveMarkup(source)), "The fix must be offered for a conditional in a type's attribute.");
        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: in a file with a syntax error elsewhere the conditional is reported, and no code fix
    /// is offered, because the formatter returns the document unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task SpecExampleNoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              class C
                              {
                                  int M(bool a)
                                  {
                                      var x = a {|RH5308:?|}
                                              1 :
                                              2;

                                      return x;
                                  }

                                  void N(
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a conditional operator behind a comment that starts its line outside the operator column is reported, although the operator behind the comment reaches that column, and that the code fix is withheld or lays the line out in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-002")]
    [Requirement("SHARED-COMMENT-011")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyOperatorBehindMisplacedLeadingCommentIsReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    int M(bool a)
                                    {
                                        return a
                                           /* t */ {|RH5308:?|} 1
                                                   : 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int M(bool a)
                                     {
                                         return a
                                                    /* t */ ? 1
                                                    : 2;
                                     }
                                 }
                                 """;

        await VerifyCodeFixIsWithheldOrProducesOneOf(ConvertEndOfLine(testData, endOfLine), true, ConvertEndOfLine(fixedData, endOfLine));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    string Method(bool a, bool b)
                                    {
                                        return (a
                                {|#0:?|} "1"
                                : "2") + (b
                                {|#1:?|} "3"
                                : "4");
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     string Method(bool a, bool b)
                                     {
                                         return (a
                                                     ? "1"
                                                     : "2") + (b
                                                                   ? "3"
                                                                   : "4");
                                     }
                                 }
                                 """;

        // The two conditionals sit side by side in one return statement, so both resolve to the identical fix
        // target: the enclosing return statement. The batch fixer discards the second, overlapping action, and
        // the surviving single fix already re-anchors both conditionals' ? and : tokens in one pass
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5308MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}