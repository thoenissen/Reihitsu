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
    /// Gets the column every link that starts a line is aligned to: the column of the chain's
    /// <see cref="FluentChain.GetAnchorLink"/>, or the column the chain starts in when it aligns to its root
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <returns>The anchor column</returns>
    private static int GetAnchorColumn(FluentChain chain)
    {
        var anchorLink = chain.GetAnchorLink();

        return SyntaxTokenPositionUtilities.GetColumn(anchorLink == null
                                                          ? chain.Node.GetFirstToken()
                                                          : anchorLink.OperatorToken);
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