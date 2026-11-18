using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for the trivia-edit helpers in <see cref="LineBreakTriviaUtilities"/>
/// </summary>
[TestClass]
public class LineBreakTriviaUtilitiesTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that an end-of-line trivia is prepended to a token's leading trivia
    /// </summary>
    [TestMethod]
    public void PrependEndOfLineAddsLeadingEndOfLine()
    {
        // Arrange
        var token = SyntaxFactory.Identifier("x");

        // Act
        var result = LineBreakTriviaUtilities.PrependEndOfLine(token, "\n");

        // Assert
        Assert.IsTrue(result.LeadingTrivia.Count > 0 && result.LeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia), "An end-of-line trivia should be prepended.");
        Assert.AreEqual("\n", result.LeadingTrivia[0].ToString(), "The prepended end-of-line should use the requested sequence.");
    }

    /// <summary>
    /// Verifies that an end-of-line trivia is appended to a trivia list
    /// </summary>
    [TestMethod]
    public void AppendEndOfLineAddsTrailingEndOfLine()
    {
        // Arrange
        var list = SyntaxFactory.TriviaList(SyntaxFactory.Space);

        // Act
        var result = LineBreakTriviaUtilities.AppendEndOfLine(list, "\n");

        // Assert
        Assert.AreEqual(2, result.Count, "The trivia list should grow by one entry.");
        Assert.IsTrue(result[result.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia), "The appended trivia should be an end-of-line.");
    }

    /// <summary>
    /// Verifies that a token preceded by a line break is collapsed onto the previous line
    /// </summary>
    [TestMethod]
    public void CollapseTokenToSameLineRemovesLineBreakBeforeToken()
    {
        // Arrange
        var block = (BlockSyntax)SyntaxFactory.ParseStatement("{\n}");

        // Act
        var result = LineBreakTriviaUtilities.CollapseTokenToSameLine(block, block.CloseBraceToken);

        // Assert
        Assert.DoesNotContain("\n", result.ToFullString(), "The line break before the collapsed token should be removed.");
    }

    /// <summary>
    /// Verifies that a token preceded by a line break is not collapsed when the previous token
    /// carries a trailing single-line comment, because the join would absorb the token into the comment
    /// </summary>
    [TestMethod]
    public void CollapseTokenToSameLineSkipsWhenPreviousTokenHasTrailingComment()
    {
        // Arrange
        var block = (BlockSyntax)SyntaxFactory.ParseStatement("{ // note\n}");

        // Act
        var result = LineBreakTriviaUtilities.CollapseTokenToSameLine(block, block.CloseBraceToken);

        // Assert
        Assert.AreEqual(block.ToFullString(), result.ToFullString(), "The collapse must be skipped so the comment is preserved.");
        Assert.Contains("\n", result.ToFullString(), "The line break after the comment must be preserved.");
    }

    /// <summary>
    /// Verifies that a token preceded by a preprocessor directive is not collapsed, because removing the
    /// line break that ends the directive's own line would move the directive off the start of its line and
    /// invalidate it. The collapse must return the very same node instance so callers that loop while a
    /// change is reported terminate instead of spinning on a rewrite that never makes progress
    /// </summary>
    [TestMethod]
    public void CollapseTokenToSameLineSkipsWhenGapContainsDirective()
    {
        // Arrange: the token after the attribute list carries a leading preprocessor directive
        var method = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("void M([NotNull]\n#nullable enable\n    string target) { }");
        var tokenAfterAttribute = method.ParameterList.Parameters[0].AttributeLists[0].CloseBracketToken.GetNextToken();

        // Act
        var result = LineBreakTriviaUtilities.CollapseTokenToSameLine(method, tokenAfterAttribute);

        // Assert
        Assert.AreSame(method, result, "The collapse must be refused and return the same node so the directive keeps its own line and callers terminate.");
    }

    /// <summary>
    /// Verifies that a token is moved onto a new line
    /// </summary>
    [TestMethod]
    public void MoveTokenToNewLineAddsLineBreakBeforeToken()
    {
        // Arrange
        var block = (BlockSyntax)SyntaxFactory.ParseStatement("{ }");

        // Act
        var result = LineBreakTriviaUtilities.MoveTokenToNewLine(block, block.CloseBraceToken, "\n");

        // Assert
        Assert.Contains("\n", result.ToFullString(), "A line break should be inserted before the moved token.");
    }

    /// <summary>
    /// Verifies that a line break is appended behind the trailing trivia of the token in front of a token that does not
    /// start its line yet, and that the whitespace at the end of that trivia is kept for the cleanup phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-029")]
    public void EndLineInFrontOfAppendsLineBreakBehindWhitespace()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(1,   2)");
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var token = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var result = LineBreakTriviaUtilities.EndLineInFrontOf(comma, token, "\n");

        // Assert
        Assert.AreEqual("   \n", result.ToFullString(), "The line break should be appended behind the whitespace, which stays.");
        Assert.IsTrue(result[result.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia), "The appended trivia should be an end-of-line.");
    }

    /// <summary>
    /// Verifies that the appended line break uses the requested end-of-line sequence
    /// </summary>
    [TestMethod]
    public void EndLineInFrontOfUsesRequestedEndOfLine()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(1,2)");
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var token = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var result = LineBreakTriviaUtilities.EndLineInFrontOf(comma, token, "\r\n");

        // Assert
        Assert.AreEqual("\r\n", result.ToFullString(), "The appended line break should use the requested sequence.");
    }

    /// <summary>
    /// Verifies that a block comment behind the token in front of the gap stays in front of the appended line break
    /// </summary>
    [TestMethod]
    public void EndLineInFrontOfAppendsLineBreakBehindBlockComment()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(1, /* c */ 2)");
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var token = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var result = LineBreakTriviaUtilities.EndLineInFrontOf(comma, token, "\n");

        // Assert
        Assert.AreEqual(" /* c */ \n", result.ToFullString(), "The line break should be appended behind the comment and the whitespace.");
    }

    /// <summary>
    /// Verifies that the trailing trivia is returned unchanged when it already ends the line, so the gap keeps exactly one
    /// line break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void EndLineInFrontOfKeepsTrailingLineBreak()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(1,  \n    2)");
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var token = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var result = LineBreakTriviaUtilities.EndLineInFrontOf(comma, token, "\n");

        // Assert
        Assert.AreEqual(comma.TrailingTrivia, result, "The trailing trivia that already ends the line should be returned unchanged.");
    }

    /// <summary>
    /// Verifies that the trailing trivia is returned unchanged when an end-of-line comment ends the line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void EndLineInFrontOfKeepsLineBreakBehindEndOfLineComment()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(1, // c\n    2)");
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var token = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var result = LineBreakTriviaUtilities.EndLineInFrontOf(comma, token, "\n");

        // Assert
        Assert.AreEqual(comma.TrailingTrivia, result, "The line break behind the comment should stay the gap's only one.");
    }

    /// <summary>
    /// Verifies that the trailing trivia is returned unchanged when the line break stands in the leading trivia of the token
    /// that starts the line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void EndLineInFrontOfKeepsLineBreakInLeadingTriviaOfToken()
    {
        // Arrange
        var previousToken = SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space);
        var token = SyntaxFactory.Identifier("b").WithLeadingTrivia(SyntaxFactory.EndOfLine("\n"), SyntaxFactory.Whitespace("    "));

        // Act
        var result = LineBreakTriviaUtilities.EndLineInFrontOf(previousToken, token, "\n");

        // Assert
        Assert.AreEqual(previousToken.TrailingTrivia, result, "The line break in front of the token should stay the gap's only one.");
    }

    /// <summary>
    /// Verifies that the trailing trivia is returned unchanged when the token stands behind the closing line of a multi-line
    /// comment whose own line is started by a line break: the shared helper takes that line break as the token's line start
    /// </summary>
    [TestMethod]
    public void EndLineInFrontOfKeepsLineBreakInFrontOfMultiLineComment()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(1,\n    /* one\n       two */ 2)");
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var token = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var result = LineBreakTriviaUtilities.EndLineInFrontOf(comma, token, "\n");

        // Assert
        Assert.AreEqual(comma.TrailingTrivia, result, "The helper should keep the trailing trivia as it is.");
        Assert.AreEqual("    /* one\n       two */ ", token.LeadingTrivia.ToFullString(), "The fixture's comment should stand in the leading trivia of the token.");
    }

    /// <summary>
    /// Verifies that joining no gap returns the very node it was given
    /// </summary>
    [TestMethod]
    public void JoinGapsWithoutGapsReturnsSameNode()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(1,\n    2)");

        // Act
        var result = LineBreakTriviaUtilities.JoinGaps(invocation, []);

        // Assert
        Assert.AreSame(invocation, result, "Without a gap the node should be returned unchanged.");
    }

    /// <summary>
    /// Verifies that a gap is joined: the line break and the whitespace on both sides of it are removed and the minimal
    /// separator is written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void JoinGapsJoinsGapWithMinimalSeparator()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(1,   \n    2)");
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var token = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var result = LineBreakTriviaUtilities.JoinGaps(invocation, [(comma, token)]);

        // Assert
        Assert.AreEqual("N(1,2)", result.ToFullString(), "The gap should be joined without whitespace.");
    }

    /// <summary>
    /// Verifies that a join writes one space where the two tokens written without whitespace would read as one token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void JoinGapsWritesSpaceWhereTokensWouldMerge()
    {
        // Arrange
        var statement = (LocalDeclarationStatementSyntax)SyntaxFactory.ParseStatement("int\r\n    x = 1;");
        var typeToken = statement.Declaration.Type.GetFirstToken();
        var identifier = statement.Declaration.Variables[0].Identifier;

        // Act
        var result = LineBreakTriviaUtilities.JoinGaps(statement, [(typeToken, identifier)]);

        // Assert
        Assert.AreEqual("int x = 1;", result.ToFullString(), "The joined gap should hold one space.");
    }

    /// <summary>
    /// Verifies that a token that stands at the end of one gap and at the start of the next carries both joins
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void JoinGapsJoinsBothSidesOfOneToken()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(\n    1\n    ,\n    2)");
        var openParenthesis = invocation.ArgumentList.OpenParenToken;
        var first = invocation.ArgumentList.Arguments[0].GetFirstToken();
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var second = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var result = LineBreakTriviaUtilities.JoinGaps(invocation, [(openParenthesis, first), (first, comma), (comma, second)]);

        // Assert
        Assert.AreEqual("N(1,2)", result.ToFullString(), "Every gap should be joined, also on both sides of one token.");
    }

    /// <summary>
    /// Verifies that the result does not depend on the order in which the gaps are given
    /// </summary>
    [TestMethod]
    public void JoinGapsResultDoesNotDependOnGapOrder()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(\n    1  \n    ,\n    2)");
        var openParenthesis = invocation.ArgumentList.OpenParenToken;
        var first = invocation.ArgumentList.Arguments[0].GetFirstToken();
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var second = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var inTextOrder = LineBreakTriviaUtilities.JoinGaps(invocation, [(openParenthesis, first), (first, comma), (comma, second)]);
        var reversed = LineBreakTriviaUtilities.JoinGaps(invocation, [(comma, second), (first, comma), (openParenthesis, first)]);
        var mixed = LineBreakTriviaUtilities.JoinGaps(invocation, [(first, comma), (comma, second), (openParenthesis, first)]);

        // Assert
        Assert.AreEqual("N(1,2)", inTextOrder.ToFullString(), "The gaps given in text order should be joined.");
        Assert.AreEqual(inTextOrder.ToFullString(), reversed.ToFullString(), "The gaps given in reverse order should give the same result.");
        Assert.AreEqual(inTextOrder.ToFullString(), mixed.ToFullString(), "The gaps given in mixed order should give the same result.");
    }

    /// <summary>
    /// Verifies that only the gaps given are joined, and every other line break of the node stays
    /// </summary>
    [TestMethod]
    public void JoinGapsLeavesOtherGapsUnchanged()
    {
        // Arrange
        var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("N(1,\n    2,\n    3)");
        var comma = invocation.ArgumentList.Arguments.GetSeparator(0);
        var second = invocation.ArgumentList.Arguments[1].GetFirstToken();

        // Act
        var result = LineBreakTriviaUtilities.JoinGaps(invocation, [(comma, second)]);

        // Assert
        Assert.AreEqual("N(1,2,\n    3)", result.ToFullString(), "Only the given gap should be joined.");
    }

    #endregion // Methods
}