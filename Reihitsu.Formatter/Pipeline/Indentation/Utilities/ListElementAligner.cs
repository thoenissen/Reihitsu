using Microsoft.CodeAnalysis;

using Reihitsu.Core;

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
    /// first token
    /// </summary>
    /// <typeparam name="TElement">The type of the list's elements</typeparam>
    /// <param name="elements">The list's elements</param>
    /// <param name="model">The layout model</param>
    internal static void AlignContinuations<TElement>(SeparatedSyntaxList<TElement> elements, LayoutModel model)
        where TElement : SyntaxNode
    {
        foreach (var element in elements)
        {
            var firstToken = element.GetFirstToken();

            if (firstToken.Span.IsEmpty)
            {
                continue;
            }

            int? column = null;

            foreach (var token in element.DescendantTokens())
            {
                if (token == firstToken
                    || LayoutComputer.IsFirstOnLine(token) == false
                    || IsInsideNestedIndentingScope(token, element))
                {
                    continue;
                }

                column ??= LayoutComputer.GetAdjustedColumn(firstToken, model);

                model.Set(LayoutComputer.GetLine(token), new TokenLayout(column.Value, "ListElementContinuation"));
            }
        }
    }

    /// <summary>
    /// Determines whether a token lies inside a brace scope nested within the element, such as the block of a lambda
    /// argument; lines inside such a scope keep the indentation their own scope assigns
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <param name="element">The list element containing the token</param>
    /// <returns><see langword="true"/> if an indenting scope lies between the token and the element</returns>
    private static bool IsInsideNestedIndentingScope(SyntaxToken token, SyntaxNode element)
    {
        for (var node = token.Parent; node != null && node != element; node = node.Parent)
        {
            if (SyntaxIndentationUtilities.IsIndentingScope(node))
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods
}