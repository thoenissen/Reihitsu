using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents generic <c>where</c> constraint clauses +1 level from the declaration.
/// </summary>
internal sealed class GenericConstraintContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        if (node is not TypeParameterConstraintClauseSyntax constraint)
        {
            return;
        }

        var parent = constraint.Parent;

        if (parent == null)
        {
            return;
        }

        var parentFirstToken = parent.GetFirstToken();
        var parentLine = LayoutComputer.GetLine(parentFirstToken);

        if (model.TryGetLayout(parentLine, out var parentLayout) == false)
        {
            return;
        }

        var whereColumn = parentLayout.Column + FormattingContext.IndentSize;

        LayoutComputer.SetIfFirstOnLine(constraint.WhereKeyword, whereColumn, "GenericConstraint", model);
    }

    #endregion // ILayoutContributor
}