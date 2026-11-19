using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5508: Struct attribute lists must use one attribute per list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5508StructAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5508";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5508StructAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               AttributeTargets.Struct,
               TargetAttributeListShapeMode.SplitLists)
    {
    }

    #endregion // Constructor
}