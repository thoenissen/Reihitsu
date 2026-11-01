using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Utilities;

/// <summary>
/// The policy half of the horizontal spacing phase. Evaluates an explicitly ordered list of
/// <see cref="ISpacingRule"/> objects and returns the first non-<see langword="null"/> decision,
/// so rule precedence is expressed by the order of the list rather than by the early-return order of
/// a predicate ladder. Deciding spacing is its only concern — applying it belongs to
/// <see cref="TrailingWhitespaceWriter"/>
/// </summary>
internal static class SpacingPolicy
{
    #region Fields

    /// <summary>
    /// The spacing rules in precedence order (FMT-SPACE-002). The first rule that returns a value wins: attribute close
    /// bracket → no-space → comma → single-space → operator → for-loop semicolon → attribute-target colon → keyword →
    /// arrow → conditional operator → name colon → <c>when</c> → declared name → accessor list → relational pattern →
    /// modifier and prefix keyword → <c>with</c> → lambda return type → function-pointer calling convention
    /// </summary>
    private static readonly ISpacingRule[] _rules = [
                                                        new AttributeListCloseBracketSpacingRule(),
                                                        new NoSpaceSpacingRule(),
                                                        new CommaSpacingRule(),
                                                        new SingleSpaceSpacingRule(),
                                                        new OperatorSpacingRule(),
                                                        new ForLoopSemicolonSpacingRule(),
                                                        new AttributeTargetColonSpacingRule(),
                                                        new KeywordSpacingRule(),
                                                        new ArrowSpacingRule(),
                                                        new ConditionalOperatorSpacingRule(),
                                                        new NameColonSpacingRule(),
                                                        new WhenSpacingRule(),
                                                        new DeclaredNameSpacingRule(),
                                                        new AccessorListSpacingRule(),
                                                        new RelationalPatternSpacingRule(),
                                                        new PrefixKeywordSpacingRule(),
                                                        new WithSpacingRule(),
                                                        new LambdaReturnTypeSpacingRule(),
                                                        new FunctionPointerCallingConventionSpacingRule()
                                                    ];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Determines the desired number of spaces after the current token and before the next token,
    /// based on horizontal spacing rules. Returns <see langword="null"/> if no specific
    /// rule applies, in which case only the collapse-multiple-spaces rule is used
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token on the same line</param>
    /// <returns>The desired space count, or <see langword="null"/> if only the collapse-multiple-spaces rule applies</returns>
    public static int? GetDesiredSpacesAfter(SyntaxToken current, SyntaxToken next)
    {
        foreach (var rule in _rules)
        {
            var desiredSpaces = rule.DesiredSpacesAfter(current, next);

            if (desiredSpaces.HasValue)
            {
                return desiredSpaces;
            }
        }

        return null;
    }

    #endregion // Methods
}