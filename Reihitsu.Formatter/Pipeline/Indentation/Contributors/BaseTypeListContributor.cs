using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns base types to the column of the first base type in the list
/// </summary>
internal sealed class BaseTypeListContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        if (node is not BaseListSyntax baseList)
        {
            return;
        }

        if (baseList.Types.Count < 2)
        {
            return;
        }

        var firstBaseToken = baseList.Types[0].GetFirstToken();
        var firstBaseColumn = LayoutComputer.GetAdjustedColumn(firstBaseToken, model);

        for (var typeIndex = 1; typeIndex < baseList.Types.Count; typeIndex++)
        {
            var baseToken = baseList.Types[typeIndex].GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(baseToken, firstBaseColumn, "BaseTypeList", model);
        }
    }

    #endregion // ILayoutContributor
}