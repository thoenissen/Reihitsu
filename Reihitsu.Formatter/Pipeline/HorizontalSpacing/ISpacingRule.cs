using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// A single horizontal spacing rule. Each rule decides the desired number of spaces between two
/// adjacent tokens on the same line, or returns <see langword="null"/> when it does not apply so the
/// next rule in precedence order may decide. Rules carry policy only — applying the decided spacing
/// is the responsibility of <see cref="TrailingWhitespaceWriter"/>
/// </summary>
internal interface ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines the desired number of spaces after <paramref name="left"/> and before
    /// <paramref name="right"/>
    /// </summary>
    /// <param name="left">The current token</param>
    /// <param name="right">The next token on the same line</param>
    /// <returns>The desired space count, or <see langword="null"/> if this rule does not apply</returns>
    int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right);

    #endregion // Methods
}