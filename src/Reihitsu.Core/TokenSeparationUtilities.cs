using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Decides whether two tokens written without any separation would be read as other tokens. The C# lexer decides it, so
/// formatter phases and code fixes that place two tokens next to each other share one exact answer
/// </summary>
public static class TokenSeparationUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether two tokens written without any separation would be lexed differently than as the left token
    /// followed by the right one, for example <c>return</c> and <c>42</c> as <c>return42</c>, <c>-</c> and <c>-x</c> as
    /// <c>--x</c>, or <c>/</c> and <c>/</c> as a comment. <c>return</c> and <c>@a</c> or <c>(</c> stay two tokens
    /// </summary>
    /// <param name="left">The left token</param>
    /// <param name="right">The right token</param>
    /// <returns><see langword="true"/> if the tokens need a separating space; otherwise, <see langword="false"/></returns>
    public static bool WouldMergeIntoOneToken(SyntaxToken left, SyntaxToken right)
    {
        return WouldMergeIntoOneToken(GetLexicalUnitText(left), GetLexicalUnitText(right));
    }

    /// <summary>
    /// Determines whether two texts written without any separation would be lexed differently than as the left text
    /// followed by the right one. Each text is one lexical unit, as provided by <see cref="GetLexicalUnitText"/>. The C#
    /// lexer decides this: the texts merge when the lexer reads their concatenation as other tokens or other trivia than
    /// it reads each text alone. That covers <c>return</c> + <c>value</c>, an identifier that starts with a Unicode escape,
    /// a comment start such as <c>/</c> + <c>/</c>, and a token the parser composes of several lexer tokens, such as
    /// <c>..</c>, in front of a token that does not change them. Pairs the parser rather than the lexer composes, such as
    /// <c>&gt;</c> + <c>&gt;</c>, are not covered
    /// </summary>
    /// <param name="left">Text of the left lexical unit</param>
    /// <param name="right">Text of the right lexical unit</param>
    /// <returns><see langword="true"/> if the texts need a separating space; otherwise, <see langword="false"/></returns>
    public static bool WouldMergeIntoOneToken(string left, string right)
    {
        var separateTokens = Lex(left).Concat(Lex(right));

        return separateTokens.SequenceEqual(Lex(left + right)) == false;
    }

    /// <summary>
    /// Gets the text the lexer reads as one unit for a token. The delimiters of an interpolated string are only produced
    /// inside the whole string, so the complete interpolated string stands for them; every other token is its own unit
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The text of the lexical unit</returns>
    public static string GetLexicalUnitText(SyntaxToken token)
    {
        if (token.Parent is InterpolatedStringExpressionSyntax interpolatedString
            && (token == interpolatedString.StringStartToken || token == interpolatedString.StringEndToken))
        {
            return interpolatedString.ToString();
        }

        return token.Text;
    }

    /// <summary>
    /// Lexes a text into the kinds and texts of its tokens and of the trivia around them, without the end-of-file token
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The kinds and texts in text order</returns>
    private static List<(SyntaxKind Kind, string Text)> Lex(string text)
    {
        var elements = new List<(SyntaxKind Kind, string Text)>();

        foreach (var token in SyntaxFactory.ParseTokens(text))
        {
            elements.AddRange(token.LeadingTrivia.Select(static trivia => (trivia.Kind(), trivia.ToFullString())));

            if (token.IsKind(SyntaxKind.EndOfFileToken) == false)
            {
                elements.Add((token.Kind(), token.Text));
            }

            elements.AddRange(token.TrailingTrivia.Select(static trivia => (trivia.Kind(), trivia.ToFullString())));
        }

        return elements;
    }

    #endregion // Methods
}