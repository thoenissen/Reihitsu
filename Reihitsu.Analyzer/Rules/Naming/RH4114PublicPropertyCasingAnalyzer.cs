using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH4114: Public property names should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4114PublicPropertyCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4114";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4114PublicPropertyCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4114Title), nameof(AnalyzerResources.RH4114MessageFormat), SyntaxKind.PropertyDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is PropertyDeclarationSyntax declaration
            && declaration.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            yield return (declaration.Identifier.ValueText, declaration.Identifier.GetLocation());
        }
    }

    #endregion // CasingAnalyzerBase
}