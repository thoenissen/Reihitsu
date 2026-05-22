using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0854: Generic parameter attributes must use merged attribute lists
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0854GenericParameterAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH0854GenericParameterAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0854";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0854GenericParameterAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0854Title),
               nameof(AnalyzerResources.RH0854MessageFormat),
               AttributeTargets.GenericParameter,
               TargetAttributeListShapeMode.MergedList)
    {
    }

    #endregion // Constructor
}