using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// The one statement of the gaps of an attached part that the line-break phase joins (FMT-BREAK-039, FMT-BREAK-040) and
/// that the precheck refuses when they hold a comment, a directive, or disabled text (FMT-UNSUP-014): the gap in front of
/// the first token of an argument list, an element access, a conditional element access, or a null-forgiving operator,
/// and the gap between the <c>?</c> and the <c>[</c> of a conditional element access. Each gap is named by the token
/// behind it and decided by the syntax tree alone
/// </summary>
internal static class AttachedPartGapUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the gap in front of a token is a gap of an attached part: the token is the first token of an
    /// attached part (<see cref="StartsAttachedPart"/>), or the <c>[</c> behind the <c>?</c> of a conditional element
    /// access
    /// </summary>
    /// <param name="token">The token behind the gap</param>
    /// <returns><see langword="true"/> if the gap in front of the token is a gap of an attached part; otherwise, <see langword="false"/></returns>
    internal static bool IsAttachedPartGap(SyntaxToken token)
    {
        return StartsAttachedPart(token)
               || IsElementBindingOpenBracket(token);
    }

    /// <summary>
    /// Determines whether a token is the first token of an attached part whose gap in front FMT-BREAK-039 or
    /// FMT-BREAK-040 joins: the <c>(</c> of the argument list of an invocation, an object creation, an implicit object
    /// creation, a constructor initializer, a primary-constructor base type, or a collection expression's <c>with(…)</c>
    /// element; the <c>[</c> of an element access; the <c>?</c> of a conditional element access; or the <c>!</c> of a
    /// null-forgiving operator that is not the first token of a link operator (<c>!.</c>, <c>!?.</c>)
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token starts such a part; otherwise, <see langword="false"/></returns>
    internal static bool StartsAttachedPart(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.OpenParenToken => token.Parent is ArgumentListSyntax
                                                                {
                                                                    Parent: InvocationExpressionSyntax
                                                                            or ObjectCreationExpressionSyntax
                                                                            or ImplicitObjectCreationExpressionSyntax
                                                                            or ConstructorInitializerSyntax
                                                                            or PrimaryConstructorBaseTypeSyntax
                                                                            or WithElementSyntax
                                                                },
                   SyntaxKind.OpenBracketToken => token.Parent is BracketedArgumentListSyntax { Parent: ElementAccessExpressionSyntax },
                   SyntaxKind.QuestionToken => IsConditionalAccessOperator(token)
                                               && IsElementBindingOpenBracket(token.GetNextToken()),
                   SyntaxKind.ExclamationToken => token.Parent.IsKind(SyntaxKind.SuppressNullableWarningExpression)
                                                  && StartsLinkOperator(token) == false,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether a token is the opening bracket of an element binding, which stands behind the <c>?</c> of a
    /// conditional element access
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token opens an element binding's argument list; otherwise, <see langword="false"/></returns>
    private static bool IsElementBindingOpenBracket(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.OpenBracketToken)
               && token.Parent is BracketedArgumentListSyntax { Parent: ElementBindingExpressionSyntax };
    }

    /// <summary>
    /// Determines whether a token is the <c>?</c> of a conditional access
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token is a conditional access's operator; otherwise, <see langword="false"/></returns>
    private static bool IsConditionalAccessOperator(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.QuestionToken)
               && token.Parent is ConditionalAccessExpressionSyntax conditionalAccess
               && conditionalAccess.OperatorToken == token;
    }

    /// <summary>
    /// Determines whether the <c>!</c> of a null-forgiving operator is the first token of a link operator: a <c>.</c>
    /// of a member access, or the <c>?.</c> of a conditional member access, follows it (SHARED-CHAIN-001)
    /// </summary>
    /// <param name="token">The <c>!</c></param>
    /// <returns><see langword="true"/> if the <c>!</c> starts a link operator; otherwise, <see langword="false"/></returns>
    private static bool StartsLinkOperator(SyntaxToken token)
    {
        var nextToken = token.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.DotToken))
        {
            return nextToken.Parent is MemberAccessExpressionSyntax;
        }

        return IsConditionalAccessOperator(nextToken)
               && nextToken.GetNextToken().IsKind(SyntaxKind.DotToken);
    }

    #endregion // Methods
}