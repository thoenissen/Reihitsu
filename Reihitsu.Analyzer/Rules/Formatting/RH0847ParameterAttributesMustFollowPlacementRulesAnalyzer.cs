using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0847: Parameter attributes must stay inline
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0847ParameterAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase<RH0847ParameterAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0847";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0847ParameterAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0847Title),
               nameof(AnalyzerResources.RH0847MessageFormat),
               AttributeTargets.Parameter,
               TargetAttributePlacementMode.SingleLine)
    {
    }

    #endregion // Constructor
}