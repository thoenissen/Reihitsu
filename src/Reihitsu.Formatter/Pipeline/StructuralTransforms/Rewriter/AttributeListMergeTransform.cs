using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core.Enumerations;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Merges the attribute lists with the merged-list shape (<see cref="AttributeListShapeUtilities.UsesMergedListShape"/>)
/// that name the same target into the first of them, on every owner except an accessor, whose list shape follows the
/// layout the line-break phase gives its accessor list: the lists of a parameter or type parameter, and the <c>param:</c>
/// and <c>typevar:</c> lists of any other owner. The merge writes no whitespace and keeps one line break the author wrote
/// in the stretch it rewrites, so the line-break phase still finds the parameter written over several lines
/// </summary>
internal sealed class AttributeListMergeTransform : CSharpSyntaxRewriter
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
    public AttributeListMergeTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var updated = base.Visit(node);

        if (updated == null || updated is AccessorDeclarationSyntax)
        {
            return updated;
        }

        return AttributeListShapeUtilities.MergeAttributeLists(updated, AttributeListShapeUtilities.UsesMergedListShape, AttributeListMergeSeparators.None);
    }

    #endregion // CSharpSyntaxVisitor
}