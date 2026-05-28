using Microsoft.CodeAnalysis;
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