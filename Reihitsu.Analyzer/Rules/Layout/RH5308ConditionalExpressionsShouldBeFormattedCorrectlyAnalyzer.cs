using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5308: Conditional expressions should be formatted correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5308";

    /// <summary>
    /// The number of spaces the <c>?</c> and <c>:</c> tokens are indented relative to the condition
    /// </summary>
    private const int IndentSize = 4;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5308Title), nameof(AnalyzerResources.RH5308MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the location of the first misaligned <c>?</c> or <c>:</c> token within the conditional
    /// chain, indenting each nested level one further step just like the formatter, or
    /// <see langword="null"/> when the whole chain is laid out correctly
    /// </summary>
    /// <param name="conditional">The conditional expression to check</param>
    /// <param name="operatorColumn">The column the <c>?</c> and <c>:</c> tokens must start at</param>
    /// <returns>The location of the first misaligned operator token, or <see langword="null"/></returns>
    private static Location FindFirstMisalignment(ConditionalExpressionSyntax conditional, int operatorColumn)
    {
        var questionMisalignment = GetMisalignment(conditional.QuestionToken, conditional.Condition.GetLastToken(), operatorColumn);

        if (questionMisalignment != null)
        {
            return questionMisalignment;
        }

        var colonMisalignment = GetMisalignment(conditional.ColonToken, conditional.WhenTrue.GetLastToken(), operatorColumn);

        if (colonMisalignment != null)
        {
            return colonMisalignment;
        }

        var nestedColumn = operatorColumn + IndentSize;

        if (conditional.WhenTrue is ConditionalExpressionSyntax nestedTrue)
        {
            var nestedMisalignment = FindFirstMisalignment(nestedTrue, nestedColumn);

            if (nestedMisalignment != null)
            {
                return nestedMisalignment;
            }
        }

        if (conditional.WhenFalse is ConditionalExpressionSyntax nestedFalse)
        {
            return FindFirstMisalignment(nestedFalse, nestedColumn);
        }

        return null;
    }

    /// <summary>
    /// Returns the operator token's location when it does not start its own line or is not indented to
    /// the expected column, otherwise <see langword="null"/>
    /// </summary>
    /// <param name="operatorToken">The <c>?</c> or <c>:</c> token</param>
    /// <param name="previousToken">The last token of the preceding operand</param>
    /// <param name="operatorColumn">The column the operator token must start at</param>
    /// <returns>The misaligned token's location, or <see langword="null"/></returns>
    private static Location GetMisalignment(SyntaxToken operatorToken, SyntaxToken previousToken, int operatorColumn)
    {
        var previousEndLine = previousToken.GetLocation().GetLineSpan().EndLinePosition.Line;
        var operatorLineSpan = operatorToken.GetLocation().GetLineSpan();

        if (operatorLineSpan.StartLinePosition.Line == previousEndLine)
        {
            return operatorToken.GetLocation();
        }

        if (operatorLineSpan.StartLinePosition.Character != operatorColumn)
        {
            return operatorToken.GetLocation();
        }

        return null;
    }

    /// <summary>
    /// Analyzing conditional (ternary) expressions for correct formatting
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConditionalExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConditionalExpressionSyntax conditional)
        {
            return;
        }

        // Only process the outermost conditional; nested conditionals are reached by recursion so the
        // anchor column matches the formatter's stair layout regardless of dispatch order.
        if (conditional.Parent is ConditionalExpressionSyntax)
        {
            return;
        }

        // A conditional that fits on a single line is never reported; only multi-line layout matters.
        var conditionalLineSpan = conditional.SyntaxTree.GetLineSpan(conditional.Span);

        if (conditionalLineSpan.StartLinePosition.Line == conditionalLineSpan.EndLinePosition.Line)
        {
            return;
        }

        var conditionFirstToken = conditional.Condition.GetFirstToken();
        var conditionColumn = conditionFirstToken.GetLocation().GetLineSpan().StartLinePosition.Character;

        // A single diagnostic is reported per conditional chain so the code fix can re-lay out the
        // whole expression once without conflicting fixes for the '?' and ':' of the same expression.
        var location = FindFirstMisalignment(conditional, conditionColumn + IndentSize);

        if (location != null)
        {
            context.ReportDiagnostic(CreateDiagnostic(location));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnConditionalExpression, SyntaxKind.ConditionalExpression);
    }

    #endregion // DiagnosticAnalyzer
}