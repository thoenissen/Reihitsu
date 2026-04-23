using System.Collections.Immutable;
using System.Threading;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared helpers for XML documentation analyzers.
/// </summary>
internal static class DocumentationAnalysisUtilities
{
    #region Fields

    /// <summary>
    /// Syntax kinds which can carry element-level XML documentation.
    /// </summary>
    internal static readonly SyntaxKind[] DocumentableDeclarationKinds = [
                                                                             SyntaxKind.ClassDeclaration,
                                                                             SyntaxKind.StructDeclaration,
                                                                             SyntaxKind.InterfaceDeclaration,
                                                                             SyntaxKind.RecordDeclaration,
                                                                             SyntaxKind.RecordStructDeclaration,
                                                                             SyntaxKind.EnumDeclaration,
                                                                             SyntaxKind.DelegateDeclaration,
                                                                             SyntaxKind.ConstructorDeclaration,
                                                                             SyntaxKind.DestructorDeclaration,
                                                                             SyntaxKind.MethodDeclaration,
                                                                             SyntaxKind.PropertyDeclaration,
                                                                             SyntaxKind.IndexerDeclaration,
                                                                             SyntaxKind.FieldDeclaration,
                                                                             SyntaxKind.EventDeclaration,
                                                                             SyntaxKind.EventFieldDeclaration
                                                                         ];

    /// <summary>
    /// Syntax kinds which can declare parameters.
    /// </summary>
    internal static readonly SyntaxKind[] ParameterOwnerKinds = [
                                                                    SyntaxKind.MethodDeclaration,
                                                                    SyntaxKind.ConstructorDeclaration,
                                                                    SyntaxKind.DelegateDeclaration,
                                                                    SyntaxKind.IndexerDeclaration
                                                                ];

    /// <summary>
    /// Syntax kinds which can declare generic type parameters.
    /// </summary>
    internal static readonly SyntaxKind[] TypeParameterOwnerKinds = [
                                                                        SyntaxKind.ClassDeclaration,
                                                                        SyntaxKind.StructDeclaration,
                                                                        SyntaxKind.InterfaceDeclaration,
                                                                        SyntaxKind.RecordDeclaration,
                                                                        SyntaxKind.RecordStructDeclaration,
                                                                        SyntaxKind.DelegateDeclaration,
                                                                        SyntaxKind.MethodDeclaration
                                                                    ];

    /// <summary>
    /// Syntax kinds which can declare return values.
    /// </summary>
    internal static readonly SyntaxKind[] ReturnValueOwnerKinds = [
                                                                      SyntaxKind.MethodDeclaration,
                                                                      SyntaxKind.DelegateDeclaration
                                                                  ];

    /// <summary>
    /// Syntax kinds whose documentation summary text should be validated.
    /// </summary>
    internal static readonly SyntaxKind[] SummaryDocumentationKinds = [
                                                                          .. DocumentableDeclarationKinds,
                                                                          SyntaxKind.EnumMemberDeclaration
                                                                      ];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Determines whether the declaration requires documentation for this repository's fixed StyleCop settings.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns><see langword="true"/> if documentation is required</returns>
    internal static bool NeedsDocumentation(MemberDeclarationSyntax declaration)
    {
        return IsExplicitInterfaceImplementation(declaration) == false;
    }

