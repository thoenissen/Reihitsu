using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5112: Wrapped chains should keep the first member access on the original line
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
        : base(DiagnosticId)
    {
    }

    #endregion // Constructor

    #region FluentChainAnalyzerBase

    /// <inheritdoc/>
    protected override void AnalyzeChain(SyntaxNodeAnalysisContext context, FluentChain chain)
    {
        if (chain.FirstLink.StartsLine == false
            || chain.IsFirstLinkBlocked)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(chain.FirstLink.OperatorToken.GetLocation()));
    }

    #endregion // FluentChainAnalyzerBase
}