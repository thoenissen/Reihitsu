using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3208: Expression style local functions should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3208";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3208Title), nameof(AnalyzerResources.RH3208MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.LocalFunctionStatement"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLocalFunctionStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalFunctionStatementSyntax localFunctionStatement)
        {
            return;
        }

        if (localFunctionStatement.ExpressionBody is null)
        {
            return;
        }

        // The formatter refuses to rebuild an expression body whose span carries a directive the
        // rewrite would relocate, so reporting here would offer a code fix that cannot converge.
        if (ExpressionBodyRewriteUtilities.BlocksRewrite(localFunctionStatement, localFunctionStatement.ExpressionBody, localFunctionStatement.SemicolonToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(localFunctionStatement.ExpressionBody.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnLocalFunctionStatement, SyntaxKind.LocalFunctionStatement);
    }

    #endregion // DiagnosticAnalyzer
}