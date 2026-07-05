using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Subphase that inserts the blank lines required around <c>#region</c> and <c>#endregion</c> directives that are not
/// already produced by <see cref="BlankLineTriviaBoundaryRewriter"/>: a blank line before each <c>#region</c> directive
/// and a blank line after each region directive. The blank line before <c>#endregion</c> directives is handled by
/// <see cref="BlankLineTriviaBoundaryRewriter"/>, and excess blank lines are collapsed afterwards by
/// <see cref="BlankLineCollapser"/>
/// </summary>
internal sealed class BlankLineRegionDirectiveRewriter : CSharpSyntaxRewriter
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
    public BlankLineRegionDirectiveRewriter(FormattingContext context, BlankLineEditor editor, CancellationToken cancellationToken)
    {
        _context = context;
        _editor = editor;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the trivia is a <c>#region</c> or <c>#endregion</c> directive
    /// </summary>
    /// <param name="trivia">Trivia to inspect</param>
    /// <returns><see langword="true"/> when the trivia is a region or end-region directive</returns>
    private static bool IsRegionDirective(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
               || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia);
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains a region or end-region directive
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if any region directive trivia is found in the leading trivia</returns>
    private static bool HasRegionDirectiveInLeadingTrivia(SyntaxToken token)
    {
        return token.LeadingTrivia.Any(IsRegionDirective);
    }

    /// <summary>
    /// Ensures a blank line follows every region directive in the leading trivia, except an <c>#endregion</c> directive
    /// that directly precedes the closing brace or the end of the file
    /// </summary>
    /// <param name="token">The token whose leading trivia should be normalized</param>
    /// <returns>The token with the required blank lines inserted after its region directives</returns>
    private SyntaxToken EnsureBlankLineAfterRegionDirectives(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;
        var ownsClosingBoundary = token.IsKind(SyntaxKind.CloseBraceToken)
                                  || token.IsKind(SyntaxKind.EndOfFileToken);

        for (var triviaIndex = trivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (IsRegionDirective(trivia[triviaIndex]) == false)
            {
                continue;
            }

            var nextIndex = triviaIndex + 1;

            while (nextIndex < trivia.Count && trivia[nextIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                nextIndex++;
            }

            if (nextIndex < trivia.Count && trivia[nextIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            if (nextIndex == trivia.Count
                && ownsClosingBoundary
                && trivia[triviaIndex].IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            {
                continue;
            }

            trivia = trivia.Insert(triviaIndex + 1, SyntaxFactory.EndOfLine(_context.EndOfLine));
        }

        return token.WithLeadingTrivia(trivia);
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc />
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        token = base.VisitToken(token);

        if (HasRegionDirectiveInLeadingTrivia(token) == false)
        {
            return token;
        }

        var previousToken = token.GetPreviousToken();
        var isFirstInBlock = BlankLineEditor.IsFirstInBlock(previousToken);
        var previousTokenEndsWithLineBreak = previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

        token = EnsureBlankLineAfterRegionDirectives(token);

        if (isFirstInBlock == false)
        {
            token = _editor.EnsureBlankLineBeforeFirstDirective(token, SyntaxKind.RegionDirectiveTrivia, previousTokenEndsWithLineBreak);
        }

        return token;
    }

    #endregion // CSharpSyntaxRewriter
}