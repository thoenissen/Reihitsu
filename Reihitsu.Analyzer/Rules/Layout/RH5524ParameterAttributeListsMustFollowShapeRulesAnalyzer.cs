using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5524: Parameter attributes must use merged attribute lists
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5524";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5524Title),
               nameof(AnalyzerResources.RH5524MessageFormat),
               AttributeTargets.Parameter,
               TargetAttributeListShapeMode.MergedList)
    {
    }

    #endregion // Constructor
}