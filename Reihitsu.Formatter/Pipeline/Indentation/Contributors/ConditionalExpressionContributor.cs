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
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        if (node is not ConditionalExpressionSyntax conditional)
        {
            return;
        }

        var operatorColumn = LayoutComputer.GetAdjustedColumn(conditional.Condition.GetFirstToken(), model)
                             + FormattingContext.IndentSize;

        LayoutComputer.SetIfFirstOnLine(conditional.QuestionToken, operatorColumn, "ConditionalExpression", model);
        LayoutComputer.SetIfFirstOnLine(conditional.ColonToken, operatorColumn, "ConditionalExpression", model);
    }

    #endregion // ILayoutContributor
}