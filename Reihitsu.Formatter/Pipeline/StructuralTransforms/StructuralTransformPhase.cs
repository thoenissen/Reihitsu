using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Enumerations;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Structural transforms that convert expression-bodied members to block body and single-statement
/// property and indexer accessors to expression body.
/// Runs every enabled structural transform rewriter sequentially
/// </summary>
internal sealed class StructuralTransformPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Creates the ordered structural transform rewriters
    /// </summary>
    /// <param name="root">The syntax node the phase transforms, before any rewriter changed it</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ordered list of rewriters to execute</returns>
    /// <remarks>
    /// <see cref="AccessorExpressionBodyTransform"/> runs after <see cref="ExpressionBodiedIndexerTransform"/>, so the
    /// block-bodied getter that the indexer transform synthesizes is converted in the same pass instead of the next one.
    /// It is the one configurable transform: it is left out when the context disables
    /// <see cref="ConfigurableStructuralTransforms.AccessorExpressionBody"/>, and when the source's language version
    /// predates expression-bodied accessors and throw expressions
    /// </remarks>
    private static IReadOnlyList<CSharpSyntaxRewriter> CreateRewriters(SyntaxNode root,
                                                                       FormattingContext context,
                                                                       CancellationToken cancellationToken)
    {
        var rewriters = new List<CSharpSyntaxRewriter>
                        {
                            new ControlFlowBraceTransform(context, cancellationToken),
                            new ExpressionBodiedMethodTransform(cancellationToken),
                            new ExpressionBodiedConstructorTransform(cancellationToken),
                            new ExpressionBodiedOperatorTransform(cancellationToken),
                            new ExpressionBodiedIndexerTransform(cancellationToken)
                        };

        if (context.IsStructuralTransformEnabled(ConfigurableStructuralTransforms.AccessorExpressionBody)
            && SupportsExpressionBodiedAccessors(root))
        {
            rewriters.Add(new AccessorExpressionBodyTransform(context, cancellationToken));
        }

        rewriters.AddRange([
                               new ExpressionBodiedConversionTransform(cancellationToken),
                               new ExpressionBodiedFinalizerTransform(cancellationToken),
                               new ExpressionBodiedLocalFunctionTransform(cancellationToken),
                               new EmptyTypeDeclarationSemicolonTransform(cancellationToken),
                               new EnumTrailingCommaRemovalTransform(cancellationToken),
                               new InitializerTrailingCommaRemovalTransform(cancellationToken),
                               new FieldDeclarationSplitTransform(context, cancellationToken),
                           ]);

        return rewriters;
    }

    /// <summary>
    /// Determines whether the source's language version supports expression-bodied accessors and throw expressions,
    /// both introduced with C# 7.0
    /// </summary>
    /// <param name="root">The syntax node the phase transforms, before any rewriter changed it</param>
    /// <returns><see langword="true"/> if the conversion produces valid syntax for the source; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The version is read once from the untouched input: a node that an earlier rewriter replaced belongs to a new tree
    /// whose options fall back to the defaults, which would silently lift the gate
    /// </remarks>
    private static bool SupportsExpressionBodiedAccessors(SyntaxNode root)
    {
        return root.SyntaxTree.Options is not CSharpParseOptions parseOptions
               || parseOptions.LanguageVersion.MapSpecifiedToEffectiveVersion() >= LanguageVersion.CSharp7;
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

        foreach (var rewriter in CreateRewriters(root, context, cancellationToken))
        {
            current = rewriter.Visit(current);

            cancellationToken.ThrowIfCancellationRequested();
        }

        return current;
    }

    #endregion // IFormattingPhase
}