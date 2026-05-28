using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer, RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a nested collection initializer with the opening brace on the next line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOpeningBraceOnNextLineAfterAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = {|#0:new Example
                                                    {
                                                        Values =
                                                        {
                                                            1,
                                                            2
                                                        }
                                                    }|};
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

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a nested collection initializer with misaligned braces is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedBracesAreDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = {|#0:new Example
                                                    {
                                                        Values = {
                                                            1,
                                                            2
                                                                  }
                                                    }|};
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

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a correctly formatted multiline nested collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectlyFormattedMultilineNestedCollectionInitializer()
    {
        const string testData = """
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

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an object initializer without nested collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForObjectInitializerWithoutNestedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public string Name { get; set; }
                                    public int Count { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Name = "test",
                                                        Count = 5
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly formatted single-line nested collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineNestedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Values = { 1, 2, 3, 4, 5 }
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nested collection initializer within a property initializer does not report for the single-line form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPropertyInitializerWithSingleLineNestedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    public Example Create() => new Example
                                                               {
                                                                   Values = { 1, 2 }
                                                               };
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nested collection initializer with target-typed new does not report for correct formatting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectlyFormattedTargetTypedNestedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        Example value = new()
                                                        {
                                                            Values = {
                                                                         1,
                                                                         2
                                                                     }
                                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multiline nested collection initializer with multiple elements on one line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMultipleElementsOnSameLineInMultilineInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = {|#0:new Example
                                                    {
                                                        Values = {
                                                            1, 2,
                                                            3
                                                                  }
                                                    }|};
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

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    #endregion // Tests
}