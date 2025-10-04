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
/// RH0220: Public property names should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0220PublicPropertyCasingAnalyzer : CasingAnalyzerBase<RH0220PublicPropertyCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0220";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0220PublicPropertyCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0220Title), nameof(AnalyzerResources.RH0220MessageFormat), SyntaxKind.PropertyDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is PropertyDeclarationSyntax declaration)
        {
            if (declaration.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                yield return (declaration.Identifier.ValueText, declaration.Identifier.GetLocation());
            }
        }
    }

    #endregion // CasingAnalyzerBase
}