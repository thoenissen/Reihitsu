using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0832: Struct attribute lists must use one attribute per list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0832StructAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH0832StructAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0832";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0832StructAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0832Title),
               nameof(AnalyzerResources.RH0832MessageFormat),
               AttributeTargets.Struct,
               TargetAttributeListShapeMode.SplitLists)
    {
    }

    #endregion // Constructor
}