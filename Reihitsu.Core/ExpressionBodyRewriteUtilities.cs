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
    /// Three hazards block the rewrite. A directive between the arrow and the expression is re-hosted
    /// directly after the injected <c>return</c> or expression statement, where a directive cannot
    /// legally sit because it must start its own line; that produces source which no longer parses.
    /// A conditional group whose partner lies outside the rebuilt span is split by the rewrite,
    /// orphaning the half that stays behind. Any region directive in or immediately before the
    /// rebuilt span blocks it as well, balanced or not, because the region phase reparses changed
    /// text right after the structural transforms and therefore re-lexes the rewritten member before
    /// horizontal spacing has run over it. An ordinary conditional or position sensitive directive
    /// between the expression and the semicolon is none of these: it travels inside the generated
    /// statement and keeps both its position relative to the surrounding code and its own line.
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

        if (SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(member, rewrittenSpan))
        {
            return true;
        }

        // The region phase reparses changed text, so it re-lexes the rewritten member before the
        // spacing phase has run over it. Refuse every region directive that reaches the rewrite,
        // including one opened in the arrow's own leading trivia, which sits outside the span above.
        var regionSpan = TextSpan.FromBounds(expressionBody.ArrowToken.FullSpan.Start, rewrittenSpan.End);

        return SyntaxTriviaUtilities.ContainsRegionDirectives(member, regionSpan);
    }

    #endregion // Methods
}