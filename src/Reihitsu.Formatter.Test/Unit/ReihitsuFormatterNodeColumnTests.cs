using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that the node-level entry points <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> and
/// <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> lay out the node they format at its own column, with
/// every line in the column the indentation phase gives it and no line moved after the last phase
/// </summary>
[TestClass]
public class ReihitsuFormatterNodeColumnTests : FormatterTestsBase
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the spec example: an equals-value target whose <c>new</c> stands behind the last line of a multi-line comment
    /// is laid out from <c>new</c> where it stands, as document-level formatting lays it out
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-INDENT-030")]
    public async Task EqualsValueTargetBehindTheLastLineOfAMultiLineCommentIsLaidOutFromNewWhereItStands()
    {
        const string input = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d = /* a\n                   b */ new List<int>\n          {\n            1\n          };\n    }\n}";
        const string expected = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d = /* a\n                   b */ new List<int>\n                        {\n                            1\n                        };\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<EqualsValueClauseSyntax>);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: an equals-value target that holds a multi-line raw string moves the content line and the
    /// closing marker under the opening quotes, as document-level formatting does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-RAW-001")]
    public async Task EqualsValueTargetHoldingRawStringMovesContentUnderTheOpeningQuotes()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var s = \"\"\"\n    a\n    \"\"\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var s = \"\"\"\n                a\n                \"\"\";\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<EqualsValueClauseSyntax>);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a raw string literal formatted as the target on its own, which reaches the indentation phase still attached
    /// to its document because no earlier phase rewrites it, moves its content line and closing marker under its opening quotes,
    /// as document-level formatting does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-RAW-001")]
    public async Task RawStringLiteralTargetMovesContentUnderTheOpeningQuotes()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var s = \"\"\"\n    a\n    \"\"\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var s = \"\"\"\n                a\n                \"\"\";\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<LiteralExpressionSyntax>);
    }

    /// <summary>
    /// Verifies the spec example: the tabs in the leading whitespace of a statement target count as four columns each, so the
    /// statement starts in column 8 and the wrapped argument aligns behind it, while the lines outside the target keep their
    /// tabs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-TAB-001")]
    public async Task StatementTargetIndentedWithTabsCountsEachOfItsTabsAsFourColumns()
    {
        const string input = "class C\n{\n\tvoid M()\n\t{\n\t\tCall(1,\n2);\n\t}\n}";
        const string expected = "class C\n{\n\tvoid M()\n\t{\n        Call(1,\n             2);\n\t}\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<ExpressionStatementSyntax>);
    }

    /// <summary>
    /// Verifies the spec example: two tabs in front of an equals-value target belong to the code in front of it and are kept,
    /// so they count as one column each, <c>new</c> stands in column 10, and the braces are written with ten spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task EqualsValueTargetBehindTabsCountsEachTabInFrontOfItAsOneColumn()
    {
        const string input = "using System.Collections.Generic;\n\nclass C\n{\n\tvoid M()\n\t{\n\t\tvar d = new List<int>\n\t\t  {\n\t\t\t1\n\t\t  };\n\t}\n}";
        const string expected = "using System.Collections.Generic;\n\nclass C\n{\n\tvoid M()\n\t{\n\t\tvar d = new List<int>\n          {\n              1\n          };\n\t}\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<EqualsValueClauseSyntax>);
    }

    /// <summary>
    /// Verifies that a tab inside a comment in front of an equals-value target is kept and counts as one column, so the braces
    /// align with <c>new</c> in its character column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-TAB-003")]
    public async Task EqualsValueTargetBehindTabInsideCommentCountsTheTabAsOneColumn()
    {
        const string input = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d /*\t*/ = new List<int>\n          {\n            1\n          };\n    }\n}";
        const string expected = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d /*\t*/ = new List<int>\n                      {\n                          1\n                      };\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<EqualsValueClauseSyntax>);
    }

    /// <summary>
    /// Verifies that a misindented statement target led by a one-line comment keeps its first token in its column, and the
    /// wrapped argument aligns under the first argument behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task MisindentedStatementTargetLedByCommentKeepsItsFirstTokenAndAlignsTheWrappedArgument()
    {
        const string input = "class C\n{\n    void M()\n    {\n     /* c */ Call(1,\n2);\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n     /* c */ Call(1,\n                  2);\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<ExpressionStatementSyntax>);
    }

    /// <summary>
    /// Verifies that a method target in a class indented with two spaces keeps its first token in column 2, and the lines the
    /// depth places keep their place relative to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task MethodTargetInTwoSpaceClassKeepsItsLinesRelativeToItsFirstToken()
    {
        const string input = "class C\n{\n  void M()\n  {\nCall();\n  }\n}";
        const string expected = "class C\n{\n  void M()\n  {\n      Call();\n  }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<MethodDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the leading comment of a type target that is a direct child of the root is written back as it stands, while
    /// the type is laid out from its first token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-PIPE-010")]
    public async Task TypeTargetKeepsItsLeadingCommentAsItStands()
    {
        const string input = "// c\n  class D\n  {\nint x;\n  }";
        const string expected = "// c\n  class D\n  {\n      int x;\n  }";

        await AssertFormatsTarget(input, expected, SelectSingle<ClassDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that formatting a chain with an equals-value clause as context aligns the wrapped link under the first link of a
    /// chain behind the last line of a multi-line comment, as document-level formatting does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-INDENT-029")]
    public async Task ChainTargetWithContextBehindTheLastLineOfAMultiLineCommentAlignsTheWrappedLink()
    {
        const string input = "class C\n{\n    void M(string s)\n    {\n        var x = /* a\n                   b */ s.Trim()\n.ToUpper();\n    }\n}";
        const string expected = "class C\n{\n    void M(string s)\n    {\n        var x = /* a\n                   b */ s.Trim()\n                         .ToUpper();\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<InvocationExpressionSyntax>().First(),
                                             SelectSingle<EqualsValueClauseSyntax>);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that formatting an argument that starts a later line of its statement, with the statement as context, writes the
    /// argument's line in the column the indentation phase gives it, under the first argument, and aligns the argument's own
    /// wrapped argument behind it, as document-level formatting does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-INDENT-012")]
    public async Task ArgumentTargetOnALaterLineOfItsContextIsWrittenInThePhaseColumnWithTheLinesAlignedToIt()
    {
        const string input = "class C\n{\n    void M()\n    {\n        Call(1,\n                 Other(2,\n                 3));\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        Call(1,\n             Other(2,\n                   3));\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<ArgumentSyntax>().Single(argument => argument.ToString().StartsWith("Other", StringComparison.Ordinal)),
                                             SelectSingle<ExpressionStatementSyntax>);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that formatting a one-line argument that starts a later line of its statement, with the statement as context,
    /// writes the argument's line in the column the indentation phase gives it, under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-INDENT-012")]
    public async Task OneLineArgumentTargetOnALaterLineOfItsContextIsWrittenInThePhaseColumn()
    {
        const string input = "class C\n{\n    void M()\n    {\n        Call(1,\n                 2);\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        Call(1,\n             2);\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<ArgumentSyntax>().Last(),
                                             SelectSingle<ExpressionStatementSyntax>);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an enum member target that starts its line behind two blank lines, one of which the phases would remove,
    /// keeps both blank lines of the document and takes the column the indentation phase gives it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task EnumMemberTargetBehindTwoBlankLinesKeepsBothBlankLinesAndTakesThePhaseColumn()
    {
        const string input = "enum E\n{\n    A,\n\n\n  B,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n\n\n    B,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().Single(member => member.Identifier.Text == "B"),
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind a comment line, above which the phases would add a blank line, gets no blank
    /// line, and the comment and the member take the columns the indentation phase gives them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task EnumMemberTargetBehindCommentLineKeepsTheMissingBlankLineAndTakesThePhaseColumns()
    {
        const string input = "enum E\n{\n    A,\n      // b\n  B,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n    // b\n    B,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().Single(member => member.Identifier.Text == "B"),
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind a conditional directive and a blank line keeps the directive in column 0, the
    /// blank line behind it, and the column the indentation phase gives the member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task EnumMemberTargetBehindConditionalDirectiveAndBlankLineKeepsBoth()
    {
        const string input = "enum E\n{\n    A,\n  #if !DEBUG\n\n      B,\n#endif\n    C\n}";
        const string expected = "enum E\n{\n    A,\n#if !DEBUG\n\n    B,\n#endif\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().Single(member => member.Identifier.Text == "B"),
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind a region directive, around which the phases would add blank lines, gets no
    /// blank line, and the directive and the member take the columns the indentation phase gives them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task EnumMemberTargetBehindRegionDirectiveKeepsTheMissingBlankLinesAndTakesThePhaseColumns()
    {
        const string input = "enum E\n{\n    A,\n  #region R\n  B,\n  #endregion\n    C\n}";
        const string expected = "enum E\n{\n    A,\n    #region R\n    B,\n  #endregion\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().Single(member => member.Identifier.Text == "B"),
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind a documentation comment, above which the phases would add a blank line, gets
    /// no blank line, and every line of the comment and the member take the columns the indentation phase gives them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task EnumMemberTargetBehindDocumentationCommentKeepsTheMissingBlankLineAndTakesThePhaseColumns()
    {
        const string input = "enum E\n{\n    A,\n      /// <summary>\n      /// B\n      /// </summary>\n  B,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n    /// <summary>\n    /// B\n    /// </summary>\n    B,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().Single(member => member.Identifier.Text == "B"),
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind two blank lines and a documentation comment keeps both blank lines, and the
    /// comment and the member take the columns the indentation phase gives them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task EnumMemberTargetBehindTwoBlankLinesAndDocumentationCommentKeepsBothBlankLines()
    {
        const string input = "enum E\n{\n    A,\n\n\n      /// <summary>\n      /// B\n      /// </summary>\n  B,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n\n\n    /// <summary>\n    /// B\n    /// </summary>\n    B,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().Single(member => member.Identifier.Text == "B"),
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind two comments with a blank line between them keeps exactly that blank line,
    /// and both comments and the member take the columns the indentation phase gives them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task EnumMemberTargetBehindSeveralCommentsKeepsTheDocumentsBlankLinesAndTakesThePhaseColumns()
    {
        const string input = "enum E\n{\n    A,\n      // b\n\n  /* c */\n      B,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n    // b\n\n    /* c */\n    B,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().Single(member => member.Identifier.Text == "B"),
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that a parameter target behind two blank lines and a comment keeps both blank lines, and the comment and the
    /// parameter align with the first parameter, where the indentation phase puts them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task ParameterTargetBehindTwoBlankLinesAndCommentKeepsBothBlankLinesAndTakesThePhaseColumns()
    {
        const string input = "class C\n{\n    void M(int a,\n\n\n      // b\n               int b)\n    {\n    }\n}";
        const string expected = "class C\n{\n    void M(int a,\n\n\n           // b\n           int b)\n    {\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<ParameterSyntax>().Last(),
                                             SelectSingle<ParameterListSyntax>);
    }

    /// <summary>
    /// Selects the only node of the given type in the root
    /// </summary>
    /// <typeparam name="TNode">Type of the node</typeparam>
    /// <param name="root">Syntax root</param>
    /// <returns>The node</returns>
    private static SyntaxNode SelectSingle<TNode>(SyntaxNode root)
        where TNode : SyntaxNode
    {
        return root.DescendantNodes().OfType<TNode>().Single();
    }

    /// <summary>
    /// Formats the selected target through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> under LF and CRLF and
    /// asserts the document's text after one pass, and that a second pass leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTarget(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectTarget)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var expected = NormalizeLineEndings(expectedWithLf, endOfLine);
            var firstPass = await Format(NormalizeLineEndings(inputWithLf, endOfLine), selectTarget, null);

            Assert.AreEqual(expected, firstPass, $"Unexpected first pass under {DescribeLineEnding(endOfLine)} line endings.");

            var secondPass = await Format(firstPass, selectTarget, null);

            Assert.AreEqual(expected, secondPass, $"The second pass must not change the output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Formats the selected target through <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> under LF and
    /// CRLF and asserts the document's text after one pass, and that a second pass leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <param name="selectContext">Selects the context node in the document's root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTargetWithContext(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectTarget, Func<SyntaxNode, SyntaxNode> selectContext)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var expected = NormalizeLineEndings(expectedWithLf, endOfLine);
            var firstPass = await Format(NormalizeLineEndings(inputWithLf, endOfLine), selectTarget, selectContext);

            Assert.AreEqual(expected, firstPass, $"Unexpected first pass under {DescribeLineEnding(endOfLine)} line endings.");

            var secondPass = await Format(firstPass, selectTarget, selectContext);

            Assert.AreEqual(expected, secondPass, $"The second pass must not change the output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Formats the selected target of the given text through a node-level entry point
    /// </summary>
    /// <param name="text">Document text</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <param name="selectContext">Selects the context node, or <see langword="null"/> to format without a context</param>
    /// <returns>The document's text after formatting</returns>
    private async Task<string> Format(string text, Func<SyntaxNode, SyntaxNode> selectTarget, Func<SyntaxNode, SyntaxNode> selectContext)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(text));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

            if (root == null)
            {
                Assert.Fail("Expected a syntax root.");
            }

            var target = selectTarget(root);
            var result = selectContext == null
                             ? await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken)
                             : await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, target, selectContext(root), TestContext.CancellationToken);

            return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
        }
    }

    #endregion // Methods
}