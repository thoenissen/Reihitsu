using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0399CollectionInitializerShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH0399CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0399CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0399CollectionInitializerShouldBeFormattedCorrectlyAnalyzer, RH0399CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider>
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
                     Diagnostics(RH0399CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0399MessageFormat));
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
                     Diagnostics(RH0399CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0399MessageFormat));
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
    /// Verifies that object initializers remain owned by RH0302
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