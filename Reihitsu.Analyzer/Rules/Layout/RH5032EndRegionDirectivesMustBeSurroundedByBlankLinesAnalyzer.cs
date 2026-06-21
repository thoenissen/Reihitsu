using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5032: End-region directives must be surrounded by blank lines
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer : RegionDirectiveBlankLineAnalyzerBase<RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer>
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
    public RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5032Title), nameof(AnalyzerResources.RH5032MessageFormat), SyntaxKind.EndRegionDirectiveTrivia)
    {
    }

    #endregion // Constructor
}