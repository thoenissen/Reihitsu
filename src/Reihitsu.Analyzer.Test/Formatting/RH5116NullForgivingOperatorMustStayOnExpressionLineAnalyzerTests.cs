using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer"/> and <see cref="RH5116NullForgivingOperatorMustStayOnExpressionLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzerTests : BatchCodeFixTestsBase<RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer, RH5116NullForgivingOperatorMustStayOnExpressionLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies the spec example: a null-forgiving operator below its expression is reported on its <c>!</c>, in front of a binary
    /// operator, an index, a conditional element access, and behind blank lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-001")]
    [Requirement("RH5116-003")]
    public async Task SpecExampleNullForgivingOperatorsBelowTheirExpressionAreReported()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s, int[]? n)
                                    {
                                        var a = s
                                            {|RH5116:!|} + "";
                                        var b = n
                                            {|RH5116:!|}[0];
                                        var c = n
                                            {|RH5116:!|}?[0];
                                        var d = s

                                            {|RH5116:!|};
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the spec example of the other side of each condition: an operator on its expression's line, an operator that is the
    /// first token of the link operators <c>!.</c> and <c>!?.</c>, and a logical negation are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-001")]
    public async Task SpecExampleOtherSideOfEachConditionIsNotReported()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s, bool f)
                                    {
                                        var a = s! + "";
                                        var b = s
                                            !.Length;
                                        var c = s
                                            !?.Length;
                                        var d = f &&
                                            !f;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a null-forgiving operator in front of an argument list is reported on its <c>!</c>, and that the fix joins
    /// only the operator
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-001")]
    [Requirement("RH5116-004")]
    public async Task FixJoinsAnOperatorInFrontOfAnArgumentList(string endOfLine)
    {
        const string testData = """
                                #nullable enable
                                using System;

                                class C
                                {
                                    int M(Func<int, int>? f)
                                    {
                                        return f
                                            {|RH5116:!|}(1);
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable
                                 using System;

                                 class C
                                 {
                                     int M(Func<int, int>? f)
                                     {
                                         return f!(1);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the null-forgiving operator in front of a wrapped conditional element access is not reported by this rule
    /// when the operator stands on its expression's line: the line break belongs to the conditional element access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-001")]
    public async Task OperatorOnItsExpressionLineInFrontOfAWrappedConditionalElementAccessIsNotReported()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s!
                                            ?[0];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the spec example: an operator with an end-of-line comment in its gap is reported, and no code fix is offered; an
    /// operator behind a block comment on its expression's line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-002")]
    [Requirement("RH5116-006")]
    public async Task SpecExampleCommentInTheGapIsReportedWithoutFix()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s // first
                                            {|RH5116:!|} + "";
                                        var b = s /* first */ ! + "";
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an operator behind a conditional directive in the gap is reported in the build that does not define the
    /// symbol, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-002")]
    [Requirement("RH5116-006")]
    public async Task ConditionalDirectiveInTheGapIsReportedWithoutFix()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                #if X
                                            ! + "a";
                                #else
                                            {|RH5116:!|} + "b";
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a conditional directive in the gap is judged in the build being analyzed: in a build that defines the
    /// symbol, the operator of the active branch is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-002")]
    [Requirement("RH5116-006")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveInTheGapIsJudgedInTheBuildThatDefinesTheSymbol()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                #if X
                                            {|RH5116:!|} + "a";
                                #else
                                            ! + "b";
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "X");
    }

    /// <summary>
    /// Verifies that an operator behind a block comment that ends on the operator's line is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-002")]
    [Requirement("RH5116-006")]
    public async Task BlockCommentEndingInFrontOfTheOperatorIsReportedWithoutFix()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                            /* first */ {|RH5116:!|} + "";
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an operator behind a comment on a line of its own in the gap is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-002")]
    [Requirement("RH5116-006")]
    public async Task CommentOnALineOfItsOwnInTheGapIsReportedWithoutFix()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                            // first
                                            {|RH5116:!|} + "";
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an operator behind disabled text in the gap is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-002")]
    [Requirement("RH5116-006")]
    public async Task DisabledTextInTheGapIsReportedWithoutFix()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                #if false
                                            + "x"
                                #endif
                                            {|RH5116:!|} + "";
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an operator behind a <c>#pragma</c> directive in the gap is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-002")]
    [Requirement("RH5116-006")]
    public async Task PragmaDirectiveInTheGapIsReportedWithoutFix()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                #pragma warning disable CS0168
                                            {|RH5116:!|} + "";
                                #pragma warning restore CS0168
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an operator behind a <c>#region</c> and its <c>#endregion</c> in the gap is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-002")]
    [Requirement("RH5116-006")]
    public async Task RegionDirectivesInTheGapAreReportedWithoutFix()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                #region Operator
                                #endregion
                                            {|RH5116:!|} + "";
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an operator behind a <c>///</c> comment in the gap is reported, and no code fix is offered, in every
    /// documentation mode
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow(DocumentationMode.None)]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.Diagnose)]
    [Requirement("RH5116-002")]
    [Requirement("RH5116-006")]
    public async Task DocumentationCommentInTheGapIsReportedWithoutFixInEveryDocumentationMode(DocumentationMode documentationMode)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                            /// first
                                            {|RH5116:!|} + "";
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, documentationMode);
    }

    /// <summary>
    /// Verifies the spec example: only the <c>!</c> of <c>s⏎!⏎?[0]</c> is this rule's, and the fix joins only the <c>!</c>, so the
    /// conditional element access keeps its line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-003")]
    [Requirement("RH5116-004")]
    public async Task SpecExampleOnlyTheOperatorInFrontOfAWrappedIndexIsReportedAndJoined(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                            {|RH5116:!|}
                                            ?[0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string? s)
                                     {
                                         var a = s!
                                             ?[0];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the fix joins an operator outside a chain with links to its expression
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-004")]
    public async Task SpecExampleFixJoinsTheOperator(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                            {|RH5116:!|} + "";
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string? s)
                                     {
                                         var a = s! + "";
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a later line that was aligned to code behind the operator keeps its column after the join
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-004")]
    public async Task SpecExampleFixKeepsTheColumnOfALaterLineAlignedBehindTheOperator(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var b = s
                                            {|RH5116:!|} + Foo(1,
                                                    2);
                                    }

                                    string Foo(int x, int y)
                                    {
                                        return "";
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string? s)
                                     {
                                         var b = s! + Foo(1,
                                                     2);
                                     }

                                     string Foo(int x, int y)
                                     {
                                         return "";
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the whole spec example of the join outside a chain with links: every operator is joined, Fix All included
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-004")]
    public async Task SpecExampleFixJoinsEveryOperatorOutsideAChainWithLinks(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                            {|RH5116:!|} + "";
                                        var b = s
                                            {|RH5116:!|} + Foo(1,
                                                    2);
                                    }

                                    string Foo(int x, int y)
                                    {
                                        return "";
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string? s)
                                     {
                                         var a = s! + "";
                                         var b = s! + Foo(1,
                                                     2);
                                     }

                                     string Foo(int x, int y)
                                     {
                                         return "";
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix deletes blank lines in the gap together with the line break
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-001")]
    [Requirement("RH5116-004")]
    public async Task FixJoinsAnOperatorBehindBlankLines(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var d = s

                                            {|RH5116:!|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string? s)
                                     {
                                         var d = s!;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins an operator in front of an index and in front of a conditional element access
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-001")]
    [Requirement("RH5116-004")]
    public async Task FixJoinsOperatorsInFrontOfAnIndexAndAConditionalElementAccess(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[]? n)
                                    {
                                        var b = n
                                            {|RH5116:!|}[0];
                                        var c = n
                                            {|RH5116:!|}?[0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(int[]? n)
                                     {
                                         var b = n![0];
                                         var c = n!?[0];
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix joins an operator inside an interpolation hole, an argument, and a lambda
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-001")]
    [Requirement("RH5116-004")]
    public async Task FixJoinsOperatorsInsideAHoleAnArgumentAndALambda(string endOfLine)
    {
        const string testData = """
                                #nullable enable
                                using System;

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var h = $"{s
                                            {|RH5116:!|}}";
                                        var e = new Exception(s
                                            {|RH5116:!|});
                                        Func<string> g = () => s
                                            {|RH5116:!|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable
                                 using System;

                                 class C
                                 {
                                     void M(string? s)
                                     {
                                         var h = $"{s!}";
                                         var e = new Exception(s!);
                                         Func<string> g = () => s!;
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies the spec example: the fix lays out the chain with links an operator is an attached part of
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-005")]
    public async Task SpecExampleFixLaysOutTheChain(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string s)
                                    {
                                        var a = s
                                            {|RH5116:!|}[0].ToString()
                                            .Trim();
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string s)
                                     {
                                         var a = s![0].ToString()
                                                      .Trim();
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the chain fix also joins a wrapped index of the chain it lays out
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5116-005")]
    public async Task FixLaysOutAChainWhoseOperatorStandsInFrontOfAWrappedIndex(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string s)
                                    {
                                        var i = s
                                            {|RH5116:!|}
                                            [0].ToString()
                                            .Trim();
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string s)
                                     {
                                         var i = s![0].ToString()
                                                      .Trim();
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the chain fix is withheld when the chain it would format holds a comment in front of an argument list, a
    /// shape the formatter refuses, although the reported gap holds no comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-006")]
    [Requirement("SHARED-FIX-010")]
    public async Task ChainFixIsWithheldWhenTheChainHoldsAnUnsupportedShape()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(dynamic x)
                                    {
                                        var a = x.B(Foo /* c */ (1))
                                            {|RH5116:!|}[0]
                                            .C();
                                    }

                                    int Foo(int v)
                                    {
                                        return v;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a syntax error in an unrelated member suppresses the rule for the whole file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-001")]
    [Requirement("SHARED-JOIN-002")]
    public async Task UnrelatedSyntaxErrorSuppressesTheWholeFile()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    private int _broken = ;

                                    void M(string? s)
                                    {
                                        var a = s
                                            ! + "";
                                    }
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the same operator is reported once the unrelated syntax error is removed, so the suppression above is caused
    /// by the error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-001")]
    [Requirement("SHARED-JOIN-002")]
    public async Task SameOperatorIsReportedWithoutTheUnrelatedSyntaxError()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                            {|RH5116:!|} + "";
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
                                #nullable enable

                                class C
                                {
                                    void M(string? s, int[]? n)
                                    {
                                        var a = s
                                            {|#0:!|} + "";
                                        var b = n
                                            {|#1:!|}[0];
                                    }
                                }
                                """;
        const string fixedCode = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string? s, int[]? n)
                                     {
                                         var a = s! + "";
                                         var b = n![0];
                                     }
                                 }
                                 """;

        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}