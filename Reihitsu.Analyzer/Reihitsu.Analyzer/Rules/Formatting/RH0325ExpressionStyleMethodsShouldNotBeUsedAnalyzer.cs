using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0325: Expression style methods should not be used.
/// </summary>
public class RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0325";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0325Title), nameof(AnalyzerResources.RH0325MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.ConstructorDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConstructorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax constructorDeclaration)
        {
            return;
        }

        if (constructorDeclaration.ExpressionBody is not null)
        {
            context.ReportDiagnostic(CreateDiagnostic(constructorDeclaration.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnConstructorDeclaration, SyntaxKind.MethodDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}