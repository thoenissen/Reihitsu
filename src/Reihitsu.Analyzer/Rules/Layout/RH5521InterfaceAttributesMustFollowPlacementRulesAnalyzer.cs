using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5521: Interface attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5521";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5521Title),
               nameof(AnalyzerResources.RH5521MessageFormat),
               AttributeTargets.Interface,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor
}