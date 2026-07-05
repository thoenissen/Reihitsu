using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5515: Property attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5515PropertyAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5515";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5515PropertyAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5515Title),
               nameof(AnalyzerResources.RH5515MessageFormat),
               AttributeTargets.Property,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor
}