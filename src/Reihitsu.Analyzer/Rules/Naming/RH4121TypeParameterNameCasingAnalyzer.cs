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
/// RH4121: Type parameter names should start with an uppercase 'T'
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4121TypeParameterNameCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4121";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4121TypeParameterNameCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4121Title), nameof(AnalyzerResources.RH4121MessageFormat), SyntaxKind.TypeParameter, CasingUtilities.IsTypeParameterName)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is TypeParameterSyntax typeParameter)
        {
            yield return (typeParameter.Identifier.ValueText, typeParameter.Identifier.GetLocation());
        }
    }

    #endregion // CasingAnalyzerBase
}