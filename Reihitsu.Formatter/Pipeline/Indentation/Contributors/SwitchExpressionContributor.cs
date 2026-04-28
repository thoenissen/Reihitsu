using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns switch expression braces to the <c>switch</c> keyword column
/// and arms at +1 level
/// </summary>
internal sealed class SwitchExpressionContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        if (node is not SwitchExpressionSyntax switchExpr)
        {
            return;
        }

        var switchColumn = LayoutComputer.GetAdjustedColumn(switchExpr.GoverningExpression.GetFirstToken(), model);

        LayoutComputer.SetIfFirstOnLine(switchExpr.OpenBraceToken, switchColumn, "SwitchExpression", model);
        LayoutComputer.SetIfFirstOnLine(switchExpr.CloseBraceToken, switchColumn, "SwitchExpression", model);

        var armColumn = switchColumn + FormattingContext.IndentSize;

        foreach (var arm in switchExpr.Arms)
        {
            var firstToken = arm.GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(firstToken, armColumn, "SwitchExpression", model);
        }
    }

    #endregion // ILayoutContributor
}