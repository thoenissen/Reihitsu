using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5111AssignmentsMustHaveProperLineBreaksAnalyzer"/>
/// </summary>
[TestClass]
public class RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests : AnalyzerTestsBase<RH5111AssignmentsMustHaveProperLineBreaksAnalyzer, RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for variable declaration with equals on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying diagnostics for property declaration with equals on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying diagnostics for assignment expression with equals on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying diagnostics for member initializer with value on new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying no diagnostics for assignment without initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying that an assignment carrying a comment in the join gap is reported without offering a code fix (issue #226)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}