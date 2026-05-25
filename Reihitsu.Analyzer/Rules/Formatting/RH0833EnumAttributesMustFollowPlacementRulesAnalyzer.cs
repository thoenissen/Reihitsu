using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0833: Enum attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0833EnumAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase<RH0833EnumAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0833";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0833EnumAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0833Title),
               nameof(AnalyzerResources.RH0833MessageFormat),
               AttributeTargets.Enum,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor
}