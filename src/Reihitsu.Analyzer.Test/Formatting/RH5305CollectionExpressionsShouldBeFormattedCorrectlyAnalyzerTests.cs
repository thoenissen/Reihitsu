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
    [Requirement("RH5305-001")]
    [Requirement("RH5305-002")]
    [Requirement("RH5305-003")]
    [Requirement("RH5305-004")]
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
    [Requirement("RH5305-001")]
    [Requirement("RH5305-003")]
    [Requirement("RH5305-004")]
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
    [Requirement("RH5305-001")]
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
    [Requirement("RH5305-001")]
    [Requirement("RH5305-002")]
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

    /// <summary>
    /// Verifies that a wrapped collection expression whose closing bracket is not in the column of its opening bracket
    /// is reported on the whole collection expression, while a collection expression on one line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-001")]
    [Requirement("RH5305-003")]
    public async Task VerifyWrappedBracketsOutOfColumnAreReportedOnCollectionExpression()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        int[] a = [1, 2];
                                        int[] b = {|RH5305:[
                                                      1
                                            ]|};
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a wrapped collection expression whose closing bracket is one column to the right of its opening
    /// bracket is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-001")]
    public async Task VerifyWrappedClosingBracketOneColumnRightIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        int[] a = {|RH5305:[
                                                      1
                                                   ]|};
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a wrapped collection expression with an element on the line of a bracket or of an earlier element
    /// is reported on the whole collection expression, while one element per line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-002")]
    [Requirement("RH5305-003")]
    public async Task VerifyWrappedElementsThatShareALineAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] v)
                                    {
                                        int[] a = {|RH5305:[1,
                                                   2]|};
                                        int[] b = {|RH5305:[
                                                      .. v,
                                                      3 ]|};
                                        int[] c = [
                                                      1,
                                                      2
                                                  ];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix puts every element on its own line one level deeper than the opening bracket and the
    /// closing bracket in the column of the opening bracket, and that a comment behind an element stays behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-004")]
    public async Task VerifyFixLaysOutCollectionExpressionAndKeepsTrailingComment()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        int[] a = {|RH5305:[1, // one
                                                   2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                         int[] a = [
                                                       1, // one
                                                       2
                                                   ];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix puts every element on its own line one level deeper than the opening bracket and the
    /// closing bracket in the column of the opening bracket, and that a comment behind an element stays behind it, with
    /// CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-004")]
    public async Task VerifyFixLaysOutCollectionExpressionAndKeepsTrailingCommentWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        int[] a = {|RH5305:[1, // one
                                                   2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                         int[] a = [
                                                       1, // one
                                                       2
                                                   ];
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
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

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that a collection expression whose closing bracket stands behind a comment on the bracket's line is not reported, also when that comment starts its line outside the column of the opening bracket, because the rule exempts a closing bracket with a comment in front of it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-001")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyClosingBracketBehindMisplacedLeadingCommentIsNotReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        int[] b = [
                                                      1
                                        /* end */ ];
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix lays out the collection expression from the column its opening bracket keeps, while the
    /// misspaced declaration in front of it stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5305-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixLeavesTheCodeInFrontOfTheCollectionExpressionAsWritten(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        int[]  a={|RH5305:[1, // one
                                                   2]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         int[]  a=[
                                                      1, // one
                                                      2
                                                  ];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a reported collection expression that is an element of a reported collection expression is laid
    /// out with the outer one, and that Fix All corrects both in one application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-004")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllCorrectsAReportedCollectionExpressionInsideAReportedCollectionExpressionInOneApplication()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        int[][] a = {|RH5305:[{|RH5305:[1,
                                            2]|},
                                            [3]]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         int[][] a = [
                                                         [
                                                             1,
                                                             2
                                                         ],
                                                         [3]
                                                     ];
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, static config => config.NumberOfFixAllIterations = 1);
    }

    /// <summary>
    /// Verifies that the fix of a wrapped collection expression that starts with a <c>with (…)</c> element places the element on
    /// the first line after the opening bracket and removes the space between <c>with</c> and its argument list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-004")]
    public async Task VerifyFixRemovesTheSpaceInFrontOfTheArgumentListOfAWithElement()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        List<string> x = {|#0:[with (4),
                                            "a"]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         List<string> x = [
                                                              with(4),
                                                              "a"
                                                          ];
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5305MessageFormat));
    }

    /// <summary>
    /// Verifies the with element example of RH5305-002: a with element counts as an element, so a wrapped collection
    /// expression whose with element shares a line with another element is reported, and the exploded and the single-
    /// line forms are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-002")]
    public async Task WithElementCountsAsAnElement()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        HashSet<string> a = {|RH5305:[with(StringComparer.Ordinal), "a",
                                                              "b"]|};
                                        HashSet<string> b = [
                                                                with(StringComparer.Ordinal),
                                                                "a",
                                                                "b"
                                                            ];
                                        HashSet<string> c = [with(StringComparer.Ordinal), "a", "b"];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix of a collection expression whose <c>with(…)</c> element's argument list stands on the line below
    /// <c>with</c> joins the argument list onto <c>with</c> and puts every element on a line of its own
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-BREAK-039")]
    public async Task VerifyFixJoinsWithElementArgumentListBelowWith(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        List<int> a = {|RH5305:[with
                                        (4), 1, 2]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         List<int> a = [
                                                           with(4),
                                                           1,
                                                           2
                                                       ];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that fixing two collection expressions in one document, each with a <c>with(…)</c> element whose argument list
    /// stands on the line below <c>with</c>, joins both argument lists
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-BREAK-039")]
    public async Task VerifyFixJoinsWithElementArgumentListsOfTwoCollectionExpressions(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        List<int> a = {|RH5305:[with
                                        (4), 1, 2]|};
                                        HashSet<int> b = {|RH5305:[with
                                        (8), 3, 4]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         List<int> a = [
                                                           with(4),
                                                           1,
                                                           2
                                                       ];
                                         HashSet<int> b = [
                                                              with(8),
                                                              3,
                                                              4
                                                          ];
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
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