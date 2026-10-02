using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3207: Expression style finalizers should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3207";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3207Title), nameof(AnalyzerResources.RH3207MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.DestructorDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDestructorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DestructorDeclarationSyntax destructorDeclaration)
        {
            return;
        }

        if (destructorDeclaration.ExpressionBody is null)
        {
            return;
        }

        // The formatter refuses to rebuild an expression body whose span carries a directive the
        // rewrite would relocate, so reporting here would offer a code fix that cannot converge.
        if (ExpressionBodyRewriteUtilities.BlocksRewrite(destructorDeclaration, destructorDeclaration.ExpressionBody, destructorDeclaration.SemicolonToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(destructorDeclaration.ExpressionBody.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDestructorDeclaration, SyntaxKind.DestructorDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}