using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// The policy half of the horizontal spacing phase. Evaluates an explicitly ordered list of
/// <see cref="ISpacingRule"/> objects and returns the first non-<see langword="null"/> decision,
/// so rule precedence is expressed by the order of the list rather than by the early-return order of
/// a predicate ladder. Deciding spacing is its only concern — applying it belongs to
/// <see cref="TrailingWhitespaceWriter"/>
/// </summary>
internal sealed class SpacingPolicy
{
    #region Fields

    /// <summary>
    /// The spacing rules in precedence order. The first rule that returns a value wins. The order
    /// reproduces the original ladder exactly: attribute close bracket → no-space → comma →
    /// single-space → operator → for-loop semicolon → keyword
    /// </summary>
    private static readonly ISpacingRule[] _rules = new ISpacingRule[]
                                                    {
                                                        new AttributeListCloseBracketSpacingRule(),
                                                        new NoSpaceSpacingRule(),
                                                        new CommaSpacingRule(),
                                                        new SingleSpaceSpacingRule(),
                                                        new OperatorSpacingRule(),
                                                        new ForLoopSemicolonSpacingRule(),
                                                        new KeywordSpacingRule()
                                                    };

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
    public int? GetDesiredSpacesAfter(SyntaxToken current, SyntaxToken next)
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