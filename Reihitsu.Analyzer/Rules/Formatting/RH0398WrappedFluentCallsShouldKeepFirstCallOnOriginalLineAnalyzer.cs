using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0398: Wrapped fluent calls should keep the first call on the original line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer : DiagnosticAnalyzerBase<RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0398";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0398Title), nameof(AnalyzerResources.RH0398MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing member access expressions for correct first-call placement in wrapped fluent calls
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
    /// Analyzing conditional access expressions for correct first-call placement in wrapped fluent calls
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