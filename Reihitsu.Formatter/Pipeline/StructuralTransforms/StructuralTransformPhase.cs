using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Structural transforms that convert expression-bodied members to block body.
/// Runs all structural transform rewriters sequentially
/// </summary>
internal static class StructuralTransformPhase
{
    #region Methods

    /// <summary>
    /// Applies all structural transforms to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to transform</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transformed syntax node</returns>
    public static SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var current = root;

        current = new ExpressionBodiedMethodTransform(cancellationToken).Visit(current);
        current = new ExpressionBodiedConstructorTransform(cancellationToken).Visit(current);
        current = new ExpressionBodiedOperatorTransform(cancellationToken).Visit(current);
        current = new ExpressionBodiedIndexerTransform(cancellationToken).Visit(current);
        current = new ExpressionBodiedConversionTransform(cancellationToken).Visit(current);
        current = new ExpressionBodiedFinalizerTransform(cancellationToken).Visit(current);
        current = new ExpressionBodiedLocalFunctionTransform(cancellationToken).Visit(current);

        return current;
    }

    #endregion // Methods
}