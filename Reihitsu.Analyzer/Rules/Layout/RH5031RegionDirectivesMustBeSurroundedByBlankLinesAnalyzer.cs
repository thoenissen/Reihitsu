using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5031: Region directives must be surrounded by blank lines
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer : RegionDirectiveBlankLineAnalyzerBase<RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer>
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
    public RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5031Title), nameof(AnalyzerResources.RH5031MessageFormat), SyntaxKind.RegionDirectiveTrivia)
    {
    }

    #endregion // Constructor
}