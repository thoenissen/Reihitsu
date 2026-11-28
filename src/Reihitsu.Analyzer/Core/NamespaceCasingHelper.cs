using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared helpers for namespace-casing analyzers
/// </summary>
internal static class NamespaceCasingHelper
{
    #region Methods

    /// <summary>
    /// Gets all identifier locations that participate in a namespace name
    /// </summary>
    /// <param name="nameSyntax">Namespace name syntax</param>
    /// <returns>Locations including identifier names</returns>
    internal static IEnumerable<(string Name, Location Location)> GetLocations(NameSyntax nameSyntax)
    {
        switch (nameSyntax)
        {
            case QualifiedNameSyntax qualifiedNameSyntax:
                {
                    foreach (var location in GetLocations(qualifiedNameSyntax.Left))
                    {
                        yield return location;
                    }

                    foreach (var location in GetLocations(qualifiedNameSyntax.Right))
                    {
                        yield return location;
                    }
                }
                break;

            case IdentifierNameSyntax identifierNameSyntax:
                {
                    yield return (identifierNameSyntax.Identifier.ValueText, identifierNameSyntax.GetLocation());
                }
                break;
        }
    }

    /// <summary>
    /// Gets the values of the identifiers of the namespace a node is declared in: the names of every namespace declaration
    /// that is or encloses the node, outermost first, each identifier as returned by <see cref="GetLocations"/>
    /// </summary>
    /// <param name="node">Namespace declaration, or a node inside one</param>
    /// <returns>The identifier values of the full namespace name, outermost first; empty when no namespace declaration encloses the node</returns>
    internal static IEnumerable<string> GetFullNameParts(SyntaxNode node)
    {
        return node.AncestorsAndSelf()
                   .OfType<BaseNamespaceDeclarationSyntax>()
                   .Reverse()
                   .SelectMany(static namespaceDeclaration => GetLocations(namespaceDeclaration.Name))
                   .Select(static part => part.Name);
    }

    #endregion // Methods
}