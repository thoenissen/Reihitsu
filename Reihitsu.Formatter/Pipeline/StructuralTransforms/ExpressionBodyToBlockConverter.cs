using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Enumerations;
using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Builds the block body (and the indexer's get-accessor-list variant) that replaces an
/// expression body, centralizing the brace construction shared by every expression-bodied
/// member transform. The member-specific decisions (statement form, which trivia carriers
/// to pass) remain in the individual transforms
/// </summary>
internal static class ExpressionBodyToBlockConverter
{
    #region Methods

    /// <summary>
    /// Creates a block body that wraps the given expression
    /// </summary>
    /// <param name="expression">The arrow expression to wrap</param>
    /// <param name="statementForm">The statement form the expression should take</param>
    /// <param name="arrowToken">The arrow token being replaced; its leading trivia moves to the open brace and any comment in its trailing trivia moves onto the open brace's trailing trivia, mirroring a hand-written <c>{ // comment</c> block opener</param>
    /// <param name="semicolonToken">The semicolon token being replaced; its trailing trivia moves to the close brace and any comment in its leading trivia moves in front of the created statement's semicolon</param>
    /// <returns>The block body</returns>
    internal static BlockSyntax CreateBlock(ExpressionSyntax expression,
                                            ExpressionBodyStatementForm statementForm,
                                            SyntaxToken arrowToken,
                                            SyntaxToken semicolonToken)
    {
        var statement = CreateStatement(expression, statementForm, semicolonToken);
        var openBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                     .WithLeadingTrivia(arrowToken.LeadingTrivia)
                                     .WithTrailingTrivia(ExtractSignificantTrivia(arrowToken.TrailingTrivia));

        return SyntaxFactory.Block(openBrace,
                                   SyntaxFactory.SingletonList(statement),
                                   SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(semicolonToken.TrailingTrivia));
    }

    /// <summary>
    /// Creates a get-accessor list whose single getter returns the given expression
    /// </summary>
    /// <param name="expression">The arrow expression to return</param>
    /// <param name="arrowToken">The arrow token being replaced; its leading trivia moves to the accessor-list open brace and any comment in its trailing trivia moves onto the accessor-list open brace's trailing trivia, mirroring a hand-written <c>{ // comment</c> block opener</param>
    /// <param name="semicolonToken">The semicolon token being replaced; its trailing trivia moves to the accessor-list close brace and any comment in its leading trivia moves in front of the created return statement's semicolon</param>
    /// <returns>The accessor list with a single get accessor</returns>
    internal static AccessorListSyntax CreateGetAccessorList(ExpressionSyntax expression,
                                                             SyntaxToken arrowToken,
                                                             SyntaxToken semicolonToken)
    {
        var returnStatement = CreateStatement(expression, ExpressionBodyStatementForm.ReturnStatement, semicolonToken);
        var openBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                     .WithLeadingTrivia(arrowToken.LeadingTrivia)
                                     .WithTrailingTrivia(ExtractSignificantTrivia(arrowToken.TrailingTrivia));

        var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                  .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList(returnStatement)));

        return SyntaxFactory.AccessorList(openBrace,
                                          SyntaxFactory.SingletonList(getter),
                                          SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(semicolonToken.TrailingTrivia));
    }

    /// <summary>
    /// Wraps the expression in the requested statement form. A <see cref="ThrowExpressionSyntax"/>
    /// always becomes a <see cref="ThrowStatementSyntax"/> regardless of the requested form, because
    /// <c>return throw ...;</c> does not compile (CS8115). Any comment or directive in the semicolon
    /// token's leading trivia that would otherwise be lost with the discarded token is carried in
    /// front of the newly created semicolon token instead
    /// </summary>
    /// <param name="expression">The expression to wrap</param>
    /// <param name="statementForm">The statement form</param>
    /// <param name="semicolonToken">The semicolon token being replaced</param>
    /// <returns>The wrapping statement</returns>
    private static StatementSyntax CreateStatement(ExpressionSyntax expression,
                                                   ExpressionBodyStatementForm statementForm,
                                                   SyntaxToken semicolonToken)
    {
        var newSemicolonToken = SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                             .WithLeadingTrivia(ExtractSignificantTrivia(semicolonToken.LeadingTrivia));

        if (expression is ThrowExpressionSyntax throwExpression)
        {
            return SyntaxFactory.ThrowStatement(throwExpression.ThrowKeyword, throwExpression.Expression, newSemicolonToken);
        }

        if (statementForm == ExpressionBodyStatementForm.ExpressionStatement)
        {
            return SyntaxFactory.ExpressionStatement(expression, newSemicolonToken);
        }

        return SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword), expression, newSemicolonToken);
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
        var hasSignificantTrivia = false;

        foreach (var trivia in triviaList)
        {
            if (SyntaxTriviaUtilities.IsCommentTrivia(trivia) || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia))
            {
                hasSignificantTrivia = true;

                break;
            }
        }

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

        return LineBreakTriviaUtilities.StripTrailingWhitespace(SyntaxFactory.TriviaList(significantTrivia));
    }

    #endregion // Methods
}