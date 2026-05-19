using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for initializer and object creation expressions
/// </summary>
internal sealed class LineBreakInitializerRewriter : LineBreakRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public LineBreakInitializerRewriter(FormattingContext context,
                                        CancellationToken cancellationToken)
        : base(context,
               cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Removes trailing whitespace from the token immediately before an initializer's close brace
    /// when the close brace has been moved to a new line
    /// </summary>
    /// <param name="node">The initializer with a close brace to clean up</param>
    /// <returns>The initializer with trailing whitespace cleaned up</returns>
    private static InitializerExpressionSyntax CleanupTrailingWhitespaceBeforeCloseBrace(InitializerExpressionSyntax node)
    {
        var closeBrace = node.CloseBraceToken;

        if (closeBrace.LeadingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false)
        {
            return node;
        }

        var previousToken = closeBrace.GetPreviousToken();

        if (previousToken == default
            || previousToken.IsKind(SyntaxKind.None)
            || LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken)
            || previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            return node;
        }

        var newTrailing = LineBreakTriviaUtilities.RemoveTrailingWhitespace(previousToken.TrailingTrivia);
        var newPreviousToken = previousToken.WithTrailingTrivia(newTrailing);

        return node.ReplaceToken(previousToken, newPreviousToken);
    }

    /// <summary>
    /// Removes trailing whitespace from the token immediately before an initializer's open brace
    /// when the brace has been moved to a new line
    /// </summary>
    /// <typeparam name="TNode">The parent syntax node type</typeparam>
    /// <param name="node">The parent node containing the initializer</param>
    /// <param name="initializer">The initializer expression, or <see langword="null"/></param>
    /// <returns>The node with trailing whitespace cleaned up</returns>
    private static TNode CleanupTrailingWhitespaceBeforeInitializerBrace<TNode>(TNode node,
                                                                                InitializerExpressionSyntax initializer)
        where TNode : SyntaxNode
    {
        if (initializer == null)
        {
            return node;
        }

        var openBrace = initializer.OpenBraceToken;

        if (openBrace.LeadingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false)
        {
            return node;
        }

        var previousToken = openBrace.GetPreviousToken();

        if (previousToken == default
            || previousToken.IsKind(SyntaxKind.None)
            || LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken)
            || previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            return node;
        }

        var newTrailing = LineBreakTriviaUtilities.RemoveTrailingWhitespace(previousToken.TrailingTrivia);
        var newPreviousToken = previousToken.WithTrailingTrivia(newTrailing);

        return node.ReplaceToken(previousToken, newPreviousToken);
    }

    /// <summary>
    /// Ensures each expression in a collection or object initializer starts on its own line
    /// </summary>
    /// <param name="node">The initializer expression node</param>
    /// <returns>The initializer with each item on a separate line</returns>
    private InitializerExpressionSyntax EnsureInitializerItemsOnSeparateLines(InitializerExpressionSyntax node)
    {
        for (var expressionIndex = 0; expressionIndex < node.Expressions.Count; expressionIndex++)
        {
            var expression = node.Expressions[expressionIndex];
            var firstToken = expression.GetFirstToken();

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken) == false)
            {
                node = MoveTokenToNewLine(node, firstToken);
            }
        }

        return node;
    }

    /// <summary>
    /// Ensures each member in an anonymous object creation expression starts on its own line
    /// </summary>
    /// <param name="node">The anonymous object creation expression node</param>
    /// <returns>The node with each member on a separate line</returns>
    private AnonymousObjectCreationExpressionSyntax EnsureAnonymousObjectMembersOnSeparateLines(AnonymousObjectCreationExpressionSyntax node)
    {
        for (var memberIndex = 0; memberIndex < node.Initializers.Count; memberIndex++)
        {
            var member = node.Initializers[memberIndex];
            var firstToken = member.GetFirstToken();

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken) == false)
            {
                node = MoveTokenToNewLine(node, firstToken);
            }
        }

        return node;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        var originalParent = node.Parent;

        node = (InitializerExpressionSyntax)base.VisitInitializerExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.IsKind(SyntaxKind.ArrayInitializerExpression))
        {
            return node;
        }

        if (node.Expressions.Count > 1)
        {
            node = EnsureInitializerItemsOnSeparateLines(node);
        }

        if (node.IsKind(SyntaxKind.CollectionInitializerExpression)
            && originalParent is AssignmentExpressionSyntax)
        {
            node = node.WithOpenBraceToken(LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(node.OpenBraceToken));

            if (node.CloseBraceToken.IsMissing == false && LineBreakTriviaUtilities.HasLeadingEndOfLine(node.CloseBraceToken) == false)
            {
                node = node.WithCloseBraceToken(PrependEndOfLine(node.CloseBraceToken));
            }
        }
        else
        {
            node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        }

        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return CleanupTrailingWhitespaceBeforeCloseBrace(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.Initializer != null)
        {
            node = NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, node.Initializer.OpenBraceToken, (n, t) => n.ReplaceToken(node.Initializer.OpenBraceToken, t), blankLineCount: 0);
            node = EnsureFirstContentOnNewLine(node, node.Initializer.OpenBraceToken);
        }

        return CleanupTrailingWhitespaceBeforeInitializerBrace(node, node.Initializer);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (ImplicitObjectCreationExpressionSyntax)base.VisitImplicitObjectCreationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.Initializer != null)
        {
            node = NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, node.Initializer.OpenBraceToken, (n, t) => n.ReplaceToken(node.Initializer.OpenBraceToken, t), blankLineCount: 0);
            node = EnsureFirstContentOnNewLine(node, node.Initializer.OpenBraceToken);
        }

        return CleanupTrailingWhitespaceBeforeInitializerBrace(node, node.Initializer);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (AnonymousObjectCreationExpressionSyntax)base.VisitAnonymousObjectCreationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.Initializers.Count > 1)
        {
            node = EnsureAnonymousObjectMembersOnSeparateLines(node);
        }

        node = NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), blankLineCount: 0);
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = NormalizeGapBeforeToken(node, node.CloseBraceToken, blankLineCount: 0);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}