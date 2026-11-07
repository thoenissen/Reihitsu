using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Test.Helpers;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests that every phase returns a tree whose text, parsed again with the parse options of the tree the formatter was
/// given, is that tree (FMT-PIPE-021)
/// </summary>
[TestClass]
public class PhaseTreeRoundTripTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The first example of FMT-PIPE-021: an expression-bodied method and an unbraced <c>if</c>/<c>else</c>, whose <c>if</c>
    /// body carries a trailing single-line comment
    /// </summary>
    private const string ConvertedAndBracedClass = """
                                                   class C
                                                   {
                                                       int Get() => 42;

                                                       void M(bool a)
                                                       {
                                                           if (a)
                                                               Foo(); // trailing
                                                           else
                                                               Bar();
                                                       }
                                                   }
                                                   """;

    /// <summary>
    /// The token-value example of FMT-PIPE-021: an interpolated raw string literal whose content lines, hole line, and closing
    /// line start in column 0
    /// </summary>
    private const string ColumnZeroInterpolatedLiteral = """"
                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 var s = $"""
                                                         a {Call(1,
                                                         2)} b
                                                         """;
                                                             }
                                                         }
                                                         """";

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that the output of every phase for the first example of FMT-PIPE-021 parses back to the phase's tree
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void EveryPhaseOfTheConvertedAndBracedClassParsesBackToItsTree()
    {
        foreach (var endOfLine in _lineEndings)
        {
            AssertEveryPhaseParsesBack(NormalizeLineEndings(ConvertedAndBracedClass, endOfLine), endOfLine, CSharpParseOptions.Default);
        }
    }

    /// <summary>
    /// Verifies that the structural-transform phase's output reads <c>int Get(){return 42;}</c>, with <c>return</c> and
    /// <c>42</c> as two tokens, as the first example of FMT-PIPE-021 states
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void StructuralTransformPhaseOutputReadsTheCreatedReturnAndTheLiteralAsTwoTokens()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ObservedPipeline.GetPhaseOutput(NormalizeLineEndings(ConvertedAndBracedClass, endOfLine), endOfLine, "StructuralTransformPhase");
            var text = output.ToFullString();

            Assert.Contains("    int Get(){return 42;}" + endOfLine, text, $"Under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.DoesNotContain("return42", text, $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that in the structural-transform phase's output of the first example of FMT-PIPE-021 a line break stands
    /// between <c>// trailing</c> and the inserted <c>}</c>, so the brace is not part of the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void StructuralTransformPhaseOutputPutsTheInsertedBraceOutsideTheTrailingComment()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ObservedPipeline.GetPhaseOutput(NormalizeLineEndings(ConvertedAndBracedClass, endOfLine), endOfLine, "StructuralTransformPhase");
            var text = output.ToFullString();

            Assert.DoesNotContain("// trailing}", text, $"Under {DescribeLineEnding(endOfLine)} line endings.");

            var reparsedBody = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken)
                                               .GetRoot(TestContext.CancellationToken)
                                               .DescendantNodes()
                                               .OfType<IfStatementSyntax>()
                                               .Single()
                                               .Statement as BlockSyntax;

            Assert.IsNotNull(reparsedBody, $"The reparsed if body should be a block under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.IsFalse(reparsedBody.CloseBraceToken.IsMissing, $"The reparsed if body should have its closing brace under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.Contains("// trailing", reparsedBody.Statements.Single().GetTrailingTrivia().ToFullString(), $"The comment should trail the braced statement under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a phase output in which the parser gives a line break and whitespace to the other of the two tokens
    /// around them is the same tree: the line-break phase writes the line break in front of the <c>{</c> it moves, behind
    /// the space that ends <c>if (a) </c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void LineBreakWrittenInFrontOfTheMovedBraceLeavesTheSameTree()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a) {
                                         M(a);
                                     }
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            AssertEveryPhaseParsesBack(NormalizeLineEndings(input, endOfLine), endOfLine, CSharpParseOptions.Default);

            var root = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var openBrace = root.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement.GetFirstToken();
            var closeParenthesis = openBrace.GetPreviousToken();
            var moved = root.ReplaceTokens([closeParenthesis, openBrace],
                                           (original, _) => original == openBrace
                                                                ? original.WithLeadingTrivia(SyntaxFactory.EndOfLine(endOfLine), SyntaxFactory.Whitespace("        "))
                                                                : original);

            Assert.IsNull(PhaseTreeRoundTrip.FindDifference(moved, CSharpParseOptions.Default), $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a tree in which <c>// leading</c> stands behind an opening brace on the brace's line, as leading trivia
    /// of the statement after it, is not the tree its text parses back to, in which the comment trails the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void LeadingCommentBehindABraceOnTheBracesLineIsNotTheSameTree()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     {
                                         M();
                                     }
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var root = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var innerBlock = root.DescendantNodes().OfType<BlockSyntax>().Last();
            var statementToken = innerBlock.Statements.Single().GetFirstToken();
            var changed = root.ReplaceTokens([innerBlock.OpenBraceToken, statementToken],
                                             (original, _) => original == innerBlock.OpenBraceToken
                                                                  ? original.WithTrailingTrivia()
                                                                  : original.WithLeadingTrivia(SyntaxFactory.Comment("// leading"), SyntaxFactory.EndOfLine(endOfLine), SyntaxFactory.Whitespace("            ")));

            var difference = PhaseTreeRoundTrip.FindDifference(changed, CSharpParseOptions.Default);

            Assert.IsNotNull(difference, $"Under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.Contains("// leading", difference, $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the output of every phase is parsed again with the preprocessor symbols of the tree the formatter was
    /// given: with <c>DEBUG</c> defined, the converted method under <c>#if DEBUG</c> is code, and every phase's output parses
    /// back to its tree with that symbol, while the same text parsed without it reads the method as disabled text
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void PhaseOutputIsParsedAgainWithThePreprocessorSymbolsOfTheGivenTree()
    {
        const string input = """
                             class C
                             {
                             #if DEBUG
                                 int Get() => 42;
                             #endif
                             }
                             """;
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG");

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);

            AssertEveryPhaseParsesBack(normalizedInput, endOfLine, parseOptions);

            var output = ObservedPipeline.GetPhaseOutput(normalizedInput, endOfLine, "StructuralTransformPhase", parseOptions);

            Assert.IsNotNull(PhaseTreeRoundTrip.FindDifference(output, CSharpParseOptions.Default), $"Without DEBUG the method should read back as disabled text under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that every phase's output parses back to its tree for a <c>Preview</c> tree, whose language version the
    /// formatter clamps for its own decisions but the reparse keeps
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void PhaseOutputOfAPreviewTreeIsParsedAgainWithThePreviewLanguageVersion()
    {
        const string input = """
                             class C
                             {
                                 int Get() => 42;
                             }
                             """;
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

        foreach (var endOfLine in _lineEndings)
        {
            AssertEveryPhaseParsesBack(NormalizeLineEndings(input, endOfLine), endOfLine, parseOptions);
        }
    }

    /// <summary>
    /// Verifies the token-value example of FMT-PIPE-021: after the indentation phase moves the lines of an interpolated raw
    /// string literal whose lines start in column 0, the text token in front of the hole still stands for <c>a </c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void MovedInterpolatedTextTokenStillStandsForItsContent()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ObservedPipeline.GetPhaseOutput(NormalizeLineEndings(ColumnZeroInterpolatedLiteral, endOfLine), endOfLine, "IndentationPhase");
            var textToken = output.DescendantNodes()
                                  .OfType<InterpolatedStringExpressionSyntax>()
                                  .Single()
                                  .Contents
                                  .OfType<InterpolatedStringTextSyntax>()
                                  .First()
                                  .TextToken;
            var description = $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output.ToFullString()}";

            Assert.AreNotEqual("a ", textToken.Text, $"The literal should have moved. {description}");
            Assert.AreEqual("a ", textToken.ValueText, description);
            Assert.IsNull(PhaseTreeRoundTrip.FindDifference(output, CSharpParseOptions.Default), description);
        }
    }

    /// <summary>
    /// Verifies the final output of the token-value example of FMT-PIPE-021, with every phase's tree, token values included,
    /// parsing back to itself
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void ColumnZeroInterpolatedLiteralMovesUnderItsOpeningQuotes()
    {
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var s = $"""
                                                 a {Call(1,
                                                         2)} b
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(ColumnZeroInterpolatedLiteral, expected);
    }

    /// <summary>
    /// Verifies that every phase's tree keeps the values of the tokens of a moved interpolated raw string literal with an
    /// indented closing line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void MovedInterpolatedLiteralWithIndentedClosingLineKeepsItsTokenValues()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var s = $"""
                                         a {Call(1,
                                         2)} b
                                         c
                                         """;
                                 }
                             }
                             """";

        AssertEveryLineEndingParsesBack(input);
    }

    /// <summary>
    /// Verifies that every phase's tree keeps the values of the tokens of an interpolated raw string literal with two dollar
    /// signs and two holes, whose lines start in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void MovedDoubleDollarInterpolatedLiteralAtColumnZeroKeepsItsTokenValues()
    {
        const string input = """"
                             class C
                             {
                                 void M(int x, int y)
                                 {
                                     var s = $$"""
                             {a} {{x}}
                             b {{y}} c
                             """;
                                 }
                             }
                             """";

        AssertEveryLineEndingParsesBack(input);
    }

    /// <summary>
    /// Verifies that every phase's tree keeps the value of a plain multi-line raw string literal whose lines start in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void MovedPlainRawLiteralAtColumnZeroKeepsItsValue()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var s = """
                             a
                               b
                             """;
                                 }
                             }
                             """";

        AssertEveryLineEndingParsesBack(input);
    }

    /// <summary>
    /// Verifies that every phase's tree keeps the value of a UTF-8 multi-line raw string literal whose lines start in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void MovedUtf8RawLiteralAtColumnZeroKeepsItsValue()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var s = """
                             a
                             """u8;
                                 }
                             }
                             """";

        AssertEveryLineEndingParsesBack(input);
    }

    /// <summary>
    /// Verifies that every phase's tree keeps the values of the tokens of an interpolated raw string literal that the
    /// indentation phase moves to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-021")]
    public void InterpolatedLiteralMovedToTheLeftKeepsItsTokenValues()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var s = $"""
                                                         a {Call(1,
                                                         2)} b
                                                         """;
                                 }
                             }
                             """";

        AssertEveryLineEndingParsesBack(input);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Asserts under LF and CRLF line endings that every phase's tree parses back to itself, token values included
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    private void AssertEveryLineEndingParsesBack(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            AssertEveryPhaseParsesBack(NormalizeLineEndings(input, endOfLine), endOfLine, CSharpParseOptions.Default);
        }
    }

    /// <summary>
    /// Formats the input through the production pipeline and fails for every phase whose tree differs from the tree its
    /// text parses back to with the given parse options
    /// </summary>
    /// <param name="input">The input with the line ending already applied</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <param name="parseOptions">The parse options of the tree the formatter is given</param>
    private void AssertEveryPhaseParsesBack(string input, string endOfLine, CSharpParseOptions parseOptions)
    {
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(endOfLine, languageVersion: LanguageVersionResolver.Resolve(tree.Options));
        var (_, failures) = PhaseTreeRoundTrip.ExecuteAndCompare(tree.GetRoot(TestContext.CancellationToken), context, parseOptions);

        Assert.IsEmpty(failures, $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    #endregion // Methods
}