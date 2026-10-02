using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3206: Expression style conversion operators should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3206";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3206Title), nameof(AnalyzerResources.RH3206MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.ConversionOperatorDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConversionOperatorDeclarationSyntax conversionOperatorDeclaration)
        {
            return;
        }

        if (conversionOperatorDeclaration.ExpressionBody is null)
        {
            return;
        }

        // The formatter refuses to rebuild an expression body whose span carries a directive the
        // rewrite would relocate, so reporting here would offer a code fix that cannot converge.
        if (ExpressionBodyRewriteUtilities.BlocksRewrite(conversionOperatorDeclaration, conversionOperatorDeclaration.ExpressionBody, conversionOperatorDeclaration.SemicolonToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(conversionOperatorDeclaration.ExpressionBody.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnConversionOperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}