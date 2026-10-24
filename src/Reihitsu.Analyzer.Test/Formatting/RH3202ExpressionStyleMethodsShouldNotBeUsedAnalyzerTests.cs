using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer, RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that expression-bodied methods are detected and can be fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3202-001")]
    [Requirement("SHARED-BODY-002")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyExpressionBodiedMethodsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    public int GetValueExpression() {|#0:=> 42|};
                                    
                                    public int GetValueBlock()
                                    {
                                        return 42;
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      public int GetValueExpression()
                                      {
                                          return 42;
                                      }
                                      
                                      public int GetValueBlock()
                                      {
                                          return 42;
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifying that a throw-expression-bodied method is fixed to a compiling throw statement (not <c>return throw</c>)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyThrowExpressionBodiedMethodIsFixedToThrowStatement()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    public int GetValue() {|#0:=> throw new System.Exception()|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      public int GetValue()
                                      {
                                          throw new System.Exception();
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifying that an async ValueTask expression-bodied method is fixed without a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3202-001")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyAsyncValueTaskExpressionBodiedMethodIsFixed()
    {
        const string testData = """
                                using System.Threading.Tasks;

                                internal class RH3202
                                {
                                    public async ValueTask DoWorkAsync() {|#0:=> await Task.CompletedTask|};
                                }
                                """;

        const string resultData = """
                                  using System.Threading.Tasks;

                                  internal class RH3202
                                  {
                                      public async ValueTask DoWorkAsync()
                                      {
                                          await Task.CompletedTask;
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that a method whose expression body carries a directive before the expression is not reported,
    /// because the formatter refuses to rewrite it and the code fix could therefore not converge
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    public async Task VerifyExpressionBodyWithDirectiveBeforeExpressionIsNotReported()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    public int GetValue() =>
                                #pragma warning disable CS0618
                                        1;
                                #pragma warning restore CS0618
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a method whose expression body carries a directive after the expression is still reported
    /// and fixed, because that directive travels into the generated statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    public int GetValue() {|#0:=> 1|}
                                #pragma warning disable CS0618
                                        ;
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      public int GetValue()
                                      {
                                          return 1
                                  #pragma warning disable CS0618
                                          ;
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifying that the fix keeps the blank line that separates the converted method from the previous member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3202-002")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyBlankLineAboveFixedMethodIsKept()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    private int _value;

                                    public int GetValue() {|#0:=> _value|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      private int _value;

                                      public int GetValue()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifying that a method whose expression starts on the line after the arrow is fixed to a single-spaced return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyExpressionOnNextLineIsFixedToSingleSpacedReturn()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    public int GetValue() {|#0:=>
                                        42|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      public int GetValue()
                                      {
                                          return 42;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifying that the fix changes only the layout of a documentation comment and of a raw string literal in the
    /// methods it converts, and keeps the comment's text and the literal's value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals()
    {
        const string testData = """"
                                public class C
                                {
                                    ///<summary>runs</summary>
                                    ///<returns>one</returns>
                                    public int M() {|RH3202:=> 1|};

                                    public string N() {|RH3202:=>
                                        """
                                        a
                                        """|};
                                }
                                """";

        const string resultData = """"
                                  public class C
                                  {
                                      /// <summary>
                                      /// runs
                                      /// </summary>
                                      /// <returns>one</returns>
                                      public int M()
                                      {
                                          return 1;
                                      }

                                      public string N()
                                      {
                                          return """
                                                 a
                                                 """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Characterizes that the fix collapses two blank lines above the converted method to one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixCollapsesTwoBlankLinesAboveTheMethod()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;


                                    public int GetValue() {|RH3202:=> _value|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      public int GetValue()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    public int First() {|#0:=> 1|};

                                    public int Second() {|#1:=> 2|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      public int First()
                                      {
                                          return 1;
                                      }

                                      public int Second()
                                      {
                                          return 2;
                                      }
                                  }
                                  """;

        // Verifies two expression-bodied methods are fixed in one Fix All iteration
        return new FixAllScenario(testData.Replace("\r\n", "\n"),
                                  resultData.Replace("\r\n", "\n"),
                                  Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}