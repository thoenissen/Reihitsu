using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0324: Method chains should be aligned
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0324MethodChainsShouldBeAlignedAnalyzer : DiagnosticAnalyzerBase<RH0324MethodChainsShouldBeAlignedAnalyzer>
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
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0324Title), nameof(AnalyzerResources.RH0324MessageFormat))
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

    /// <summary>
    /// Analyzing member access expressions for correct method chain alignment
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMemberAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (FluentChainAnalysisHelper.IsInnerChainMember(memberAccess))
        {
            return;
        }

        AnalyzeChain(context, memberAccess);
    }

    /// <summary>
    /// Analyzing conditional access expressions for correct method chain alignment
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConditionalAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConditionalAccessExpressionSyntax conditionalAccess)
        {
            return;
        }

        if (FluentChainAnalysisHelper.IsInnerChainMember(conditionalAccess))
        {
            return;
        }

        AnalyzeChain(context, conditionalAccess);
    }

    /// <summary>
    /// Analyzes a chain starting from the outermost node
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="outermostNode">The outermost node of the chain</param>
    private void AnalyzeChain(SyntaxNodeAnalysisContext context, SyntaxNode outermostNode)
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

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(OnConditionalAccessExpression, SyntaxKind.ConditionalAccessExpression);
    }

    #endregion // DiagnosticAnalyzer
}