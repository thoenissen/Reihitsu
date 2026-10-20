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
    [Requirement("RH5301-001")]
    [Requirement("RH5301-003")]
    [Requirement("RH5301-004")]
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
    [Requirement("RH5301-002")]
    [Requirement("RH5301-003")]
    [Requirement("RH5301-004")]
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
    [Requirement("RH5301-001")]
    [Requirement("RH5301-003")]
    [Requirement("RH5301-004")]
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
    [Requirement("RH5301-001")]
    [Requirement("RH5301-002")]
    [Requirement("RH5301-003")]
    [Requirement("RH5301-004")]
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
    [Requirement("RH5301-001")]
    [Requirement("RH5301-002")]
    [Requirement("RH5301-003")]
    [Requirement("RH5301-004")]
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
    [Requirement("RH5301-001")]
    [Requirement("RH5301-003")]
    [Requirement("RH5301-004")]
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
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5301-001")]
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
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5301-001")]
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
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5301-002")]
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
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5301-002")]
    [Requirement("RH5301-003")]
    [Requirement("RH5301-004")]
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

    /// <summary>
    /// Verifies that an object initializer on the line of new and one whose braces are not in the column of new are
    /// reported on the whole object creation, while braces in the column of new are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5301-001")]
    [Requirement("RH5301-003")]
    public async Task VerifyBracesOutsideColumnOfNewAreReportedOnCreation()
    {
        const string testCode = """
                                class C
                                {
                                    int P { get; set; }

                                    void M()
                                    {
                                        var a = {|RH5301:new C { P = 1 }|};
                                        C b = {|RH5301:new()
                                            {
                                                P = 1
                                            }|};
                                        var c = new C
                                                {
                                                    P = 1
                                                };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a closing brace one column to the right of new is reported on the whole object creation while the
    /// opening brace is in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5301-001")]
    [Requirement("RH5301-003")]
    public async Task VerifyClosingBraceOneColumnRightOfNewIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    int P { get; set; }

                                    void M()
                                    {
                                        var c = {|RH5301:new C
                                                {
                                                    P = 1
                                                 }|};
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a member outside the column four to the right of new is reported on the member while its braces
    /// are in place, and that the members of an initializer whose brace is reported are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5301-002")]
    [Requirement("RH5301-003")]
    public async Task VerifyMembersOutsideTheirColumnAreReportedOnTheMember()
    {
        const string testCode = """
                                class C
                                {
                                    int P { get; set; }
                                    int Q { get; set; }

                                    void M()
                                    {
                                        var a = new C
                                                {
                                            {|RH5301:P = 1|},
                                                    Q = 2
                                                };
                                        var b = {|RH5301:new C
                                            {
                                            P = 1,
                                               Q = 2
                                            }|};
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix lays out the object creation with its braces in the column of new and every member on its
    /// own line one level deeper, and that a comment behind a member stays behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5301-004")]
    public async Task VerifyFixLaysOutObjectCreationAndKeepsTrailingComment()
    {
        const string testCode = """
                                class C
                                {
                                    int P { get; set; }
                                    int Q { get; set; }

                                    void M()
                                    {
                                        var a = {|RH5301:new C { P = 1, // one
                                            Q = 2 }|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int P { get; set; }
                                     int Q { get; set; }

                                     void M()
                                     {
                                         var a = new C
                                                 {
                                                     P = 1, // one
                                                     Q = 2
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix lays out the object creation with its braces in the column of new and every member on its
    /// own line one level deeper, and that a comment behind a member stays behind it, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5301-004")]
    public async Task VerifyFixLaysOutObjectCreationAndKeepsTrailingCommentWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    int P { get; set; }
                                    int Q { get; set; }

                                    void M()
                                    {
                                        var a = {|RH5301:new C { P = 1, // one
                                            Q = 2 }|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int P { get; set; }
                                     int Q { get; set; }

                                     void M()
                                     {
                                         var a = new C
                                                 {
                                                     P = 1, // one
                                                     Q = 2
                                                 };
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that comments in front of the braces and the members anchor their lines at the expected column, and
    /// that a comment which starts above a closing brace one level deeper lets the brace follow it on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    public async Task VerifyCommentsInFrontOfTokensAnchorTheirLines()
    {
        const string testCode = """
                                class C
                                {
                                    int P { get; set; }
                                    int Q { get; set; }

                                    void M()
                                    {
                                        var c = new C
                                                /* open */ {
                                                    // first
                                                    P = 1,
                                                    /* second */ Q = 2
                                                    // end
                                                };
                                        var d = new C
                                                {
                                                    P = 1
                                                    /* last
                                                       one */ };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a comment which starts the line of a member in the wrong column is reported on the member, and
    /// that a comment which starts above a closing brace in the column of the brace is reported on the object creation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5301-001")]
    [Requirement("RH5301-002")]
    public async Task VerifyCommentsOutsideTheirColumnAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int P { get; set; }

                                    void M()
                                    {
                                        var c = new C
                                                {
                                            /* shifted */ {|RH5301:P = 1|}
                                                };
                                        var d = {|RH5301:new C
                                                {
                                                    P = 1
                                                /* last
                                                   one */ }|};
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a documentation comment whose further line does not start its /// in the column of its first line
    /// is reported on the member behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5301-002")]
    public async Task VerifyDocumentationContinuationOutsideColumnOfFirstLineIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    int P { get; set; }

                                    void M()
                                    {
                                        var c = new C
                                                {
                                                    /// first
                                                  /// second
                                                    {|RH5301:P = 1|}
                                                };
                                    }
                                }
                                """;

        await Verify(testCode);
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