using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5523: Parameter attributes must stay inline
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5523";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5523Title),
               nameof(AnalyzerResources.RH5523MessageFormat),
               AttributeTargets.Parameter,
               TargetAttributePlacementMode.SingleLine)
    {
    }

    #endregion // Constructor
}