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
        var updatedLastExpression = TrailingCommaRemovalUtilities.PreserveTrailingTrivia(lastExpression, TrailingCommaRemovalUtilities.GetTriviaToPreserve(lastSeparator));
        var updatedExpressions = node.Expressions.Replace(lastExpression, updatedLastExpression);
        var updatedExpressionsAndSeparators = updatedExpressions.GetWithSeparators().RemoveAt(updatedExpressions.GetWithSeparators().Count - 1);

        return node.WithExpressions(SyntaxFactory.SeparatedList<ExpressionSyntax>(updatedExpressionsAndSeparators));
    }

    #endregion // CSharpSyntaxVisitor
}