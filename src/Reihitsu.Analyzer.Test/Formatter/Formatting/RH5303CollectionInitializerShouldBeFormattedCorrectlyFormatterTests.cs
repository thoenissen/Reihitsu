using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5303CollectionInitializerShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter aligns explicit collection initializer braces and members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesExplicitCollectionInitializerViolation()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values = {|#0:new List<int>
                                     {
                                     1,
                                     2
                                     }|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var values = new List<int>
                                                      {
                                                          1,
                                                          2
                                                      };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns target-typed collection initializer braces and members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesTargetTypedCollectionInitializerViolation()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     List<int> values = {|#0:new()
                                     {
                                     1,
                                     2
                                     }|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         List<int> values = new()
                                                            {
                                                                1,
                                                                2
                                                            };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter fixes complex-element interior alignment and produces analyzer-clean, idempotent output
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesComplexElementInitializerViolation()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Data
                             {
                                 public int A { get; set; }
                             }

                             internal class Example
                             {
                                 private static void Method(Data existingValue)
                                 {
                                     var values = new Dictionary<Data, Data>()
                                                  {
                                                      {
                                                          new Data
                                                          {
                                                              A = 1
                                                          },
                                                      {|#0:existingValue|}
                                                      }
                                                  };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data
                                 {
                                     public int A { get; set; }
                                 }

                                 internal class Example
                                 {
                                     private static void Method(Data existingValue)
                                     {
                                         var values = new Dictionary<Data, Data>()
                                                      {
                                                          {
                                                              new Data
                                                              {
                                                                  A = 1
                                                              },
                                                              existingValue
                                                          }
                                                      };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that an analyzer-clean single-line complex element remains formatter-stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsSingleLineComplexElementStable()
    {
        const string source = """
                              using System.Collections.Generic;

                              internal class Data;

                              internal class Example
                              {
                                  private static void Method(Data existingKey, Data existingValue)
                                  {
                                      var values = new Dictionary<Data, Data>()
                                                   {
                                                       { existingKey, existingValue }
                                                   };
                                  }
                              }
                              """;

        await VerifyFormatter(source);
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
                                  private static void Method(Data existingKey, Data existingValue)
                                  {
                                      var values = new Dictionary<Data, Data>()
                                                   {
                                                       /* Keep element. */ {
                                                                               /* Keep key. */ existingKey,

                                                                               /* Keep value. */ existingValue
                                                                           /* Keep close. */ }
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
                                 private static void Method(Data existingKey, Data existingValue)
                                 {
                                     var values = new Dictionary<Data, Data>()
                                                  {
                                                      /* Keep element. */ {|#0:{
                                                          /* Keep key. */ existingKey,

                                                          /* Keep value. */ existingValue
                                                      /* Keep close. */ }|}
                                                  };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var values = new Dictionary<Data, Data>()
                                                      {
                                                          /* Keep element. */ {
                                                                                  /* Keep key. */ existingKey,

                                                                                  /* Keep value. */ existingValue
                                                                              /* Keep close. */ }
                                                      };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
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
                                 private static void Method(Data existingKey, Data existingValue)
                                 {
                                     var values = new Dictionary<Data, Data>()
                                                  {
                                                      /* Keep
                                                         element. */ {|#0:{
                                                          existingKey,
                                                          existingValue
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
                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var values = new Dictionary<Data, Data>()
                                                      {
                                                          /* Keep
                                                             element. */ {
                                                                             existingKey,
                                                                             existingValue
                                                                         }
                                                      };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns the braces of a collection creation whose <c>new</c> stands behind a comment that leads
    /// its line to <c>new</c>, where the analyzer measures it, and clears the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-001")]
    [Requirement("FMT-INDENT-022")]
    [Requirement("FMT-INDENT-030")]
    public async Task VerifyFormatterAlignsCollectionCreationToNewBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values =

                                     /* c */ {|#0:new List<int>
                                     {
                                         2
                                     }|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var values =

                                         /* c */ new List<int>
                                                 {
                                                     2
                                                 };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that comment-prefixed collection anchors remain analyzer-clean and formatter-stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsCommentPrefixedCollectionStable()
    {
        const string source = """
                              using System.Collections.Generic;

                              internal class Example
                              {
                                  private static void Method()
                                  {
                                      var values = new List<int>()
                                                   /* Keep open. */ {
                                                       /* Keep value. */ 1
                                                   /* Keep close. */ };
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    #endregion // Tests
}