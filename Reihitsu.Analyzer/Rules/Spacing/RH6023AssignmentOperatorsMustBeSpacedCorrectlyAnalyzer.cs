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
/// RH6023: Assignment operators must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6023";

    /// <summary>
    /// Every <see cref="SyntaxKind"/> that produces an <see cref="AssignmentExpressionSyntax"/>. This is the
    /// closed set <see cref="SyntaxFacts.IsAssignmentExpression(SyntaxKind)"/> already owns; keeping the array
    /// here, rather than hand-listing the kinds again at the registration call, gives the test suite a single
    /// place to assert this list is still exhaustive
    /// </summary>
    internal static readonly ImmutableArray<SyntaxKind> AssignmentExpressionKinds = [
                                                                                        SyntaxKind.SimpleAssignmentExpression,
                                                                                        SyntaxKind.AddAssignmentExpression,
                                                                                        SyntaxKind.SubtractAssignmentExpression,
                                                                                        SyntaxKind.MultiplyAssignmentExpression,
                                                                                        SyntaxKind.DivideAssignmentExpression,
                                                                                        SyntaxKind.ModuloAssignmentExpression,
                                                                                        SyntaxKind.AndAssignmentExpression,
                                                                                        SyntaxKind.ExclusiveOrAssignmentExpression,
                                                                                        SyntaxKind.OrAssignmentExpression,
                                                                                        SyntaxKind.LeftShiftAssignmentExpression,
                                                                                        SyntaxKind.RightShiftAssignmentExpression,
                                                                                        SyntaxKind.UnsignedRightShiftAssignmentExpression,
                                                                                        SyntaxKind.CoalesceAssignmentExpression
                                                                                    ];

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6023Title), nameof(AnalyzerResources.RH6023MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the assignment operator token of a node, if any
    /// </summary>
    /// <param name="node">Node to inspect</param>
    /// <param name="operatorToken">Assignment operator token</param>
    /// <returns><see langword="true"/> if the node carries an assignment operator token</returns>
    private static bool TryGetAssignmentOperator(SyntaxNode node, out SyntaxToken operatorToken)
    {
        switch (node)
        {
            case AssignmentExpressionSyntax assignmentExpression:
                {
                    operatorToken = assignmentExpression.OperatorToken;

                    return true;
                }

            case EqualsValueClauseSyntax equalsValueClause:
                {
                    operatorToken = equalsValueClause.EqualsToken;

                    return true;
                }

            case NameEqualsSyntax nameEquals:
                {
                    operatorToken = nameEquals.EqualsToken;

                    return true;
                }

            default:
                {
                    operatorToken = default;

                    return false;
                }
        }
    }

    /// <summary>
    /// Analyzes an assignment expression, an equals-value clause, or a name-equals clause
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        if (TryGetAssignmentOperator(context.Node, out var operatorToken) == false)
        {
            return;
        }

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

        context.RegisterSyntaxNodeAction(OnSyntaxNode, [.. AssignmentExpressionKinds, SyntaxKind.EqualsValueClause, SyntaxKind.NameEquals]);
    }

    #endregion // DiagnosticAnalyzer
}