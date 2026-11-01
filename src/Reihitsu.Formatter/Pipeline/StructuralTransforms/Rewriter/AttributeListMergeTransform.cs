using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core.Enumerations;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Merges the attribute lists of a parameter or type parameter that name the same target into the first of them. The
/// merge writes no whitespace and keeps one line break the author wrote between the merged lists, so the line-break phase
/// still finds the parameter written over several lines
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

    #region Methods

    /// <summary>
    /// Merges the attribute lists of a parameter or type parameter
    /// </summary>
    /// <param name="owner">The parameter or type parameter</param>
    /// <returns>Updated owner node</returns>
    private static SyntaxNode MergeAttributeLists(SyntaxNode owner)
    {
        return AttributeListShapeUtilities.MergeAttributeLists(owner, AttributeListShapeUtilities.UsesMergedListShape, AttributeListMergeSeparators.None);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitParameter(ParameterSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var updated = base.VisitParameter(node);

        return updated == null ? null : MergeAttributeLists(updated);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitTypeParameter(TypeParameterSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var updated = base.VisitTypeParameter(node);

        return updated == null ? null : MergeAttributeLists(updated);
    }

    #endregion // CSharpSyntaxVisitor
}