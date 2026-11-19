using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Guards of <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> that keep parentheses whose removal
/// would make the parser read a type: a cast, or a type argument list that detaches from its name or joins across
/// arguments; and the scanner that reads whether a token sequence has the shape of a type
/// </summary>
internal static class UnnecessaryParenthesesTypeArgumentListUtilities
{
    #region Methods

    /// <summary>
    /// Create a reader for the kinds of a token sequence. The sequence is only enumerated as far as the reader is asked,
    /// so that a scan that decides early does not evaluate the rest of the sequence.
    /// </summary>
    /// <param name="tokens">Tokens</param>
    /// <returns>
    /// A function returning the kind of the token at a position, or <see cref="SyntaxKind.None"/> after the last
    /// token
    /// </returns>
    internal static Func<int, SyntaxKind> CreateKindReader(IEnumerable<SyntaxToken> tokens)
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
    /// Scan a name at a position of a token list: an optional alias qualifier such as <c>global::</c>, followed by
    /// identifiers separated by dots, each optionally followed by a type argument list
    /// </summary>
    /// <param name="kindAt">Reader for the token kinds</param>
    /// <param name="index">Position of the first token of the name</param>
    /// <returns>The position after the name, or <c>-1</c> if the tokens do not start with a name</returns>
    internal static int ScanName(Func<int, SyntaxKind> kindAt, int index)
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
    /// Determine whether a node could become a type once a sequence of fixes has removed every pair of parentheses inside
    /// it, as the parser reads a type wherever it decides between a type and an expression
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node could become a type</returns>
    internal static bool IsTypeShaped(SyntaxNode node)
    {
        var kindAt = CreateKindReader(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.GetTokensWithoutParentheses(node, null));
        var index = ScanType(kindAt, 0);

        return index >= 0
               && kindAt(index) == SyntaxKind.None;
    }

