using System;
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
    /// Gets the first direct XML node with the specified tag name
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>The matching node, when present</returns>
    internal static XmlNodeSyntax GetFirstDirectTag(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        return GetDirectTags(documentationComment, tagName).FirstOrDefault();
    }

    /// <summary>
    /// Gets the first XML node with the specified tag name which is a direct child of the documentation comment
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>The matching node, when present</returns>
    /// <remarks>
    /// Unlike <see cref="GetFirstDirectTag"/> this never falls back to nested nodes, so a tag written inside a
    /// <c>&lt;code&gt;</c> or <c>&lt;example&gt;</c> sample is never returned. Rules which remove the tag they
    /// find must use this overload, otherwise the fix deletes sample text the documentation only illustrates
    /// </remarks>
    internal static XmlNodeSyntax GetFirstTopLevelTag(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        if (documentationComment == null)
        {
            return null;
        }

        return documentationComment.Content
                                   .Where(obj => obj is XmlElementSyntax or XmlEmptyElementSyntax)
                                   .FirstOrDefault(obj => string.Equals(XmlDocumentationElementOrderingUtilities.GetTagName(obj), tagName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all direct XML nodes with the specified tag name
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>Matching nodes</returns>
    internal static ImmutableArray<XmlNodeSyntax> GetDirectTags(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        if (documentationComment == null)
        {
            return [];
        }

        var directNodes = documentationComment.Content
                                              .Where(obj => obj is XmlElementSyntax or XmlEmptyElementSyntax)
                                              .Where(obj => string.Equals(XmlDocumentationElementOrderingUtilities.GetTagName(obj), tagName, StringComparison.OrdinalIgnoreCase))
                                              .ToImmutableArray();

        if (directNodes.Length > 0)
        {
            return directNodes;
        }

        return documentationComment.DescendantNodes()
                                   .Where(obj => obj is XmlElementSyntax or XmlEmptyElementSyntax)
                                   .Cast<XmlNodeSyntax>()
                                   .Where(obj => string.Equals(XmlDocumentationElementOrderingUtilities.GetTagName(obj), tagName, StringComparison.OrdinalIgnoreCase))
                                   .ToImmutableArray();
    }

    /// <summary>
    /// Determines whether the documentation contains a direct tag
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns><see langword="true"/> if the tag exists</returns>
    internal static bool HasDirectTag(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        return GetDirectTags(documentationComment, tagName).Length > 0;
    }

    #endregion // Methods
}