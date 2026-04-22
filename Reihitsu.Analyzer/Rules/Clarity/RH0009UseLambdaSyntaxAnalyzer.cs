using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH0009: Use lambda syntax.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0009UseLambdaSyntaxAnalyzer : DiagnosticAnalyzerBase<RH0009UseLambdaSyntaxAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0009";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0009UseLambdaSyntaxAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH0009Title), nameof(AnalyzerResources.RH0009MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze anonymous method expressions.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnAnonymousMethodExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is AnonymousMethodExpressionSyntax { ParameterList: not null } anonymousMethodExpression)
        {
            context.ReportDiagnostic(CreateDiagnostic(anonymousMethodExpression.DelegateKeyword.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnAnonymousMethodExpression, SyntaxKind.AnonymousMethodExpression);
    }

    #endregion // DiagnosticAnalyzer
}