using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter moves the nested collection opening brace onto the assignment line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesOpeningBraceOnNextLineAfterAssignment()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 public List<int> Values { get; set; }

                                 private static void Method()
                                 {
                                     var value = new Example
                                                 {
                                                     {|#0:Values =
                                                     {
                                                         1,
                                                         2
                                                     }|}
                                                 };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 
                                 internal class Example
                                 {
                                     public List<int> Values { get; set; }
 
                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      1,
                                                                      2
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter separates multiline nested collection elements onto individual lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMultipleElementsOnSameLineInMultilineInitializer()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 public List<int> Values { get; set; }

                                 private static void Method()
                                 {
                                     var value = new Example
                                                 {
                                                     {|#0:Values = {
                                                         1, 2,
                                                         3
                                                               }|}
                                                 };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 
                                 internal class Example
                                 {
                                     public List<int> Values { get; set; }
 
                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      1,
                                                                      2,
                                                                      3
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter fixes complex-element interior alignment and produces analyzer-clean, idempotent output
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesComplexElementInteriorMisalignment()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Data;

                             internal class Example
                             {
                                 public Dictionary<Data, Data> Values { get; } = [];

                                 private static void Method(Data existingKey, Data existingValue)
                                 {
                                     var value = new Example
                                                 {
                                                     {|#0:Values = {
                                                                  {
                                                                      existingKey,
                                                                  existingValue
                                                                  }
                                                              }|}
                                                 };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<Data, Data> Values { get; } = [];

                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      {
                                                                          existingKey,
                                                                          existingValue
                                                                      }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter fixes a shifted complex-element anchor and produces analyzer-clean, idempotent output
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesShiftedComplexElementAnchor()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Data;

                             internal class Example
                             {
                                 public Dictionary<Data, Data> Values { get; } = [];

                                 private static void Method(Data existingKey, Data existingValue)
                                 {
                                     var value = new Example
                                                 {
                                                     {|#0:Values = {
                                                                      {
                                                                          existingKey,
                                                                          existingValue
                                                                      }
                                                              }|}
                                                 };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<Data, Data> Values { get; } = [];

                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      {
                                                                          existingKey,
                                                                          existingValue
                                                                      }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that comment-prefixed complex-element tokens measured from the opening brace behind its leading comment remain
    /// analyzer-clean and formatter-stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("FMT-INDENT-023")]
    [Requirement("FMT-INDENT-030")]
    public async Task VerifyFormatterKeepsCommentPrefixedComplexElementStable()
    {
        const string source = """
                              using System.Collections.Generic;

                              internal class Data;

                              internal class Example
                              {
                                  public Dictionary<Data, Data> Values { get; } = [];

                                  private static void Method(Data existingKey, Data existingValue)
                                  {
                                      var value = new Example
                                                  {
                                                      Values = {
                                                                   /* Keep element. */ {
                                                                                           /* Keep key. */ existingKey,

                                                                                           /* Keep value. */ existingValue
                                                                                       /* Keep close. */ }
                                                               }
                                                  };
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter moves the expressions and the closing brace of a comment-prefixed complex element from the
    /// comment's column to the brace's column and clears the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("FMT-INDENT-023")]
    [Requirement("FMT-INDENT-030")]
    public async Task VerifyFormatterAlignsCommentPrefixedComplexElementToItsBrace()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Data;

                             internal class Example
                             {
                                 public Dictionary<Data, Data> Values { get; } = [];

                                 private static void Method(Data existingKey, Data existingValue)
                                 {
                                     var value = new Example
                                                 {
                                                     {|#0:Values = {
                                                                  /* Keep element. */ {
                                                                      /* Keep key. */ existingKey,

                                                                      /* Keep value. */ existingValue
                                                                  /* Keep close. */ }
                                                              }|}
                                                 };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<Data, Data> Values { get; } = [];

                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      /* Keep element. */ {
                                                                                              /* Keep key. */ existingKey,

                                                                                              /* Keep value. */ existingValue
                                                                                          /* Keep close. */ }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns a complex element whose brace stands behind the last line of a multi-line comment to
    /// that brace and clears the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("FMT-INDENT-023")]
    [Requirement("FMT-INDENT-030")]
    public async Task VerifyFormatterAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineComment()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Data;

                             internal class Example
                             {
                                 public Dictionary<Data, Data> Values { get; } = [];

                                 private static void Method(Data existingKey, Data existingValue)
                                 {
                                     var value = new Example
                                                 {
                                                     {|#0:Values = {
                                                                  /* Keep
                                                                     element. */ {
                                                                      existingKey,
                                                                      existingValue
                                                                  }
                                                              }|}
                                                 };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<Data, Data> Values { get; } = [];

                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      /* Keep
                                                                         element. */ {
                                                                                         existingKey,
                                                                                         existingValue
                                                                                     }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that comment-prefixed ordinary elements and the closing brace remain analyzer-clean and formatter-stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsCommentPrefixedOrdinaryElementsStable()
    {
        const string source = """
                              using System.Collections.Generic;

                              internal class Example
                              {
                                  public List<int> Values { get; set; }

                                  private static void Method()
                                  {
                                      var value = new Example
                                                  {
                                                      Values = {
                                                                   /* Keep one. */ 1,

                                                                   /* Keep two. */ 2
                                                               /* Keep close. */ }
                                                  };
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter fixes ordinary-element alignment and remains stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesOrdinaryElementMisalignment()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 public List<int> Values { get; set; }

                                 private static void Method()
                                 {
                                     var value = new Example
                                                 {
                                                     {|#0:Values = {
                                                         1,
                                                         2
                                                              }|}
                                                 };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     public List<int> Values { get; set; }

                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      1,
                                                                      2
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that an element of a nested collection initializer behind two comments, the first of which starts its line in the element column, is not reported and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5304-002")]
    public async Task VerifyElementBehindTwoLeadingCommentsInElementColumnIsNotReported()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    List<int> L { get; } = new List<int>();

                                    void M()
                                    {
                                        var c = new C
                                                {
                                                    L = {
                                                            /* a */ /* b */ 1,
                                                            2
                                                        }
                                                };
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that a element of a nested collection initializer below a line of its own that holds two comments, the first of which starts the line outside the expected column is reported, and that the formatter moves the line's first comment into the expected column, so its output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5304-002")]
    public async Task VerifyNestedElementBelowLineOfTwoMisplacedCommentsIsFixed()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 List<int> L { get; } = new List<int>();

                                 void M()
                                 {
                                     var c = new C
                                             {
                                                 {|#0:L = {
                                                 /* a */ /* b */
                                                         1,
                                                         2
                                                     }|}
                                             };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     List<int> L { get; } = new List<int>();

                                     void M()
                                     {
                                         var c = new C
                                                 {
                                                     L = {
                                                             /* a */ /* b */
                                                             1,
                                                             2
                                                         }
                                                 };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a element of a nested collection initializer behind a further comment that starts on an earlier line behind a comment outside the expected column and ends on the element of a nested collection initializer's line is reported, and that the formatter moves the line's first comment into the expected column, so its output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5304-002")]
    public async Task VerifyNestedElementBehindFurtherCommentWhoseLineStartsOutsideExpectedColumnIsFixed()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 List<int> L { get; } = new List<int>();

                                 void M()
                                 {
                                     var c = new C
                                             {
                                                 {|#0:L = {
                                                         1,
                                                 /* a */ /* b
                                                 */ 2
                                                     }|}
                                             };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     List<int> L { get; } = new List<int>();

                                     void M()
                                     {
                                         var c = new C
                                                 {
                                                     L = {
                                                             1,
                                                             /* a */ /* b
                                                     */ 2
                                                         }
                                                 };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    #endregion // Tests
}