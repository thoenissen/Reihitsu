using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5116: Null-forgiving operator must stay on the line of its expression
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5116";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a null-forgiving operator. Its <c>!</c> is reported when the attached-part predicate states it as the first
    /// token of an attached part — not the first token of a link operator <c>!.</c> or <c>!?.</c>
    /// (<see cref="AttachedPartGapUtilities.StartsAttachedPart"/>) — and it starts on a later line than the token in front of
    /// it ends on, whatever comment, directive, or disabled text stands in the gap (RH5116-001, RH5116-002)
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="errorVerdicts">Syntax error verdict per tree</param>
    private void OnSuppressNullableWarning(SyntaxNodeAnalysisContext context, SyntaxTreeErrorVerdictCache errorVerdicts)
    {
        if (context.Node is not PostfixUnaryExpressionSyntax postfixUnary)
        {
            return;
        }

        var operatorToken = postfixUnary.OperatorToken;

        if (operatorToken.IsMissing
            || AttachedPartGapUtilities.StartsAttachedPart(operatorToken) == false)
        {
            return;
        }

        var previousToken = SyntaxTokenNavigationUtilities.GetPreviousToken(operatorToken);

        if (operatorToken.GetLocation().GetLineSpan().StartLinePosition.Line <= previousToken.GetLocation().GetLineSpan().EndLinePosition.Line)
        {
            return;
        }

        // The rule withholds itself for a whole file with malformed syntax (SHARED-JOIN-002), so the verdict covers the
        // entire tree rather than the analyzed operator.
        if (errorVerdicts.ContainsError(context.Node.SyntaxTree, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(operatorToken.GetLocation()));
    }

    /// <summary>
    /// Starts the analysis of a compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var errorVerdicts = new SyntaxTreeErrorVerdictCache();

        context.RegisterSyntaxNodeAction(nodeContext => OnSuppressNullableWarning(nodeContext, errorVerdicts), SyntaxKind.SuppressNullableWarningExpression);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    #endregion // DiagnosticAnalyzer
}