using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Cleanup;

/// <summary>
/// Tests for the whitespace that ends a file, with and without a final line break. The fixtures are explicit strings, so the
/// whitespace at the end of the text and the missing final line break are part of the input as written
/// </summary>
[TestClass]
public class EndOfFileWhitespaceTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: three spaces behind the last token of a file without a final line break are removed, and the
    /// file ends with the last token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void SpacesBehindTheLastTokenWithoutFinalLineBreakAreRemoved()
    {
        const string input = "class C\n{\n    int x;\n}   ";
        const string expected = "class C\n{\n    int x;\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example's prose: a tab behind the last token of a file without a final line break, which the preparation
    /// phase replaces with four spaces, is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void TabBehindTheLastTokenWithoutFinalLineBreakIsRemoved()
    {
        const string input = "class C\n{\n    int x;\n}\t";
        const string expected = "class C\n{\n    int x;\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the spaces behind the last token of a file without a final line break are removed while the byte order mark
    /// at the start of the text stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    [Requirement("FMT-CLEAN-009")]
    public void SpacesBehindTheLastTokenOfAFileWithAByteOrderMarkAreRemoved()
    {
        const string input = "﻿class C\n{\n    int x;\n}   ";
        const string expected = "﻿class C\n{\n    int x;\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example's prose: the spaces behind a block comment that follows the last token on its line are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void SpacesBehindABlockCommentOnTheLastTokensLineAreRemoved()
    {
        const string input = "class C\n{\n    int x;\n} /* c */   ";
        const string expected = "class C\n{\n    int x;\n} /* c */";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example's prose: the spaces behind a block comment on a line of its own after the last token are removed,
    /// and the blank line in front of the comment stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void SpacesBehindABlockCommentOnItsOwnLineAreRemoved()
    {
        const string input = "class C\n{\n    int x;\n}\n\n/* end */   ";
        const string expected = "class C\n{\n    int x;\n}\n\n/* end */";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the spaces behind the second of two block comments on lines of their own after the last token are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void SpacesBehindTheLastOfTwoBlockCommentsOnTheirOwnLinesAreRemoved()
    {
        const string input = "class C\n{\n    int x;\n}\n\n/* a */\n/* b */   ";
        const string expected = "class C\n{\n    int x;\n}\n\n/* a */\n/* b */";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example's prose: a file that holds nothing but a block comment followed by three spaces ends with the
    /// comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void SpacesBehindTheOnlyCommentOfAFileAreRemoved()
    {
        const string input = "/* only */   ";
        const string expected = "/* only */";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the requirement's normative sentence: a file without a final line break gives the same output as the same file
    /// with one, formatted through <see cref="ReihitsuFormatter.FormatSyntaxTree"/>, and the output ends without whitespace
    /// </summary>
    /// <param name="input">The input, without a final line break</param>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    [DataRow("class C\n{\n    int x;\n}   ")]
    [DataRow("class C\n{\n    int x;\n}\t")]
    [DataRow("class C\n{\n    int x;\n} /* c */   ")]
    [DataRow("class C\n{\n    int x;\n}  /* a */  /* b */  ")]
    [DataRow("class C\n{\n    int x;\n}\n\n/* end */   ")]
    [DataRow("class C\n{\n    int x;\n}\n\n/* a */\n/* b */   ")]
    [DataRow("/* only */   ")]
    public void OutputWithoutFinalLineBreakEqualsOutputWithFinalLineBreak(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var withoutLineBreak = FormatTree(NormalizeLineEndings(input, endOfLine));
            var withLineBreak = FormatTree(NormalizeLineEndings(input, endOfLine) + endOfLine);

            Assert.AreEqual(Escape(withLineBreak), Escape(withoutLineBreak), $"The output must not depend on the final line break under {endingName} line endings.");
            Assert.AreEqual(withoutLineBreak.TrimEnd(), withoutLineBreak, $"The output must end without whitespace under {endingName} line endings.");
        }

        AssertSecondPassIsStable(input);
    }

    /// <summary>
    /// Verifies that whole-document formatting, the path of the command-line tool, removes the spaces behind the last token of a
    /// document without a final line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public async Task FormatDocumentRemovesTheSpacesBehindTheLastToken()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = NormalizeLineEndings("class C\n{\n    int x;\n}   ", endOfLine);
            var expected = NormalizeLineEndings("class C\n{\n    int x;\n}", endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = workspace.AddProject("TestProject", LanguageNames.CSharp).AddDocument("Test.cs", SourceText.From(input));

                var result = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken);
                var actual = (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(Escape(expected), Escape(actual), $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies that formatting the compilation unit with <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>, which formats
    /// the whole file, removes the spaces behind the last token of a document without a final line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public async Task FormatNodeInDocumentOnTheCompilationUnitRemovesTheSpacesBehindTheLastToken()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = NormalizeLineEndings("class C\n{\n    int x;\n}   ", endOfLine);
            var expected = NormalizeLineEndings("class C\n{\n    int x;\n}", endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = workspace.AddProject("TestProject", LanguageNames.CSharp).AddDocument("Test.cs", SourceText.From(input));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, root, TestContext.CancellationToken);
                var actual = (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(Escape(expected), Escape(actual), $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies the holding side: the spaces behind the last token are removed together with the final line break that follows them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void SpacesBehindTheLastTokenFollowedByAFinalLineBreakAreRemoved()
    {
        const string input = "class C\n{\n    int x;\n}   \n";
        const string expected = "class C\n{\n    int x;\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the holding side: a whitespace-only last line after the last token's line break is removed together with the line
    /// break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void WhitespaceOnlyLastLineIsRemoved()
    {
        const string input = "class C\n{\n    int x;\n}\n   ";
        const string expected = "class C\n{\n    int x;\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the holding side: the spaces on a whitespace-only last line behind a block comment on the last token's line are
    /// removed together with the line break in front of them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void WhitespaceOnlyLastLineBehindABlockCommentIsRemoved()
    {
        const string input = "class C\n{\n    int x;\n} /* c */\n   ";
        const string expected = "class C\n{\n    int x;\n} /* c */";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example's other side of the condition: the gaps on the line of the last statement, which end the trailing
    /// whitespace trivia of a token with code following on the line, stay
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void GapsOnTheLineOfTheLastStatementAreKept()
    {
        const string input = "class C\n{\n    void M(int x)\n    {\n        int y = x + 1;\n    }\n}";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example's other side of the condition: the spaces at the end of a single-line comment that ends the file
    /// are part of the comment and stay
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void SpacesInsideASingleLineCommentThatEndsTheFileAreKept()
    {
        const string input = "class C\n{\n    int x;\n}\n\n// end   ";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the requirement's condition: a whitespace-only last line behind a directive that ends the file is removed, and the
    /// directive keeps its line break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    [Requirement("FMT-CLEAN-007")]
    public void WhitespaceOnlyLastLineBehindATrailingDirectiveIsRemovedAndTheDirectiveKeepsItsLineBreak()
    {
        const string input = "class C\n{\n    int x;\n}\n\n#pragma warning restore CS1591\n   ";
        const string expected = "class C\n{\n    int x;\n}\n\n#pragma warning restore CS1591\n";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Characterizes the whitespace behind an <c>#endif</c> that ends a file without a final line break, which is part of the
    /// directive's structure and stays
    /// </summary>
    [TestMethod]
    public void WhitespaceBehindAnEndIfThatEndsTheFileIsKept()
    {
        const string input = "class C\n{\n    int x;\n}\n#if NEVER\nint ignored;   \n#endif   ";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Characterizes the node-level path: when a code fix formats a member that shares its line with the token in front of it, the
    /// gap behind that token stays, because no line break follows it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task GapInFrontOfATargetOnTheSameLineIsKept()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = NormalizeLineEndings("class C\n{\n    int a;   int b;\n}", endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = workspace.AddProject("TestProject", LanguageNames.CSharp).AddDocument("Test.cs", SourceText.From(input));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var target = root.DescendantNodes()
                                 .OfType<FieldDeclarationSyntax>()
                                 .Single(field => field.Declaration.Variables.Single().Identifier.ValueText == "b");

                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken);
                var actual = (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(Escape(input), Escape(actual), $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Makes whitespace and line breaks visible in an assertion message
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The text with whitespace and line breaks escaped</returns>
    private static string Escape(string text)
    {
        return text.Replace("\r", "\\r", StringComparison.Ordinal)
                   .Replace("\n", "\\n", StringComparison.Ordinal)
                   .Replace("\t", "\\t", StringComparison.Ordinal)
                   .Replace(" ", "·", StringComparison.Ordinal);
    }

    /// <summary>
    /// Formats the text with <see cref="ReihitsuFormatter.FormatSyntaxTree"/>
    /// </summary>
    /// <param name="text">The text to format</param>
    /// <returns>The formatted text</returns>
    private string FormatTree(string text)
    {
        var tree = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken);

        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.IsNull(result.Unsupported, "The formatter must not refuse the input.");

        return result.Output.GetText(TestContext.CancellationToken).ToString();
    }

    #endregion // Methods
}