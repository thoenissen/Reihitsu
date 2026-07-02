using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for initializer and object creation expressions
/// </summary>
internal sealed class LineBreakInitializerRewriter : CSharpSyntaxRewriter
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
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="gapNormalizer">The token gap normalizer</param>
    /// <param name="bracePlacer">The brace placer</param>
    public LineBreakInitializerRewriter(FormattingContext context,
                                        CancellationToken cancellationToken,
                                        TokenGapNormalizer gapNormalizer,
                                        BracePlacer bracePlacer)
    {
        _context = context;
        _cancellationToken = cancellationToken;
        _gapNormalizer = gapNormalizer;
        _bracePlacer = bracePlacer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Removes trailing whitespace from the token immediately before a brace or bracket
    /// when that token has been moved to a new line
    /// </summary>
    /// <typeparam name="TNode">The owning syntax node type</typeparam>
    /// <param name="node">The node that owns the token before <paramref name="token"/></param>
    /// <param name="token">The brace or bracket token to clean up before, or <see langword="default"/> to skip</param>
    /// <returns>The node with trailing whitespace cleaned up</returns>
    private static TNode CleanupTrailingWhitespaceBeforeToken<TNode>(TNode node,
                                                                     SyntaxToken token)
        where TNode : SyntaxNode
    {
        if (token.LeadingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false)
        {
            return node;
        }

        var previousToken = token.GetPreviousToken();

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
    /// Determines whether a recursive pattern is introduced directly by a case label,
    /// looking through enclosing combinator (<c>and</c>/<c>or</c>) and <c>not</c> patterns
    /// </summary>
    /// <param name="node">The recursive pattern</param>
    /// <returns><see langword="true"/> if the pattern is introduced by a case label; otherwise, <see langword="false"/></returns>
    private static bool IsIntroducedByCaseLabel(RecursivePatternSyntax node)
    {
        SyntaxNode current = node;

        while (current.Parent is BinaryPatternSyntax or UnaryPatternSyntax)
        {
            current = current.Parent;
        }

        return current.Parent is CasePatternSwitchLabelSyntax;
    }

    /// <summary>
    /// Ensures each element in a multi-line list starts on its own line
    /// </summary>
    /// <typeparam name="TNode">The owning syntax node type</typeparam>
    /// <typeparam name="TElement">The element type</typeparam>
    /// <param name="node">The node owning the elements</param>
    /// <param name="selectElements">Selects the element list from the current node</param>
    /// <returns>The node with each element on a separate line</returns>
    private TNode EnsureElementsOnSeparateLines<TNode, TElement>(TNode node,
                                                                 Func<TNode, SeparatedSyntaxList<TElement>> selectElements)
        where TNode : SyntaxNode
        where TElement : SyntaxNode
    {
        var elementCount = selectElements(node).Count;

        for (var elementIndex = 0; elementIndex < elementCount; elementIndex++)
        {
            var firstToken = selectElements(node)[elementIndex].GetFirstToken();

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken) == false)
            {
                node = LineBreakTriviaUtilities.MoveTokenToNewLine(node, firstToken, _context.EndOfLine);
            }
        }

        return node;
    }

    /// <summary>
    /// Ensures the opening brace of a recursive pattern's property clause starts on its own line.
    /// Unlike the close brace, the token preceding the open brace (the <c>is</c> keyword, a pattern
    /// type, or the containing subpattern's name) lives outside this rebuilt pattern node, so the
    /// gap normalizer would no-op and leave the brace in place. <see cref="LineBreakTriviaUtilities.MoveTokenToNewLine{TNode}"/>
    /// — the same helper used to split subpatterns onto their own lines — handles that cross-node case
    /// and keeps the layout idempotent for nested patterns
    /// </summary>
    /// <param name="node">The recursive pattern</param>
    /// <returns>The recursive pattern with the opening brace on its own line</returns>
    private RecursivePatternSyntax EnsureOpenBraceOnOwnLine(RecursivePatternSyntax node)
    {
        var openBrace = node.PropertyPatternClause.OpenBraceToken;

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(openBrace))
        {
            return node;
        }

        return LineBreakTriviaUtilities.MoveTokenToNewLine(node, openBrace, _context.EndOfLine);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

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
            node = EnsureElementsOnSeparateLines(node, static initializer => initializer.Expressions);
        }

        if (node.IsKind(SyntaxKind.CollectionInitializerExpression)
            && originalParent is AssignmentExpressionSyntax)
        {
            node = node.WithOpenBraceToken(LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(node.OpenBraceToken));

            if (node.CloseBraceToken.IsMissing == false && LineBreakTriviaUtilities.HasLeadingEndOfLine(node.CloseBraceToken) == false)
            {
                node = node.WithCloseBraceToken(LineBreakTriviaUtilities.PrependEndOfLine(node.CloseBraceToken, _context.EndOfLine));
            }
        }
        else
        {
            node = _bracePlacer.EnsureBraceOnOwnLine(node, n => n.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), n => n.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        }

        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return CleanupTrailingWhitespaceBeforeToken(node, node.CloseBraceToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.Initializer != null)
        {
            node = _gapNormalizer.NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, node.Initializer.OpenBraceToken, static (n, t) => n.WithInitializer(n.Initializer.WithOpenBraceToken(t)), blankLineCount: 0);
            node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.Initializer.OpenBraceToken);
        }

        return CleanupTrailingWhitespaceBeforeToken(node, node.Initializer != null ? node.Initializer.OpenBraceToken : default);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ImplicitObjectCreationExpressionSyntax)base.VisitImplicitObjectCreationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.Initializer != null)
        {
            node = _gapNormalizer.NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, node.Initializer.OpenBraceToken, static (n, t) => n.WithInitializer(n.Initializer.WithOpenBraceToken(t)), blankLineCount: 0);
            node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.Initializer.OpenBraceToken);
        }

        return CleanupTrailingWhitespaceBeforeToken(node, node.Initializer != null ? node.Initializer.OpenBraceToken : default);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AnonymousObjectCreationExpressionSyntax)base.VisitAnonymousObjectCreationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.Initializers.Count > 1)
        {
            node = EnsureElementsOnSeparateLines(node, static creation => creation.Initializers);
        }

        node = _gapNormalizer.NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), blankLineCount: 0);
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _gapNormalizer.NormalizeGapBeforeToken(node, node.CloseBraceToken, blankLineCount: 0);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitCollectionExpression(CollectionExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (CollectionExpressionSyntax)base.VisitCollectionExpression(node);

        if (node == null)
        {
            return null;
        }

        var isMultiLineCollection = LineBreakTriviaUtilities.SpansMultipleLines(node.OpenBracketToken, node.CloseBracketToken);

        if (isMultiLineCollection == false)
        {
            return node;
        }

        if (node.Elements.Count > 1)
        {
            node = EnsureElementsOnSeparateLines(node, static collection => collection.Elements);
        }

        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBracketToken);
        node = _gapNormalizer.NormalizeGapBeforeOwnedToken(node, node.CloseBracketToken, (n, t) => n.WithCloseBracketToken(t), blankLineCount: 0);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBracketToken);

        return CleanupTrailingWhitespaceBeforeToken(node, node.CloseBracketToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitListPattern(ListPatternSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ListPatternSyntax)base.VisitListPattern(node);

        if (node == null)
        {
            return null;
        }

        var isMultiLinePattern = LineBreakTriviaUtilities.SpansMultipleLines(node.OpenBracketToken, node.CloseBracketToken);

        if (isMultiLinePattern == false)
        {
            return node;
        }

        if (node.Patterns.Count > 1)
        {
            node = EnsureElementsOnSeparateLines(node, static pattern => pattern.Patterns);
        }

        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBracketToken);
        node = _gapNormalizer.NormalizeGapBeforeOwnedToken(node, node.CloseBracketToken, (n, t) => n.WithCloseBracketToken(t), blankLineCount: 0);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBracketToken);

        return CleanupTrailingWhitespaceBeforeToken(node, node.CloseBracketToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitRecursivePattern(RecursivePatternSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        // A case label keeps its open brace on the case line so the label is not confused with the
        // section body. This is determined before the rewrite because the parent is unreachable once
        // the node is rebuilt
        var keepOpenBraceInline = IsIntroducedByCaseLabel(node);

        node = (RecursivePatternSyntax)base.VisitRecursivePattern(node);

        if (node == null)
        {
            return null;
        }

        var propertyClause = node.PropertyPatternClause;

        if (propertyClause == null)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.SpansMultipleLines(propertyClause.OpenBraceToken, propertyClause.CloseBraceToken) == false)
        {
            return node;
        }

        if (propertyClause.Subpatterns.Count > 1)
        {
            var updatedClause = EnsureElementsOnSeparateLines(propertyClause, static clause => clause.Subpatterns);

            node = node.WithPropertyPatternClause(updatedClause);
        }

        if (keepOpenBraceInline == false)
        {
            node = EnsureOpenBraceOnOwnLine(node);
        }

        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.PropertyPatternClause.OpenBraceToken);
        node = _gapNormalizer.NormalizeGapBeforeOwnedToken(node, node.PropertyPatternClause.CloseBraceToken, static (n, t) => n.WithPropertyPatternClause(n.PropertyPatternClause.WithCloseBraceToken(t)), blankLineCount: 0);

        // A designation (for example "{ ... } shape") follows the close brace and must stay on the
        // brace's line, so the continuation break is only applied when the pattern has no designation
        if (node.Designation == null)
        {
            node = _bracePlacer.EnsureCloseBraceContinuation(node, node.PropertyPatternClause.CloseBraceToken);
        }

        return CleanupTrailingWhitespaceBeforeToken(node, node.PropertyPatternClause.CloseBraceToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitParenthesizedPattern(ParenthesizedPatternSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ParenthesizedPatternSyntax)base.VisitParenthesizedPattern(node);

        if (node == null)
        {
            return null;
        }

        if (LineBreakTriviaUtilities.SpansMultipleLines(node.OpenParenToken, node.CloseParenToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(node.OpenParenToken) == false)
        {
            node = LineBreakTriviaUtilities.MoveTokenToNewLine(node, node.OpenParenToken, _context.EndOfLine);
        }

        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenParenToken);
        node = _gapNormalizer.NormalizeGapBeforeOwnedToken(node, node.CloseParenToken, static (n, t) => n.WithCloseParenToken(t), blankLineCount: 0);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseParenToken);

        return CleanupTrailingWhitespaceBeforeToken(node, node.CloseParenToken);
    }

    #endregion // CSharpSyntaxVisitor
}