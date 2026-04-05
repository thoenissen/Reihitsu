using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Rules.Spacing;

/// <summary>
/// Normalizes horizontal spacing: single space around binary operators,
/// after commas, after keywords before parentheses, and removes unnecessary spaces.
/// </summary>
internal sealed class HorizontalSpacingRule : FormattingRuleBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public HorizontalSpacingRule(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Ensures a single space before and after binary and assignment operator tokens.
    /// Only operates on tokens that are on the same line as adjacent tokens.
    /// </summary>
    /// <param name="token">The token to process.</param>
    /// <returns>The token with corrected spacing.</returns>
    private static SyntaxToken EnsureSpacingAroundBinaryOperator(SyntaxToken token)
    {
        if (IsBinaryOrAssignmentOperator(token) == false)
        {
            return token;
        }

        // Only adjust spacing if the operator is on the same line as its neighbors
        var previousToken = token.GetPreviousToken();
        var nextToken = token.GetNextToken();

        if (previousToken.IsKind(SyntaxKind.None) || nextToken.IsKind(SyntaxKind.None))
        {
            return token;
        }

        // Ensure single space before (in the token's leading trivia)
        if (IsOnSameLine(previousToken, token))
        {
            if (PreviousTokenHasTrailingWhitespace(previousToken))
            {
                token = RemoveLeadingWhitespace(token);
            }
            else
            {
                token = EnsureSingleLeadingSpace(token);
            }
        }

        // Ensure single space after (in the token's trailing trivia)
        if (IsOnSameLine(token, nextToken))
        {
            token = EnsureSingleTrailingSpace(token);
        }

        return token;
    }

    /// <summary>
    /// Ensures a single space after comma tokens.
    /// </summary>
    /// <param name="token">The token to process.</param>
    /// <returns>The token with corrected spacing.</returns>
    private static SyntaxToken EnsureSpaceAfterComma(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.CommaToken) == false)
        {
            return token;
        }

        // Don't add space after commas in array rank specifiers (e.g., int[,])
        if (token.Parent is ArrayRankSpecifierSyntax)
        {
            return token;
        }

        var nextToken = token.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None) || IsOnSameLine(token, nextToken) == false)
        {
            return token;
        }

        return EnsureSingleTrailingSpace(token);
    }

    /// <summary>
    /// Ensures a single space after semicolons in for-statement headers.
    /// </summary>
    /// <param name="token">The token to process.</param>
    /// <returns>The token with corrected spacing.</returns>
    private static SyntaxToken EnsureSpaceAfterSemicolonInFor(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.SemicolonToken) == false)
        {
            return token;
        }

        if (token.Parent is ForStatementSyntax == false)
        {
            return token;
        }

        var nextToken = token.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None) || IsOnSameLine(token, nextToken) == false)
        {
            return token;
        }

        return EnsureSingleTrailingSpace(token);
    }

    /// <summary>
    /// Ensures a single space after control-flow keywords before their opening parenthesis.
    /// </summary>
    /// <param name="token">The token to process.</param>
    /// <returns>The token with corrected spacing.</returns>
    private static SyntaxToken EnsureSpaceAfterKeyword(SyntaxToken token)
    {
        if (IsKeywordRequiringSpace(token) == false)
        {
            return token;
        }

        var nextToken = token.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None) || IsOnSameLine(token, nextToken) == false)
        {
            return token;
        }

        // Don't add space before semicolon (e.g., `return;` or `throw;`)
        if (nextToken.IsKind(SyntaxKind.SemicolonToken))
        {
            return token;
        }

        // Don't add space before ( or [ for `new()` or `new[]` (target-typed new / implicit array)
        if (token.IsKind(SyntaxKind.NewKeyword)
            && (nextToken.IsKind(SyntaxKind.OpenParenToken) || nextToken.IsKind(SyntaxKind.OpenBracketToken)))
        {
            return token;
        }

        return EnsureSingleTrailingSpace(token);
    }

    /// <summary>
    /// Removes whitespace immediately before a closing parenthesis.
    /// </summary>
    /// <param name="token">The token to process.</param>
    /// <returns>The token with trailing space removed if it precedes a close paren.</returns>
    private static SyntaxToken RemoveSpaceBeforeCloseParen(SyntaxToken token)
    {
        var nextToken = token.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.CloseParenToken) == false)
        {
            return token;
        }

        if (IsOnSameLine(token, nextToken) == false)
        {
            return token;
        }

        if (HasOnlyWhitespaceTrailingTrivia(token) == false)
        {
            return token;
        }

        return token.WithTrailingTrivia(SyntaxTriviaList.Empty);
    }

    /// <summary>
    /// Removes whitespace immediately after an opening parenthesis.
    /// </summary>
    /// <param name="token">The token to process.</param>
    /// <returns>The token with trailing space removed if it is an open paren.</returns>
    private static SyntaxToken RemoveSpaceAfterOpenParen(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.OpenParenToken) == false)
        {
            return token;
        }

        var nextToken = token.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None) || IsOnSameLine(token, nextToken) == false)
        {
            return token;
        }

        if (HasOnlyWhitespaceTrailingTrivia(token) == false)
        {
            return token;
        }

        return token.WithTrailingTrivia(SyntaxTriviaList.Empty);
    }

    /// <summary>
    /// Determines whether the given token is a binary or assignment operator.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><c>true</c> if the token is a binary or assignment operator; otherwise, <c>false</c>.</returns>
    private static bool IsBinaryOrAssignmentOperator(SyntaxToken token)
    {
        if (token.Parent is BinaryExpressionSyntax binary && binary.OperatorToken == token)
        {
            return true;
        }

        if (token.Parent is AssignmentExpressionSyntax assignment && assignment.OperatorToken == token)
        {
            return true;
        }

        // Equals sign in variable declarations (e.g., `var x = 1;`)
        if (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is EqualsValueClauseSyntax)
        {
            return true;
        }

        // Equals sign in anonymous object member assignments (e.g., `new { Name = value }`)
        if (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is NameEqualsSyntax)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given token is the first token on its line.
    /// </summary>
    /// <param name="token">The token to inspect.</param>
    /// <returns><c>true</c> if the token starts a line; otherwise, <c>false</c>.</returns>
    private static bool IsFirstTokenOnLine(SyntaxToken token)
    {
        if (token.GetPreviousToken().IsKind(SyntaxKind.None))
        {
            return true;
        }

        foreach (var trivia in token.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        foreach (var trivia in token.GetPreviousToken().TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given token is a keyword that requires a space after it.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><c>true</c> if the keyword requires a trailing space; otherwise, <c>false</c>.</returns>
    private static bool IsKeywordRequiringSpace(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.IfKeyword
                   or SyntaxKind.ForKeyword
                   or SyntaxKind.ForEachKeyword
                   or SyntaxKind.WhileKeyword
                   or SyntaxKind.SwitchKeyword
                   or SyntaxKind.CatchKeyword
                   or SyntaxKind.UsingKeyword
                   or SyntaxKind.LockKeyword
                   or SyntaxKind.ReturnKeyword
                   or SyntaxKind.ThrowKeyword
                   or SyntaxKind.NewKeyword => true,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether two adjacent tokens appear on the same line
    /// by checking that no end-of-line trivia exists between them.
    /// </summary>
    /// <param name="left">The left token.</param>
    /// <param name="right">The right token.</param>
    /// <returns><c>true</c> if both tokens are on the same line; otherwise, <c>false</c>.</returns>
    private static bool IsOnSameLine(SyntaxToken left, SyntaxToken right)
    {
        foreach (var trivia in left.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return false;
            }
        }

        foreach (var trivia in right.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Ensures a single whitespace trivia in the token's trailing trivia.
    /// Replaces existing whitespace-only trailing trivia with exactly one space.
    /// </summary>
    /// <param name="token">The token to adjust.</param>
    /// <returns>The token with exactly one trailing space.</returns>
    private static SyntaxToken EnsureSingleTrailingSpace(SyntaxToken token)
    {
        var trailing = token.TrailingTrivia;

        if (trailing.Count == 1
            && trailing[0].IsKind(SyntaxKind.WhitespaceTrivia)
            && trailing[0].ToString() == " ")
        {
            return token;
        }

        var result = new List<SyntaxTrivia>();
        var addedSpace = false;

        foreach (var trivia in trailing)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (addedSpace == false)
                {
                    result.Add(SyntaxFactory.Space);
                    addedSpace = true;
                }
            }
            else
            {
                if (addedSpace == false)
                {
                    result.Add(SyntaxFactory.Space);
                    addedSpace = true;
                }

                result.Add(trivia);
            }
        }

        if (addedSpace == false)
        {
            result.Add(SyntaxFactory.Space);
        }

        return token.WithTrailingTrivia(SyntaxFactory.TriviaList(result));
    }

    /// <summary>
    /// Ensures a single whitespace trivia in the token's leading trivia,
    /// replacing only whitespace that does not follow an end-of-line.
    /// </summary>
    /// <param name="token">The token to adjust.</param>
    /// <returns>The token with exactly one leading space (on the same line).</returns>
    private static SyntaxToken EnsureSingleLeadingSpace(SyntaxToken token)
    {
        var leading = token.LeadingTrivia;

        if (leading.Count == 0)
        {
            return token.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Space));
        }

        // If leading trivia contains end-of-line, don't modify (multi-line case)
        foreach (var trivia in leading)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return token;
            }
        }

        // Replace all whitespace with a single space
        if (leading.Count == 1
            && leading[0].IsKind(SyntaxKind.WhitespaceTrivia)
            && leading[0].ToString() == " ")
        {
            return token;
        }

        var result = new List<SyntaxTrivia>();
        var addedSpace = false;

        foreach (var trivia in leading)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (addedSpace == false)
                {
                    result.Add(SyntaxFactory.Space);
                    addedSpace = true;
                }
            }
            else
            {
                result.Add(trivia);
            }
        }

        if (result.Count == 0)
        {
            result.Add(SyntaxFactory.Space);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(result));
    }

    /// <summary>
    /// Determines whether a token's trailing trivia consists only of whitespace.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><c>true</c> if all trailing trivia are whitespace; otherwise, <c>false</c>.</returns>
    private static bool HasOnlyWhitespaceTrailingTrivia(SyntaxToken token)
    {
        if (token.TrailingTrivia.Count == 0)
        {
            return false;
        }

        foreach (var trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether a token has trailing whitespace trivia.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><c>true</c> if the token has trailing whitespace trivia; otherwise, <c>false</c>.</returns>
    private static bool PreviousTokenHasTrailingWhitespace(SyntaxToken token)
    {
        foreach (var trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Normalizes trailing whitespace trivia that contains more than one space character
    /// to a single space, when the token is on the same line as the next token.
    /// </summary>
    /// <param name="token">The token to normalize.</param>
    /// <returns>The token with normalized trailing whitespace.</returns>
    private static SyntaxToken NormalizeTrailingMultiSpace(SyntaxToken token)
    {
        var trailing = token.TrailingTrivia;

        if (trailing.Count == 0)
        {
            return token;
        }

        var nextToken = token.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None) || IsOnSameLine(token, nextToken) == false)
        {
            return token;
        }

        var needsFix = false;

        foreach (var trivia in trailing)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) && trivia.ToString().Length > 1)
            {
                needsFix = true;

                break;
            }
        }

        if (needsFix == false)
        {
            return token;
        }

        var result = new List<SyntaxTrivia>();

        foreach (var trivia in trailing)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                result.Add(SyntaxFactory.Space);
            }
            else
            {
                result.Add(trivia);
            }
        }

        return token.WithTrailingTrivia(SyntaxFactory.TriviaList(result));
    }

    /// <summary>
    /// Removes all leading whitespace trivia from the token.
    /// </summary>
    /// <param name="token">The token to clean.</param>
    /// <returns>The token with leading whitespace removed.</returns>
    private static SyntaxToken RemoveLeadingWhitespace(SyntaxToken token)
    {
        var leading = token.LeadingTrivia;

        if (leading.Count == 0)
        {
            return token;
        }

        var result = new List<SyntaxTrivia>();

        foreach (var trivia in leading)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                result.Add(trivia);
            }
        }

        if (result.Count == leading.Count)
        {
            return token;
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(result));
    }

    /// <summary>
    /// Sets the leading indentation whitespace of a token.
    /// </summary>
    /// <param name="token">The token to update.</param>
    /// <param name="whitespace">The indentation whitespace.</param>
    /// <returns>The token with updated indentation.</returns>
    private static SyntaxToken SetLeadingWhitespace(SyntaxToken token, string whitespace)
    {
        token = RemoveLeadingWhitespace(token);

        if (string.IsNullOrEmpty(whitespace))
        {
            return token;
        }

        var leading = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(whitespace)).AddRange(token.LeadingTrivia);

        return token.WithLeadingTrivia(leading);
    }

    #endregion // Methods

    #region FormattingRuleBase

    /// <inheritdoc/>
    public override SyntaxNode VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
    {
        var visited = (AnonymousObjectCreationExpressionSyntax)base.VisitAnonymousObjectCreationExpression(node);

        if (visited == null)
        {
            return null;
        }

        var newLine = node.NewKeyword.GetLocation().GetLineSpan().StartLinePosition.Line;
        var openBraceLine = node.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (openBraceLine == newLine)
        {
            return visited;
        }

        var newColumn = node.NewKeyword.GetLocation().GetLineSpan().StartLinePosition.Character;
        var braceWhitespace = new string(' ', newColumn);
        var memberWhitespace = new string(' ', newColumn + FormattingContext.IndentSize);
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        if (IsFirstTokenOnLine(visited.OpenBraceToken))
        {
            replacements[visited.OpenBraceToken] = SetLeadingWhitespace(visited.OpenBraceToken, braceWhitespace);
        }

        if (IsFirstTokenOnLine(visited.CloseBraceToken))
        {
            replacements[visited.CloseBraceToken] = SetLeadingWhitespace(visited.CloseBraceToken, braceWhitespace);
        }

        foreach (var initializer in visited.Initializers)
        {
            var firstToken = initializer.GetFirstToken();

            if (IsFirstTokenOnLine(firstToken))
            {
                replacements[firstToken] = SetLeadingWhitespace(firstToken, memberWhitespace);
            }
        }

        if (replacements.Count == 0)
        {
            return visited;
        }

        return visited.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <inheritdoc/>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        token = base.VisitToken(token);

        if (token.IsKind(SyntaxKind.None))
        {
            return token;
        }

        token = NormalizeTrailingMultiSpace(token);
        token = EnsureSpacingAroundBinaryOperator(token);
        token = EnsureSpaceAfterComma(token);
        token = EnsureSpaceAfterSemicolonInFor(token);
        token = EnsureSpaceAfterKeyword(token);
        token = RemoveSpaceBeforeCloseParen(token);
        token = RemoveSpaceAfterOpenParen(token);

        return token;
    }

    #endregion // FormattingRuleBase

    #region IFormattingRule

    /// <inheritdoc/>
    public override FormattingPhase Phase => FormattingPhase.Spacing;

    #endregion // IFormattingRule
}