using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5503: Module attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5503ModuleAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase<RH5503ModuleAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5503";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5503ModuleAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5503Title),
               nameof(AnalyzerResources.RH5503MessageFormat),
               AttributeTargets.Module,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor
}