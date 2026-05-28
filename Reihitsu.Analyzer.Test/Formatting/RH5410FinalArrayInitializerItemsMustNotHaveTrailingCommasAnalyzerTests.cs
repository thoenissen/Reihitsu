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

    #endregion // Tests
}