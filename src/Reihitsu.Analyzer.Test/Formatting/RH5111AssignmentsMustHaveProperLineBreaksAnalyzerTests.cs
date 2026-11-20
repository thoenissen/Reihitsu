using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5111AssignmentsMustHaveProperLineBreaksAnalyzer"/> and <see cref="RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider"/>: what is reported, the join the fix writes, and the comments and directives that withhold it
/// </summary>
[TestClass]
public class RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests : BatchCodeFixTestsBase<RH5111AssignmentsMustHaveProperLineBreaksAnalyzer, RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider>
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that applying the first RH5111 code fix produces the same bytes as formatting the source
    /// </summary>
    /// <param name="source">Source containing one RH5111 diagnostic</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertCodeFixMatchesFormatterAsync(string source)
    {
        var cancellationToken = TestContext.CancellationToken;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var expectedText = await ReihitsuFormatter.FormatSyntaxTree(syntaxTree, cancellationToken)
                                                  .Output
                                                  .GetTextAsync(cancellationToken);
        var expected = expectedText.ToString();
        var actual = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods

    #region Tests

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
                              namespace TestNamespace
                              {
                                  class TestClass
                                  {
                                      public void TestMethod()
                                      {
                                          int first
                                              = 1, second
                                              = 2;
                                      }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifying diagnostics for variable declaration with equals on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-003")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForVariableDeclarationWithEqualsOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var {|#0:value
                                                = "test"|};
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             var value = "test";
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for variable declaration with value on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    [Requirement("RH5111-003")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForVariableDeclarationWithValueOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var {|#0:value =
                                                "test"|};
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             var value = "test";
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for field declaration with equals on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForFieldDeclarationWithEqualsOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        private string {|#0:_field
                                            = "test"|};
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         private string _field = "test";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that a parameter default value whose equals sign was wrapped onto the next line is reported and that
    /// the formatter-backed fix joins it in one pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-003")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForParameterDefaultValueWithEqualsOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        void Method({|#0:int value
                                            = 0|})
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         void Method(int value = 0)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for property declaration with equals on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-003")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForPropertyDeclarationWithEqualsOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        {|#0:public string Property { get; set; }
                                            = "test";|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public string Property { get; set; } = "test";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing a multiline property initializer converges with the analyzer's target line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-004")]
    public async Task VerifyCodeFixConvergesForMultilinePropertyInitializer()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        {|#0:public
                                        string Property
                                        {
                                            get;
                                            set;
                                        }
                                        = "test";|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public
                                         string Property
                                         {
                                             get;
                                             set;
                                         } = "test";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for assignment expression with equals on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-003")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForAssignmentExpressionWithEqualsOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            string value;
                                            {|#0:value
                                                = "test"|};
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             string value;
                                             value = "test";
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying code fix only formats the reported assignment expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task CodeFixOnlyFormatsReportedAssignmentExpression()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            string value;
                                            Log( 1,2 );
                                            {|#0:value
                                                = "test"|};
                                            Log( 3,4 );
                                        }

                                        private static void Log(int left, int right)
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             string value;
                                             Log( 1,2 );
                                             value = "test";
                                             Log( 3,4 );
                                         }

                                         private static void Log(int left, int right)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying code fix output matches formatter output when equals spacing also needs normalization
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task CodeFixMatchesFormatterWhenEqualsSpacingNeedsNormalization()
    {
        const string source = """
                              namespace TestNamespace
                              {
                                  class TestClass
                                  {
                                      public void TestMethod()
                                      {
                                          var value  =
                                              "test";
                                      }
                                  }
                              }
                              """;

        await AssertCodeFixMatchesFormatterAsync(source);
    }

    /// <summary>
    /// Verifies that a code fix does not format sibling declarators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task CodeFixOnlyFormatsReportedVariableDeclarator()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            int {|#0:first
                                                = 1|}, second=Get( 1,2 );
                                        }

                                        private static int Get(int left, int right)
                                        {
                                            return left + right;
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             int first = 1, second=Get( 1,2 );
                                         }

                                         private static int Get(int left, int right)
                                         {
                                             return left + right;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a for-statement variable fix uses the owner context for raw-string alignment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task CodeFixMatchesFormatterForForStatementVariable()
    {
        const string source = """"
                              namespace TestNamespace
                              {
                                  class TestClass
                                  {
                                      public void TestMethod()
                                      {
                                          for (var value =
                                              """
                                              test
                                              """;;)
                                          {
                                          }
                                      }
                                  }
                              }
                              """";

        await AssertCodeFixMatchesFormatterAsync(source);
    }

    /// <summary>
    /// Verifies that a using-statement variable fix uses the owner context for raw-string alignment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task CodeFixMatchesFormatterForUsingStatementVariable()
    {
        const string source = """"
                              namespace TestNamespace
                              {
                                  class TestClass
                                  {
                                      public void TestMethod()
                                      {
                                          using (var value =
                                              new System.IO.StringReader(
                                                  """
                                                  test
                                                  """))
                                          {
                                          }
                                      }
                                  }
                              }
                              """";

        await AssertCodeFixMatchesFormatterAsync(source);
    }

    /// <summary>
    /// Verifies that a fixed-statement variable fix uses the owner context for raw-string alignment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task CodeFixMatchesFormatterForFixedStatementVariable()
    {
        const string source = """"
                              namespace TestNamespace
                              {
                                  class TestClass
                                  {
                                      public unsafe void TestMethod()
                                      {
                                          fixed (char* value =
                                              """
                                              test
                                              """)
                                          {
                                          }
                                      }
                                  }
                              }
                              """";

        await AssertCodeFixMatchesFormatterAsync(source);
    }

    /// <summary>
    /// Verifying diagnostics for member initializer with value on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForMemberInitializerWithValueOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var value = new Data
                                            {
                                                {|#0:Name =
                                                    "test"|}
                                            };
                                        }
                                    }

                                    class Data
                                    {
                                        public string Name { get; set; }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             var value = new Data
                                             {
                                                 Name = "test"
                                             };
                                         }
                                     }

                                     class Data
                                     {
                                         public string Name { get; set; }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for index assignment with equals on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForIndexAssignmentWithEqualsOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var values = new int[1];
                                            {|#0:values[0]
                                                = 42|};
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             var values = new int[1];
                                             values[0] = 42;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for correctly formatted variable declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    public async Task VerifyNoDiagnosticsForCorrectlyFormattedVariableDeclaration()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var value = "test";
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostics for multiline value starting on same line as equals
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    public async Task VerifyNoDiagnosticsForMultilineValueStartingOnEqualsLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var value = "multiline " +
                                                        "value";
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying diagnostics for raw multiline string whose opening quotes start on the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForRawMultilineStringWhenQuotesStartOnNextLine()
    {
        const string testData = """"
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var {|#0:value =
                                                """
                                                This is a
                                                multiline string
                                                """|};
                                        }
                                    }
                                }
                                """";

        const string fixedData = """"
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             var value = """
                                                         This is a
                                                         multiline string
                                                         """;
                                         }
                                     }
                                 }
                                 """";

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for interpolated raw multiline string whose opening quotes start on the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    [Requirement("RH5111-004")]
    public async Task VerifyDiagnosticsForInterpolatedRawMultilineStringWhenQuotesStartOnNextLine()
    {
        const string testData = """"
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod(string name)
                                        {
                                            var {|#0:value =
                                                $"""
                                                Hello {name}
                                                """|};
                                        }
                                    }
                                }
                                """";

        const string fixedData = """"
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod(string name)
                                         {
                                             var value = $"""
                                                          Hello {name}
                                                          """;
                                         }
                                     }
                                 }
                                 """";

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for raw multiline string whose opening quotes already start on the equals line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    public async Task VerifyNoDiagnosticsForRawMultilineStringStartingOnEqualsLine()
    {
        const string testData = """"
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var value = """
                                                This is a
                                                multiline string
                                                """;
                                        }
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostics for correctly formatted field declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    public async Task VerifyNoDiagnosticsForCorrectlyFormattedFieldDeclaration()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        private string _field = "test";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostics for correctly formatted property declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    public async Task VerifyNoDiagnosticsForCorrectlyFormattedPropertyDeclaration()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public string Property { get; set; } = "test";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostics for correctly formatted assignment expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    public async Task VerifyNoDiagnosticsForCorrectlyFormattedAssignmentExpression()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            string value;
                                            value = "test";
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostics for correctly formatted member initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    public async Task VerifyNoDiagnosticsForCorrectlyFormattedMemberInitializer()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var value = new Data
                                            {
                                                Name = "test"
                                            };
                                        }
                                    }

                                    class Data
                                    {
                                        public string Name { get; set; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostics for correctly formatted index assignment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    public async Task VerifyNoDiagnosticsForCorrectlyFormattedIndexAssignment()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var values = new int[1];
                                            values[0] = 42;
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostics for an object-initializer member whose value is a nested initializer expression on
    /// the next line, because the formatter never collapses that shape onto the operator line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    public async Task VerifyNoDiagnosticsForNestedInitializerMemberValue()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var value = new Parent
                                            {
                                                Child =
                                                {
                                                    Name = "test"
                                                }
                                            };
                                        }
                                    }

                                    class ChildData
                                    {
                                        public string Name { get; set; }
                                    }

                                    class Parent
                                    {
                                        public ChildData Child { get; } = new ChildData();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostics for assignment without initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    public async Task VerifyNoDiagnosticsForVariableDeclarationWithoutInitializer()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            string value;
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an assignment carrying a comment in the join gap is reported without offering a code fix.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    public async Task VerifyCommentedAssignmentIsReportedWithoutCodeFix()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            var {|#0:value // note
                                                = "test"|};
                                        }
                                    }
                                }
                                """;
        const string codeFixData = """
                                   namespace TestNamespace
                                   {
                                       class TestClass
                                       {
                                           public void TestMethod()
                                           {
                                               var value // note
                                                   = "test";
                                           }
                                       }
                                   }
                                   """;

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a documentation comment in the join gap is gated like other comments, so the registration guard
    /// and the formatter agree on what counts as a comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    public async Task VerifyDocumentationCommentedAssignmentIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   namespace TestNamespace
                                   {
                                       class TestClass
                                       {
                                           public void TestMethod()
                                           {
                                               var value
                                                   /// note
                                                   = "test";
                                           }
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a documentation comment on the member itself does not withhold the fix. It precedes the
    /// property declaration and the join never reaches it, so the documented member is fixed exactly like an
    /// undocumented one and the comment survives.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task VerifyDocumentedPropertyIsFixedAndKeepsItsComment()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        /// <summary>
                                        /// The value.
                                        /// </summary>
                                        {|#0:public string Property { get; set; }
                                            = "test";|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         /// <summary>
                                         /// The value.
                                         /// </summary>
                                         public string Property { get; set; } = "test";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that a region directive preceding the member does not withhold the fix either. Like the member's
    /// documentation comment it sits outside the property declaration's own span, so the interior guard lets the
    /// fix through and the directives survive it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task VerifyPropertyPrecededByRegionDirectiveIsFixedAndKeepsTheDirectives()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        #region Properties

                                        {|#0:public string Property { get; set; }
                                            = "test";|}

                                        #endregion
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         #region Properties

                                         public string Property { get; set; } = "test";

                                         #endregion
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that parameter defaults and enum-member equals values are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    [Requirement("RH5111-003")]
    [Requirement("RH5111-004")]
    public async Task VerifyParameterAndEnumEqualsValuesAreDetectedAndFixed()
    {
        const string testData = """
                                internal enum Values
                                {
                                    {|#0:First =
                                        1|}
                                }

                                internal class TestClass
                                {
                                    void Method({|#1:int value =
                                        1|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First = 1
                                 }

                                 internal class TestClass
                                 {
                                     void Method(int value = 1)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that protected trivia still allows the diagnostic while withholding an unsafe action
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    public async Task VerifyCommentedParameterDefaultIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method({|#0:int value = // keep
                                        1|})
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<ParameterSyntax>().Single().GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that an object-initializer member whose equals sign starts a line below its target is reported and
    /// joined
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-004")]
    public async Task VerifyMemberInitializerWithEqualsOnNewLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    public TestClass Create()
                                    {
                                        return new TestClass
                                               {
                                                   {|#0:Value
                                                       = 1|}
                                               };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public int Value { get; set; }

                                     public TestClass Create()
                                     {
                                         return new TestClass
                                                {
                                                    Value = 1
                                                };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that an array initializer that starts a line below the equals sign is reported on the whole declarator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    [Requirement("RH5111-003")]
    public async Task VerifyArrayInitializerOnNewLineIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void TestMethod()
                                    {
                                        int[] {|#0:values =
                                        {
                                            1
                                        }|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that an object-initializer member whose collection initializer starts a line below the equals sign is
    /// reported, unlike a nested object initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    public async Task VerifyCollectionInitializerMemberValueOnNewLineIsReported()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    public List<int> Values { get; set; } = new();

                                    public TestClass Create()
                                    {
                                        return new TestClass
                                               {
                                                   {|#0:Values =
                                                   {
                                                       1
                                                   }|}
                                               };
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that a declarator whose equals sign and value both start their own lines is reported once and that
    /// the fix joins both
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-002")]
    [Requirement("RH5111-003")]
    [Requirement("RH5111-004")]
    public async Task VerifyEqualsAndValueBothOnNewLinesAreReportedOnceAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void TestMethod()
                                    {
                                        var {|#0:value
                                            =
                                            1|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public void TestMethod()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that the fix of a property whose target and value both span several lines matches the formatter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task CodeFixMatchesFormatterForMultilinePropertyWithMultilineValue()
    {
        const string source = """"
                              namespace TestNamespace
                              {
                                  class TestClass
                                  {
                                      public string Property
                                      {
                                          get;
                                          set;
                                      }
                                          = """
                                            test
                                            """;
                                  }
                              }
                              """";

        await AssertCodeFixMatchesFormatterAsync(source);
    }

    /// <summary>
    /// Verifying that a property carrying a comment between its accessor list and its equals sign is reported without
    /// offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    public async Task VerifyCommentedPropertyInitializerIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:public int Value { get; set; } // note
                                        = 5;|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Single().GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that an assignment carrying a comment between its equals sign and its value is reported without
    /// offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    public async Task VerifyCommentedAssignmentExpressionIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void TestMethod(int value)
                                    {
                                        {|#0:value = /* note */
                                            2|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<AssignmentExpressionSyntax>().Single().GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a declarator with an empty conditional directive pair between its name and its equals sign is
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDeclaratorWithConditionalDirectiveInFrontOfItsEqualsSignIsReportedWithoutFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void TestMethod()
                                    {
                                        var {|RH5111:value
                                #if !UNDEFINED_SYMBOL
                                #endif
                                            = 1|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifying that a declarator is reported, and no code fix is offered, for every shape of conditional directive
    /// between its name and its value: the analyzer judges the target and the value the analyzed build compiles
    /// </summary>
    /// <param name="directives">The conditional directives, the code they hold, and the value with the end of the reported span</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if FEATURE\n#endif\n            = 1|}", DisplayName = "Empty block")]
    [DataRow("#if NEVER\n            = 2\n#endif\n            = 1|}", DisplayName = "Excluded value")]
    [DataRow("#if !NEVER\n            = 1|}\n#endif", DisplayName = "Active value")]
    [DataRow("#if NEVER\n            = 2\n#else\n            = 1|}\n#endif", DisplayName = "If and else")]
    [DataRow("#if NEVER\n            = 2\n#elif !NEVER\n            = 1|}\n#endif", DisplayName = "If and elif")]
    [DataRow("#if false\n            = 2\n#endif\n            = 1|}", DisplayName = "If false")]
    [Requirement("RH5111-005")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyEveryShapeOfConditionalDirectiveAroundTheEqualsSignKeepsTheReportAndWithholdsTheFix(string directives)
    {
        var testData = $$"""
                         class C
                         {
                             void M()
                             {
                                 var {|RH5111:value
                         {{directives}}
                                     ;
                             }
                         }
                         """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifying that Fix All joins the declarators without a conditional directive around their equals signs and leaves
    /// the one with such a directive reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyFixAllJoinsOnlyTheDeclaratorsWithoutAConditionalDirective()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var {|RH5111:value
                                #if FEATURE
                                #endif
                                            = 1|};
                                        var {|RH5111:other
                                            = 2|};
                                        var {|RH5111:third
                                            = 3|};
                                    }
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var {|RH5111:value
                                 #if FEATURE
                                 #endif
                                             = 1|};
                                         var other = 2;
                                         var third = 3;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifying the specification example: a conditional directive between the target and its equals sign, and one
    /// inside the value, withhold only the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyConditionalDirectiveInTheJudgedSpanOrInsideTheValueWithholdsOnlyTheFix()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var {|RH5111:value
                                #if DEBUG
                                #endif
                                            = 1|};
                                        var {|RH5111:other
                                            = N(1,
                                #if DEBUG
                                #endif
                                                2)|};
                                    }

                                    int N(int a, int b) => a + b;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifying that a declarator with an empty conditional directive pair between its equals sign and its value is
    /// reported, with and without the symbol defined, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDeclaratorWithConditionalDirectiveBehindItsEqualsSignIsReportedWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var {|RH5111:value =
                                #if DEBUG
                                #endif
                                            1|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifying that a comment in front of a reported enum member does not withhold the fix and is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    [Requirement("RH5111-005")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyCommentInFrontOfEnumMemberDoesNotWithholdTheFix()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                    // second
                                    {|#0:Second =
                                        2|},
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                     // second
                                     Second = 2,
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that a comment in front of a reported parameter does not withhold the fix and is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-002")]
    [Requirement("RH5111-005")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyCommentInFrontOfParameterDoesNotWithholdTheFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first,
                                                // second
                                                {|#0:int second =
                                                    1|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 // second
                                                 int second = 1)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that the fix joins the equals sign under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task VerifyVariableDeclarationIsFixedWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void TestMethod()
                                    {
                                        var {|#0:value
                                            = 1|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public void TestMethod()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: an enum member's <c>=</c> below its name is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    public async Task VerifyEnumMemberEqualsBelowNameIsReported()
    {
        const string testData = """
                                enum E
                                {
                                    {|#0:A
                                        = 1|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member whose <c>=</c> stands below its name joins the member in one application and
    /// keeps the trailing comma, which belongs to the enum, not to the member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-004")]
    public async Task VerifyEnumMemberEqualsBelowNameIsFixedInOneApplication()
    {
        const string testData = """
                                enum E
                                {
                                    {|#0:A
                                        = 1|},
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config =>
                            {
                                config.NumberOfIncrementalIterations = 1;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     static config =>
                            {
                                config.NumberOfIncrementalIterations = 1;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member whose <c>=</c> stands alone on its line joins name, <c>=</c>, and value in one
    /// application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task VerifyLoneEnumMemberEqualsIsFixedInOneApplication()
    {
        const string testData = """
                                enum E
                                {
                                    {|#0:A
                                        =
                                        1|},
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config =>
                            {
                                config.NumberOfIncrementalIterations = 1;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All joins two enum members whose <c>=</c> stands below the name in one iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task VerifyFixAllJoinsSeveralEnumMembers()
    {
        const string testData = """
                                enum E
                                {
                                    {|#0:A
                                        = 1|},
                                    {|#1:B
                                        = 2|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                     B = 2
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that in a CRLF document the fix writes the LF behind the last member of an enum, which it joins, as CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesTheDetectedLineEndingBehindTheLastEnumMemberInAMixedDocument()
    {
        const string testData = "enum E\r\n{\r\n    A = 1,\r\n    B =\r\n    2\n}\r\n";
        const string resultData = "enum E\r\n{\r\n    A = 1,\r\n    B = 2\r\n}\r\n";

        Assert.AreEqual(resultData, await ApplyCodeFixAsync(testData));
    }

    /// <summary>
    /// Verifies that the fix removes the whitespace behind the last member of an enum, which it joins, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-PIPE-010")]
    public async Task FixRemovesWhitespaceBehindTheLastEnumMember(string endOfLine)
    {
        const string testData = "enum E\n{\n    A = 1,\n    {|RH5111:B =\n    2|}   \n}\n";
        const string resultData = "enum E\n{\n    A = 1,\n    B = 2\n}\n";

        await VerifyCodeFixWithEndOfLine(testData, resultData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a comment or a <c>#pragma</c> directive in each wrapped gap around the equals sign of an enum member, a property, a declarator, or an assignment keeps the report, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyCommentOrPragmaInEveryWrappedGapKeepsTheReportWithoutCodeFix()
    {
        const string testData = """
                                enum E
                                {
                                    {|RH5111:A // c
                                        = 1|},
                                }

                                class C
                                {
                                    {|RH5111:int Q { get; set; } // c
                                        = 5;|}

                                    void M()
                                    {
                                        var {|RH5111:y // c
                                            = 1|};
                                        int z;
                                        {|RH5111:z = /* c */
                                            2|};
                                        {|RH5111:z
                                #pragma warning disable CS0168
                                            = 3|};
                                    }

                                    void N()
                                    {
                                        int /* t */ {|RH5111:x
                                #pragma warning disable CS0168
                                            = 1|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a region directive pair in the wrapped gap in front of the equals sign of an assignment, a comment on a line of its own behind the equals sign, and a comment behind a parameter in front of its wrapped default value keep the report, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyRegionDirectiveOrCommentInTheWrappedGapKeepsTheReportWithoutCodeFix()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        int z;
                                        {|RH5111:z
                                #region Value
                                #endregion
                                            = 3|};
                                        {|RH5111:z =
                                            // c
                                            4|};
                                    }

                                    void N({|RH5111:int a // c
                                               = 1|})
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the spec example: the fix of a declarator in the body of an assigned lambda joins only the declarator,
    /// while the assignment and the other statement of the lambda stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfDeclaratorInAssignedLambdaLeavesTheAssignmentAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a;
                                        a = () => { var {|RH5111:x
                                            = 1|}; int  y = x; };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a;
                                         a = () => { var x = 1; int  y = x; };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the one-line documentation comment above the property whose initializer the fix joins
    /// stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTheOneLineDocumentationCommentAboveTheProperty(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>P</summary>
                                    {|RH5111:int P { get; set; }
                                        = 5;|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>P</summary>
                                     int P { get; set; } = 5;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a default value of a local function parameter in the body of an assigned lambda joins
    /// only the parameter, while the assignment, the local function, and the other statement of the lambda stay as
    /// written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfParameterDefaultInAssignedLambdaLeavesTheAssignmentAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a;
                                        a = () => { int L({|RH5111:int p
                                            = 1|}) => p; int  y = L(); };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a;
                                         a = () => { int L(int p = 1) => p; int  y = L(); };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a declarator in the body of an assigned anonymous method joins only the declarator,
    /// while the assignment and the other statement stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfDeclaratorInAssignedAnonymousMethodLeavesTheAssignmentAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a;
                                        a = delegate { var {|RH5111:x
                                            = 1|}; int  y = x; };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a;
                                         a = delegate { var x = 1; int  y = x; };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a declarator in the body of a lambda assigned to a member in an object initializer
    /// joins only the declarator, while the member assignment and the other statement stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfDeclaratorInLambdaOfObjectInitializerLeavesTheMemberAssignmentAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    Action X { get; set; }

                                    void M()
                                    {
                                        var c = new C { X = () => { var {|RH5111:x
                                            = 1|}; int  y = x; } };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     Action X { get; set; }

                                     void M()
                                     {
                                         var c = new C { X = () => { var x = 1; int  y = x; } };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of an assignment in the body of an assigned lambda joins only that assignment, while the
    /// outer assignment and the other statement stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfAssignmentInAssignedLambdaJoinsOnlyTheInnerAssignment(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M(int b)
                                    {
                                        Action a;
                                        a = () => { {|RH5111:b
                                            = 1|}; int  y = 2; };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M(int b)
                                     {
                                         Action a;
                                         a = () => { b = 1; int  y = 2; };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a declarator in the body of a lambda that initializes a field joins only the
    /// declarator, while the other statement of the lambda stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfDeclaratorInLambdaOfFieldInitializerLeavesTheLambdaAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    Action _a = () => { var {|RH5111:x
                                        = 1|}; int  y = x; };
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     Action _a = () => { var x = 1; int  y = x; };
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment that leads the assignment statement whose value the fix
    /// joins, where formatting the document would insert one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheAssignment(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        int b = 0;
                                        // c
                                        {|RH5111:b
                                            = 1|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         int b = 0;
                                         // c
                                         b = 1;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a property whose initializer the fix joins, and which shares its line with a field in front of it,
    /// starts on the next line in the column of that line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAPropertyThatSharesItsLineWithAFieldInFrontALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _a; {|RH5111:public int P { get; } =
                                        1;|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     private int _a;
                                     public int P { get; } = 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a property whose initializer the fix joins, and which shares its line with a field behind it, keeps
    /// its own line, and that the field behind it starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAPropertyThatSharesItsLineWithAFieldBehindItALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    {|RH5111:public int P { get; } =
                                        1;|} private int _b;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     public int P { get; } = 1;
                                     private int _b;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix, which formats the property through the formatter, joins the initializer of a property whose
    /// setter carries two <c>param:</c> lists, and that the lists come back merged with one space behind the comma, under LF
    /// and CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixForPropertyWithMergedSetterParamListsKeepsTheCommaSpace()
    {
        const string testData = """
                                class TestClass
                                {
                                    {|#0:int Property { get; [param: A] [param: B] set; }
                                        = 1;|}
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        const string fixedData = """
                                 class TestClass
                                 {
                                     int Property { get; [param: A, B] set; } = 1;
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies the one diagnostic marked in the source and the fix result under LF and under CRLF
    /// </summary>
    /// <param name="testData">The source with LF line endings</param>
    /// <param name="fixedData">The fixed source with LF line endings</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyUnderBothLineEndings(string testData, string fixedData)
    {
        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            int {|#0:first
                                                = 1|}, {|#1:second
                                                = 2|};
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             int first = 1, second = 2;
                                         }
                                     }
                                 }
                                 """;

        // Verifies that Fix All converges for multiple declarators in the same declaration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}