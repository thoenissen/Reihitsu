using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;
using Reihitsu.Formatter.Utilities;

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
    /// Determines whether the specified token opens the node it belongs to
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is the first token of its parent node</returns>
    private static bool OpensItsNode(SyntaxToken token)
    {
        return token.Parent != null
               && token.Parent.GetFirstToken() == token;
    }

    /// <summary>
    /// Determines whether the specified trivia is a comment the blank-line boundary applies to
    /// </summary>
    /// <param name="trivia">The trivia to inspect</param>
    /// <param name="tokenOpensItsNode">Whether the owning token opens the node it belongs to</param>
    /// <returns><see langword="true"/> if the boundary applies to the trivia</returns>
    /// <remarks>
    /// A documentation comment in front of a token that does not open its node documents nothing. It is stranded
    /// before a <c>;</c>, <c>]</c> or <c>}</c>, where the compiler reports CS1587, and deciding that it belongs on
    /// its own line separated by a blank line is a placement choice the formatter cannot make. Ordinary comments
    /// keep the boundary, because they only reach such a position when the author already put them on their own
    /// line (issues #591, #625)
    /// </remarks>
    private static bool IsBoundaryComment(SyntaxTrivia trivia, bool tokenOpensItsNode)
    {
        return ReihitsuFormatterHelpers.IsCommentTrivia(trivia)
               && (tokenOpensItsNode
                   || SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia) == false);
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains a comment the boundary applies to
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <param name="tokenOpensItsNode">Whether the token opens the node it belongs to</param>
    /// <returns><see langword="true"/> if any such comment trivia is found in the leading trivia</returns>
    private static bool HasCommentInLeadingTrivia(SyntaxToken token, bool tokenOpensItsNode)
    {
        return token.LeadingTrivia.Any(trivia => IsBoundaryComment(trivia, tokenOpensItsNode));
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
    /// Finds the first comment trivia index the boundary applies to in the leading trivia list
    /// </summary>
    /// <param name="trivia">The trivia list to inspect</param>
    /// <param name="tokenOpensItsNode">Whether the owning token opens the node it belongs to</param>
    /// <returns>The zero-based trivia index or -1 if no such comment exists</returns>
    private static int FindFirstCommentIndex(SyntaxTriviaList trivia, bool tokenOpensItsNode)
    {
        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (IsBoundaryComment(trivia[triviaIndex], tokenOpensItsNode))
            {
                return triviaIndex;
            }
        }

        return -1;
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
    /// <param name="tokenOpensItsNode">Whether the token opens the node it belongs to</param>
    /// <returns>The token with a single blank line before the first comment</returns>
    /// <remarks>
    /// No blank line is inserted when the comment is immediately preceded by a preprocessor directive,
    /// mirroring the exemption RH5020 applies (issue #415)
    /// </remarks>
    private SyntaxToken EnsureBlankLineBeforeFirstComment(SyntaxToken token, bool tokenOpensItsNode)
    {
        var trivia = token.LeadingTrivia;
        var commentIndex = FindFirstCommentIndex(trivia, tokenOpensItsNode);

        if (commentIndex < 0)
        {
            return token;
        }

        if (SyntaxTriviaUtilities.IsPrecededByDirective(trivia.Take(commentIndex)))
        {
            return token;
        }

        var previousTokenLine = token.GetPreviousToken();

        if (previousTokenLine != default && previousTokenLine.IsKind(SyntaxKind.None) == false)
        {
            var previousLine = previousTokenLine.GetLocation().GetLineSpan().EndLinePosition.Line;
            var commentLine = trivia[commentIndex].GetLocation().GetLineSpan().StartLinePosition.Line;
            var blankLineCountByLine = commentLine - previousLine - 1;

            if (blankLineCountByLine == 1)
            {
                return token;
            }
        }

        var lineStartIndex = FindLineStartIndex(trivia, commentIndex);
        var gapStartIndex = FindGapStartIndex(trivia, lineStartIndex);
        var localBlankLineCount = CountLocalBlankLines(trivia, gapStartIndex, lineStartIndex);
        var blankLineCount = BlankLineEditor.CountBlankLinesBeforeLeadingTriviaIndex(token, commentIndex);

        if (blankLineCount == 1 || (localBlankLineCount == 0 && BlankLineEditor.HasBlankLineBeforeIndex(trivia, commentIndex)))
        {
            return token;
        }

        var indentationTrivia = new List<SyntaxTrivia>(commentIndex - lineStartIndex);

        for (var triviaIndex = lineStartIndex; triviaIndex < commentIndex; triviaIndex++)
        {
            indentationTrivia.Add(trivia[triviaIndex]);
        }

        var newTrivia = new List<SyntaxTrivia>(trivia.Count - (lineStartIndex - gapStartIndex) + indentationTrivia.Count + 1);

        for (var triviaIndex = 0; triviaIndex < gapStartIndex; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        newTrivia.Add(SyntaxFactory.EndOfLine(_context.EndOfLine));
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
            if (trivia[triviaIndex].IsKind(SyntaxKind.EndRegionDirectiveTrivia))
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
        var isFirstInBlock = BlankLineEditor.IsFirstInBlock(previousToken);
        var isExemptFromPrecedingBlankLineBeforeRegionDirective = BlankLineEditor.IsExemptFromPrecedingBlankLineBeforeRegionDirective(previousToken);

        var tokenOpensItsNode = OpensItsNode(token);

        if (HasCommentInLeadingTrivia(token, tokenOpensItsNode)
            && isFirstInBlock == false)
        {
            token = EnsureBlankLineBeforeFirstComment(token, tokenOpensItsNode);
        }

        if (HasEndRegionDirectiveInLeadingTrivia(token)
            && isExemptFromPrecedingBlankLineBeforeRegionDirective == false)
        {
            token = _editor.EnsureBlankLineBeforeFirstDirective(token, SyntaxKind.EndRegionDirectiveTrivia, previousToken);
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