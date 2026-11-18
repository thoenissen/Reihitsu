using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Enumerations;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

/// <summary>
/// Builds the block body that replaces an expression body, centralizing the brace
/// construction shared by every expression-bodied member transform. The member-specific
/// decisions (statement form, which trivia carriers to pass) remain in the individual transforms
/// </summary>
internal static class ExpressionBodyToBlockConverter
{
    #region Methods

    /// <summary>
    /// Creates a block body that wraps the given expression. The arrow's leading trivia moves to the open brace. Neither gap
    /// around the arrow holds a comment, a directive, or disabled text, because the precheck refuses an input that holds one
    /// there (FMT-UNSUP-012, FMT-UNSUP-013)
    /// </summary>
    /// <param name="expression">The arrow expression to wrap</param>
    /// <param name="statementForm">The statement form the expression should take</param>
    /// <param name="arrowToken">The arrow token being replaced; its leading trivia moves to the open brace</param>
    /// <param name="semicolonToken">The semicolon token being replaced; its trailing trivia moves to the close brace and any comment in its leading trivia moves in front of the created statement's semicolon</param>
    /// <returns>The block body</returns>
    internal static BlockSyntax CreateBlock(ExpressionSyntax expression,
                                            ExpressionBodyStatementForm statementForm,
                                            SyntaxToken arrowToken,
                                            SyntaxToken semicolonToken)
    {
        var statement = CreateStatement(expression, statementForm, semicolonToken);

        // The created tokens are built with explicit trivia, so they carry no elastic trivia (FMT-STRUCT-026)
        var openBrace = SyntaxFactory.Token(arrowToken.LeadingTrivia, SyntaxKind.OpenBraceToken, default);

        return SyntaxFactory.Block(openBrace,
                                   SyntaxFactory.SingletonList(statement),
                                   SyntaxFactory.Token(default, SyntaxKind.CloseBraceToken, semicolonToken.TrailingTrivia));
    }

    /// <summary>
    /// Wraps the expression in the requested statement form. A <see cref="ThrowExpressionSyntax"/>
    /// always becomes a <see cref="ThrowStatementSyntax"/> regardless of the requested form, because
    /// <c>return throw ...;</c> does not compile (CS8115). Any comment or directive in the semicolon
    /// token's leading trivia that would otherwise be lost with the discarded token is carried in
    /// front of the newly created semicolon token instead. A <c>return</c> statement drops the
    /// expression's leading indentation when no line break is left in front of the expression, so an
    /// expression written on the line after the arrow joins the keyword exactly like one written on the
    /// arrow's line
    /// </summary>
    /// <param name="expression">The expression to wrap</param>
    /// <param name="statementForm">The statement form</param>
    /// <param name="semicolonToken">The semicolon token being replaced</param>
    /// <returns>The wrapping statement</returns>
    private static StatementSyntax CreateStatement(ExpressionSyntax expression,
                                                   ExpressionBodyStatementForm statementForm,
                                                   SyntaxToken semicolonToken)
    {
        var newSemicolonToken = SyntaxFactory.Token(ExtractSignificantTrivia(semicolonToken.LeadingTrivia), SyntaxKind.SemicolonToken, default);

        if (expression is ThrowExpressionSyntax throwExpression)
        {
            return SyntaxFactory.ThrowStatement(throwExpression.ThrowKeyword, throwExpression.Expression, newSemicolonToken);
        }

        if (statementForm == ExpressionBodyStatementForm.ExpressionStatement)
        {
            return SyntaxFactory.ExpressionStatement(expression, newSemicolonToken);
        }

        return CreateReturnStatement(expression, newSemicolonToken);
    }

    /// <summary>
    /// Wraps the expression in a <c>return</c> statement that the expression joins on the keyword's line. When the
    /// expression's leading trivia holds no line break, it is the indentation of an expression that started its own line
    /// after the arrow: the line break itself belonged to the discarded arrow token, and the indentation is dropped. Leading
    /// trivia that carries a line break is kept, because the layout it describes is then the author's. Otherwise the keyword
    /// gets one space behind it only when nothing separates it from the expression and the two would read as one token, as
    /// the lexer decides for <c>return42</c>
    /// </summary>
    /// <param name="expression">The expression to wrap</param>
    /// <param name="semicolonToken">The semicolon token of the created statement</param>
    /// <returns>The <c>return</c> statement</returns>
    private static ReturnStatementSyntax CreateReturnStatement(ExpressionSyntax expression, SyntaxToken semicolonToken)
    {
        var returnKeyword = SyntaxFactory.Token(default, SyntaxKind.ReturnKeyword, default);
        var leadingTrivia = expression.GetLeadingTrivia();

        if (leadingTrivia.Count > 0
            && leadingTrivia.All(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            expression = expression.WithLeadingTrivia(default(SyntaxTriviaList));
        }

        if (expression.GetLeadingTrivia().FullSpan.Length == 0
            && TokenSeparationUtilities.WouldMergeIntoOneToken(returnKeyword, expression.GetFirstToken()))
        {
            returnKeyword = returnKeyword.WithTrailingTrivia(SyntaxFactory.Space);
        }

        return SyntaxFactory.ReturnStatement(returnKeyword, expression, semicolonToken);
    }

    /// <summary>
    /// Extracts the comment or directive trivia worth preserving from a trivia list that would
    /// otherwise be discarded outright. A list carrying nothing but whitespace and line breaks
    /// collapses to an empty list, unchanged from today's behavior. A list carrying a comment or
    /// directive keeps that trivia together with whatever separates it from the far end (such as the
    /// end-of-line that terminates a single-line comment), with its own leading run of whitespace
    /// stripped so it does not leave a stray space when relocated
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns>The significant trivia to carry over, or an empty list when nothing but whitespace was present</returns>
    private static SyntaxTriviaList ExtractSignificantTrivia(SyntaxTriviaList triviaList)
    {
        var hasSignificantTrivia = triviaList.Any(static trivia => SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                                                                   || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));

        if (hasSignificantTrivia == false)
        {
            return default;
        }

        var significantTrivia = new List<SyntaxTrivia>();
        var skippingLeadingWhitespace = true;

        foreach (var trivia in triviaList)
        {
            if (skippingLeadingWhitespace && trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            skippingLeadingWhitespace = false;
            significantTrivia.Add(trivia);
        }

        return TriviaListUtilities.RemoveTrailingWhitespace(SyntaxFactory.TriviaList(significantTrivia));
    }

    #endregion // Methods
}