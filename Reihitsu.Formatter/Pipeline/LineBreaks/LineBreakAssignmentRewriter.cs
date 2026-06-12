using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for assignments and equals-value clauses
/// </summary>
internal sealed class LineBreakAssignmentRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public LineBreakAssignmentRewriter(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Removes an end-of-line from a token's trailing trivia and ensures a trailing space remains
    /// </summary>
    /// <param name="token">The token to normalize</param>
    /// <returns>The normalized token</returns>
    private static SyntaxToken NormalizeTrailingTriviaToSameLine(SyntaxToken token)
    {
        var newTrailingTrivia = LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(token.TrailingTrivia);

        if (newTrailingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            newTrailingTrivia = newTrailingTrivia.Add(SyntaxFactory.Space);
        }

        return token.WithTrailingTrivia(newTrailingTrivia);
    }

    /// <summary>
    /// Moves a token onto the same line as the previous token and preserves a separating space
    /// </summary>
    /// <param name="token">The token to normalize</param>
    /// <returns>The normalized token</returns>
    private static SyntaxToken NormalizeLeadingTriviaToSameLine(SyntaxToken token)
    {
        var newToken = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(token);

        if (newToken.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            newToken = newToken.WithLeadingTrivia(newToken.LeadingTrivia.Add(SyntaxFactory.Space));
        }

        return newToken;
    }

    /// <summary>
    /// Keeps a collection initializer on the same line as the assignment operator
    /// </summary>
    /// <param name="node">The assignment expression to normalize</param>
    /// <returns>The updated assignment expression</returns>
    private static AssignmentExpressionSyntax NormalizeCollectionInitializerAssignment(AssignmentExpressionSyntax node)
    {
        if (node.Right is not InitializerExpressionSyntax initializer
            || initializer.IsKind(SyntaxKind.CollectionInitializerExpression) == false
            || LineBreakTriviaUtilities.HasTrailingEndOfLine(node.OperatorToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(node.OperatorToken, initializer.GetFirstToken()))
        {
            return node;
        }

        return node.WithOperatorToken(node.OperatorToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(node.OperatorToken.TrailingTrivia)));
    }

    /// <summary>
    /// Keeps a collection expression on the same line as the assignment operator
    /// </summary>
    /// <param name="node">The assignment expression to normalize</param>
    /// <returns>The updated assignment expression</returns>
    private static AssignmentExpressionSyntax NormalizeCollectionExpressionAssignment(AssignmentExpressionSyntax node)
    {
        if (node.Right is not CollectionExpressionSyntax
            || LineBreakTriviaUtilities.HasTrailingEndOfLine(node.OperatorToken) == false)
        {
            return node;
        }

        var operatorToken = node.OperatorToken;
        var openBracket = node.Right.GetFirstToken();

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(operatorToken, openBracket))
        {
            return node;
        }

        var newOperatorToken = NormalizeTrailingTriviaToSameLine(operatorToken);
        var newOpenBracket = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(openBracket);

        return node.ReplaceTokens([operatorToken, openBracket],
                                  (original, _) => original == operatorToken
                                                       ? newOperatorToken
                                                       : newOpenBracket);
    }

    /// <summary>
    /// Moves a simple-assignment operator onto the target line
    /// </summary>
    /// <param name="node">The assignment expression to normalize</param>
    /// <returns>The updated assignment expression</returns>
    private static AssignmentExpressionSyntax NormalizeSimpleAssignmentOperatorPlacement(AssignmentExpressionSyntax node)
    {
        var operatorToken = node.OperatorToken;

        if (node.IsKind(SyntaxKind.SimpleAssignmentExpression) == false
            || LineBreakTriviaUtilities.HasLeadingEndOfLine(operatorToken) == false)
        {
            return node;
        }

        var previousToken = operatorToken.GetPreviousToken();

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(previousToken, operatorToken))
        {
            return node;
        }

        var newOperatorToken = NormalizeLeadingTriviaToSameLine(operatorToken);

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken))
        {
            var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));

            return node.ReplaceTokens([previousToken, operatorToken],
                                      (original, _) => original == previousToken
                                                           ? newPreviousToken
                                                           : newOperatorToken);
        }

        return node.WithOperatorToken(newOperatorToken);
    }

    /// <summary>
    /// Moves a simple-assignment value onto the operator line
    /// </summary>
    /// <param name="node">The assignment expression to normalize</param>
    /// <returns>The updated assignment expression</returns>
    private static AssignmentExpressionSyntax NormalizeSimpleAssignmentValuePlacement(AssignmentExpressionSyntax node)
    {
        if (node.IsKind(SyntaxKind.SimpleAssignmentExpression) == false
            || node.Right is CollectionExpressionSyntax
            || node.Right is InitializerExpressionSyntax
            || LineBreakTriviaUtilities.HasTrailingEndOfLine(node.OperatorToken) == false)
        {
            return node;
        }

        var rightFirstToken = node.Right.GetFirstToken();

        if (rightFirstToken == default || LineBreakTriviaUtilities.HasLeadingEndOfLine(rightFirstToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(node.OperatorToken, rightFirstToken))
        {
            return node;
        }

        var operatorToken = node.OperatorToken;
        var newOperatorToken = NormalizeTrailingTriviaToSameLine(operatorToken);
        var newRightFirstToken = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(rightFirstToken);

        return node.ReplaceTokens([operatorToken, rightFirstToken],
                                  (original, _) => original == operatorToken
                                                       ? newOperatorToken
                                                       : newRightFirstToken);
    }

    /// <summary>
    /// Keeps a collection expression on the same line as the equals token
    /// </summary>
    /// <param name="node">The equals-value clause to normalize</param>
    /// <returns>The updated equals-value clause</returns>
    private static EqualsValueClauseSyntax NormalizeCollectionExpressionEqualsValue(EqualsValueClauseSyntax node)
    {
        if (node.Value is not CollectionExpressionSyntax
            || LineBreakTriviaUtilities.HasTrailingEndOfLine(node.EqualsToken) == false)
        {
            return node;
        }

        var equalsToken = node.EqualsToken;
        var openBracket = node.Value.GetFirstToken();

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(equalsToken, openBracket))
        {
            return node;
        }

        var newEqualsToken = NormalizeTrailingTriviaToSameLine(equalsToken);
        var newOpenBracket = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(openBracket);

        return node.ReplaceTokens([equalsToken, openBracket],
                                  (original, _) => original == equalsToken
                                                       ? newEqualsToken
                                                       : newOpenBracket);
    }

    /// <summary>
    /// Moves an equals-value clause value onto the equals line
    /// </summary>
    /// <param name="node">The equals-value clause to normalize</param>
    /// <returns>The updated equals-value clause</returns>
    private static EqualsValueClauseSyntax NormalizeEqualsValuePlacement(EqualsValueClauseSyntax node)
    {
        if (node.Value is CollectionExpressionSyntax
            || LineBreakTriviaUtilities.HasTrailingEndOfLine(node.EqualsToken) == false)
        {
            return node;
        }

        var valueFirstToken = node.Value.GetFirstToken();

        if (valueFirstToken == default || LineBreakTriviaUtilities.HasLeadingEndOfLine(valueFirstToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(node.EqualsToken, valueFirstToken))
        {
            return node;
        }

        var equalsToken = node.EqualsToken;
        var newEqualsToken = NormalizeTrailingTriviaToSameLine(equalsToken);
        var newValueFirstToken = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(valueFirstToken);

        return node.ReplaceTokens([equalsToken, valueFirstToken],
                                  (original, _) => original == equalsToken
                                                       ? newEqualsToken
                                                       : newValueFirstToken);
    }

    /// <summary>
    /// Moves a variable initializer equals token onto the declarator line
    /// </summary>
    /// <param name="node">The variable declarator to normalize</param>
    /// <returns>The updated variable declarator</returns>
    private static VariableDeclaratorSyntax NormalizeVariableInitializerOperatorPlacement(VariableDeclaratorSyntax node)
    {
        if (node.Initializer == null)
        {
            return node;
        }

        var equalsToken = node.Initializer.EqualsToken;

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(equalsToken) == false)
        {
            return node;
        }

        var previousToken = equalsToken.GetPreviousToken();

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(previousToken, equalsToken))
        {
            return node;
        }

        var newEqualsToken = NormalizeLeadingTriviaToSameLine(equalsToken);

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken))
        {
            var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));

            return node.ReplaceTokens([previousToken, equalsToken],
                                      (original, _) => original == previousToken
                                                           ? newPreviousToken
                                                           : newEqualsToken);
        }

        return node.ReplaceToken(equalsToken, newEqualsToken);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AssignmentExpressionSyntax)base.VisitAssignmentExpression(node);

        if (node == null)
        {
            return null;
        }

        node = NormalizeCollectionInitializerAssignment(node);
        node = NormalizeCollectionExpressionAssignment(node);
        node = NormalizeSimpleAssignmentOperatorPlacement(node);

        return NormalizeSimpleAssignmentValuePlacement(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (EqualsValueClauseSyntax)base.VisitEqualsValueClause(node);

        if (node == null)
        {
            return null;
        }

        node = NormalizeCollectionExpressionEqualsValue(node);

        return NormalizeEqualsValuePlacement(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (VariableDeclaratorSyntax)base.VisitVariableDeclarator(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeVariableInitializerOperatorPlacement(node);
    }

    #endregion // CSharpSyntaxVisitor
}