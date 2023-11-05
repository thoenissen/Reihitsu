using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Analyzer.Extensions;

namespace Reihitsu.Analyzer.Design;

/// <summary>
/// RH0101 Private auto-implemented properties should not be used.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0101";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0101Title), nameof(AnalyzerResources.RH0101MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.LogicalNotExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLogicalNotExpressionSyntaxNode(SymbolAnalysisContext context)
    {
        if (context.Symbol is IPropertySymbol { DeclaredAccessibility: Accessibility.Private } symbol
         && symbol.IsAutoProperty())
        {
            context.ReportDiagnostic(CreateDiagnostic(symbol.Locations));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <summary>
    /// Called once at session start to register actions in the analysis context.
    /// </summary>
    /// <param name="context">Context</param>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSymbolAction(OnLogicalNotExpressionSyntaxNode, SymbolKind.Property);
    }

    #endregion // DiagnosticAnalyzer
}