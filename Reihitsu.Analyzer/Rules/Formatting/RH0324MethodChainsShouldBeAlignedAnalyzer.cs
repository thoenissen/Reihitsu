using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0324: Method chains should be aligned
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0324MethodChainsShouldBeAlignedAnalyzer : FluentChainAnalyzerBase<RH0324MethodChainsShouldBeAlignedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0324";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0324MethodChainsShouldBeAlignedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0324Title), nameof(AnalyzerResources.RH0324MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the line number of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Line number</returns>
    private static int GetLine(SyntaxToken token)
    {
        return FluentChainAnalysisHelper.GetLine(token);
    }

    /// <summary>
    /// Gets the column of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Column</returns>
    private static int GetColumn(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Character;
    }

    #endregion // Methods

    #region FluentChainAnalyzerBase

    /// <inheritdoc/>
    protected override void AnalyzeChain(SyntaxNodeAnalysisContext context, SyntaxNode outermostNode)
    {
        var chainLinks = FluentChainAnalysisHelper.CollectChainLinks(outermostNode);

        if (chainLinks.Count < 2)
        {
            return;
        }

        var firstLine = GetLine(chainLinks[0]);

        if (chainLinks.TrueForAll(link => GetLine(link) == firstLine))
        {
            return;
        }

        var referenceColumn = GetColumn(chainLinks[0]);

        for (var linkIndex = 1; linkIndex < chainLinks.Count; linkIndex++)
        {
            var linkLine = GetLine(chainLinks[linkIndex]);
            var linkColumn = GetColumn(chainLinks[linkIndex]);

            if (linkLine == firstLine)
            {
                if (chainLinks.Skip(linkIndex + 1).Any(subsequentLink => GetLine(subsequentLink) != firstLine))
                {
                    context.ReportDiagnostic(CreateDiagnostic(chainLinks[linkIndex].GetLocation()));
                }
            }
            else
            {
                if (linkColumn != referenceColumn)
                {
                    context.ReportDiagnostic(CreateDiagnostic(chainLinks[linkIndex].GetLocation()));
                }
            }
        }
    }

    #endregion // FluentChainAnalyzerBase
}