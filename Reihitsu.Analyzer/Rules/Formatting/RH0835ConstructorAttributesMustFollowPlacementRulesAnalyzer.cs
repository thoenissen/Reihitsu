using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0835: Constructor attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0835ConstructorAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase<RH0835ConstructorAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0835";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0835ConstructorAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0835Title),
               nameof(AnalyzerResources.RH0835MessageFormat),
               AttributeTargets.Constructor,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor
}