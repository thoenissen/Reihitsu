using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0211: Method parameter names should be in camelCase
/// </summary>
public class RH0211MethodParameterCasingAnalyzer : CasingAnalyzerBase<RH0211MethodParameterCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0211";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0211MethodParameterCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0211Title), nameof(AnalyzerResources.RH0211MessageFormat), SyntaxKind.Parameter, CasingUtilities.IsCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is ParameterSyntax parameter)
        {
            yield return (parameter.Identifier.ValueText, parameter.Identifier.GetLocation());
        }
    }

    #endregion // CasingAnalyzerBase
}