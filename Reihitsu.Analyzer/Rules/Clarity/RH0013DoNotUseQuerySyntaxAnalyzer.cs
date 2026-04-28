using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH0013: Do not use query syntax
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0013DoNotUseQuerySyntaxAnalyzer : DiagnosticAnalyzerBase<RH0013DoNotUseQuerySyntaxAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0013";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0013DoNotUseQuerySyntaxAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH0013Title), nameof(AnalyzerResources.RH0013MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze query expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnQueryExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is QueryExpressionSyntax queryExpression)
        {
            context.ReportDiagnostic(CreateDiagnostic(queryExpression.FromClause.FromKeyword.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnQueryExpression, SyntaxKind.QueryExpression);
    }

    #endregion // DiagnosticAnalyzer
}