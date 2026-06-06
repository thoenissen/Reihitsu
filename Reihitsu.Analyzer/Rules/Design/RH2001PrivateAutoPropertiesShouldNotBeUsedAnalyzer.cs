using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Analyzer.Extensions;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2001: Private auto-implemented properties should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2001";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2001Title), nameof(AnalyzerResources.RH2001MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.LogicalNotExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnPropertySymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is IPropertySymbol { DeclaredAccessibility: Accessibility.Private } symbol
            && symbol.IsAutoProperty()
            && symbol.GetAttributes().IsEmpty)
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

        context.RegisterSymbolAction(OnPropertySymbol, SymbolKind.Property);
    }

    #endregion // DiagnosticAnalyzer
}