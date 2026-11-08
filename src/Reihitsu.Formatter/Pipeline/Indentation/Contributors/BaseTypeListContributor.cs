using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns the later base types of a base list to the column of the first base type (FMT-INDENT-016), and every line that
/// starts inside a base type at a token other than its first token with that first token, whatever the number of types,
/// an enum's underlying type included (FMT-INDENT-013)
/// </summary>
internal sealed class BaseTypeListContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node is not BaseListSyntax baseList)
        {
            return;
        }

        if (baseList.Types.Count >= 2)
        {
            var firstBaseToken = baseList.Types[0].GetFirstToken();
            var firstBaseColumn = LayoutComputer.GetAdjustedColumn(firstBaseToken, model);

            for (var typeIndex = 1; typeIndex < baseList.Types.Count; typeIndex++)
            {
                var baseToken = baseList.Types[typeIndex].GetFirstToken();

                LayoutComputer.SetIfFirstOnLine(baseToken, firstBaseColumn, "BaseTypeList", model);
            }
        }

        // The continuations are aligned after the later base types, so a line inside a later type reads that type's final
        // column
        ListElementAligner.AlignContinuations(baseList.Types, model);
    }

    #endregion // ILayoutContributor
}