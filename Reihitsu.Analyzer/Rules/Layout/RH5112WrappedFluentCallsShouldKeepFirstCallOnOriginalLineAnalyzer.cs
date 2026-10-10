using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5112: Wrapped fluent calls should keep the first call on the original line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer : FluentChainAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5112";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5112Title), nameof(AnalyzerResources.RH5112MessageFormat))
    {
    }

    #endregion // Constructor

    #region FluentChainAnalyzerBase

    /// <inheritdoc/>
    protected override void AnalyzeChain(SyntaxNodeAnalysisContext context, FluentChain chain)
    {
        if (FluentChainAnalysisHelper.IsFirstLinkWrapped(chain) == false
            || FluentChainAnalysisHelper.IsFirstLinkBlocked(chain))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(chain.FirstLink.OperatorToken.GetLocation()));
    }

    #endregion // FluentChainAnalyzerBase
}