using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5032: Region directives should be followed by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer : RegionDirectiveBlankLineAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5032";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5032Title), nameof(AnalyzerResources.RH5032MessageFormat), requirePrecedingBlankLine: false)
    {
    }

    #endregion // Constructor
}