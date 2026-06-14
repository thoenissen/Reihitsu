using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3001: The logical operator ! should not be used for clarity
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3001NotOperatorShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH3001NotOperatorShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3001";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3001NotOperatorShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3001Title), nameof(AnalyzerResources.RH3001MessageFormat))
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
        if (context.Node is not PrefixUnaryExpressionSyntax node)
        {
            return;
        }

        // Only plain bool operands can be rewritten to "operand == false" without changing semantics.
        // For bool? the rewrite drops null propagation, and for operands with a user-defined operator !
        // the rewrite is not equivalent (and "== false" may not even compile).
        if (context.SemanticModel.GetTypeInfo(node.Operand, context.CancellationToken).Type is not { SpecialType: SpecialType.System_Boolean })
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(node.OperatorToken.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnLogicalNotExpressionSyntaxNode, SyntaxKind.LogicalNotExpression);
    }

    #endregion // DiagnosticAnalyzer
}