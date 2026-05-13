using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Normalizes horizontal spacing between tokens on the same line.
/// Handles operator spacing, comma spacing, semicolons in for-loops,
/// keyword spacing, parenthesis spacing, and multiple consecutive spaces
/// </summary>
internal static class HorizontalSpacingPhase
{
    #region Fields

    /// <summary>
    /// A single space whitespace trivia
    /// </summary>
    private static readonly SyntaxTrivia _singleSpace = SyntaxFactory.Whitespace(" ");

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Applies horizontal spacing rules to the given syntax tree
    /// </summary>
    /// <param name="root">The root syntax node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The syntax tree with normalized horizontal spacing</returns>
    public static SyntaxNode Execute(SyntaxNode root, CancellationToken cancellationToken)
    {
        var rewriter = new HorizontalSpacingRewriter(cancellationToken);

        return rewriter.Visit(root);
    }

    /// <summary>
    /// Determines whether two adjacent tokens are separated by an end-of-line trivia,
    /// meaning they are on different lines and spacing should not be adjusted
    /// </summary>
    /// <param name="token">The first token</param>
    /// <param name="nextToken">The second token</param>
    /// <returns><see langword="true"/> if the tokens are on different lines; otherwise, <see langword="false"/></returns>
    internal static bool AreSeparatedByEndOfLine(SyntaxToken token, SyntaxToken nextToken)
    {
        return token.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
               || nextToken.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    /// <summary>
    /// Determines the desired number of spaces after the current token and before the next token,
    /// based on horizontal spacing rules. Returns <see langword="null"/> if no specific
    /// rule applies, in which case only the collapse-multiple-spaces rule is used
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token on the same line</param>
    /// <returns>The desired space count, or <see langword="null"/> if only the collapse-multiple-spaces rule applies</returns>
    internal static int? GetDesiredSpacesAfter(SyntaxToken current, SyntaxToken next)
    {
        if (HasNoSpaceAfter(current, next))
        {
            return 0;
        }

        if (current.IsKind(SyntaxKind.CommaToken))
        {
            return GetSpacesAfterComma(current);
        }

        if (RequiresSingleSpace(current, next))
        {
            return 1;
        }

        if (IsBinaryOrAssignmentOperator(current) || IsBinaryOrAssignmentOperator(next))
        {
            return 1;
        }

        // Semicolons in for-loop headers — exactly one space after
        if (current.IsKind(SyntaxKind.SemicolonToken) && current.Parent is ForStatementSyntax)
        {
            return 1;
        }

        return GetKeywordSpacing(current, next);
    }

    /// <summary>
    /// Determines whether spacing must be removed between the two tokens
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token</param>
    /// <returns><see langword="true"/> if no separating space is allowed; otherwise, <see langword="false"/></returns>
    private static bool HasNoSpaceAfter(SyntaxToken current, SyntaxToken next)
    {
        return current.IsKind(SyntaxKind.OpenParenToken)
               || current.IsKind(SyntaxKind.OpenBracketToken)
               || next.IsKind(SyntaxKind.CloseParenToken)
               || next.IsKind(SyntaxKind.CloseBracketToken)
               || next.IsKind(SyntaxKind.CommaToken)
               || next.IsKind(SyntaxKind.SemicolonToken)
               || IsGenericOpeningAngleBracket(current)
               || IsGenericOpeningAngleBracket(next)
               || IsGenericClosingAngleBracket(next)
               || IsNullableTypeQuestionToken(next)
               || IsMemberAccessToken(current)
               || IsMemberAccessToken(next)
               || IsUnarySignToken(current)
               || IsPrefixTightUnaryOperatorToken(current)
               || IsPostfixTightUnaryOperatorToken(next);
    }

    /// <summary>
    /// Determines whether spacing must be normalized to a single space between the two tokens
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token</param>
    /// <returns><see langword="true"/> if the tokens must be separated by a single space; otherwise, <see langword="false"/></returns>
    private static bool RequiresSingleSpace(SyntaxToken current, SyntaxToken next)
    {
        return RequiresSpaceBeforeBrace(next)
               || RequiresSpaceBeforeColon(current, next);
    }

    /// <summary>
    /// Determines the spacing after a comma token
    /// </summary>
    /// <param name="current">The comma token</param>
    /// <returns>The required number of spaces after the comma</returns>
    private static int GetSpacesAfterComma(SyntaxToken current)
    {
        if (current.Parent is ArrayRankSpecifierSyntax arrayRankSpecifier
            && IsRankOnlyArraySpecifier(arrayRankSpecifier))
        {
            return 0;
        }

        return 1;
    }

    /// <summary>
    /// Determines spacing for keyword tokens
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token</param>
    /// <returns>The required space count, or <see langword="null"/> if no keyword-specific rule applies</returns>
    private static int? GetKeywordSpacing(SyntaxToken current, SyntaxToken next)
    {
        if (IsSpacedKeyword(current) == false)
        {
            // No specific rule — return null so only collapse of multiple spaces is applied.
            return null;
        }

        // Special case: return and throw statements have no space before the semicolon.
        if ((current.IsKind(SyntaxKind.ReturnKeyword) || current.IsKind(SyntaxKind.ThrowKeyword))
            && next.IsKind(SyntaxKind.SemicolonToken))
        {
            return 0;
        }

        if (current.IsKind(SyntaxKind.NewKeyword))
        {
            return GetNewKeywordSpacing(current, next);
        }

        return 1;
    }

    /// <summary>
    /// Determines spacing after the <c>new</c> keyword
    /// </summary>
    /// <param name="current">The <c>new</c> keyword token</param>
    /// <param name="next">The next token</param>
    /// <returns>The required number of spaces</returns>
    private static int GetNewKeywordSpacing(SyntaxToken current, SyntaxToken next)
    {
        // target-typed new() and constructor constraint new() keep parentheses adjacent.
        if (next.IsKind(SyntaxKind.OpenParenToken)
            && (current.Parent is ImplicitObjectCreationExpressionSyntax
                || current.Parent is ConstructorConstraintSyntax))
        {
            return 0;
        }

        // new[] keeps brackets adjacent.
        if (next.IsKind(SyntaxKind.OpenBracketToken)
            && current.Parent is ImplicitArrayCreationExpressionSyntax)
        {
            return 0;
        }

        return 1;
    }

    /// <summary>
    /// Determines whether the specified token is a binary or assignment operator
    /// based on its parent syntax node
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns><see langword="true"/> if the token is a binary or assignment operator; otherwise, <see langword="false"/></returns>
    private static bool IsBinaryOrAssignmentOperator(SyntaxToken token)
    {
        return token.Parent is BinaryExpressionSyntax
               || token.Parent is AssignmentExpressionSyntax
               || (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is EqualsValueClauseSyntax)
               || (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is NameEqualsSyntax);
    }

    /// <summary>
    /// Determines whether an array rank specifier only declares the rank and does not contain size expressions
    /// </summary>
    /// <param name="arrayRankSpecifier">The array rank specifier to inspect</param>
    /// <returns><see langword="true"/> if all entries are omitted size expressions; otherwise, <see langword="false"/></returns>
    private static bool IsRankOnlyArraySpecifier(ArrayRankSpecifierSyntax arrayRankSpecifier)
    {
        return arrayRankSpecifier.Sizes.All(static size => size.IsKind(SyntaxKind.OmittedArraySizeExpression));
    }

    /// <summary>
    /// Determines whether the token is a generic opening angle bracket
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token opens a generic argument or parameter list; otherwise, <see langword="false"/></returns>
    private static bool IsGenericOpeningAngleBracket(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.LessThanToken)
               && (token.Parent is TypeArgumentListSyntax || token.Parent is TypeParameterListSyntax);
    }

