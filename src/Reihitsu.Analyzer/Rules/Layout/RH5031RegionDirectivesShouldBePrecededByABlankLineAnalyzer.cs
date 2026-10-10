using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5031: Region directives should be preceded by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer : RegionDirectiveBlankLineAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5031";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5031Title), nameof(AnalyzerResources.RH5031MessageFormat), requirePrecedingBlankLine: true)
    {
    }

    #endregion // Constructor
}