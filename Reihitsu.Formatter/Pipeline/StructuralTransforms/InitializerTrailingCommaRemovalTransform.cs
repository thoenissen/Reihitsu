using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Removes trailing commas from final array and collection initializer items while preserving attached trivia
/// </summary>
internal sealed class InitializerTrailingCommaRemovalTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public InitializerTrailingCommaRemovalTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the trivia list contains content that should be preserved when removing a separator
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns><see langword="true"/> if the trivia contains comments, directives, or other non-formatting content</returns>
    private static bool ContainsNonFormattingTrivia(SyntaxTriviaList triviaList)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the separator trivia that should be preserved after removing the trailing comma
    /// </summary>
    /// <param name="separator">Separator token</param>
    /// <returns>The trivia that should stay attached to the final initializer expression</returns>
    private static SyntaxTriviaList GetTriviaToPreserve(SyntaxToken separator)
    {
        var triviaToPreserve = SyntaxFactory.TriviaList();

        if (ContainsNonFormattingTrivia(separator.LeadingTrivia))
        {
            triviaToPreserve = triviaToPreserve.AddRange(separator.LeadingTrivia);
        }

        return triviaToPreserve.AddRange(separator.TrailingTrivia);
    }

    /// <summary>
    /// Adds preserved separator trivia to the final token of the expression
    /// </summary>
    /// <param name="expression">Initializer expression</param>
    /// <param name="triviaToPreserve">Trivia to preserve</param>
    /// <returns>The updated initializer expression</returns>
    private static ExpressionSyntax PreserveTrailingTrivia(ExpressionSyntax expression, SyntaxTriviaList triviaToPreserve)
    {
        if (triviaToPreserve.Count == 0)
        {
            return expression;
        }

        var lastToken = expression.GetLastToken();
        var updatedLastToken = lastToken.WithTrailingTrivia(lastToken.TrailingTrivia.AddRange(triviaToPreserve));

        return expression.ReplaceToken(lastToken, updatedLastToken);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (InitializerExpressionSyntax)base.VisitInitializerExpression(node);

        if (node == null
            || (node.IsKind(SyntaxKind.ArrayInitializerExpression) == false && node.IsKind(SyntaxKind.CollectionInitializerExpression) == false)
            || node.Expressions.Count == 0
            || node.Expressions.SeparatorCount != node.Expressions.Count)
        {
            return node;
        }

        var lastExpression = node.Expressions[node.Expressions.Count - 1];
        var lastSeparator = node.Expressions.GetSeparator(node.Expressions.SeparatorCount - 1);
        var updatedLastExpression = PreserveTrailingTrivia(lastExpression, GetTriviaToPreserve(lastSeparator));
        var updatedExpressions = node.Expressions.Replace(lastExpression, updatedLastExpression);
        var updatedExpressionsAndSeparators = updatedExpressions.GetWithSeparators().RemoveAt(updatedExpressions.GetWithSeparators().Count - 1);

        return node.WithExpressions(SyntaxFactory.SeparatedList<ExpressionSyntax>(updatedExpressionsAndSeparators));
    }

    #endregion // CSharpSyntaxVisitor
}