using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2103: Structs should not use parameterized primary constructors
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2103StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2103";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2103StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2103Title), nameof(AnalyzerResources.RH2103MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.StructDeclaration"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnStructDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not StructDeclarationSyntax structDeclaration)
        {
            return;
        }

        if (structDeclaration.ParameterList is not { Parameters.Count: > 0 })
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(structDeclaration.Identifier.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnStructDeclaration, SyntaxKind.StructDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}