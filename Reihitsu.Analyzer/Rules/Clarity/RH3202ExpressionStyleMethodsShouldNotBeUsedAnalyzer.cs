using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3202: Expression style methods should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3202";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3202Title), nameof(AnalyzerResources.RH3202MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.MethodDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        if (methodDeclaration.ExpressionBody is not null)
        {
            context.ReportDiagnostic(CreateDiagnostic(methodDeclaration.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}