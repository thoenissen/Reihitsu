using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0852: Return value attribute lists must use one attribute per list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0852ReturnValueAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH0852ReturnValueAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0852";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0852ReturnValueAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0852Title),
               nameof(AnalyzerResources.RH0852MessageFormat),
               AttributeTargets.ReturnValue,
               TargetAttributeListShapeMode.SplitLists)
    {
    }

    #endregion // Constructor
}