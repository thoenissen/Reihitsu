using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5201: Method chains should be aligned
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5201MethodChainsShouldBeAlignedAnalyzer : FluentChainAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5201";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5201MethodChainsShouldBeAlignedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5201Title), nameof(AnalyzerResources.RH5201MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the column every link that starts a line is aligned to. A first link kept on its own line by a comment, a
    /// preprocessor directive or disabled text aligns the chain to the column the chain starts in. Otherwise the anchor
    /// is the first invoked link in front of the first link that starts a line, or the chain's first link when no
    /// invoked link comes first
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <returns>The anchor column</returns>
    private static int GetAnchorColumn(FluentChain chain)
    {
        if (FluentChainAnalysisHelper.IsFirstLinkWrapped(chain)
            && FluentChainAnalysisHelper.IsFirstLinkBlocked(chain))
        {
            return SyntaxTokenPositionUtilities.GetColumn(chain.Node.GetFirstToken());
        }

        foreach (var link in chain.Links)
        {
            if (FluentChainAnalysisHelper.StartsLine(link))
            {
                break;
            }

            if (link.IsInvoked)
            {
                return SyntaxTokenPositionUtilities.GetColumn(link.OperatorToken);
            }
        }

        return SyntaxTokenPositionUtilities.GetColumn(chain.FirstLink.OperatorToken);
    }

    /// <summary>
    /// Determines whether a link is misplaced in a wrapped chain: a link of the chain part after its first link that
    /// does not start a line, a link that starts a line outside the anchor column, or a link whose operator holds a line
    /// break between its own tokens
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <param name="linkIndex">The index of the link</param>
    /// <param name="anchorColumn">The anchor column</param>
    /// <returns><see langword="true"/> if the link is misplaced</returns>
    private static bool IsMisplaced(FluentChain chain, int linkIndex, int anchorColumn)
    {
        var link = chain.Links[linkIndex];

        if (FluentChainAnalysisHelper.HasInnerLineBreak(link, true))
        {
            return true;
        }

        if (FluentChainAnalysisHelper.StartsLine(link))
        {
            return SyntaxTokenPositionUtilities.GetColumn(link.OperatorToken) != anchorColumn;
        }

        return chain.IsCallLess == false
               && linkIndex > chain.FirstInvokedLinkIndex;
    }

    #endregion // Methods

    #region FluentChainAnalyzerBase

    /// <inheritdoc/>
    protected override void AnalyzeChain(SyntaxNodeAnalysisContext context, FluentChain chain)
    {
        if (FluentChainAnalysisHelper.IsWrapped(chain) == false)
        {
            return;
        }

        var anchorColumn = GetAnchorColumn(chain);

        for (var linkIndex = 0; linkIndex < chain.Links.Count; linkIndex++)
        {
            if (IsMisplaced(chain, linkIndex, anchorColumn))
            {
                context.ReportDiagnostic(CreateDiagnostic(chain.Links[linkIndex].OperatorToken.GetLocation()));
            }
        }
    }

    #endregion // FluentChainAnalyzerBase
}