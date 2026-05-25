using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0826: Assembly attribute lists must use one attribute per list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0826AssemblyAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH0826AssemblyAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0826";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0826AssemblyAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0826Title),
               nameof(AnalyzerResources.RH0826MessageFormat),
               AttributeTargets.Assembly,
               TargetAttributeListShapeMode.SplitLists)
    {
    }

    #endregion // Constructor
}