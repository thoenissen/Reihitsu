using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Shared decision for rewriting an expression-bodied member into a block body. The formatter's
/// structural transforms and the analyzers that report these members both consult it, so an analyzer
/// never reports a member whose code fix the formatter would then refuse to rewrite
/// </summary>
public static class ExpressionBodyRewriteUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether an expression-bodied member must keep its expression body
    /// </summary>
    /// <param name="member">The member declaration that owns the expression body</param>
    /// <param name="expressionBody">The expression body to inspect</param>
    /// <param name="semicolonToken">The member's terminating semicolon</param>
    /// <returns><see langword="true"/> if the rewrite must be refused; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Two distinct hazards block the rewrite, and only these two. A directive between the arrow and
    /// the expression is re-hosted directly after the injected <c>return</c> or expression statement,
    /// where a directive cannot legally sit because it must start its own line; that produces source
    /// which no longer parses. A conditional or region group whose partner lies outside the rebuilt
    /// span is split by the rewrite, orphaning the half that stays behind. A directive that sits
    /// between the expression and the semicolon is neither: it travels inside the generated statement
    /// and keeps both its position relative to the surrounding code and its own line.
    /// </remarks>
    public static bool BlocksRewrite(SyntaxNode member,
                                     ArrowExpressionClauseSyntax expressionBody,
                                     SyntaxToken semicolonToken)
    {
        if (member == null || expressionBody?.Expression == null)
        {
            return false;
        }

        var arrowStart = expressionBody.ArrowToken.SpanStart;

        if (SyntaxTriviaUtilities.ContainsDirectives(member, TextSpan.FromBounds(arrowStart, expressionBody.Expression.SpanStart)))
        {
            return true;
        }

        var rewrittenSpan = TextSpan.FromBounds(arrowStart,
                                                semicolonToken.IsKind(SyntaxKind.None) || semicolonToken.IsMissing
                                                    ? expressionBody.Span.End
                                                    : semicolonToken.Span.End);

        return SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(member, rewrittenSpan)
               || SyntaxTriviaUtilities.ContainsUnbalancedRegionDirectives(member, rewrittenSpan);
    }

    #endregion // Methods
}