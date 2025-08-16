using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0207: Event names should be in PascalCase
/// </summary>
public class RH0207EventNameCasingAnalyzer : CasingAnalyzerBase<RH0207EventNameCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0207";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0207EventNameCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0207Title), nameof(AnalyzerResources.RH0207MessageFormat), SyntaxKind.EventFieldDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is EventFieldDeclarationSyntax declaration)
        {
            foreach (var variable in declaration.Declaration.Variables)
            {
                yield return (variable.Identifier.ValueText, variable.Identifier.GetLocation());
            }
        }
    }

    #endregion // CasingAnalyzerBase
}