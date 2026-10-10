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

    #endregion // Methods
}