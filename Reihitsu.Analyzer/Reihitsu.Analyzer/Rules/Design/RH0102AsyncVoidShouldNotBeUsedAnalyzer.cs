using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0102: Async void methods should not be used.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0102AsyncVoidShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0102AsyncVoidShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0102";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0102AsyncVoidShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0102Title), nameof(AnalyzerResources.RH0102MessageFormat))
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

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSymbolAction(OnMethodSymbol, SymbolKind.Method);
    }

    #endregion // DiagnosticAnalyzer
}