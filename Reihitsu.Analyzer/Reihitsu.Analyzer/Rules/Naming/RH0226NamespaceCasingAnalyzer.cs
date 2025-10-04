using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0226: Namespace declarations should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0226NamespaceCasingAnalyzer : CasingAnalyzerBase<RH0226NamespaceCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0226";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0226NamespaceCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0226Title), nameof(AnalyzerResources.RH0226MessageFormat), SyntaxKind.NamespaceDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    /// <summary>
    /// Get all locations and names of the name syntax node
    /// </summary>
    /// <param name="nameSyntax">Name node</param>
    /// <returns>Locations including names</returns>
    private IEnumerable<(string Name, Location Location)> GetLocations(NameSyntax nameSyntax)
    {
        if (nameSyntax is QualifiedNameSyntax qualifiedNameSyntax)
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
        else if (nameSyntax is IdentifierNameSyntax identifierNameSyntax)
        {
            yield return (identifierNameSyntax.Identifier.ValueText, identifierNameSyntax.GetLocation());
        }
    }

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            foreach (var location in GetLocations(namespaceDeclarationSyntax.Name))
            {
                yield return location;
            }
        }
    }

    #endregion // CasingAnalyzerBase
}