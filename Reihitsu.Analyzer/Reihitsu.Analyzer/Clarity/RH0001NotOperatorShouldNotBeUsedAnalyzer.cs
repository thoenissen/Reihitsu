using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Clarity;

/// <summary>
/// RH0001: The logical operator ! should not be used for clarity.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0001NotOperatorShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0001NotOperatorShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0001";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0001NotOperatorShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH0001Title), nameof(AnalyzerResources.RH0001MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.LogicalNotExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLogicalNotExpressionSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is PrefixUnaryExpressionSyntax node)
        {
            context.ReportDiagnostic(CreateDiagnostic(node.OperatorToken.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <summary>
    /// Called once at session start to register actions in the analysis context.
    /// </summary>
    /// <param name="context">Context</param>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnLogicalNotExpressionSyntaxNode, SyntaxKind.LogicalNotExpression);
    }

    #endregion // DiagnosticAnalyzer
}