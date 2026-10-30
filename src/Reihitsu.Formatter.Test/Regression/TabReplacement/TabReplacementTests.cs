using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.TabReplacement;

/// <summary>
/// Tests for tabs in whitespace and inside directives through the whole pipeline over two passes, and for the text whose tabs
/// are kept
/// </summary>
[TestClass]
public class TabReplacementTests : FormatterTestsBase
{
    #region Whitespace of the code

    /// <summary>
    /// Verifies the spec example: a tab in front of a trailing comment becomes four spaces regardless of the column it starts in
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void TabsBeforeTrailingCommentsBecomeFourSpacesEach()
    {
        const string input = "class C\n{\n    int z;\t// e\n    int w; \t // f\n}";
        const string expected = "class C\n{\n    int z;    // e\n    int w;      // f\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped chain link aligns, in one pass, to a link that stands behind a tab and a delimited
    /// documentation comment, exactly as it does when the tab is written as four spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    [Requirement("FMT-PIPE-004")]
    public void ChainLinkBehindTabAndDelimitedDocumentationCommentAlignsInOnePass()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      var x = a→/** c */.B()
                              .C();
                                  }
                              }
                              """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a    /** c */.B()
                                                             .C();
                                    }
                                }
                                """;

        AssertRuleResult(source.Replace("→", "\t"), expected);
        AssertRuleResult(source.Replace("→", "    "), expected);
    }

    /// <summary>
    /// Verifies that a wrapped chain link anchored behind the closing marker of a raw string, whose opening quotes stand behind
    /// a tab and a delimited documentation comment, aligns in one pass, exactly as it does when the tab is written as four spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    [Requirement("FMT-PIPE-004")]
    public void ChainLinkBehindRawStringBehindTabAndDelimitedDocumentationCommentAlignsInOnePass()
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var s = x→/** d */.Foo(
                              """
                                  a
                                  """.Trim()
                              .ToUpper());
                                  }
                              }
                              """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var s = x    /** d */.Foo("""
                                                                  a
                                                                  """.Trim()
                                                                     .ToUpper());
                                    }
                                }
                                """";

        AssertRuleResult(source.Replace("→", "\t"), expected);
        AssertRuleResult(source.Replace("→", "    "), expected);
    }

    /// <summary>
    /// Verifies that an argument behind a tab and a delimited documentation comment is formatted as it is when the tab is
    /// written as four spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void ArgumentBehindTabAndDelimitedDocumentationCommentFormatsLikeFourSpaces()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      Call(a,→/** c */ b);
                                  }
                              }
                              """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Call(a,

                                             /** c */ b);
                                    }
                                }
                                """;

        AssertRuleResult(source.Replace("→", "\t"), expected);
        AssertRuleResult(source.Replace("→", "    "), expected);
    }

    /// <summary>
    /// Verifies that a binary operand behind a tab and a delimited documentation comment is formatted as it is when the tab is
    /// written as four spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void BinaryOperandBehindTabAndDelimitedDocumentationCommentFormatsLikeFourSpaces()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      var x = a +→/** c */ b;
                                  }
                              }
                              """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a +

                                        /** c */ b;
                                    }
                                }
                                """;

        AssertRuleResult(source.Replace("→", "\t"), expected);
        AssertRuleResult(source.Replace("→", "    "), expected);
    }

    /// <summary>
    /// Verifies that an initializer element behind a tab and a delimited documentation comment is formatted as it is when the
    /// tab is written as four spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void InitializerElementBehindTabAndDelimitedDocumentationCommentFormatsLikeFourSpaces()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      var x = new[] { a,→/** c */ b };
                                  }
                              }
                              """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new[] { a,

                                                    /** c */ b };
                                    }
                                }
                                """;

        AssertRuleResult(source.Replace("→", "\t"), expected);
        AssertRuleResult(source.Replace("→", "    "), expected);
    }

    /// <summary>
    /// Verifies that a tab in front of a comment the left token carries is collapsed to one space, as four spaces are
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void TabInFrontOfCommentCarriedByTheLeftTokenCollapsesLikeFourSpaces()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      var x = a→/* c */ + b;
                                  }
                              }
                              """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a /* c */ + b;
                                    }
                                }
                                """;

        AssertRuleResult(source.Replace("→", "\t"), expected);
        AssertRuleResult(source.Replace("→", "    "), expected);
    }

    /// <summary>
    /// Verifies that detached-node formatting replaces the tab before it measures the chain link's anchor, so the wrapped link
    /// aligns in one pass as it does through the document-level entry point
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void FormatNodeAlignsChainLinkBehindTabAndDelimitedDocumentationCommentInOnePass()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var x = a\t/** c */.B()\n.C();\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var x = a    /** c */.B()\n                             .C();\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var root = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken)
                                       .GetRoot(TestContext.CancellationToken);

            var actual = ReihitsuFormatter.FormatNode(root, cancellationToken: TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), actual, $"FormatNode output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that document-scoped node formatting replaces the tab inside the target before it measures the chain link's
    /// anchor, so the wrapped link aligns in one pass as it does through the document-level entry point
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public async Task FormatNodeInDocumentAlignsChainLinkBehindTabAndDelimitedDocumentationCommentInOnePass()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var x = a\t/** c */.B()\n.C();\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var x = a    /** c */.B()\n                             .C();\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(input, endOfLine)));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var method = root?.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

                if (method == null)
                {
                    Assert.Fail("Expected a method declaration in the test document.");
                }

                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, method, TestContext.CancellationToken);
                var actual = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), actual, $"FormatNodeInDocumentAsync output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    #endregion // Whitespace of the code

    #region Directives

    /// <summary>
    /// Verifies the spec example: tabs between the tokens of <c>#define</c> and <c>#pragma</c> directives become four spaces each
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-002")]
    public void TabsBetweenDirectiveTokensBecomeFourSpacesEach()
    {
        const string input = "#define\tDEBUG\nclass C\n{\n#pragma warning\tdisable CS1591\n    int x;\n}";
        const string expected = "#define    DEBUG\nclass C\n{\n#pragma warning    disable CS1591\n    int x;\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the four spaces that replace the tab between <c>#region</c> and its description reach the
    /// output when the region phase keeps the line, and one space does when it rewrites the description
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-002")]
    public void TabBetweenRegionKeywordAndDescriptionKeepsFourSpacesUnlessTheRegionLineIsRewritten()
    {
        const string input = "class C\n{\n    #region\tBar\n\n    int y;\n\n    #endregion\n\n    #region\tfoo\n\n    int x;\n\n    #endregion\n}";
        const string expected = "class C\n{\n    #region    Bar\n\n    int y;\n\n    #endregion // Bar\n\n    #region Foo\n\n    int x;\n\n    #endregion // Foo\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a tab inside a region description is kept in the <c>#region</c> line and in the description
    /// the <c>#endregion</c> comment repeats
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-002")]
    public void TabInsideRegionDescriptionIsKeptInBothDirectives()
    {
        const string input = "class C\n{\n    #region\tBaz\tqux\n\n    int z;\n\n    #endregion\n}";
        const string expected = "class C\n{\n    #region    Baz\tqux\n\n    int z;\n\n    #endregion // Baz\tqux\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab inside a region description and inside the message of a <c>#warning</c> directive is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-002")]
    public void TabsInsideRegionDescriptionAndWarningMessageAreKept()
    {
        const string input = "class C\n{\n    #region Fields\tAnd\tMore\n\n    int x;\n\n    #endregion // Fields\tAnd\tMore\n\n#warning keep\tthis\n    int y;\n}";

        AssertRuleResult(input);
    }

    #endregion // Directives

    #region Text that keeps its tabs

    /// <summary>
    /// Verifies that tabs inside character, interpolated, and raw string literals and inside a documentation comment are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-003")]
    public void TabsInsideOtherLiteralsAndDocumentationCommentsAreKept()
    {
        const string input = "class C\n{\n    /// <summary>\n    /// a\tb\n    /// </summary>\n    char c = '\t';\n    string i = $\"x\t{c}\";\n    string r = \"\"\"\n               y\tz\n               \"\"\";\n}";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: a tab between the parts of an XML element in a documentation comment is kept, while a tab
    /// in front of the member becomes whitespace that the member's indentation replaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-003")]
    public void TabInsideDocumentationCommentElementIsKeptAndTabInFrontOfMemberIsReplaced()
    {
        const string input = "class C\n{\n    /// <param\tname=\"x\">b</param>\n\tvoid M(int x)\n    {\n    }\n}";
        const string expected = "class C\n{\n    /// <param\tname=\"x\">b</param>\n    void M(int x)\n    {\n    }\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a hole line of a misaligned interpolated raw string indented with a tab and four spaces
    /// starts in the column of the lines around it once the tab is four spaces, and moves with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-003")]
    public void TabIndentedHoleLineStartsInTheColumnOfItsNeighboursAndMovesWithTheLiteral()
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var name = "x";
                                      var a = $"""
                                  Hello {
                              →    name
                                  }
                                  """;
                                  }
                              }
                              """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                     name
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(source.Replace("→", "\t"), expected);
    }

    #endregion // Text that keeps its tabs
}