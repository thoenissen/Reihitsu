using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5522: Interface attribute lists must use one attribute per list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5522";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               AttributeTargets.Interface,
               TargetAttributeListShapeMode.SplitLists)
    {
    }

    #endregion // Constructor
}