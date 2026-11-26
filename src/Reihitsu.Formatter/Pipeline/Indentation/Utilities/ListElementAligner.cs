using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Shared alignment rules for the elements of a wrapped separated list: anchoring every element start, separator, and
/// closer to the list's first element, and aligning the lines that start inside an element with that element's first
/// token
/// </summary>
/// <remarks>
/// A line can start inside an element at a token other than the element's own first token only when the line-break
/// phase could not join it to the previous line - a comment, a directive, or disabled text sits in between, or a
/// construct inside the element owns that line break. The list contributors position element first tokens,
/// separators, and closers, so without the continuation rule such a line would keep the block column of the enclosing
/// statement or declaration instead of the element's column. Callers run the continuation rule from the list node's own
/// contribution, so contributors of nodes nested inside the element (nested lists, attribute arguments, binary
/// expressions, lambdas) are visited later in the same sweep and still override the lines they own
/// </remarks>
internal static class ListElementAligner
{
    #region Methods

    /// <summary>
    /// Aligns the element starts after the first one, every separator, and the closing token of a wrapped list to the
    /// column of the list's first element
    /// </summary>
    /// <typeparam name="TElement">The type of the list's elements</typeparam>
    /// <param name="elements">The list's elements</param>
    /// <param name="closeToken">The list's closing token</param>
    /// <param name="layoutReason">The reason recorded with every layout this method writes</param>
    /// <param name="model">The layout model</param>
    internal static void AlignToFirstElement<TElement>(SeparatedSyntaxList<TElement> elements,
                                                       SyntaxToken closeToken,
                                                       string layoutReason,
                                                       LayoutModel model)
        where TElement : SyntaxNode
    {
        if (elements.Count == 0)
        {
            return;
        }

        // The first element's own token is the anchor, also when it has no text, such as the omitted type argument of a list
        // that stands on one line (Dictionary<,>); such a list never reaches this phase wrapped (FMT-BREAK-014, FMT-UNSUP-018)
        var anchorToken = elements[0].GetFirstToken(includeZeroWidth: true);
        var anchorColumn = LayoutComputer.GetAdjustedColumn(anchorToken, model);

        for (var elementIndex = 1; elementIndex < elements.Count; elementIndex++)
        {
            LayoutComputer.SetIfFirstOnLine(elements[elementIndex].GetFirstToken(), anchorColumn, layoutReason, model);
        }

        AlignSeparators(elements, anchorColumn, layoutReason, model);

        LayoutComputer.SetIfFirstOnLine(closeToken, anchorColumn, layoutReason, model);
    }

    /// <summary>
    /// Aligns every separator of a list that starts a line to the given column
    /// </summary>
    /// <typeparam name="TElement">The type of the list's elements</typeparam>
    /// <param name="elements">The list's elements</param>
    /// <param name="column">The list's element column</param>
    /// <param name="layoutReason">The reason recorded with every layout this method writes</param>
    /// <param name="model">The layout model</param>
    internal static void AlignSeparators<TElement>(SeparatedSyntaxList<TElement> elements,
                                                   int column,
                                                   string layoutReason,
                                                   LayoutModel model)
        where TElement : SyntaxNode
    {
        for (var separatorIndex = 0; separatorIndex < elements.SeparatorCount; separatorIndex++)
        {
            LayoutComputer.SetIfFirstOnLine(elements.GetSeparator(separatorIndex), column, layoutReason, model);
        }
    }

    /// <summary>
    /// Aligns every line that starts inside an element at a token other than the element's first token with that
    /// first token. A line inside a construct that lays out its own lines - as
    /// <see cref="SyntaxIndentationUtilities.IsOwningConstruct"/> decides, the same constructs whose interior wraps the
    /// line-break phase keeps - is left to that construct; the clauses of a query are the query clause
    /// contributor's (FMT-INDENT-035), and a line inside a clause is a continuation line of that clause (FMT-INDENT-034). The
    /// alignment is a fallback (<see cref="LayoutModel.SetUnlessPlaced"/>): a construct inside the element whose contributor
    /// places the line, such as a wrapped binary operator, a conditional, or a chain link, is the line's only owner
    /// </summary>
    /// <typeparam name="TElement">The type of the list's elements</typeparam>
    /// <param name="elements">The list's elements</param>
    /// <param name="model">The layout model</param>
    internal static void AlignContinuations<TElement>(SeparatedSyntaxList<TElement> elements, LayoutModel model)
        where TElement : SyntaxNode
    {
        foreach (var element in elements)
        {
            var firstToken = element.GetFirstToken(includeZeroWidth: true);

            foreach (var token in element.DescendantTokens())
            {
                if (token == firstToken
                    || LayoutComputer.IsFirstOnLine(token) == false)
                {
                    continue;
                }

                if (ListElementInteriorUtilities.FindOwningConstruct(token, element) != null)
                {
                    continue;
                }

                model.SetUnlessPlaced(LayoutComputer.GetLine(token), new TokenLayout(LayoutComputer.GetAdjustedColumn(firstToken, model), "ListElementContinuation"));
            }
        }
    }

    #endregion // Methods
}