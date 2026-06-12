using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Enumerations;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Builds the block body (and the indexer's get-accessor-list variant) that replaces an
/// expression body, centralizing the brace construction shared by every expression-bodied
/// member transform. The member-specific decisions (statement form, which trivia carriers
/// to pass) remain in the individual transforms
/// </summary>
internal sealed class ExpressionBodyToBlockConverter
{
    #region Methods

    /// <summary>
    /// Creates a block body that wraps the given expression
    /// </summary>
    /// <param name="expression">The arrow expression to wrap</param>
    /// <param name="statementForm">The statement form the expression should take</param>
    /// <param name="openBraceLeadingTrivia">Leading trivia carried onto the open brace (typically the arrow's leading trivia)</param>
    /// <param name="closeBraceTrailingTrivia">Trailing trivia carried onto the close brace (typically the semicolon's trailing trivia)</param>
    /// <returns>The block body</returns>
    internal BlockSyntax CreateBlock(ExpressionSyntax expression,
                                     ExpressionBodyStatementForm statementForm,
                                     SyntaxTriviaList openBraceLeadingTrivia,
                                     SyntaxTriviaList closeBraceTrailingTrivia)
    {
        var statement = CreateStatement(expression, statementForm);

        return SyntaxFactory.Block(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(openBraceLeadingTrivia),
                                   SyntaxFactory.SingletonList(statement),
                                   SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(closeBraceTrailingTrivia));
    }

    /// <summary>
    /// Creates a get-accessor list whose single getter returns the given expression
    /// </summary>
    /// <param name="expression">The arrow expression to return</param>
    /// <param name="openBraceLeadingTrivia">Leading trivia carried onto the accessor-list open brace (typically the arrow's leading trivia)</param>
    /// <param name="closeBraceTrailingTrivia">Trailing trivia carried onto the accessor-list close brace (typically the semicolon's trailing trivia)</param>
    /// <returns>The accessor list with a single get accessor</returns>
    internal AccessorListSyntax CreateGetAccessorList(ExpressionSyntax expression,
                                                      SyntaxTriviaList openBraceLeadingTrivia,
                                                      SyntaxTriviaList closeBraceTrailingTrivia)
    {
        var returnStatement = CreateStatement(expression, ExpressionBodyStatementForm.ReturnStatement);

        var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                  .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList(returnStatement)));

        return SyntaxFactory.AccessorList(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(openBraceLeadingTrivia),
                                          SyntaxFactory.SingletonList(getter),
                                          SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(closeBraceTrailingTrivia));
    }

    /// <summary>
    /// Wraps the expression in the requested statement form. A <see cref="ThrowExpressionSyntax"/>
    /// always becomes a <see cref="ThrowStatementSyntax"/> regardless of the requested form, because
    /// <c>return throw ...;</c> does not compile (CS8115)
    /// </summary>
    /// <param name="expression">The expression to wrap</param>
    /// <param name="statementForm">The statement form</param>
    /// <returns>The wrapping statement</returns>
    private static StatementSyntax CreateStatement(ExpressionSyntax expression, ExpressionBodyStatementForm statementForm)
    {
        if (expression is ThrowExpressionSyntax throwExpression)
        {
            return SyntaxFactory.ThrowStatement(throwExpression.ThrowKeyword,
                                                throwExpression.Expression,
                                                SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        if (statementForm == ExpressionBodyStatementForm.ExpressionStatement)
        {
            return SyntaxFactory.ExpressionStatement(expression);
        }

        return SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                                             expression,
                                             SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    #endregion // Methods
}