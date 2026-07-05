using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3006: Conditional expressions must declare precedence
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3006";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3006Title), nameof(AnalyzerResources.RH3006MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze logical or expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLogicalOrExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryExpressionSyntax logicalOrExpression)
        {
            return;
        }

        if (logicalOrExpression.Left is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalAndExpression } leftLogicalAndExpression)
        {
            context.ReportDiagnostic(CreateDiagnostic(leftLogicalAndExpression.GetLocation()));
        }

        if (logicalOrExpression.Right is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalAndExpression } rightLogicalAndExpression)
        {
            context.ReportDiagnostic(CreateDiagnostic(rightLogicalAndExpression.GetLocation()));
        }
    }

    /// <summary>
    /// Analyze or patterns
    /// </summary>
    /// <param name="context">Context</param>
    private void OnOrPattern(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryPatternSyntax orPattern)
        {
            return;
        }

        if (orPattern.Left is BinaryPatternSyntax { RawKind: (int)SyntaxKind.AndPattern } leftAndPattern)
        {
            context.ReportDiagnostic(CreateDiagnostic(leftAndPattern.GetLocation()));
        }

        if (orPattern.Right is BinaryPatternSyntax { RawKind: (int)SyntaxKind.AndPattern } rightAndPattern)
        {
            context.ReportDiagnostic(CreateDiagnostic(rightAndPattern.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnLogicalOrExpression, SyntaxKind.LogicalOrExpression);
        context.RegisterSyntaxNodeAction(OnOrPattern, SyntaxKind.OrPattern);
    }

    #endregion // DiagnosticAnalyzer
}