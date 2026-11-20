using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5115IndexAccessMustStayOnExpressionLineAnalyzer"/> and <see cref="RH5115IndexAccessMustStayOnExpressionLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5115IndexAccessMustStayOnExpressionLineAnalyzerTests : BatchCodeFixTestsBase<RH5115IndexAccessMustStayOnExpressionLineAnalyzer, RH5115IndexAccessMustStayOnExpressionLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies the spec example: an element access and a conditional element access below their expression are reported
    /// on their first token, with either gap of a conditional element access broken, behind blank lines, and behind the
    /// last link of a wrapped chain
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    [Requirement("RH5115-003")]
    public async Task SpecExampleIndexAccessesBelowTheirExpressionAreReported()
    {
        const string testData = """
                                #nullable enable
                                using System.Linq;

                                class C
                                {
                                    void M(int[] arr, int[]? n, int[] items)
                                    {
                                        var a = arr
                                            {|RH5115:[|}0];
                                        var b = n
                                            {|RH5115:?|}[0];
                                        var c = n{|RH5115:?|}
                                            [0];
                                        var e = (arr)

                                            {|RH5115:[|}1];
                                        var f = items.Where(x => x > 0)
                                                     .ToArray()
                                                     {|RH5115:[|}0];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the spec example of the other side of each condition: an access on its expression's line, a conditional member
    /// access, an argument list, and an implicit element access of an initializer are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    public async Task SpecExampleOtherSideOfEachConditionIsNotReported()
    {
        const string testData = """
                                #nullable enable
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(int[] arr, string? s)
                                    {
                                        var a = arr[0];
                                        var b = s
                                            ?.Length;
                                        var c = Foo
                                            (1);
                                        var d = new Dictionary<string, int>
                                                {
                                                    ["a"] = 1
                                                };
                                    }

                                    int Foo(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a <c>[</c> that is not the first token of an element access or an element binding is not reported: an
    /// array rank specifier, a collection expression, a list pattern, and an attribute list, each on the line below the token
    /// in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    public async Task BracketsThatStartNoIndexAccessAreNotReported()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = new int
                                            [2];
                                        int[] b =
                                            [1, 2];
                                        var c = arr is
                                            [1, ..];
                                    }

                                    [Obsolete]
                                    void N()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the spec example: an access with an end-of-line comment or a conditional directive in its gap is reported in a
    /// build that does not define the symbol, and no code fix is offered; an access behind a block comment on its expression's
    /// line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-006")]
    public async Task SpecExampleCommentOrDirectiveInTheGapIsReportedWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr // first
                                            {|RH5115:[|}0];
                                        var b = arr
                                #if X
                                            [0];
                                #else
                                            {|RH5115:[|}1];
                                #endif
                                        var c = arr /* first */ [0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a conditional directive in the gap is judged in the build being analyzed: in a build that defines the
    /// symbol, the access of the active branch is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-006")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveInTheGapIsJudgedInTheBuildThatDefinesTheSymbol()
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var b = arr
                                #if X
                                            {|RH5115:[|}0];
                                #else
                                            [1];
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "X");
    }

    /// <summary>
    /// Verifies that an access behind a block comment that ends on the access's line is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-006")]
    public async Task BlockCommentEndingInFrontOfTheAccessIsReportedWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr
                                            /* first */ {|RH5115:[|}0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an access behind a comment on a line of its own in the gap is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-006")]
    public async Task CommentOnALineOfItsOwnInTheGapIsReportedWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr
                                            // first
                                            {|RH5115:[|}0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a conditional element access whose <c>?</c> and <c>[</c> are separated by a line break and an end-of-line
    /// comment is reported on its <c>?</c>, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-003")]
    [Requirement("RH5115-006")]
    public async Task CommentBetweenQuestionMarkAndBracketIsReportedOnTheQuestionMarkWithoutFix()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[]? n)
                                    {
                                        var a = n{|RH5115:?|} // first
                                            [0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a conditional element access whose <c>?</c> stands below an end-of-line comment behind its expression is
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-006")]
    public async Task CommentInFrontOfTheQuestionMarkIsReportedWithoutFix()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[]? n)
                                    {
                                        var a = n // first
                                            {|RH5115:?|}[0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an access behind disabled text in the gap is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-006")]
    public async Task DisabledTextInTheGapIsReportedWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr
                                #if false
                                            [1]
                                #endif
                                            {|RH5115:[|}0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an access behind a <c>#pragma</c> directive in the gap is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-006")]
    public async Task PragmaDirectiveInTheGapIsReportedWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr
                                #pragma warning disable CS0168
                                            {|RH5115:[|}0];
                                #pragma warning restore CS0168
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an access behind a <c>#region</c> and its <c>#endregion</c> in the gap is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-006")]
    public async Task RegionDirectivesInTheGapAreReportedWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr
                                #region Index
                                #endregion
                                            {|RH5115:[|}0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an access behind a <c>///</c> comment in the gap is reported, and no code fix is offered, in every
    /// documentation mode
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow(DocumentationMode.None)]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.Diagnose)]
    [Requirement("RH5115-002")]
    [Requirement("RH5115-006")]
    public async Task DocumentationCommentInTheGapIsReportedWithoutFixInEveryDocumentationMode(DocumentationMode documentationMode)
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr
                                            /// first
                                            {|RH5115:[|}0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, documentationMode);
    }

    /// <summary>
    /// Verifies that a conditional element access with a block comment between its <c>?</c> and its <c>[</c>, or in front of its
    /// <c>?</c>, on its expression's line is not reported, because no line break separates the access from its expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    public async Task ConditionalElementAccessWithCommentOnItsExpressionLineIsNotReported()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[]? n)
                                    {
                                        var a = n? /* first */ [0];
                                        var b = n /* first */ ?[0];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the spec example: two accesses of one expression are reported each on its own, a conditional element access
    /// whose <c>?</c> and <c>[</c> both start a later line is reported once, on its <c>?</c>, and the fix joins every access
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-003")]
    [Requirement("RH5115-004")]
    public async Task SpecExampleEachAccessIsReportedOnItsFirstTokenAndJoined(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[][] grid, int[]? n)
                                    {
                                        var a = grid
                                            {|RH5115:[|}0]
                                            {|RH5115:[|}1];
                                        var b = n
                                            {|RH5115:?|}
                                            [0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(int[][] grid, int[]? n)
                                     {
                                         var a = grid[0][1];
                                         var b = n?[0];
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies the spec example: the fix joins an element access outside a chain with links to its expression
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-004")]
    public async Task SpecExampleFixJoinsAnElementAccess(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr
                                            {|RH5115:[|}0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int[] arr)
                                     {
                                         var a = arr[0];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the fix joins a conditional element access whose <c>[</c> stands below its <c>?</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-004")]
    public async Task SpecExampleFixDeletesTheGapBetweenQuestionMarkAndBracket(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[]? n)
                                    {
                                        var b = n{|RH5115:?|}
                                            [0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(int[]? n)
                                     {
                                         var b = n?[0];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins a conditional element access whose <c>?</c> stands below its expression
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-004")]
    public async Task FixJoinsAConditionalElementAccessWhoseQuestionMarkStandsBelowItsExpression(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[]? n)
                                    {
                                        var b = n
                                            {|RH5115:?|}[0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(int[]? n)
                                     {
                                         var b = n?[0];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix deletes blank lines in the gap together with the line break
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-001")]
    [Requirement("RH5115-004")]
    public async Task FixJoinsAnAccessBehindBlankLines(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var e = (arr)

                                            {|RH5115:[|}1];
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int[] arr)
                                     {
                                         var e = (arr)[1];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: an access inside an argument of a chain with links is not an attached part of that chain, so
    /// the fix only joins it, and the later link that was aligned to code behind it keeps its column
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-004")]
    public async Task SpecExampleFixKeepsTheColumnOfALaterLineAlignedBehindTheAccess(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[] arr, int[]? n)
                                    {
                                        var c = Foo(arr
                                                {|RH5115:[|}0]).ToString()
                                                     .Trim();
                                    }

                                    int Foo(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(int[] arr, int[]? n)
                                     {
                                         var c = Foo(arr[0]).ToString()
                                                      .Trim();
                                     }

                                     int Foo(int x)
                                     {
                                         return x;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the whole spec example of the join outside a chain with links: every access is joined, Fix All included
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-004")]
    public async Task SpecExampleFixJoinsEveryAccessOutsideAChainWithLinks(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[] arr, int[]? n)
                                    {
                                        var a = arr
                                            {|RH5115:[|}0];
                                        var b = n{|RH5115:?|}
                                            [0];
                                        var c = Foo(arr
                                                {|RH5115:[|}0]).ToString()
                                                     .Trim();
                                    }

                                    int Foo(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(int[] arr, int[]? n)
                                     {
                                         var a = arr[0];
                                         var b = n?[0];
                                         var c = Foo(arr[0]).ToString()
                                                      .Trim();
                                     }

                                     int Foo(int x)
                                     {
                                         return x;
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix joins an access behind the closing delimiter of a multi-line raw string literal
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-001")]
    [Requirement("RH5115-004")]
    public async Task FixJoinsAnAccessBehindAMultiLineRawStringLiteral(string endOfLine)
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var t = """
                                                abc
                                                """
                                            {|RH5115:[|}0];
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M()
                                     {
                                         var t = """
                                                 abc
                                                 """[0];
                                     }
                                 }
                                 """";

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins an access of <c>this</c> and of <c>base</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-001")]
    [Requirement("RH5115-004")]
    public async Task FixJoinsAccessesOfThisAndBase(string endOfLine)
    {
        const string testData = """
                                class B
                                {
                                    public int this[int i]
                                    {
                                        get
                                        {
                                            return i;
                                        }
                                    }
                                }

                                class C : B
                                {
                                    int M()
                                    {
                                        var a = this
                                            {|RH5115:[|}0];

                                        return a + base
                                            {|RH5115:[|}1];
                                    }
                                }
                                """;
        const string fixedData = """
                                 class B
                                 {
                                     public int this[int i]
                                     {
                                         get
                                         {
                                             return i;
                                         }
                                     }
                                 }

                                 class C : B
                                 {
                                     int M()
                                     {
                                         var a = this[0];

                                         return a + base[1];
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix joins an access behind another access with a string key
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-001")]
    [Requirement("RH5115-004")]
    public async Task FixJoinsAnAccessBehindAnAccessWithAStringKey(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(Dictionary<string, int[]> d)
                                    {
                                        var a = d["k"]
                                            {|RH5115:[|}0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M(Dictionary<string, int[]> d)
                                     {
                                         var a = d["k"][0];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins an access inside an expression lambda
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-001")]
    [Requirement("RH5115-004")]
    public async Task FixJoinsAnAccessInsideALambda(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        Func<int> g = () => arr
                                            {|RH5115:[|}0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M(int[] arr)
                                     {
                                         Func<int> g = () => arr[0];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins an access inside an interpolation hole with a format
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-001")]
    [Requirement("RH5115-004")]
    public async Task FixJoinsAnAccessInsideAnInterpolationHole(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var h = $"{arr
                                            {|RH5115:[|}0]:D2}";
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int[] arr)
                                     {
                                         var h = $"{arr[0]:D2}";
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a conditional element access behind a null-forgiving operator is reported on its <c>?</c> and joined
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-001")]
    [Requirement("RH5115-003")]
    [Requirement("RH5115-004")]
    public async Task FixJoinsAConditionalElementAccessBehindANullForgivingOperator(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s!
                                            {|RH5115:?|}[0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string? s)
                                     {
                                         var a = s!?[0];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the null-forgiving operator of <c>s⏎!⏎?[0]</c> is not this rule's: only the <c>?</c> is reported, and the
    /// fix joins only its own gap, so the <c>!</c> keeps its line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-003")]
    [Requirement("RH5115-004")]
    public async Task OnlyTheQuestionMarkBehindAWrappedNullForgivingOperatorIsReportedAndJoined(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s
                                            !
                                            {|RH5115:?|}[0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(string? s)
                                     {
                                         var a = s
                                             !?[0];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that an index behind a wrapped null-forgiving operator is not reported by this rule when the index stands on the
    /// operator's line: the line break belongs to the null-forgiving operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    public async Task IndexOnTheLineOfAWrappedNullForgivingOperatorIsNotReported()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(int[]? n)
                                    {
                                        var a = n
                                            ![0];
                                        var b = n
                                            !?[0];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the spec example: the fix lays out the chain with links an access is an attached part of, behind a call and
    /// in front of the first link
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-005")]
    public async Task SpecExampleFixLaysOutTheChainBehindACall(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(dynamic x)
                                    {
                                        var a = x.B()
                                            {|RH5115:[|}0]
                                            .C();
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(dynamic x)
                                     {
                                         var a = x.B()[0]
                                                  .C();
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the fix lays out the chain when the access stands in front of the chain's first link, so every
    /// wrapped link follows the first link that the join moves
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-005")]
    public async Task SpecExampleFixLaysOutTheChainInFrontOfTheFirstLink(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(dynamic x)
                                    {
                                        var e = x
                                            {|RH5115:[|}0].B()
                                            .C();
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(dynamic x)
                                     {
                                         var e = x[0].B()
                                                     .C();
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of an access behind the last link of an aligned chain joins the access onto that link's line and
    /// keeps the chain's alignment
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-005")]
    public async Task FixJoinsAnAccessBehindTheLastLinkOfAnAlignedChain(string endOfLine)
    {
        const string testData = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] items)
                                    {
                                        var f = items.Where(x => x > 0)
                                                     .ToArray()
                                                     {|RH5115:[|}0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Linq;

                                 class C
                                 {
                                     void M(int[] items)
                                     {
                                         var f = items.Where(x => x > 0)
                                                      .ToArray()[0];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the chain fix also joins a wrapped null-forgiving operator of the chain it lays out, so the access and the
    /// operator in front of it end on the chain's first line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-005")]
    public async Task FixLaysOutAChainWhoseAccessStandsBehindAWrappedNullForgivingOperator(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    void M(string s)
                                    {
                                        var i = s
                                            !
                                            {|RH5115:[|}0].ToString()
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
    /// Verifies that two accesses of one chain with links, each below the code in front of it, are both corrected, one fix at a
    /// time and with Fix All, to the one layout of their chain
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-005")]
    public async Task FixAllLaysOutOneChainWithTwoReportedAccesses(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(dynamic x)
                                    {
                                        var a = x.B()
                                            {|RH5115:[|}0]
                                            {|RH5115:[|}1]
                                            .C();
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(dynamic x)
                                     {
                                         var a = x.B()[0][1]
                                                  .C();
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that an access inside an argument of a chain, which is joined, and an access of the chain itself, whose fix lays
    /// out the chain that contains the first access's gap, are both corrected, one fix at a time and with Fix All
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-004")]
    [Requirement("RH5115-005")]
    public async Task FixAllCorrectsAJoinedAccessInsideAChainItLaysOut(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(dynamic x, int[] arr)
                                    {
                                        var b = x.B(arr
                                            {|RH5115:[|}0])
                                            {|RH5115:[|}1]
                                            .C();
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(dynamic x, int[] arr)
                                     {
                                         var b = x.B(arr[0])[1]
                                                  .C();
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that the chain fix is withheld when the chain it would format holds a comment in front of an argument list, a
    /// shape the formatter refuses, although the reported gap holds no comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-006")]
    [Requirement("SHARED-FIX-010")]
    public async Task ChainFixIsWithheldWhenTheChainHoldsAnUnsupportedShape()
    {
        const string testData = """
                                class C
                                {
                                    void M(dynamic x)
                                    {
                                        var a = x.B(Foo /* c */ (1))
                                            {|RH5115:[|}0]
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
    /// Verifies that the join outside a chain with links is offered although the statement holds a shape the formatter refuses
    /// elsewhere, because the join formats nothing
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5115-004")]
    public async Task JoinIsOfferedNextToAnUnsupportedShapeOutsideTheGap(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var v = arr
                                            {|RH5115:[|}0] + Foo /* c */ (1);
                                    }

                                    int Foo(int v)
                                    {
                                        return v;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int[] arr)
                                     {
                                         var v = arr[0] + Foo /* c */ (1);
                                     }

                                     int Foo(int v)
                                     {
                                         return v;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the join of an access that is an element of a collection expression leaves nothing for RH5305 to report,
    /// so the two fixes do not undo each other
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-004")]
    public async Task JoinInsideACollectionExpressionLeavesNothingForRH5305()
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        int[] e = [arr
                                                    {|RH5115:[|}0], 2];
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int[] arr)
                                     {
                                         int[] e = [arr[0], 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, "\n");

        var test = new CSharpAnalyzerVerifierTest<RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer>
                   {
                       TestCode = fixedData,
                       ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                   };

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Verifies that a syntax error in an unrelated member suppresses the rule for the whole file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    [Requirement("SHARED-JOIN-002")]
    public async Task UnrelatedSyntaxErrorSuppressesTheWholeFile()
    {
        const string testData = """
                                class C
                                {
                                    private int _broken = ;

                                    void M(int[] arr)
                                    {
                                        var a = arr
                                            [0];
                                    }
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the same access is reported once the unrelated syntax error is removed, so the suppression above is caused
    /// by the error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    [Requirement("SHARED-JOIN-002")]
    public async Task SameAccessIsReportedWithoutTheUnrelatedSyntaxError()
    {
        const string testData = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr
                                            {|RH5115:[|}0];
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
                                    void M(int[] arr, int[]? n)
                                    {
                                        var a = arr
                                            {|#0:[|}0];
                                        var b = n
                                            {|#1:?|}[0];
                                    }
                                }
                                """;
        const string fixedCode = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(int[] arr, int[]? n)
                                     {
                                         var a = arr[0];
                                         var b = n?[0];
                                     }
                                 }
                                 """;

        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5115IndexAccessMustStayOnExpressionLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}