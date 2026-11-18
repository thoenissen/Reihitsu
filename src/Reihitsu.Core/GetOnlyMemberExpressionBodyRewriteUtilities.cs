using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared decision whether a get-only property or indexer is replaced with a member-level expression body, and the
/// conversion itself, together with the conversion of block-bodied accessors to expression-bodied accessors that a
/// block-bodied getter qualifies under. The formatter's accessor-level and get-only structural transforms, the analyzers
/// that report get-only members, and their code fixes all use it, so the analyzers report exactly the members whose shape
/// the formatter converts, and the fixes write what the formatter writes
/// </summary>
/// <remarks>
/// The shape of a get-only member decides whether it is reported: an accessor list whose only accessor is a <c>get</c>
/// accessor without attributes and modifiers, expression-bodied or with a block of one convertible statement, on a member
/// without an initializer, from C# 6 on and from C# 7 on for a getter that throws. The trivia decides whether the conversion
/// can be made: it is refused whenever a comment, directive, or disabled text sits in trivia the rewrite would delete or join
/// onto another line; a comment trailing the getter's semicolon or the accessor list's closing brace already ends the
/// member's last line and follows the new semicolon instead
/// </remarks>
public static class GetOnlyMemberExpressionBodyRewriteUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a property or indexer has the shape of a get-only member the formatter converts to an expression
    /// body, regardless of the comments, directives, and disabled text that may keep the formatter from converting it
    /// </summary>
    /// <param name="member">The property or indexer declaration</param>
    /// <param name="languageVersion">The language version the member is compiled with</param>
    /// <returns><see langword="true"/> if the member has a convertible get-only shape; otherwise, <see langword="false"/></returns>
    public static bool HasConvertibleShape(BasePropertyDeclarationSyntax member, LanguageVersion languageVersion)
    {
        return TryGetGetter(member, out _, out var getter)
               && TryGetGetterExpression(getter, out var expression)
               && IsAvailable(expression, languageVersion);
    }

    /// <summary>
    /// Replaces the accessor list of a get-only property or indexer with a member-level expression body, so
    /// <c>{ get => e; }</c> and <c>{ get { return e; } }</c> both become <c>=> e;</c>
    /// </summary>
    /// <param name="member">The property or indexer declaration</param>
    /// <param name="languageVersion">The language version the member is compiled with</param>
    /// <param name="endOfLine">The end-of-line sequence of the document</param>
    /// <param name="convertedMember">The expression-bodied member</param>
    /// <returns><see langword="true"/> if the member is converted; <see langword="false"/> when its shape or its trivia keeps the accessor list</returns>
    /// <remarks>
    /// The created arrow gets no whitespace on either side, and the layout trivia between the last token in front of the
    /// accessor list and its opening brace is removed with the list; the formatter's spacing phase spaces the arrow like every
    /// arrow (FMT-STRUCT-026, FMT-SPACE-017). A block-bodied getter qualifies exactly when
    /// <see cref="ConvertAccessorToExpressionBody"/> converts it, so it is converted under the shape, seam, and directive
    /// rules of the accessor-level conversion
    /// </remarks>
    public static bool TryConvertToExpressionBody(BasePropertyDeclarationSyntax member,
                                                  LanguageVersion languageVersion,
                                                  string endOfLine,
                                                  out BasePropertyDeclarationSyntax convertedMember)
    {
        convertedMember = null;

        if (TryGetGetter(member, out var accessorList, out var getter) == false)
        {
            return false;
        }

        if (getter.Body != null)
        {
            getter = ConvertAccessorToExpressionBody(getter, endOfLine);
        }

        if (getter.ExpressionBody == null
            || getter.SemicolonToken.IsMissing
            || IsAvailable(getter.ExpressionBody.Expression, languageVersion) == false)
        {
            return false;
        }

        if (HasLayoutOnlySeams(accessorList, getter) == false
            || ContainsBlockingDirective(member, accessorList)
            || TryRehostExpression(getter.ExpressionBody.Expression, out var bodyExpression) == false
            || TryBuildSemicolonTrailingTrivia(getter.SemicolonToken, accessorList.CloseBraceToken, endOfLine, out var semicolonTrailingTrivia) == false)
        {
            return false;
        }

        // The created tokens are built with explicit trivia, so they carry no elastic trivia (FMT-STRUCT-026)
        var arrowExpressionClause = SyntaxFactory.ArrowExpressionClause(SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.EqualsGreaterThanToken, SyntaxFactory.TriviaList()),
                                                                        bodyExpression);
        var semicolonToken = SyntaxFactory.Token(default, SyntaxKind.SemicolonToken, semicolonTrailingTrivia);
        var signatureEnd = accessorList.OpenBraceToken.GetPreviousToken();
        var memberWithoutGap = member.ReplaceToken(signatureEnd, signatureEnd.WithTrailingTrivia(SyntaxFactory.TriviaList()));

        convertedMember = memberWithoutGap switch
                          {
                              PropertyDeclarationSyntax property => property.WithAccessorList(null)
                                                                            .WithExpressionBody(arrowExpressionClause)
                                                                            .WithSemicolonToken(semicolonToken),
                              IndexerDeclarationSyntax indexer => indexer.WithAccessorList(null)
                                                                         .WithExpressionBody(arrowExpressionClause)
                                                                         .WithSemicolonToken(semicolonToken),
                              _ => null
                          };

        return convertedMember != null;
    }

    /// <summary>
    /// Converts a block-bodied <c>get</c>, <c>set</c>, or <c>init</c> accessor whose body consists of exactly one convertible
    /// statement into an expression-bodied accessor. A getter's <c>return e;</c> becomes <c>=> e;</c>, a setter's or
    /// initializer's expression statement <c>e;</c> becomes <c>=> e;</c>, and a <c>throw e;</c> in any of them becomes the
    /// throw expression <c>=> throw e;</c>, with <c>e</c> parenthesized when it binds more loosely than a throw expression's
    /// operand. The accessor keeps its attributes, modifiers, and position in the accessor list
    /// </summary>
    /// <param name="accessor">The accessor to convert</param>
    /// <param name="endOfLine">The end-of-line sequence of the document</param>
    /// <returns>The expression-bodied accessor, or the unchanged accessor when the conversion is refused</returns>
    /// <remarks>
    /// The block is kept whenever a comment, directive, or disabled text sits in trivia the rewrite would delete or join onto
    /// another line, because moving it would change its position relative to the code. The one exception is a comment
    /// trailing the statement's semicolon or the closing brace: it already ends the accessor's last line and follows the new
    /// semicolon instead. The created arrow gets no whitespace on either side; the formatter's spacing phase spaces it like
    /// every arrow (FMT-STRUCT-026, FMT-SPACE-017), and the layout trivia that stood between the keyword and the removed body
    /// is dropped
    /// </remarks>
    public static AccessorDeclarationSyntax ConvertAccessorToExpressionBody(AccessorDeclarationSyntax accessor, string endOfLine)
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

        // The created tokens are built with explicit trivia, so they carry no elastic trivia (FMT-STRUCT-026)
        var arrowExpressionClause = SyntaxFactory.ArrowExpressionClause(SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.EqualsGreaterThanToken, SyntaxFactory.TriviaList()),
                                                                        bodyExpression);

        return accessor.WithKeyword(accessor.Keyword.WithTrailingTrivia(default(SyntaxTriviaList)))
                       .WithBody(null)
                       .WithExpressionBody(arrowExpressionClause)
                       .WithSemicolonToken(SyntaxFactory.Token(default, SyntaxKind.SemicolonToken, semicolonTrailingTrivia));
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
    public static bool ContainsBlockingDirective(SyntaxNode owner, SyntaxNode bracedNode)
    {
        return SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(owner, bracedNode.Span)
               || SyntaxTriviaUtilities.ContainsRegionDirectives(owner, bracedNode.FullSpan);
    }

    /// <summary>
    /// Gets the accessor list and its only accessor of a property or indexer whose accessor list holds nothing but a plain
    /// getter
    /// </summary>
    /// <param name="member">The property or indexer declaration</param>
    /// <param name="accessorList">The member's accessor list</param>
    /// <param name="getter">The list's only accessor</param>
    /// <returns><see langword="true"/> if the member has such an accessor list; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// An initializer, a second accessor, an attribute, or a modifier has no place on an expression-bodied member: an
    /// initializer or a second accessor cannot be written there, moving an attribute would change its target, and dropping a
    /// modifier would lose a token
    /// </remarks>
    private static bool TryGetGetter(BasePropertyDeclarationSyntax member, out AccessorListSyntax accessorList, out AccessorDeclarationSyntax getter)
    {
        accessorList = null;
        getter = null;

        if (member is not (PropertyDeclarationSyntax { Initializer: null } or IndexerDeclarationSyntax)
            || member.AccessorList is not { } list
            || list.OpenBraceToken.IsMissing
            || list.CloseBraceToken.IsMissing
            || list.Accessors.Count != 1)
        {
            return false;
        }

        var accessor = list.Accessors[0];

        if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration) == false
            || accessor.AttributeLists.Count > 0
            || accessor.Modifiers.Count > 0)
        {
            return false;
        }

        accessorList = list;
        getter = accessor;

        return true;
    }

    /// <summary>
    /// Gets the expression a getter's expression body would hold, judged by the statement shape alone
    /// </summary>
    /// <param name="getter">The getter</param>
    /// <param name="expression">The expression of its expression body, or the expression its single statement converts to</param>
    /// <returns><see langword="true"/> if the getter is expression-bodied or its block holds a single convertible statement; otherwise, <see langword="false"/></returns>
    private static bool TryGetGetterExpression(AccessorDeclarationSyntax getter, out ExpressionSyntax expression)
    {
        expression = null;

        if (getter.ExpressionBody != null)
        {
            expression = getter.ExpressionBody.Expression;

            return getter.Body == null
                   && getter.SemicolonToken.IsMissing == false;
        }

        return getter.Body is { Statements.Count: 1, CloseBraceToken.IsMissing: false } body
               && TryGetConvertibleExpression(getter, body.Statements[0], out expression, out _);
    }

    /// <summary>
    /// Determines whether the language version allows an expression-bodied member with the given expression
    /// </summary>
    /// <param name="expression">The expression of the expression body</param>
    /// <param name="languageVersion">The language version the member is compiled with</param>
    /// <returns><see langword="true"/> if the expression-bodied member can be written; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Expression-bodied properties and indexers exist from C# 6 on, and throw expressions from C# 7 on. The formatter passes
    /// its context's language version rather than the node's parse options, because a node that an earlier rewriter replaced
    /// belongs to a new tree whose options fall back to the defaults
    /// </remarks>
    private static bool IsAvailable(ExpressionSyntax expression, LanguageVersion languageVersion)
    {
        return languageVersion >= LanguageVersion.CSharp6
               && (expression is not ThrowExpressionSyntax || languageVersion >= LanguageVersion.CSharp7);
    }

    /// <summary>
    /// Determines whether every trivia in the list is plain layout — whitespace, an elastic marker, or a line
    /// break — so the rewrite may drop it
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list holds nothing but layout trivia; otherwise, <see langword="false"/></returns>
    private static bool IsLayoutOnly(SyntaxTriviaList triviaList)
    {
        return SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(triviaList) < 0;
    }

    /// <summary>
    /// Determines whether the trivia the member-level rewrite deletes or joins onto the member signature's line is plain
    /// layout. These seams are the trailing trivia of the token before the accessor list, both sides of the opening brace,
    /// the leading trivia of the <c>get</c> keyword, the getter semicolon's leading trivia, and the closing brace's leading
    /// trivia. Neither gap around the getter's arrow is a seam to check: a getter written with an expression body that holds
    /// a comment, a directive, or disabled text there is refused by the formatter's precheck (FMT-UNSUP-012, FMT-UNSUP-013),
    /// and a getter converted from a block keeps its block when its keyword's trailing trivia holds one (FMT-STRUCT-012) and
    /// gets an arrow with no trivia
    /// </summary>
    /// <param name="accessorList">The accessor list to inspect</param>
    /// <param name="getter">The expression-bodied getter</param>
    /// <returns><see langword="true"/> if every seam holds only layout trivia; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The token before the accessor list lies outside the list. A comment trailing it would leave the arrow on the
    /// next line once the opening brace is gone, so it is inspected together with the list's own seams
    /// </remarks>
    private static bool HasLayoutOnlySeams(AccessorListSyntax accessorList, AccessorDeclarationSyntax getter)
    {
        return IsLayoutOnly(accessorList.OpenBraceToken.GetPreviousToken().TrailingTrivia)
               && IsLayoutOnly(accessorList.OpenBraceToken.LeadingTrivia)
               && IsLayoutOnly(accessorList.OpenBraceToken.TrailingTrivia)
               && IsLayoutOnly(getter.Keyword.LeadingTrivia)
               && IsLayoutOnly(getter.SemicolonToken.LeadingTrivia)
               && IsLayoutOnly(accessorList.CloseBraceToken.LeadingTrivia);
    }

    /// <summary>
    /// Determines whether the trivia the accessor-level rewrite deletes or joins onto the accessor keyword's line is plain
    /// layout. These seams are the accessor keyword's trailing trivia, both sides of the opening brace, the statement's
    /// leading trivia, the gap after <c>return</c> (the keyword's trailing trivia and the returned expression's leading
    /// trivia), the statement semicolon's leading trivia, and the closing brace's leading trivia. A comment, directive, or
    /// disabled text in any of them would either be deleted with its token or end up in a different position relative to
    /// the code
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
                        // The parentheses are built with explicit trivia, so they carry no elastic trivia (FMT-STRUCT-026)
                        operand = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.Token(default, SyntaxKind.OpenParenToken, default),
                                                                        operand.WithoutTrivia(),
                                                                        SyntaxFactory.Token(default, SyntaxKind.CloseParenToken, default))
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
    /// Prepares an expression to become an expression body: its leading trivia is dropped and the trivia that
    /// trailed it is re-hosted without line breaks in front of the new semicolon
    /// </summary>
    /// <param name="expression">The expression that becomes the expression body</param>
    /// <param name="bodyExpression">The expression prepared for the expression body</param>
    /// <returns><see langword="true"/> if the expression can be re-hosted; <see langword="false"/> when a single-line comment trails it, because that comment would swallow the new semicolon</returns>
    private static bool TryRehostExpression(ExpressionSyntax expression, out ExpressionSyntax bodyExpression)
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
    /// Builds the trailing trivia of a new semicolon from the trivia that trailed the semicolon and the closing
    /// brace the rewrite removes — the statement's semicolon and the body's closing brace for an accessor, the
    /// getter's semicolon and the accessor list's closing brace for a get-only member. A comment that trailed either
    /// of them follows the new semicolon. The line is ended by the closing brace's own trailing trivia, or by a new
    /// line break when a single-line comment needs one and the closing brace did not end its line
    /// </summary>
    /// <param name="semicolonToken">The removed semicolon</param>
    /// <param name="closeBraceToken">The removed closing brace</param>
    /// <param name="endOfLine">The end-of-line sequence of the document</param>
    /// <param name="trailingTrivia">The trailing trivia for the new semicolon</param>
    /// <returns><see langword="true"/> if the trivia can be transferred without losing or misplacing a comment; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// When both carry a comment, the two cannot share one semicolon without reordering them relative to the
    /// line structure the author wrote, so the braced form is kept. A single-line comment behind the removed semicolon
    /// always ends its line, so the new semicolon's trivia ends with a line break even when the closing brace shared
    /// its line with the next token; that keeps the comment from swallowing that token, and it makes the decision
    /// independent of how the accessor list is laid out
    /// </remarks>
    private static bool TryBuildSemicolonTrailingTrivia(SyntaxToken semicolonToken,
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
    /// be re-hosted in the middle of the line of the converted accessor or member
    /// </summary>
    /// <param name="triviaList">The trivia list to clean</param>
    /// <returns>The trivia without line breaks and trailing whitespace</returns>
    private static SyntaxTriviaList RemoveLineBreaks(SyntaxTriviaList triviaList)
    {
        var remaining = triviaList.Where(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
                                  .ToList();

        while (remaining.Count > 0
               && remaining[remaining.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            remaining.RemoveAt(remaining.Count - 1);
        }

        return SyntaxFactory.TriviaList(remaining);
    }

    #endregion // Methods
}