using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5303CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer, RH5303CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that an explicit collection initializer reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForExplicitCollectionInitializer()
    {
        const string testData = """
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
        const string resultData = """
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

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that a target-typed collection initializer reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForTargetTypedCollectionInitializer()
    {
        const string testData = """
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
        const string resultData = """
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

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned expression inside a multi-line complex element reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForComplexElementInitializer()
    {
        const string testData = """
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
        const string resultData = """
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

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned closing brace on a multi-line complex element reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForComplexElementClosingBrace()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         {|#0:{
                                                             existingKey,
                                                             existingValue
                                                             }|}
                                                     };
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  internal class Data;

                                  internal class Example
                                  {
                                      private static void Method(Data existingKey, Data existingValue)
                                      {
                                          var values = new Dictionary<Data, Data>()
                                                       {
                                                           {
                                                               existingKey,
                                                               existingValue
                                                           }
                                                       };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All converges when multiple complex elements contain misaligned expressions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixAllConvergesForMultipleComplexElementDiagnostics()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data firstValue, Data secondValue)
                                    {
                                        var values = new Dictionary<string, Data>()
                                                     {
                                                         {
                                                             "first",
                                                         {|#0:firstValue|}
                                                         },
                                                         {
                                                             "second",
                                                         {|#1:secondValue|}
                                                         }
                                                     };
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  internal class Data;

                                  internal class Example
                                  {
                                      private static void Method(Data firstValue, Data secondValue)
                                      {
                                          var values = new Dictionary<string, Data>()
                                                       {
                                                           {
                                                               "first",
                                                               firstValue
                                                           },
                                                           {
                                                               "second",
                                                               secondValue
                                                           }
                                                       };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a correctly formatted explicit collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectExplicitCollectionInitializer()
    {
        const string testData = """
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

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly formatted target-typed collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectTargetTypedCollectionInitializer()
    {
        const string testData = """
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

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly aligned multi-line complex element does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectComplexElementInitializer()
    {
        const string testData = """
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

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single-line complex element remains valid without interior column checks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineComplexElementInitializer()
    {
        const string testData = """
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

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that object initializers remain owned by RH5301
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForObjectInitializer()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Name = "Test"
                                                    };
                                    }

                                    private string Name { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that collection initializer assignments inside object initializers do not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCollectionInitializerAssignmentInsideObjectInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
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

                                    public List<int> Values { get; set; } = [];
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}