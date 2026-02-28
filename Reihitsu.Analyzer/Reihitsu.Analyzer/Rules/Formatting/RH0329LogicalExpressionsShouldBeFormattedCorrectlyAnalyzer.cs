using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0329: Logical expressions should be formatted correctly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0329";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0329Title), nameof(AnalyzerResources.RH0329MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing logical binary expressions for correct formatting
    /// </summary>
    /// <param name="context">Context</param>
    private void OnBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        if (binaryExpression.Parent is BinaryExpressionSyntax parentBinary
            && (parentBinary.IsKind(SyntaxKind.LogicalAndExpression) || parentBinary.IsKind(SyntaxKind.LogicalOrExpression)))
        {
            return;
        }

        CheckBinaryExpressions(context, binaryExpression);
    }

    /// <summary>
    /// Recursively checks a binary expression and its nested logical binary children
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="binaryExpression">The binary expression to check</param>
    private void CheckBinaryExpressions(SyntaxNodeAnalysisContext context, BinaryExpressionSyntax binaryExpression)
    {
        CheckBinaryExpression(context, binaryExpression);

        if (binaryExpression.Left is BinaryExpressionSyntax leftBinary
            && (leftBinary.IsKind(SyntaxKind.LogicalAndExpression) || leftBinary.IsKind(SyntaxKind.LogicalOrExpression)))
        {
            CheckBinaryExpressions(context, leftBinary);
        }

        if (binaryExpression.Right is BinaryExpressionSyntax rightBinary
            && (rightBinary.IsKind(SyntaxKind.LogicalAndExpression) || rightBinary.IsKind(SyntaxKind.LogicalOrExpression)))
        {
            CheckBinaryExpressions(context, rightBinary);
        }
    }

    /// <summary>
    /// Checks a binary expression for correct formatting of the operator in relation to its operands
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="binaryExpression">The binary expression to check</param>
    private void CheckBinaryExpression(SyntaxNodeAnalysisContext context, BinaryExpressionSyntax binaryExpression)
    {
        if (binaryExpression.OperatorToken.SyntaxTree == null)
        {
            return;
        }

        var leftLineSpan = binaryExpression.Left.SyntaxTree.GetLineSpan(binaryExpression.Left.Span);
        var leftEndLine = leftLineSpan.EndLinePosition.Line;

        var operatorLineSpan = binaryExpression.OperatorToken.SyntaxTree.GetLineSpan(binaryExpression.OperatorToken.Span);
        var operatorLine = operatorLineSpan.StartLinePosition.Line;

        var rightLineSpan = binaryExpression.Right.SyntaxTree.GetLineSpan(binaryExpression.Right.Span);
        var rightStartLine = rightLineSpan.StartLinePosition.Line;

        if (operatorLine != leftEndLine || rightStartLine != operatorLine)
        {
            if (operatorLine > leftEndLine)
            {
                var leftStartCharacter = leftLineSpan.StartLinePosition.Character;
                var operatorCharacter = operatorLineSpan.StartLinePosition.Character;

                if (operatorCharacter != leftStartCharacter)
                {
                    context.ReportDiagnostic(CreateDiagnostic(binaryExpression.OperatorToken.GetLocation()));
                }
            }
            else
            {
                context.ReportDiagnostic(CreateDiagnostic(binaryExpression.OperatorToken.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnBinaryExpression, SyntaxKind.LogicalAndExpression, SyntaxKind.LogicalOrExpression);
    }

    #endregion // DiagnosticAnalyzer
}
