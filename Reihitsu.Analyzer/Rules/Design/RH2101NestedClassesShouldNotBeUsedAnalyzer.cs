using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2101: Nested classes should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2101NestedClassesShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH2101NestedClassesShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2101";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2101NestedClassesShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2101Title), nameof(AnalyzerResources.RH2101MessageFormat))
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