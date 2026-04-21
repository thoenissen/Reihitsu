using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0109: Debug.Fail must provide message text.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0109DebugFailMustProvideMessageTextAnalyzer : DiagnosticAnalyzerBase<RH0109DebugFailMustProvideMessageTextAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0109";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0109DebugFailMustProvideMessageTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0109Title), nameof(AnalyzerResources.RH0109MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the invocation targets <see cref="System.Diagnostics.Debug.Fail(string)"/>.
    /// </summary>
    /// <param name="methodSymbol">Method symbol</param>
    /// <returns><see langword="true"/> if the invocation targets <see cref="System.Diagnostics.Debug.Fail(string)"/></returns>
    private static bool IsDebugFail(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name == "Fail"
               && methodSymbol.ContainingType.ToDisplayString() == "System.Diagnostics.Debug";
    }

    /// <summary>
    /// Determine whether the message argument is <see langword="null"/>, empty, or whitespace.
    /// </summary>
    /// <param name="invocationExpression">Invocation expression</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the invocation should be reported</returns>
    private static bool ShouldReport(InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (invocationExpression.ArgumentList.Arguments.Count == 0)
        {
            return false;
        }

        var constantValue = semanticModel.GetConstantValue(invocationExpression.ArgumentList.Arguments[0].Expression, cancellationToken);

        if (constantValue.HasValue == false)
        {
            return false;
        }

        return constantValue.Value is null
               || (constantValue.Value is string text && string.IsNullOrWhiteSpace(text));
    }

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

        if (methodSymbol == null
            || IsDebugFail(methodSymbol) == false
            || ShouldReport(invocationExpression, context.SemanticModel, context.CancellationToken) == false)
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