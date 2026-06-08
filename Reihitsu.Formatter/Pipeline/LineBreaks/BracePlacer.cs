using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Places braces and contained blocks on their own lines during line-break formatting
/// </summary>
internal sealed class BracePlacer
{
    #region Fields

    /// <summary>
    /// The gap normalizer used to adjust line breaks before brace tokens
    /// </summary>
    private readonly TokenGapNormalizer _gapNormalizer;

    /// <summary>
    /// The end-of-line sequence to emit when inserting line breaks
    /// </summary>
    private readonly string _endOfLine;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="gapNormalizer">The gap normalizer used to adjust line breaks before brace tokens</param>
    /// <param name="endOfLine">The end-of-line sequence to emit when inserting line breaks</param>
    public BracePlacer(TokenGapNormalizer gapNormalizer,
                       string endOfLine)
    {
        _gapNormalizer = gapNormalizer;
        _endOfLine = endOfLine;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Ensures an opening brace is on its own line by prepending an end-of-line trivia if missing.
    /// Also ensures the closing brace is on its own line
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the braces</typeparam>
    /// <param name="node">The node containing the braces</param>
    /// <param name="openBrace">The open brace token</param>
    /// <param name="withOpenBrace">Function to replace the open brace on the node</param>
    /// <param name="closeBrace">The close brace token</param>
    /// <param name="withCloseBrace">Function to replace the close brace on the node</param>
    /// <returns>The node with braces placed on their own lines</returns>
    public TNode EnsureBraceOnOwnLine<TNode>(TNode node,
                                             SyntaxToken openBrace,
                                             Func<TNode, SyntaxToken, TNode> withOpenBrace,
                                             SyntaxToken closeBrace,
                                             Func<TNode, SyntaxToken, TNode> withCloseBrace)
        where TNode : SyntaxNode
    {
        if (closeBrace.IsMissing == false)
        {
            node = _gapNormalizer.NormalizeGapBeforeOwnedToken(node, closeBrace, withCloseBrace, blankLineCount: 0);
        }

        if (openBrace.IsMissing == false)
        {
            node = _gapNormalizer.NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, openBrace, withOpenBrace, blankLineCount: 0);
        }

        return node;
    }

    /// <summary>
    /// Ensures the first token after an opening brace is on a new line.
    /// Also strips trailing whitespace from the open brace token
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The node containing the opening brace</param>
    /// <param name="openBrace">The opening brace token</param>
    /// <returns>The node with the first content token on a new line</returns>
    public TNode EnsureFirstContentOnNewLine<TNode>(TNode node,
                                                    SyntaxToken openBrace)
        where TNode : SyntaxNode
    {
        if (openBrace.IsMissing)
        {
            return node;
        }

        var nextToken = openBrace.GetNextToken();

        if (nextToken == default || nextToken.IsMissing)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(nextToken))
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasTrailingEndOfLine(openBrace))
        {
            return node;
        }

        return LineBreakTriviaUtilities.MoveTokenToNewLine(node, nextToken, _endOfLine);
    }

    /// <summary>
    /// Ensures a line break after a closing brace unless the next token is <c>;</c>, <c>,</c>, or <c>)</c>
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The node containing the closing brace</param>
    /// <param name="closeBrace">The closing brace token</param>
    /// <returns>The node with correct close-brace continuation</returns>
    public TNode EnsureCloseBraceContinuation<TNode>(TNode node,
                                                     SyntaxToken closeBrace)
        where TNode : SyntaxNode
    {
        if (closeBrace.IsMissing)
        {
            return node;
        }

        var nextToken = closeBrace.GetNextToken();

        if (nextToken == default || nextToken.IsMissing)
        {
            return node;
        }

        if (nextToken.IsKind(SyntaxKind.SemicolonToken)
            || nextToken.IsKind(SyntaxKind.CommaToken)
            || nextToken.IsKind(SyntaxKind.CloseParenToken))
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(nextToken) || LineBreakTriviaUtilities.HasTrailingEndOfLine(closeBrace))
        {
            return _gapNormalizer.NormalizeGapBeforeToken(node, nextToken, blankLineCount: 0);
        }

        var newNextToken = LineBreakTriviaUtilities.PrependEndOfLine(nextToken, _endOfLine);

        return node.ReplaceToken(nextToken, newNextToken);
    }

    /// <summary>
    /// Normalizes brace placement for a block contained by a parent syntax node
    /// </summary>
    /// <typeparam name="TNode">The parent syntax node type</typeparam>
    /// <param name="node">The parent node that contains the block</param>
    /// <param name="block">The contained block</param>
    /// <returns>The updated parent node</returns>
    public TNode NormalizeContainedBlock<TNode>(TNode node,
                                                BlockSyntax block)
        where TNode : SyntaxNode
    {
        node = _gapNormalizer.NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, block.OpenBraceToken, (n, t) => n.ReplaceToken(block.OpenBraceToken, t), blankLineCount: 0);
        node = EnsureFirstContentOnNewLine(node, block.OpenBraceToken);
        node = _gapNormalizer.NormalizeGapBeforeToken(node, block.CloseBraceToken, blankLineCount: 0);

        return node;
    }

    #endregion // Methods
}
