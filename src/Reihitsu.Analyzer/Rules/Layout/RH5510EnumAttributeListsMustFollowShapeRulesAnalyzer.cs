using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5510: Enum attribute lists must use one attribute per list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5510EnumAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5510";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5510EnumAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5510Title),
               nameof(AnalyzerResources.RH5510MessageFormat),
               AttributeTargets.Enum,
               TargetAttributeListShapeMode.SplitLists)
    {
    }

    #endregion // Constructor
}