using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5305CollectionExpressionsShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer, RH5305CollectionExpressionsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a multiline collection expression with multiple elements on one line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForMultilineCollectionExpressionWithMultipleElementsOnOneLine()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        int[] values = {|#0:[
                                            1, 2,
                                            3
                                                  ]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         int[] values = [
                                                            1,
                                                            2,
                                                            3
                                                        ];
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5305MessageFormat));
    }

    /// <summary>
    /// Verifies that a multiline collection expression with misaligned brackets is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMultilineCollectionExpressionWithMisalignedBrackets()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        int[] values = {|#0:[
                                            1,
                                            2
                                                  ]|};
                                    }
                                }
                                """;

        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         int[] values = [
                                                            1,
                                                            2
                                                        ];
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5305MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line collection expression remains valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineCollectionExpression()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        int[] values = [1, 2, 3];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly formatted multiline collection expression remains valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectlyFormattedMultilineCollectionExpression()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        int[] values = [
                                                           1,
                                                           2
                                                       ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that list patterns are not analyzed by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForListPattern()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is [1, 2, 3];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that list patterns with slice variants are not analyzed by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForListPatternWithSliceVariant()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is [1,
                                                         .. var rest];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that nested list patterns are not analyzed by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForNestedListPattern()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[][] values)
                                    {
                                        return values is [[1,
                                                         2], [3, 4]];
                                    }
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
                                    private static void Method()
                                    {
                                        int[][] values = {|#0:[
                                        [
                                            1,
                                                2
                                        ],
                                            {|#1:[
                                        3,
                                            4
                                        ]|}
                                            ]|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         int[][] values = [
                                                              [
                                                                  1,
                                                                  2
                                                              ],
                                                              [
                                                                  3,
                                                                  4
                                                              ]
                                                          ];
                                     }
                                 }
                                 """;

        // The first inner collection expression is already aligned and stays unreported. Both reported
        // diagnostics - the outer collection expression and the second inner one - resolve to the identical fix
        // target: the enclosing equals-value clause of the "values = ..." declaration, since neither has a
        // closer statement, initializer, or arrow clause of its own. The batch fixer discards the second,
        // overlapping action, and the surviving single fix already re-aligns the outer brackets together with
        // every inner collection expression
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5305MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}