using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3007: Do not use query syntax
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3007DoNotUseQuerySyntaxAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3007";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3007DoNotUseQuerySyntaxAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3007Title), nameof(AnalyzerResources.RH3007MessageFormat))
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