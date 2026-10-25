using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for anchors that are the first token of a line that a comment leads: the comment takes the column the line starts
/// in, the anchor is measured behind the comment where it stands, and every line aligned to the anchor follows it, through the
/// whole pipeline over two passes
/// </summary>
[TestClass]
public class CommentLedFirstTokenAnchorTests : FormatterTestsBase
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Initializer braces

    /// <summary>
    /// Verifies the spec example: a comment leads the line of a complex element initializer's opening brace, so the element's
    /// expressions start one level deeper than the brace and its closing brace starts under it, while the next element, whose
    /// comment stands on the line above, keeps its brace at the line's column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-INDENT-023")]
    public void ComplexElementAlignsToItsBraceBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var d = new Dictionary<int, int>
                                             {
                                                 /* e */ {
                                                     1,
                                                     2
                                                 },

                                                 // f
                                                 {
                                                     3,
                                                     4
                                                 }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* e */ {
                                                                1,
                                                                2
                                                            },

                                                    // f
                                                    {
                                                        3,
                                                        4
                                                    }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec sentence on a token behind the last line of a multi-line comment: the complex element's brace is
    /// measured where that line puts it, and the expressions and the closing brace follow the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-INDENT-023")]
    public void ComplexElementAlignsToItsBraceBehindTheLastLineOfAMultiLineComment()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var d = new Dictionary<int, int>
                                             {
                                                 /* a
                                                    b */ {
                                                     1,
                                                     2
                                                 }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* a
                                                       b */ {
                                                                1,
                                                                2
                                                            }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a comment leads the line of a nested initializer's opening brace, takes the column of the
    /// assigned member, and the member and the closing brace follow the brace behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-023")]
    [Requirement("FMT-INDENT-030")]
    public void NestedInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class P
                             {
                                 public int X { get; set; }

                                 public P Child { get; set; } = new P();
                             }

                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 Child =
                                                 /* c */ {
                                                     X = 2
                                                 }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public int X { get; set; }

                                    public P Child { get; set; } = new P();
                                }

                                class C
                                {
                                    void M()
                                    {
                                        var p = new P
                                                {
                                                    Child =
                                                    /* c */ {
                                                                X = 2
                                                            }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the nested initializer's qualifier: a comment on the line above leaves the opening brace at
    /// the start of its line, in the column of the assigned member, and the member and the closing brace follow that column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-023")]
    [Requirement("FMT-INDENT-030")]
    public void NestedInitializerBelowCommentOnTheLineAboveKeepsItsBraceAtTheMemberColumn()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      var p = new P
                                              {
                                                  Child =

                                                  // c
                                                  {
                                                      X = 2
                                                  }
                                              };
                                  }
                              }
                              """;

        AssertRuleResult(source);
    }

    /// <summary>
    /// Verifies the change doc's row for an array initializer: the comment keeps column 8, the opening brace stands behind it
    /// in column 16, and the elements and the closing brace follow the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-023")]
    [Requirement("FMT-INDENT-030")]
    public void ArrayInitializerAlignsToItsBraceBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[] b =
                                     /* c */ {
                                     1,
                                     2
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[] b =

                                        /* c */ {
                                                    1,
                                                    2
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the array initializer's row: a comment on the line above leaves the opening brace at the start
    /// of its line, and the elements and the closing brace follow the line's column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-023")]
    [Requirement("FMT-INDENT-030")]
    public void ArrayInitializerBelowCommentOnTheLineAboveKeepsItsBraceAtTheLineColumn()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      int[] b =

                                      // c
                                      {
                                          1,
                                          2
                                      };
                                  }
                              }
                              """;

        AssertRuleResult(source);
    }

    /// <summary>
    /// Verifies the spec sentence on a token that anchors nothing: the opening brace of a creation initializer behind a comment
    /// that leads its line moves no other line, and the members stay one level deeper than <c>new</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-022")]
    [Requirement("FMT-INDENT-030")]
    public void CreationInitializerBraceBehindCommentThatLeadsTheLineMovesNoOtherLine()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      var p = new P
                                              /* o */ {
                                                  X = 2
                                              };
                                  }
                              }
                              """;

        AssertRuleResult(source);
    }

    #endregion // Initializer braces

    #region Collection expressions and list patterns

    /// <summary>
    /// Verifies the change doc's row for a collection expression: the opening bracket stands behind the comment in column 16,
    /// and the elements and the closing bracket follow it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-025")]
    [Requirement("FMT-INDENT-030")]
    public void CollectionExpressionAlignsToItsBracketBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     List<int> x =
                                     /* c */ [
                                     1,
                                     2
                                     ];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        List<int> x =

                                        /* c */ [
                                                    1,
                                                    2
                                                ];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the collection expression's row: a comment on the line above leaves the opening bracket at the
    /// start of its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-025")]
    [Requirement("FMT-INDENT-030")]
    public void CollectionExpressionBelowCommentOnTheLineAboveKeepsItsBracketAtTheLineColumn()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      List<int> x =

                                      // c
                                      [
                                          1,
                                          2
                                      ];
                                  }
                              }
                              """;

        AssertRuleResult(source);
    }

    /// <summary>
    /// Verifies the change doc's row for a list pattern behind <c>is</c>: the opening bracket stands behind the comment in
    /// column 16, and the subpatterns and the closing bracket follow it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-025")]
    [Requirement("FMT-INDENT-030")]
    public void ListPatternAlignsToItsBracketBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var t = o is
                                     /* c */ [
                                     1,
                                     2
                                     ];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var t = o is

                                        /* c */ [
                                                    1,
                                                    2
                                                ];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Collection expressions and list patterns

    #region Creation expressions

    /// <summary>
    /// Verifies the change doc's row for an object creation: <c>new</c> stands behind the comment in column 16, the braces align
    /// to it, and the member starts one level deeper
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-022")]
    [Requirement("FMT-INDENT-030")]
    public void ObjectCreationAlignsToNewBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p =
                                     /* c */ new P
                                     {
                                     X = 2
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var p =

                                        /* c */ new P
                                                {
                                                    X = 2
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change doc's row for a collection creation: <c>new</c> stands behind the comment in column 16, the braces
    /// align to it, and the element starts one level deeper
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-022")]
    [Requirement("FMT-INDENT-030")]
    public void CollectionCreationAlignsToNewBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p =
                                     /* c */ new List<int>
                                     {
                                     2
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var p =

                                        /* c */ new List<int>
                                                {
                                                    2
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change doc's row for an anonymous object: <c>new</c> stands behind the comment in column 16, the braces align
    /// to it, and the member starts one level deeper
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-024")]
    [Requirement("FMT-INDENT-030")]
    public void AnonymousObjectAlignsToNewBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p =
                                     /* c */ new
                                     {
                                     X = 1
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var p =

                                        /* c */ new
                                                {
                                                    X = 1
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Creation expressions

    #region Other anchors

    /// <summary>
    /// Verifies the change doc's row for a switch expression: its governing expression stands behind the comment in column 16,
    /// the braces align to it, and the arms start one level deeper
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-027")]
    [Requirement("FMT-INDENT-030")]
    public void SwitchExpressionAlignsToGoverningExpressionBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 int M(int v)
                                 {
                                     return
                                     /* c */ v switch
                                     {
                                     1 => 2,
                                     _ => 3
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int v)
                                    {
                                        return

                                        /* c */ v switch
                                                {
                                                    1 => 2,
                                                    _ => 3
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change doc's row for a lambda: its parameter list stands behind the comment in column 16, the braces of the
    /// block body align to it, and the statement starts one level deeper
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    public void LambdaBlockAlignsToLambdaBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Action a =
                                     /* c */ () =>
                                     {
                                     M();
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Action a =

                                        /* c */ () =>
                                                {
                                                    M();
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change doc's row for a wrapped binary expression: the left operand stands behind the comment in column 16,
    /// and the wrapped operator aligns to it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-018")]
    [Requirement("FMT-INDENT-030")]
    public void BinaryOperatorAlignsToLeftOperandBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 bool M(bool a, bool b)
                                 {
                                     return
                                     /* c */ a
                                     && b;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        return

                                        /* c */ a
                                                && b;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change doc's row for a conditional expression: the condition stands behind the comment in column 16, and the
    /// wrapped branches start one level deeper
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-020")]
    [Requirement("FMT-INDENT-030")]
    public void ConditionalBranchesAlignToConditionBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     return
                                     /* c */ a
                                     ? 1
                                     : 2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool a)
                                    {
                                        return

                                        /* c */ a
                                                    ? 1
                                                    : 2;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change doc's row for a method chain: the chain starts behind the comment, and the wrapped link aligns to the
    /// first link, <c>.Trim</c>, in column 17
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    public void ChainLinkAlignsToFirstLinkOfChainThatStartsBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 string M(string s)
                                 {
                                     return
                                     /* c */ s.Trim()
                                     .ToUpper();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    string M(string s)
                                    {
                                        return

                                        /* c */ s.Trim()
                                                 .ToUpper();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change doc's row for a wrapped pattern combinator: the pattern's <c>is</c> stands behind the comment in
    /// column 18, and the wrapped <c>or</c> aligns to it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-019")]
    [Requirement("FMT-INDENT-030")]
    public void PatternCombinatorAlignsToIsBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return
                                     /* c */ o is int
                                     or long;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o)
                                    {
                                        return

                                        /* c */ o is int
                                                  or long;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Other anchors

    #region Node-level entry point

    /// <summary>
    /// Verifies that document-scoped node formatting of a statement and of its equals-value clause measures a complex
    /// element's brace behind the last line of a multi-line comment where that line puts it, and gives the output the
    /// document-level entry point gives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public async Task FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineComment()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var d = new Dictionary<int, int>
                                             {
                                                 /* a
                                                    b */ {
                                                     1,
                                                     2
                                                 }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* a
                                                       b */ {
                                                                1,
                                                                2
                                                            }
                                                };
                                    }
                                }
                                """;

        await AssertFormatNodeInDocument(input, expected);
    }

    /// <summary>
    /// Verifies that document-scoped node formatting of a statement and of its equals-value clause, when the collection
    /// initializer's opening brace must move, measures a complex element's brace behind the last line of a multi-line comment
    /// where that line puts it, and gives the output the document-level entry point gives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public async Task FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineCommentWhenTheInitializerBraceMoves()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var d = new Dictionary<int, int>
                                           {
                                                 /* a
                                                    b */ {
                                                     1,
                                                     2
                                                 }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* a
                                                       b */ {
                                                                1,
                                                                2
                                                            }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);

        await AssertFormatNodeInDocument(input, expected);
    }

    /// <summary>
    /// Verifies the one-line side of the node-level case: document-scoped node formatting of a statement and of its
    /// equals-value clause measures a complex element's brace behind a one-line comment that leads its line, as the
    /// document-level entry point does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public async Task FormatNodeInDocumentAlignsComplexElementToItsBraceBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var d = new Dictionary<int, int>
                                             {
                                                 /* e */ {
                                                     1,
                                                     2
                                                 }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* e */ {
                                                                1,
                                                                2
                                                            }
                                                };
                                    }
                                }
                                """;

        await AssertFormatNodeInDocument(input, expected);
    }

    /// <summary>
    /// Formats the only local declaration statement of the input, and then the equals-value clause of its declarator, through
    /// <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> under LF and CRLF, and asserts the document's text after one and
    /// after two applications of each
    /// </summary>
    /// <param name="input">Input document text</param>
    /// <param name="expected">Expected document text</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatNodeInDocument(string input, string expected)
    {
        foreach (var targetKind in new[] { typeof(LocalDeclarationStatementSyntax), typeof(EqualsValueClauseSyntax) })
        {
            foreach (var endOfLine in _lineEndings)
            {
                var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
                var firstPass = await FormatNodeInDocument(NormalizeLineEndings(input, endOfLine), targetKind);

                Assert.AreEqual(normalizedExpected, firstPass, $"FormatNodeInDocumentAsync of the {targetKind.Name} output mismatch under {DescribeLineEnding(endOfLine)} line endings.");

                var secondPass = await FormatNodeInDocument(firstPass, targetKind);

                Assert.AreEqual(normalizedExpected, secondPass, $"FormatNodeInDocumentAsync of the {targetKind.Name} is not idempotent under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Formats the only node of the given kind in the given text through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>
    /// </summary>
    /// <param name="text">Document text</param>
    /// <param name="targetKind">Syntax type of the node to format</param>
    /// <returns>The document's text after formatting</returns>
    private async Task<string> FormatNodeInDocument(string text, Type targetKind)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(text));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
            var target = root?.DescendantNodes().Single(node => node.GetType() == targetKind);

            if (target == null)
            {
                Assert.Fail($"Expected a {targetKind.Name} in the test document.");
            }

            var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken);

            return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
        }
    }

    #endregion // Node-level entry point
}