using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0115: Structs should not use parameterized primary constructors
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0115StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer : DiagnosticAnalyzerBase<RH0115StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0115";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0115StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0115Title), nameof(AnalyzerResources.RH0115MessageFormat))
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