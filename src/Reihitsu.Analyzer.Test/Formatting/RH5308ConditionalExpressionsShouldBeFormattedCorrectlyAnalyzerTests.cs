using System.Threading.Tasks;

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