using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5111AssignmentsMustHaveProperLineBreaksAnalyzer"/>
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
                                                  .GetTextAsync(cancellationToken);
        var expected = expectedText.ToString();
        var actual = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods

    #region Tests

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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));

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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat, 2));
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

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));

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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));

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

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<AssignmentExpressionSyntax>().Single().GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a declarator carrying a preprocessor directive in front of its equals sign is reported without
    /// offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    public async Task VerifyDeclaratorWithDirectiveIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void TestMethod()
                                    {
                                        var {|#0:value
                                #if !UNDEFINED_SYMBOL
                                #endif
                                            = 1|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single().GetLocation());

        Assert.IsEmpty(actions);
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Characterizes the gap FMT-PIPE-017 records for a declarator that starts a later line of its declaration: the
    /// declarator's line keeps its column in the document, while the wrapped argument stands in the column the formatter gave
    /// it with the declarator in the formatter's column, and not under the first argument where it ends up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDeclaratorOnALaterLineKeepsItsFirstLineColumnAndLeavesTheWrappedArgumentInTheFormattersLayout()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int a = 1,
                                              {|#0:b =
                                                  Other(1,
                                                        2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int a = 1,
                                               b = Other(1,
                                                   2);
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter that starts a later line of its parameter list joins it and keeps the parameter's
    /// column as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyParameterOnALaterLineIsJoinedInItsColumn()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                               {|#0:int b =
                                               5|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                                int b = 5)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member that starts a later line of its enum joins it and keeps the member's column as
    /// in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberOnALaterLineIsJoinedInItsColumn()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1,
                                      {|#0:B =
                                      2|}
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
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of a declaration indented with tabs keeps the tabs and
    /// spaces in front of the declarator as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyDeclaratorOnALaterLineOfATabIndentedDeclarationKeepsTheWhitespaceInFrontOfIt()
    {
        const string testData = "internal class TestClass\n{\n\tvoid Method()\n\t{\n\t\tint a = 1,\n\t\t    {|#0:b =\n\t\t        2|};\n\t}\n}\n";
        const string fixedData = "internal class TestClass\n{\n\tvoid Method()\n\t{\n\t\tint a = 1,\n\t\t    b = 2;\n\t}\n}\n";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member whose documentation comment is written behind the member in front of it keeps
    /// that line as written, with nothing inserted in front of the comment, and keeps the member's column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberWithDocumentationCommentBehindThePreviousMemberKeepsThatLineAsWritten()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First, /// Doc
                                    {|#0:Second =
                                        2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First, /// Doc
                                     Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter whose documentation comment is written behind the parameter in front of it keeps
    /// that line as written, with nothing inserted in front of the comment, and keeps the parameter's column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyParameterWithDocumentationCommentBehindThePreviousParameterKeepsThatLineAsWritten()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first, /// Doc
                                                {|#0:int second =
                                                    1|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first, /// Doc
                                                 int second = 1)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind two blank lines keeps both blank lines, one of which document-level
    /// formatting would remove, and the member's column as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindTwoBlankLinesKeepsBothBlankLines()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,


                                  {|#0:Second =
                                  2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,


                                   Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind a conditional directive and a blank line keeps the directive, the blank
    /// line, and the columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindConditionalDirectiveKeepsTheDirectiveAndTheBlankLineAsInTheDocument()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                  #if !DEBUG

                                      {|#0:Second =
                                      2|},
                                #endif
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                   #if !DEBUG

                                       Second = 2,
                                 #endif
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind a region directive adds none of the blank lines document-level formatting
    /// puts around the directive, and keeps the directive and the member in their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindRegionDirectiveAddsNoBlankLineAndKeepsTheColumns()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                  #region Values
                                  {|#0:Second =
                                  2|},
                                  #endregion
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                   #region Values
                                   Second = 2,
                                   #endregion
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind a documentation comment adds no blank line above the comment, and keeps
    /// every line of the comment and the member in their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindDocumentationCommentAddsNoBlankLineAndKeepsTheColumns()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                      /// <summary>
                                      /// Second
                                      /// </summary>
                                  {|#0:Second =
                                  2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                       /// <summary>
                                       /// Second
                                       /// </summary>
                                   Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind two comments with a blank line between them keeps exactly that blank
    /// line, and both comments and the member in their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindSeveralCommentsKeepsThemAsInTheDocument()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                      // b

                                  /* c */
                                      {|#0:Second =
                                      2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                       // b

                                   /* c */
                                       Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind two comments with a blank line between them keeps them as in the document
    /// under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindSeveralCommentsKeepsThemAsInTheDocumentWithCrLf()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                      // b

                                  /* c */
                                      {|#0:Second =
                                      2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                       // b

                                   /* c */
                                       Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter behind two blank lines and a comment keeps both blank lines, the comment, and the
    /// columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyParameterBehindTwoBlankLinesAndCommentKeepsThemAsInTheDocument()
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

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator behind a blank line on a later line of its declaration keeps the blank line and
    /// the declarator's column as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyDeclaratorBehindBlankLineKeepsTheBlankLineAndItsColumn()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1,

                                              {|#0:second =
                                              2|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = 1,

                                               second = 2;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix on an assignment inside a conditional access, where the assigned member and the value each stand on their own line behind a member binding, keeps the line break in front
    /// of the assigned member and moves the value onto the line of the operator, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-024")]
    public async Task VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberBehindAMemberBinding()
    {
        const string testData = """
                                class A
                                {
                                    public A B { get; set; }

                                    public A this[int i] => this;

                                    public int C { get; set; }
                                }

                                class C
                                {
                                    void M(A a)
                                    {
                                        a?{|#0:.B
                                          .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B { get; set; }

                                     public A this[int i] => this;

                                     public int C { get; set; }
                                 }

                                 class C
                                 {
                                     void M(A a)
                                     {
                                         a?.B
                                           .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix on an assignment inside a conditional access, where the assigned member and the value each stand on their own line behind an element binding, keeps the line break in front
    /// of the assigned member and moves the value onto the line of the operator, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-024")]
    public async Task VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberBehindAnElementBinding()
    {
        const string testData = """
                                class A
                                {
                                    public A B { get; set; }

                                    public A this[int i] => this;

                                    public int C { get; set; }
                                }

                                class C
                                {
                                    void M(A a)
                                    {
                                        a?{|#0:[0]
                                          .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B { get; set; }

                                     public A this[int i] => this;

                                     public int C { get; set; }
                                 }

                                 class C
                                 {
                                     void M(A a)
                                     {
                                         a?[0]
                                           .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix on an assignment inside a conditional access, where the assigned member and the value each stand on their own line behind a nested conditional access, keeps the line break in front
    /// of the assigned member and moves the value onto the line of the operator, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-024")]
    public async Task VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberInANestedConditionalAccess()
    {
        const string testData = """
                                class A
                                {
                                    public A B { get; set; }

                                    public A this[int i] => this;

                                    public int C { get; set; }
                                }

                                class C
                                {
                                    void M(A a)
                                    {
                                        a?.B?{|#0:.B
                                            .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B { get; set; }

                                     public A this[int i] => this;

                                     public int C { get; set; }
                                 }

                                 class C
                                 {
                                     void M(A a)
                                     {
                                         a?.B?.B
                                              .C = 5;
                                     }
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
        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
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
                                  Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}