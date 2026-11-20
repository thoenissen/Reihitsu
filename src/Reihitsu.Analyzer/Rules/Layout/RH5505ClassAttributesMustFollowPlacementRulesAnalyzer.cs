using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5505: Class attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5505ClassAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5505";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5505ClassAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               AttributeTargets.Class,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor
}