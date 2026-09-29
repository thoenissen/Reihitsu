using Microsoft.CodeAnalysis;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Aligns the continuation lines of the elements of a wrapped parameter-like list (parameter, bracketed parameter,
/// type parameter, or function-pointer parameter list) with the first token of the element they belong to
/// </summary>
/// <remarks>
/// A line can start inside an element at a token other than the element's own first token either because the
/// line-break phase refused to join it to the previous line - a comment, a directive, or disabled text sits in
/// between - or because the author wrapped the element and the line-break phase keeps that wrap. The list
/// contributors position only element first tokens, separators, and closers, so without this rule such a line would
/// keep the block column of the enclosing declaration instead of the element's column. Callers run this from the
/// list node's own contribution, so contributors of nodes nested inside the element (attribute arguments, type
/// arguments, binary expressions, lambdas) are visited later in the same sweep and still override the lines they own
/// </remarks>
internal static class ParameterContinuationAligner
{
    #region Methods

    /// <summary>
    /// Aligns every line that starts inside an element at a token other than the element's first token with that
    /// first token
    /// </summary>
    /// <typeparam name="TElement">The type of the list's elements</typeparam>
    /// <param name="elements">The list's elements</param>
    /// <param name="model">The layout model</param>
    internal static void Align<TElement>(SeparatedSyntaxList<TElement> elements, LayoutModel model)
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

                model.Set(LayoutComputer.GetLine(token), new TokenLayout(column.Value, "ParameterContinuation"));
            }
        }
    }

    /// <summary>
    /// Determines whether a token lies inside a brace scope nested within the element, such as the block of a lambda
    /// default value; lines inside such a scope keep the indentation their own scope assigns
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