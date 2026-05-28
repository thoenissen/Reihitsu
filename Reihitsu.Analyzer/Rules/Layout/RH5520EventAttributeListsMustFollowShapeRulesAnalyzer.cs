using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5520: Event attribute lists must use one attribute per list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5520EventAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH5520EventAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5520";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5520EventAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5520Title),
               nameof(AnalyzerResources.RH5520MessageFormat),
               AttributeTargets.Event,
               TargetAttributeListShapeMode.SplitLists)
    {
    }

    #endregion // Constructor
}