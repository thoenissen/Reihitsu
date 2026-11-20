using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.Test.Verifiers;

/// <summary>
/// Probe analyzer for the harness tests of the code-fix verifier: it reports every parenthesized expression under the
/// ID of <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/>, whether or not the parentheses are
/// needed, so that <see cref="ParenthesisRemovalProbeCodeFixProvider"/> can write a fixed state that holds compiler
/// diagnostics the source state does not hold
/// </summary>
public sealed class ParenthesisRemovalProbeAnalyzer : DiagnosticAnalyzerBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public ParenthesisRemovalProbeAnalyzer()
        : base(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, DiagnosticCategory.Clarity)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Reports the parenthesized expression
    /// </summary>
    /// <param name="context">Context</param>
    private void OnParenthesizedExpression(SyntaxNodeAnalysisContext context)
    {
        context.ReportDiagnostic(CreateDiagnostic(context.Node.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnParenthesizedExpression, SyntaxKind.ParenthesizedExpression);
    }

    #endregion // DiagnosticAnalyzer
}