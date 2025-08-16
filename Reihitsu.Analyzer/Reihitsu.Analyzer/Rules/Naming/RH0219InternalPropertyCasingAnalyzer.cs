using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0219: Internal property names should be in PascalCase
/// </summary>
public class RH0219InternalPropertyCasingAnalyzer : CasingAnalyzerBase<RH0219InternalPropertyCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0219";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0219InternalPropertyCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0219Title), nameof(AnalyzerResources.RH0219MessageFormat), SyntaxKind.PropertyDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is PropertyDeclarationSyntax declaration)
        {
            if (declaration.Modifiers.Any(SyntaxKind.InternalKeyword))
            {
                yield return (declaration.Identifier.ValueText, declaration.Identifier.GetLocation());
            }
        }
    }

    #endregion // CasingAnalyzerBase
}