    /// <summary>
    /// Determines whether the declaration is partial.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration is partial</returns>
    internal static bool IsPartialElement(MemberDeclarationSyntax declaration)
    {
        return declaration switch
               {
                   TypeDeclarationSyntax typeDeclaration => typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword),
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword),
                   _ => false
               };
    }

    /// <summary>
    /// Gets the preferred diagnostic location for the declaration.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns>Diagnostic location</returns>
    internal static Location GetDiagnosticLocation(MemberDeclarationSyntax declaration)
    {
        return declaration switch
               {
                   TypeDeclarationSyntax typeDeclaration => typeDeclaration.Identifier.GetLocation(),
                   EnumDeclarationSyntax enumDeclaration => enumDeclaration.Identifier.GetLocation(),
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.GetLocation(),
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.Identifier.GetLocation(),
                   ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.Identifier.GetLocation(),
                   DestructorDeclarationSyntax destructorDeclaration => destructorDeclaration.Identifier.GetLocation(),
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Identifier.GetLocation(),
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.ThisKeyword.GetLocation(),
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables[0].Identifier.GetLocation(),
                   EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.GetLocation(),
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.Declaration.Variables[0].Identifier.GetLocation(),
                   _ => declaration.GetLocation()
               };
    }

    /// <summary>
    /// Gets the preferred diagnostic location for the enum member declaration.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns>Diagnostic location</returns>
    internal static Location GetDiagnosticLocation(EnumMemberDeclarationSyntax declaration)
    {
        return declaration.Identifier.GetLocation();
    }

    /// <summary>
    /// Gets the XML documentation comment for the declaration.
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
    /// Gets the first direct XML node with the specified tag name.
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>The matching node, when present</returns>
    internal static XmlNodeSyntax GetFirstDirectTag(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        return GetDirectTags(documentationComment, tagName).FirstOrDefault();
    }

    /// <summary>
    /// Gets all direct XML nodes with the specified tag name.
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
                                              .Where(obj => string.Equals(GetTagName(obj), tagName, StringComparison.OrdinalIgnoreCase))
                                              .ToImmutableArray();

        if (directNodes.Length > 0)
        {
            return directNodes;
        }

        return documentationComment.DescendantNodes()
                                   .Where(obj => obj is XmlElementSyntax or XmlEmptyElementSyntax)
                                   .Cast<XmlNodeSyntax>()
                                   .Where(obj => string.Equals(GetTagName(obj), tagName, StringComparison.OrdinalIgnoreCase))
                                   .ToImmutableArray();
    }

    /// <summary>
    /// Determines whether the XML node is empty.
    /// </summary>
    /// <param name="node">XML node</param>
    /// <returns><see langword="true"/> if the node has no meaningful content</returns>
    internal static bool IsEmpty(XmlNodeSyntax node)
    {
        return node switch
               {
                   XmlEmptyElementSyntax => true,
                   XmlElementSyntax element => element.Content.Any(HasMeaningfulContent) == false,
                   _ => true
               };
    }

    /// <summary>
    /// Determines whether the XML element is empty.
    /// </summary>
    /// <param name="element">Element</param>
    /// <returns><see langword="true"/> if the element has no meaningful content</returns>
    internal static bool IsEmpty(XElement element)
    {
        if (element == null)
        {
            return true;
        }

        return element.Elements().Any() == false
               && string.IsNullOrWhiteSpace(element.Value);
    }

    /// <summary>
    /// Parses the expanded documentation XML for the declaration.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The expanded XML documentation root, when available</returns>
    internal static XElement GetExpandedDocumentation(MemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var declaredSymbol = GetDeclaredSymbol(declaration, semanticModel, cancellationToken);
        var rawDocumentation = declaredSymbol?.GetDocumentationCommentXml(expandIncludes: true, cancellationToken: cancellationToken);

        return string.IsNullOrWhiteSpace(rawDocumentation)
                   ? null
                   : XElement.Parse(rawDocumentation);
    }

    /// <summary>
    /// Determines whether the documentation contains a tag, considering expanded include XML.
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="expandedDocumentation">Expanded documentation</param>
    /// <param name="tagName">Tag name</param>
    /// <returns><see langword="true"/> if the tag exists</returns>
    internal static bool HasTag(DocumentationCommentTriviaSyntax documentationComment, XElement expandedDocumentation, string tagName)
    {
        if (HasDirectTag(documentationComment, tagName))
        {
            return true;
        }

        return expandedDocumentation?.Elements().Any(obj => string.Equals(obj.Name.LocalName, tagName, StringComparison.OrdinalIgnoreCase)) == true;
    }

    /// <summary>
    /// Gets the first expanded XML element for a tag name.
    /// </summary>
    /// <param name="expandedDocumentation">Expanded documentation</param>
    /// <param name="tagName">Tag name</param>
    /// <returns>The first matching element, when present</returns>
    internal static XElement GetFirstExpandedElement(XElement expandedDocumentation, string tagName)
    {
        return expandedDocumentation?.Elements().FirstOrDefault(obj => string.Equals(obj.Name.LocalName, tagName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the expanded XML elements for a tag name.
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
    /// Gets the declared parameters for a declaration.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns>Parameters</returns>
    internal static ImmutableArray<ParameterSyntax> GetParameters(MemberDeclarationSyntax declaration)
    {
        return declaration switch
               {
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.ParameterList.Parameters.ToImmutableArray(),
                   ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.ParameterList.Parameters.ToImmutableArray(),
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.ParameterList.Parameters.ToImmutableArray(),
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.ParameterList.Parameters.ToImmutableArray(),
                   _ => []
               };
    }

    /// <summary>
    /// Gets the declared generic type parameters for a declaration.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns>Type parameters</returns>
    internal static ImmutableArray<TypeParameterSyntax> GetTypeParameters(MemberDeclarationSyntax declaration)
    {
        return declaration switch
               {
                   TypeDeclarationSyntax { TypeParameterList: not null } typeDeclaration => typeDeclaration.TypeParameterList.Parameters.ToImmutableArray(),
                   MethodDeclarationSyntax { TypeParameterList: not null } methodDeclaration => methodDeclaration.TypeParameterList.Parameters.ToImmutableArray(),
                   DelegateDeclarationSyntax { TypeParameterList: not null } delegateDeclaration => delegateDeclaration.TypeParameterList.Parameters.ToImmutableArray(),
                   _ => []
               };
    }

    /// <summary>
    /// Determines whether the declaration returns <see langword="void"/>.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns><see langword="true"/> if the return type is void</returns>
    internal static bool IsVoidReturnType(MemberDeclarationSyntax declaration)
    {
        var returnType = declaration switch
                         {
                             MethodDeclarationSyntax methodDeclaration => methodDeclaration.ReturnType,
                             DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.ReturnType,
                             _ => null
                         };

        return returnType is PredefinedTypeSyntax predefinedType
               && predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword);
    }

    /// <summary>
    /// Determines whether the declaration returns a non-void value.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="returnType">Return type</param>
    /// <returns><see langword="true"/> if the declaration has a non-void return type</returns>
    internal static bool TryGetNonVoidReturnType(MemberDeclarationSyntax declaration, out TypeSyntax returnType)
    {
        returnType = declaration switch
                     {
                         MethodDeclarationSyntax methodDeclaration => methodDeclaration.ReturnType,
                         DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.ReturnType,
                         _ => null
                     };

        return returnType != null
               && (returnType is not PredefinedTypeSyntax predefinedType || predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword) == false);
    }

    /// <summary>
    /// Determines whether an <c>&lt;inheritdoc/&gt;</c> tag is present.
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="expandedDocumentation">Expanded documentation</param>
    /// <returns><see langword="true"/> if an inheritdoc tag exists</returns>
    internal static bool HasInheritdoc(DocumentationCommentTriviaSyntax documentationComment, XElement expandedDocumentation)
    {
        return HasTag(documentationComment, expandedDocumentation, "inheritdoc");
    }

    /// <summary>
    /// Determines whether an <c>&lt;inheritdoc/&gt;</c> tag has a cref attribute.
    /// </summary>
    /// <param name="node">XML node</param>
    /// <returns><see langword="true"/> if a cref attribute exists</returns>
    internal static bool HasCrefAttribute(XmlNodeSyntax node)
    {
        return node switch
               {
                   XmlElementSyntax element => element.StartTag.Attributes.Any(SyntaxKind.XmlCrefAttribute),
                   XmlEmptyElementSyntax element => element.Attributes.Any(SyntaxKind.XmlCrefAttribute),
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether an <c>&lt;inheritdoc/&gt;</c> element has a cref attribute.
    /// </summary>
    /// <param name="element">Expanded XML element</param>
    /// <returns><see langword="true"/> if a cref attribute exists</returns>
    internal static bool HasCrefAttribute(XElement element)
    {
        return element?.Attribute("cref") != null;
    }

    /// <summary>
    /// Gets the <c>name</c> attribute value from an XML node.
    /// </summary>
    /// <param name="node">XML node</param>
    /// <returns>The attribute value, when present</returns>
    internal static string GetNameAttributeValue(XmlNodeSyntax node)
    {
        return node switch
               {
                   XmlElementSyntax element => GetNameAttributeValue(element.StartTag.Attributes),
                   XmlEmptyElementSyntax element => GetNameAttributeValue(element.Attributes),
                   _ => null
               };
    }

    /// <summary>
    /// Determines whether the declaration can inherit documentation.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the declaration inherits or implements another declaration</returns>
    internal static bool CanInheritDocumentation(MemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (declaration is TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.BaseList?.Types.Any() == true;
        }

        var declaredSymbol = GetDeclaredSymbol(declaration, semanticModel, cancellationToken);

        return declaredSymbol switch
               {
                   IMethodSymbol { MethodKind: MethodKind.Constructor } constructorSymbol => HasMatchingBaseConstructor(constructorSymbol),
                   IMethodSymbol methodSymbol => methodSymbol.OverriddenMethod != null || methodSymbol.ExplicitInterfaceImplementations.Any(),
                   IPropertySymbol propertySymbol => propertySymbol.OverriddenProperty != null || propertySymbol.ExplicitInterfaceImplementations.Any(),
                   IEventSymbol eventSymbol => eventSymbol.OverriddenEvent != null || eventSymbol.ExplicitInterfaceImplementations.Any(),
                   _ => false
               };
    }

    /// <summary>
    /// Gets the first line-oriented span which fully contains the XML node.
    /// </summary>
    /// <param name="text">Source text</param>
    /// <param name="node">XML node</param>
    /// <returns>The line span</returns>
    internal static TextSpan GetLineSpanContainingNode(SourceText text, XmlNodeSyntax node)
    {
        var startLine = text.Lines.GetLineFromPosition(node.FullSpan.Start);
        var endLine = text.Lines.GetLineFromPosition(node.FullSpan.End);

        return TextSpan.FromBounds(startLine.Start, endLine.EndIncludingLineBreak);
    }

    /// <summary>
    /// Gets the first line-oriented span which fully contains the source span.
    /// </summary>
    /// <param name="text">Source text</param>
    /// <param name="span">Source span</param>
    /// <returns>The line span</returns>
    internal static TextSpan GetLineSpanContainingSpan(SourceText text, TextSpan span)
    {
        var startLine = text.Lines.GetLineFromPosition(span.Start);
        var endLine = text.Lines.GetLineFromPosition(span.End);

        return TextSpan.FromBounds(startLine.Start, endLine.EndIncludingLineBreak);
    }

    /// <summary>
    /// Gets the tag name of an XML node.
    /// </summary>
    /// <param name="node">XML node</param>
    /// <returns>The tag name</returns>
    internal static string GetTagName(XmlNodeSyntax node)
    {
        return node switch
               {
                   XmlElementSyntax element => element.StartTag.Name.LocalName.ValueText,
                   XmlEmptyElementSyntax element => element.Name.LocalName.ValueText,
                   _ => string.Empty
               };
    }

    /// <summary>
    /// Determines whether the declaration already has the required documentation contract.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the declaration is documented sufficiently</returns>
    internal static bool HasRequiredDocumentation(MemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var documentationComment = GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return false;
        }

        var expandedDocumentation = GetExpandedDocumentation(declaration, semanticModel, cancellationToken);

        if (HasInheritdoc(documentationComment, expandedDocumentation))
        {
            return true;
        }

        if (HasTag(documentationComment, expandedDocumentation, "summary"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the enum member declaration already has the required documentation contract.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the declaration is documented sufficiently</returns>
    internal static bool HasRequiredDocumentation(EnumMemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var documentationComment = GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return false;
        }

        var expandedDocumentation = GetExpandedDocumentation(declaration, semanticModel, cancellationToken);

        return HasInheritdoc(documentationComment, expandedDocumentation)
               || HasTag(documentationComment, expandedDocumentation, "summary");
    }

    /// <summary>
    /// Determines whether the declaration belongs to the requested accessibility group.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="accessibilityGroup">Accessibility group</param>
    /// <returns><see langword="true"/> if the declaration matches the requested group</returns>
    internal static bool MatchesAccessibilityGroup(MemberDeclarationSyntax declaration, DocumentationAccessibilityGroup accessibilityGroup)
    {
        return accessibilityGroup switch
               {
                   DocumentationAccessibilityGroup.Any => true,
                   DocumentationAccessibilityGroup.NonPrivate => IsPurePrivateDeclaration(declaration) == false,
                   DocumentationAccessibilityGroup.Private => IsPurePrivateDeclaration(declaration),
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether the enum member belongs to the requested accessibility group.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="accessibilityGroup">Accessibility group</param>
    /// <returns><see langword="true"/> if the declaration matches the requested group</returns>
    internal static bool MatchesAccessibilityGroup(EnumMemberDeclarationSyntax declaration, DocumentationAccessibilityGroup accessibilityGroup)
    {
        return declaration.Parent is EnumDeclarationSyntax enumDeclaration
               && MatchesAccessibilityGroup(enumDeclaration, accessibilityGroup);
    }

    /// <summary>
    /// Determines whether the declaration is an explicit interface implementation.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration explicitly implements an interface member</returns>
    private static bool IsExplicitInterfaceImplementation(MemberDeclarationSyntax declaration)
    {
        return declaration switch
               {
                   MethodDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   PropertyDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   IndexerDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   EventDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether the documentation contains a direct tag.
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="tagName">Tag name</param>
    /// <returns><see langword="true"/> if the tag exists</returns>
    private static bool HasDirectTag(DocumentationCommentTriviaSyntax documentationComment, string tagName)
    {
        return GetDirectTags(documentationComment, tagName).Length > 0;
    }

    /// <summary>
    /// Parses the expanded documentation XML for the enum member declaration.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The expanded XML documentation root, when available</returns>
    private static XElement GetExpandedDocumentation(EnumMemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var declaredSymbol = GetDeclaredSymbol(declaration, semanticModel, cancellationToken);
        var rawDocumentation = declaredSymbol?.GetDocumentationCommentXml(expandIncludes: true, cancellationToken: cancellationToken);

        return string.IsNullOrWhiteSpace(rawDocumentation)
                   ? null
                   : XElement.Parse(rawDocumentation);
    }

    /// <summary>
    /// Gets the declared symbol for the member.
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
    /// Gets the declared symbol for the enum member.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The declared symbol, when available</returns>
    private static ISymbol GetDeclaredSymbol(EnumMemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return semanticModel.GetDeclaredSymbol(declaration, cancellationToken);
    }

    /// <summary>
    /// Determines whether the XML node contributes meaningful documentation content.
    /// </summary>
    /// <param name="node">XML node</param>
    /// <returns><see langword="true"/> if the node contains meaningful content</returns>
    private static bool HasMeaningfulContent(XmlNodeSyntax node)
    {
        return node switch
               {
                   XmlTextSyntax textSyntax => textSyntax.TextTokens.Any(obj => string.IsNullOrWhiteSpace(obj.ValueText) == false),
                   XmlElementSyntax or XmlEmptyElementSyntax or XmlCDataSectionSyntax => true,
                   _ => string.IsNullOrWhiteSpace(node.ToString()) == false
               };
    }

    /// <summary>
    /// Gets the <c>name</c> attribute value from an attribute list.
    /// </summary>
    /// <param name="attributes">Attributes</param>
    /// <returns>The attribute value, when present</returns>
    private static string GetNameAttributeValue(SyntaxList<XmlAttributeSyntax> attributes)
    {
        return attributes.OfType<XmlNameAttributeSyntax>()
                         .FirstOrDefault(obj => string.Equals(obj.Name.LocalName.ValueText, "name", StringComparison.OrdinalIgnoreCase))
                         ?.Identifier.Identifier.ValueText;
    }

    /// <summary>
    /// Determines whether a constructor matches a base constructor signature.
    /// </summary>
    /// <param name="constructorSymbol">Constructor symbol</param>
    /// <returns><see langword="true"/> if a matching base constructor exists</returns>
    private static bool HasMatchingBaseConstructor(IMethodSymbol constructorSymbol)
    {
        var baseType = constructorSymbol.ContainingType?.BaseType;

        if (baseType == null)
        {
            return false;
        }

        foreach (var baseConstructor in baseType.Constructors)
        {
            if (baseConstructor.Parameters.Length != constructorSymbol.Parameters.Length)
            {
                continue;
            }

            var allParametersMatch = true;

            for (var parameterIndex = 0; parameterIndex < constructorSymbol.Parameters.Length; parameterIndex++)
            {
                if (SymbolEqualityComparer.Default.Equals(constructorSymbol.Parameters[parameterIndex].Type, baseConstructor.Parameters[parameterIndex].Type) == false)
                {
                    allParametersMatch = false;

                    break;
                }
            }

            if (allParametersMatch)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the declaration uses the pure <c>private</c> accessibility.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration is purely private</returns>
    private static bool IsPurePrivateDeclaration(MemberDeclarationSyntax declaration)
    {
        var modifiers = GetModifiers(declaration);
        var hasPrivateModifier = modifiers.Any(SyntaxKind.PrivateKeyword);
        var hasNonPrivateModifier = modifiers.Any(SyntaxKind.PublicKeyword)
                                    || modifiers.Any(SyntaxKind.InternalKeyword)
                                    || modifiers.Any(SyntaxKind.ProtectedKeyword)
                                    || modifiers.Any(SyntaxKind.FileKeyword);

        return hasPrivateModifier
               && hasNonPrivateModifier == false;
    }

    /// <summary>
    /// Gets the declaration modifiers.
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns>Modifier tokens</returns>
    private static SyntaxTokenList GetModifiers(MemberDeclarationSyntax declaration)
    {
        return declaration switch
               {
                   TypeDeclarationSyntax typeDeclaration => typeDeclaration.Modifiers,
                   EnumDeclarationSyntax enumDeclaration => enumDeclaration.Modifiers,
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Modifiers,
                   BaseMethodDeclarationSyntax methodDeclaration => methodDeclaration.Modifiers,
                   BasePropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Modifiers,
                   BaseFieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Modifiers,
                   _ => default
               };
    }

    #endregion // Methods
}