using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0326: Expression style constructors should not be used.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0326";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0326Title), nameof(AnalyzerResources.RH0326MessageFormat))
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
        if (context.Node is not ConstructorDeclarationSyntax constructorDeclaration)
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

        context.RegisterSyntaxNodeAction(OnConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}