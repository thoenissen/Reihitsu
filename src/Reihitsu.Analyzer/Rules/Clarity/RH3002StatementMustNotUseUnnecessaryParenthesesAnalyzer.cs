using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
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
        : base(DiagnosticId, DiagnosticCategory.Clarity)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get the expression with every pair of parentheses directly around it removed
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns>The unparenthesized expression</returns>
    internal static ExpressionSyntax GetUnparenthesizedExpression(ExpressionSyntax expressionSyntax)
    {
        while (expressionSyntax is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            expressionSyntax = parenthesizedExpression.Expression;
        }

        return expressionSyntax;
    }

    /// <summary>
    /// Get the tokens of a node without the parentheses of the parenthesized expressions it contains
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="decisions">
    /// Decisions already taken during the current evaluation, to leave out only the parentheses that are reported
    /// themselves, as a Fix All removes them; or <see langword="null"/> to leave out the parentheses of every
    /// parenthesized expression, as the most a sequence of fixes can remove
    /// </param>
    /// <returns>The remaining tokens</returns>
    internal static IEnumerable<SyntaxToken> GetTokensWithoutParentheses(SyntaxNode node, Dictionary<ParenthesizedExpressionSyntax, bool> decisions)
    {
        // Each nested pair is decided once, at its opening parenthesis, so that its closing parenthesis follows that
        // decision without evaluating the pair again
        var removedExpressions = new HashSet<ParenthesizedExpressionSyntax>();

        foreach (var token in node.DescendantTokens())
        {
            if (token.Parent is ParenthesizedExpressionSyntax nestedExpression)
            {
                if (token == nestedExpression.OpenParenToken
                    && (decisions == null || ShouldReport(nestedExpression, decisions)))
                {
                    removedExpressions.Add(nestedExpression);

                    continue;
                }

                if (token == nestedExpression.CloseParenToken
                    && removedExpressions.Contains(nestedExpression))
                {
                    continue;
                }
            }

            yield return token;
        }
    }

    /// <summary>
    /// Determine whether the parentheses are reported, taking every decision at most once per evaluation. The guards
    /// look at nested parentheses from several enclosing pairs, so without the shared decisions the same nested pair
    /// would be decided again for each of them.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <param name="decisions">Decisions already taken for parenthesized expressions during the current evaluation</param>
    /// <returns><see langword="true"/> if the parentheses are unnecessary</returns>
    internal static bool ShouldReport(ParenthesizedExpressionSyntax parenthesizedExpression, Dictionary<ParenthesizedExpressionSyntax, bool> decisions)
    {
        if (decisions.TryGetValue(parenthesizedExpression, out var isUnnecessary) == false)
        {
            isUnnecessary = IsUnnecessary(parenthesizedExpression, decisions);
            decisions[parenthesizedExpression] = isUnnecessary;
        }

        return isUnnecessary;
    }

    /// <summary>
    /// Determine whether the token after a pair of parentheses around a type makes the parser read the pair as a cast: an
    /// opening parenthesis, a null-forgiving operator, a <see langword="with"/> expression, or an opening bracket after a
    /// generic name or a tuple
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression around a type shaped expression</param>
    /// <returns><see langword="true"/> if the parentheses would be read as a cast</returns>
    private static bool IsFollowedByCastOperand(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        switch (parenthesizedExpression.CloseParenToken.GetNextToken().Kind())
        {
            case SyntaxKind.OpenParenToken:
            case SyntaxKind.ExclamationToken:
            case SyntaxKind.WithKeyword:
                {
                    return true;
                }

            case SyntaxKind.OpenBracketToken:
                {
                    var tokens = GetTokensWithoutParentheses(parenthesizedExpression.Expression, null).ToList();

                    return tokens.Count > 0
                           && (tokens[0].IsKind(SyntaxKind.OpenParenToken) || tokens[tokens.Count - 1].IsKind(SyntaxKind.GreaterThanToken));
                }

            default:
                {
                    return false;
                }
        }
    }

    /// <summary>
    /// Determine whether removing the parentheses could leave a pair that the parser reads as a cast. That happens inside
    /// an outermost pair of parentheses that is followed by a cast operand and whose content could become a type once the
    /// pairs inside it are gone. If that content is itself parenthesized, the outermost pair and the pair directly inside
    /// it both stay, and deeper pairs can be removed because two pairs remain around the content. Otherwise, every pair
    /// inside the content stays, so the content never becomes a type, while the outermost pair itself can be removed.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if a cast could be left behind</returns>
    private static bool WouldLeaveCast(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        for (var node = (SyntaxNode)parenthesizedExpression; node is ExpressionSyntax or ArgumentSyntax; node = node.Parent)
        {
            if (node is not ParenthesizedExpressionSyntax outermostExpression
                || outermostExpression.Parent is ParenthesizedExpressionSyntax
                || IsFollowedByCastOperand(outermostExpression) == false
                || UnnecessaryParenthesesTypeArgumentListUtilities.IsTypeShaped(outermostExpression.Expression) == false)
            {
                continue;
            }

            if (outermostExpression.Expression is ParenthesizedExpressionSyntax directlyNestedExpression)
            {
                if (parenthesizedExpression == outermostExpression
                    || parenthesizedExpression == directlyNestedExpression)
                {
                    return true;
                }
            }
            else if (parenthesizedExpression != outermostExpression)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determine whether the parentheses open a position where the parser looks ahead for a declaration before it parses
    /// an expression: the start of an expression statement, the first initializer of a <see langword="for"/> statement
    /// without a declaration, the resource of a <see langword="using"/> statement, an <see langword="out"/> argument, or
    /// an element of a tuple
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <param name="isTupleElement">
    /// Whether the position is an element of a tuple, where only a declaration ending the
    /// element is read
    /// </param>
    /// <param name="lastToken">The last token the lookahead can read</param>
    /// <returns><see langword="true"/> if the parentheses open a declaration position</returns>
    private static bool OpensDeclarationPosition(ParenthesizedExpressionSyntax parenthesizedExpression, out bool isTupleElement, out SyntaxToken lastToken)
    {
        var openParenToken = parenthesizedExpression.OpenParenToken;

        isTupleElement = false;
        lastToken = default;

        for (SyntaxNode node = parenthesizedExpression; node != null && node.GetFirstToken() == openParenToken; node = node.Parent)
        {
            switch (node.Parent)
            {
                case ExpressionStatementSyntax expressionStatement:
                    {
                        lastToken = expressionStatement.SemicolonToken;

                        return true;
                    }

                case ForStatementSyntax forStatement when forStatement.Declaration == null
                                                          && forStatement.Initializers.FirstOrDefault() == node:
                    {
                        lastToken = forStatement.FirstSemicolonToken;

                        return true;
                    }

                case UsingStatementSyntax usingStatement when usingStatement.Expression == node:
                    {
                        lastToken = usingStatement.CloseParenToken;

                        return true;
                    }

                case ArgumentSyntax { Parent: TupleExpressionSyntax tupleExpression }:
                    {
                        isTupleElement = true;
                        lastToken = tupleExpression.CloseParenToken;

                        return true;
                    }

                case ArgumentSyntax argument when argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword):
                    {
                        lastToken = argument.Parent?.GetLastToken() ?? argument.GetLastToken();

                        return true;
                    }
            }
        }

        return false;
    }

    /// <summary>
    /// Get the tokens following a token, up to and including a last token
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="lastToken">Last token to return</param>
    /// <returns>The following tokens</returns>
    private static IEnumerable<SyntaxToken> GetFollowingTokens(SyntaxToken token, SyntaxToken lastToken)
    {
        while (token != lastToken)
        {
            token = token.GetNextToken();

            if (token.IsKind(SyntaxKind.None))
            {
                yield break;
            }

            yield return token;
        }
    }

    /// <summary>
    /// Determine whether removing the parentheses would let the parser read a declaration where an expression stands
    /// today, as in <c>(a * b) = 5;</c>, <c>(a with { }) = a;</c>, or the tuple <c>(d, (a &lt; b &gt; c))</c>. The parser
    /// reads a declaration when a name, optionally followed by pointer asterisks, is directly followed by an identifier.
    /// Inside a tuple, it only reads a declaration of a generic name that ends the element, so a pointer shape stays a
    /// multiplication there.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <param name="decisions">Decisions already taken for parenthesized expressions during the current evaluation</param>
    /// <returns><see langword="true"/> if a declaration would be read</returns>
    private static bool WouldStartDeclaration(ParenthesizedExpressionSyntax parenthesizedExpression, Dictionary<ParenthesizedExpressionSyntax, bool> decisions)
    {
        if (OpensDeclarationPosition(parenthesizedExpression, out var isTupleElement, out var lastToken) == false)
        {
            return false;
        }

        var kindAt = UnnecessaryParenthesesTypeArgumentListUtilities.CreateKindReader(GetTokensWithoutParentheses(parenthesizedExpression.Expression, decisions).Concat(GetFollowingTokens(parenthesizedExpression.CloseParenToken, lastToken)));
        var index = UnnecessaryParenthesesTypeArgumentListUtilities.ScanName(kindAt, 0);

        if (index < 0)
        {
            return false;
        }

        if (isTupleElement)
        {
            return kindAt(index - 1) == SyntaxKind.GreaterThanToken
                   && kindAt(index) == SyntaxKind.IdentifierToken
                   && kindAt(index + 1) is SyntaxKind.CommaToken or SyntaxKind.CloseParenToken;
        }

        while (kindAt(index) == SyntaxKind.AsteriskToken)
        {
            index++;
        }

        return kindAt(index) == SyntaxKind.IdentifierToken
               || SyntaxFacts.IsContextualKeyword(kindAt(index));
    }

    /// <summary>
    /// Determine whether removing the parentheses would turn an element of an initializer into a member initializer.
    /// The parser reads an element as a member initializer when it starts with an identifier followed by <c>=</c>, or
    /// with an opening bracket, inside an object, collection, or <see langword="with"/> initializer and inside an
    /// anonymous object creation.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <param name="decisions">Decisions already taken for parenthesized expressions during the current evaluation</param>
    /// <returns><see langword="true"/> if a member initializer would be read</returns>
    private static bool WouldStartMemberInitializer(ParenthesizedExpressionSyntax parenthesizedExpression, Dictionary<ParenthesizedExpressionSyntax, bool> decisions)
    {
        if (parenthesizedExpression.Parent is not AssignmentExpressionSyntax assignmentExpression
            || assignmentExpression.Left != parenthesizedExpression
            || assignmentExpression.IsKind(SyntaxKind.SimpleAssignmentExpression) == false)
        {
            return false;
        }

        var isInitializerElement = assignmentExpression.Parent switch
                                   {
                                       InitializerExpressionSyntax initializerExpression => initializerExpression.IsKind(SyntaxKind.ObjectInitializerExpression)
                                                                                            || initializerExpression.IsKind(SyntaxKind.CollectionInitializerExpression)
                                                                                            || initializerExpression.IsKind(SyntaxKind.WithInitializerExpression),
                                       AnonymousObjectMemberDeclaratorSyntax memberDeclarator => memberDeclarator.NameEquals == null,
                                       _ => false
                                   };

        if (isInitializerElement == false)
        {
            return false;
        }

        var remainingTokens = GetTokensWithoutParentheses(parenthesizedExpression.Expression, decisions).Take(2).ToList();

        return remainingTokens.Count > 0
               && (remainingTokens[0].IsKind(SyntaxKind.OpenBracketToken)
                   || (remainingTokens.Count == 1 && remainingTokens[0].IsKind(SyntaxKind.IdentifierToken)));
    }

    /// <summary>
    /// Determine whether the inner expression is safe in chaining contexts. Nested parentheses that are reported
    /// themselves are looked through, because they are removed as well, and the expression that remains decides. A
    /// nested pair that is not reported stays in place and therefore keeps the chain safe. A nest whose remaining pair
    /// would be read as a cast is kept by <see cref="WouldLeaveCast"/> before this check is reached.
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <param name="decisions">Decisions already taken for parenthesized expressions during the current evaluation</param>
    /// <returns><see langword="true"/> if the expression is safe</returns>
    private static bool IsSafeChainExpression(ExpressionSyntax expressionSyntax, Dictionary<ParenthesizedExpressionSyntax, bool> decisions)
    {
        // An expression needing one pair keeps its outer pair unreported and only the redundant inner pairs are
        // reported. Fix All therefore never removes the last pair
        while (expressionSyntax is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            if (ShouldReport(parenthesizedExpression, decisions) == false)
            {
                return true;
            }

            expressionSyntax = parenthesizedExpression.Expression;
        }

        return expressionSyntax is IdentifierNameSyntax
                                or GenericNameSyntax
                                or LiteralExpressionSyntax
                                or ThisExpressionSyntax
                                or BaseExpressionSyntax
                                or InvocationExpressionSyntax
                                or MemberAccessExpressionSyntax
                                or ElementAccessExpressionSyntax
                                or ObjectCreationExpressionSyntax
                                or ImplicitObjectCreationExpressionSyntax;
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
        return GetUnparenthesizedExpression(expressionSyntax) is not (ConditionalExpressionSyntax or AssignmentExpressionSyntax or QueryExpressionSyntax);
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
    /// <param name="decisions">Decisions already taken for parenthesized expressions during the current evaluation</param>
    /// <returns><see langword="true"/> if the expression is safe</returns>
    private static bool IsSafeAssignmentTarget(ExpressionSyntax expressionSyntax, Dictionary<ParenthesizedExpressionSyntax, bool> decisions)
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

                case ParenthesizedExpressionSyntax parenthesizedExpression when ShouldReport(parenthesizedExpression, decisions):
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
    /// <returns>The trailing expression, or <see langword="null"/> if the pattern ends in a closing token, a type, a designation, or a discard</returns>
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
    /// <param name="decisions">Decisions already taken for parenthesized expressions during the current evaluation</param>
    /// <returns><see langword="true"/> if the parentheses are unnecessary</returns>
    private static bool IsUnnecessary(ParenthesizedExpressionSyntax parenthesizedExpression, Dictionary<ParenthesizedExpressionSyntax, bool> decisions)
    {
        var innerExpression = parenthesizedExpression.Expression;

        if (innerExpression is CastExpressionSyntax
                            or LambdaExpressionSyntax
                            or AnonymousMethodExpressionSyntax
                            or SwitchExpressionSyntax)
        {
            return false;
        }

        // Parentheses stay whenever the parser would read the code around them differently once they are gone: as a cast,
        // as a local declaration, as a member initializer, or as a generic name. These checks apply to every parent,
        // because the reading depends on the tokens before and after the parentheses rather than on the parent alone
        if (WouldLeaveCast(parenthesizedExpression)
            || WouldStartDeclaration(parenthesizedExpression, decisions)
            || WouldStartMemberInitializer(parenthesizedExpression, decisions)
            || UnnecessaryParenthesesTypeArgumentListUtilities.WouldDetachTypeArgumentList(parenthesizedExpression, decisions)
            || UnnecessaryParenthesesTypeArgumentListUtilities.WouldJoinTypeArgumentListAcrossArguments(parenthesizedExpression))
        {
            return false;
        }

        // Parentheses ending an assignment's left operand stay when the assignment operator would otherwise be parsed
        // into the inner expression. This applies to every parent, because the pair does not have to be the left
        // operand itself, for example the operand of a throw expression ending a coalesce target
        if (IsFollowedByAssignmentOperator(parenthesizedExpression)
            && IsSafeAssignmentTarget(innerExpression, decisions) == false)
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
                   MemberAccessExpressionSyntax memberAccessExpression when memberAccessExpression.Expression == parenthesizedExpression => IsSafeChainExpression(innerExpression, decisions),
                   InvocationExpressionSyntax invocationExpression when invocationExpression.Expression == parenthesizedExpression => IsSafeChainExpression(innerExpression, decisions),
                   ElementAccessExpressionSyntax elementAccessExpression when elementAccessExpression.Expression == parenthesizedExpression => IsSafeChainExpression(innerExpression, decisions),
                   AwaitExpressionSyntax => IsSafeChainExpression(innerExpression, decisions),
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
            && ShouldReport(parenthesizedExpression, new Dictionary<ParenthesizedExpressionSyntax, bool>()))
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

        context.RegisterStructuredTriviaAction(OnParenthesizedExpression, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ParenthesizedExpression);
    }

    #endregion // DiagnosticAnalyzer
}