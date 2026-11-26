using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer"/> and <see cref="RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzerTests : BatchCodeFixTestsBase<RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer, RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a trailing comma on the final array initializer item is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
    [Requirement("SHARED-TRAIL-003")]
    [Requirement("RH5410-001")]
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

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a trailing comma on an implicit array initializer is detected and fixed without reformatting surrounding code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
    [Requirement("SHARED-TRAIL-003")]
    [Requirement("RH5410-001")]
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

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a same-line comment after the final array item is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-003")]
    [Requirement("RH5410-001")]
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

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment placed before a comma-only line is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-003")]
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

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that arrays without a trailing comma on the final item are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
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
    [Requirement("SHARED-TRAIL-001")]
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
    /// Verifies that the trailing comma is reported without a code fix when a conditional-compilation block carrying a
    /// further element follows the final active item
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyArrayWithConditionalBlockAfterFinalItemIsReportedWithoutFix()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new[]
                                        {
                                            1{|RH5410:,|}
                                #if SYMBOL
                                            2,
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that, where the symbol is defined, the comma of the final item inside the conditional block, in front of
    /// its <c>#endif</c>, is reported and removed by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyCommaOfFinalItemInsideConditionalBlockIsFixedWhereTheSymbolIsDefined()
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
                                            2{|RH5410:,|}
                                #endif
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
                                 #if SYMBOL
                                             2
                                 #endif
                                         };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test => test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                                                                      ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("SYMBOL"))
                                                                                      : solution));
    }

    /// <summary>
    /// Verifies that the comma written inside the branch, in front of the item that needs it, is not reported in a
    /// build with or without the symbol
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyArrayWithCommaInsideTheConditionalBlockIsNotReportedInEitherBuild()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new[]
                                        {
                                            1
                                #if SYMBOL
                                            , 2
                                #endif
                                        };
                                    }
                                }
                                """;

        await Verify(testData);
        await VerifyWithPreprocessorSymbols(testData, "SYMBOL");
    }

    /// <summary>
    /// Verifies the change's example of an array initializer: the comma in front of the <c>#endif</c> of an active block is
    /// reported and removed by the code fix, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    private int[] _numbers = new[]
                                                             {
                                                                 1,
                                #if !X
                                                                 2{|RH5410:,|}
                                #endif
                                                             };
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     private int[] _numbers = new[]
                                                              {
                                                                  1,
                                 #if !X
                                                                  2
                                 #endif
                                                              };
                                 }
                                 """;

        await Verify(testData, fixedData);
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData));
    }

    /// <summary>
    /// Verifies that the comma of an inner array initializer in front of an <c>#endif</c> and the comma of the outer
    /// initializer are both reported and removed by Fix All
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyInnerCommaInFrontOfEndIfAndOuterCommaAreReportedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    private int[][] _numbers = new[]
                                                               {
                                                                   new[]
                                                                   {
                                                                       1,
                                #if !X
                                                                       2{|RH5410:,|}
                                #endif
                                                                   }{|RH5410:,|}
                                                               };
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     private int[][] _numbers = new[]
                                                                {
                                                                    new[]
                                                                    {
                                                                        1,
                                 #if !X
                                                                        2
                                 #endif
                                                                    }
                                                                };
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that Fix All removes the trailing comma in front of the closing brace and leaves the comma in front of a
    /// conditional block reported and in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyFixAllKeepsOnlyTheCommaInFrontOfAConditionalBlock()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var a = new[]
                                        {
                                            1{|RH5410:,|}
                                #if SYMBOL
                                            2,
                                #endif
                                        };
                                        var b = new[]
                                        {
                                            1,
                                            2{|RH5410:,|}
                                        };
                                        var c = new[]
                                        {
                                            3{|RH5410:,|}
                                        };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var a = new[]
                                         {
                                             1{|RH5410:,|}
                                 #if SYMBOL
                                             2,
                                 #endif
                                         };
                                         var b = new[]
                                         {
                                             1,
                                             2
                                         };
                                         var c = new[]
                                         {
                                             3
                                         };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that the trailing comma is still flagged when a "#region"/"#endregion" pair follows the final
    /// item, since a region directive can never introduce a further list element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-TRAIL-003")]
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

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a trailing comma on a collection initializer is not flagged, since that shape belongs to RH5411
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5410-002")]
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
    [Requirement("RH5410-002")]
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

    /// <summary>
    /// Verifies the specification example of array initializers: the initializer of a field, of an explicit and an
    /// implicit array creation, of each nested initializer of a multi-dimensional array, and of both
    /// <see langword="stackalloc"/> forms is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
    [Requirement("SHARED-TRAIL-003")]
    [Requirement("RH5410-001")]
    public async Task VerifySpecificationExampleOfArrayInitializersIsReportedAndFixed()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    int[] _a = { 1, 2{|#0:,|} };
                                    int[] _b = new int[] { 1{|#1:,|} };
                                    int[] _c = new[] { 1{|#2:,|} };
                                    int[,] _d = { { 1, 2{|#3:,|} }, { 3, 4 }{|#4:,|} };

                                    void M()
                                    {
                                        Span<int> s = stackalloc int[] { 1{|#5:,|} };
                                        Span<int> t = stackalloc[] { 1{|#6:,|} };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     int[] _a = { 1, 2 };
                                     int[] _b = new int[] { 1 };
                                     int[] _c = new[] { 1 };
                                     int[,] _d = { { 1, 2 }, { 3, 4 } };

                                     void M()
                                     {
                                         Span<int> s = stackalloc int[] { 1 };
                                         Span<int> t = stackalloc[] { 1 };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat, 7));
    }

    /// <summary>
    /// Verifies that the trailing comma of an inner and of the outer initializer of one multi-dimensional array are
    /// reported on their own and both removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5410-001")]
    public async Task VerifyInnerAndOuterTrailingCommasOfMultiDimensionalArrayAreReportedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    int[,] _d =
                                    {
                                        { 1, 2{|#0:,|} },
                                        { 3, 4{|#1:,|} }{|#2:,|}
                                    };
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int[,] _d =
                                     {
                                         { 1, 2 },
                                         { 3, 4 }
                                     };
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the specification example of other initializers: a collection initializer, an object initializer, a
    /// collection expression, and an anonymous object creation are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5410-002")]
    public async Task VerifySpecificationExampleOfOtherInitializersIsNotReported()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class D
                                {
                                    public int P { get; set; }
                                }

                                class C
                                {
                                    List<int> _a = new List<int> { 1, 2, };
                                    D _b = new D { P = 1, };
                                    int[] _c = [1, 2,];
                                    object _d = new { A = 1, };
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    private static readonly int[] First = { 1, 2{|#0:,|} };
                                    private static readonly int[] Second = { 3, 4{|#1:,|} };
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     private static readonly int[] First = { 1, 2 };
                                     private static readonly int[] Second = { 3, 4 };
                                 }
                                 """;

        // The rule reports at most one trailing comma per array initializer (its final item), so a shared owner
        // between two occurrences is unreachable; two independent, adjacent field initializers exercise the batch
        // fixer applying two genuinely separate, surgical comma removals
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}