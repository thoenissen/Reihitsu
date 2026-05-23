using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0848: Parameter attributes must use merged attribute lists
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0848ParameterAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH0848ParameterAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0848";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0848ParameterAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0848Title),
               nameof(AnalyzerResources.RH0848MessageFormat),
               AttributeTargets.Parameter,
               TargetAttributeListShapeMode.MergedList)
    {
    }

    #endregion // Constructor
}