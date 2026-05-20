using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0114: Classes should not use parameterized primary constructors
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0114ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer : DiagnosticAnalyzerBase<RH0114ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0114";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0114ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0114Title), nameof(AnalyzerResources.RH0114MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.ClassDeclaration"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
        {
            return;
        }

        if (classDeclaration.ParameterList is not { Parameters.Count: > 0 })
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(classDeclaration.Identifier.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}