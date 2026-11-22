using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Data.Enumerations;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

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
                   RecursivePatternSyntax { PropertyPatternClause: { } clause } => LineBreakTriviaUtilities.SpansMultipleLines(clause.OpenBraceToken, clause.CloseBraceToken),
                   ListPatternSyntax list => LineBreakTriviaUtilities.SpansMultipleLines(list.OpenBracketToken, list.CloseBracketToken),
                   ParenthesizedPatternSyntax parenthesized => LineBreakTriviaUtilities.SpansMultipleLines(parenthesized.OpenParenToken, parenthesized.CloseParenToken),
                   _ => false
               };
    }

    /// <summary>
    /// Collects the binary patterns of a pattern chain in text order: the pattern itself and every binary pattern that is its
    /// left or right operand, directly and not through a parenthesized pattern, and so on down (FMT-BREAK-017)
    /// </summary>
    /// <param name="pattern">The pattern</param>
    /// <param name="chain">The binary patterns collected so far</param>
    private static void CollectChain(PatternSyntax pattern, List<BinaryPatternSyntax> chain)
    {
        if (pattern is not BinaryPatternSyntax binary)
        {
            return;
        }

        CollectChain(binary.Left, chain);
        chain.Add(binary);
        CollectChain(binary.Right, chain);
    }

    /// <summary>
    /// Gets the current replacement of a token, or the token itself when it has none
    /// </summary>
    /// <param name="replacements">The replacements so far</param>
    /// <param name="token">The original token</param>
    /// <returns>The token as currently rewritten</returns>
    private static SyntaxToken GetCurrent(Dictionary<SyntaxToken, SyntaxToken> replacements, SyntaxToken token)
    {
        return replacements.TryGetValue(token, out var replaced)
                   ? replaced
                   : token;
    }

    /// <summary>
    /// Determines whether a pattern chain is wrapped: a line break stands next to one of its combinators, or an operand next to
    /// one of its combinators is a property, list, or parenthesized pattern that spans several lines (FMT-BREAK-017)
    /// </summary>
    /// <param name="chain">The binary patterns of the chain</param>
    /// <returns><see langword="true"/> if the chain is wrapped; otherwise, <see langword="false"/></returns>
    private static bool IsWrappedChain(List<BinaryPatternSyntax> chain)
    {
        return chain.Any(static binary => TokenGapUtilities.HasLineBreakBetween(binary.Left.GetLastToken(), binary.OperatorToken)
                                          || TokenGapUtilities.HasLineBreakBetween(binary.OperatorToken, binary.Right.GetFirstToken())
                                          || IsMultiLineDelimitedPattern(RightmostLeaf(binary.Left))
                                          || IsMultiLineDelimitedPattern(LeftmostLeaf(binary.Right)));
    }

    /// <summary>
    /// Normalizes binary operator position by moving end-of-line trivia to the left operand. The <c>is</c> of a type test keeps
    /// its place: the type behind it joins it (FMT-BREAK-041), and an <c>is</c> that starts its line stays there, unless a line break already
    /// separates the left operand from the operator, which then stays the gap's only one (FMT-BREAK-030). The operator and the right
    /// operand are joined with the minimal separator (FMT-BREAK-031), so an operand such as <c>-a</c> behind <c>-</c> stays
    /// a separate token; the spaces of the joined gap are the horizontal-spacing phase's
    /// </summary>
    /// <param name="node">The binary expression node</param>
    /// <returns>The binary expression with the operator at the beginning of the continuation line</returns>
    private BinaryExpressionSyntax NormalizeBinaryOperatorPosition(BinaryExpressionSyntax node)
    {
        var operatorToken = node.OperatorToken;

        if (node.IsKind(SyntaxKind.IsExpression)
            || LineBreakTriviaUtilities.HasTrailingEndOfLine(operatorToken) == false)
        {
            return node;
        }

        var leftLastToken = node.Left.GetLastToken();
        var rightFirstToken = node.Right.GetFirstToken();
        var newLeftTrailing = LineBreakTriviaUtilities.EndLineInFrontOf(leftLastToken, operatorToken, _context.EndOfLine);
        var newLeftLastToken = leftLastToken.WithTrailingTrivia(newLeftTrailing);
        var newOperatorToken = operatorToken.WithTrailingTrivia(LineBreakTriviaUtilities.CreateJoinedTrailingTrivia(operatorToken, rightFirstToken));
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
    /// Lays out a wrapped pattern chain (FMT-BREAK-017): every combinator starts its own line, and the right pattern of each
    /// combinator joins it, except where FMT-BREAK-022 starts the right pattern's first token on a line of its own
    /// (<see cref="PatternLineStartUtilities.StartsOwnLine"/>). A chain that is not wrapped keeps its layout. No gap behind a
    /// combinator holds a comment, a directive, or disabled text: the precheck refuses an input that holds one there
    /// (FMT-UNSUP-017)
    /// </summary>
    /// <param name="node">The outermost binary pattern of the chain, possibly rebuilt and then without a parent</param>
    /// <param name="chainConstruct">The facts captured for the chain before it was rebuilt, case-label pattern when a case label introduces it</param>
    /// <returns>The binary pattern with its chain laid out</returns>
    private BinaryPatternSyntax NormalizePatternChain(BinaryPatternSyntax node, RootConstruct chainConstruct)
    {
        var chain = new List<BinaryPatternSyntax>();

        CollectChain(node, chain);

        if (IsWrappedChain(chain) == false)
        {
            return node;
        }

        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var binary in chain)
        {
            var leftLastToken = binary.Left.GetLastToken();
            var operatorToken = binary.OperatorToken;
            var rightFirstToken = binary.Right.GetFirstToken();

            if (TokenGapUtilities.HasLineBreakBetween(leftLastToken, operatorToken) == false)
            {
                var currentLeft = GetCurrent(replacements, leftLastToken);

                replacements[leftLastToken] = currentLeft.WithTrailingTrivia(LineBreakTriviaUtilities.EndLineInFrontOf(currentLeft, operatorToken, _context.EndOfLine));
            }

            if (PatternLineStartUtilities.StartsOwnLine(rightFirstToken, chainConstruct) == false
                && TokenGapUtilities.HasLineBreakBetween(operatorToken, rightFirstToken))
            {
                var currentOperator = GetCurrent(replacements, operatorToken);

                replacements[operatorToken] = currentOperator.WithTrailingTrivia(LineBreakTriviaUtilities.CreateJoinedTrailingTrivia(currentOperator, rightFirstToken));
                replacements[rightFirstToken] = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(GetCurrent(replacements, rightFirstToken));
            }
        }

        return replacements.Count == 0
                   ? node
                   : node.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
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

        var isChainRoot = node.Parent is not BinaryPatternSyntax;

        // Whether a case label introduces the chain is decided before the rewrite, because a rebuilt chain has no parent; the
        // walk of FMT-BREAK-022's condition from a right operand then ends at the chain and reads this fact there
        var chainConstruct = isChainRoot && PatternLineStartUtilities.IsIntroducedByCaseLabel(node, _context.RootConstruct)
                                 ? RootConstruct.CaseLabelPattern
                                 : RootConstruct.None;

        node = (BinaryPatternSyntax)base.VisitBinaryPattern(node);

        if (node == null)
        {
            return null;
        }

        return isChainRoot
                   ? NormalizePatternChain(node, chainConstruct)
                   : node;
    }

    #endregion // CSharpSyntaxVisitor
}