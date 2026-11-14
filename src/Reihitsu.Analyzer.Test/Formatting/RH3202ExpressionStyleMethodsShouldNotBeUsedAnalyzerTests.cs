using System.Threading.Tasks;

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
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat, 2));
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
    /// Verifies that the fix carries a comment on its own line after the arrow into the block, in front of the created
    /// statement, as the formatter does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyCommentAfterArrowLeadsTheCreatedStatement()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    public int GetValue() {|#0:=>
                                        // why
                                        42|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      public int GetValue()
                                      {
                                          // why
                                          return 42;
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix carries a comment on its own line before the arrow into the block, in front of the
    /// created statement, as the formatter does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyCommentBeforeArrowLeadsTheCreatedStatement()
    {
        const string testData = """
                                internal class RH3202
                                {
                                    public int GetValue()
                                        // why
                                        {|#0:=> 42|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3202
                                  {
                                      public int GetValue()
                                      {
                                          // why
                                          return 42;
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps the own-line comment before the arrow out of the block, and both comments in their
    /// source order, when the arrow's line carries a comment, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyFixKeepsCommentOrderWhenTheArrowLineCarriesAComment()
    {
        const string source = """
                              internal class RH3202
                              {
                                  public int GetValue()
                                      // a
                                      => // b
                                      42;
                              }
                              """;

        foreach (var text in new[] { source.Replace("\r\n", "\n"), NormalizeToCarriageReturnLineFeed(source) })
        {
            var actual = await ApplyCodeFixAsync(text);
            var ownLineIndex = actual.IndexOf("// a", System.StringComparison.Ordinal);
            var arrowLineIndex = actual.IndexOf("// b", System.StringComparison.Ordinal);
            var statementIndex = actual.IndexOf("return 42;", System.StringComparison.Ordinal);

            Assert.IsGreaterThanOrEqualTo(0, statementIndex, "The fix must convert the method.");
            Assert.IsGreaterThanOrEqualTo(0, ownLineIndex, "The own-line comment must survive the fix.");
            Assert.IsLessThan(arrowLineIndex, ownLineIndex, "The comments must keep their source order.");
            Assert.IsLessThan(statementIndex, arrowLineIndex, "The arrow-line comment must stay in front of the statement.");
            Assert.IsLessThanOrEqualTo(actual.LastIndexOf('{', statementIndex), ownLineIndex, "The own-line comment must not be moved between the opening brace and the created statement.");
        }
    }

    /// <summary>
    /// Verifies that the fix of a method indented with tabs keeps the tabs in front of the signature, which it does not
    /// replace, and lays out the new block from the column the tabs stand for, four columns each
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyFixKeepsTheTabsInFrontOfTheSignatureOfAMethodIndentedWithTabs()
    {
        const string testData = "namespace N\n{\n\tclass C\n\t{\n\t\tint Get() {|#0:=> 1|};\n\t}\n}";
        const string fixedData = "namespace N\n{\n\tclass C\n\t{\n\t\tint Get()\n        {\n            return 1;\n        }\n\t}\n}";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a method indented with tabs keeps the tabs in front of the signature and moves the content
    /// line and the closing marker of a raw string under its opening quotes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-RAW-001")]
    public async Task VerifyFixMovesRawStringOfMethodIndentedWithTabsUnderTheOpeningQuotes()
    {
        const string testData = "namespace N\n{\n\tclass C\n\t{\n\t\tstring Get() {|#0:=> \"\"\"\n                a\n                \"\"\"|};\n\t}\n}";
        const string fixedData = "namespace N\n{\n\tclass C\n\t{\n\t\tstring Get()\n        {\n            return \"\"\"\n                   a\n                   \"\"\";\n        }\n\t}\n}";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
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
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
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
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
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
                     Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix starts a new line in front of a delimited documentation comment behind the <c>:</c> of a
    /// one-line conditional expression, puts <c>?</c> and <c>:</c> at the start of their own lines, and lays out the
    /// converted method around them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public async Task VerifyFixMovesDelimitedDocumentationBehindColonOfOneLineConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|#0:=> b ? 1 : /** d */ 2|};\n}";
        const string fixedData = "internal class C\n{\n    internal int M(bool b)\n    {\n        return b\n                   ? 1\n                   :\n\n        /** d */ 2;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix starts a new line in front of a delimited documentation comment behind a block comment
    /// behind the <c>:</c> of a one-line conditional expression, puts <c>?</c> and <c>:</c> at the start of their own
    /// lines, and lays out the converted method around them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public async Task VerifyFixMovesDelimitedDocumentationBehindBlockCommentBehindColonOfConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|#0:=> b ? 1 : /* c */ /** d */ 2|};\n}";
        const string fixedData = "internal class C\n{\n    internal int M(bool b)\n    {\n        return b\n                   ? 1\n                   : /* c */\n\n        /** d */ 2;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix starts a new line in front of a delimited documentation comment behind the <c>?</c> of a
    /// one-line conditional expression, puts <c>?</c> and <c>:</c> at the start of their own lines, and lays out the
    /// converted method around them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public async Task VerifyFixMovesDelimitedDocumentationBehindQuestionMarkOfOneLineConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|#0:=> b ? /** d */ 1 : 2|};\n}";
        const string fixedData = "internal class C\n{\n    internal int M(bool b)\n    {\n        return b\n                   ?\n\n        /** d */ 1\n                   : 2;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix starts a new line in front of a delimited documentation comment behind a block comment
    /// behind the <c>?</c> of a one-line conditional expression, puts <c>?</c> and <c>:</c> at the start of their own
    /// lines, and lays out the converted method around them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public async Task VerifyFixMovesDelimitedDocumentationBehindBlockCommentBehindQuestionMarkOfConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|#0:=> b ? /* c */ /** d */ 1 : 2|};\n}";
        const string fixedData = "internal class C\n{\n    internal int M(bool b)\n    {\n        return b\n                   ? /* c */\n\n        /** d */ 1\n                   : 2;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix starts a new line in front of a delimited documentation comment behind the <c>:</c> of a
    /// conditional expression wrapped in front of <c>?</c> only, puts <c>?</c> and <c>:</c> at the start of their own
    /// lines, and lays out the converted method around them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public async Task VerifyFixMovesDelimitedDocumentationBehindColonOfPartlyWrappedConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|#0:=> b\n        ? 1 : /** d */ 2|};\n}";
        const string fixedData = "internal class C\n{\n    internal int M(bool b)\n    {\n        return b\n                   ? 1\n                   :\n\n        /** d */ 2;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix starts a new line in front of a delimited documentation comment behind the <c>?</c> of a
    /// conditional expression wrapped in front of <c>:</c> only, puts <c>?</c> and <c>:</c> at the start of their own
    /// lines, and lays out the converted method around them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public async Task VerifyFixMovesDelimitedDocumentationBehindQuestionMarkOfPartlyWrappedConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|#0:=> b ? /** d */ 1\n        : 2|};\n}";
        const string fixedData = "internal class C\n{\n    internal int M(bool b)\n    {\n        return b\n                   ?\n\n        /** d */ 1\n                   : 2;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix starts a new line in front of a delimited documentation comment behind the <c>:</c> of a
    /// parenthesized conditional nested in the <c>?</c> branch of a conditional expression, puts <c>?</c> and <c>:</c>
    /// at the start of their own lines, and lays out the converted method around them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public async Task VerifyFixMovesDelimitedDocumentationBehindColonOfParenthesizedNestedConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b, bool c) {|#0:=> b ? (c ? 1 : /** d */ 2) : 3|};\n}";
        const string fixedData = "internal class C\n{\n    internal int M(bool b, bool c)\n    {\n        return b\n                   ? (c\n                          ? 1\n                          :\n\n        /** d */ 2)\n                   : 3;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix starts a new line in front of a delimited documentation comment behind the <c>?</c> of a
    /// conditional nested in the <c>:</c> branch of a conditional expression, puts <c>?</c> and <c>:</c> at the start
    /// of their own lines, and lays out the converted method around them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public async Task VerifyFixMovesDelimitedDocumentationBehindQuestionMarkOfNestedConditional()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b, bool c) {|#0:=> b ? 1 : c ? /** d */ 2 : 3|};\n}";
        const string fixedData = "internal class C\n{\n    internal int M(bool b, bool c)\n    {\n        return b\n                   ? 1\n                   : c\n                       ?\n\n        /** d */ 2\n                       : 3;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix starts a new line in front of a delimited documentation comment behind a <c>:</c> that
    /// already starts its line in a conditional expression, puts <c>?</c> and <c>:</c> at the start of their own lines,
    /// and lays out the converted method around them, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public async Task VerifyFixMovesDelimitedDocumentationBehindColonStartingItsLine()
    {
        const string testData = "internal class C\n{\n    internal int M(bool b) {|#0:=> b\n        ? 1\n        : /** d */ 2|};\n}";
        const string fixedData = "internal class C\n{\n    internal int M(bool b)\n    {\n        return b\n                   ? 1\n                   :\n\n        /** d */ 2;\n    }\n}";

        await Verify(testData, fixedData, Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3202MessageFormat));
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
    /// Verifies that a block comment behind the signature on its line stays there, the spacing of the signature stays as
    /// written, and the whitespace between the comment and the arrow goes with the arrow
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task FixKeepsABlockCommentBehindTheSignatureOnItsLine()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public int  GetValue( ) /* c */   {|RH3202:=> _value|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      public int  GetValue( ) /* c */
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that a line comment behind the signature stays there when the arrow starts the next line, and the spacing
    /// of the signature stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task FixKeepsALineCommentBehindTheSignatureWhenTheArrowStartsTheNextLine()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public int  GetValue( ) // c
                                        {|RH3202:=> _value|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      public int  GetValue( ) // c
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
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