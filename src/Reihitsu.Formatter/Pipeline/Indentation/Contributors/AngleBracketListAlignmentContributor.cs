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
/// block-indentation fallback and every continuation line slid to the enclosing block's column.
/// Unlike <see cref="BaseTypeListContributor"/>, this contributor does not skip a
/// single-element list: a one-argument list can still wrap after its opening bracket, and its closing
/// bracket can still land on its own line, so both need the same alignment as a multi-element list.
/// A line that starts inside an element at a token other than the element's first token is aligned
/// with that first token
/// </remarks>
internal sealed class AngleBracketListAlignmentContributor : ILayoutContributor
{
    #region Private methods

    /// <summary>
    /// Aligns the element starts, separators, and closing bracket of a wrapped angle-bracket list to its
    /// first element's column, then aligns every line that starts inside an element at a token other than
    /// the element's first token with that first token
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
        ListElementAligner.AlignToFirstElement(elements, closeToken, "AngleBracketList", model);
        ListElementAligner.AlignContinuations(elements, model);
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