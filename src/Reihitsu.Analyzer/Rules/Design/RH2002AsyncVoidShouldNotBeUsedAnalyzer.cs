using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2002: Async void methods should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2002AsyncVoidShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
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

        if (symbol.IsAsync == false
            || symbol.ReturnsVoid == false)
        {
            return;
        }

        if (PartialMemberUtilities.TryGetParts(symbol, context.CancellationToken, out _, out _))
        {
            ReportPartialMethod(context, symbol);

            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(symbol.Locations));
    }

    /// <summary>
    /// Reports a partial method once, on the part SHARED-DIAG-012 selects: the part that writes <see langword="async"/>,
    /// which only the implementing part can do. Both parts are analyzed, so each reports only when it is the selected part
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="symbol">Symbol of the analyzed part</param>
    private void ReportPartialMethod(SymbolAnalysisContext context, IMethodSymbol symbol)
    {
        if (symbol.DeclaringSyntaxReferences.Length != 1
            || symbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) is not MethodDeclarationSyntax declaration)
        {
            return;
        }

        var reportedPart = PartialMemberUtilities.GetReportedPart(declaration,
                                                                  symbol,
                                                                  static part => part.Modifiers.Any(SyntaxKind.AsyncKeyword),
                                                                  context.Compilation,
                                                                  context.CancellationToken);

        if (reportedPart is MethodDeclarationSyntax reportedMethod)
        {
            context.ReportDiagnostic(CreateDiagnostic(reportedMethod.Identifier.GetLocation()));
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