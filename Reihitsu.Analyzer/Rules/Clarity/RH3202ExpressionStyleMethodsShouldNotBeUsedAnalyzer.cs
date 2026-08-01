using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

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

        if (methodDeclaration.ExpressionBody is null)
        {
            return;
        }

        // The formatter refuses to rebuild an expression body whose span carries a directive the
        // rewrite would relocate, so reporting here would offer a code fix that cannot converge.
        if (ExpressionBodyRewriteUtilities.BlocksRewrite(methodDeclaration, methodDeclaration.ExpressionBody, methodDeclaration.SemicolonToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(methodDeclaration.GetLocation()));
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