using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns <c>?</c> and <c>:</c> tokens in conditional (ternary) expressions
/// relative to the condition's column
/// </summary>
internal sealed class ConditionalExpressionContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node is not ConditionalExpressionSyntax conditional)
        {
            return;
        }

        var operatorColumn = GetOperatorColumn(conditional, model) + FormattingContext.IndentSize;

        LayoutComputer.SetIfFirstOnLine(conditional.QuestionToken, operatorColumn, "ConditionalExpression", model);
        LayoutComputer.SetIfFirstOnLine(conditional.ColonToken, operatorColumn, "ConditionalExpression", model);
    }

    #endregion // ILayoutContributor

    #region Private methods

    /// <summary>
    /// Gets the base column the <c>?</c> and <c>:</c> operators are aligned one indent deeper than.
    /// For a conditional nested directly in the true or false branch of another conditional, the base
    /// is the parent operator column so every level is offset by a consistent indent. Otherwise the
    /// base is the column of the condition's first token
    /// </summary>
    /// <param name="conditional">The conditional expression being aligned</param>
    /// <param name="model">The layout model from the preceding passes</param>
    /// <returns>The base column for the operator alignment</returns>
    private static int GetOperatorColumn(ConditionalExpressionSyntax conditional, LayoutModel model)
    {
        if (conditional.Parent is ConditionalExpressionSyntax parent)
        {
            if (conditional == parent.WhenTrue)
            {
                return LayoutComputer.GetAdjustedColumn(parent.QuestionToken, model);
            }

            if (conditional == parent.WhenFalse)
            {
                return LayoutComputer.GetAdjustedColumn(parent.ColonToken, model);
            }
        }

        return LayoutComputer.GetAdjustedColumn(conditional.Condition.GetFirstToken(), model);
    }

    #endregion // Private methods
}