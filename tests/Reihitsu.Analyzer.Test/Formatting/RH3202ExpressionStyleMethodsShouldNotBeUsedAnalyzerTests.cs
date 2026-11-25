using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
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
    /// Verifies the change example: the fix formats the method it converts, the formatter refuses a method with a comment between
    /// its arrow and its expression, so no fix is offered for either method and both diagnostics stay
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-FIX-010")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBehindTheArrow(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int Get() {|RH3202:=> // c
                                      42|};

                                  int Other() {|RH3202:=>
                                      // after arrow
                                      42|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a block comment behind the arrow on one line withholds the fix, while the diagnostic stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedForABlockCommentBehindTheArrowOnOneLine()
    {
        const string source = """
                              class C
                              {
                                  int Get() {|RH3202:=> /* c */ 42|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix asks the formatter about the method it converts only: of two methods on one line, the one with a
    /// comment behind its arrow gets no fix and stays reported, and the one beside it gets a fix that converts it and gives
    /// the other method a line of its own, leaving its text as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixIsOfferedForAMethodBesideAMethodWithACommentBehindItsArrow(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    int A() => 1; int B() => /* c */ 2;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      int A()
                                      {
                                          return 1;
                                      }
                                      int B() {|RH3202:=> /* c */ 2|};
                                  }
                                  """;

        var source = ConvertEndOfLine(testData, endOfLine);
        var refused = await GetCodeFixActionsAsync(source, RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, root => FindArrow(root, "B").GetLocation());

        Assert.IsEmpty(refused, "The fix must not be offered for the method with the comment behind its arrow.");

        var expected = ConvertEndOfLine(resultData, endOfLine);
        var actual = await TryApplyCodeFixAtAsync(source, RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, root => FindArrow(root, "A").GetLocation());

        Assert.IsNotNull(actual, "The fix must be offered for the method without the comment.");
        Assert.AreEqual(RemoveMarkup(expected), actual, "The fix must convert the method without the comment and leave the other method as written.");

        await Verify(expected);
    }

    /// <summary>
    /// Verifies the change example: the fix formats the method it builds, the formatter refuses a method that holds a comment behind a
    /// binary operator, so no fix is offered and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBehindABinaryOperator(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int M(int a, int b) {|RH3202:=> a + // c
                                                         b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
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
                              internal class RH3202
                              {
                                  public int First() => 1;

                                  public int Second() => 2;
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

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
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a method whose expression stands behind a blank line below the arrow joins the expression to the
    /// created <c>return</c> keyword and drops the blank line, as the formatter does, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    [Requirement("SHARED-BODY-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyExpressionBehindABlankLineIsFixedToSingleSpacedReturn(string endOfLine)
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

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(resultData, endOfLine), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the change example: the fix formats the method it converts, so the blank line in front of the closing
    /// parenthesis of a positional pattern clause in its expression is removed, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixRemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClause(string endOfLine)
    {
        const string testData = """
                                class P
                                {
                                    public void Deconstruct(out int a, out int b)
                                    {
                                        a = 0;
                                        b = 0;
                                    }
                                }

                                class C
                                {
                                    bool M(object o) {|#0:=> o is P(
                                        1,
                                        2

                                    )|};
                                }
                                """;

        const string resultData = """
                                  class P
                                  {
                                      public void Deconstruct(out int a, out int b)
                                      {
                                          a = 0;
                                          b = 0;
                                      }
                                  }

                                  class C
                                  {
                                      bool M(object o)
                                      {
                                          return o is P(
                                                           1,
                                                           2
                                                       );
                                      }
                                  }
                                  """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(resultData, endOfLine), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that the fix keeps the documentation comment of the method it converts as written, and changes only the
    /// indentation of a raw string literal in the body it replaces, keeping the literal's value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTheDocumentationCommentAndChangesOnlyTheLayoutOfRawStringLiterals()
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
                                      ///<summary>runs</summary>
                                      ///<returns>one</returns>
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
    /// Verifies the specified layout example: the fix keeps the two blank lines above the converted method and changes
    /// only the indentation of the raw string literal in the other converted method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3202-002")]
    public async Task VerifyFixKeepsTwoBlankLinesAboveTheMethod()
    {
        const string testData = """"
                                public class C
                                {
                                    private int _value;


                                    public int GetValue() {|RH3202:=> _value|};

                                    public string N() {|RH3202:=>
                                        """
                                        a
                                        """|};
                                }
                                """";

        const string resultData = """"
                                  public class C
                                  {
                                      private int _value;


                                      public int GetValue()
                                      {
                                          return _value;
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
    /// Verifies that the fix keeps the two blank lines below the converted method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3202-002")]
    public async Task VerifyFixKeepsTwoBlankLinesBelowTheMethod()
    {
        const string testData = """
                                class C
                                {
                                    public int GetValue() {|RH3202:=> _value|};


                                    private int _value;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      public int GetValue()
                                      {
                                          return _value;
                                      }


                                      private int _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies the specified example: an async method returning an alias of <c>Task</c> is reported, and no code
    /// fix is offered for it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-004")]
    public async Task VerifyAsyncMethodReturningTaskAliasIsReportedWithoutCodeFix()
    {
        const string source = """
                              using T = System.Threading.Tasks.Task;

                              class C
                              {
                                  async T RunAsync() {|RH3202:=> await T.Delay(1)|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that an async method returning an alias of a generic task type is reported, and no code fix is
    /// offered for it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-004")]
    public async Task VerifyAsyncMethodReturningGenericTaskAliasIsReportedWithoutCodeFix()
    {
        const string source = """
                              using V = System.Threading.Tasks.ValueTask<int>;

                              class C
                              {
                                  async V GetAsync() {|RH3202:=> await System.Threading.Tasks.Task.FromResult(1)|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that async methods returning nullable-annotated task types are fixed: <c>Task?</c> to an expression
    /// statement and <c>Task&lt;int&gt;?</c> to a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-004")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyAsyncMethodsReturningNullableTaskTypesAreFixed()
    {
        const string testData = """
                                #nullable enable

                                using System.Threading.Tasks;

                                internal class RH3202
                                {
                                    public async Task? RunAsync() {|#0:=> await Task.Delay(1)|};

                                    public async Task<int>? GetAsync() {|#1:=> await Task.FromResult(1)|};
                                }
                                """;

        const string resultData = """
                                  #nullable enable

                                  using System.Threading.Tasks;

                                  internal class RH3202
                                  {
                                      public async Task? RunAsync()
                                      {
                                          await Task.Delay(1);
                                      }

                                      public async Task<int>? GetAsync()
                                      {
                                          return await Task.FromResult(1);
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that fixing a document converts the methods with a recognized task type and leaves the method
    /// returning a task alias reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-004")]
    public async Task VerifyFixLeavesAsyncMethodReturningTaskAliasNextToConvertedMethods()
    {
        const string testData = """
                                using System.Threading.Tasks;
                                using T = System.Threading.Tasks.Task;

                                internal class RH3202
                                {
                                    public async T KeptAsync() {|RH3202:=> await T.Delay(1)|};

                                    public async Task FirstAsync() {|RH3202:=> await Task.Delay(1)|};

                                    public async Task<int> SecondAsync() {|RH3202:=> await Task.FromResult(1)|};
                                }
                                """;

        const string resultData = """
                                  using System.Threading.Tasks;
                                  using T = System.Threading.Tasks.Task;

                                  internal class RH3202
                                  {
                                      public async T KeptAsync() {|RH3202:=> await T.Delay(1)|};

                                      public async Task FirstAsync()
                                      {
                                          await Task.Delay(1);
                                      }

                                      public async Task<int> SecondAsync()
                                      {
                                          return await Task.FromResult(1);
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     static config =>
                            {
                                // The diagnostic that keeps no fix is marked in the fixed source
                                config.FixedState.MarkupHandling = MarkupMode.Allow;
                                config.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                config.NumberOfIncrementalIterations = 2;
                                config.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that a comment on its own line between the arrow and the expression withholds the fix, because the formatter
    /// refuses the method it would format, while the diagnostic stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedForACommentLineBehindTheArrow()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    public int GetValue() {|RH3202:=>
                                        // why
                                        42|};
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the change example: a comment on its own line before the arrow, and a comment behind the signature followed by a
    /// comment line before the arrow, withhold the fix, because the formatter refuses the method the fix formats, while both
    /// diagnostics stay
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-BODY-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBeforeTheArrow(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int Other()
                                  // before arrow
                                  {|RH3202:=> 1|};

                                  void Run() // run
                                      // first
                                      {|RH3202:=> Foo()|};

                                  void Foo()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment behind the arrow withholds the fix also when a comment on its own line stands in front of the
    /// arrow, under LF and CRLF, while the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-FIX-010")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedWhenTheArrowLineCarriesAComment(string endOfLine)
    {
        const string source = """
                              internal class RH3202
                              {
                                  public int GetValue()
                                      // a
                                      {|RH3202:=> // b
                                      42|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix of a method indented with tabs keeps the tabs in front of the signature, which it does not
    /// replace, and lays out the new block from the column of the method's first token, each tab counted as one column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyFixKeepsTheTabsInFrontOfTheSignatureOfAMethodIndentedWithTabs()
    {
        const string testData = "namespace N\n{\n\tclass C\n\t{\n\t\tint Get() {|#0:=> 1|};\n\t}\n}";
        const string fixedData = "namespace N\n{\n\tclass C\n\t{\n\t\tint Get()\n  {\n      return 1;\n  }\n\t}\n}";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a method indented with tabs keeps the tabs in front of the signature, lays out the new block
    /// from the column of the method's first token, each tab counted as one column, and moves the content line and the closing
    /// marker of a raw string under its opening quotes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-RAW-001")]
    public async Task VerifyFixMovesRawStringOfMethodIndentedWithTabsUnderTheOpeningQuotes()
    {
        const string testData = "namespace N\n{\n\tclass C\n\t{\n\t\tstring Get() {|#0:=> \"\"\"\n                a\n                \"\"\"|};\n\t}\n}";
        const string fixedData = "namespace N\n{\n\tclass C\n\t{\n\t\tstring Get()\n  {\n      return \"\"\"\n             a\n             \"\"\";\n  }\n\t}\n}";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out a raw string whose closing quotes stand behind tabs as the formatter does: the literal
    /// moves under its opening quotes and its stripped tabs are written as spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public async Task VerifyFixWritesTheStrippedTabsOfARawStringAsSpaces()
    {
        const string testData = """"
                                class C
                                {
                                    string M() {|#0:=> """
                                →→Hello
                                →→"""|};
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string M()
                                     {
                                         return """
                                                Hello
                                                """;
                                     }
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData),
                     fixedData,
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: in a class indented with two spaces, the fix keeps the method's first token in column 2, and
    /// the lines the depth places keep their place relative to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyFixKeepsMethodOfTwoSpaceClassInItsColumn()
    {
        const string testData = "class C\n{\n  int Get() {|#0:=> 1|};\n}";
        const string fixedData = "class C\n{\n  int Get()\n  {\n      return 1;\n  }\n}";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: in a class indented with two spaces, the fix moves the content line and the closing marker of
    /// a raw string under its opening quotes in column 13
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-RAW-001")]
    public async Task VerifyFixMovesRawStringOfMethodInTwoSpaceClassUnderTheOpeningQuotes()
    {
        const string testData = "class C\n{\n  string S() {|#0:=> \"\"\"\n      a\n      \"\"\"|};\n}";
        const string fixedData = "class C\n{\n  string S()\n  {\n      return \"\"\"\n             a\n             \"\"\";\n  }\n}";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that in a class indented with two spaces, the fix moves every line of an interpolated raw string under its
    /// opening quotes in column 14, and the hole's expression line with them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-RAW-001")]
    [Requirement("FMT-RAW-007")]
    public async Task VerifyFixMovesInterpolatedRawStringOfMethodInTwoSpaceClassWithItsHoleLines()
    {
        const string testData = "class C\n{\n  string S(int x) {|#0:=> $\"\"\"\n      a {\n          x\n      } b\n      \"\"\"|};\n}";
        const string fixedData = "class C\n{\n  string S(int x)\n  {\n      return $\"\"\"\n              a {\n                  x\n              } b\n              \"\"\";\n  }\n}";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps two delimited documentation comments behind the previous member where they stand,
    /// inserts no blank line, and lays out the converted method below them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTwoDelimitedDocumentationCommentsBehindPreviousMember()
    {
        const string testData = "internal class C\n{\n    private int _a; /** a */ /** b */\n    public int M() {|#0:=> 1|};\n}";
        const string fixedData = "internal class C\n{\n    private int _a; /** a */ /** b */\n    public int M()\n    {\n        return 1;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps a delimited documentation comment behind the opening brace of the type when the converted
    /// method starts behind it on the brace's line, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixKeepsDelimitedDocumentationBehindOpeningBraceWhenMethodSharesItsLine()
    {
        const string testData = "internal class C\n{ /** d */ public int M() {|#0:=> 1|};\n}";
        const string fixedData = "internal class C\n{ /** d */ public int M()\n  {\n      return 1;\n  }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps a delimited documentation comment behind the previous member when the converted method
    /// starts behind it on its line, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixKeepsDelimitedDocumentationBehindPreviousMemberWhenMethodSharesItsLine()
    {
        const string testData = "internal class C\n{\n    private int _a; /** d */ public int M() {|#0:=> 1|};\n}";
        const string fixedData = "internal class C\n{\n    private int _a; /** d */ public int M()\n                    {\n                        return 1;\n                    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an expression body with a delimited documentation comment behind the <c>:</c> of a one-line conditional:
    /// the formatter refuses the converted method as unsupported, and the diagnostic is still reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixForDelimitedDocumentationBehindColonOfOneLineConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|RH3202:=> b ? 1 : /** d */ 2|};\n}";

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an expression body with a delimited documentation comment behind a block comment behind the <c>:</c> of a conditional:
    /// the formatter refuses the converted method as unsupported, and the diagnostic is still reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixForDelimitedDocumentationBehindBlockCommentBehindColonOfConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|RH3202:=> b ? 1 : /* c */ /** d */ 2|};\n}";

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an expression body with a delimited documentation comment behind the <c>?</c> of a one-line conditional:
    /// the formatter refuses the converted method as unsupported, and the diagnostic is still reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixForDelimitedDocumentationBehindQuestionMarkOfOneLineConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|RH3202:=> b ? /** d */ 1 : 2|};\n}";

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an expression body with a delimited documentation comment behind a block comment behind the <c>?</c> of a conditional:
    /// the formatter refuses the converted method as unsupported, and the diagnostic is still reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixForDelimitedDocumentationBehindBlockCommentBehindQuestionMarkOfConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|RH3202:=> b ? /* c */ /** d */ 1 : 2|};\n}";

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an expression body with a delimited documentation comment behind the <c>:</c> of a partly wrapped conditional:
    /// the formatter refuses the converted method as unsupported, and the diagnostic is still reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixForDelimitedDocumentationBehindColonOfPartlyWrappedConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|RH3202:=> b\n        ? 1 : /** d */ 2|};\n}";

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an expression body with a delimited documentation comment behind the <c>?</c> of a partly wrapped conditional:
    /// the formatter refuses the converted method as unsupported, and the diagnostic is still reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixForDelimitedDocumentationBehindQuestionMarkOfPartlyWrappedConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|RH3202:=> b ? /** d */ 1\n        : 2|};\n}";

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an expression body with a delimited documentation comment behind the <c>:</c> of a parenthesized nested conditional:
    /// the formatter refuses the converted method as unsupported, and the diagnostic is still reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixForDelimitedDocumentationBehindColonOfParenthesizedNestedConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b, bool c) {|RH3202:=> b ? (c ? 1 : /** d */ 2) : 3|};\n}";

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an expression body with a delimited documentation comment behind the <c>?</c> of a nested conditional:
    /// the formatter refuses the converted method as unsupported, and the diagnostic is still reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixForDelimitedDocumentationBehindQuestionMarkOfNestedConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b, bool c) {|RH3202:=> b ? 1 : c ? /** d */ 2 : 3|};\n}";

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an expression body with a delimited documentation comment behind a <c>:</c> that starts its line:
    /// the formatter refuses the converted method as unsupported, and the diagnostic is still reported, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixForDelimitedDocumentationBehindColonStartingItsLine()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|RH3202:=> b\n        ? 1\n        : /** d */ 2|};\n}";

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that a member behind the converted expression body on its line starts on the line after the new closing
    /// brace, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task FixPutsAMemberBehindTheExpressionBodyOnTheNextLine()
    {
        const string testCode = """
                                class C
                                {
                                    private int _value;

                                    public int GetValue() {|RH3202:=> _value|}; private int _x;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     private int _value;

                                     public int GetValue()
                                     {
                                         return _value;
                                     }
                                     private int _x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies the specification example: when the line of the converted method begins with the header of a type
    /// written on one line, the fix separates neither the member in front of it nor the member behind it, and lays out
    /// the block from the method's column, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3202-002")]
    public async Task FixLeavesTheMembersOnTheLinesOfAMethodOfATypeWrittenOnOneLine()
    {
        const string testCode = """
                                namespace N
                                {
                                    class C { int _value; public int GetValue() {|RH3202:=> _value|}; private int _x; }
                                }
                                """;
        const string fixedCode = """
                                 namespace N
                                 {
                                     class C { int _value; public int GetValue()
                                                           {
                                                               return _value;
                                                           } private int _x; }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a member behind the converted method, separated from it by a comment on their line, stays behind
    /// the comment on the line of the new closing brace, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixKeepsACommentBetweenTheConvertedMethodAndTheMemberBehindItAndDoesNotSeparateThem()
    {
        const string testCode = """
                                class C
                                {
                                    private int _value;

                                    public int GetValue() {|RH3202:=> _value|}; /* c */ private int _x;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     private int _value;

                                     public int GetValue()
                                     {
                                         return _value;
                                     } /* c */ private int _x;
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the converted method, separated from the member in front of it by a comment on their line, stays
    /// behind the comment, and its block is laid out from its column, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixKeepsACommentBetweenTheMemberInFrontAndTheConvertedMethodAndDoesNotSeparateThem()
    {
        const string testCode = """
                                class C
                                {
                                    private int _value; /* c */ public int GetValue() {|RH3202:=> _value|};
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     private int _value; /* c */ public int GetValue()
                                                                 {
                                                                     return _value;
                                                                 }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that when the line of the converted method begins with the continuation of a sibling's wrapped
    /// initializer, the fix does not separate the method from the sibling, and lays out the block from the method's
    /// column, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3202-002")]
    public async Task FixLeavesTheMethodOnALineThatBeginsWithTheContinuationOfASiblingsInitializer()
    {
        const string testCode = """
                                class C
                                {
                                    int _a = Compute(1,
                                                     2); int Get() {|RH3202:=> _a|};

                                    static int Compute(int a, int b)
                                    {
                                        return a;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int _a = Compute(1,
                                                      2); int Get()
                                                          {
                                                              return _a;
                                                          }

                                     static int Compute(int a, int b)
                                     {
                                         return a;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that when the line of the converted method begins with a statement inside a sibling's body, the fix does
    /// not separate the method from the sibling's closing brace, and lays out the block from the method's column, under
    /// LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3202-002")]
    public async Task FixLeavesTheMethodOnALineThatBeginsWithAStatementOfASiblingsBody()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;

                                    void A()
                                    {
                                        A(); } int Get() {|RH3202:=> _a|};
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int _a;

                                     void A()
                                     {
                                         A(); } int Get()
                                                {
                                                    return _a;
                                                }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that when the line of the converted method begins with the closing line of a sibling's verbatim string,
    /// the fix does not separate the method from the sibling, and the result is the one the fix gave before the
    /// shared-line separation existed, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3202-002")]
    public async Task FixLeavesTheMethodOnALineThatBeginsWithTheClosingLineOfASiblingsVerbatimString()
    {
        const string testCode = """
                                class C
                                {
                                    string _s = @"a
                                b"; int Get() {|RH3202:=> 1|};
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     string _s = @"a
                                 b"; int Get()
                                     {
                                         return 1;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that when the line of the converted method begins with the tail of the method's own multi-line comment,
    /// the fix does not separate the member behind it, and the result is the one the fix gave before the shared-line
    /// separation existed, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3202-002")]
    public async Task FixLeavesTheMemberBehindOnALineThatBeginsWithTheTailOfAMultiLineComment()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;

                                    /* a
                                       b */ int Get() {|RH3202:=> _a|}; int _y;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int _a;

                                     /* a
                                        b */ int Get()
                                     {
                                         return _a;
                                     } int _y;
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that when the converted method is the first token of its line behind a comment that starts on that line,
    /// the fix still gives the member behind it a line of its own in the column of the line, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3202-002")]
    [Requirement("SHARED-FIX-007")]
    public async Task FixSeparatesTheMemberBehindWhenTheConvertedMethodBeginsItsLineBehindAComment()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;

                                    /* c */ int Get() {|RH3202:=> _a|}; int _y;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int _a;

                                     /* c */ int Get()
                                     {
                                         return _a;
                                     }
                                     int _y;
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specified example: the fix converts only the body and keeps the spacing of the signature, and leaves
    /// no whitespace at the end of the signature's line, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-BODY-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixKeepsTheSpacingOfTheSignature(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public int  GetValue( ) {|RH3202:=> _value|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      public int  GetValue( )
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await VerifyCodeFixWithEndOfLine(testData, resultData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix keeps the documentation comment, the attribute on the method's line, and the spacing of the
    /// modifiers and the signature as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixKeepsTheDocumentationCommentAndTheAttributeOnTheMethodsLine()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    private int _value;

                                    ///<summary>x</summary>
                                    [Obsolete] public  int GetValue( ) {|RH3202:=> _value|};
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class C
                                  {
                                      private int _value;

                                      ///<summary>x</summary>
                                      [Obsolete] public  int GetValue( )
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the whitespace between the signature and the arrow goes with the arrow, so no whitespace is left at
    /// the end of the signature's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task FixLeavesNoWhitespaceBehindTheSignatureWhenSeveralSpacesStoodInFrontOfTheArrow()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public int  GetValue( )    {|RH3202:=> _value|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      public int  GetValue( )
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that a block comment behind the signature on the arrow's line withholds the fix, because the formatter refuses the
    /// method the fix formats, while the diagnostic stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyNoFixIsOfferedForABlockCommentBehindTheSignatureOnTheArrowsLine()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int  GetValue( ) /* c */   {|RH3202:=> _value|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a line comment behind the signature withholds the fix when the arrow starts the next line, because the
    /// formatter refuses the method the fix formats, while the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForALineCommentBehindTheSignatureWhenTheArrowStartsTheNextLine(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int  GetValue( ) // c
                                      {|RH3202:=> _value|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix of a method whose arrow starts the next line keeps the spacing of the signature
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task FixKeepsTheSpacingOfTheSignatureWhenTheArrowStartsTheNextLine()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public int  GetValue( )
                                        {|RH3202:=> _value|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      public int  GetValue( )
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix keeps a constraint clause on the signature's line as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task FixKeepsAConstraintClauseAsWritten()
    {
        const string testData = """
                                class C
                                {
                                    public T  M<T>( )  where T : class {|RH3202:=> default|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      public T  M<T>( )  where T : class
                                      {
                                          return default;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix of an <c>async</c> method keeps the spacing of its signature
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task FixKeepsTheSpacingOfTheSignatureOfAnAsyncMethod()
    {
        const string testData = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    public async  Task<int>  GetAsync( ) {|RH3202:=> await Task.FromResult(1)|};
                                }
                                """;

        const string resultData = """
                                  using System.Threading.Tasks;

                                  class C
                                  {
                                      public async  Task<int>  GetAsync( )
                                      {
                                          return await Task.FromResult(1);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that Fix All converts two methods of one type and keeps the spacing of both signatures and the blank
    /// lines between them, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-BODY-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixAllKeepsTheSignaturesOfTwoMethods(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    public int  First( ) {|RH3202:=> 1|};


                                    public int Second(  int a ) {|RH3202:=> a|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      public int  First( )
                                      {
                                          return 1;
                                      }


                                      public int Second(  int a )
                                      {
                                          return a;
                                      }
                                  }
                                  """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that no fix is offered for a method whose body holds a comment inside a variable declaration, which the
    /// formatter refuses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task NoFixIsOfferedForAMethodWhoseBodyHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              using System;

                              internal class C
                              {
                                  public int Run() {|RH3202:=> Get(() => { int /* c */ y = 1; return y; })|};

                                  private static int Get(Func<int> value)
                                  {
                                      return value();
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment the formatter refuses in another member of the type does not withhold the fix, and that
    /// member stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task FixIsOfferedWhenAnotherMemberHoldsACommentTheFormatterRefuses()
    {
        const string testData = """
                                class C
                                {
                                    private int /* c */ _value = 1;

                                    public int  GetValue( ) {|RH3202:=> _value|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int /* c */ _value = 1;

                                      public int  GetValue( )
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix does not write the whitespace that stood behind the expression body behind the closing brace of the
    /// block, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-PIPE-010")]
    public async Task FixRemovesWhitespaceBehindTheConvertedMethod(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public int GetValue() {|RH3202:=> _value|};···
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

        await VerifyCodeFixWithEndOfLine(testData.Replace('·', ' '), resultData, endOfLine);
    }

    /// <summary>
    /// Verifies the change doc's example: the fix removes the whitespace behind a delimited comment behind the converted method
    /// and keeps the space in front of the comment, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-PIPE-010")]
    public async Task FixRemovesWhitespaceBehindADelimitedCommentBehindTheConvertedMethod(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public int GetValue() {|RH3202:=> _value|}; /* c */···
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      public int GetValue()
                                      {
                                          return _value;
                                      } /* c */
                                  }
                                  """;

        await VerifyCodeFixWithEndOfLine(testData.Replace('·', ' '), resultData, endOfLine);
    }

    /// <summary>
    /// Verifies that in a CRLF document the fix writes the LF behind the converted method as CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesTheDetectedLineEndingBehindTheConvertedMethodInAMixedDocument()
    {
        const string testData = "class C\r\n{\r\n    public int GetValue() => 1;\n    public int GetOther()\r\n    {\r\n        return 2;\r\n    }\r\n}\r\n";
        const string resultData = "class C\r\n{\r\n    public int GetValue()\r\n    {\r\n        return 1;\r\n    }\r\n    public int GetOther()\r\n    {\r\n        return 2;\r\n    }\r\n}\r\n";

        Assert.AreEqual(resultData, await ApplyCodeFixAsync(testData));
    }

    /// <summary>
    /// Verifies that in a document whose line breaks are all a lone carriage return, the fix writes every line break of the
    /// converted method, the one behind its closing brace included, as LF, and the lines outside it keep their carriage return
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesTheDetectedLineEndingBehindTheConvertedMethodInALoneCarriageReturnDocument()
    {
        const string testData = "class C\r{\r    public int GetValue() => 1;\r    public int GetOther()\r    {\r        return 2;\r    }\r}\r";
        const string resultData = "class C\r{\r    public int GetValue()\n    {\n        return 1;\n    }\n    public int GetOther()\r    {\r        return 2;\r    }\r}\r";

        Assert.AreEqual(resultData, await ApplyCodeFixAsync(testData));
    }

    /// <summary>
    /// Verifies the specification example: a conditional group around the signature that closes in front of the arrow is reported,
    /// and no fix is offered, because the formatter refuses the method; parsed without the symbol
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-FIX-010")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SpecExampleConditionalGroupAroundTheSignatureIsReportedWithoutAFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if NET8_0
                                  public int M(int a)
                              #else
                                  public int M(long a)
                              #endif
                                      {|RH3202:=> 1|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example for the build that defines the symbol: the group's <c>#else</c>, its disabled signature,
    /// and its <c>#endif</c> stand in front of the arrow, the method is reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-FIX-010")]
    public async Task ConditionalGroupAroundTheSignatureIsReportedWithoutAFixWhereTheSymbolIsDefined()
    {
        const string source = """
                              class C
                              {
                              #if NET8_0
                                  public int M(int a)
                              #else
                                  public int M(long a)
                              #endif
                                      {|RH3202:=> 1|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, Microsoft.CodeAnalysis.DocumentationMode.Parse, "NET8_0");
    }

    /// <summary>
    /// Verifies the specification example: a conditional group around a constraint clause that closes in front of the arrow is
    /// reported, and no fix is offered, because the formatter refuses the method
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-FIX-010")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task ConditionalGroupAroundAConstraintClauseIsReportedWithoutAFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  T M<T>()
                              #if NET7_0_OR_GREATER
                                      where T : struct
                              #endif
                                      {|RH3202:=> default|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example: a conditional group that chooses the arrow and closes behind the semicolon is reported
    /// in its active branch, and no fix is offered, because the formatter refuses the method
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-FIX-010")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task ConditionalGroupThatChoosesTheArrowIsReportedWithoutAFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int M()
                              #if DEBUG
                                      => 1;
                              #else
                                      {|RH3202:=> 0|};
                              #endif
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of the other side: the same group written around the whole member is reported in the
    /// active branch, and the fix converts that branch's member, as the formatter does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    public async Task SpecExampleConditionalGroupAroundTheWholeMemberIsFixed()
    {
        const string testData = """
                                class C
                                {
                                #if NET8_0
                                    public int M(int a) => 1;
                                #else
                                    public int M(long a) {|RH3202:=> 1|};
                                #endif
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                  #if NET8_0
                                      public int M(int a) => 1;
                                  #else
                                      public int M(long a)
                                      {
                                          return 1;
                                      }
                                  #endif
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that a method with a region directive in the leading trivia of its arrow is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    public async Task VerifyMethodWithARegionDirectiveInFrontOfTheArrowIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    int M()
                                        #region R
                                        => 1;
                                        #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a pragma directive in front of the arrow, which the formatter refuses, withholds the fix, while the diagnostic
    /// stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyNoFixIsOfferedForADirectiveInFrontOfTheArrow()
    {
        const string source = """
                              class C
                              {
                                  int M()
                              #pragma warning disable CS0618
                                      {|RH3202:=> 1|};
                              #pragma warning restore CS0618
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix asks the formatter about the method it converts only: of two methods, the one with a comment in front
    /// of its arrow gets no fix and stays reported, and the one beside it is converted
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixIsOfferedForAMethodBesideAMethodWithACommentInFrontOfItsArrow(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    int A() => 1;

                                    int B() /* c */ => 2;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      int A()
                                      {
                                          return 1;
                                      }

                                      int B() /* c */ {|RH3202:=> 2|};
                                  }
                                  """;

        var source = ConvertEndOfLine(testData, endOfLine);
        var refused = await GetCodeFixActionsAsync(source, RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, root => FindArrow(root, "B").GetLocation());

        Assert.IsEmpty(refused, "The fix must not be offered for the method with the comment in front of its arrow.");

        var expected = ConvertEndOfLine(resultData, endOfLine);
        var actual = await TryApplyCodeFixAtAsync(source, RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, root => FindArrow(root, "A").GetLocation());

        Assert.IsNotNull(actual, "The fix must be offered for the method without the comment.");
        Assert.AreEqual(RemoveMarkup(expected), actual, "The fix must convert the method without the comment and leave the other method as written.");

        await Verify(expected);
    }

    /// <summary>
    /// Verifies the change example: the fix formats the method it builds, the formatter refuses a method that holds a comment in
    /// front of an element access, so no fix is offered and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfAnElementAccess(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int M(int[] arr) {|RH3202:=> arr // first
                                                      [0]|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld for a method that holds a block comment in front of an argument list on one line, or
    /// between the <c>?</c> and the <c>[</c> of a conditional element access, and the diagnostic stays
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("class C\n{\n    int M() {|RH3202:=> Foo /* c */ (1)|};\n\n    int Foo(int x)\n    {\n        return x;\n    }\n}", DisplayName = "Block comment in front of an argument list")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(int[]? n) {|RH3202:=> n? /* c */ [0]|};\n}", DisplayName = "Block comment between the question mark and the bracket")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfAnAttachedPart(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the change example: the fix formats the method it builds, the formatter refuses a method that holds a comment
    /// between the <c>?</c> and the <c>.</c> of a link, so no fix is offered and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentInsideALinkOperator(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int M(string s) {|RH3202:=> s? // note
                                                      .Length ?? 0|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld for a method that holds a block comment between the tokens of a <c>!.</c> or a
    /// <c>!?.</c> on one line, and the diagnostic stays
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int M(string? t) {|RH3202:=> t! /* c */ .Length|};\n}", DisplayName = "Block comment between the exclamation mark and the dot")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? u) {|RH3202:=> u! /* c */ ?.Length|};\n}", DisplayName = "Block comment between the exclamation mark and the question mark")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? u) {|RH3202:=> u!? /* c */ .Length|};\n}", DisplayName = "Block comment between the question mark and the dot of !?.")]
    public async Task VerifyNoFixIsOfferedForACommentInsideANullForgivingLinkOperator(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the change example: the fix formats the method it builds, the formatter refuses a method that holds a comment
    /// between the <c>.</c> of a link and its member name, so no fix is offered and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBetweenTheDotAndTheMemberName(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int M(string s) {|RH3202:=> s. // note
                                                     Length|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld for a method that holds a block comment between the <c>.</c> of a member access or a
    /// member binding and its member name on one line, and the diagnostic stays
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("class C\n{\n    int M(string t) {|RH3202:=> t. /* c */ Length|};\n}", DisplayName = "Block comment behind the dot of a member access")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? s) {|RH3202:=> s?. /* c */ Length|};\n}", DisplayName = "Block comment behind the dot of ?.")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int M(string? t) {|RH3202:=> t!. /* c */ Length|};\n}", DisplayName = "Block comment behind the dot of !.")]
    public async Task VerifyNoFixIsOfferedForABlockCommentBetweenTheDotAndTheMemberName(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the diagnostics marked in the source and the fix result under LF and under CRLF
    /// </summary>
    /// <param name="testCode">The source with markup and LF line endings</param>
    /// <param name="fixedCode">The fixed source with LF line endings</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyUnderBothLineEndings(string testCode, string fixedCode)
    {
        await Verify(testCode, fixedCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Finds the arrow clause of the method with the given name
    /// </summary>
    /// <param name="root">The root to search</param>
    /// <param name="name">The method's name</param>
    /// <returns>The arrow clause</returns>
    private static ArrowExpressionClauseSyntax FindArrow(SyntaxNode root, string name)
    {
        return root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single(method => method.Identifier.ValueText == name).ExpressionBody;
    }

    #endregion // Methods

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
                                  Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}