using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0839: Property attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0839PropertyAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase<RH0839PropertyAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0839";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0839PropertyAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0839Title),
               nameof(AnalyzerResources.RH0839MessageFormat),
               AttributeTargets.Property,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor
}