using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5511: Constructor attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase<RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5511";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5511Title),
               nameof(AnalyzerResources.RH5511MessageFormat),
               AttributeTargets.Constructor,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor
}