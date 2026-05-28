using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5112: Wrapped fluent calls should keep the first call on the original line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer : FluentChainAnalyzerBase<RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer>
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
    protected override void AnalyzeChain(SyntaxNodeAnalysisContext context, SyntaxNode outermostNode)
    {
        var chainLinks = FluentChainAnalysisHelper.CollectChainLinks(outermostNode);

        if (chainLinks.Count == 0)
        {
            return;
        }

        var firstLink = chainLinks[0];
        var previousToken = firstLink.GetPreviousToken();

        if (previousToken == default
            || previousToken.IsKind(SyntaxKind.None))
        {
            return;
        }

        if (FluentChainAnalysisHelper.GetLine(firstLink) == FluentChainAnalysisHelper.GetLine(previousToken))
        {
            return;
        }

        if (FluentChainAnalysisHelper.HasIntermediateMemberAccess(firstLink))
        {
            return;
        }

        if (FluentChainAnalysisHelper.HasCommentDirectlyAbove(firstLink))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(firstLink.GetLocation()));
    }

    #endregion // FluentChainAnalyzerBase
}