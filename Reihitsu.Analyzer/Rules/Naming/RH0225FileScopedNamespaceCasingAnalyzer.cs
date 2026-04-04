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
/// RH0225: File scoped namespace declarations should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0225FileScopedNamespaceCasingAnalyzer : CasingAnalyzerBase<RH0225FileScopedNamespaceCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0225";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0225FileScopedNamespaceCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0225Title), nameof(AnalyzerResources.RH0225MessageFormat), SyntaxKind.FileScopedNamespaceDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    /// <summary>
    /// Get all locations and names of the name syntax node
    /// </summary>
    /// <param name="nameSyntax">Name node</param>
    /// <returns>Locations including names</returns>
    private static IEnumerable<(string Name, Location Location)> GetLocations(NameSyntax nameSyntax)
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

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is FileScopedNamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            foreach (var location in GetLocations(namespaceDeclarationSyntax.Name))
            {
                yield return location;
            }
        }
    }

    #endregion // CasingAnalyzerBase
}