    /// <summary>
    /// Determine whether removing the parentheses would separate a type argument list from the token that keeps it a
    /// type argument list. A type argument list is only read as one when the token after its <c>&gt;</c> is one the parser
    /// accepts there, as listed by <see cref="IsTypeArgumentListFollower"/>, or when one of its direct type arguments is
    /// a predefined, nullable, or array type; otherwise the angle brackets are read as relational operators.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <param name="decisions">Decisions already taken for parenthesized expressions during the current evaluation</param>
    /// <returns><see langword="true"/> if the type argument list would lose its following token</returns>
    internal static bool WouldDetachTypeArgumentList(ParenthesizedExpressionSyntax parenthesizedExpression, Dictionary<ParenthesizedExpressionSyntax, bool> decisions)
    {
        if (IsTypeArgumentListFollower(parenthesizedExpression.CloseParenToken.GetNextToken()))
        {
            return false;
        }

        var lastToken = parenthesizedExpression.Expression.GetLastToken();

        while (lastToken.Parent is ParenthesizedExpressionSyntax nestedExpression
               && lastToken == nestedExpression.CloseParenToken
               && RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.ShouldReport(nestedExpression, decisions))
        {
            lastToken = lastToken.GetPreviousToken();
        }

        return lastToken.IsKind(SyntaxKind.GreaterThanToken)
               && lastToken.Parent is TypeArgumentListSyntax typeArgumentList
               && typeArgumentList.Arguments.Any(static typeArgument => typeArgument is PredefinedTypeSyntax
                                                                                     or NullableTypeSyntax
                                                                                     or ArrayTypeSyntax) == false;
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
    internal static bool WouldJoinTypeArgumentListAcrossArguments(ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        if (parenthesizedExpression.Parent is not ArgumentSyntax argument
            || GetArguments(argument) is not { } arguments)
        {
            return false;
        }

        var index = arguments.IndexOf(argument);

        if (ClosesPrecedingTypeArgumentList(arguments, index))
        {
            return true;
        }

        var isOpening = CanOpenTypeArgumentList(argument, out var declarationEnd);

        if (isOpening == false)
        {
            if (CanContinueTypeArgumentList(argument) == false)
            {
                return false;
            }

            for (var previousIndex = index - 1; previousIndex >= 0 && isOpening == false; previousIndex--)
            {
                if (CanOpenTypeArgumentList(arguments[previousIndex], out declarationEnd))
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
            if (CanCloseTypeArgumentList(arguments[nextIndex], declarationEnd))
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
    /// Get the arguments of the argument list or the tuple an argument belongs to
    /// </summary>
    /// <param name="argument">Argument</param>
    /// <returns>The arguments, or <see langword="null"/> when the argument belongs to neither</returns>
    private static SeparatedSyntaxList<ArgumentSyntax>? GetArguments(ArgumentSyntax argument)
    {
        return argument.Parent switch
               {
                   BaseArgumentListSyntax argumentList => argumentList.Arguments,
                   TupleExpressionSyntax tupleExpression => tupleExpression.Arguments,
                   _ => null
               };
    }

    /// <summary>
    /// Determine whether an argument can close a span that an opening argument in front of it starts, with only type
    /// shaped arguments between them
    /// </summary>
    /// <param name="arguments">Arguments of the list</param>
    /// <param name="index">Position of the argument</param>
    /// <returns><see langword="true"/> if the argument can close such a span</returns>
    private static bool ClosesPrecedingTypeArgumentList(SeparatedSyntaxList<ArgumentSyntax> arguments, int index)
    {
        for (var previousIndex = index - 1; previousIndex >= 0; previousIndex--)
        {
            if (CanOpenTypeArgumentList(arguments[previousIndex], out var precedingDeclarationEnd))
            {
                return CanCloseTypeArgumentList(arguments[index], precedingDeclarationEnd);
            }

            if (CanContinueTypeArgumentList(arguments[previousIndex]) == false)
            {
                return false;
            }
        }

        return false;
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
    /// <param name="declarationEnd">
    /// When the name stands where the parser also reads a declaration, a predicate for the token kind that may follow an
    /// identifier after the type argument list so that the identifier is read as the declared variable; otherwise
    /// <see langword="null"/>
    /// </param>
    /// <returns><see langword="true"/> if the argument can open a type argument list</returns>
    private static bool CanOpenTypeArgumentList(ArgumentSyntax argument, out Func<SyntaxKind, bool> declarationEnd)
    {
        var unparenthesizedExpression = RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.GetUnparenthesizedExpression(argument.Expression);

        for (var expression = unparenthesizedExpression; expression != null; expression = GetTrailingOperand(expression))
        {
            if (expression is BinaryExpressionSyntax binaryExpression
                && binaryExpression.IsKind(SyntaxKind.LessThanExpression)
                && binaryExpression.Left.GetLastToken().IsKind(SyntaxKind.IdentifierToken)
                && IsTypeShaped(binaryExpression.Right))
            {
                declarationEnd = GetDeclarationEnd(argument, unparenthesizedExpression, binaryExpression.Left.GetLastToken());

                return true;
            }
        }

        declarationEnd = null;

        return false;
    }

    /// <summary>
    /// Determine whether the name ending in the given identifier could be read as a type and stands where the parser
    /// reads a declaration when a type is followed by an identifier, and which token has to follow that identifier there: after <see langword="is"/> or
    /// a pattern combinator and after <see langword="out"/>, the identifier alone is enough; at the start of a tuple
    /// element, it has to end the element, and the first element of a tuple has to be followed by another one.
    /// </summary>
    /// <param name="argument">Argument containing the name</param>
    /// <param name="unparenthesizedExpression">Expression of the argument without its parentheses</param>
    /// <param name="lastNameToken">Last identifier of the name</param>
    /// <returns>
    /// A predicate for the token kind that may follow the declared identifier, or <see langword="null"/> if the
    /// name does not stand at the start of a declaration
    /// </returns>
    private static Func<SyntaxKind, bool> GetDeclarationEnd(ArgumentSyntax argument, ExpressionSyntax unparenthesizedExpression, SyntaxToken lastNameToken)
    {
        var name = lastNameToken.Parent;

        while ((name?.Parent is MemberAccessExpressionSyntax memberAccessExpression && memberAccessExpression.Name == name)
               || (name?.Parent is QualifiedNameSyntax qualifiedName && qualifiedName.Right == name)
               || (name?.Parent is AliasQualifiedNameSyntax aliasQualifiedName && aliasQualifiedName.Name == name))
        {
            name = name.Parent;
        }

        // Only a name that could be read as a type starts a declaration; a qualifier such as this, an invocation, or an
        // element access keeps the angle brackets relational operators
        if (name == null
            || IsTypeShaped(name) == false)
        {
            return null;
        }

        var firstNameToken = name.GetFirstToken();

        if (firstNameToken.GetPreviousToken().Kind() is SyntaxKind.IsKeyword or SyntaxKind.OutKeyword or SyntaxKind.NotKeyword or SyntaxKind.AndKeyword or SyntaxKind.OrKeyword)
        {
            return static _ => true;
        }

        if (firstNameToken != unparenthesizedExpression.GetFirstToken())
        {
            return null;
        }

        if (argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword))
        {
            return static _ => true;
        }

        if (argument.Parent is TupleExpressionSyntax tupleExpression)
        {
            return tupleExpression.Arguments[0] == argument
                       ? static kind => kind == SyntaxKind.CommaToken
                       : static kind => kind is SyntaxKind.CommaToken or SyntaxKind.CloseParenToken;
        }

        return null;
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
    /// Get the token following a token once the parentheses ending right after it are gone
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>The first following token that is not the closing parenthesis of a parenthesized expression</returns>
    private static SyntaxToken GetTokenAfterParentheses(SyntaxToken token)
    {
        var nextToken = token.GetNextToken();

        while (nextToken.IsKind(SyntaxKind.CloseParenToken)
               && nextToken.Parent is ParenthesizedExpressionSyntax)
        {
            nextToken = nextToken.GetNextToken();
        }

        return nextToken;
    }

    /// <summary>
    /// Determine whether the argument, once its parentheses are gone, starts with <c>type &gt;</c> followed by a token
    /// that both keeps a type argument list and starts an operand, or by an identifier that ends a declaration the
    /// opening argument starts, so that it closes a type argument list that a preceding argument opened
    /// </summary>
    /// <param name="argument">Argument</param>
    /// <param name="declarationEnd">
    /// The predicate the opening argument provides when it stands where the parser reads a declaration, so that an
    /// identifier followed by an accepted token also closes the type argument list; otherwise <see langword="null"/>
    /// </param>
    /// <returns><see langword="true"/> if the argument can close a type argument list</returns>
    private static bool CanCloseTypeArgumentList(ArgumentSyntax argument, Func<SyntaxKind, bool> declarationEnd)
    {
        if (argument.NameColon != null
            || argument.RefKindKeyword.IsKind(SyntaxKind.None) == false)
        {
            return false;
        }

        for (var expression = RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.GetUnparenthesizedExpression(argument.Expression); expression != null; expression = GetLeadingOperand(expression))
        {
            if (expression is BinaryExpressionSyntax binaryExpression
                && binaryExpression.IsKind(SyntaxKind.GreaterThanExpression)
                && IsTypeShaped(binaryExpression.Left))
            {
                var nextToken = binaryExpression.OperatorToken.GetNextToken();

                if ((IsTypeArgumentListFollower(nextToken)
                     && (SyntaxFacts.IsPrefixUnaryExpressionOperatorToken(nextToken.Kind())
                         || nextToken.Kind() is SyntaxKind.OpenParenToken or SyntaxKind.OpenBracketToken))
                    || (declarationEnd != null
                        && nextToken.IsKind(SyntaxKind.IdentifierToken)
                        && declarationEnd(GetTokenAfterParentheses(nextToken).Kind())))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion // Methods
}