using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Converts block-bodied <c>get</c>, <c>set</c>, and <c>init</c> accessors whose body consists of exactly one
/// convertible statement into expression-bodied accessors. A getter's <c>return e;</c> becomes <c>=> e;</c>, a
/// setter's or initializer's expression statement <c>e;</c> becomes <c>=> e;</c>, and a <c>throw e;</c> in any of
/// them becomes the throw expression <c>=> throw e;</c>, with <c>e</c> parenthesized when it binds more loosely than a
/// throw expression's operand. The accessor keeps its attributes, modifiers, and position
/// in the accessor list. The block is kept whenever a comment, directive, or disabled text sits in trivia the
/// rewrite would delete or join onto another line, because moving it would change its position relative to the code.
/// The one exception is a comment trailing the statement's semicolon or the closing brace: it already ends the
/// accessor's last line and follows the new semicolon instead. Its shape, seam, directive, and trivia-transfer rules
/// are shared with <see cref="GetOnlyMemberExpressionBodyTransform"/>, which lifts a get-only accessor list to the
/// member level under the same rules
/// </summary>
internal sealed class AccessorExpressionBodyTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public AccessorExpressionBodyTransform(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether every trivia in the list is plain layout — whitespace, an elastic marker, or a line
    /// break — so the rewrite may drop it
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list holds nothing but layout trivia; otherwise, <see langword="false"/></returns>
    internal static bool IsLayoutOnly(SyntaxTriviaList triviaList)
    {
        return SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(triviaList) < 0;
    }

    /// <summary>
    /// Builds the trailing trivia of a new semicolon from the trivia that trailed the semicolon and the closing
    /// brace the rewrite removes — the statement's semicolon and the body's closing brace for an accessor, the
    /// getter's semicolon and the accessor list's closing brace for a get-only member. A comment that trailed either
    /// of them follows the new semicolon. The line is ended by the closing brace's own trailing trivia, or by a new
    /// line break when a single-line comment needs one and the closing brace did not end its line
    /// </summary>
    /// <param name="semicolonToken">The removed semicolon</param>
    /// <param name="closeBraceToken">The removed closing brace</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting run</param>
    /// <param name="trailingTrivia">The trailing trivia for the new semicolon</param>
    /// <returns><see langword="true"/> if the trivia can be transferred without losing or misplacing a comment; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// When both carry a comment, the two cannot share one semicolon without reordering them relative to the
    /// line structure the author wrote, so the braced form is kept. A single-line comment behind the removed semicolon
    /// always ends its line, so the new semicolon's trivia ends with a line break even when the closing brace shared
    /// its line with the next token; that keeps the comment from swallowing that token, and it makes the decision
    /// independent of how the accessor list is laid out
    /// </remarks>
    internal static bool TryBuildSemicolonTrailingTrivia(SyntaxToken semicolonToken,
                                                         SyntaxToken closeBraceToken,
                                                         string endOfLine,
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

        var requiresLineEnd = semicolonToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                              && closeBraceToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)) == false;
        var lineEnd = requiresLineEnd
                          ? SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine))
                          : closeBraceToken.TrailingTrivia;

        trailingTrivia = RemoveLineBreaks(semicolonToken.TrailingTrivia).AddRange(lineEnd);

        return true;
    }

    /// <summary>
    /// Determines whether a directive inside a braced node that the rewrite removes would be split or reconstructed.
    /// A conditional group whose partner lies outside the braces is orphaned once they disappear, and a region
    /// directive cannot keep both endpoints in place while the node is rebuilt. These are the hazards
    /// <see cref="ExpressionBodyRewriteUtilities.BlocksRewrite"/> refuses for the inverse rewrite. A balanced
    /// conditional group or other directive wholly inside the expression travels with it unchanged
    /// </summary>
    /// <param name="owner">The declaration that contains the braced node</param>
    /// <param name="bracedNode">The accessor body or accessor list the rewrite removes</param>
    /// <returns><see langword="true"/> if the braced node carries such a directive; otherwise, <see langword="false"/></returns>
    internal static bool ContainsBlockingDirective(SyntaxNode owner, SyntaxNode bracedNode)
    {
        return SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(owner, bracedNode.Span)
               || SyntaxTriviaUtilities.ContainsRegionDirectives(owner, bracedNode.FullSpan);
    }

    /// <summary>
    /// Prepares an expression to become an expression body: its leading trivia is dropped and the trivia that
    /// trailed it is re-hosted without line breaks in front of the new semicolon
    /// </summary>
    /// <param name="expression">The expression that becomes the expression body</param>
    /// <param name="bodyExpression">The expression prepared for the expression body</param>
    /// <returns><see langword="true"/> if the expression can be re-hosted; <see langword="false"/> when a single-line comment trails it, because that comment would swallow the new semicolon</returns>
    internal static bool TryRehostExpression(ExpressionSyntax expression, out ExpressionSyntax bodyExpression)
    {
        bodyExpression = null;

        var expressionTail = expression.GetLastToken().TrailingTrivia;

        if (expressionTail.Any(static trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)))
        {
            return false;
        }

        bodyExpression = expression.WithoutLeadingTrivia()
                                   .WithTrailingTrivia(RemoveLineBreaks(expressionTail));

        return true;
    }

    /// <summary>
    /// Converts the accessor's body to an expression body when its statement shape and trivia allow it
    /// </summary>
    /// <param name="accessor">The accessor to convert</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting run</param>
    /// <returns>The expression-bodied accessor, or the unchanged accessor when the conversion is refused</returns>
    internal static AccessorDeclarationSyntax ConvertToExpressionBody(AccessorDeclarationSyntax accessor, string endOfLine)
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
            || ContainsBlockingDirective(accessor, accessor.Body))
        {
            return accessor;
        }

        if (TryRehostExpression(expression, out var bodyExpression) == false
            || TryBuildSemicolonTrailingTrivia(semicolonToken, body.CloseBraceToken, endOfLine, out var semicolonTrailingTrivia) == false)
        {
            return accessor;
        }

        var arrowExpressionClause = SyntaxFactory.ArrowExpressionClause(SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken).WithTrailingTrivia(SyntaxFactory.Space),
                                                                        bodyExpression);

        return accessor.WithKeyword(accessor.Keyword.WithTrailingTrivia(SyntaxFactory.Space))
                       .WithBody(null)
                       .WithExpressionBody(arrowExpressionClause)
                       .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken).WithTrailingTrivia(semicolonTrailingTrivia));
    }

    /// <summary>
    /// Removes line breaks and the whitespace that trails the remaining trivia, so trivia that ended a line can
    /// be re-hosted in the middle of the line of the converted accessor or member
    /// </summary>
    /// <param name="triviaList">The trivia list to clean</param>
    /// <returns>The trivia without line breaks and trailing whitespace</returns>
    private static SyntaxTriviaList RemoveLineBreaks(SyntaxTriviaList triviaList)
    {
        var withoutLineBreaks = SyntaxFactory.TriviaList(triviaList.Where(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false));

        return LineBreakTriviaUtilities.StripTrailingWhitespace(withoutLineBreaks);
    }

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
    /// <c>yield return</c>, declarations, compound statements — has no expression form at all. A throw operand that
    /// <see cref="RequiresThrowExpressionParentheses"/> flags is wrapped in parentheses, so the throw expression keeps
    /// the operand the statement had
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
                    var operand = throwStatement.Expression;

                    if (RequiresThrowExpressionParentheses(operand))
                    {
                        operand = SyntaxFactory.ParenthesizedExpression(operand.WithoutTrivia())
                                               .WithTriviaFrom(operand);
                    }

                    expression = SyntaxFactory.ThrowExpression(throwStatement.ThrowKeyword, operand);
                    semicolonToken = throwStatement.SemicolonToken;
                }
                break;

            case ReturnStatementSyntax { Expression: not null and not ThrowExpressionSyntax } returnStatement when isGetter:
                {
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
    /// Determines whether a throw statement's operand needs parentheses to stay the operand once it moves into a
    /// throw expression
    /// </summary>
    /// <param name="operand">The throw statement's operand, as written</param>
    /// <returns><see langword="true"/> if the operand's top-level node binds more loosely than a throw expression's operand; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// A throw statement's operand is a full expression, but a throw expression parses its operand only down to the
    /// null-coalescing level. A bare conditional or assignment would therefore bind the throw expression inside it,
    /// which does not compile, a bare lambda would not parse, and a bare query draws the precedence warning CS8848.
    /// Only the top-level node decides: a looser node nested below a tighter one, or an operand that is already
    /// parenthesized, re-parses unchanged. RH3002 rests on the same parse fact but answers the opposite question —
    /// whether an existing pair around a throw-expression operand may be removed — after unwrapping nested pairs,
    /// and it leaves lambda pairs to its general exclusion; this predicate only decides whether the conversion must
    /// add a pair to the operand as written
    /// </remarks>
    private static bool RequiresThrowExpressionParentheses(ExpressionSyntax operand)
    {
        return operand is ConditionalExpressionSyntax
                       or AssignmentExpressionSyntax
                       or LambdaExpressionSyntax
                       or QueryExpressionSyntax;
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
    /// Determines whether the trivia the rewrite deletes or joins onto the accessor keyword's line is plain layout.
    /// These seams are the accessor keyword's trailing trivia, both sides of the opening brace, the statement's
    /// leading trivia, the gap after <c>return</c> (the keyword's trailing trivia and the returned expression's
    /// leading trivia), the statement semicolon's leading trivia, and the closing brace's leading trivia. A comment,
    /// directive, or disabled text in any of them would either be deleted with its token or end up in a different
    /// position relative to the code
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

        if (statement is ReturnStatementSyntax returnStatement
            && IsLayoutOnly(returnStatement.ReturnKeyword.TrailingTrivia) == false)
        {
            return false;
        }

        return IsLayoutOnly(accessor.Keyword.TrailingTrivia)
               && IsLayoutOnly(body.OpenBraceToken.LeadingTrivia)
               && IsLayoutOnly(body.OpenBraceToken.TrailingTrivia)
               && IsLayoutOnly(statement.GetFirstToken().LeadingTrivia)
               && IsLayoutOnly(expression.GetFirstToken().LeadingTrivia)
               && IsLayoutOnly(semicolonToken.LeadingTrivia)
               && IsLayoutOnly(body.CloseBraceToken.LeadingTrivia);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AccessorDeclarationSyntax)base.VisitAccessorDeclaration(node);

        return ConvertToExpressionBody(node, _context.EndOfLine);
    }

    #endregion // CSharpSyntaxVisitor
}