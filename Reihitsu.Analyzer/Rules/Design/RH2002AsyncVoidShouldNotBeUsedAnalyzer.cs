using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2002: Async void methods should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2002AsyncVoidShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH2002AsyncVoidShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2002";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2002AsyncVoidShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2002Title), nameof(AnalyzerResources.RH2002MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SymbolKind.Method"/> symbols
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMethodSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol symbol)
        {
            return;
        }

        if (symbol.MethodKind != MethodKind.Ordinary)
        {
            return;
        }

        if (symbol.IsAsync
            && symbol.ReturnsVoid)
        {
            context.ReportDiagnostic(CreateDiagnostic(symbol.Locations));
        }
    }

    /// <summary>
    /// Analyzing all <see cref="LocalFunctionStatementSyntax"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLocalFunction(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalFunctionStatementSyntax localFunction
            || localFunction.Modifiers.Any(SyntaxKind.AsyncKeyword) == false)
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(localFunction, context.CancellationToken) is IMethodSymbol symbol
            && symbol.ReturnsVoid)
        {
            context.ReportDiagnostic(CreateDiagnostic(localFunction.Identifier.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzing all anonymous function (lambda and anonymous method) nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnAnonymousFunction(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AnonymousFunctionExpressionSyntax anonymousFunction
            || anonymousFunction.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword) == false)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(anonymousFunction, context.CancellationToken).Symbol is IMethodSymbol symbol
            && symbol.ReturnsVoid)
        {
            context.ReportDiagnostic(CreateDiagnostic(anonymousFunction.AsyncKeyword.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSymbolAction(OnMethodSymbol, SymbolKind.Method);
        context.RegisterSyntaxNodeAction(OnLocalFunction, SyntaxKind.LocalFunctionStatement);
        context.RegisterSyntaxNodeAction(OnAnonymousFunction, SyntaxKind.SimpleLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.AnonymousMethodExpression);
    }

    #endregion // DiagnosticAnalyzer
}