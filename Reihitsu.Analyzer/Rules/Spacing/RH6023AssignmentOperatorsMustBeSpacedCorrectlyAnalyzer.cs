using Microsoft.CodeAnalysis;
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
                operatorToken = assignmentExpression.OperatorToken;

                return true;

            case EqualsValueClauseSyntax equalsValueClause:
                operatorToken = equalsValueClause.EqualsToken;

                return true;

            case NameEqualsSyntax nameEquals:
                operatorToken = nameEquals.EqualsToken;

                return true;

            default:
                operatorToken = default;

                return false;
        }
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var node in root.DescendantNodes())
        {
            if (TryGetAssignmentOperator(node, out var operatorToken)
                && FormattingTextAnalysisUtilities.HasOperatorSpacingViolation(sourceText, operatorToken))
            {
                context.ReportDiagnostic(CreateDiagnostic(operatorToken.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}