using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns <c>?</c> and <c>:</c> tokens in conditional (ternary) expressions
/// relative to the condition's column.
/// </summary>
internal sealed class ConditionalExpressionContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        if (node is not ConditionalExpressionSyntax conditional)
        {
            return;
        }

        int operatorColumn;

        if (conditional.Parent is ConditionalExpressionSyntax parentConditional)
        {
            var parentOperator = parentConditional.WhenTrue == conditional
                                     ? parentConditional.QuestionToken
                                     : parentConditional.ColonToken;

            operatorColumn = LayoutComputer.GetAdjustedColumn(parentOperator, model) + FormattingContext.IndentSize;
        }
        else
        {
            var conditionColumn = LayoutComputer.GetAdjustedColumn(conditional.Condition.GetFirstToken(), model);

            operatorColumn = conditionColumn + FormattingContext.IndentSize;
        }

        LayoutComputer.SetIfFirstOnLine(conditional.QuestionToken, operatorColumn, "ConditionalExpression", model);
        LayoutComputer.SetIfFirstOnLine(conditional.ColonToken, operatorColumn, "ConditionalExpression", model);
    }

    #endregion // ILayoutContributor
}