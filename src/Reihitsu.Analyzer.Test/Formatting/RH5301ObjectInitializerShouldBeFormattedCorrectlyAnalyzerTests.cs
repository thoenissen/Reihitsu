using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5301ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer, RH5301ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for case 1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase1()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH7301
                                    {
                                        public RH7301()
                                        {
                                            var tmp1 = {|#0:new RH7301
                                            {
                                                Test1 = "123"
                                            }|};
                                        }

                                        public string Test1 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH7301
                                      {
                                          public RH7301()
                                          {
                                              var tmp1 = new RH7301
                                                         {
                                                             Test1 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5301MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for case 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase2()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH7301
                                    {
                                        public RH7301()
                                        {
                                            var tmp2 = new RH7301
                                                       {
                                                {|#0:Test1 = "123"|}
                                                       };
                                        }

                                        public string Test1 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH7301
                                      {
                                          public RH7301()
                                          {
                                              var tmp2 = new RH7301
                                                         {
                                                             Test1 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5301MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for case 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase3()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH7301
                                    {
                                        public RH7301()
                                        {
                                            var tmp3 = {|#0:new RH7301
                                                                        {
                                                                            Test1 = "123"
                                                                        }|};
                                        }

                                        public string Test1 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH7301
                                      {
                                          public RH7301()
                                          {
                                              var tmp3 = new RH7301
                                                         {
                                                             Test1 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5301MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for case 4
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase4()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH7301
                                    {
                                        public RH7301()
                                        {
                                            var tmp4 = {|#0:new RH7301
                                                {
                                                Test1 = "123",
                                                   Test2 = "123",
                                                   Test3 = "123",
                                                   Test4 = "123"
                                                }|};
                                        }

                                        public string Test1 { get; set; }
                                        public string Test2 { get; set; }
                                        public string Test3 { get; set; }
                                        public string Test4 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH7301
                                      {
                                          public RH7301()
                                          {
                                              var tmp4 = new RH7301
                                                         {
                                                             Test1 = "123",
                                                             Test2 = "123",
                                                             Test3 = "123",
                                                             Test4 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                          public string Test2 { get; set; }
                                          public string Test3 { get; set; }
                                          public string Test4 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5301MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for case 5
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase5()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH7301
                                    {
                                        public RH7301()
                                        {
                                            var list = new List<string>();

                                            var tmp4 = {|#0:new RH7301
                                                {
                                                Test1 = "123",
                                                   Test2 = "123",
                                                   Test3 = list.Where(s => s == "123")
                                                            .FirstOrDefault(),
                                                   Test4 = "123"
                                                }|};
                                        }

                                        public string Test1 { get; set; }
                                        public string Test2 { get; set; }
                                        public string Test3 { get; set; }
                                        public string Test4 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Linq;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH7301
                                      {
                                          public RH7301()
                                          {
                                              var list = new List<string>();

                                              var tmp4 = new RH7301
                                                         {
                                                             Test1 = "123",
                                                             Test2 = "123",
                                                             Test3 = list.Where(s => s == "123")
                                                                         .FirstOrDefault(),
                                                             Test4 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                          public string Test2 { get; set; }
                                          public string Test3 { get; set; }
                                          public string Test4 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5301MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for target-typed object initializers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase6()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH7301
                                    {
                                        public RH7301()
                                        {
                                            RH7301 tmp6 = {|#0:new()
                                                {
                                                    Test1 = "123"
                                                }|};
                                        }

                                        public string Test1 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH7301
                                      {
                                          public RH7301()
                                          {
                                              RH7301 tmp6 = new()
                                                            {
                                                                Test1 = "123"
                                                            };
                                          }

                                          public string Test1 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5301MessageFormat));
    }

    /// <summary>
    /// Verifies that a formatter-stable same-line comment can prefix the opening brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentPrefixedOpeningBrace()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    /* Keep open. */ {
                                                        Value = 1
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a formatter-stable same-line comment can prefix the closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentPrefixedClosingBrace()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Value = 1
                                                    /* Keep close. */ };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a formatter-stable same-line comment can prefix an object member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentPrefixedAssignment()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        /* Keep value. */ Value = 1
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a shifted same-line comment before an object member reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyShiftedCommentBeforeAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                        /* Keep value. */ {|#0:Value = 1|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public int Value { get; set; }

                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         /* Keep value. */ Value = 1
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5301MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Inner
                                {
                                    public string Name { get; set; }
                                }

                                internal class Container
                                {
                                    public Inner Value { get; set; }
                                }

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var container = {|#0:new Container
                                            {
                                                Value = {|#1:new Inner
                                                    {
                                                        Name = "test"
                                                    }|}
                                            }|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Inner
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal class Container
                                 {
                                     public Inner Value { get; set; }
                                 }

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var container = new Container
                                                         {
                                                             Value = new Inner
                                                                     {
                                                                         Name = "test"
                                                                     }
                                                         };
                                     }
                                 }
                                 """;

        // The outer object creation's fix reformats the whole "new Container { ... }" expression, whose span
        // fully contains the nested "new Inner { ... }" flagged by the second diagnostic, so the batch fixer
        // discards the overlapping inner change while the outer rewrite already re-aligns the nested initializer
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5301MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}