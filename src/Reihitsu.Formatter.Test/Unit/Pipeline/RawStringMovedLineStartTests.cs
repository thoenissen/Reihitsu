using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests that the indentation phase's move of a multi-line interpolated raw string literal leaves a tree that parses back to
/// itself (FMT-PIPE-021) and keeps every annotation (FMT-PIPE-022), whatever starts a moved line: text, a hole's opening
/// brace, a line inside a multi-line hole, the closing marker, or an empty or whitespace-only line, for one and two dollar
/// signs, for a closing marker in column 0 or indented, moved to the right or to the left, and for the first or a later
/// content line. Each test also checks the final output under LF and CRLF line endings against FMT-RAW-001 to FMT-RAW-003:
/// every line moves by the distance between the closing marker and the opening quotes
/// </summary>
[TestClass]
public class RawStringMovedLineStartTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The kind of every annotation these tests put on the input
    /// </summary>
    private const string AnnotationKind = "RawStringMovedLineStartTests";

    /// <summary>
    /// The column of the first <c>$</c> of every fixture's literal, behind <c>var s = </c> in the method body; its opening
    /// quotes stand one column further for each <c>$</c>
    /// </summary>
    private const int DollarColumn = 16;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies the move of a literal whose first content line starts with a hole and whose lines start in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void FirstLineStartingWithAHoleAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "{x} a");
    }

    /// <summary>
    /// Verifies the move of a two-dollar literal whose first content line starts with a hole and whose lines start in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-003")]
    public void DoubleDollarFirstLineStartingWithAHoleAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$$", 0, "{{x}} a");
    }

    /// <summary>
    /// Verifies the move of a literal whose only content line is a hole, with every line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void OnlyLineBeingAHoleAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "{x}");
    }

    /// <summary>
    /// Verifies the move of a literal whose second content line starts with a hole behind a line of text, with every line in
    /// column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void LaterLineStartingWithAHoleBehindTextAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a", "{x}");
    }

    /// <summary>
    /// Verifies the move of a two-dollar literal whose second content line starts with a hole behind a line of text, with
    /// every line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-003")]
    public void DoubleDollarLaterLineStartingWithAHoleBehindTextAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$$", 0, "a", "{{x}}");
    }

    /// <summary>
    /// Verifies the move of a literal whose second content line starts with a hole behind a line that ends with a hole, with
    /// every line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void LaterLineStartingWithAHoleBehindAHoleAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a {x}", "{y} b");
    }

    /// <summary>
    /// Verifies the move of a literal whose line behind the end of a multi-line hole starts with a hole, with every line in
    /// column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void LaterLineStartingWithAHoleBehindAMultiLineHoleAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a {Call(1,", "        2)}", "{y} b");
    }

    /// <summary>
    /// Verifies the move of a literal whose content line behind an empty line starts with a hole, with every line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-002")]
    public void LineStartingWithAHoleBehindAnEmptyLineAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a", string.Empty, "{x}");
    }

    /// <summary>
    /// Verifies the move of a literal whose first content line is empty and whose second starts with a hole, with every line
    /// in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-002")]
    public void LineStartingWithAHoleBehindAnEmptyFirstLineAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, string.Empty, "{x}");
    }

    /// <summary>
    /// Verifies the move of a literal whose content line behind a whitespace-only line starts with a hole, with every other
    /// line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void LineStartingWithAHoleBehindAWhitespaceOnlyLineAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a", "  ", "{x}");
    }

    /// <summary>
    /// Verifies the move of a literal whose last content line starts with a hole and is followed by the closing line, with
    /// every line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void LastLineStartingWithAHoleInFrontOfTheClosingLineAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a", "b {x}", "{y}");
    }

    /// <summary>
    /// Verifies the move of a literal whose first content line starts with a hole that spans two lines, with every line in
    /// column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void FirstLineStartingWithAMultiLineHoleAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "{Call(1,", "      2)} b");
    }

    /// <summary>
    /// Verifies the move of a literal whose content line starts with text, with every line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void LinesStartingWithTextAtColumnZeroMoveRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a {x}", "b");
    }

    /// <summary>
    /// Verifies the move of a two-dollar literal whose content line starts with a single brace, which is text, with every
    /// line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-003")]
    public void DoubleDollarLineStartingWithASingleBraceAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$$", 0, "{a} {{x}}");
    }

    /// <summary>
    /// Verifies the move of a literal whose hole line starts with the second argument of a call in a multi-line hole, with every line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void HoleLineStartingWithAnArgumentAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a {Call(1,", "        2)} b");
    }

    /// <summary>
    /// Verifies the move of a literal whose hole line starts with the hole's closing brace, with every line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a {x", "} b");
    }

    /// <summary>
    /// Verifies the move of a literal whose content line starts with a hole behind indentation that its closing marker in
    /// column 0 does not strip
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-002")]
    public void LineStartingWithAHoleBehindRelativeIndentationAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a", "  {x}");
    }

    /// <summary>
    /// Verifies the move of a literal whose first content line starts with a hole and whose closing marker stands indented,
    /// left of the opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void FirstLineStartingWithAHoleIndentedLeftOfTheQuotesMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 4, "{x} a");
    }

    /// <summary>
    /// Verifies the move of a two-dollar literal whose first content line starts with a hole and whose closing marker stands
    /// indented, left of the opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-003")]
    public void DoubleDollarFirstLineStartingWithAHoleIndentedLeftOfTheQuotesMovesRight()
    {
        AssertMovesUnderTheQuotes("$$", 4, "{{x}} a");
    }

    /// <summary>
    /// Verifies the move of a literal whose later content line starts with a hole and whose closing marker stands indented,
    /// left of the opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void LaterLineStartingWithAHoleIndentedLeftOfTheQuotesMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 4, "a", "{x}");
    }

    /// <summary>
    /// Verifies the move of a literal whose first content line starts with a hole and whose closing marker stands right of
    /// the opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void FirstLineStartingWithAHoleIndentedRightOfTheQuotesMovesLeft()
    {
        AssertMovesUnderTheQuotes("$", 24, "{x} a");
    }

    /// <summary>
    /// Verifies the move of a two-dollar literal whose first content line starts with a hole and whose closing marker stands
    /// right of the opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-003")]
    public void DoubleDollarFirstLineStartingWithAHoleIndentedRightOfTheQuotesMovesLeft()
    {
        AssertMovesUnderTheQuotes("$$", 24, "{{x}} a");
    }

    /// <summary>
    /// Verifies the move of a literal whose later content line starts with a hole and whose closing marker stands right of
    /// the opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void LaterLineStartingWithAHoleIndentedRightOfTheQuotesMovesLeft()
    {
        AssertMovesUnderTheQuotes("$", 24, "a", "{x}");
    }

    /// <summary>
    /// Verifies the move of a literal with an empty line between two lines of text, with every line in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-002")]
    public void EmptyLineBetweenTextLinesAtColumnZeroStaysEmpty()
    {
        AssertMovesUnderTheQuotes("$", 0, "a {x}", string.Empty, "b");
    }

    /// <summary>
    /// Verifies the move of a literal with a whitespace-only line between two lines of text, with every other line in
    /// column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void WhitespaceOnlyLineBetweenTextLinesAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes("$", 0, "a {x}", "  ", "b");
    }

    /// <summary>
    /// Verifies the move of a plain raw string literal whose lines start in column 0, which has no hole a line could start with
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void PlainLiteralAtColumnZeroMovesRight()
    {
        AssertMovesUnderTheQuotes(string.Empty, 0, "{x} a", string.Empty, "b");
    }

    /// <summary>
    /// Verifies the move of a plain raw string literal whose closing marker stands right of the opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void PlainLiteralIndentedRightOfTheQuotesMovesLeft()
    {
        AssertMovesUnderTheQuotes(string.Empty, 24, "{x} a", "b");
    }

    /// <summary>
    /// Verifies the move of a UTF-8 raw string literal whose lines start in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-004")]
    public void Utf8LiteralAtColumnZeroMovesRight()
    {
        const string input = """"
                             class C
                             {
                                 void M(int x, int y)
                                 {
                                     var s = """
                             {x} a
                             """u8;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M(int x, int y)
                                    {
                                        var s = """
                                                {x} a
                                                """u8;
                                    }
                                }
                                """";

        AssertMoved(input, expected);
    }

    /// <summary>
    /// Verifies the move of a literal nested in a hole whose own content line starts with a hole, with every line of both
    /// literals in column 0: the nested literal's lines move with the outer literal and then under the nested opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    [Requirement("FMT-PIPE-022")]
    [Requirement("FMT-RAW-001")]
    public void NestedLiteralLineStartingWithAHoleAtColumnZeroMovesRight()
    {
        const string input = """"""
                             class C
                             {
                                 void M(int x, int y)
                                 {
                                     var s = $"""
                             a {$"""
                             {x}
                             """} b
                             """;
                                 }
                             }
                             """""";
        const string expected = """"""
                                class C
                                {
                                    void M(int x, int y)
                                    {
                                        var s = $"""
                                                 a {$"""
                                                     {x}
                                                     """} b
                                                 """;
                                    }
                                }
                                """""";

        AssertMoved(input, expected);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Writes a class whose method declares an interpolated raw string literal with the given lines, each of them and the
    /// closing marker behind the given number of spaces; an empty line stays empty
    /// </summary>
    /// <param name="dollars">The <c>$</c> characters of the literal</param>
    /// <param name="column">The column of the closing marker</param>
    /// <param name="lines">The content lines, relative to the closing marker</param>
    /// <returns>The source text with LF line endings</returns>
    private static string CreateSource(string dollars, int column, IReadOnlyList<string> lines)
    {
        var indentation = new string(' ', column);
        var builder = new StringBuilder();

        builder.Append("class C\n{\n    void M(int x, int y)\n    {\n        var s = ")
               .Append(dollars)
               .Append("\"\"\"\n");

        foreach (var line in lines)
        {
            if (line.Length > 0)
            {
                builder.Append(indentation).Append(line);
            }

            builder.Append('\n');
        }

        builder.Append(indentation).Append("\"\"\";\n    }\n}");

        return builder.ToString();
    }

    /// <summary>
    /// Asserts that a literal with the given lines, written with its closing marker in the given column, moves under its
    /// opening quotes: every line and the closing marker by the same number of columns (FMT-RAW-001 to FMT-RAW-003), with
    /// every phase's tree parsing back to itself and keeping every annotation
    /// </summary>
    /// <param name="dollars">The <c>$</c> characters of the literal</param>
    /// <param name="column">The column of the closing marker in the input</param>
    /// <param name="lines">The content lines, relative to the closing marker</param>
    private void AssertMovesUnderTheQuotes(string dollars, int column, params string[] lines)
    {
        var quoteColumn = DollarColumn + dollars.Length;

        AssertMoved(CreateSource(dollars, column, lines), CreateSource(dollars, quoteColumn, lines));
    }

    /// <summary>
    /// Asserts under LF and CRLF line endings, in this order so a failure names the first invariant that breaks: every phase
    /// keeping the annotations on every token of each literal and on every node inside its holes (FMT-PIPE-022); the final
    /// output of <see cref="ReihitsuFormatter.FormatSyntaxTree"/>, which does not compare the phases' trees; and, through
    /// <see cref="FormatterTestsBase.AssertRuleResult(string, string, CSharpParseOptions)"/>, the same output with a stable
    /// second pass and every phase's tree parsing back to itself with token values (FMT-PIPE-021)
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    /// <param name="expected">The expected output with LF line endings</param>
    private void AssertMoved(string input, string expected)
    {
        Assert.AreNotEqual(expected, input, "The fixture should move the literal.");

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);

            AssertAnnotationsKept(normalizedInput, endOfLine);

            var tree = CSharpSyntaxTree.ParseText(normalizedInput, cancellationToken: TestContext.CancellationToken);
            var output = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).GetText(TestContext.CancellationToken).ToString();

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), output, $"Final output under {DescribeLineEnding(endOfLine)} line endings.");
        }

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Puts one annotation of its own on every token of each interpolated string literal and on every node inside its
    /// holes, formats the input through the production pipeline, and fails for every annotation a phase's output no longer
    /// carries on exactly one element of the original kind
    /// </summary>
    /// <param name="input">The input with the line ending already applied</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    private void AssertAnnotationsKept(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var literals = root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>().ToList();
        var nodes = literals.SelectMany(literal => literal.Contents.OfType<InterpolationSyntax>())
                            .SelectMany(hole => hole.DescendantNodesAndSelf())
                            .Distinct()
                            .ToList();
        var tokens = literals.SelectMany(literal => literal.DescendantTokens())
                             .Concat(root.DescendantTokens().Where(token => token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken) || token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken)))
                             .Distinct()
                             .ToList();
        var annotations = new Dictionary<SyntaxNodeOrToken, SyntaxAnnotation>();

        foreach (var element in nodes.Select(node => (SyntaxNodeOrToken)node).Concat(tokens.Select(token => (SyntaxNodeOrToken)token)))
        {
            annotations[element] = new SyntaxAnnotation(AnnotationKind, $"{element.Kind()} '{element}' at {element.SpanStart}");
        }

        var annotated = root.ReplaceSyntax(nodes,
                                           (original, rewritten) => rewritten.WithAdditionalAnnotations(annotations[original]),
                                           tokens,
                                           (original, rewritten) => rewritten.WithAdditionalAnnotations(annotations[original]),
                                           [],
                                           (_, rewritten) => rewritten);
        var context = new FormattingContext(endOfLine, languageVersion: LanguageVersionResolver.Resolve(tree.Options));

        FormattingPipeline.Execute(annotated,
                                   context,
                                   (phaseName, _, after) =>
                                   {
                                       var lost = annotations.Where(entry =>
                                                                    {
                                                                        var carriers = after.GetAnnotatedNodesAndTokens(entry.Value).ToList();

                                                                        return carriers.Count != 1
                                                                               || carriers[0].RawKind != entry.Key.RawKind;
                                                                    })
                                                             .Select(entry => entry.Value.Data)
                                                             .ToList();

                                       Assert.IsEmpty(lost, $"{phaseName} dropped annotations (FMT-PIPE-022) under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{string.Join(Environment.NewLine, lost)}{Environment.NewLine}{after.ToFullString()}");
                                   },
                                   CancellationToken.None);
    }

    #endregion // Methods
}