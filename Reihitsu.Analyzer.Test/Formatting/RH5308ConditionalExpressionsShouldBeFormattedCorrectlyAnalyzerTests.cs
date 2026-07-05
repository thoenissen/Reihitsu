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
public class RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer, RH5308ConditionalExpressionsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that a conditional expression on a single line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}