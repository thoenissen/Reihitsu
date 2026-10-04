using System.Collections.Generic;
using System.Linq;

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
    /// Determine whether the expression has the shape of a type name: a simple or generic name, a member access of such
    /// names, or a tuple of such names. The parser reads parentheses directly around such an expression as a cast when a
    /// cast operand follows them.
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns><see langword="true"/> if the expression has the shape of a type name</returns>
    private static bool IsTypeShaped(ExpressionSyntax expressionSyntax)
    {
        return expressionSyntax switch
               {
                   IdentifierNameSyntax or GenericNameSyntax => true,
                   MemberAccessExpressionSyntax memberAccessExpression => memberAccessExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                                                                          && IsTypeShaped(memberAccessExpression.Expression),
                   TupleExpressionSyntax tupleExpression => tupleExpression.Arguments.All(static argument => argument.NameColon == null
                                                                                                             && argument.RefKindKeyword.IsKind(SyntaxKind.None)
                                                                                                             && IsTypeShaped(argument.Expression)),
                   _ => false
               };
    }

    /// <summary>
    /// Determine whether the parentheses are one of the two outermost pairs of directly nested parentheses around a type
    /// shaped expression that are followed by a token starting a cast operand: an opening parenthesis, a null-forgiving
    /// operator, a <see langword="with"/> expression, or an opening bracket after a generic name or a tuple. Removing one
    /// of these pairs leaves a single pair that the parser reads as a cast, so both stay. Deeper pairs can be removed,
    /// because two pairs remain around the expression.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if the parentheses would leave a cast behind</returns>
    private static bool IsInCastProneNest(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        var outermostExpression = parenthesizedExpression;
        var depthFromOutermost = 0;

        while (outermostExpression.Parent is ParenthesizedExpressionSyntax parentExpression)
        {
            outermostExpression = parentExpression;
            depthFromOutermost++;
        }

        if (depthFromOutermost > 1)
        {
            return false;
        }

        var innermostExpression = parenthesizedExpression.Expression;

        while (innermostExpression is ParenthesizedExpressionSyntax nestedExpression)
        {
            innermostExpression = nestedExpression.Expression;
        }

        if (IsTypeShaped(innermostExpression) == false)
        {
            return false;
        }

        var nextToken = outermostExpression.CloseParenToken.GetNextToken();

        return nextToken.Kind() switch
               {
                   SyntaxKind.OpenParenToken or SyntaxKind.ExclamationToken or SyntaxKind.WithKeyword => true,
                   SyntaxKind.OpenBracketToken => innermostExpression is TupleExpressionSyntax
                                                  || innermostExpression.GetLastToken().IsKind(SyntaxKind.GreaterThanToken),
                   _ => false
               };
    }

    /// <summary>
    /// Get the tokens of the inner expression that remain once every nested pair of parentheses that is reported itself
    /// has been removed as well
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns>The remaining tokens of the inner expression</returns>
    private static IEnumerable<SyntaxToken> GetRemainingInnerTokens(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        foreach (var token in parenthesizedExpression.Expression.DescendantTokens())
        {
            if (token.Parent is ParenthesizedExpressionSyntax nestedExpression
                && (token == nestedExpression.OpenParenToken || token == nestedExpression.CloseParenToken)
                && ShouldReport(nestedExpression))
            {
                continue;
            }

            yield return token;
        }
    }

    /// <summary>
    /// Get the last token of the inner expression that remains once every nested pair of parentheses that is reported
    /// itself has been removed as well
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns>The remaining last token of the inner expression</returns>
    private static SyntaxToken GetRemainingLastInnerToken(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        var token = parenthesizedExpression.Expression.GetLastToken();

        while (token.Parent is ParenthesizedExpressionSyntax nestedExpression
               && token == nestedExpression.CloseParenToken
               && ShouldReport(nestedExpression))
        {
            token = token.GetPreviousToken();
        }

        return token;
    }

    /// <summary>
    /// Determine whether the parentheses open a position where the parser looks ahead for a local declaration before it
    /// parses an expression: the start of an expression statement, the first initializer of a <see langword="for"/>
    /// statement without a declaration, the resource of a <see langword="using"/> statement, or an <see langword="out"/>
    /// argument
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if the parentheses open a declaration position</returns>
    private static bool OpensDeclarationPosition(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        var openParenToken = parenthesizedExpression.OpenParenToken;
        SyntaxNode node = parenthesizedExpression;

        while (node != null
               && node.GetFirstToken() == openParenToken)
        {
            switch (node.Parent)
            {
                case ExpressionStatementSyntax:
                    return true;

                case ForStatementSyntax forStatement when forStatement.Declaration == null
                                                          && forStatement.Initializers.FirstOrDefault() == node:
                    return true;

                case UsingStatementSyntax usingStatement when usingStatement.Expression == node:
                    return true;

                case ArgumentSyntax argument when argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword):
                    return true;
            }

            node = node.Parent;
        }

        return false;
    }

    /// <summary>
    /// Determine whether the tokens start like a local declaration: a type name, optionally followed by pointer
    /// asterisks, and then an identifier
    /// </summary>
    /// <param name="tokens">Tokens</param>
    /// <returns><see langword="true"/> if the tokens start like a local declaration</returns>
    private static bool StartsLikeDeclaration(IEnumerable<SyntaxToken> tokens)
    {
        using (var enumerator = tokens.GetEnumerator())
        {
            if (enumerator.MoveNext() == false
                || TryScanTypeName(enumerator) == false)
            {
                return false;
            }

            while (enumerator.Current.IsKind(SyntaxKind.AsteriskToken))
            {
                if (enumerator.MoveNext() == false)
                {
                    return false;
                }
            }

            return enumerator.Current.IsKind(SyntaxKind.IdentifierToken)
                   || SyntaxFacts.IsContextualKeyword(enumerator.Current.Kind());
        }
    }

    /// <summary>
    /// Scan a type name, starting at the current token: identifiers separated by dots or <c>::</c>, each optionally
    /// followed by a type argument list. On success, the enumerator is positioned on the token following the name.
    /// </summary>
    /// <param name="enumerator">Token enumerator, positioned on the first token of the name</param>
    /// <returns><see langword="true"/> if a type name was scanned and a token follows it</returns>
    private static bool TryScanTypeName(IEnumerator<SyntaxToken> enumerator)
    {
        while (true)
        {
            if (enumerator.Current.IsKind(SyntaxKind.IdentifierToken) == false
                || enumerator.MoveNext() == false)
            {
                return false;
            }

            if (enumerator.Current.IsKind(SyntaxKind.LessThanToken))
            {
                do
                {
                    if (enumerator.MoveNext() == false)
                    {
                        return false;
                    }

                    if (SyntaxFacts.IsPredefinedType(enumerator.Current.Kind()))
                    {
                        if (enumerator.MoveNext() == false)
                        {
                            return false;
                        }
                    }
                    else if (TryScanTypeName(enumerator) == false)
                    {
                        return false;
                    }
                }
                while (enumerator.Current.IsKind(SyntaxKind.CommaToken));

                if (enumerator.Current.IsKind(SyntaxKind.GreaterThanToken) == false
                    || enumerator.MoveNext() == false)
                {
                    return false;
                }
            }

            if (enumerator.Current.IsKind(SyntaxKind.DotToken) == false
                && enumerator.Current.IsKind(SyntaxKind.ColonColonToken) == false)
            {
                return true;
            }

            if (enumerator.MoveNext() == false)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Determine whether removing the parentheses would let the parser read a local declaration where an expression
    /// stands today, as in <c>(a * b) = 5;</c> or <c>(a with { }) = a;</c>
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if a declaration would be read</returns>
    private static bool WouldStartDeclaration(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        return OpensDeclarationPosition(parenthesizedExpression)
               && StartsLikeDeclaration(GetRemainingInnerTokens(parenthesizedExpression).Concat(GetFollowingTokens(parenthesizedExpression.CloseParenToken)));
    }

    /// <summary>
    /// Get the tokens following a token, up to the end of the syntax tree
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>The following tokens</returns>
    private static IEnumerable<SyntaxToken> GetFollowingTokens(SyntaxToken token)
    {
        token = token.GetNextToken();

        while (token.IsKind(SyntaxKind.None) == false)
        {
            yield return token;

            token = token.GetNextToken();
        }
    }

    /// <summary>
    /// Determine whether removing the parentheses would turn an element of an initializer into a member initializer.
    /// The parser reads an element as a member initializer when it starts with an identifier followed by <c>=</c>, or
    /// with an opening bracket, inside an object, collection, or <see langword="with"/> initializer and inside an
    /// anonymous object creation.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if a member initializer would be read</returns>
    private static bool WouldStartMemberInitializer(ParenthesizedExpressionSyntax parenthesizedExpression)
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

        var remainingTokens = GetRemainingInnerTokens(parenthesizedExpression).Take(2).ToList();

        return remainingTokens.Count > 0
               && (remainingTokens[0].IsKind(SyntaxKind.OpenBracketToken)
                   || (remainingTokens.Count == 1 && remainingTokens[0].IsKind(SyntaxKind.IdentifierToken)));
    }

    /// <summary>
    /// Determine whether removing the parentheses would separate a type argument list from the token that keeps it a
    /// type argument list. A name followed by <c>&lt;…&gt;</c> is only read as a generic name when the token after the
    /// <c>&gt;</c> is one of <c>( ) ] } : ; , . ? == != | ^ &amp;&amp; || &amp; [</c>; otherwise the angle brackets are read
    /// as relational operators.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if the type argument list would lose its following token</returns>
    private static bool WouldDetachTypeArgumentList(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        if (IsTypeArgumentListFollower(parenthesizedExpression.CloseParenToken.GetNextToken()))
        {
            return false;
        }

        var lastToken = GetRemainingLastInnerToken(parenthesizedExpression);

        return lastToken.IsKind(SyntaxKind.GreaterThanToken)
               && lastToken.Parent is TypeArgumentListSyntax;
    }

    /// <summary>
    /// Determine whether the token keeps a preceding type argument list a type argument list
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token follows a type argument list</returns>
    private static bool IsTypeArgumentListFollower(SyntaxToken token)
    {
        return token.Kind() is SyntaxKind.OpenParenToken
                            or SyntaxKind.CloseParenToken
                            or SyntaxKind.CloseBracketToken
                            or SyntaxKind.CloseBraceToken
                            or SyntaxKind.ColonToken
                            or SyntaxKind.SemicolonToken
                            or SyntaxKind.CommaToken
                            or SyntaxKind.DotToken
                            or SyntaxKind.QuestionToken
                            or SyntaxKind.EqualsEqualsToken
                            or SyntaxKind.ExclamationEqualsToken
                            or SyntaxKind.BarToken
                            or SyntaxKind.CaretToken
                            or SyntaxKind.AmpersandAmpersandToken
                            or SyntaxKind.BarBarToken
                            or SyntaxKind.AmpersandToken
                            or SyntaxKind.OpenBracketToken;
    }

    /// <summary>
    /// Get the expression of an argument with every pair of parentheses directly around it removed
    /// </summary>
    /// <param name="argument">Argument</param>
    /// <returns>The unparenthesized expression</returns>
    private static ExpressionSyntax GetUnparenthesizedExpression(ArgumentSyntax argument)
    {
        var expression = argument.Expression;

        while (expression is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            expression = parenthesizedExpression.Expression;
        }

        return expression;
    }

    /// <summary>
    /// Determine whether the argument, once its parentheses are gone, ends in <c>name &lt; type</c>, so that it can open a
    /// type argument list that a following argument closes
    /// </summary>
    /// <param name="argument">Argument</param>
    /// <returns><see langword="true"/> if the argument can open a type argument list</returns>
    private static bool CanOpenTypeArgumentList(ArgumentSyntax argument)
    {
        var expression = GetUnparenthesizedExpression(argument);

        while (expression is BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.IsKind(SyntaxKind.LessThanExpression)
                && IsTypeShaped(binaryExpression.Right)
                && binaryExpression.Left.GetLastToken().IsKind(SyntaxKind.IdentifierToken))
            {
                return true;
            }

            expression = binaryExpression.Right;
        }

        return false;
    }

    /// <summary>
    /// Determine whether the argument, once its parentheses are gone, can continue a type argument list that a preceding
    /// argument opened: an unnamed type shaped argument without <see langword="ref"/>, <see langword="out"/>, or
    /// <see langword="in"/>
    /// </summary>
    /// <param name="argument">Argument</param>
    /// <returns><see langword="true"/> if the argument can continue a type argument list</returns>
    private static bool CanContinueTypeArgumentList(ArgumentSyntax argument)
    {
        return argument.NameColon == null
               && argument.RefKindKeyword.IsKind(SyntaxKind.None)
               && IsTypeShaped(GetUnparenthesizedExpression(argument));
    }

    /// <summary>
    /// Determine whether the argument, once its parentheses are gone, starts with <c>type &gt; (</c> or
    /// <c>type &gt; [</c>, so that it closes a type argument list that a preceding argument opened
    /// </summary>
    /// <param name="argument">Argument</param>
    /// <returns><see langword="true"/> if the argument can close a type argument list</returns>
    private static bool CanCloseTypeArgumentList(ArgumentSyntax argument)
    {
        if (argument.NameColon != null
            || argument.RefKindKeyword.IsKind(SyntaxKind.None) == false)
        {
            return false;
        }

        var expression = GetUnparenthesizedExpression(argument);

        while (expression is BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.IsKind(SyntaxKind.GreaterThanExpression)
                && IsTypeShaped(binaryExpression.Left)
                && binaryExpression.OperatorToken.GetNextToken().Kind() is SyntaxKind.OpenParenToken or SyntaxKind.OpenBracketToken)
            {
                return true;
            }

            expression = binaryExpression.Left;
        }

        return false;
    }

    /// <summary>
    /// Determine whether removing the parentheses of an argument could join it with neighboring arguments into one
    /// generic name, as <c>M((g &lt; a), (b &gt; (7)))</c> becomes <c>M(g&lt;a, b&gt;(7))</c> once both pairs are gone. Within a
    /// span of an opening argument, type shaped arguments, and a closing argument, the closing argument keeps its pair:
    /// it is not reported whenever an opening argument precedes it, whatever the parentheses of the preceding arguments.
    /// The opening and the continuing arguments are not reported only when the closing argument has no parentheses that
    /// keep the span apart.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if the argument could become part of a generic name</returns>
    private static bool WouldJoinTypeArgumentListAcrossArguments(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        if (parenthesizedExpression.Parent is not ArgumentSyntax argument)
        {
            return false;
        }

        SeparatedSyntaxList<ArgumentSyntax> arguments;

        switch (argument.Parent)
        {
            case BaseArgumentListSyntax argumentList:
                arguments = argumentList.Arguments;
                break;

            case TupleExpressionSyntax tupleExpression:
                arguments = tupleExpression.Arguments;
                break;

            default:
                return false;
        }

        var index = arguments.IndexOf(argument);

        if (CanCloseTypeArgumentList(argument))
        {
            for (var previousIndex = index - 1; previousIndex >= 0; previousIndex--)
            {
                if (CanOpenTypeArgumentList(arguments[previousIndex]))
                {
                    return true;
                }

                if (CanContinueTypeArgumentList(arguments[previousIndex]) == false)
                {
                    break;
                }
            }
        }

        var isOpening = CanOpenTypeArgumentList(argument);

        if (isOpening == false)
        {
            if (CanContinueTypeArgumentList(argument) == false)
            {
                return false;
            }

            for (var previousIndex = index - 1; previousIndex >= 0 && isOpening == false; previousIndex--)
            {
                if (CanOpenTypeArgumentList(arguments[previousIndex]))
                {
                    isOpening = true;
                }
                else if (CanContinueTypeArgumentList(arguments[previousIndex]) == false)
                {
                    return false;
                }
            }

            if (isOpening == false)
            {
                return false;
            }
        }

        for (var nextIndex = index + 1; nextIndex < arguments.Count; nextIndex++)
        {
            if (CanCloseTypeArgumentList(arguments[nextIndex]))
            {
                return arguments[nextIndex].Expression is not ParenthesizedExpressionSyntax;
            }

            if (CanContinueTypeArgumentList(arguments[nextIndex]) == false)
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Determine whether the inner expression is safe in chaining contexts. Nested parentheses that are reported
    /// themselves are looked through, because they are removed as well, and the expression that remains decides. A
    /// nested pair that is not reported stays in place and therefore keeps the chain safe. A nest whose remaining pair
    /// would be read as a cast is kept by <see cref="IsInCastProneNest"/> before this check is reached.
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns><see langword="true"/> if the expression is safe</returns>
    private static bool IsSafeChainExpression(ExpressionSyntax expressionSyntax)
    {
        // An expression needing one pair keeps its outer pair unreported and only the redundant inner pairs are
        // reported. Fix All therefore never removes the last pair
        while (expressionSyntax is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            if (ShouldReport(parenthesizedExpression) == false)
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

        // Parentheses stay whenever the parser would read the code around them differently once they are gone: as a cast,
        // as a local declaration, as a member initializer, or as a generic name. These checks apply to every parent,
        // because the reading depends on the tokens before and after the parentheses rather than on the parent alone
        if (IsInCastProneNest(parenthesizedExpression)
            || WouldStartDeclaration(parenthesizedExpression)
            || WouldStartMemberInitializer(parenthesizedExpression)
            || WouldDetachTypeArgumentList(parenthesizedExpression)
            || WouldJoinTypeArgumentListAcrossArguments(parenthesizedExpression))
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