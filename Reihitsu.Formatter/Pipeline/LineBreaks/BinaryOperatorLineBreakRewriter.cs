using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for binary operator placement
/// </summary>
internal sealed class BinaryOperatorLineBreakRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BinaryOperatorLineBreakRewriter(FormattingContext context,
                                           CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Normalizes binary operator position by moving end-of-line trivia to the left operand
    /// </summary>
    /// <param name="node">The binary expression node</param>
    /// <returns>The binary expression with the operator at the beginning of the continuation line</returns>
    private BinaryExpressionSyntax NormalizeBinaryOperatorPosition(BinaryExpressionSyntax node)
    {
        var operatorToken = node.OperatorToken;

        if (LineBreakTriviaUtilities.HasTrailingEndOfLine(operatorToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(operatorToken, node.Right.GetFirstToken()))
        {
            return node;
        }

        var leftLastToken = node.Left.GetLastToken();
        var newOperatorTrailing = LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(operatorToken.TrailingTrivia);
        var newLeftTrailing = LineBreakTriviaUtilities.AppendEndOfLine(leftLastToken.TrailingTrivia, _context.EndOfLine);
        var newLeftLastToken = leftLastToken.WithTrailingTrivia(newLeftTrailing);
        var newOperatorToken = operatorToken.WithTrailingTrivia(newOperatorTrailing);
        var rightFirstToken = node.Right.GetFirstToken();
        var newRightFirstToken = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(rightFirstToken);

        return node.ReplaceTokens([leftLastToken, operatorToken, rightFirstToken],
                                  (original, _) =>
                                  {
                                      if (original == leftLastToken)
                                      {
                                          return newLeftLastToken;
                                      }

                                      if (original == operatorToken)
                                      {
                                          return newOperatorToken;
                                      }

                                      return newRightFirstToken;
                                  });
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (BinaryExpressionSyntax)base.VisitBinaryExpression(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeBinaryOperatorPosition(node);
    }

    #endregion // CSharpSyntaxVisitor
}