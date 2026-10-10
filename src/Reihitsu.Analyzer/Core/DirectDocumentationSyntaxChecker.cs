using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Performs direct XML documentation syntax inspection without semantic expansion
/// </summary>
internal static class DirectDocumentationSyntaxChecker
{
    #region Fields

    /// <summary>
    /// Tag names whose content illustrates usage instead of documenting the member
    /// </summary>
    private static readonly HashSet<string> _sampleTagNames = new(StringComparer.OrdinalIgnoreCase)
                                                              {
                                                                  "code",
                                                                  "example"
                                                              };

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Gets the XML documentation comment for the declaration
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns>The documentation comment, when present</returns>
    internal static DocumentationCommentTriviaSyntax GetDocumentationComment(SyntaxNode declaration)
    {
        return declaration.GetLeadingTrivia()
                          .Select(obj => obj.GetStructure())
                          .OfType<DocumentationCommentTriviaSyntax>()
                          .FirstOrDefault();
    }

    /// <summary>
    /// Gets the first top-level XML node with the specified tag name
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>The matching node, when present</returns>
    internal static XmlNodeSyntax GetFirstDirectTag(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        return GetDirectTags(documentationComment, tagName).FirstOrDefault();
    }

    /// <summary>
    /// Gets the first XML node with the specified tag name which documents the member itself
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>The matching node, when present</returns>
    /// <remarks>
    /// This searches the nodes returned by <see cref="GetTagsIncludingNested"/> but skips anything inside a
    /// <c>&lt;code&gt;</c> or <c>&lt;example&gt;</c> sample, where a tag illustrates usage instead of documenting
    /// the member. Rules which remove the tag they find must use this overload, otherwise the fix deletes sample
    /// text. Tags nested in prose such as <c>&lt;summary&gt;</c> or <c>&lt;remarks&gt;</c> are still returned —
    /// they document the member and remain real violations
    /// </remarks>
    internal static XmlNodeSyntax GetFirstDocumentingTag(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        return GetTagsIncludingNested(documentationComment, tagName).FirstOrDefault(obj => IsInsideSample(obj) == false);
    }

    /// <summary>
    /// Gets the XML nodes with the specified tag name. Top-level nodes win; when the documentation comment has no
    /// top-level match the search falls back to nested nodes, including content inside samples
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>Matching nodes</returns>
    internal static ImmutableArray<XmlNodeSyntax> GetTagsIncludingNested(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        var directNodes = GetDirectTags(documentationComment, tagName);

        if (directNodes.Length > 0)
        {
            return directNodes;
        }

        if (documentationComment == null)
        {
            return [];
        }

        return documentationComment.DescendantNodes()
                                   .Where(obj => obj is XmlElementSyntax or XmlEmptyElementSyntax)
                                   .Cast<XmlNodeSyntax>()
                                   .Where(obj => string.Equals(XmlDocumentationElementOrderingUtilities.GetTagName(obj), tagName, StringComparison.OrdinalIgnoreCase))
                                   .ToImmutableArray();
    }

    /// <summary>
    /// Gets the top-level XML nodes with the specified tag name in source order
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>Matching top-level nodes</returns>
    internal static ImmutableArray<XmlNodeSyntax> GetDirectTags(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        if (documentationComment == null)
        {
            return [];
        }

        return documentationComment.Content
                                   .Where(obj => obj is XmlElementSyntax or XmlEmptyElementSyntax)
                                   .Where(obj => string.Equals(XmlDocumentationElementOrderingUtilities.GetTagName(obj), tagName, StringComparison.OrdinalIgnoreCase))
                                   .ToImmutableArray();
    }

    /// <summary>
    /// Determines whether the documentation contains a top-level tag
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns><see langword="true"/> if the top-level tag exists</returns>
    internal static bool HasDirectTag(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        return GetDirectTags(documentationComment, tagName).Length > 0;
    }

    /// <summary>
    /// Determines whether the node is contained in a code or example sample
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if an ancestor illustrates usage instead of documenting the member</returns>
    private static bool IsInsideSample(XmlNodeSyntax node)
    {
        for (var ancestor = node.Parent; ancestor != null; ancestor = ancestor.Parent)
        {
            if (ancestor is XmlElementSyntax element
                && _sampleTagNames.Contains(XmlDocumentationElementOrderingUtilities.GetTagName(element)))
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods
}