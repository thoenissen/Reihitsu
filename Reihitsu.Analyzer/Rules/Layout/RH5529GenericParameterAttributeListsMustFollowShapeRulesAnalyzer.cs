using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5529: Generic parameter attributes must use merged attribute lists
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5529GenericParameterAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH5529GenericParameterAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5529";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5529GenericParameterAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5529Title),
               nameof(AnalyzerResources.RH5529MessageFormat),
               AttributeTargets.GenericParameter,
               TargetAttributeListShapeMode.MergedList)
    {
    }

    #endregion // Constructor
}