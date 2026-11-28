using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Safety checks for using directive reordering
/// </summary>
public static class UsingDirectiveOrderingSafety
{
    #region Methods

    /// <summary>
    /// Determines whether the given using block can be reordered safely. A block is unsafe when a preprocessor
    /// directive stands in the leading trivia of any token of the block other than the first token of its first
    /// directive — in front of a later directive, or between the tokens of a directive — or when the leading trivia
    /// of that first token holds a conditional directive (<c>#if</c>, <c>#elif</c>, <c>#else</c>, <c>#endif</c>).
    /// Any other directive in front of the block stands in front of every directive whatever order they take and
    /// does not make the block unsafe. The decision reads trivia kinds, not text, so a line of a comment that starts
    /// with <c>#</c> is comment text and does not make the block unsafe
    /// </summary>
    /// <param name="usingDirectives">Using directives</param>
    /// <returns><see langword="true"/> if the block can be reordered safely</returns>
    public static bool CanSafelyReorder(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        for (var directiveIndex = 0; directiveIndex < usingDirectives.Count; directiveIndex++)
        {
            var usingDirective = usingDirectives[directiveIndex];

            if (usingDirective.ContainsDirectives == false)
            {
                continue;
            }

            foreach (var token in usingDirective.DescendantTokens())
            {
                var isFrontOfBlock = directiveIndex == 0
                                     && token == usingDirective.GetFirstToken();

                if (isFrontOfBlock)
                {
                    if (token.LeadingTrivia.Any(SyntaxTriviaUtilities.IsConditionalDirective))
                    {
                        return false;
                    }
                }
                else if (token.LeadingTrivia.Any(static trivia => trivia.IsDirective))
                {
                    return false;
                }
            }
        }

        return true;
    }

    #endregion // Methods
}