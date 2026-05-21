using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0117: Nested interfaces should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0117NestedInterfacesShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0117NestedInterfacesShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0117";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0117NestedInterfacesShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0117Title), nameof(AnalyzerResources.RH0117MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.InterfaceDeclaration"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InterfaceDeclarationSyntax interfaceDeclaration || NestedTypeAnalyzerHelper.IsNestedType(interfaceDeclaration) is false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(NestedTypeAnalyzerHelper.GetIdentifierLocation(interfaceDeclaration)));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}