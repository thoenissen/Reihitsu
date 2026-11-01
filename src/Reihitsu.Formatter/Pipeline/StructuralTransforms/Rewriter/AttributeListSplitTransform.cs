using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Splits each attribute list that holds several attributes into one list per attribute, on every owner except an
/// accessor, whose list shape follows the layout the line-break phase gives its accessor list, and except a parameter or
/// type parameter, whose lists are merged instead
/// </summary>
internal sealed class AttributeListSplitTransform : CSharpSyntaxRewriter
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
    public AttributeListSplitTransform(CancellationToken cancellationToken)
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

        return AttributeListShapeUtilities.SplitAttributeLists(updated);
    }

    #endregion // CSharpSyntaxVisitor
}