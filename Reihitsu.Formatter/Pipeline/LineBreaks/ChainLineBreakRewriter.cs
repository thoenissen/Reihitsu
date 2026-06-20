using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for method chains and conditional access chains
/// </summary>
internal sealed class ChainLineBreakRewriter : CSharpSyntaxRewriter
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
    public ChainLineBreakRewriter(FormattingContext context,
                                  CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
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

        return LineBreakTriviaUtilities.CollapseTokenToSameLine(node, memberBinding.OperatorToken);
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
            && ReihitsuFormatterHelpers.HasCommentDirectlyAbove(chainDot) == false)
        {
            return LineBreakTriviaUtilities.CollapseTokenToSameLine(node, chainDot);
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

        var previousToken = firstDot.GetPreviousToken();

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.WouldJoinIntoComment(previousToken, firstDot))
        {
            return;
        }

        replacements[firstDot] = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(firstDot);

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken))
        {
            replacements[previousToken] = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));
        }
    }

    /// <summary>
    /// Determines whether an access chain node is the outermost node of its chain, i.e. its parent
    /// is not another access/invocation node that would treat it as an inner link
    /// </summary>
    /// <param name="node">The chain node to check</param>
    /// <returns><see langword="true"/> if the node is the outermost chain node; otherwise, <see langword="false"/></returns>
    private static bool IsOutermostChainNode(SyntaxNode node)
    {
        return node.Parent is not MemberAccessExpressionSyntax
               && node.Parent is not MemberBindingExpressionSyntax
               && node.Parent is not InvocationExpressionSyntax
               && node.Parent is not ConditionalAccessExpressionSyntax
               && node.Parent is not ElementAccessExpressionSyntax
               && node.Parent is not PostfixUnaryExpressionSyntax;
    }

    /// <summary>
    /// Counts the number of invocations along the spine of an access chain, ignoring invocations
    /// that appear inside argument lists. A chain with at most one invocation is short enough to be
    /// rejoined onto a single line rather than kept wrapped
    /// </summary>
    /// <param name="expression">The chain expression to inspect</param>
    /// <returns>The number of invocations on the chain spine</returns>
    private static int CountSpineInvocations(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case InvocationExpressionSyntax invocation:
                return 1 + CountSpineInvocations(invocation.Expression);

            case MemberAccessExpressionSyntax memberAccess:
                return CountSpineInvocations(memberAccess.Expression);

            case ConditionalAccessExpressionSyntax conditionalAccess:
                return CountSpineInvocations(conditionalAccess.Expression) + CountSpineInvocations(conditionalAccess.WhenNotNull);

            case ElementAccessExpressionSyntax elementAccess:
                return CountSpineInvocations(elementAccess.Expression);

            case PostfixUnaryExpressionSyntax postfixUnary:
                return CountSpineInvocations(postfixUnary.Operand);

            default:
                return 0;
        }
    }

    /// <summary>
    /// Determines whether any invocation on the chain spine has a multi-line argument list. Such a
    /// chain is intentionally wrapped and must keep its alignment rather than being rejoined
    /// </summary>
    /// <param name="expression">The chain expression to inspect</param>
    /// <returns><see langword="true"/> if a spine invocation wraps its arguments; otherwise, <see langword="false"/></returns>
    private static bool HasMultiLineArgumentList(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case InvocationExpressionSyntax invocation:
                return LineBreakDetection.IsMultiLine(invocation.ArgumentList)
                       || HasMultiLineArgumentList(invocation.Expression);

            case MemberAccessExpressionSyntax memberAccess:
                return HasMultiLineArgumentList(memberAccess.Expression);

            case ConditionalAccessExpressionSyntax conditionalAccess:
                return HasMultiLineArgumentList(conditionalAccess.Expression)
                       || HasMultiLineArgumentList(conditionalAccess.WhenNotNull);

            case ElementAccessExpressionSyntax elementAccess:
                return HasMultiLineArgumentList(elementAccess.Expression);

            case PostfixUnaryExpressionSyntax postfixUnary:
                return HasMultiLineArgumentList(postfixUnary.Operand);

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether the chain contains a member access whose own expression is another member
    /// or conditional access. Such fluent chains (for example <c>x.Prop.Select(...)</c>) are kept
    /// wrapped and aligned rather than rejoined
    /// </summary>
    /// <param name="expression">The chain expression to inspect</param>
    /// <returns><see langword="true"/> if the chain has an intermediate member access; otherwise, <see langword="false"/></returns>
    private static bool ChainHasIntermediateMemberAccess(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case MemberAccessExpressionSyntax memberAccess:
                return memberAccess.Expression is MemberAccessExpressionSyntax
                       || memberAccess.Expression is ConditionalAccessExpressionSyntax
                       || ChainHasIntermediateMemberAccess(memberAccess.Expression);

            case ConditionalAccessExpressionSyntax conditionalAccess:
                return ChainHasIntermediateMemberAccess(conditionalAccess.Expression)
                       || ChainHasIntermediateMemberAccess(conditionalAccess.WhenNotNull);

            case InvocationExpressionSyntax invocation:
                return ChainHasIntermediateMemberAccess(invocation.Expression);

            case ElementAccessExpressionSyntax elementAccess:
                return ChainHasIntermediateMemberAccess(elementAccess.Expression);

            case PostfixUnaryExpressionSyntax postfixUnary:
                return ChainHasIntermediateMemberAccess(postfixUnary.Operand);

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether a short access chain (at most one spine invocation that does not wrap its
    /// arguments and has no intermediate member access) should be rejoined onto a single line
    /// </summary>
    /// <param name="expression">The chain expression to inspect</param>
    /// <returns><see langword="true"/> if the chain is eligible to be collapsed; otherwise, <see langword="false"/></returns>
    private static bool IsCollapsibleChain(ExpressionSyntax expression)
    {
        return CountSpineInvocations(expression) <= 1
               && HasMultiLineArgumentList(expression) == false
               && ChainHasIntermediateMemberAccess(expression) == false;
    }

    /// <summary>
    /// Collects the spine tokens of an access chain, separating the operator tokens (dots, the
    /// conditional-access <c>?</c>, the null-forgiving <c>!</c>) from the member-name and root tokens.
    /// Tokens inside argument lists are intentionally left untouched
    /// </summary>
    /// <param name="expression">The chain expression to walk</param>
    /// <param name="operatorTokens">The list that receives operator tokens</param>
    /// <param name="otherTokens">The list that receives member-name and root tokens</param>
    private static void CollectSpineTokens(ExpressionSyntax expression,
                                           List<SyntaxToken> operatorTokens,
                                           List<SyntaxToken> otherTokens)
    {
        switch (expression)
        {
            case MemberAccessExpressionSyntax memberAccess:
                {
                    CollectSpineTokens(memberAccess.Expression, operatorTokens, otherTokens);
                    operatorTokens.Add(memberAccess.OperatorToken);
                    otherTokens.Add(memberAccess.Name.GetFirstToken());
                }
                break;

            case MemberBindingExpressionSyntax memberBinding:
                {
                    operatorTokens.Add(memberBinding.OperatorToken);
                    otherTokens.Add(memberBinding.Name.GetFirstToken());
                }
                break;

            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    CollectSpineTokens(conditionalAccess.Expression, operatorTokens, otherTokens);
                    operatorTokens.Add(conditionalAccess.OperatorToken);
                    CollectSpineTokens(conditionalAccess.WhenNotNull, operatorTokens, otherTokens);
                }
                break;

            case InvocationExpressionSyntax invocation:
                {
                    CollectSpineTokens(invocation.Expression, operatorTokens, otherTokens);
                }
                break;

            case ElementAccessExpressionSyntax elementAccess:
                {
                    CollectSpineTokens(elementAccess.Expression, operatorTokens, otherTokens);
                }
                break;

            case PostfixUnaryExpressionSyntax postfixUnary:
                {
                    CollectSpineTokens(postfixUnary.Operand, operatorTokens, otherTokens);
                    operatorTokens.Add(postfixUnary.OperatorToken);
                }
                break;

            default:
                {
                    otherTokens.Add(expression.GetLastToken());
                }
                break;
        }
    }

    /// <summary>
    /// Determines whether any of the spine tokens carry comment trivia that would be lost or merged
    /// if the chain were rejoined onto a single line
    /// </summary>
    /// <param name="tokens">The spine tokens to inspect</param>
    /// <returns><see langword="true"/> if a token carries comment trivia; otherwise, <see langword="false"/></returns>
    private static bool SpineHasComment(List<SyntaxToken> tokens)
    {
        foreach (var token in tokens)
        {
            if (LineBreakTriviaUtilities.WouldJoinIntoComment(token, token))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Rejoins a short access chain (at most one invocation on its spine) onto a single line by
    /// removing the end-of-line and indentation trivia at every chain-link boundary. Argument lists
    /// and chains that carry comments are left untouched
    /// </summary>
    /// <param name="node">The outermost chain node</param>
    /// <returns>The node with its spine collapsed onto a single line</returns>
    private static SyntaxNode CollapseChainToSingleLine(SyntaxNode node)
    {
        if (node is not ExpressionSyntax expression)
        {
            return node;
        }

        var operatorTokens = new List<SyntaxToken>();
        var otherTokens = new List<SyntaxToken>();

        CollectSpineTokens(expression, operatorTokens, otherTokens);

        if (SpineHasComment(operatorTokens) || SpineHasComment(otherTokens))
        {
            return node;
        }

        // The root token keeps its leading trivia so the statement's indentation is preserved, and the
        // last token keeps its trailing trivia so a line break that belongs to the enclosing expression
        // (for example a wrapped binary operator after the chain) is not absorbed. Only the interior
        // chain-link boundaries are collapsed onto a single line.
        var rootToken = expression.GetFirstToken();
        var lastToken = expression.GetLastToken();
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var token in operatorTokens)
        {
            var current = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(token);

            if (token != lastToken)
            {
                current = current.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(current.TrailingTrivia)));
            }

            replacements[token] = current;
        }

        foreach (var token in otherTokens)
        {
            var current = token == rootToken
                              ? token
                              : LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(token);

            if (token != lastToken)
            {
                current = current.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(current.TrailingTrivia));
            }

            replacements[token] = current;
        }

        if (replacements.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Normalizes a method chain or conditional access chain
    /// </summary>
    /// <param name="node">The outermost chain node (invocation or conditional access)</param>
    /// <returns>The node with normalized chain line breaks</returns>
    private SyntaxNode NormalizeChain(SyntaxNode node)
    {
        var chainDots = new List<SyntaxToken>();

        ChainWalker.CollectInvokedLinkDots(node, chainDots);

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
            && ReihitsuFormatterHelpers.HasCommentDirectlyAbove(chainDots[0]))
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
        var endOfLine = SyntaxFactory.EndOfLine(_context.EndOfLine);

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

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var isOutermost = IsOutermostChainInvocation(node);

        node = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost)
        {
            if (IsCollapsibleChain(node))
            {
                return CollapseChainToSingleLine(node);
            }

            return NormalizeChain(node);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var isOutermost = node.Parent is not ConditionalAccessExpressionSyntax;

        node = (ConditionalAccessExpressionSyntax)base.VisitConditionalAccessExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost == false)
        {
            return node;
        }

        if (IsCollapsibleChain(node))
        {
            return CollapseChainToSingleLine(node);
        }

        if (ContainsInvocation(node.WhenNotNull))
        {
            node = (ConditionalAccessExpressionSyntax)NormalizeChain(node);
            node = CollapseMemberBindingToQuestionToken(node);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var isOutermost = IsOutermostChainNode(node);

        node = (MemberAccessExpressionSyntax)base.VisitMemberAccessExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost && IsCollapsibleChain(node))
        {
            return CollapseChainToSingleLine(node);
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}