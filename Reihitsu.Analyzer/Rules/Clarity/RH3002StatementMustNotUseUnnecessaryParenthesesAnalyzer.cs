using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3002: Statement must not use unnecessary parentheses
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3002";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3002Title), nameof(AnalyzerResources.RH3002MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the inner expression is safe in chaining contexts
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns><see langword="true"/> if the expression is safe</returns>
    private static bool IsSafeChainExpression(ExpressionSyntax expressionSyntax)
    {
        return expressionSyntax is IdentifierNameSyntax
                                or GenericNameSyntax
                                or LiteralExpressionSyntax
                                or ThisExpressionSyntax
                                or BaseExpressionSyntax
                                or InvocationExpressionSyntax
                                or MemberAccessExpressionSyntax
                                or ElementAccessExpressionSyntax
                                or ObjectCreationExpressionSyntax
                                or ImplicitObjectCreationExpressionSyntax
                                or ParenthesizedExpressionSyntax;
    }

    /// <summary>
    /// Determine whether the inner expression keeps its meaning as the operand of a throw expression. The operand of a
    /// throw expression is only parsed down to the null-coalescing level, so a conditional or an assignment would bind
    /// the throw expression into itself once the parentheses are gone, and a query expression would only be accepted
    /// with a compiler warning.
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns><see langword="true"/> if the expression is safe</returns>
    private static bool IsSafeThrowExpressionOperand(ExpressionSyntax expressionSyntax)
    {
        // Nested parentheses are looked through, so that an operand needing one pair keeps its outer pair unreported
        // and only the redundant inner pairs are reported. Fix All therefore never removes the last pair
        while (expressionSyntax is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            expressionSyntax = parenthesizedExpression.Expression;
        }

        return expressionSyntax is not (ConditionalExpressionSyntax or AssignmentExpressionSyntax or QueryExpressionSyntax);
    }

    /// <summary>
    /// Determine whether the parentheses are unnecessary
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if the parentheses are unnecessary</returns>
    private static bool ShouldReport(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        var innerExpression = parenthesizedExpression.Expression;

        if (innerExpression is CastExpressionSyntax
                            or LambdaExpressionSyntax
                            or AnonymousMethodExpressionSyntax
                            or SwitchExpressionSyntax)
        {
            return false;
        }

        return parenthesizedExpression.Parent switch
               {
                   ParenthesizedExpressionSyntax => true,
                   ReturnStatementSyntax => true,
                   ThrowStatementSyntax => true,
                   ThrowExpressionSyntax => IsSafeThrowExpressionOperand(innerExpression),
                   EqualsValueClauseSyntax => true,
                   ArrowExpressionClauseSyntax => true,
                   ArgumentSyntax => true,
                   AssignmentExpressionSyntax => true,
                   MemberAccessExpressionSyntax memberAccessExpression when memberAccessExpression.Expression == parenthesizedExpression => IsSafeChainExpression(innerExpression),
                   InvocationExpressionSyntax invocationExpression when invocationExpression.Expression == parenthesizedExpression => IsSafeChainExpression(innerExpression),
                   ElementAccessExpressionSyntax elementAccessExpression when elementAccessExpression.Expression == parenthesizedExpression => IsSafeChainExpression(innerExpression),
                   AwaitExpressionSyntax => IsSafeChainExpression(innerExpression),
                   _ => false
               };
    }

    /// <summary>
    /// Analyze parenthesized expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnParenthesizedExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ParenthesizedExpressionSyntax parenthesizedExpression
            && ShouldReport(parenthesizedExpression))
        {
            context.ReportDiagnostic(CreateDiagnostic(parenthesizedExpression.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnParenthesizedExpression, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ParenthesizedExpression);
    }

    #endregion // DiagnosticAnalyzer
}