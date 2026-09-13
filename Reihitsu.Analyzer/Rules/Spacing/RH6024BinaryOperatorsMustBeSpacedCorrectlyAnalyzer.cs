using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6024: Binary operators must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6024";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Every <see cref="SyntaxKind"/> that produces a <see cref="BinaryExpressionSyntax"/>. This is the closed
    /// set <see cref="SyntaxFacts.IsBinaryExpression(SyntaxKind)"/> already owns; keeping the array here,
    /// rather than hand-listing the kinds again at the registration call, gives the test suite a single place
    /// to assert this list is still exhaustive
    /// </summary>
    internal static readonly ImmutableArray<SyntaxKind> BinaryExpressionKinds = [
                                                                                    SyntaxKind.AddExpression,
                                                                                    SyntaxKind.SubtractExpression,
                                                                                    SyntaxKind.MultiplyExpression,
                                                                                    SyntaxKind.DivideExpression,
                                                                                    SyntaxKind.ModuloExpression,
                                                                                    SyntaxKind.LeftShiftExpression,
                                                                                    SyntaxKind.RightShiftExpression,
                                                                                    SyntaxKind.UnsignedRightShiftExpression,
                                                                                    SyntaxKind.LogicalOrExpression,
                                                                                    SyntaxKind.LogicalAndExpression,
                                                                                    SyntaxKind.BitwiseOrExpression,
                                                                                    SyntaxKind.BitwiseAndExpression,
                                                                                    SyntaxKind.ExclusiveOrExpression,
                                                                                    SyntaxKind.EqualsExpression,
                                                                                    SyntaxKind.NotEqualsExpression,
                                                                                    SyntaxKind.LessThanExpression,
                                                                                    SyntaxKind.LessThanOrEqualExpression,
                                                                                    SyntaxKind.GreaterThanExpression,
                                                                                    SyntaxKind.GreaterThanOrEqualExpression,
                                                                                    SyntaxKind.IsExpression,
                                                                                    SyntaxKind.AsExpression,
                                                                                    SyntaxKind.CoalesceExpression
                                                                                ];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6024Title), nameof(AnalyzerResources.RH6024MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a binary expression
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        // A directive condition (e.g. #if A && B) parses its operators into the same BinaryExpressionSyntax
        // shape as ordinary code, so it must be excluded explicitly: the formatter never rewrites a directive
        // condition, and this rule must not report a diagnostic there either.
        if (context.Node.IsPartOfStructuredTrivia())
        {
            return;
        }

        var operatorToken = ((BinaryExpressionSyntax)context.Node).OperatorToken;
        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);

        if (FormattingTextAnalysisUtilities.HasOperatorSpacingViolation(sourceText, operatorToken))
        {
            context.ReportDiagnostic(CreateDiagnostic(operatorToken.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSyntaxNode, [.. BinaryExpressionKinds]);
    }

    #endregion // DiagnosticAnalyzer
}