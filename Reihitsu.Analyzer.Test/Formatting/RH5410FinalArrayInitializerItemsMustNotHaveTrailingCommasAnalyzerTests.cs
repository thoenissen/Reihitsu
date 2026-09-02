using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer"/> and <see cref="RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzerTests : AnalyzerTestsBase<RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer, RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a trailing comma on the final array initializer item is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnFinalExplicitArrayItemIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static readonly int[] Values = { 1, 2{|#0:,|} };
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static readonly int[] Values = { 1, 2 };
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5410MessageFormat));
    }

    /// <summary>
    /// Verifies that a trailing comma on an implicit array initializer is detected and fixed without reformatting surrounding code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnImplicitArrayInitializerIsRemovedSurgically()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new[] { 1,  2{|#0:,|} };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var values = new[] { 1,  2 };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5410MessageFormat));
    }

    /// <summary>
    /// Verifies that a same-line comment after the final array item is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaWithSameLineCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static readonly int[] Values =
                                    new int[]
                                    {
                                        1,
                                        2{|#0:,|} // Final value
                                    };
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static readonly int[] Values =
                                     new int[]
                                     {
                                         1,
                                         2 // Final value
                                     };
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5410MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment placed before a comma-only line is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnOwnLineAfterCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static readonly int[] Values =
                                    new int[]
                                    {
                                        2
                                        // Final value
                                        {|#0:,|}
                                    };
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static readonly int[] Values =
                                     new int[]
                                     {
                                         2
                                         // Final value
                                     };
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5410MessageFormat));
    }

    /// <summary>
    /// Verifies that arrays without a trailing comma on the final item are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyArrayWithoutTrailingCommaIsNotFlagged()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static readonly int[] Values = { 1, 2 };
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that empty array initializers are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyArrayInitializerIsNotFlagged()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static readonly int[] Values = { };
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the trailing comma is not flagged when a conditional-compilation block carrying a further
    /// element follows the final active item, since the formatter withholds its own removal for the same shape
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyArrayWithConditionalBlockAfterFinalItemIsNotFlagged()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new[]
                                        {
                                            1,
                                #if SYMBOL
                                            2,
                                #endif
                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the trailing comma is still flagged when a "#region"/"#endregion" pair follows the final
    /// item, since a region directive can never introduce a further list element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyArrayWithRegionDirectiveAfterFinalItemIsStillFlagged()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new[]
                                        {
                                            1,
                                            2{|#0:,|}
                                #region Trailer
                                #endregion
                                        };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var values = new[]
                                         {
                                             1,
                                             2
                                 #region Trailer
                                 #endregion
                                         };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5410MessageFormat));
    }

    /// <summary>
    /// Verifies that a trailing comma on a collection initializer is not flagged, since that shape belongs to RH5411
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnCollectionInitializerIsNotFlagged()
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
                                            2,
                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a trailing comma on an object initializer is not flagged, since no trailing-comma rule covers that kind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnObjectInitializerIsNotFlagged()
    {
        const string testData = """
                                internal class Example
                                {
                                    private int First { get; set; }

                                    private int Second { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                        {
                                            First = 1,
                                            Second = 2,
                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}