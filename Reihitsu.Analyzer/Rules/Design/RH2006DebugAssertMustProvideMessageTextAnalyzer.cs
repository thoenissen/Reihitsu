using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2006: Debug.Assert must provide message text
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2006DebugAssertMustProvideMessageTextAnalyzer : DiagnosticAnalyzerBase<RH2006DebugAssertMustProvideMessageTextAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2006";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2006DebugAssertMustProvideMessageTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2006Title), nameof(AnalyzerResources.RH2006MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.InvocationExpression"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocationExpression)
        {
            return;
        }

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        if (methodSymbol == null)
        {
            return;
        }

        if (methodSymbol.Name != "Assert")
        {
            return;
        }

        if (methodSymbol.ContainingType.ToDisplayString() != "System.Diagnostics.Debug")
        {
            return;
        }

        if (invocationExpression.ArgumentList.Arguments.Count >= 2)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(invocationExpression.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnInvocationExpression, SyntaxKind.InvocationExpression);
    }

    #endregion // DiagnosticAnalyzer
}