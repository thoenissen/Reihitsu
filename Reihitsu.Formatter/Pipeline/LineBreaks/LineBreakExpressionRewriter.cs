using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for operators, ternaries, and method chains
/// </summary>
internal sealed class LineBreakExpressionRewriter : LineBreakRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public LineBreakExpressionRewriter(FormattingContext context,
                                       CancellationToken cancellationToken)
        : base(context,
               cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses the first <see cref="MemberBindingExpressionSyntax"/> in the
    /// <see cref="ConditionalAccessExpressionSyntax.WhenNotNull"/> subtree onto the
    /// same line as the <c>?</c> operator token, so that <c>?\n.Member()</c> becomes <c>?.Member()</c>
    /// </summary>
    /// <param name="node">The conditional access expression to process</param>
    /// <returns>The modified node with the member binding collapsed</returns>
    private static ConditionalAccessExpressionSyntax CollapseMemberBindingToQuestionToken(ConditionalAccessExpressionSyntax node)
    {
        var memberBinding = node.WhenNotNull
                                .DescendantNodesAndSelf()
                                .OfType<MemberBindingExpressionSyntax>()
                                .FirstOrDefault();

        if (memberBinding == null)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(memberBinding.OperatorToken) == false)
        {
            return node;
        }

        return CollapseTokenToSameLine(node, memberBinding.OperatorToken);
    }

    /// <summary>
    /// Determines whether an invocation expression is the outermost node in a method chain.
    /// An invocation is outermost if it is not an inner link of a larger chain
    /// and not nested inside a conditional access expression
    /// </summary>
    /// <param name="node">The invocation expression to check</param>
    /// <returns><see langword="true"/> if the invocation is the outermost chain node; otherwise, <see langword="false"/></returns>
    private static bool IsOutermostChainInvocation(InvocationExpressionSyntax node)
    {
        if (node.Expression is not MemberAccessExpressionSyntax
            && node.Expression is not MemberBindingExpressionSyntax)
        {
            return false;
        }

        if (node.Parent is MemberAccessExpressionSyntax parentAccess
            && parentAccess.Parent is InvocationExpressionSyntax)
        {
            return false;
        }

        return IsInsideConditionalAccess(node) == false;
    }

    /// <summary>
    /// Collects all chain link dot tokens from a method chain or conditional access chain.
    /// Only invoked member accesses count as chain links.
    /// For conditional access, the <c>?</c> operator token is collected (not the binding dot)
    /// </summary>
    /// <param name="node">The chain node to walk</param>
    /// <param name="dots">The list to accumulate dot tokens into</param>
    private static void CollectChainLinkDots(SyntaxNode node,
                                             List<SyntaxToken> dots)
    {
        switch (node)
        {
            case InvocationExpressionSyntax invocation when invocation.Expression is MemberAccessExpressionSyntax memberAccess:
                {
                    if (memberAccess.Expression is InvocationExpressionSyntax innerInvocation)
                    {
                        CollectChainLinkDots(innerInvocation, dots);
                    }
                    else if (memberAccess.Expression is ConditionalAccessExpressionSyntax innerConditional)
                    {
                        CollectChainLinkDots(innerConditional, dots);
                    }

                    dots.Add(memberAccess.OperatorToken);
                }
                break;

            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    if (conditionalAccess.Expression is InvocationExpressionSyntax innerInvocation)
                    {
                        CollectChainLinkDots(innerInvocation, dots);
                    }
                    else if (conditionalAccess.Expression is ConditionalAccessExpressionSyntax innerConditional)
                    {
                        CollectChainLinkDots(innerConditional, dots);
                    }

                    dots.Add(conditionalAccess.OperatorToken);
                    CollectWhenNotNullChainDots(conditionalAccess.WhenNotNull, dots);
                }
                break;
        }
    }

    /// <summary>
    /// Collects chain link dot tokens from the <c>WhenNotNull</c> part of a conditional access expression
    /// </summary>
    /// <param name="node">The WhenNotNull expression to walk</param>
    /// <param name="dots">The list to accumulate dot tokens into</param>
    private static void CollectWhenNotNullChainDots(SyntaxNode node,
                                                    List<SyntaxToken> dots)
    {
        if (node is InvocationExpressionSyntax invocation
            && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            CollectWhenNotNullChainDots(memberAccess.Expression, dots);
            dots.Add(memberAccess.OperatorToken);
        }
    }

    /// <summary>
    /// Determines whether an expression contains an invocation expression
    /// </summary>
    /// <param name="expression">The expression to inspect</param>
    /// <returns><see langword="true"/> if the expression contains an invocation; otherwise, <see langword="false"/></returns>
    private static bool ContainsInvocation(ExpressionSyntax expression)
    {
        if (expression is InvocationExpressionSyntax)
        {
            return true;
        }

        foreach (var child in expression.ChildNodes())
        {
            if (child is ExpressionSyntax childExpression && ContainsInvocation(childExpression))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a syntax node is nested inside a <see cref="ConditionalAccessExpressionSyntax"/>
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns><see langword="true"/> if the node is inside a conditional access expression; otherwise, <see langword="false"/></returns>
    private static bool IsInsideConditionalAccess(SyntaxNode node)
    {
        var current = node.Parent;

        while (current != null)
        {
            if (current is ConditionalAccessExpressionSyntax)
            {
                return true;
            }

            if (current is StatementSyntax || current is MemberDeclarationSyntax)
            {
                return false;
            }

            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a chain dot token has intermediate member accesses between
    /// the dot and the chain root
    /// </summary>
    /// <param name="dotToken">The dot token from a member access expression</param>
    /// <returns><see langword="true"/> if there are intermediate member accesses; otherwise, <see langword="false"/></returns>
    private static bool HasIntermediateMemberAccess(SyntaxToken dotToken)
    {
        if (dotToken.Parent is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Expression is MemberAccessExpressionSyntax
                   || memberAccess.Expression is ConditionalAccessExpressionSyntax;
        }

        return false;
    }

    /// <summary>
    /// Recursively collects all <c>?</c> and <c>:</c> tokens from a nested ternary expression tree
    /// </summary>
    /// <param name="node">The conditional expression to collect tokens from</param>
    /// <param name="tokens">The list to populate with operator tokens</param>
    private static void CollectTernaryOperatorTokens(ConditionalExpressionSyntax node,
                                                     List<SyntaxToken> tokens)
    {
        tokens.Add(node.QuestionToken);
        tokens.Add(node.ColonToken);

        if (node.WhenTrue is ConditionalExpressionSyntax nestedTrue)
        {
            CollectTernaryOperatorTokens(nestedTrue, tokens);
        }

        if (node.WhenFalse is ConditionalExpressionSyntax nestedFalse)
        {
            CollectTernaryOperatorTokens(nestedFalse, tokens);
        }
    }

    /// <summary>
    /// Normalizes a chain containing a single dot token
    /// </summary>
    /// <param name="node">The chain node</param>
    /// <param name="chainDot">The chain dot token</param>
    /// <returns>The updated chain node</returns>
    private static SyntaxNode NormalizeSingleChainDot(SyntaxNode node,
                                                      SyntaxToken chainDot)
    {
        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(chainDot)
            && HasIntermediateMemberAccess(chainDot) == false
            && HasCommentDirectlyAbove(chainDot) == false)
        {
            return CollapseTokenToSameLine(node, chainDot);
        }

        return node;
    }

    /// <summary>
    /// Collapses the first chain dot onto the root line when it starts on a continuation line
    /// </summary>
    /// <param name="firstDot">The first chain dot token</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private static void TryCollapseFirstChainDot(SyntaxToken firstDot,
                                                 Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstDot) == false
            || HasIntermediateMemberAccess(firstDot))
        {
            return;
        }

        replacements[firstDot] = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(firstDot);

        var previousToken = firstDot.GetPreviousToken();

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken))
        {
            replacements[previousToken] = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));
        }
    }

    /// <summary>
    /// Determines whether the token has a comment directly above its line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a comment is directly above the token; otherwise, <see langword="false"/></returns>
    private static bool HasCommentDirectlyAbove(SyntaxToken token)
    {
        if (token.LeadingTrivia.Any(IsCommentTrivia) == false)
        {
            return false;
        }

        if (token.SyntaxTree == null)
        {
            return true;
        }

        var line = token.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (line <= 0)
        {
            return false;
        }

        var previousLine = token.SyntaxTree.GetText().Lines[line - 1].ToString().Trim();

        return previousLine.StartsWith("//", StringComparison.Ordinal)
               || previousLine.StartsWith("/*", StringComparison.Ordinal)
               || previousLine.StartsWith("*", StringComparison.Ordinal)
               || previousLine.EndsWith("*/", StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether a trivia is a comment
    /// </summary>
    /// <param name="trivia">The trivia to inspect</param>
    /// <returns><see langword="true"/> if the trivia is a comment; otherwise, <see langword="false"/></returns>
    private static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <summary>
    /// Normalizes a method chain or conditional access chain
    /// </summary>
    /// <param name="node">The outermost chain node (invocation or conditional access)</param>
    /// <returns>The node with normalized chain line breaks</returns>
    private SyntaxNode NormalizeChain(SyntaxNode node)
    {
        var chainDots = new List<SyntaxToken>();

        CollectChainLinkDots(node, chainDots);

        if (chainDots.Count == 0)
        {
            return node;
        }

        if (chainDots.Count == 1)
        {
            return NormalizeSingleChainDot(node, chainDots[0]);
        }

        if (chainDots.Exists(LineBreakTriviaUtilities.HasLeadingEndOfLine) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(chainDots[0])
            && HasCommentDirectlyAbove(chainDots[0]))
        {
            return node;
        }

        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        TryCollapseFirstChainDot(chainDots[0], replacements);
        EnsureContinuationDotsStartOnNewLine(chainDots, replacements);

        if (replacements.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Ensures continuation dots in a chain start on their own lines
    /// </summary>
    /// <param name="chainDots">The chain dot tokens</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private void EnsureContinuationDotsStartOnNewLine(List<SyntaxToken> chainDots,
                                                      Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        var endOfLine = SyntaxFactory.EndOfLine(Context.EndOfLine);

        for (var dotIndex = 1; dotIndex < chainDots.Count; dotIndex++)
        {
            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(chainDots[dotIndex]))
            {
                continue;
            }

            var newLeading = chainDots[dotIndex].LeadingTrivia.Insert(0, endOfLine);
            replacements[chainDots[dotIndex]] = chainDots[dotIndex].WithLeadingTrivia(newLeading);

            var previousToken = chainDots[dotIndex].GetPreviousToken();

            if (previousToken != default
                && previousToken.IsKind(SyntaxKind.None) == false
                && replacements.ContainsKey(previousToken) == false
                && previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia))
            {
                replacements[previousToken] = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(previousToken.TrailingTrivia));
            }
        }
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

        var leftLastToken = node.Left.GetLastToken();
        var newOperatorTrailing = LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(operatorToken.TrailingTrivia);
        var newLeftTrailing = AppendEndOfLine(leftLastToken.TrailingTrivia);
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
    /// Normalizes ternary operator placement
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <returns>The conditional expression with ternary operators on new lines</returns>
    private ConditionalExpressionSyntax NormalizeTernaryOperatorPosition(ConditionalExpressionSyntax node)
    {
        if (IsMultiLine(node) == false)
        {
            if (node.WhenFalse is ConditionalExpressionSyntax == false
                && node.WhenTrue is ConditionalExpressionSyntax == false)
            {
                return node;
            }

            var operatorTokens = new List<SyntaxToken>();

            CollectTernaryOperatorTokens(node, operatorTokens);

            node = node.ReplaceTokens(operatorTokens, (original, _) => PrependEndOfLine(original));
        }

        node = NormalizeQuestionTokenPosition(node);

        return NormalizeColonTokenPosition(node);
    }

    /// <summary>
    /// Normalizes placement of the <c>?</c> token in a ternary expression
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <returns>The updated conditional expression</returns>
    private ConditionalExpressionSyntax NormalizeQuestionTokenPosition(ConditionalExpressionSyntax node)
    {
        var questionToken = node.QuestionToken;

        if (LineBreakTriviaUtilities.HasTrailingEndOfLine(questionToken))
        {
            return MoveQuestionTokenToNextLine(node, questionToken);
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(questionToken))
        {
            return node;
        }

        return node.WithQuestionToken(PrependEndOfLine(questionToken));
    }

    /// <summary>
    /// Moves the ternary <c>?</c> token from line-end position to line-start position
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <param name="questionToken">The question mark token</param>
    /// <returns>The updated conditional expression</returns>
    private ConditionalExpressionSyntax MoveQuestionTokenToNextLine(ConditionalExpressionSyntax node,
                                                                    SyntaxToken questionToken)
    {
        var conditionLastToken = node.Condition.GetLastToken();
        var newQuestionTrailing = LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(questionToken.TrailingTrivia);
        var newConditionTrailing = AppendEndOfLine(conditionLastToken.TrailingTrivia);
        var newConditionLastToken = conditionLastToken.WithTrailingTrivia(newConditionTrailing);
        var newQuestionToken = questionToken.WithTrailingTrivia(newQuestionTrailing);
        var whenTrueFirstToken = node.WhenTrue.GetFirstToken();
        var newWhenTrueFirstToken = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(whenTrueFirstToken);

        if (newWhenTrueFirstToken.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            newWhenTrueFirstToken = newWhenTrueFirstToken.WithLeadingTrivia(newWhenTrueFirstToken.LeadingTrivia.Add(SyntaxFactory.Space));
        }

        return node.ReplaceTokens([conditionLastToken, questionToken, whenTrueFirstToken],
                                  (original, _) =>
                                  {
                                      if (original == conditionLastToken)
                                      {
                                          return newConditionLastToken;
                                      }

                                      if (original == questionToken)
                                      {
                                          return newQuestionToken;
                                      }

                                      return newWhenTrueFirstToken;
                                  });
    }

    /// <summary>
    /// Normalizes placement of the <c>:</c> token in a ternary expression
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <returns>The updated conditional expression</returns>
    private ConditionalExpressionSyntax NormalizeColonTokenPosition(ConditionalExpressionSyntax node)
    {
        var colonToken = node.ColonToken;

        if (LineBreakTriviaUtilities.HasTrailingEndOfLine(colonToken))
        {
            var whenTrueLastToken = node.WhenTrue.GetLastToken();
            var newColonTrailing = LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(colonToken.TrailingTrivia);
            var newWhenTrueTrailing = AppendEndOfLine(whenTrueLastToken.TrailingTrivia);
            var newWhenTrueLastToken = whenTrueLastToken.WithTrailingTrivia(newWhenTrueTrailing);
            var newColonToken = colonToken.WithTrailingTrivia(newColonTrailing);

            return node.ReplaceTokens([whenTrueLastToken, colonToken],
                                      (original, _) =>
                                      {
                                          if (original == whenTrueLastToken)
                                          {
                                              return newWhenTrueLastToken;
                                          }

                                          return newColonToken;
                                      });
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(colonToken))
        {
            return node;
        }

        return node.WithColonToken(PrependEndOfLine(colonToken));
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (BinaryExpressionSyntax)base.VisitBinaryExpression(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeBinaryOperatorPosition(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (ConditionalExpressionSyntax)base.VisitConditionalExpression(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeTernaryOperatorPosition(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        var isOutermost = IsOutermostChainInvocation(node);

        node = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost)
        {
            return NormalizeChain(node);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        var isOutermost = node.Parent is not ConditionalAccessExpressionSyntax;

        node = (ConditionalAccessExpressionSyntax)base.VisitConditionalAccessExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost && ContainsInvocation(node.WhenNotNull))
        {
            node = (ConditionalAccessExpressionSyntax)NormalizeChain(node);
            node = CollapseMemberBindingToQuestionToken(node);
        }

        return node;
    }

    #endregion // CSharpSyntaxRewriter
}