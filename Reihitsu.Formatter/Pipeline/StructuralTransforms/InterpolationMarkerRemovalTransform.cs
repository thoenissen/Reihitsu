using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Removes unnecessary interpolation markers from interpolated strings without interpolation holes
/// </summary>
internal sealed class InterpolationMarkerRemovalTransform : CSharpSyntaxRewriter
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
    public InterpolationMarkerRemovalTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (InterpolatedStringExpressionSyntax)base.VisitInterpolatedStringExpression(node);

        if (node == null
            || StringInterpolationUtilities.HasInterpolations(node))
        {
            return node;
        }

        return StringInterpolationUtilities.RemoveInterpolationMarkers(node);
    }

    #endregion // CSharpSyntaxVisitor
}