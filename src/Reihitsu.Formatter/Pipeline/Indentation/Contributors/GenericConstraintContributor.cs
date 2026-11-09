using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents generic <c>where</c> constraint clauses +1 level from the declaration (FMT-INDENT-017), and aligns the lines of
/// a clause that start with its colon, a constraint, or a comma between two constraints (FMT-INDENT-032)
/// </summary>
internal sealed class GenericConstraintContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Aligns the lines of a clause that start with its colon, a constraint, or a comma between two constraints
    /// (FMT-INDENT-032): a colon one level deeper than the <c>where</c> keyword, and the constraints and commas to the
    /// column of the first constraint, or to the column behind the colon and one space when the first constraint starts
    /// a line itself
    /// </summary>
    /// <param name="constraint">The constraint clause</param>
    /// <param name="model">The layout model</param>
    private static void AlignConstraints(TypeParameterConstraintClauseSyntax constraint, LayoutModel model)
    {
        if (constraint.Constraints.Count == 0)
        {
            return;
        }

        var colonToken = constraint.ColonToken;

        LayoutComputer.SetIfFirstOnLine(colonToken, LayoutComputer.GetAdjustedColumn(constraint.WhereKeyword, model) + FormattingContext.IndentSize, "GenericConstraintColon", model);

        var firstConstraintToken = constraint.Constraints[0].GetFirstToken();
        var constraintColumn = LayoutComputer.IsFirstOnLine(firstConstraintToken)
                                   ? LayoutComputer.GetAdjustedColumn(colonToken, model) + colonToken.Span.Length + 1
                                   : LayoutComputer.GetAdjustedColumn(firstConstraintToken, model);

        foreach (var typeParameterConstraint in constraint.Constraints)
        {
            LayoutComputer.SetIfFirstOnLine(typeParameterConstraint.GetFirstToken(), constraintColumn, "GenericConstraintAlignment", model);
        }

        ListElementAligner.AlignSeparators(constraint.Constraints, constraintColumn, "GenericConstraintAlignment", model);
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node is not TypeParameterConstraintClauseSyntax constraint)
        {
            return;
        }

        var parent = constraint.Parent;

        if (parent != null
            && model.TryGetPlacedColumn(LayoutComputer.GetLine(parent.GetFirstToken()), out var parentColumn))
        {
            LayoutComputer.SetIfFirstOnLine(constraint.WhereKeyword, parentColumn + FormattingContext.IndentSize, "GenericConstraint", model);
        }

        AlignConstraints(constraint, model);
    }

    #endregion // ILayoutContributor
}