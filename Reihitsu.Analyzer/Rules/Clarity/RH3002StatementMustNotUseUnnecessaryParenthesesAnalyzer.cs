using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    /// Determine whether the inner expression keeps its meaning when the parentheses are removed in front of an
    /// assignment operator. Without the parentheses, the assignment operator would be parsed into the inner expression
    /// whenever that expression ends, along its rightmost operand, in a conditional's false branch, an assignment's
    /// right operand, a query's last clause, or a conditional access. The rightmost operand is followed through binary,
    /// prefix unary, null-forgiving, await, cast, throw, range, and pattern expressions, and through nested parentheses
    /// that are reported themselves, because those are removed as well. Any other node closes the expression before
    /// the assignment operator.
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns><see langword="true"/> if the expression is safe</returns>
    private static bool IsSafeAssignmentTarget(ExpressionSyntax expressionSyntax)
    {
        while (true)
        {
            switch (expressionSyntax)
            {
                case ConditionalExpressionSyntax:
                case AssignmentExpressionSyntax:
                case QueryExpressionSyntax:
                case ConditionalAccessExpressionSyntax:
                    return false;

                case ParenthesizedExpressionSyntax parenthesizedExpression when ShouldReport(parenthesizedExpression):
                    expressionSyntax = parenthesizedExpression.Expression;
                    break;

                case BinaryExpressionSyntax binaryExpression:
                    expressionSyntax = binaryExpression.Right;
                    break;

                case PrefixUnaryExpressionSyntax prefixUnaryExpression:
                    expressionSyntax = prefixUnaryExpression.Operand;
                    break;

                case PostfixUnaryExpressionSyntax postfixUnaryExpression when postfixUnaryExpression.IsKind(SyntaxKind.SuppressNullableWarningExpression):
                    expressionSyntax = postfixUnaryExpression.Operand;
                    break;

                case AwaitExpressionSyntax awaitExpression:
                    expressionSyntax = awaitExpression.Expression;
                    break;

                case CastExpressionSyntax castExpression:
                    expressionSyntax = castExpression.Expression;
                    break;

                case ThrowExpressionSyntax throwExpression:
                    expressionSyntax = throwExpression.Expression;
                    break;

                case RangeExpressionSyntax { RightOperand: { } rightOperand }:
                    expressionSyntax = rightOperand;
                    break;

                case IsPatternExpressionSyntax isPatternExpression when GetTrailingPatternExpression(isPatternExpression.Pattern) is { } patternExpression:
                    expressionSyntax = patternExpression;
                    break;

                default:
                    return true;
            }
        }
    }

    /// <summary>
    /// Get the expression a pattern ends in, following the rightmost operand of combined and negated patterns
    /// </summary>
    /// <param name="patternSyntax">Pattern syntax</param>
    /// <returns>The trailing expression, or <see langword="null"/> if the pattern ends in a closing token or a type</returns>
    private static ExpressionSyntax GetTrailingPatternExpression(PatternSyntax patternSyntax)
    {
        while (true)
        {
            switch (patternSyntax)
            {
                case ConstantPatternSyntax constantPattern:
                    return constantPattern.Expression;

                case RelationalPatternSyntax relationalPattern:
                    return relationalPattern.Expression;

                case UnaryPatternSyntax unaryPattern:
                    patternSyntax = unaryPattern.Pattern;
                    break;

                case BinaryPatternSyntax binaryPattern:
                    patternSyntax = binaryPattern.Right;
                    break;

                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Determine whether the parentheses are directly followed by the operator of an assignment, so that they end the
    /// assignment's left operand
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if an assignment operator follows the parentheses</returns>
    private static bool IsFollowedByAssignmentOperator(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        var nextToken = parenthesizedExpression.CloseParenToken.GetNextToken();

        return nextToken.Parent is AssignmentExpressionSyntax assignmentExpression
               && assignmentExpression.OperatorToken == nextToken;
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

        // Parentheses ending an assignment's left operand stay when the assignment operator would otherwise be parsed
        // into the inner expression. This applies to every parent, because the pair does not have to be the left
        // operand itself, for example the operand of a throw expression ending a coalesce target
        if (IsFollowedByAssignmentOperator(parenthesizedExpression)
            && IsSafeAssignmentTarget(innerExpression) == false)
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