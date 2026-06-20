using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns <c>?</c> and <c>:</c> tokens in conditional (ternary) expressions. The outermost
/// conditional anchors one indent deeper than its condition, and every nested conditional is
/// indented one further level, producing a consistent stair
/// </summary>
internal sealed class ConditionalExpressionContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Processes a conditional expression chain. Only the outermost conditional is processed; the
    /// anchor is computed once from the outermost condition's column and pushed to every nested
    /// conditional by recursion, so alignment does not depend on the contributor dispatch order or
    /// on columns contributed for sibling nodes
    /// </summary>
    /// <param name="conditional">The conditional expression</param>
    /// <param name="model">The layout model</param>
    private static void ProcessConditional(ConditionalExpressionSyntax conditional, LayoutModel model)
    {
        if (conditional.Parent is ConditionalExpressionSyntax)
        {
            return;
        }

        var operatorColumn = LayoutComputer.GetAdjustedColumn(conditional.Condition.GetFirstToken(), model)
                             + FormattingContext.IndentSize;

        AlignConditionalChain(conditional, operatorColumn, model);
    }

    /// <summary>
    /// Recursively aligns the <c>?</c> and <c>:</c> tokens of a conditional and indents each nested
    /// conditional one further level
    /// </summary>
    /// <param name="conditional">The conditional expression</param>
    /// <param name="operatorColumn">The column to align this conditional's operators to</param>
    /// <param name="model">The layout model</param>
    private static void AlignConditionalChain(ConditionalExpressionSyntax conditional, int operatorColumn, LayoutModel model)
    {
        LayoutComputer.SetIfFirstOnLine(conditional.QuestionToken, operatorColumn, "ConditionalExpression", model);
        LayoutComputer.SetIfFirstOnLine(conditional.ColonToken, operatorColumn, "ConditionalExpression", model);

        var nestedColumn = operatorColumn + FormattingContext.IndentSize;

        if (conditional.WhenTrue is ConditionalExpressionSyntax nestedTrue)
        {
            AlignConditionalChain(nestedTrue, nestedColumn, model);
        }

        if (conditional.WhenFalse is ConditionalExpressionSyntax nestedFalse)
        {
            AlignConditionalChain(nestedFalse, nestedColumn, model);
        }
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node is ConditionalExpressionSyntax conditional)
        {
            ProcessConditional(conditional, model);
        }
    }

    #endregion // ILayoutContributor
}