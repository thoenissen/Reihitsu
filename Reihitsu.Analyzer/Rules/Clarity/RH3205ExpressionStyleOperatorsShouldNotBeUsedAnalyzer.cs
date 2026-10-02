using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3205: Expression style operators should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3205";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3205Title), nameof(AnalyzerResources.RH3205MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.OperatorDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnOperatorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not OperatorDeclarationSyntax operatorDeclaration)
        {
            return;
        }

        if (operatorDeclaration.ExpressionBody is null)
        {
            return;
        }

        // The formatter refuses to rebuild an expression body whose span carries a directive the
        // rewrite would relocate, so reporting here would offer a code fix that cannot converge.
        if (ExpressionBodyRewriteUtilities.BlocksRewrite(operatorDeclaration, operatorDeclaration.ExpressionBody, operatorDeclaration.SemicolonToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(operatorDeclaration.ExpressionBody.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnOperatorDeclaration, SyntaxKind.OperatorDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}