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

        await VerifyFormatterFixAndIdempotency(input,
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

        await VerifyFormatterFixAndIdempotency(input,
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

        await VerifyFormatterFixAndIdempotency(input,
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

        await VerifyFormatterStability(source);
    }

    /// <summary>
    /// Verifies that comment-prefixed complex-element tokens remain analyzer-clean and formatter-stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await VerifyFormatterStability(source);
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

        await VerifyFormatterStability(source);
    }

    #endregion // Tests
}