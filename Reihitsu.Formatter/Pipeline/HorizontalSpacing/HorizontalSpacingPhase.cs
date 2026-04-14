using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Normalizes horizontal spacing between tokens on the same line.
/// Handles operator spacing, comma spacing, semicolons in for-loops,
/// keyword spacing, parenthesis spacing, and multiple consecutive spaces.
/// </summary>
internal static class HorizontalSpacingPhase
{
    #region Fields

    /// <summary>
    ///     A single space whitespace trivia.
    /// </summary>
    private static readonly SyntaxTrivia _singleSpace = SyntaxFactory.Whitespace(" ");

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Applies horizontal spacing rules to the given syntax tree.
    /// </summary>
    /// <param name="root">The root syntax node.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The syntax tree with normalized horizontal spacing.</returns>
    public static SyntaxNode Execute(SyntaxNode root, CancellationToken cancellationToken)
    {
        var rewriter = new HorizontalSpacingRewriter(cancellationToken);

        return rewriter.Visit(root);
    }

    /// <summary>
    /// Determines whether two adjacent tokens are separated by an end-of-line trivia,
    /// meaning they are on different lines and spacing should not be adjusted.
    /// </summary>
    /// <param name="token">The first token.</param>
    /// <param name="nextToken">The second token.</param>
    /// <returns><see langword="true"/> if the tokens are on different lines; otherwise, <see langword="false"/>.</returns>
    internal static bool AreSeparatedByEndOfLine(SyntaxToken token, SyntaxToken nextToken)
    {
        foreach (var trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        foreach (var trivia in nextToken.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines the desired number of spaces after the current token and before the next token,
    /// based on horizontal spacing rules. Returns <see langword="null"/> if no specific
    /// rule applies, in which case only the collapse-multiple-spaces rule is used.
    /// </summary>
    /// <param name="current">The current token.</param>
    /// <param name="next">The next token on the same line.</param>
    /// <returns>The desired space count, or <see langword="null"/> if only the collapse-multiple-spaces rule applies.</returns>
    internal static int? GetDesiredSpacesAfter(SyntaxToken current, SyntaxToken next)
    {
        // No space after ( or [
        if (current.IsKind(SyntaxKind.OpenParenToken) || current.IsKind(SyntaxKind.OpenBracketToken))
        {
            return 0;
        }

        // No space before ) or ]
        if (next.IsKind(SyntaxKind.CloseParenToken) || next.IsKind(SyntaxKind.CloseBracketToken))
        {
            return 0;
        }

        // No space before comma
        if (next.IsKind(SyntaxKind.CommaToken))
        {
            return 0;
        }

        // Exactly one space after comma (except rank-only array declarations such as int[,])
        if (current.IsKind(SyntaxKind.CommaToken))
        {
            if (current.Parent is ArrayRankSpecifierSyntax arrayRankSpecifier
                && IsRankOnlyArraySpecifier(arrayRankSpecifier))
            {
                return 0;
            }

            return 1;
        }

        // Binary and assignment operators — exactly one space on each side
        if (IsBinaryOrAssignmentOperator(current))
        {
            return 1;
        }

        if (IsBinaryOrAssignmentOperator(next))
        {
            return 1;
        }

        // Semicolons in for-loop headers — exactly one space after
        if (current.IsKind(SyntaxKind.SemicolonToken) && current.Parent is ForStatementSyntax)
        {
            return 1;
        }

        // Keyword spacing
        if (IsSpacedKeyword(current))
        {
            // Exception: return; and throw; — no space before ;
            if ((current.IsKind(SyntaxKind.ReturnKeyword) || current.IsKind(SyntaxKind.ThrowKeyword))
                && next.IsKind(SyntaxKind.SemicolonToken))
            {
                return 0;
            }

            // Exception: target-typed new() — no space before (
            if (current.IsKind(SyntaxKind.NewKeyword)
                && next.IsKind(SyntaxKind.OpenParenToken)
                && current.Parent is ImplicitObjectCreationExpressionSyntax)
            {
                return 0;
            }

            // Exception: new[] — no space before [
            if (current.IsKind(SyntaxKind.NewKeyword)
                && next.IsKind(SyntaxKind.OpenBracketToken)
                && current.Parent is ImplicitArrayCreationExpressionSyntax)
            {
                return 0;
            }

            // Exception: constructor constraint new() — no space before (
            if (current.IsKind(SyntaxKind.NewKeyword)
                && next.IsKind(SyntaxKind.OpenParenToken)
                && current.Parent is ConstructorConstraintSyntax)
            {
                return 0;
            }

            return 1;
        }

        // No specific rule — return null so only collapse of multiple spaces is applied
        return null;
    }

    /// <summary>
    /// Determines whether the specified token is a binary or assignment operator
    /// based on its parent syntax node.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><see langword="true"/> if the token is a binary or assignment operator; otherwise, <see langword="false"/>.</returns>
    private static bool IsBinaryOrAssignmentOperator(SyntaxToken token)
    {
        return token.Parent is BinaryExpressionSyntax
               || token.Parent is AssignmentExpressionSyntax
               || (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is EqualsValueClauseSyntax)
               || (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is NameEqualsSyntax);
    }

    /// <summary>
    /// Determines whether an array rank specifier only declares the rank and does not contain size expressions.
    /// </summary>
    /// <param name="arrayRankSpecifier">The array rank specifier to inspect.</param>
    /// <returns><see langword="true"/> if all entries are omitted size expressions; otherwise, <see langword="false"/>.</returns>
    private static bool IsRankOnlyArraySpecifier(ArrayRankSpecifierSyntax arrayRankSpecifier)
    {
        return arrayRankSpecifier.Sizes.All(static size => size.IsKind(SyntaxKind.OmittedArraySizeExpression));
    }

    /// <summary>
    /// Determines whether the token is a keyword that should be followed by exactly one space
    /// before its parenthesis or expression.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><see langword="true"/> if the token is a spaced keyword; otherwise, <see langword="false"/>.</returns>
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
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Sets the trailing whitespace of a token to the specified number of spaces.
    /// Preserves non-whitespace trivia (such as inline comments).
    /// </summary>
    /// <param name="token">The token whose trailing whitespace to normalize.</param>
    /// <param name="desiredSpaces">The desired number of trailing spaces.</param>
    /// <returns>The token with adjusted trailing whitespace.</returns>
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
        var allWhitespace = true;

        foreach (var trivia in trailing)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                allWhitespace = false;

                break;
            }
        }

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
    /// whitespace after the last non-whitespace item is set to the desired space count.
    /// </summary>
    /// <param name="token">The token whose trailing trivia to normalize.</param>
    /// <param name="trailing">The trailing trivia list.</param>
    /// <param name="desiredSpaces">The desired number of spaces at the end of the trivia.</param>
    /// <returns>The token with normalized trailing trivia.</returns>
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
    /// Does not add or remove whitespace — only normalizes multi-space whitespace trivia items.
    /// </summary>
    /// <param name="token">The token whose trailing trivia to normalize.</param>
    /// <returns>The token with collapsed trailing whitespace.</returns>
    internal static SyntaxToken CollapseMultipleTrailingSpaces(SyntaxToken token)
    {
        var trailing = token.TrailingTrivia;

        if (trailing.Count == 0)
        {
            return token;
        }

        // First pass: check if any normalization is needed
        var needsNormalization = false;
        var prevWasWhitespace = false;

        foreach (var trivia in trailing)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (trivia.Span.Length > 1 || prevWasWhitespace)
                {
                    needsNormalization = true;

                    break;
                }

                prevWasWhitespace = true;
            }
            else
            {
                prevWasWhitespace = false;
            }
        }

        if (needsNormalization == false)
        {
            return token;
        }

        // Second pass: rebuild trivia with collapsed whitespace
        var newTrivia = SyntaxFactory.TriviaList();
        prevWasWhitespace = false;

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

        return token.WithTrailingTrivia(newTrivia);
    }

    #endregion // Methods
}