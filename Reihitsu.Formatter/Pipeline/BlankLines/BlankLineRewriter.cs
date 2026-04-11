using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Syntax rewriter that inserts blank lines before and after statements and comments,
/// and removes blank lines after opening braces.
/// </summary>
internal sealed class BlankLineRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context providing configuration such as end-of-line style.
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Token used to observe cancellation requests.
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public BlankLineRewriter(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a blank line is required before the specified statement.
    /// </summary>
    /// <param name="statement">The current statement.</param>
    /// <param name="previous">The preceding statement.</param>
    /// <param name="inSwitchSection">Whether the statements are inside a switch section.</param>
    /// <returns><see langword="true"/> if a blank line should be inserted before the statement.</returns>
    private static bool NeedsBlankLineBefore(StatementSyntax statement, StatementSyntax previous, bool inSwitchSection)
    {
        switch (statement)
        {
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

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains a blank line.
    /// </summary>
    /// <param name="token">The token to inspect.</param>
    /// <returns><see langword="true"/> if a blank line is found in the leading trivia.</returns>
    private static bool HasBlankLineInLeadingTrivia(SyntaxToken token)
    {
        var trivia = token.LeadingTrivia;
        var atLineStart = true;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            var kind = trivia[triviaIndex].Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                if (atLineStart)
                {
                    return true;
                }

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
    /// Determines whether the specified trivia is a comment.
    /// </summary>
    /// <param name="trivia">The trivia to check.</param>
    /// <returns><see langword="true"/> if the trivia is a single-line, multi-line, or documentation comment.</returns>
    private static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        var kind = trivia.Kind();

        return kind is SyntaxKind.SingleLineCommentTrivia
                    or SyntaxKind.MultiLineCommentTrivia
                    or SyntaxKind.SingleLineDocumentationCommentTrivia
                    or SyntaxKind.MultiLineDocumentationCommentTrivia;
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains a comment.
    /// </summary>
    /// <param name="token">The token to inspect.</param>
    /// <returns><see langword="true"/> if any comment trivia is found in the leading trivia.</returns>
    private static bool HasCommentInLeadingTrivia(SyntaxToken token)
    {
        foreach (var trivia in token.LeadingTrivia)
        {
            if (IsCommentTrivia(trivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the leading trivia of the specified token contains an end region directive.
    /// </summary>
    /// <param name="token">The token to inspect.</param>
    /// <returns><see langword="true"/> if any end region directive trivia is found in the leading trivia.</returns>
    private static bool HasEndRegionDirectiveInLeadingTrivia(SyntaxToken token)
    {
        foreach (var trivia in token.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified token is the first token in a block or switch section.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><see langword="true"/> if the token is the first in its containing block.</returns>
    private static bool IsFirstInBlock(SyntaxToken token)
    {
        var previous = token.GetPreviousToken();

        if (previous == default)
        {
            return true;
        }

        if (previous.IsKind(SyntaxKind.OpenBraceToken))
        {
            return true;
        }

        if (previous.IsKind(SyntaxKind.ColonToken)
            && previous.Parent is SwitchLabelSyntax)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a blank line exists in the trivia list before the specified index.
    /// </summary>
    /// <param name="trivia">The trivia list to search.</param>
    /// <param name="endIndex">The exclusive upper bound index to search up to.</param>
    /// <returns><see langword="true"/> if a blank line is found before the specified index.</returns>
    private static bool HasBlankLineBeforeIndex(SyntaxTriviaList trivia, int endIndex)
    {
        var atLineStart = true;

        for (var triviaIndex = 0; triviaIndex < endIndex; triviaIndex++)
        {
            var kind = trivia[triviaIndex].Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                if (atLineStart)
                {
                    return true;
                }

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
    /// Removes all leading blank lines from the specified token's leading trivia.
    /// </summary>
    /// <param name="token">The token whose leading blank lines should be removed.</param>
    /// <returns>The token with leading blank lines removed.</returns>
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
    /// Ensures a blank line exists before the specified statement by inserting one if absent.
    /// </summary>
    /// <param name="statement">The statement to check and potentially modify.</param>
    /// <returns>The statement with a blank line inserted before it, or the original if one already exists.</returns>
    private StatementSyntax EnsureBlankLineBeforeStatement(StatementSyntax statement)
    {
        var firstToken = statement.GetFirstToken();

        if (HasBlankLineInLeadingTrivia(firstToken))
        {
            return statement;
        }

        var eol = SyntaxFactory.EndOfLine(_context.EndOfLine);
        var newLeading = firstToken.LeadingTrivia.Insert(0, eol);
        var newToken = firstToken.WithLeadingTrivia(newLeading);

        return statement.ReplaceToken(firstToken, newToken);
    }

    /// <summary>
    /// Ensures a blank line exists before the first comment in the specified token's leading trivia.
    /// </summary>
    /// <param name="token">The token whose leading trivia should be checked.</param>
    /// <returns>The token with a blank line inserted before the first comment, or the original if one already exists.</returns>
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

        // Check if there's already a blank line before the comment
        if (HasBlankLineBeforeIndex(trivia, commentIndex))
        {
            return token;
        }

        // Insert before the whitespace that precedes the comment on the same line
        var insertIndex = commentIndex;

        while (insertIndex > 0 && trivia[insertIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            insertIndex--;
        }

        var eol = SyntaxFactory.EndOfLine(_context.EndOfLine);
        var newTrivia = trivia.Insert(insertIndex, eol);

        return token.WithLeadingTrivia(newTrivia);
    }

    /// <summary>
    /// Ensures a blank line exists before the first end region directive in the specified token's leading trivia.
    /// </summary>
    /// <param name="token">The token whose leading trivia should be checked.</param>
    /// <returns>The token with a blank line inserted before the first end region directive, or the original if one already exists.</returns>
    private SyntaxToken EnsureBlankLineBeforeFirstEndRegion(SyntaxToken token)
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

        // Check if there's already a blank line before the directive
        if (HasBlankLineBeforeIndex(trivia, directiveIndex))
        {
            return token;
        }

        // Insert before the whitespace that precedes the directive on the same line
        var insertIndex = directiveIndex;

        while (insertIndex > 0 && trivia[insertIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            insertIndex--;
        }

        var eol = SyntaxFactory.EndOfLine(_context.EndOfLine);
        var newTrivia = trivia.Insert(insertIndex, eol);

        return token.WithLeadingTrivia(newTrivia);
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        token = base.VisitToken(token);

        // No blank line after opening brace
        var previousToken = token.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.OpenBraceToken))
        {
            token = RemoveLeadingBlankLines(token);
        }

        // Blank line before comments
        if (HasCommentInLeadingTrivia(token)
            && IsFirstInBlock(token) == false)
        {
            token = EnsureBlankLineBeforeFirstComment(token);
        }

        // Blank line before #endregion
        if (HasEndRegionDirectiveInLeadingTrivia(token)
            && IsFirstInBlock(token) == false)
        {
            token = EnsureBlankLineBeforeFirstEndRegion(token);
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
            var needsBlankLine = false;

            // Blank line before certain statement types
            if (NeedsBlankLineBefore(current, prev, inSwitchSection))
            {
                needsBlankLine = true;
            }

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

        return node.WithStatements(new SyntaxList<StatementSyntax>(newStatements));
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

        return node.WithStatements(new SyntaxList<StatementSyntax>(newStatements));
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

                if (HasBlankLineInLeadingTrivia(firstToken) == false)
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

        return node.WithSections(new SyntaxList<SwitchSectionSyntax>(newSections));
    }

    #endregion // CSharpSyntaxRewriter
}