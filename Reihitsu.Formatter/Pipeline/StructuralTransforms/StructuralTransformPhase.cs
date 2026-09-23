using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Structural transforms that convert expression-bodied members to block body and single-statement
/// property and indexer accessors to expression body.
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
    /// <remarks>
    /// <see cref="AccessorExpressionBodyTransform"/> runs after <see cref="ExpressionBodiedIndexerTransform"/>, so the
    /// block-bodied getter that the indexer transform synthesizes is converted in the same pass instead of the next one
    /// </remarks>
    private static IReadOnlyList<CSharpSyntaxRewriter> CreateRewriters(FormattingContext context,
                                                                       CancellationToken cancellationToken)
    {
        return [
                   new ControlFlowBraceTransform(context, cancellationToken),
                   new ExpressionBodiedMethodTransform(cancellationToken),
                   new ExpressionBodiedConstructorTransform(cancellationToken),
                   new ExpressionBodiedOperatorTransform(cancellationToken),
                   new ExpressionBodiedIndexerTransform(cancellationToken),
                   new AccessorExpressionBodyTransform(cancellationToken),
                   new ExpressionBodiedConversionTransform(cancellationToken),
                   new ExpressionBodiedFinalizerTransform(cancellationToken),
                   new ExpressionBodiedLocalFunctionTransform(cancellationToken),
                   new EmptyTypeDeclarationSemicolonTransform(cancellationToken),
                   new EnumTrailingCommaRemovalTransform(cancellationToken),
                   new InitializerTrailingCommaRemovalTransform(cancellationToken),
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