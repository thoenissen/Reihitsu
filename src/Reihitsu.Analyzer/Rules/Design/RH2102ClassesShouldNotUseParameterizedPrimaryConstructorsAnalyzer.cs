using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2102: Classes should not use parameterized primary constructors
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2102";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2102Title), nameof(AnalyzerResources.RH2102MessageFormat))
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