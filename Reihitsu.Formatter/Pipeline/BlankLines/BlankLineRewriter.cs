using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Syntax rewriter that inserts blank lines before and after statements and comments,
/// and removes blank lines after opening braces
/// </summary>
internal sealed class BlankLineRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context providing configuration such as end-of-line style
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Token used to observe cancellation requests
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BlankLineRewriter(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a blank line is required before the specified statement
    /// </summary>
    /// <param name="statement">The current statement</param>
    /// <param name="previous">The preceding statement</param>
    /// <param name="inSwitchSection">Whether the statements are inside a switch section</param>
    /// <returns><see langword="true"/> if a blank line should be inserted before the statement</returns>
    private static bool NeedsBlankLineBefore(StatementSyntax statement, StatementSyntax previous, bool inSwitchSection)
    {
        // Require a blank line after a closing brace, unless the following statement is a break inside a switch section
        if (previous.GetLastToken().IsKind(SyntaxKind.CloseBraceToken)
            && (statement is BreakStatementSyntax == false || inSwitchSection == false))
        {
            return true;
        }

        switch (statement)
        {
            case LocalDeclarationStatementSyntax:
                return previous is LocalDeclarationStatementSyntax == false;

            case TryStatementSyntax:
            case IfStatementSyntax:
            case WhileStatementSyntax:
            case DoStatementSyntax:
            case UsingStatementSyntax:
            case CommonForEachStatementSyntax:
            case ForStatementSyntax:
            case ReturnStatementSyntax:
            case GotoStatementSyntax:
            case ContinueStatementSyntax:
            case ThrowStatementSyntax:
            case SwitchStatementSyntax:
            case CheckedStatementSyntax:
            case FixedStatementSyntax:
            case LockStatementSyntax:
                return true;

            case BreakStatementSyntax:
                return inSwitchSection == false;

            case YieldStatementSyntax:
                return previous is YieldStatementSyntax == false;

            case ExpressionStatementSyntax expressionStatement:
                return previous is LocalDeclarationStatementSyntax
                       && expressionStatement.Expression is AssignmentExpressionSyntax == false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains a blank line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a blank line is found in the leading trivia</returns>
    private static bool HasBlankLineInLeadingTrivia(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;
        var atLineStart = true;
        var sawLineBreak = false;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
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
            else
            {
                atLineStart = false;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the full gap before the specified token already contains a blank line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a blank line exists before the token; otherwise, <see langword="false"/></returns>
    private static bool HasBlankLineBeforeToken(SyntaxToken token)
    {
        var previousToken = token.GetPreviousToken();

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
        {
            return HasBlankLineInLeadingTrivia(token);
        }

        var sawLineBreak = false;
        var lineHasContent = false;
        var blankLineCount = 0;

        TokenGapUtilities.ProcessGapTrivia(previousToken.TrailingTrivia, ref sawLineBreak, ref lineHasContent, ref blankLineCount);
        TokenGapUtilities.ProcessGapTrivia(token.LeadingTrivia, ref sawLineBreak, ref lineHasContent, ref blankLineCount);

        return blankLineCount > 0;
    }

    /// <summary>
    /// Determines whether the specified trivia is a comment
    /// </summary>
    /// <param name="trivia">The trivia to check</param>
    /// <returns><see langword="true"/> if the trivia is a single-line, multi-line, or documentation comment</returns>
    private static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        var kind = trivia.Kind();

        return kind is SyntaxKind.SingleLineCommentTrivia
                    or SyntaxKind.MultiLineCommentTrivia
                    or SyntaxKind.SingleLineDocumentationCommentTrivia
                    or SyntaxKind.MultiLineDocumentationCommentTrivia;
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains a comment
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if any comment trivia is found in the leading trivia</returns>
    private static bool HasCommentInLeadingTrivia(SyntaxToken token)
    {
        return token.LeadingTrivia.Any(IsCommentTrivia);
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
    /// Determines whether the leading trivia contains documentation comments
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if documentation comment trivia is present</returns>
    private static bool HasDocumentationCommentInLeadingTrivia(SyntaxToken token)
    {
        return token.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                                        || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
    }

    /// <summary>
    /// Determines whether the specified token is the first token in a block or switch section
    /// </summary>
    /// <param name="previousToken">The token that precedes the token being evaluated</param>
    /// <returns><see langword="true"/> if the token is the first in its containing block</returns>
    private static bool IsFirstInBlock(SyntaxToken previousToken)
    {
        if (previousToken == default)
        {
            return true;
        }

        if (previousToken.IsKind(SyntaxKind.OpenBraceToken))
        {
            return true;
        }

        if (previousToken.IsKind(SyntaxKind.ColonToken)
            && previousToken.Parent is SwitchLabelSyntax)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a blank line exists in the trivia list before the specified index
    /// </summary>
    /// <param name="trivia">The trivia list to search</param>
    /// <param name="endIndex">The exclusive upper bound index to search up to</param>
    /// <returns><see langword="true"/> if a blank line is found before the specified index</returns>
    private static bool HasBlankLineBeforeIndex(SyntaxTriviaList trivia, int endIndex)
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
            else if (kind is SyntaxKind.RegionDirectiveTrivia
                          or SyntaxKind.EndRegionDirectiveTrivia)
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
    /// Removes all leading blank lines from the specified token's leading trivia
    /// </summary>
    /// <param name="token">The token whose leading blank lines should be removed</param>
    /// <returns>The token with leading blank lines removed</returns>
    private static SyntaxToken RemoveLeadingBlankLines(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;

        // Find the last EndOfLine in the leading blank-line sequence
        var removeUntil = -1;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            var kind = trivia[triviaIndex].Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                removeUntil = triviaIndex;
            }
            else if (kind == SyntaxKind.WhitespaceTrivia)
            {
                // Whitespace on blank lines is OK — continue scanning
            }
            else
            {
                break;
            }
        }

        if (removeUntil < 0)
        {
            return token;
        }

        var newTrivia = new List<SyntaxTrivia>();

        for (var triviaIndex = removeUntil + 1; triviaIndex < trivia.Count; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newTrivia));
    }

    /// <summary>
    /// Collapses a leading run of blank-line trivia to zero or one line break
    /// </summary>
    /// <param name="token">The token to update</param>
    /// <param name="keepSingleLineBreak">Whether one line break should be preserved</param>
    /// <returns>The updated token</returns>
    private static SyntaxToken CollapseLeadingBlankLines(SyntaxToken token, bool keepSingleLineBreak)
    {
        var trivia = token.LeadingTrivia;
        var endOfLineCount = 0;
        var runEnd = 0;
        var indentationTrivia = new List<SyntaxTrivia>();
        var afterEndOfLine = false;
        var endOfLineText = Environment.NewLine;

        while (runEnd < trivia.Count)
        {
            if (trivia[runEnd].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endOfLineCount++;
                indentationTrivia.Clear();
                afterEndOfLine = true;
                endOfLineText = trivia[runEnd].ToString();
                runEnd++;

                continue;
            }

            if (trivia[runEnd].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (afterEndOfLine)
                {
                    indentationTrivia.Add(trivia[runEnd]);
                }

                runEnd++;

                continue;
            }

            break;
        }

        if (endOfLineCount == 0 || (keepSingleLineBreak && endOfLineCount == 1))
        {
            return token;
        }

        var newTrivia = new List<SyntaxTrivia>(trivia.Count - runEnd + indentationTrivia.Count + (keepSingleLineBreak ? 1 : 0));

        if (keepSingleLineBreak)
        {
            newTrivia.Add(SyntaxFactory.EndOfLine(endOfLineText));
        }

        newTrivia.AddRange(indentationTrivia);

        for (var triviaIndex = runEnd; triviaIndex < trivia.Count; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newTrivia));
    }

    /// <summary>
    /// Removes blank lines that appear after leading documentation comments
    /// </summary>
    /// <param name="token">The token to update</param>
    /// <returns>The updated token</returns>
    private static SyntaxToken RemoveBlankLinesAfterLeadingDocumentationComments(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;
        var lastDocumentationCommentIndex = -1;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (trivia[triviaIndex].IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                || trivia[triviaIndex].IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            {
                lastDocumentationCommentIndex = triviaIndex;
            }
        }

        if (lastDocumentationCommentIndex < 0 || lastDocumentationCommentIndex == trivia.Count - 1)
        {
            return token;
        }

        var eolIndex = lastDocumentationCommentIndex + 1;

        if (eolIndex >= trivia.Count || trivia[eolIndex].IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            return token;
        }

        var indentationTrivia = new List<SyntaxTrivia>();
        var removeUntil = eolIndex;

        while (removeUntil + 1 < trivia.Count
               && (trivia[removeUntil + 1].IsKind(SyntaxKind.EndOfLineTrivia)
                   || trivia[removeUntil + 1].IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            removeUntil++;

            if (trivia[removeUntil].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                indentationTrivia.Clear();
            }
            else if (trivia[removeUntil].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                indentationTrivia.Add(trivia[removeUntil]);
            }
        }

        if (removeUntil == eolIndex)
        {
            return token;
        }

        var newTrivia = new List<SyntaxTrivia>(trivia.Count - (removeUntil - eolIndex));

        for (var triviaIndex = 0; triviaIndex <= lastDocumentationCommentIndex; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        newTrivia.AddRange(indentationTrivia);

        for (var triviaIndex = removeUntil + 1; triviaIndex < trivia.Count; triviaIndex++)
        {
            newTrivia.Add(trivia[triviaIndex]);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newTrivia));
    }

    /// <summary>
    /// Ensures a blank line exists before the specified statement by inserting one if absent
    /// </summary>
    /// <param name="statement">The statement to check and potentially modify</param>
    /// <returns>The statement with a blank line inserted before it, or the original if one already exists</returns>
    private StatementSyntax EnsureBlankLineBeforeStatement(StatementSyntax statement)
    {
        var firstToken = statement.GetFirstToken();

        if (HasBlankLineBeforeToken(firstToken))
        {
            return statement;
        }

        var eol = SyntaxFactory.EndOfLine(_context.EndOfLine);
        var newLeading = firstToken.LeadingTrivia.Insert(0, eol);
        var newToken = firstToken.WithLeadingTrivia(newLeading);

        return statement.ReplaceToken(firstToken, newToken);
    }

    /// <summary>
    /// Ensures exactly one blank line exists before the first comment in the specified token's leading trivia
    /// </summary>
    /// <param name="token">The token whose leading trivia should be checked</param>
    /// <returns>The token with a single blank line before the first comment</returns>
    private SyntaxToken EnsureBlankLineBeforeFirstComment(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;

        // Find the first comment
        var commentIndex = -1;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (IsCommentTrivia(trivia[triviaIndex]))
            {
                commentIndex = triviaIndex;

                break;
            }
        }

        if (commentIndex < 0)
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

        var lineStartIndex = commentIndex;

        while (lineStartIndex > 0 && trivia[lineStartIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            lineStartIndex--;
        }

        var gapStartIndex = lineStartIndex;

        while (gapStartIndex > 0
               && (trivia[gapStartIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia)
                   || trivia[gapStartIndex - 1].IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            gapStartIndex--;
        }

        var localBlankLineCount = 0;
        var localSawLineBreak = false;
        var localAtLineStart = true;

        for (var triviaIndex = gapStartIndex; triviaIndex < lineStartIndex; triviaIndex++)
        {
            var kind = trivia[triviaIndex].Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                if (localSawLineBreak && localAtLineStart)
                {
                    localBlankLineCount++;
                }

                localSawLineBreak = true;
                localAtLineStart = true;
            }
            else if (kind != SyntaxKind.WhitespaceTrivia)
            {
                localAtLineStart = false;
            }
        }

        var blankLineCount = 0;
        var sawLineBreak = false;
        var lineHasContent = false;
        var previousToken = token.GetPreviousToken();

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            TokenGapUtilities.ProcessGapTrivia(previousToken.TrailingTrivia, ref sawLineBreak, ref lineHasContent, ref blankLineCount);
        }

        for (var triviaIndex = 0; triviaIndex < commentIndex; triviaIndex++)
        {
            TokenGapUtilities.ProcessGapTrivia(SyntaxFactory.TriviaList(trivia[triviaIndex]), ref sawLineBreak, ref lineHasContent, ref blankLineCount);
        }

        if (blankLineCount == 1 || (localBlankLineCount == 0 && HasBlankLineBeforeIndex(trivia, commentIndex)))
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
    /// Ensures a blank line exists before the first end region directive in the specified token's leading trivia
    /// </summary>
    /// <param name="token">The token whose leading trivia should be checked</param>
    /// <param name="previousTokenEndsWithLineBreak">Whether the original previous token already ended with a line break</param>
    /// <returns>The token with a blank line inserted before the first end region directive, or the original if one already exists</returns>
    private SyntaxToken EnsureBlankLineBeforeFirstEndRegion(SyntaxToken token, bool previousTokenEndsWithLineBreak)
    {
        var trivia = token.LeadingTrivia;

        // Find the first EndRegionDirectiveTrivia
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

        // Insert before the whitespace that precedes the directive on the same line
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

        var requiredEndOfLineCount = previousTokenEndsWithLineBreak
                                         ? 1
                                         : 2;

        if (endOfLineCount >= requiredEndOfLineCount)
        {
            return token;
        }

        var newTrivia = trivia;

        while (endOfLineCount < requiredEndOfLineCount)
        {
            newTrivia = newTrivia.Insert(insertIndex, SyntaxFactory.EndOfLine(_context.EndOfLine));
            insertIndex++;
            endOfLineCount++;
        }

        return token.WithLeadingTrivia(newTrivia);
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

    /// <inheritdoc/>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        token = base.VisitToken(token);

        var previousToken = token.GetPreviousToken();
        var isFirstInBlock = IsFirstInBlock(previousToken);
        var previousTokenEndsWithLineBreak = previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
        {
            token = CollapseLeadingBlankLines(token, keepSingleLineBreak: false);
        }

        // No blank line after opening brace
        if (previousToken.IsKind(SyntaxKind.OpenBraceToken))
        {
            token = RemoveLeadingBlankLines(token);
        }

        if (token.IsKind(SyntaxKind.OpenBraceToken)
            || token.IsKind(SyntaxKind.CloseBraceToken)
            || token.IsKind(SyntaxKind.ElseKeyword)
            || token.IsKind(SyntaxKind.CatchKeyword)
            || token.IsKind(SyntaxKind.FinallyKeyword)
            || token.IsKind(SyntaxKind.WhileKeyword))
        {
            var keepSingleLineBreak = previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)) == false;

            token = CollapseLeadingBlankLines(token, keepSingleLineBreak);
        }

        if (HasDocumentationCommentInLeadingTrivia(token))
        {
            token = RemoveBlankLinesAfterLeadingDocumentationComments(token);
        }

        // Blank line before comments
        if (HasCommentInLeadingTrivia(token)
            && isFirstInBlock == false)
        {
            token = EnsureBlankLineBeforeFirstComment(token);
        }

        // Blank line before #endregion
        if (HasEndRegionDirectiveInLeadingTrivia(token)
            && isFirstInBlock == false)
        {
            token = EnsureBlankLineBeforeFirstEndRegion(token, previousTokenEndsWithLineBreak);
        }

        if (HasEndRegionDirectiveInTrailingTrivia(token)
            && isFirstInBlock == false)
        {
            token = EnsureBlankLineBeforeFirstEndRegionInTrailingTrivia(token);
        }

        return token;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var inSwitchSection = node.Parent is SwitchSectionSyntax;

        node = (BlockSyntax)base.VisitBlock(node);

        var statements = node.Statements;

        if (statements.Count == 0)
        {
            return node;
        }

        var modified = false;
        var newStatements = new StatementSyntax[statements.Count];

        for (var statementIndex = 0; statementIndex < statements.Count; statementIndex++)
        {
            newStatements[statementIndex] = statements[statementIndex];
        }

        for (var statementIndex = 1; statementIndex < statements.Count; statementIndex++)
        {
            var prev = newStatements[statementIndex - 1];
            var current = newStatements[statementIndex];

            // Blank line before certain statement types
            var needsBlankLine = NeedsBlankLineBefore(current, prev, inSwitchSection);

            // Blank line after break
            if (prev is BreakStatementSyntax)
            {
                needsBlankLine = true;
            }

            if (needsBlankLine)
            {
                var result = EnsureBlankLineBeforeStatement(current);

                if (result != current)
                {
                    newStatements[statementIndex] = result;
                    modified = true;
                }
            }
        }

        if (modified == false)
        {
            return node;
        }

        return node.WithStatements(SyntaxFactory.List(newStatements));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchSectionSyntax)base.VisitSwitchSection(node);

        var statements = node.Statements;

        if (statements.Count <= 1)
        {
            return node;
        }

        var modified = false;
        var newStatements = new StatementSyntax[statements.Count];

        for (var statementIndex = 0; statementIndex < statements.Count; statementIndex++)
        {
            newStatements[statementIndex] = statements[statementIndex];
        }

        for (var statementIndex = 1; statementIndex < statements.Count; statementIndex++)
        {
            var prev = newStatements[statementIndex - 1];
            var current = newStatements[statementIndex];

            // Blank line before certain statement types (inside switch section)
            var needsBlankLine = NeedsBlankLineBefore(current, prev, inSwitchSection: true);

            // Blank line after break within switch section
            if (prev is BreakStatementSyntax)
            {
                needsBlankLine = true;
            }

            if (needsBlankLine)
            {
                var result = EnsureBlankLineBeforeStatement(current);

                if (result != current)
                {
                    newStatements[statementIndex] = result;
                    modified = true;
                }
            }
        }

        if (modified == false)
        {
            return node;
        }

        return node.WithStatements(SyntaxFactory.List(newStatements));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        var sections = node.Sections;

        if (sections.Count <= 1)
        {
            return node;
        }

        var modified = false;
        var newSections = new SwitchSectionSyntax[sections.Count];

        for (var sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++)
        {
            newSections[sectionIndex] = sections[sectionIndex];
        }

        // Blank line after break between switch sections
        for (var sectionIndex = 1; sectionIndex < sections.Count; sectionIndex++)
        {
            var prevSection = newSections[sectionIndex - 1];
            var lastStmt = prevSection.Statements.LastOrDefault();

            if (lastStmt is BreakStatementSyntax)
            {
                var section = newSections[sectionIndex];
                var firstToken = section.GetFirstToken();

                if (HasBlankLineBeforeToken(firstToken) == false)
                {
                    var eol = SyntaxFactory.EndOfLine(_context.EndOfLine);
                    var newLeading = firstToken.LeadingTrivia.Insert(0, eol);
                    var newToken = firstToken.WithLeadingTrivia(newLeading);

                    newSections[sectionIndex] = section.ReplaceToken(firstToken, newToken);
                    modified = true;
                }
            }
        }

        if (modified == false)
        {
            return node;
        }

        return node.WithSections(SyntaxFactory.List(newSections));
    }

    #endregion // CSharpSyntaxRewriter
}