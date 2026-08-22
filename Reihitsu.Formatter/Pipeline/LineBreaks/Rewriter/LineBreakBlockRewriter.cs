using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies line-break rules for blocks, accessor lists, and switch braces
/// </summary>
internal sealed class LineBreakBlockRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The token gap normalizer
    /// </summary>
    private readonly TokenGapNormalizer _gapNormalizer;

    /// <summary>
    /// The brace placer
    /// </summary>
    private readonly BracePlacer _bracePlacer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="gapNormalizer">The token gap normalizer</param>
    /// <param name="bracePlacer">The brace placer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public LineBreakBlockRewriter(TokenGapNormalizer gapNormalizer,
                                  BracePlacer bracePlacer,
                                  CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _gapNormalizer = gapNormalizer;
        _bracePlacer = bracePlacer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the gap between two adjacent statement tokens carries a preprocessor directive or
    /// disabled text
    /// </summary>
    /// <param name="previousToken">The token that ends the preceding statement</param>
    /// <param name="currentToken">The token that starts the following statement</param>
    /// <returns><see langword="true"/> if the gap contains a directive or disabled-text trivia; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// A directive splits the gap into independent blank-line runs on either side of it, which
    /// <see cref="Pipeline.BlankLines.BlankLinePhase"/> already collapses correctly, one run at a time.
    /// <see cref="TokenGapUtilities.CountBlankLinesBetween"/> sums the blank lines on both sides into one
    /// number, so a single blank line on each side is miscounted as two — exactly the excess this method's
    /// caller is trying to detect. Withholding the correction here leaves the earlier, run-aware phase's
    /// answer standing instead of adding a blank line the gap never actually had (issue #695)
    /// </remarks>
    private static bool GapCarriesDirectiveOrDisabledText(SyntaxToken previousToken, SyntaxToken currentToken)
    {
        foreach (var trivia in previousToken.TrailingTrivia)
        {
            if (SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia))
            {
                return true;
            }
        }

        foreach (var trivia in currentToken.LeadingTrivia)
        {
            if (SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Ensures sibling statements in a block start on separate lines
    /// </summary>
    /// <param name="node">The block node</param>
    /// <returns>The updated block</returns>
    private BlockSyntax EnsureStatementsStartOnSeparateLines(BlockSyntax node)
    {
        if (node.Statements.Count <= 1)
        {
            return node;
        }

        var modified = false;
        var statements = node.Statements.ToArray();

        for (var statementIndex = 1; statementIndex < statements.Length; statementIndex++)
        {
            if (statements[statementIndex - 1] is EmptyStatementSyntax
                || statements[statementIndex] is EmptyStatementSyntax)
            {
                continue;
            }

            var previousToken = statements[statementIndex - 1].GetLastToken();
            var currentToken = statements[statementIndex].GetFirstToken();

            if (TokenGapUtilities.HasLineBreakBetween(previousToken, currentToken) == false)
            {
                statements[statementIndex] = _gapNormalizer.NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 0);
                modified = true;

                continue;
            }

            if (TokenGapUtilities.CountBlankLinesBetween(previousToken, currentToken) > 1
                && GapCarriesDirectiveOrDisabledText(previousToken, currentToken) == false)
            {
                statements[statementIndex] = _gapNormalizer.NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 1);
                modified = true;
            }
        }

        return modified
                   ? node.WithStatements(SyntaxFactory.List(statements))
                   : node;
    }

    /// <summary>
    /// Ensures sibling statements in a switch section start on separate lines
    /// </summary>
    /// <param name="node">The switch section node</param>
    /// <returns>The updated switch section</returns>
    private SwitchSectionSyntax EnsureStatementsStartOnSeparateLines(SwitchSectionSyntax node)
    {
        if (node.Statements.Count <= 1)
        {
            return node;
        }

        var modified = false;
        var statements = node.Statements.ToArray();

        for (var statementIndex = 1; statementIndex < statements.Length; statementIndex++)
        {
            if (statements[statementIndex - 1] is EmptyStatementSyntax
                || statements[statementIndex] is EmptyStatementSyntax)
            {
                continue;
            }

            var previousToken = statements[statementIndex - 1].GetLastToken();
            var currentToken = statements[statementIndex].GetFirstToken();

            if (TokenGapUtilities.HasLineBreakBetween(previousToken, currentToken) == false)
            {
                statements[statementIndex] = _gapNormalizer.NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 0);
                modified = true;

                continue;
            }

            if (TokenGapUtilities.CountBlankLinesBetween(previousToken, currentToken) > 1
                && GapCarriesDirectiveOrDisabledText(previousToken, currentToken) == false)
            {
                statements[statementIndex] = _gapNormalizer.NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 1);
                modified = true;
            }
        }

        return modified
                   ? node.WithStatements(SyntaxFactory.List(statements))
                   : node;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (BlockSyntax)base.VisitBlock(node);

        if (node == null)
        {
            return null;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, owner => owner.OpenBraceToken, (owner, token) => owner.WithOpenBraceToken(token), owner => owner.CloseBraceToken, (owner, token) => owner.WithCloseBraceToken(token));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);
        node = EnsureStatementsStartOnSeparateLines(node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAccessorList(AccessorListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AccessorListSyntax)base.VisitAccessorList(node);

        if (node == null)
        {
            return null;
        }

        if (LineBreakDetection.ShouldNormalizeAccessorListBraces(node) == false)
        {
            return node;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, owner => owner.OpenBraceToken, (owner, token) => owner.WithOpenBraceToken(token), owner => owner.CloseBraceToken, (owner, token) => owner.WithCloseBraceToken(token));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        if (node == null)
        {
            return null;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, owner => owner.OpenBraceToken, (owner, token) => owner.WithOpenBraceToken(token), owner => owner.CloseBraceToken, (owner, token) => owner.WithCloseBraceToken(token));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchExpression(SwitchExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchExpressionSyntax)base.VisitSwitchExpression(node);

        if (node == null)
        {
            return null;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, owner => owner.OpenBraceToken, (owner, token) => owner.WithOpenBraceToken(token), owner => owner.CloseBraceToken, (owner, token) => owner.WithCloseBraceToken(token));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchSectionSyntax)base.VisitSwitchSection(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureStatementsStartOnSeparateLines(node);

        if (node.Statements.Count > 0 && node.Statements[0] is BlockSyntax block)
        {
            node = _bracePlacer.NormalizeContainedBlock(node, block);
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}