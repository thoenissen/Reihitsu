using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH4007: File scoped namespace declarations should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4007FileScopedNamespaceCasingAnalyzer : CasingAnalyzerBase<RH4007FileScopedNamespaceCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4007";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4007FileScopedNamespaceCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4007Title), nameof(AnalyzerResources.RH4007MessageFormat), SyntaxKind.FileScopedNamespaceDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override System.Collections.Generic.IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is FileScopedNamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            foreach (var location in NamespaceCasingHelper.GetLocations(namespaceDeclarationSyntax.Name))
            {
                yield return location;
            }
        }
    }

    #endregion // CasingAnalyzerBase
}