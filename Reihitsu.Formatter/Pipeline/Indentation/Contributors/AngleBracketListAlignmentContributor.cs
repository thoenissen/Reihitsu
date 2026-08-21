using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns the elements of a wrapped angle-bracket delimited list (type argument, type parameter, or
/// function-pointer parameter list) to the column of its first element, instead of the enclosing
/// block indentation level
/// </summary>
/// <remarks>
/// No contributor covered these lists before this one, so a list the line-break phase left wrapped —
/// an interior comment, a directive, or a multi-line element refuses the join — fell through to the
/// block-indentation fallback and every continuation line slid to the enclosing block's column
/// (issue #693). Unlike <see cref="BaseTypeListContributor"/>, this contributor does not skip a
/// single-element list: a one-argument list can still wrap after its opening bracket, and its closing
/// bracket can still land on its own line, so both need the same alignment as a multi-element list
/// </remarks>
internal sealed class AngleBracketListAlignmentContributor : ILayoutContributor
{
    #region Private methods

    /// <summary>
    /// Aligns every continuation line of an angle-bracket list to its first element's column
    /// </summary>
    /// <typeparam name="TElement">The type of the list's elements</typeparam>
    /// <param name="elements">The list's elements</param>
    /// <param name="closeToken">The closing angle bracket</param>
    /// <param name="model">The layout model</param>
    private static void Align<TElement>(SeparatedSyntaxList<TElement> elements,
                                        SyntaxToken closeToken,
                                        LayoutModel model)
        where TElement : SyntaxNode
    {
        if (elements.Count == 0)
        {
            return;
        }

        var anchorToken = elements[0].GetFirstToken();

        // An omitted type argument's token is zero-width (an unbound generic such as
        // "Dictionary<,>"), so it carries no real column to anchor to. Leave this list to the
        // block-indentation fallback pass 1 already computed, rather than align to a degenerate
        // position
        if (anchorToken.Span.IsEmpty)
        {
            return;
        }

        var anchorColumn = LayoutComputer.GetAdjustedColumn(anchorToken, model);

        for (var elementIndex = 1; elementIndex < elements.Count; elementIndex++)
        {
            LayoutComputer.SetIfFirstOnLine(elements[elementIndex].GetFirstToken(), anchorColumn, "AngleBracketList", model);
        }

        for (var separatorIndex = 0; separatorIndex < elements.SeparatorCount; separatorIndex++)
        {
            LayoutComputer.SetIfFirstOnLine(elements.GetSeparator(separatorIndex), anchorColumn, "AngleBracketList", model);
        }

        LayoutComputer.SetIfFirstOnLine(closeToken, anchorColumn, "AngleBracketList", model);
    }

    #endregion // Private methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node,
                           LayoutModel model,
                           FormattingContext context)
    {
        switch (node)
        {
            case TypeArgumentListSyntax typeArgumentList:
                Align(typeArgumentList.Arguments, typeArgumentList.GreaterThanToken, model);

                break;

            case TypeParameterListSyntax typeParameterList:
                Align(typeParameterList.Parameters, typeParameterList.GreaterThanToken, model);

                break;

            case FunctionPointerParameterListSyntax functionPointerParameterList:
                Align(functionPointerParameterList.Parameters, functionPointerParameterList.GreaterThanToken, model);

                break;
        }
    }

    #endregion // ILayoutContributor
}