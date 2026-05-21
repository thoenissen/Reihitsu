using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0120: Nested delegates should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0120NestedDelegatesShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0120NestedDelegatesShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0120";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0120NestedDelegatesShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0120Title), nameof(AnalyzerResources.RH0120MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.DelegateDeclaration"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDelegateDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DelegateDeclarationSyntax delegateDeclaration || NestedTypeAnalyzerHelper.IsNestedType(delegateDeclaration) is false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(NestedTypeAnalyzerHelper.GetIdentifierLocation(delegateDeclaration)));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDelegateDeclaration, SyntaxKind.DelegateDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}