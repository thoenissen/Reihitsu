using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Structural transforms that convert expression-bodied members to block body.
/// Runs all structural transform rewriters sequentially
/// </summary>
internal sealed class StructuralTransformPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Creates the ordered structural transform rewriters
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ordered list of rewriters to execute</returns>
    private static IReadOnlyList<CSharpSyntaxRewriter> CreateRewriters(FormattingContext context,
                                                                       CancellationToken cancellationToken)
    {
        var expressionBodyConverter = new ExpressionBodyToBlockConverter();

        return [
                   new ControlFlowBraceTransform(cancellationToken),
                   new ExpressionBodiedMethodTransform(expressionBodyConverter, cancellationToken),
                   new ExpressionBodiedConstructorTransform(expressionBodyConverter, cancellationToken),
                   new ExpressionBodiedOperatorTransform(expressionBodyConverter, cancellationToken),
                   new ExpressionBodiedIndexerTransform(expressionBodyConverter, cancellationToken),
                   new ExpressionBodiedConversionTransform(expressionBodyConverter, cancellationToken),
                   new ExpressionBodiedFinalizerTransform(expressionBodyConverter, cancellationToken),
                   new ExpressionBodiedLocalFunctionTransform(expressionBodyConverter, cancellationToken),
                   new EmptyTypeDeclarationSemicolonTransform(cancellationToken),
                   new EnumTrailingCommaRemovalTransform(cancellationToken),
                   new InitializerTrailingCommaRemovalTransform(cancellationToken),
                   new InterpolationMarkerRemovalTransform(cancellationToken),
                   new FieldDeclarationSplitTransform(context, cancellationToken),
               ];
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <summary>
    /// Applies all structural transforms to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to transform</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transformed syntax node</returns>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var current = root;

        foreach (var rewriter in CreateRewriters(context, cancellationToken))
        {
            current = rewriter.Visit(current);

            cancellationToken.ThrowIfCancellationRequested();
        }

        return current;
    }

    #endregion // IFormattingPhase
}