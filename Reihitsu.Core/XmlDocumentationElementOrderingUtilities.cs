using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers describing the canonical order of top-level XML documentation elements. The helpers
/// are reused by the XML documentation analyzers and their code fixes so that both rely on the same
/// ordering definition without one assembly reaching into the other's internals
/// </summary>
public static class XmlDocumentationElementOrderingUtilities
{
    #region Fields

    /// <summary>
    /// Rank used for XML documentation elements that are not part of the canonical order
    /// </summary>
    public const int UnknownElementRank = int.MaxValue;

    /// <summary>
    /// Canonical order of the top-level XML documentation elements
    /// </summary>
    public static readonly ImmutableArray<string> CanonicalElementOrder = [
                                                                              "summary",
                                                                              "typeparam",
                                                                              "param",
                                                                              "returns",
                                                                              "value",
                                                                              "exception",
                                                                              "example",
                                                                              "remarks"
                                                                          ];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Gets the canonical order rank of a top-level XML documentation element
    /// </summary>
    /// <param name="tagName">Tag name</param>
    /// <returns>The zero-based rank, or <see cref="UnknownElementRank"/> when the element is not part of the canonical order</returns>
    public static int GetCanonicalElementRank(string tagName)
    {
        for (var index = 0; index < CanonicalElementOrder.Length; index++)
        {
            if (string.Equals(CanonicalElementOrder[index], tagName, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return UnknownElementRank;
    }

    /// <summary>
    /// Gets the tag name of an XML node
    /// </summary>
    /// <param name="node">XML node</param>
    /// <returns>The tag name</returns>
    public static string GetTagName(XmlNodeSyntax node)
    {
        return node switch
               {
                   XmlElementSyntax element => element.StartTag.Name.LocalName.ValueText,
                   XmlEmptyElementSyntax element => element.Name.LocalName.ValueText,
                   _ => string.Empty
               };
    }

    #endregion // Methods
}