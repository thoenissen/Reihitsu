using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0113: Nested classes should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0113NestedClassesShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0113NestedClassesShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0113";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0113NestedClassesShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0113Title), nameof(AnalyzerResources.RH0113MessageFormat))
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

        if (NestedTypeAnalyzerHelper.IsNestedType(classDeclaration) is false || NestedTypeAnalyzerHelper.IsStaticType(classDeclaration))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(NestedTypeAnalyzerHelper.GetIdentifierLocation(classDeclaration)));
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