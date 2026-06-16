using System.Collections.Immutable;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Expands and queries semantic XML documentation for declarations
/// </summary>
internal static class XmlDocumentationExpander
{
    #region Methods

    /// <summary>
    /// Parses the expanded documentation XML for the declaration
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The expanded XML documentation root, when available</returns>
    internal static XElement GetExpandedDocumentation(MemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var declaredSymbol = GetDeclaredSymbol(declaration, semanticModel, cancellationToken);

        return ParseExpandedDocumentation(declaredSymbol, cancellationToken);
    }

    /// <summary>
    /// Parses the expanded documentation XML for the enum member declaration
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The expanded XML documentation root, when available</returns>
    internal static XElement GetExpandedDocumentation(EnumMemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var declaredSymbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

        return ParseExpandedDocumentation(declaredSymbol, cancellationToken);
    }

    /// <summary>
    /// Determines whether the documentation contains a tag, considering expanded include XML
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="expandedDocumentation">Expanded documentation</param>
    /// <param name="tagName">Tag name</param>
    /// <returns><see langword="true"/> if the tag exists</returns>
    internal static bool HasTag(DocumentationCommentTriviaSyntax documentationComment, XElement expandedDocumentation, string tagName)
    {
        if (DirectDocumentationSyntaxChecker.HasDirectTag(documentationComment, tagName))
        {
            return true;
        }

        return expandedDocumentation?.Elements().Any(obj => string.Equals(obj.Name.LocalName, tagName, StringComparison.OrdinalIgnoreCase)) == true;
    }

    /// <summary>
    /// Determines whether an <c>&lt;inheritdoc/&gt;</c> tag is present
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="expandedDocumentation">Expanded documentation</param>
    /// <returns><see langword="true"/> if an inheritdoc tag exists</returns>
    internal static bool HasInheritdoc(DocumentationCommentTriviaSyntax documentationComment, XElement expandedDocumentation)
    {
        return HasTag(documentationComment, expandedDocumentation, "inheritdoc");
    }

    /// <summary>
    /// Gets the first expanded XML element for a tag name
    /// </summary>
    /// <param name="expandedDocumentation">Expanded documentation</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>The first matching element, when present</returns>
    internal static XElement GetFirstExpandedElement(XElement expandedDocumentation, string tagName)
    {
        return expandedDocumentation?.Elements().FirstOrDefault(obj => string.Equals(obj.Name.LocalName, tagName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the expanded XML elements for a tag name
    /// </summary>
    /// <param name="expandedDocumentation">Expanded documentation</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>The matching elements</returns>
    internal static ImmutableArray<XElement> GetExpandedElements(XElement expandedDocumentation, string tagName)
    {
        return expandedDocumentation?.Elements()
                                    .Where(obj => string.Equals(obj.Name.LocalName, tagName, StringComparison.OrdinalIgnoreCase))
                                    .ToImmutableArray()
                   ?? [];
    }

    /// <summary>
    /// Gets the declared symbol for the member
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The declared symbol, when available</returns>
    private static ISymbol GetDeclaredSymbol(MemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

        if (symbol != null)
        {
            return symbol;
        }

        if (declaration is FieldDeclarationSyntax fieldDeclaration)
        {
            return semanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables[0], cancellationToken);
        }

        if (declaration is EventFieldDeclarationSyntax eventFieldDeclaration)
        {
            return semanticModel.GetDeclaredSymbol(eventFieldDeclaration.Declaration.Variables[0], cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Parses expanded documentation for a declared symbol
    /// </summary>
    /// <param name="declaredSymbol">Declared symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The parsed XML root when available</returns>
    private static XElement ParseExpandedDocumentation(ISymbol declaredSymbol, CancellationToken cancellationToken)
    {
        var rawDocumentation = declaredSymbol?.GetDocumentationCommentXml(expandIncludes: true, cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(rawDocumentation))
        {
            return null;
        }

        try
        {
            return XElement.Parse(rawDocumentation);
        }
        catch (XmlException)
        {
            return null;
        }
    }

    #endregion // Methods
}