using System;
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
    /// Get the expression with every pair of parentheses directly around it removed
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns>The unparenthesized expression</returns>
    private static ExpressionSyntax GetUnparenthesizedExpression(ExpressionSyntax expressionSyntax)
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
    /// <param name="onlyReported">
    /// Whether only the parentheses that are reported themselves are left out, as a Fix All
    /// removes them, instead of the parentheses of every parenthesized expression, as the most a sequence of fixes can
    /// remove
    /// </param>
    /// <returns>The remaining tokens</returns>
    private static IEnumerable<SyntaxToken> GetTokensWithoutParentheses(SyntaxNode node, bool onlyReported)
    {
        // Each nested pair is decided once, at its opening parenthesis, so that its closing parenthesis follows that
        // decision without evaluating the pair again
        var removedExpressions = new HashSet<ParenthesizedExpressionSyntax>();

        foreach (var token in node.DescendantTokens())
        {
            if (token.Parent is ParenthesizedExpressionSyntax nestedExpression)
            {
                if (token == nestedExpression.OpenParenToken
                    && (onlyReported == false || ShouldReport(nestedExpression)))
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
    /// Create a reader for the kinds of a token sequence. The sequence is only enumerated as far as the reader is asked,
    /// so that a scan that decides early does not evaluate the rest of the sequence.
    /// </summary>
    /// <param name="tokens">Tokens</param>
    /// <returns>
    /// A function returning the kind of the token at a position, or <see cref="SyntaxKind.None"/> after the last
    /// token
    /// </returns>
    private static Func<int, SyntaxKind> CreateKindReader(IEnumerable<SyntaxToken> tokens)
    {
        var enumerator = tokens.GetEnumerator();
        var kinds = new List<SyntaxKind>();

        return index =>
               {
                   while (kinds.Count <= index
                          && enumerator.MoveNext())
                   {
                       kinds.Add(enumerator.Current.Kind());
                   }

                   return index < kinds.Count
                              ? kinds[index]
                              : SyntaxKind.None;
               };
    }

    /// <summary>
    /// Scan a type at a position of a token list, the way the parser scans a type when it decides between a type and an
    /// expression: a name, a predefined type, or a tuple of at least two types
    /// </summary>
    /// <param name="kindAt">Reader for the token kinds</param>
    /// <param name="index">Position of the first token of the type</param>
    /// <returns>The position after the type, or <c>-1</c> if the tokens do not start with a type</returns>
    private static int ScanType(Func<int, SyntaxKind> kindAt, int index)
    {
        if (SyntaxFacts.IsPredefinedType(kindAt(index)))
        {
            return index + 1;
        }

        if (kindAt(index) != SyntaxKind.OpenParenToken)
        {
            return ScanName(kindAt, index);
        }

        var elementCount = 0;

        do
        {
            index = ScanType(kindAt, index + 1);

            if (index < 0)
            {
                return -1;
            }

            elementCount++;
        }
        while (kindAt(index) == SyntaxKind.CommaToken);

        return elementCount > 1 && kindAt(index) == SyntaxKind.CloseParenToken
                   ? index + 1
                   : -1;
    }

    /// <summary>
    /// Scan a name at a position of a token list: an optional alias qualifier such as <c>global::</c>, followed by
    /// identifiers separated by dots, each optionally followed by a type argument list
    /// </summary>
    /// <param name="kindAt">Reader for the token kinds</param>
    /// <param name="index">Position of the first token of the name</param>
    /// <returns>The position after the name, or <c>-1</c> if the tokens do not start with a name</returns>
    private static int ScanName(Func<int, SyntaxKind> kindAt, int index)
    {
        if (kindAt(index) is SyntaxKind.IdentifierToken or SyntaxKind.GlobalKeyword
            && kindAt(index + 1) == SyntaxKind.ColonColonToken)
        {
            index += 2;
        }

        while (true)
        {
            if (kindAt(index) != SyntaxKind.IdentifierToken)
            {
                return -1;
            }

            index++;

            if (kindAt(index) == SyntaxKind.LessThanToken)
            {
                index = ScanTypeArgumentList(kindAt, index);

                if (index < 0)
                {
                    return -1;
                }
            }

            if (kindAt(index) != SyntaxKind.DotToken)
            {
                return index;
            }

            index++;
        }
    }

    /// <summary>
    /// Scan a type argument list at a position of a token list. A type argument may carry nullable, pointer, and array
    /// suffixes.
    /// </summary>
    /// <param name="kindAt">Reader for the token kinds</param>
    /// <param name="index">Position of the <c>&lt;</c> token</param>
    /// <returns>The position after the closing <c>&gt;</c>, or <c>-1</c> if no type argument list starts there</returns>
    private static int ScanTypeArgumentList(Func<int, SyntaxKind> kindAt, int index)
    {
        do
        {
            index = ScanType(kindAt, index + 1);

            if (index < 0)
            {
                return -1;
            }

            while (kindAt(index) is SyntaxKind.QuestionToken or SyntaxKind.AsteriskToken or SyntaxKind.OpenBracketToken)
            {
                if (kindAt(index) == SyntaxKind.OpenBracketToken)
                {
                    index++;

                    while (kindAt(index) == SyntaxKind.CommaToken)
                    {
                        index++;
                    }

                    if (kindAt(index) != SyntaxKind.CloseBracketToken)
                    {
                        return -1;
                    }
                }

                index++;
            }
        }
        while (kindAt(index) == SyntaxKind.CommaToken);

        return kindAt(index) == SyntaxKind.GreaterThanToken
                   ? index + 1
                   : -1;
    }

    /// <summary>
    /// Determine whether a node could become a type once a sequence of fixes has removed every pair of parentheses inside
    /// it, as the parser reads a type wherever it decides between a type and an expression
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node could become a type</returns>
    private static bool IsTypeShaped(SyntaxNode node)
    {
        var kindAt = CreateKindReader(GetTokensWithoutParentheses(node, false));
        var index = ScanType(kindAt, 0);

        return index >= 0
               && kindAt(index) == SyntaxKind.None;
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
                    var tokens = GetTokensWithoutParentheses(parenthesizedExpression.Expression, false).ToList();

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
                || IsTypeShaped(outermostExpression.Expression) == false)
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
    /// <returns><see langword="true"/> if a declaration would be read</returns>
    private static bool WouldStartDeclaration(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        if (OpensDeclarationPosition(parenthesizedExpression, out var isTupleElement, out var lastToken) == false)
        {
            return false;
        }

        var kindAt = CreateKindReader(GetTokensWithoutParentheses(parenthesizedExpression.Expression, true).Concat(GetFollowingTokens(parenthesizedExpression.CloseParenToken, lastToken)));
        var index = ScanName(kindAt, 0);

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

        var remainingTokens = GetTokensWithoutParentheses(parenthesizedExpression.Expression, true).Take(2).ToList();

        return remainingTokens.Count > 0
               && (remainingTokens[0].IsKind(SyntaxKind.OpenBracketToken)
                   || (remainingTokens.Count == 1 && remainingTokens[0].IsKind(SyntaxKind.IdentifierToken)));
    }

    /// <summary>
    /// Determine whether removing the parentheses would separate a type argument list from the token that keeps it a
    /// type argument list. A type argument list is only read as one when the token after its <c>&gt;</c> is one the parser
    /// accepts there, as listed by <see cref="IsTypeArgumentListFollower"/>, or when one of its direct type arguments is
    /// a predefined, nullable, array, or pointer type; otherwise the angle brackets are read as relational operators.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if the type argument list would lose its following token</returns>
    private static bool WouldDetachTypeArgumentList(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        if (IsTypeArgumentListFollower(parenthesizedExpression.CloseParenToken.GetNextToken()))
        {
            return false;
        }

        var lastToken = parenthesizedExpression.Expression.GetLastToken();

        while (lastToken.Parent is ParenthesizedExpressionSyntax nestedExpression
               && lastToken == nestedExpression.CloseParenToken
               && ShouldReport(nestedExpression))
        {
            lastToken = lastToken.GetPreviousToken();
        }

        return lastToken.IsKind(SyntaxKind.GreaterThanToken)
               && lastToken.Parent is TypeArgumentListSyntax typeArgumentList
               && typeArgumentList.Arguments.Any(static typeArgument => typeArgument is PredefinedTypeSyntax
                                                                                     or NullableTypeSyntax
                                                                                     or ArrayTypeSyntax
                                                                                     or PointerTypeSyntax) == false;
    }

    /// <summary>
    /// Determine whether the token keeps a preceding type argument list a type argument list. Besides the tokens the
    /// language specification lists, the parser accepts the relational operators, <see langword="is"/>,
    /// <see langword="as"/>, an opening brace, and <c>=&gt;</c>.
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
                            or SyntaxKind.OpenBracketToken
                            or SyntaxKind.LessThanToken
                            or SyntaxKind.LessThanEqualsToken
                            or SyntaxKind.GreaterThanEqualsToken
                            or SyntaxKind.IsKeyword
                            or SyntaxKind.AsKeyword
                            or SyntaxKind.OpenBraceToken
                            or SyntaxKind.EqualsGreaterThanToken;
    }

    /// <summary>
    /// Get the right operand an expression ends in, for expressions whose last part is an operand that a following token
    /// can continue
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns>The trailing operand, or <see langword="null"/> if the expression ends in a closing token</returns>
    private static ExpressionSyntax GetTrailingOperand(ExpressionSyntax expressionSyntax)
    {
        return expressionSyntax switch
               {
                   BinaryExpressionSyntax binaryExpression => binaryExpression.Right,
                   ConditionalExpressionSyntax conditionalExpression => conditionalExpression.WhenFalse,
                   AssignmentExpressionSyntax assignmentExpression => assignmentExpression.Right,
                   PrefixUnaryExpressionSyntax prefixUnaryExpression => prefixUnaryExpression.Operand,
                   CastExpressionSyntax castExpression => castExpression.Expression,
                   AwaitExpressionSyntax awaitExpression => awaitExpression.Expression,
                   ThrowExpressionSyntax throwExpression => throwExpression.Expression,
                   RefExpressionSyntax refExpression => refExpression.Expression,
                   RangeExpressionSyntax rangeExpression => rangeExpression.RightOperand,
                   LambdaExpressionSyntax lambdaExpression => lambdaExpression.ExpressionBody,
                   _ => null
               };
    }

    /// <summary>
    /// Get the left operand an expression starts with, for expressions whose first part is an operand that a preceding
    /// token can continue
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns>The leading operand, or <see langword="null"/> if the expression starts with an opening token</returns>
    private static ExpressionSyntax GetLeadingOperand(ExpressionSyntax expressionSyntax)
    {
        return expressionSyntax switch
               {
                   BinaryExpressionSyntax binaryExpression => binaryExpression.Left,
                   ConditionalExpressionSyntax conditionalExpression => conditionalExpression.Condition,
                   AssignmentExpressionSyntax assignmentExpression => assignmentExpression.Left,
                   PostfixUnaryExpressionSyntax postfixUnaryExpression => postfixUnaryExpression.Operand,
                   IsPatternExpressionSyntax isPatternExpression => isPatternExpression.Expression,
                   SwitchExpressionSyntax switchExpression => switchExpression.GoverningExpression,
                   WithExpressionSyntax withExpression => withExpression.Expression,
                   RangeExpressionSyntax rangeExpression => rangeExpression.LeftOperand,
                   _ => null
               };
    }

    /// <summary>
    /// Determine whether the argument, once its parentheses are gone, ends in <c>name &lt; type</c>, so that it can open a
    /// type argument list that a following argument closes
    /// </summary>
    /// <param name="argument">Argument</param>
    /// <param name="opensDeclaration">
    /// Whether the name stands where the parser also reads a declaration, so that an
    /// identifier following the type argument list is read as the declared variable
    /// </param>
    /// <returns><see langword="true"/> if the argument can open a type argument list</returns>
    private static bool CanOpenTypeArgumentList(ArgumentSyntax argument, out bool opensDeclaration)
    {
        var unparenthesizedExpression = GetUnparenthesizedExpression(argument.Expression);

        for (var expression = unparenthesizedExpression; expression != null; expression = GetTrailingOperand(expression))
        {
            if (expression is BinaryExpressionSyntax binaryExpression
                && binaryExpression.IsKind(SyntaxKind.LessThanExpression)
                && binaryExpression.Left.GetLastToken().IsKind(SyntaxKind.IdentifierToken)
                && IsTypeShaped(binaryExpression.Right))
            {
                opensDeclaration = StandsAtDeclarationStart(argument, unparenthesizedExpression, binaryExpression.Left.GetLastToken());

                return true;
            }
        }

        opensDeclaration = false;

        return false;
    }

    /// <summary>
    /// Determine whether the name ending in the given identifier stands where the parser reads a declaration when a type
    /// is followed by an identifier: after <see langword="is"/> or a pattern combinator, after <see langword="out"/>, or
    /// at the start of a tuple element
    /// </summary>
    /// <param name="argument">Argument containing the name</param>
    /// <param name="unparenthesizedExpression">Expression of the argument without its parentheses</param>
    /// <param name="lastNameToken">Last identifier of the name</param>
    /// <returns><see langword="true"/> if the name stands at the start of a declaration</returns>
    private static bool StandsAtDeclarationStart(ArgumentSyntax argument, ExpressionSyntax unparenthesizedExpression, SyntaxToken lastNameToken)
    {
        var firstNameToken = lastNameToken;
        var previousToken = firstNameToken.GetPreviousToken();

        while (previousToken.Kind() is SyntaxKind.DotToken or SyntaxKind.ColonColonToken
               && previousToken.GetPreviousToken().Kind() is SyntaxKind.IdentifierToken or SyntaxKind.GlobalKeyword)
        {
            firstNameToken = previousToken.GetPreviousToken();
            previousToken = firstNameToken.GetPreviousToken();
        }

        if (previousToken.Kind() is SyntaxKind.IsKeyword or SyntaxKind.OutKeyword or SyntaxKind.NotKeyword or SyntaxKind.AndKeyword or SyntaxKind.OrKeyword)
        {
            return true;
        }

        return firstNameToken == unparenthesizedExpression.GetFirstToken()
               && (argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword) || argument.Parent is TupleExpressionSyntax);
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
               && IsTypeShaped(argument.Expression);
    }

    /// <summary>
    /// Determine whether the argument, once its parentheses are gone, starts with <c>type &gt;</c> followed by a token
    /// that both keeps a type argument list and starts an operand, or by an identifier where the opening argument starts
    /// a declaration, so that it closes a type argument list that a preceding argument opened
    /// </summary>
    /// <param name="argument">Argument</param>
    /// <param name="closesDeclaration">
    /// Whether the opening argument stands where the parser reads a declaration, so that an
    /// identifier also closes the type argument list
    /// </param>
    /// <returns><see langword="true"/> if the argument can close a type argument list</returns>
    private static bool CanCloseTypeArgumentList(ArgumentSyntax argument, bool closesDeclaration)
    {
        if (argument.NameColon != null
            || argument.RefKindKeyword.IsKind(SyntaxKind.None) == false)
        {
            return false;
        }

        for (var expression = GetUnparenthesizedExpression(argument.Expression); expression != null; expression = GetLeadingOperand(expression))
        {
            if (expression is BinaryExpressionSyntax binaryExpression
                && binaryExpression.IsKind(SyntaxKind.GreaterThanExpression)
                && IsTypeShaped(binaryExpression.Left))
            {
                var nextToken = binaryExpression.OperatorToken.GetNextToken();

                if ((IsTypeArgumentListFollower(nextToken)
                     && (SyntaxFacts.IsPrefixUnaryExpressionOperatorToken(nextToken.Kind())
                         || nextToken.Kind() is SyntaxKind.OpenParenToken or SyntaxKind.OpenBracketToken))
                    || (closesDeclaration && nextToken.IsKind(SyntaxKind.IdentifierToken)))
                {
                    return true;
                }
            }
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

        for (var previousIndex = index - 1; previousIndex >= 0; previousIndex--)
        {
            if (CanOpenTypeArgumentList(arguments[previousIndex], out var opensPrecedingDeclaration))
            {
                if (CanCloseTypeArgumentList(argument, opensPrecedingDeclaration))
                {
                    return true;
                }

                break;
            }

            if (CanContinueTypeArgumentList(arguments[previousIndex]) == false)
            {
                break;
            }
        }

        var isOpening = CanOpenTypeArgumentList(argument, out var opensDeclaration);

        if (isOpening == false)
        {
            if (CanContinueTypeArgumentList(argument) == false)
            {
                return false;
            }

            for (var previousIndex = index - 1; previousIndex >= 0 && isOpening == false; previousIndex--)
            {
                if (CanOpenTypeArgumentList(arguments[previousIndex], out opensDeclaration))
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
            if (CanCloseTypeArgumentList(arguments[nextIndex], opensDeclaration))
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
    /// would be read as a cast is kept by <see cref="WouldLeaveCast"/> before this check is reached.
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
        if (WouldLeaveCast(parenthesizedExpression)
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