    /// <summary>
    /// Determines whether the token is a generic closing angle bracket
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token closes a generic argument or parameter list; otherwise, <see langword="false"/></returns>
    private static bool IsGenericClosingAngleBracket(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.GreaterThanToken)
               && (token.Parent is TypeArgumentListSyntax || token.Parent is TypeParameterListSyntax);
    }

    /// <summary>
    /// Determines whether the token is the nullable type question mark
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is part of a nullable type; otherwise, <see langword="false"/></returns>
    private static bool IsNullableTypeQuestionToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.QuestionToken) && token.Parent is NullableTypeSyntax;
    }

    /// <summary>
    /// Determines whether the token is a member-access dot token
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is a member-access dot; otherwise, <see langword="false"/></returns>
    private static bool IsMemberAccessToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.DotToken)
               && (token.Parent is MemberAccessExpressionSyntax
                   || token.Parent is QualifiedNameSyntax
                   || token.Parent is AliasQualifiedNameSyntax);
    }

    /// <summary>
    /// Determines whether the token is a unary plus or minus sign
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is a unary sign; otherwise, <see langword="false"/></returns>
    private static bool IsUnarySignToken(SyntaxToken token)
    {
        return (token.IsKind(SyntaxKind.PlusToken) || token.IsKind(SyntaxKind.MinusToken))
               && token.Parent is PrefixUnaryExpressionSyntax;
    }

    /// <summary>
    /// Determines whether the token is a prefix unary operator that must stay adjacent to its operand
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token belongs to a prefix tight unary operator; otherwise, <see langword="false"/></returns>
    private static bool IsPrefixTightUnaryOperatorToken(SyntaxToken token)
    {
        return token.Parent is PrefixUnaryExpressionSyntax;
    }

    /// <summary>
    /// Determines whether the token is a postfix unary operator that must stay adjacent to its operand
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token belongs to a postfix tight unary operator; otherwise, <see langword="false"/></returns>
    private static bool IsPostfixTightUnaryOperatorToken(SyntaxToken token)
    {
        return token.Parent is PostfixUnaryExpressionSyntax;
    }

    /// <summary>
    /// Determines whether a single space is required before the specified brace token
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a single space is required before the brace token; otherwise, <see langword="false"/></returns>
    private static bool RequiresSpaceBeforeBrace(SyntaxToken token)
    {
        if ((token.IsKind(SyntaxKind.OpenBraceToken) || token.IsKind(SyntaxKind.CloseBraceToken)) == false)
        {
            return false;
        }

        return token.Parent is not InterpolationSyntax;
    }

    /// <summary>
    /// Determines whether a single space is required around the specified colon token
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token</param>
    /// <returns><see langword="true"/> if the colon should be surrounded by single spaces; otherwise, <see langword="false"/></returns>
    private static bool RequiresSpaceBeforeColon(SyntaxToken current, SyntaxToken next)
    {
        return IsSpacedColonToken(current) || IsSpacedColonToken(next);
    }

    /// <summary>
    /// Determines whether the token is a colon that should be surrounded by spaces
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is a spaced colon; otherwise, <see langword="false"/></returns>
    private static bool IsSpacedColonToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.ColonToken)
               && (token.Parent is BaseListSyntax || token.Parent is ConstructorInitializerSyntax);
    }

    /// <summary>
    /// Determines whether the token is a keyword that should be followed by exactly one space
    /// before its parenthesis or expression
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns><see langword="true"/> if the token is a spaced keyword; otherwise, <see langword="false"/></returns>
    private static bool IsSpacedKeyword(SyntaxToken token)
    {
        switch (token.Kind())
        {
            case SyntaxKind.IfKeyword:
            case SyntaxKind.ForKeyword:
            case SyntaxKind.ForEachKeyword:
            case SyntaxKind.WhileKeyword:
            case SyntaxKind.SwitchKeyword:
            case SyntaxKind.CatchKeyword:
            case SyntaxKind.UsingKeyword:
            case SyntaxKind.LockKeyword:
            case SyntaxKind.ReturnKeyword:
            case SyntaxKind.ThrowKeyword:
            case SyntaxKind.NewKeyword:
            case SyntaxKind.OperatorKeyword:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Sets the trailing whitespace of a token to the specified number of spaces.
    /// Preserves non-whitespace trivia (such as inline comments)
    /// </summary>
    /// <param name="token">The token whose trailing whitespace to normalize</param>
    /// <param name="desiredSpaces">The desired number of trailing spaces</param>
    /// <returns>The token with adjusted trailing whitespace</returns>
    internal static SyntaxToken SetTrailingWhitespace(SyntaxToken token, int desiredSpaces)
    {
        var trailing = token.TrailingTrivia;

        if (trailing.Count == 0)
        {
            if (desiredSpaces == 0)
            {
                return token;
            }

            return token.WithTrailingTrivia(SyntaxFactory.Whitespace(new string(' ', desiredSpaces)));
        }

        // Check if trailing trivia is only whitespace
        var allWhitespace = trailing.All(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia));

        if (allWhitespace)
        {
            if (desiredSpaces == 0)
            {
                return token.WithTrailingTrivia();
            }

            return token.WithTrailingTrivia(SyntaxFactory.Whitespace(new string(' ', desiredSpaces)));
        }

        // Complex case: non-whitespace trivia present (e.g., inline multi-line comment).
        // Normalize whitespace around the non-whitespace items and set the final spacing.
        return NormalizeTrailingTriviaWithNonWhitespace(token, trailing, desiredSpaces);
    }

    /// <summary>
    /// Normalizes trailing trivia that contains non-whitespace items (such as inline comments).
    /// Whitespace between non-whitespace items is collapsed to a single space. The trailing
    /// whitespace after the last non-whitespace item is set to the desired space count
    /// </summary>
    /// <param name="token">The token whose trailing trivia to normalize</param>
    /// <param name="trailing">The trailing trivia list</param>
    /// <param name="desiredSpaces">The desired number of spaces at the end of the trivia</param>
    /// <returns>The token with normalized trailing trivia</returns>
    private static SyntaxToken NormalizeTrailingTriviaWithNonWhitespace(SyntaxToken token, SyntaxTriviaList trailing, int desiredSpaces)
    {
        // Find the index of the last non-whitespace trivia
        var lastNonWhitespaceIndex = -1;

        for (var triviaIndex = trailing.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (trailing[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                lastNonWhitespaceIndex = triviaIndex;

                break;
            }
        }

        var newTrivia = SyntaxFactory.TriviaList();
        var prevWasWhitespace = false;

        for (var triviaIndex = 0; triviaIndex < trailing.Count; triviaIndex++)
        {
            var trivia = trailing[triviaIndex];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (triviaIndex > lastNonWhitespaceIndex)
                {
                    // Whitespace after the last non-whitespace item — will be replaced below
                    continue;
                }

                if (prevWasWhitespace == false)
                {
                    newTrivia = newTrivia.Add(_singleSpace);
                }

                prevWasWhitespace = true;
            }
            else
            {
                newTrivia = newTrivia.Add(trivia);
                prevWasWhitespace = false;
            }
        }

        // Append desired trailing whitespace
        if (desiredSpaces > 0)
        {
            newTrivia = newTrivia.Add(SyntaxFactory.Whitespace(new string(' ', desiredSpaces)));
        }

        return token.WithTrailingTrivia(newTrivia);
    }

    /// <summary>
    /// Collapses multiple consecutive whitespace characters in trailing trivia to a single space.
    /// Does not add or remove whitespace — only normalizes multi-space whitespace trivia items
    /// </summary>
    /// <param name="token">The token whose trailing trivia to normalize</param>
    /// <returns>The token with collapsed trailing whitespace</returns>
    internal static SyntaxToken CollapseMultipleTrailingSpaces(SyntaxToken token)
    {
        var trailing = token.TrailingTrivia;

        if (trailing.Count == 0)
        {
            return token;
        }

        if (NeedsTrailingSpaceNormalization(trailing) == false)
        {
            return token;
        }

        return token.WithTrailingTrivia(BuildCollapsedTrailingTrivia(trailing));
    }

    /// <summary>
    /// Determines whether trailing trivia requires whitespace normalization
    /// </summary>
    /// <param name="trailing">The trailing trivia list to inspect</param>
    /// <returns><see langword="true"/> if normalization is needed; otherwise, <see langword="false"/></returns>
    private static bool NeedsTrailingSpaceNormalization(SyntaxTriviaList trailing)
    {
        var prevWasWhitespace = false;

        foreach (var trivia in trailing)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (trivia.Span.Length > 1 || prevWasWhitespace)
                {
                    return true;
                }

                prevWasWhitespace = true;
            }
            else
            {
                prevWasWhitespace = false;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds a trailing trivia list with consecutive whitespace collapsed to single spaces
    /// </summary>
    /// <param name="trailing">The original trailing trivia list</param>
    /// <returns>The normalized trailing trivia list</returns>
    private static SyntaxTriviaList BuildCollapsedTrailingTrivia(SyntaxTriviaList trailing)
    {
        var newTrivia = SyntaxFactory.TriviaList();
        var prevWasWhitespace = false;

        foreach (var trivia in trailing)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (prevWasWhitespace == false)
                {
                    newTrivia = newTrivia.Add(_singleSpace);
                }

                prevWasWhitespace = true;
            }
            else
            {
                newTrivia = newTrivia.Add(trivia);
                prevWasWhitespace = false;
            }
        }

        return newTrivia;
    }

    #endregion // Methods
}