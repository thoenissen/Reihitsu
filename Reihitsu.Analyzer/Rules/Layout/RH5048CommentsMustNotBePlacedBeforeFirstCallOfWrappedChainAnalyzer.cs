using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5048: Comments must not be placed before the first call of a wrapped chain
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer : FluentChainAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5048";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5048Title), nameof(AnalyzerResources.RH5048MessageFormat))
    {
    }

    #endregion // Constructor

    #region FluentChainAnalyzerBase

    /// <inheritdoc/>
    protected override void AnalyzeChain(SyntaxNodeAnalysisContext context, SyntaxNode outermostNode)
    {
        if (FluentChainAnalysisHelper.TryGetWrappedFirstLink(outermostNode, out var firstLink, out var previousToken) == false)
        {
            return;
        }

        foreach (var comment in previousToken.TrailingTrivia.Concat(firstLink.LeadingTrivia).Where(CommentPositionUtilities.IsOrdinaryComment))
        {
            context.ReportDiagnostic(CreateDiagnostic(comment.GetLocation()));
        }
    }

    #endregion // FluentChainAnalyzerBase
}