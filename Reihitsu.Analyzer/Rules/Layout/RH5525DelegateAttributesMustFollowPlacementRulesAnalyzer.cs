using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5525: Delegate attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5525DelegateAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase<RH5525DelegateAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5525";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5525DelegateAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5525Title),
               nameof(AnalyzerResources.RH5525MessageFormat),
               AttributeTargets.Delegate,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor
}