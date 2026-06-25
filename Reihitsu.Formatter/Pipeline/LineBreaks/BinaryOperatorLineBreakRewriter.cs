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
    /// Determines whether two tokens sit on different lines
    /// </summary>
    /// <param name="openToken">The opening token</param>
    /// <param name="closeToken">The closing token</param>
    /// <returns><see langword="true"/> if the tokens span multiple lines; otherwise, <see langword="false"/></returns>
    private static bool SpansMultipleLines(SyntaxToken openToken,
                                           SyntaxToken closeToken)
    {
        return openToken.GetLocation().GetLineSpan().StartLinePosition.Line
               != closeToken.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Returns the rightmost leaf pattern of a pattern, descending through combinator chains
    /// </summary>
    /// <param name="pattern">The pattern</param>
    /// <returns>The rightmost leaf pattern</returns>
    private static PatternSyntax RightmostLeaf(PatternSyntax pattern)
    {
        return pattern is BinaryPatternSyntax binary ? RightmostLeaf(binary.Right) : pattern;
    }

    /// <summary>
    /// Returns the leftmost leaf pattern of a pattern, descending through combinator chains
    /// </summary>
    /// <param name="pattern">The pattern</param>
    /// <returns>The leftmost leaf pattern</returns>
    private static PatternSyntax LeftmostLeaf(PatternSyntax pattern)
    {
        return pattern is BinaryPatternSyntax binary ? LeftmostLeaf(binary.Left) : pattern;
    }

    /// <summary>
    /// Determines whether a pattern is a delimited pattern (recursive, list, or parenthesized)
    /// that wraps across multiple lines. A <c>not</c> wrapper is unwrapped first
    /// </summary>
    /// <param name="pattern">The pattern to inspect</param>
    /// <returns><see langword="true"/> if the pattern is a multi-line delimited pattern; otherwise, <see langword="false"/></returns>
    private static bool IsMultiLineDelimitedPattern(PatternSyntax pattern)
    {
        if (pattern is UnaryPatternSyntax unary)
        {
            pattern = unary.Pattern;
        }

        return pattern switch
               {
                   RecursivePatternSyntax { PropertyPatternClause: { } clause } => SpansMultipleLines(clause.OpenBraceToken, clause.CloseBraceToken),
                   ListPatternSyntax list => SpansMultipleLines(list.OpenBracketToken, list.CloseBracketToken),
                   ParenthesizedPatternSyntax parenthesized => SpansMultipleLines(parenthesized.OpenParenToken, parenthesized.CloseParenToken),
                   _ => false
               };
    }

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

    /// <summary>
    /// Places a pattern combinator (<c>and</c>/<c>or</c>) on its own line when the pattern wraps
    /// across multiple lines, so the operator aligns with the introducing construct
    /// </summary>
    /// <param name="node">The binary pattern node</param>
    /// <returns>The binary pattern with the combinator on its own line</returns>
    private BinaryPatternSyntax NormalizePatternOperatorPosition(BinaryPatternSyntax node)
    {
        // Only break the combinator onto its own line when one of the adjacent operands is a
        // delimited pattern that wraps; simple operands keep the conventional "or operand" layout
        if (IsMultiLineDelimitedPattern(RightmostLeaf(node.Left)) == false
            && IsMultiLineDelimitedPattern(LeftmostLeaf(node.Right)) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(node.OperatorToken) == false)
        {
            node = LineBreakTriviaUtilities.MoveTokenToNewLine(node, node.OperatorToken, _context.EndOfLine);
        }

        return node;
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

    /// <inheritdoc/>
    public override SyntaxNode VisitBinaryPattern(BinaryPatternSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (BinaryPatternSyntax)base.VisitBinaryPattern(node);

        if (node == null)
        {
            return null;
        }

        return NormalizePatternOperatorPosition(node);
    }

    #endregion // CSharpSyntaxVisitor
}