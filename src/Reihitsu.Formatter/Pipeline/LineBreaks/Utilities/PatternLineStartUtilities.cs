using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data.Enumerations;
using Reihitsu.Formatter.Extensions;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

/// <summary>
/// The one statement of FMT-BREAK-022's case-label condition and of the pattern tokens FMT-BREAK-022 starts on a line of their
/// own, which no join behind <c>is</c>, <c>not</c>, or a pattern combinator undoes (FMT-BREAK-017, FMT-BREAK-041): the opening
/// brace of a property pattern without a type that a <c>case</c> label does not introduce, and the opening parenthesis of a
/// parenthesized pattern, each spanning several lines
/// </summary>
internal static class PatternLineStartUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a pattern is introduced by a <c>case</c> label (FMT-BREAK-022): the label's own pattern, or a pattern
    /// reached from it through the operands of <c>and</c>, <c>or</c>, and <c>not</c>. When the walk reaches a node without a
    /// parent, the formatting root, the facts the entry point captured from the document answer instead (FMT-PIPE-024)
    /// </summary>
    /// <param name="node">The pattern</param>
    /// <param name="rootConstruct">The facts about the construct the node without a parent stands in</param>
    /// <returns><see langword="true"/> if the pattern is introduced by a case label; otherwise, <see langword="false"/></returns>
    public static bool IsIntroducedByCaseLabel(SyntaxNode node, RootConstruct rootConstruct)
    {
        var current = node;

        while (current.Parent is BinaryPatternSyntax or UnaryPatternSyntax)
        {
            current = current.Parent;
        }

        return current.Parent is CasePatternSwitchLabelSyntax
               || rootConstruct.IsRootIn(current, RootConstruct.CaseLabelPattern);
    }

    /// <summary>
    /// Determines whether a token is the first token of a pattern that keeps its own line below the token in front of it
    /// (FMT-BREAK-022): the opening brace of a multi-line property pattern without a type, unless a <c>case</c> label
    /// introduces the pattern (<see cref="IsIntroducedByCaseLabel"/>), or the opening parenthesis of a multi-line
    /// parenthesized pattern
    /// </summary>
    /// <param name="token">The token behind the gap</param>
    /// <param name="rootConstruct">The facts about the construct the node without a parent, reached from the token, stands in</param>
    /// <returns><see langword="true"/> if the token keeps its own line; otherwise, <see langword="false"/></returns>
    public static bool StartsOwnLine(SyntaxToken token, RootConstruct rootConstruct)
    {
        return token.Kind() switch
               {
                   SyntaxKind.OpenBraceToken => token.Parent is PropertyPatternClauseSyntax { Parent: RecursivePatternSyntax { Type: null, PositionalPatternClause: null } pattern } clause
                                                && LineBreakTriviaUtilities.SpansMultipleLines(clause.OpenBraceToken, clause.CloseBraceToken)
                                                && IsIntroducedByCaseLabel(pattern, rootConstruct) == false,
                   SyntaxKind.OpenParenToken => token.Parent is ParenthesizedPatternSyntax parenthesized
                                                && LineBreakTriviaUtilities.SpansMultipleLines(parenthesized.OpenParenToken, parenthesized.CloseParenToken),
                   _ => false
               };
    }

    #endregion // Methods
}