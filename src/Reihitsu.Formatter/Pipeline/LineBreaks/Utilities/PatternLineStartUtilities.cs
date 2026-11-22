using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

/// <summary>
/// The one statement of the pattern tokens FMT-BREAK-022 starts on a line of their own and that no join behind <c>is</c>,
/// <c>not</c>, or a pattern combinator undoes (FMT-BREAK-017, FMT-BREAK-041): the opening brace of a property pattern without a
/// type, and the opening parenthesis of a parenthesized pattern, each spanning several lines
/// </summary>
internal static class PatternLineStartUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a token is the first token of a pattern that keeps its own line below the token in front of it
    /// </summary>
    /// <param name="token">The token behind the gap</param>
    /// <returns><see langword="true"/> if the token keeps its own line; otherwise, <see langword="false"/></returns>
    public static bool StartsOwnLine(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.OpenBraceToken => token.Parent is PropertyPatternClauseSyntax { Parent: RecursivePatternSyntax { Type: null, PositionalPatternClause: null } } clause
                                                && LineBreakTriviaUtilities.SpansMultipleLines(clause.OpenBraceToken, clause.CloseBraceToken),
                   SyntaxKind.OpenParenToken => token.Parent is ParenthesizedPatternSyntax parenthesized
                                                && LineBreakTriviaUtilities.SpansMultipleLines(parenthesized.OpenParenToken, parenthesized.CloseParenToken),
                   _ => false
               };
    }

    #endregion // Methods
}