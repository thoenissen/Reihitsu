using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Pipeline.BlankLines.Utilities;

/// <summary>
/// Shared collaborator for the blank-line subphases. Provides the blank-line gap queries and the
/// blank-line insertion edit that previously lived on the removed <c>BlankLineSubphaseRewriter</c> base type
/// </summary>
internal sealed class BlankLineEditor
{
    #region Fields

    /// <summary>
    /// Formatting context used to source the end-of-line sequence for inserted blank lines
    /// </summary>
    private readonly FormattingContext _context;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    public BlankLineEditor(FormattingContext context)
    {
        _context = context;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the gap from the previous token to a leading-trivia prefix of the specified token
    /// </summary>
    /// <param name="token">The token whose leading-trivia prefix should be inspected</param>
    /// <param name="leadingTriviaEndExclusive">Exclusive upper bound in the leading-trivia list</param>
    /// <param name="previousToken">
    /// The facts about the token that precedes <paramref name="token"/>, resolved through
    /// <see cref="PrecedingTokenFacts.Resolve"/> so that a detached formatting root still counts the line break that ends
    /// the preceding token's line
    /// </param>
    /// <returns>The analysis of the trivia up to the specified leading-trivia index</returns>
    public static TokenGapAnalysis AnalyzeGapBeforeLeadingTriviaIndex(SyntaxToken token, int leadingTriviaEndExclusive, PrecedingTokenFacts previousToken)
    {
        var gap = SyntaxFactory.TriviaList(previousToken.TrailingTrivia.Concat(token.LeadingTrivia.Take(leadingTriviaEndExclusive)));

        return TokenGapAnalysis.OfTriviaRange(gap, 0, gap.Count);
    }

    /// <summary>
    /// Determines whether the region or end region directive whose line starts at the specified index of a trivia list is
    /// missing the blank line that must precede it. The decision is the Core rule
    /// <see cref="RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(bool, bool, bool)"/>, which RH5031's
    /// analyzer and code fix read from the source text; this method only reads its facts about the line directly above the
    /// directive from the trivia. Only that one line counts: a blank line further up the gap, such as above a header comment,
    /// does not satisfy the rule, and a conditional directive, a comment, or another directive on the line above does not
    /// exempt the directive
    /// </summary>
    /// <param name="trivia">The trivia list that holds the directive</param>
    /// <param name="lineStartIndex">The index in <paramref name="trivia"/> at which the directive's line starts, in front of its indentation</param>
    /// <param name="hasTokenBefore">Whether a token stands in front of <paramref name="trivia"/>; without one the list starts the file</param>
    /// <param name="tokenBeforeEndsLine">Whether the trivia of the token in front of <paramref name="trivia"/> ends that token's line</param>
    /// <param name="tokenBeforeIsOpenBraceWithoutComment">
    /// Whether the token in front of <paramref name="trivia"/> is an opening brace with no comment behind it on its line
    /// </param>
    /// <returns><see langword="true"/> when a blank line must be inserted at <paramref name="lineStartIndex"/></returns>
    public static bool IsMissingRequiredBlankLineBeforeRegionDirective(SyntaxTriviaList trivia, int lineStartIndex, bool hasTokenBefore, bool tokenBeforeEndsLine, bool tokenBeforeIsOpenBraceWithoutComment)
    {
        if (lineStartIndex == 0)
        {
            // The directive's line starts the list, so the line above is the line of the token in front of it
            return RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(hasTokenBefore, false, tokenBeforeIsOpenBraceWithoutComment);
        }

        if (trivia[lineStartIndex - 1].IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            // A directive, a documentation comment, or disabled text carries the line break that ends the line above in its
            // own text, so that line holds it
            return RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(true, false, false);
        }

        var triviaIndex = lineStartIndex - 2;

        while (triviaIndex >= 0 && trivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            triviaIndex--;
        }

        if (triviaIndex >= 0)
        {
            return RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(true, SyntaxTriviaUtilities.EndsLine(trivia[triviaIndex]), false);
        }

        // The line above starts the list: it is blank when the list starts a line, and otherwise it continues the line of
        // the token in front of the list
        var isLineAboveBlank = hasTokenBefore == false || tokenBeforeEndsLine;

        return RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(true,
                                                                                  isLineAboveBlank,
                                                                                  isLineAboveBlank == false && tokenBeforeIsOpenBraceWithoutComment);
    }

    /// <summary>
    /// Determines whether a blank line exists in the trivia list before the specified index
    /// </summary>
    /// <param name="trivia">The trivia list to search</param>
    /// <param name="endIndex">The exclusive upper bound index to search up to</param>
    /// <returns><see langword="true"/> if a blank line is found before the specified index</returns>
    /// <remarks>
    /// This intentionally does not route through <see cref="TokenGapAnalysis"/>. Region directive trivia carries
    /// its own trailing newline, so this method must treat it as resetting the line-start state, whereas
    /// <see cref="TokenGapAnalysis"/> treats directives as ordinary line content. Folding that quirk into the shared
    /// collaborator would change its behaviour for every other call site, so the two are deliberately kept distinct
    /// </remarks>
    public static bool HasBlankLineBeforeIndex(SyntaxTriviaList trivia, int endIndex)
    {
        var atLineStart = true;
        var sawLineBreak = false;

        for (var triviaIndex = 0; triviaIndex < endIndex; triviaIndex++)
        {
            var kind = trivia[triviaIndex].Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                if (sawLineBreak && atLineStart)
                {
                    return true;
                }

                sawLineBreak = true;
                atLineStart = true;
            }
            else if (kind == SyntaxKind.WhitespaceTrivia)
            {
                // Whitespace doesn't change line-start status
            }
            else if (SyntaxTriviaUtilities.IsRegionDirective(trivia[triviaIndex]))
            {
                // Structured directive trivia includes its own trailing newline,
                // so the next line effectively starts after it.
                atLineStart = true;
            }
            else
            {
                atLineStart = false;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the full gap before the specified token already contains a blank line. The gap of the formatting
    /// root's first token starts at the root's preceding token in its document, resolved through
    /// <see cref="PrecedingTokenFacts.Resolve"/>, so that a detached root still counts the line break that ends that token's line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a blank line exists before the token; otherwise, <see langword="false"/></returns>
    public bool HasBlankLineBeforeToken(SyntaxToken token)
    {
        return AnalyzeGapBeforeLeadingTriviaIndex(token, token.LeadingTrivia.Count, ResolvePrecedingToken(token)).BlankLineCount > 0;
    }

    /// <summary>
    /// Ensures a blank line exists before every active region or end region directive of the specified kind in the token's
    /// leading trivia that SHARED-BLANK-005 requires one for (<see cref="IsMissingRequiredBlankLineBeforeRegionDirective"/>).
    /// Directives inside a branch the compiler skipped are passed over
    /// </summary>
    /// <param name="token">The token whose leading trivia should be checked</param>
    /// <param name="directiveKind">Kind of the directive that requires a preceding blank line</param>
    /// <param name="previousToken">
    /// The facts about the token that precedes <paramref name="token"/> in the original, unmodified tree. Callers must
    /// resolve them before making any earlier trivia edit to <paramref name="token"/> in the same rewrite step and pass
    /// them through rather than letting them be re-derived from a possibly detached <paramref name="token"/>.
    /// </param>
    /// <returns>The token with a blank line inserted before each directive that lacks one, or the original if none does</returns>
    /// <remarks>
    /// The directives are visited from the last to the first, so an insertion never shifts the index of a directive still to
    /// be visited, and an insertion only adds a blank line above its own directive, so it never changes the line directly
    /// above another one
    /// </remarks>
    public SyntaxToken EnsureBlankLineBeforeRegionDirectives(SyntaxToken token, SyntaxKind directiveKind, PrecedingTokenFacts previousToken)
    {
        var trivia = token.LeadingTrivia;
        var isChanged = false;
        var tokenBeforeIsOpenBraceWithoutComment = previousToken.Kind == SyntaxKind.OpenBraceToken
                                                   && previousToken.HasTrailingComment == false;

        for (var directiveIndex = trivia.Count - 1; directiveIndex >= 0; directiveIndex--)
        {
            // A directive inside a branch the compiler skipped is surrounded by disabled text, so the inserted
            // line break merges into that text where the line above can never be read as blank again - the
            // next run would insert another one.
            if (trivia[directiveIndex].IsKind(directiveKind) == false
                || SyntaxTriviaUtilities.IsInactiveDirective(trivia[directiveIndex]))
            {
                continue;
            }

            var lineStartIndex = directiveIndex;

            while (lineStartIndex > 0 && trivia[lineStartIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                lineStartIndex--;
            }

            if (IsMissingRequiredBlankLineBeforeRegionDirective(trivia, lineStartIndex, previousToken.Exists, previousToken.EndsLine, tokenBeforeIsOpenBraceWithoutComment))
            {
                trivia = trivia.Insert(lineStartIndex, SyntaxFactory.EndOfLine(_context.EndOfLine));
                isChanged = true;
            }
        }

        return isChanged
                   ? token.WithLeadingTrivia(trivia)
                   : token;
    }

    /// <summary>
    /// Ensures a blank line exists before the specified statement by inserting one if absent, exempting
    /// statements directly preceded by a preprocessor directive (FMT-BLANK-006). When the gap holds a directive that a
    /// comment separates from the statement, the blank line goes after the last directive (FMT-BLANK-016)
    /// </summary>
    /// <param name="statement">The statement to check and potentially modify</param>
    /// <returns>The statement with a blank line inserted before it, or the original if none is required or one already exists</returns>
    /// <remarks>
    /// No blank line is inserted when the statement is directly preceded by a preprocessor directive,
    /// mirroring the exemption the PrecededBy/FollowedBy analyzer bases apply. Callers whose
    /// rule has no such exemption, such as the "blank line after a closing brace" rule, must use
    /// <see cref="EnsureBlankLineAfterClosingBrace"/> instead
    /// </remarks>
    public StatementSyntax EnsureBlankLineBeforeStatement(StatementSyntax statement)
    {
        return EnsureBlankLineUnlessPrecededByDirective(statement);
    }

    /// <summary>
    /// Ensures a blank line exists before the first token of the specified switch section (FMT-BLANK-004), unless a
    /// preprocessor directive directly precedes the section (FMT-BLANK-006). When the gap holds a directive that a comment
    /// separates from the section, the blank line goes after the last directive (FMT-BLANK-016). The line-break phase,
    /// which runs before this phase, has already started the section's line (FMT-BREAK-033), so only the blank line is
    /// added
    /// </summary>
    /// <param name="section">The switch section</param>
    /// <returns>The section with a blank line inserted before it, or the original if none is required or one already exists</returns>
    public SwitchSectionSyntax EnsureBlankLineBeforeSection(SwitchSectionSyntax section)
    {
        return EnsureBlankLineUnlessPrecededByDirective(section);
    }

    /// <summary>
    /// Ensures a blank line exists between a closing brace and the specified statement, repositioning the
    /// insertion past any leading directive rather than exempting it
    /// </summary>
    /// <param name="statement">The statement to check and potentially modify</param>
    /// <returns>The statement with a blank line inserted before it, or the original if one already exists</returns>
    /// <remarks>
    /// RH5030 (blank line required after a closing brace) carries no directive exemption, unlike the
    /// PrecededBy/FollowedBy statement rules, so the blank line must still be inserted here — just after
    /// the directive instead of at leading-trivia index 0, which would otherwise land it inside the
    /// conditional region the directive opens or closes.
    /// </remarks>
    public StatementSyntax EnsureBlankLineAfterClosingBrace(StatementSyntax statement)
    {
        var firstToken = statement.GetFirstToken();

        if (HasBlankLineBeforeToken(firstToken))
        {
            return statement;
        }

        var insertIndex = SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(firstToken.LeadingTrivia);

        return InsertBlankLineBeforeToken(statement, firstToken, insertIndex);
    }

    /// <summary>
    /// Ensures a blank line exists before the first token of the specified statement or switch section. No blank line is
    /// inserted when the gap already holds one or when the last trivia in front of the token, apart from whitespace and
    /// line breaks, is a directive — a whole conditional group ends with its <c>#endif</c> and so counts (FMT-BLANK-006).
    /// Otherwise the blank line is inserted after the last directive of the token's leading trivia, in front of the
    /// comments between that directive and the token, and never directly in front of a directive (FMT-BLANK-016)
    /// </summary>
    /// <typeparam name="TNode">The statement or switch section type</typeparam>
    /// <param name="node">The statement or switch section</param>
    /// <returns>The node with a blank line inserted before it, or the original if none is required or one already exists</returns>
    private TNode EnsureBlankLineUnlessPrecededByDirective<TNode>(TNode node)
        where TNode : SyntaxNode
    {
        var firstToken = node.GetFirstToken();

        if (HasBlankLineBeforeToken(firstToken)
            || SyntaxTriviaUtilities.IsPrecededByDirective(CombinedLeadingTrivia(firstToken)))
        {
            return node;
        }

        var insertIndex = SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(firstToken.LeadingTrivia);

        return InsertBlankLineBeforeToken(node, firstToken, insertIndex);
    }

    /// <summary>
    /// Combines the trailing trivia of the token preceding the specified token with its own leading trivia
    /// </summary>
    /// <param name="token">The token whose preceding trivia gap should be combined</param>
    /// <returns>The combined trivia sequence, in source order</returns>
    private IEnumerable<SyntaxTrivia> CombinedLeadingTrivia(SyntaxToken token)
    {
        return ResolvePrecedingToken(token).TrailingTrivia.Concat(token.LeadingTrivia);
    }

    /// <summary>
    /// Resolves the facts about the token that precedes the specified token, falling back to the formatting root's
    /// preceding token in its document for the root's first token
    /// </summary>
    /// <param name="token">The token whose preceding token should be resolved</param>
    /// <returns>The facts about the preceding token</returns>
    private PrecedingTokenFacts ResolvePrecedingToken(SyntaxToken token)
    {
        return PrecedingTokenFacts.Resolve(token.GetPreviousToken(), _context);
    }

    /// <summary>
    /// Inserts a blank line into the leading trivia of the specified token at the given index. When the gap in front of the
    /// index does not end its line — the token stands on the line of the preceding token, or behind the last line of a
    /// multi-line comment there — the line break that ends that line is inserted together with the blank line
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The node whose token should be replaced</param>
    /// <param name="token">The token to insert the blank line before</param>
    /// <param name="insertIndex">The leading-trivia index at which to insert the blank line</param>
    /// <returns>The node with the token replaced by a copy carrying the inserted blank line</returns>
    private TNode InsertBlankLineBeforeToken<TNode>(TNode node, SyntaxToken token, int insertIndex)
        where TNode : SyntaxNode
    {
        var eol = SyntaxFactory.EndOfLine(_context.EndOfLine);
        var previousToken = ResolvePrecedingToken(token);
        var lineBreakCount = previousToken.Exists
                                 ? AnalyzeGapBeforeLeadingTriviaIndex(token, token.LeadingTrivia.Count, previousToken).RequiredLineBreakCountForBlankLine
                                 : 1;
        var newLeading = token.LeadingTrivia;

        for (var lineBreakIndex = 0; lineBreakIndex < lineBreakCount; lineBreakIndex++)
        {
            newLeading = newLeading.Insert(insertIndex, eol);
        }

        var newToken = token.WithLeadingTrivia(newLeading);

        return node.ReplaceToken(token, newToken);
    }

    #endregion // Methods
}