using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Pipeline.BlankLines.Rewriter;

/// <summary>
/// Subphase that enforces blank-line boundaries before comments and #endregion directives
/// </summary>
internal sealed class BlankLineTriviaBoundaryRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Formatting context of the current blank-line subphase
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Shared blank-line query and edit collaborator
    /// </summary>
    private readonly BlankLineEditor _editor;

    /// <summary>
    /// Cancellation token of the current blank-line subphase
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="editor">Shared blank-line query and edit collaborator</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BlankLineTriviaBoundaryRewriter(FormattingContext context, BlankLineEditor editor, CancellationToken cancellationToken)
    {
        _context = context;
        _editor = editor;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains a comment
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if any comment trivia is found in the leading trivia</returns>
    private static bool HasCommentInLeadingTrivia(SyntaxToken token)
    {
        return token.LeadingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia);
    }

    /// <summary>
    /// Determines whether the specified token's leading trivia opens with a documentation comment that shares its line
    /// with the code in front of it
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <param name="previousTokenFacts">The facts about the token that precedes <paramref name="token"/>, resolved through <see cref="PrecedingTokenFacts.Resolve"/></param>
    /// <returns><see langword="true"/> if the boundary must not be enforced for the token</returns>
    /// <remarks>
    /// Whether a documentation comment written behind code starts a new line is decided by the documentation-comment
    /// phase alone (FMT-DOC-001), which runs before this phase. A documentation comment that still shares its line with
    /// the code in front of it was left there on purpose - it documents nothing (FMT-DOC-003), or it stands in a
    /// position that phase does not move - so breaking it onto its own line would be a placement decision rather than
    /// spacing. The boundary insert would produce both the line break and the blank line, which is why the boundary is
    /// skipped rather than adjusted (FMT-BLANK-012). The exemption deliberately stops at the line boundary: a
    /// documentation comment that starts its own line still gets its blank line, which is what keeps the formatter in
    /// step with RH8303.
    /// <para>
    /// A comment starts its own line only when a line break follows the last content in front of it, see
    /// <see cref="StartsOwnLine"/>. A multi-line comment in the gap that ends on the documentation comment's line
    /// therefore leaves it on the author's filled line, exactly like the same comment behind a single-line one
    /// </para>
    /// </remarks>
    private static bool IsInlineDocumentation(SyntaxToken token, PrecedingTokenFacts previousTokenFacts)
    {
        if (previousTokenFacts.Exists == false)
        {
            return false;
        }

        var commentIndex = TriviaListUtilities.IndexOfFirstComment(token.LeadingTrivia);

        if (commentIndex < 0
            || SyntaxTriviaUtilities.IsDocumentationCommentTrivia(token.LeadingTrivia[commentIndex]) == false)
        {
            return false;
        }

        return StartsOwnLine(token, commentIndex, previousTokenFacts) == false;
    }

    /// <summary>
    /// Determines whether the trivia at the specified leading-trivia index starts its own line, that is whether a line
    /// break follows the last content between the preceding token and that index
    /// </summary>
    /// <param name="token">The token whose leading trivia contains the index</param>
    /// <param name="leadingTriviaEndExclusive">The exclusive end of the leading-trivia prefix to measure</param>
    /// <param name="previousTokenFacts">The facts about the token that precedes <paramref name="token"/></param>
    /// <returns><see langword="true"/> if a line break follows the last content in front of the index</returns>
    /// <remarks>
    /// Line breaks inside comment text do not count: a multi-line comment that ends on the line of the index leaves
    /// that index on the comment's last line. No position is read, so the result is valid for the first token of a
    /// detached formatting root, whose facts still carry the preceding token's trailing trivia. The indentation phase
    /// decides which comments it aligns through the same predicate,
    /// <see cref="TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(SyntaxToken, int, bool, SyntaxTriviaList)"/>
    /// </remarks>
    private static bool StartsOwnLine(SyntaxToken token, int leadingTriviaEndExclusive, PrecedingTokenFacts previousTokenFacts)
    {
        return TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, leadingTriviaEndExclusive, previousTokenFacts.Exists, previousTokenFacts.TrailingTrivia);
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains an end region directive
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if any end region directive trivia is found in the leading trivia</returns>
    private static bool HasEndRegionDirectiveInLeadingTrivia(SyntaxToken token)
    {
        return token.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia));
    }

    /// <summary>
    /// Determines whether the trailing trivia of the specified token contains an end region directive
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if any end region directive trivia is found in the trailing trivia</returns>
    private static bool HasEndRegionDirectiveInTrailingTrivia(SyntaxToken token)
    {
        return token.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia));
    }

    /// <summary>
    /// Finds the start index of the line that contains the specified trivia index
    /// </summary>
    /// <param name="trivia">The trivia list to inspect</param>
    /// <param name="commentIndex">The trivia index that points at the comment</param>
    /// <returns>The first trivia index on the comment line</returns>
    private static int FindLineStartIndex(SyntaxTriviaList trivia, int commentIndex)
    {
        var lineStartIndex = commentIndex;

        while (lineStartIndex > 0 && trivia[lineStartIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            lineStartIndex--;
        }

        return lineStartIndex;
    }

    /// <summary>
    /// Finds the first trivia index after preceding blank-line trivia and indentation trivia
    /// </summary>
    /// <param name="trivia">The trivia list to inspect</param>
    /// <param name="lineStartIndex">The first trivia index on the comment line</param>
    /// <returns>The first trivia index belonging to the removable gap</returns>
    private static int FindGapStartIndex(SyntaxTriviaList trivia, int lineStartIndex)
    {
        var gapStartIndex = lineStartIndex;

        while (gapStartIndex > 0
               && (trivia[gapStartIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia)
                   || trivia[gapStartIndex - 1].IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            gapStartIndex--;
        }

        return gapStartIndex;
    }

    /// <summary>
    /// Counts blank lines in the trivia span before the comment line
    /// </summary>
    /// <param name="trivia">The trivia list to inspect</param>
    /// <param name="gapStartIndex">The first trivia index belonging to the removable gap</param>
    /// <param name="lineStartIndex">The first trivia index on the comment line</param>
    /// <returns>The number of blank lines found in the gap</returns>
    private static int CountLocalBlankLines(SyntaxTriviaList trivia, int gapStartIndex, int lineStartIndex)
    {
        return TokenGapAnalysis.OfTriviaRange(trivia, gapStartIndex, lineStartIndex).BlankLineCount;
    }

    /// <summary>
    /// Ensures exactly one blank line exists before the first comment in the specified token's leading trivia
    /// </summary>
    /// <param name="token">The token whose leading trivia should be checked</param>
    /// <param name="previousTokenFacts">The facts about the token that precedes <paramref name="token"/>, resolved through <see cref="PrecedingTokenFacts.Resolve"/></param>
    /// <returns>The token with a single blank line before the first comment</returns>
    /// <remarks>
    /// No blank line is inserted when the comment is immediately preceded by a preprocessor directive,
    /// mirroring the exemption RH5020 applies.
    /// <para>
    /// Blank lines are counted on the trivia between the preceding token and the comment, so a line break inside
    /// comment text - such as the inner line of a multi-line block comment behind the preceding token - is never
    /// mistaken for the blank line. This is the measure RH5020 and RH8303 apply, whose blank line has to be a line
    /// that holds nothing but whitespace.
    /// </para>
    /// <para>
    /// How many line breaks the boundary is short of depends on whether the comment starts its own line once the
    /// removable gap is dropped, see <see cref="StartsOwnLine"/>. A comment that does not start its own line needs two:
    /// one to end the filled line and one for the blank line; every other comment needs only the blank line. A
    /// documentation comment never takes the first branch: one that shares its line with code in front of it is
    /// exempt from the boundary (<see cref="IsInlineDocumentation"/>), because the documentation-comment phase already
    /// started a new line in front of every such comment it moves
    /// </para>
    /// <para>
    /// Both measures read <paramref name="previousTokenFacts"/> instead of the live previous token, because a node-level
    /// formatting root has no parent (FMT-PIPE-023), so its first token has no previous token in its tree. The facts carry
    /// the trailing trivia of the token in front of the root in its document, so the root's first token is measured as the
    /// document has it
    /// </para>
    /// </remarks>
    private SyntaxToken EnsureBlankLineBeforeFirstComment(SyntaxToken token, PrecedingTokenFacts previousTokenFacts)
    {
        var trivia = token.LeadingTrivia;
        var commentIndex = TriviaListUtilities.IndexOfFirstComment(trivia);

        if (commentIndex < 0)
        {
            return token;
        }

        if (SyntaxTriviaUtilities.IsPrecededByDirective(trivia.Take(commentIndex)))
        {
            return token;
        }

        var lineStartIndex = FindLineStartIndex(trivia, commentIndex);
        var gapStartIndex = FindGapStartIndex(trivia, lineStartIndex);
        var localBlankLineCount = CountLocalBlankLines(trivia, gapStartIndex, lineStartIndex);
        var blankLineCount = BlankLineEditor.AnalyzeGapBeforeLeadingTriviaIndex(token, commentIndex, previousTokenFacts).BlankLineCount;

        if (blankLineCount == 1 || (localBlankLineCount == 0 && BlankLineEditor.HasBlankLineBeforeIndex(trivia, commentIndex)))
        {
            return token;
        }

        var indentationTrivia = new List<SyntaxTrivia>(commentIndex - lineStartIndex);

        for (var triviaIndex = lineStartIndex; triviaIndex < commentIndex; triviaIndex++)
        {
            indentationTrivia.Add(trivia[triviaIndex]);
        }

        var lineBreakCount = StartsOwnLine(token, gapStartIndex, previousTokenFacts) ? 1 : 2;
        var newTrivia = new List<SyntaxTrivia>(trivia.Count - (lineStartIndex - gapStartIndex) + indentationTrivia.Count + lineBreakCount);

        for (var triviaIndex = 0; triviaIndex < gapStartIndex; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        for (var lineBreakIndex = 0; lineBreakIndex < lineBreakCount; lineBreakIndex++)
        {
            newTrivia.Add(SyntaxFactory.EndOfLine(_context.EndOfLine));
        }

        newTrivia.AddRange(indentationTrivia);

        for (var triviaIndex = commentIndex; triviaIndex < trivia.Count; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newTrivia));
    }

    /// <summary>
    /// Ensures a blank line exists before the first end region directive in the specified token's trailing trivia
    /// </summary>
    /// <param name="token">The token whose trailing trivia should be checked</param>
    /// <returns>The token with a blank line inserted before the first end region directive, or the original if one already exists</returns>
    private SyntaxToken EnsureBlankLineBeforeFirstEndRegionInTrailingTrivia(SyntaxToken token)
    {
        var trivia = token.TrailingTrivia;
        var directiveIndex = -1;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            // The leading-trivia counterpart in BlankLineEditor.EnsureBlankLineBeforeFirstDirective passes over
            // directives inside a branch the compiler skipped, because the line break it inserts merges into the
            // surrounding disabled text and is inserted again on the next run. Disabled text attaches as leading
            // trivia of the following token, so no fixture reaches this list with such a directive - the guard is
            // here so the two copies of the policy cannot drift apart.
            if (trivia[triviaIndex].IsKind(SyntaxKind.EndRegionDirectiveTrivia)
                && SyntaxTriviaUtilities.IsInactiveDirective(trivia[triviaIndex]) == false)
            {
                directiveIndex = triviaIndex;

                break;
            }
        }

        if (directiveIndex < 0)
        {
            return token;
        }

        var insertIndex = directiveIndex;

        while (insertIndex > 0 && trivia[insertIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            insertIndex--;
        }

        var endOfLineCount = 0;

        for (var triviaIndex = insertIndex - 1; triviaIndex >= 0 && trivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia); triviaIndex--)
        {
            endOfLineCount++;
        }

        if (endOfLineCount >= 2)
        {
            return token;
        }

        var newTrivia = trivia;

        while (endOfLineCount < 2)
        {
            newTrivia = newTrivia.Insert(insertIndex, SyntaxFactory.EndOfLine(_context.EndOfLine));
            insertIndex++;
            endOfLineCount++;
        }

        return token.WithTrailingTrivia(newTrivia);
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc />
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        token = base.VisitToken(token);

        var previousToken = token.GetPreviousToken();
        var previousTokenFacts = PrecedingTokenFacts.Resolve(previousToken, _context);
        var isFirstInBlock = BlankLineEditor.IsFirstInBlock(previousTokenFacts);
        var isExemptFromPrecedingBlankLineBeforeRegionDirective = BlankLineEditor.IsExemptFromPrecedingBlankLineBeforeRegionDirective(previousTokenFacts);

        if (HasCommentInLeadingTrivia(token)
            && isFirstInBlock == false
            && IsInlineDocumentation(token, previousTokenFacts) == false)
        {
            token = EnsureBlankLineBeforeFirstComment(token, previousTokenFacts);
        }

        if (HasEndRegionDirectiveInLeadingTrivia(token))
        {
            token = _editor.EnsureBlankLineBeforeFirstDirective(token, SyntaxKind.EndRegionDirectiveTrivia, previousTokenFacts);
        }

        if (HasEndRegionDirectiveInTrailingTrivia(token)
            && isExemptFromPrecedingBlankLineBeforeRegionDirective == false)
        {
            token = EnsureBlankLineBeforeFirstEndRegionInTrailingTrivia(token);
        }

        return token;
    }

    #endregion // CSharpSyntaxRewriter
}