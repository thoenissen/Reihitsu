using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Tests the final output of the formatter for attribute lists that the structural transforms split (FMT-STRUCT-028) or
/// merge (FMT-STRUCT-029), and for accessor attribute lists whose shape follows their accessor list (FMT-BREAK-032), under LF
/// and CRLF line endings and for a second pass
/// </summary>
[TestClass]
public class AttributeListStructuralFinalOutputTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the first example of FMT-STRUCT-028: lists of the compilation unit, a method, and a property are split, and
    /// created lists keep their specifier
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void SplitExampleOfTheRequirement()
    {
        const string input = """
                             [assembly: A, B]

                             class C
                             {
                                 [method: A, B] [C] void M() { }

                                 [A, B] int P { get; set; }
                             }
                             """;
        const string expected = """
                                [assembly: A]
                                [assembly: B]

                                class C
                                {
                                    [method: A]
                                    [method: B]
                                    [C]
                                    void M()
                                    {
                                    }

                                    [A]
                                    [B]
                                    int P { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the second example of FMT-STRUCT-028 and the first visible output change of the change: a line break behind
    /// the comma settles in one pass, and a line break in front of the comma or a blank line behind it is dropped
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void LineBreaksBetweenTwoAttributesAreDroppedWithTheComma()
    {
        const string input = """
                             class C
                             {
                                 [A,
                                  B] void M() { }

                                 [A
                                  , B] void N() { }

                                 [A,

                                  B] void O() { }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    [A]
                                    [B]
                                    void M()
                                    {
                                    }

                                    [A]
                                    [B]
                                    void N()
                                    {
                                    }

                                    [A]
                                    [B]
                                    void O()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break behind the comma settles in one pass for a list behind a comment line, a type, and a local
    /// function as well
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void LineBreakBehindTheCommaSettlesInOnePassOnEveryOwner()
    {
        const string input = """
                             [A,
                              B] class C
                             {
                                 // about M
                                 [A,
                                     B] void M()
                                 {
                                     [A,
                                      B] void L() { }
                                 }
                             }
                             """;
        const string expected = """
                                [A]
                                [B]
                                class C
                                {
                                    // about M
                                    [A]
                                    [B]
                                    void M()
                                    {
                                        [A]
                                        [B]
                                        void L()
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace on either side of the comma and inside an attribute's arguments does not stay inside a created
    /// list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void WhitespaceAroundTheCommaIsDropped()
    {
        const string input = """
                             class C
                             {
                                 [A , B] void M() { }

                                 [Obsolete( "x" ) , B( 1 )] void N() { }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    [A]
                                    [B]
                                    void M()
                                    {
                                    }

                                    [Obsolete("x")]
                                    [B(1)]
                                    void N()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a list with a comment between its brackets stays one list, also a single-line and a documentation
    /// comment, because no created list could take the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void ListWithACommentBetweenItsBracketsIsNotSplit()
    {
        const string input = """
                             class C
                             {
                                 [A /* c */, B]
                                 void M()
                                 {
                                 }

                                 [A, // c
                                  B]
                                 void N()
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a list with a preprocessor directive between its brackets stays one list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void ListWithADirectiveBetweenItsBracketsIsNotSplit()
    {
        const string input = """
                             class C
                             {
                                 [A,
                             #if DEBUG
                                  B,
                             #endif
                                  C]
                                 void M()
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a list whose specifier names no target the compiler knows stays one list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void ListWithAnUnknownSpecifierIsNotSplit()
    {
        const string input = """
                             class C
                             {
                                 [unknown: A, B]
                                 void M()
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a list with a comment in front of it, a documentation comment in front of it, or a comment behind it is
    /// split, and the comments keep their place
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void ListWithACommentAroundItIsSplit()
    {
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// M
                                 /// </summary>
                                 [A, B] void M() { }

                                 // about N
                                 [A, B] // tail
                                 void N() { }

                                 /* lead */ [A, B] void O() { }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// M
                                    /// </summary>
                                    [A]
                                    [B]
                                    void M()
                                    {
                                    }

                                    // about N
                                    [A]
                                    [B] // tail
                                    void N()
                                    {
                                    }

                                    /* lead */ [A]
                                    [B]
                                    void O()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a list led by a <c>#region</c> or an active <c>#if</c> directive, a list inside an active <c>#if</c>
    /// block, and a list in front of an active <c>#if</c> directive are split, because no directive lies between the list's
    /// brackets
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void ListNextToADirectiveOutsideItsBracketsIsSplit()
    {
        const string input = """
                             class C
                             {
                                 #region R

                                 [A, B]
                                 void O()
                                 {
                                 }

                                 #endregion // R

                             #if true
                                 [A, B]
                             #endif
                                 void M()
                                 {
                                 }

                             #if true
                                 [A, B]
                                 void N()
                                 {
                                 }
                             #endif

                                 [A, B]
                             #if true
                                 void P()
                                 {
                                 }
                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region R

                                    [A]
                                    [B]
                                    void O()
                                    {
                                    }

                                    #endregion // R

                                #if true
                                    [A]
                                    [B]
                                #endif
                                    void M()
                                    {
                                    }

                                #if true
                                    [A]
                                    [B]
                                    void N()
                                    {
                                    }
                                #endif

                                    [A]
                                    [B]
                                #if true
                                    void P()
                                    {
                                    }
                                #endif
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the split of a field's list and the split of the field into one declaration per declarator give every
    /// declaration its own lists
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void SplitListOfAFieldWithSeveralDeclarators()
    {
        const string input = """
                             class C
                             {
                                 [A, B] int a, b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    [A]
                                    [B]
                                    int a;
                                    [A]
                                    [B]
                                    int b;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break in front of the original <c>]</c>, which does not separate two attributes, keeps the output
    /// of the formatter as it was before the split moved
    /// </summary>
    [TestMethod]
    public void LineBreakInFrontOfTheClosingBracketKeepsItsOutput()
    {
        const string input = """
                             class C
                             {
                                 [A, B
                                 ] void M() { }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    [A]
                                    [B
                                     ]
                                    void M()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the first example of FMT-STRUCT-029: the lists of each target of a parameter and a type parameter are merged,
    /// a comment between the lists blocks the merge, and a <c>type:</c> list is not merged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void MergeExampleOfTheRequirement()
    {
        const string input = """
                             record R([A] [property: B] [C] [property: D] int Id);

                             class C<[A] [B] T>
                             {
                                 void M([A] [B] int a, [A] /* c */ [B] int b, [param: A] [B] int c, [type: A] [B] int d) { }
                             }
                             """;
        const string expected = """
                                record R([A, C] [property: B, D] int Id);

                                class C<[A, B] T>
                                {
                                    void M([A, B] int a, [A] /* c */ [B] int b, [param: A, B] int c, [type: A] [B] int d)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the second example of FMT-STRUCT-029: a line break in the stretch the merge rewrites still wraps the parameter
    /// list, and without one the list stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void LineBreakBetweenMergedListsStillWrapsTheParameterList()
    {
        const string input = """
                             class C
                             {
                                 void P([A]
                                        [B] int x, int y) { }

                                 void Q([A] [B]
                                        int x, int y) { }

                                 void R([A] [B] int x, int y) { }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void P([A, B] int x,
                                           int y)
                                    {
                                    }

                                    void Q([A, B] int x,
                                           int y)
                                    {
                                    }

                                    void R([A, B] int x, int y)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break between the merged lists of a record parameter, also one behind a list of another target,
    /// wraps the record's parameter list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void LineBreakBetweenMergedListsWrapsARecordParameterList()
    {
        const string input = """
                             record R([A]
                                      [B] int X, int Y);

                             record R2([A]
                                       [property: B] [C] int X, int Y);
                             """;
        const string expected = """
                                record R([A, B] int X,
                                         int Y);

                                record R2([A, C] [property: B] int X,
                                          int Y);
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break between the merged lists of a local function parameter wraps its parameter list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void LineBreakBetweenMergedListsWrapsALocalFunctionParameterList()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     void L([A]
                                            [B] int x, int y) { }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        void L([A, B] int x,
                                               int y)
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break between the merged lists of a lambda parameter inside an argument list wraps the lambda's
    /// parameter list and the argument list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void LineBreakBetweenMergedListsWrapsALambdaParameterList()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Use(([A]
                                          [B] int x, int y) => x, 1);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Use(([A, B] int x,
                                             int y) => x,
                                            1);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the attribute lists of a lambda keep the output the formatter gave them before the split moved, and that
    /// a second pass leaves it unchanged; no requirement specifies the attribute lists of a lambda, so the phase-level check of
    /// <see cref="FormatterTestsBase.AssertRuleResult(string, string, Microsoft.CodeAnalysis.CSharp.CSharpParseOptions)"/>,
    /// which reports the line-break phase run alone on this output, is not applied
    /// </summary>
    [TestMethod]
    public void LambdaAttributeListKeepsItsOutput()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var f = [A, B] (int x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var f = [A]
                                        [B]
                                        (int x) => x;
                                    }
                                }
                                """;

        foreach (var endOfLine in _lineEndings)
        {
            var firstPass = FormatOnce(input, endOfLine);

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), firstPass, $"Under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.AreEqual(firstPass, FormatOnce(firstPass, endOfLine), $"Second pass under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a line break between the merged lists of an indexer parameter and of a type parameter is joined with the
    /// rest of their bracketed lists
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void LineBreakBetweenMergedListsOfBracketedListsIsJoined()
    {
        const string input = """
                             class G<[A]
                                     [B] T, U>
                             {
                                 int this[[A]
                                          [B] int i, int j] => i;
                             }
                             """;
        const string expected = """
                                class G<[A, B] T, U>
                                {
                                    int this[[A, B] int i, int j] => i;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the lists of a single parameter written over several lines, a blank line included, are merged onto the
    /// parameter's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void MergedListsOfASingleParameterAreJoinedOntoItsLine()
    {
        const string input = """
                             class C
                             {
                                 void Q([A]
                                        [B] int x) { }

                                 void S([First]
                                        [Second]

                                        int id) { }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void Q([A, B] int x)
                                    {
                                    }

                                    void S([First, Second] int id)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of the first list of a parameter does not block the merge
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void CommentInFrontOfTheFirstListDoesNotBlockTheMerge()
    {
        const string input = """
                             class C
                             {
                                 void M(/* c */ [A] [B] int a)
                                 {
                                 }
                             }
                             """;

        AssertSecondPassIsStable(input);

        foreach (var endOfLine in _lineEndings)
        {
            var output = FormatOnce(input, endOfLine);

            Assert.Contains("[A, B] int a", output, $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
            Assert.Contains("/* c */", output, $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
        }
    }

    /// <summary>
    /// Verifies that a comment that leads the parameter's type behind its lists blocks the merge
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void CommentLeadingTheTokenBehindTheListsBlocksTheMerge()
    {
        const string input = """
                             class C
                             {
                                 void M([A] [B] /* c */ int a)
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a directive that leads the first list of a parameter blocks the merge
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void DirectiveLeadingTheFirstListBlocksTheMerge()
    {
        const string input = """
                             class C
                             {
                                 void M(int x,
                             #pragma warning disable CS0618
                                        [A] [B] int a)
                                 {
                                 }
                             }
                             """;

        AssertSecondPassIsStable(input);

        foreach (var endOfLine in _lineEndings)
        {
            var output = FormatOnce(input, endOfLine);

            Assert.Contains("[A] [B] int a", output, $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
        }
    }

    /// <summary>
    /// Verifies that a documentation comment between the lists of a parameter blocks the merge
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void DocumentationCommentBetweenTheListsBlocksTheMerge()
    {
        const string input = """
                             class C
                             {
                                 void M([A]
                                        /// <summary>x</summary>
                                        [B] int p)
                                 {
                                 }
                             }
                             """;

        AssertSecondPassIsStable(input);

        foreach (var endOfLine in _lineEndings)
        {
            var output = FormatOnce(input, endOfLine);

            Assert.DoesNotContain("[A, B]", output, $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
        }
    }

    /// <summary>
    /// Verifies the example of FMT-BREAK-032: accessor lists on the declaration's line are merged and not split, accessor
    /// lists laid out over several lines are split, also when the layout differs from the one the author wrote
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-032")]
    public void AccessorExampleOfTheRequirement()
    {
        const string input = """
                             class C
                             {
                                 int P { [A] [B] get; [C, D] set; }

                                 int Q
                                 {
                                     [A, B] get;
                                     [C] [D] set;
                                 }

                                 int R { [A, B] get => 1; }

                                 int S { [A] [B] get; set; } =
                                     1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P { [A, B] get; [C, D] set; }

                                    int Q
                                    {
                                        [A]
                                        [B]
                                        get;
                                        [C]
                                        [D]
                                        set;
                                    }

                                    int R
                                    {
                                        [A]
                                        [B]
                                        get => 1;
                                    }

                                    int S { [A, B] get; set; } = 1;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the visible output changes for an accessor: the lists created for an accessor in an accessor list laid out over
    /// several lines hold none of the line breaks that separated two attributes, and settle in one pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-032")]
    public void AccessorSplitDropsTheLineBreaksBetweenTwoAttributes()
    {
        const string input = """
                             class C
                             {
                                 int G
                                 {
                                     [C,
                                      D] get;
                                     set;
                                 }

                                 int H
                                 {
                                     [C
                                      , D] get => 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int G
                                    {
                                        [C]
                                        [D]
                                        get;
                                        set;
                                    }

                                    int H
                                    {
                                        [C]
                                        [D]
                                        get => 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the accessor lists of an indexer follow its accessor list's layout
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-032")]
    public void IndexerAccessorListsFollowTheAccessorListLayout()
    {
        const string input = """
                             class C
                             {
                                 int this[int i] { [A] [B] get; set; }

                                 int this[long i] { [A, B] get => 0; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int i] { [A, B] get; set; }

                                    int this[long i]
                                    {
                                        [A]
                                        [B]
                                        get => 0;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>, which a code fix calls on the member it
    /// rewrites, splits the member's attribute list and merges its parameter's lists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-016")]
    public async Task FormatNodeInDocumentAsyncSplitsAndMergesAttributeLists()
    {
        const string input = """
                             class C
                             {
                                 [A, B] void M([A] [B] int x) { }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    [A]
                                    [B]
                                    void M([A, B] int x)
                                    {
                                    }
                                }
                                """;

        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = workspace.AddProject("TestProject", LanguageNames.CSharp).AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(input, endOfLine)));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var method = root?.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

                Assert.IsNotNull(method);

                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, method, TestContext.CancellationToken);
                var text = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), text, $"Under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Formats the input once with the given line endings
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    /// <param name="endOfLine">The line ending to format with</param>
    /// <returns>The formatted text</returns>
    private string FormatOnce(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);

        return ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).GetRoot(TestContext.CancellationToken).ToFullString();
    }

    #endregion // Methods
}