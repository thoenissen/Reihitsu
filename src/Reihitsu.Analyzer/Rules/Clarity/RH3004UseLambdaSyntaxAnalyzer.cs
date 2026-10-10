using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3004: Use lambda syntax
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3004UseLambdaSyntaxAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3004";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3004UseLambdaSyntaxAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3004Title), nameof(AnalyzerResources.RH3004MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze anonymous method expressions
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