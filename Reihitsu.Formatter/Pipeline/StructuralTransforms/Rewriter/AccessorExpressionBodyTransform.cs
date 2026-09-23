using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Converts block-bodied <c>get</c>, <c>set</c>, and <c>init</c> accessors whose body consists of exactly one
/// convertible statement into expression-bodied accessors. A getter's <c>return e;</c> becomes <c>=> e;</c>, a
/// setter's or initializer's expression statement <c>e;</c> becomes <c>=> e;</c>, and a <c>throw e;</c> in any of
/// them becomes the throw expression <c>=> throw e;</c>. The accessor keeps its attributes, modifiers, and position
/// in the accessor list. The block is kept whenever a comment, directive, or disabled text sits in trivia the
/// rewrite would delete or join onto another line, because moving it would change its position relative to the code
/// </summary>
internal sealed class AccessorExpressionBodyTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public AccessorExpressionBodyTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines the expression that replaces the accessor's single statement, together with the tokens the
    /// rewrite deletes around it
    /// </summary>
    /// <param name="accessor">The accessor to inspect</param>
    /// <param name="statement">The accessor body's only statement</param>
    /// <param name="expression">The expression that becomes the accessor's expression body</param>
    /// <param name="semicolonToken">The statement's semicolon, which the accessor's new semicolon replaces</param>
    /// <returns><see langword="true"/> if the statement has a convertible shape for this accessor kind; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Only shapes whose meaning an expression body keeps qualify. A getter must produce its value, so it accepts a
    /// <c>return</c> with a value (never <c>return throw …;</c>, which does not compile) or a <c>throw</c> with an
    /// operand. A setter or initializer produces no value, so it accepts an expression statement or a <c>throw</c>
    /// with an operand. A rethrow without an operand has no throw-expression form, and every other statement kind —
    /// <c>yield return</c>, declarations, compound statements — has no expression form at all
    /// </remarks>
    private static bool TryGetConvertibleExpression(AccessorDeclarationSyntax accessor,
                                                    StatementSyntax statement,
                                                    out ExpressionSyntax expression,
                                                    out SyntaxToken semicolonToken)
    {
        expression = null;
        semicolonToken = default;

        if (statement.AttributeLists.Count > 0)
        {
            return false;
        }

        var isGetter = accessor.IsKind(SyntaxKind.GetAccessorDeclaration);

        switch (statement)
        {
            case ThrowStatementSyntax { Expression: not null } throwStatement:
                {
                    expression = SyntaxFactory.ThrowExpression(throwStatement.ThrowKeyword, throwStatement.Expression);
                    semicolonToken = throwStatement.SemicolonToken;
                }
                break;

            case ReturnStatementSyntax { Expression: not null and not ThrowExpressionSyntax } returnStatement when isGetter:
                {
                    if (IsLayoutOnly(returnStatement.ReturnKeyword.TrailingTrivia) == false)
                    {
                        return false;
                    }

                    expression = returnStatement.Expression;
                    semicolonToken = returnStatement.SemicolonToken;
                }
                break;

            case ExpressionStatementSyntax expressionStatement when isGetter == false:
                {
                    expression = expressionStatement.Expression;
                    semicolonToken = expressionStatement.SemicolonToken;
                }
                break;

            default:
                {
                    return false;
                }
        }

        return semicolonToken.IsMissing == false;
    }

    /// <summary>
    /// Determines whether every trivia in the list is plain layout — whitespace, an elastic marker, or a line
    /// break — so the rewrite may drop it
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list holds nothing but layout trivia; otherwise, <see langword="false"/></returns>
    private static bool IsLayoutOnly(SyntaxTriviaList triviaList)
    {
        return triviaList.All(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                                               || trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    /// <summary>
    /// Determines whether the trivia list carries a comment
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list contains a comment; otherwise, <see langword="false"/></returns>
    private static bool ContainsComment(SyntaxTriviaList triviaList)
    {
        return triviaList.Any(SyntaxTriviaUtilities.IsCommentTrivia);
    }

    /// <summary>
    /// Removes line breaks and the whitespace that trails the remaining trivia, so trivia that ended a line can
    /// be re-hosted in the middle of the converted accessor's line
    /// </summary>
    /// <param name="triviaList">The trivia list to clean</param>
    /// <returns>The trivia without line breaks and trailing whitespace</returns>
    private static SyntaxTriviaList RemoveLineBreaks(SyntaxTriviaList triviaList)
    {
        var withoutLineBreaks = SyntaxFactory.TriviaList(triviaList.Where(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false));

        return LineBreakTriviaUtilities.StripTrailingWhitespace(withoutLineBreaks);
    }

    /// <summary>
    /// Determines whether the trivia the rewrite deletes or joins onto the accessor keyword's line is plain layout.
    /// These seams are the accessor keyword's trailing trivia, both sides of the opening brace, the statement's
    /// leading trivia, the gap after <c>return</c>, the statement semicolon's leading trivia, and the closing
    /// brace's leading trivia. A comment, directive, or disabled text in any of them would either be deleted with
    /// its token or end up in a different position relative to the code
    /// </summary>
    /// <param name="accessor">The accessor to inspect</param>
    /// <param name="statement">The accessor body's only statement</param>
    /// <param name="expression">The expression that becomes the accessor's expression body</param>
    /// <param name="semicolonToken">The statement's semicolon</param>
    /// <returns><see langword="true"/> if every seam holds only layout trivia; otherwise, <see langword="false"/></returns>
    private static bool HasLayoutOnlySeams(AccessorDeclarationSyntax accessor,
                                           StatementSyntax statement,
                                           ExpressionSyntax expression,
                                           SyntaxToken semicolonToken)
    {
        var body = accessor.Body;

        return IsLayoutOnly(accessor.Keyword.TrailingTrivia)
               && IsLayoutOnly(body.OpenBraceToken.LeadingTrivia)
               && IsLayoutOnly(body.OpenBraceToken.TrailingTrivia)
               && IsLayoutOnly(statement.GetFirstToken().LeadingTrivia)
               && IsLayoutOnly(expression.GetFirstToken().LeadingTrivia)
               && IsLayoutOnly(semicolonToken.LeadingTrivia)
               && IsLayoutOnly(body.CloseBraceToken.LeadingTrivia);
    }

    /// <summary>
    /// Determines whether a directive inside the accessor body would be split or reconstructed by the rewrite.
    /// A conditional group whose partner lies outside the body is orphaned once the braces disappear, and a
    /// region directive cannot keep both endpoints in place while the block is rebuilt. These are the hazards
    /// <see cref="ExpressionBodyRewriteUtilities.BlocksRewrite"/> refuses for the inverse rewrite. A balanced
    /// conditional group or other directive wholly inside the expression travels with it unchanged
    /// </summary>
    /// <param name="accessor">The accessor to inspect</param>
    /// <returns><see langword="true"/> if the body carries such a directive; otherwise, <see langword="false"/></returns>
    private static bool ContainsBlockingDirective(AccessorDeclarationSyntax accessor)
    {
        return SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(accessor, accessor.Body.Span)
               || SyntaxTriviaUtilities.ContainsRegionDirectives(accessor, accessor.Body.FullSpan);
    }

    /// <summary>
    /// Builds the trailing trivia of the converted accessor's semicolon from the trivia that trailed the
    /// statement's semicolon and the body's closing brace. A comment that trailed either of them follows the new
    /// semicolon, and the closing brace's own trailing trivia still ends the line
    /// </summary>
    /// <param name="semicolonToken">The statement's semicolon</param>
    /// <param name="closeBraceToken">The body's closing brace</param>
    /// <param name="trailingTrivia">The trailing trivia for the converted accessor's semicolon</param>
    /// <returns><see langword="true"/> if the trivia can be transferred without losing or misplacing a comment; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// When both carry a comment, the two cannot share one semicolon without reordering them relative to the
    /// line structure the author wrote, so the block is kept. A single-line comment behind the statement's
    /// semicolon would swallow whatever follows it, so it only transfers when the closing brace ended its line
    /// </remarks>
    private static bool TryBuildSemicolonTrailingTrivia(SyntaxToken semicolonToken,
                                                        SyntaxToken closeBraceToken,
                                                        out SyntaxTriviaList trailingTrivia)
    {
        trailingTrivia = closeBraceToken.TrailingTrivia;

        if (ContainsComment(semicolonToken.TrailingTrivia) == false)
        {
            return true;
        }

        if (ContainsComment(closeBraceToken.TrailingTrivia))
        {
            return false;
        }

        if (semicolonToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            && closeBraceToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)) == false)
        {
            return false;
        }

        trailingTrivia = RemoveLineBreaks(semicolonToken.TrailingTrivia).AddRange(closeBraceToken.TrailingTrivia);

        return true;
    }

    /// <summary>
    /// Converts the accessor's body to an expression body when its statement shape and trivia allow it
    /// </summary>
    /// <param name="accessor">The accessor to convert</param>
    /// <returns>The expression-bodied accessor, or the unchanged accessor when the conversion is refused</returns>
    private static AccessorDeclarationSyntax ConvertToExpressionBody(AccessorDeclarationSyntax accessor)
    {
        if (accessor.Kind() is not (SyntaxKind.GetAccessorDeclaration or SyntaxKind.SetAccessorDeclaration or SyntaxKind.InitAccessorDeclaration))
        {
            return accessor;
        }

        var body = accessor.Body;

        if (body == null
            || accessor.ExpressionBody != null
            || body.Statements.Count != 1
            || body.CloseBraceToken.IsMissing)
        {
            return accessor;
        }

        var statement = body.Statements[0];

        if (TryGetConvertibleExpression(accessor, statement, out var expression, out var semicolonToken) == false
            || HasLayoutOnlySeams(accessor, statement, expression, semicolonToken) == false
            || ContainsBlockingDirective(accessor))
        {
            return accessor;
        }

        var expressionTail = expression.GetLastToken().TrailingTrivia;

        if (expressionTail.Any(static trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            || TryBuildSemicolonTrailingTrivia(semicolonToken, body.CloseBraceToken, out var semicolonTrailingTrivia) == false)
        {
            return accessor;
        }

        var bodyExpression = expression.WithoutLeadingTrivia()
                                       .WithTrailingTrivia(RemoveLineBreaks(expressionTail));
        var arrowExpressionClause = SyntaxFactory.ArrowExpressionClause(SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken).WithTrailingTrivia(SyntaxFactory.Space),
                                                                        bodyExpression);

        return accessor.WithKeyword(accessor.Keyword.WithTrailingTrivia(SyntaxFactory.Space))
                       .WithBody(null)
                       .WithExpressionBody(arrowExpressionClause)
                       .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken).WithTrailingTrivia(semicolonTrailingTrivia));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AccessorDeclarationSyntax)base.VisitAccessorDeclaration(node);

        return ConvertToExpressionBody(node);
    }

    #endregion // CSharpSyntaxVisitor
}