using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Enumerations;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Structural transforms that convert expression-bodied members to block body, single-statement
/// property and indexer accessors to expression body, and get-only properties and indexers to
/// expression-bodied members.
/// Runs every enabled structural transform rewriter sequentially
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
    /// <see cref="AccessorExpressionBodyTransform"/> and <see cref="GetOnlyMemberExpressionBodyTransform"/> are the
    /// configurable transforms: both are left out when the context disables
    /// <see cref="ConfigurableStructuralTransforms.AccessorExpressionBody"/>. The accessor-level transform additionally
    /// requires C# 7.0, which introduced expression-bodied accessors and throw expressions, and the member-level
    /// transform requires C# 6.0, which introduced expression-bodied properties and indexers. The member-level
    /// transform runs after the accessor-level one, so a block-bodied getter that the accessor-level transform
    /// converts becomes an expression-bodied member in the same pass instead of the next one.
    /// Version gates read <see cref="FormattingContext.LanguageVersion"/> rather than a node's parse options: a node that
    /// an earlier rewriter replaced belongs to a new tree whose options fall back to the defaults, which would silently
    /// lift the gate
    /// </remarks>
    private static IReadOnlyList<CSharpSyntaxRewriter> CreateRewriters(FormattingContext context, CancellationToken cancellationToken)
    {
        var rewriters = new List<CSharpSyntaxRewriter>
                        {
                            new ControlFlowBraceTransform(context, cancellationToken),
                            new ExpressionBodiedMethodTransform(cancellationToken),
                            new ExpressionBodiedConstructorTransform(cancellationToken),
                            new ExpressionBodiedOperatorTransform(cancellationToken)
                        };

        if (context.IsStructuralTransformEnabled(ConfigurableStructuralTransforms.AccessorExpressionBody))
        {
            if (context.LanguageVersion >= LanguageVersion.CSharp7)
            {
                rewriters.Add(new AccessorExpressionBodyTransform(context, cancellationToken));
            }

            if (context.LanguageVersion >= LanguageVersion.CSharp6)
            {
                rewriters.Add(new GetOnlyMemberExpressionBodyTransform(context, cancellationToken));
            }
        }

        rewriters.AddRange([
                               new ExpressionBodiedConversionTransform(cancellationToken),
                               new ExpressionBodiedFinalizerTransform(cancellationToken),
                               new ExpressionBodiedLocalFunctionTransform(cancellationToken),
                               new EmptyTypeDeclarationSemicolonTransform(context, cancellationToken),
                               new EnumTrailingCommaRemovalTransform(cancellationToken),
                               new InitializerTrailingCommaRemovalTransform(cancellationToken),
                               new FieldDeclarationSplitTransform(context, cancellationToken),
                           ]);

        return rewriters;
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <inheritdoc/>
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