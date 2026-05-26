using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

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