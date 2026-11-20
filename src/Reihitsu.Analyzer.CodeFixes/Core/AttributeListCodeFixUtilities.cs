using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared guards of the code fixes that put an expression-bodied member on one line and let the formatter lay out its
/// attribute lists: the fix of a property (RH5401-004) and the fix of an indexer (RH5419-004). Both withhold the fix where
/// an attribute list holds something between its brackets that the layout would move, and where formatting would change the
/// blank lines behind an attribute list
/// </summary>
internal static class AttributeListCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether one of the member's attribute lists holds, between its brackets, a blank line, a directive of any
    /// kind, disabled text, or a comment that starts its own line (RH5401-004, RH5419-004). Between its brackets means the
    /// gaps between two tokens of the list, from its <c>[</c> through its <c>]</c>; the trivia in front of the <c>[</c> and
    /// behind the <c>]</c> is outside. A comment with code in front of it on its line does not count
    /// </summary>
    /// <param name="member">Member as written</param>
    /// <returns><see langword="true"/> when an attribute list holds such a blank line, directive, disabled text, or comment; otherwise, <see langword="false"/></returns>
    public static bool AnyHoldsBlankLineDirectiveDisabledTextOrOwnLineCommentBetweenBrackets(MemberDeclarationSyntax member)
    {
        return member.AttributeLists.Any(HoldsBlankLineDirectiveDisabledTextOrOwnLineCommentBetweenBrackets);
    }

    /// <summary>
    /// Determines whether the formatted member holds, in the gap behind each of its attribute lists as written, as many blank
    /// lines as that gap as written (RH5401-004, RH5419-004). The gap runs from the <c>]</c> closing the list to the next
    /// token, with every comment, directive, and disabled text in between. The formatter may split a list into one list per
    /// attribute (FMT-STRUCT-028); the gap behind a list as written is then the gap behind the formatted list that holds its
    /// last attribute, and the gaps between the split lists have no counterpart as written. A member without attribute
    /// lists keeps them trivially
    /// </summary>
    /// <param name="member">Member as written</param>
    /// <param name="formatted">Member as formatted</param>
    /// <returns><see langword="true"/> when every gap keeps its blank lines; otherwise, <see langword="false"/></returns>
    public static bool KeepsBlankLinesBehindAttributeLists(MemberDeclarationSyntax member, MemberDeclarationSyntax formatted)
    {
        if (member.AttributeLists.Count == 0)
        {
            return true;
        }

        var formattedAttributes = formatted.AttributeLists.SelectMany(obj => obj.Attributes).ToList();

        if (formattedAttributes.Count != member.AttributeLists.Sum(obj => obj.Attributes.Count))
        {
            return false;
        }

        var attributeIndex = -1;

        foreach (var attributeList in member.AttributeLists)
        {
            if (attributeList.Attributes.Count == 0)
            {
                return false;
            }

            attributeIndex += attributeList.Attributes.Count;

            if (formattedAttributes[attributeIndex].Parent is not AttributeListSyntax formattedList)
            {
                return false;
            }

            var originalCloseBracket = attributeList.CloseBracketToken;
            var formattedCloseBracket = formattedList.CloseBracketToken;

            if (TokenGapAnalysis.Between(originalCloseBracket, originalCloseBracket.GetNextToken()).BlankLineCount
                != TokenGapAnalysis.Between(formattedCloseBracket, formattedCloseBracket.GetNextToken()).BlankLineCount)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether an attribute list holds, between its brackets, a blank line, a directive of any kind, disabled
    /// text, or a comment that starts its own line
    /// </summary>
    /// <param name="attributeList">Attribute list as written</param>
    /// <returns><see langword="true"/> when the list holds such a blank line, directive, disabled text, or comment; otherwise, <see langword="false"/></returns>
    private static bool HoldsBlankLineDirectiveDisabledTextOrOwnLineCommentBetweenBrackets(AttributeListSyntax attributeList)
    {
        for (var token = attributeList.OpenBracketToken; token != attributeList.CloseBracketToken; token = token.GetNextToken())
        {
            var nextToken = token.GetNextToken();

            if (TokenGapAnalysis.Between(token, nextToken).BlankLineCount > 0)
            {
                return true;
            }

            // A directive, disabled text, and a comment that starts its own line always lie in the leading trivia of the
            // token behind them, because a token's trailing trivia ends with the first line break behind it
            for (var triviaIndex = 0; triviaIndex < nextToken.LeadingTrivia.Count; triviaIndex++)
            {
                var trivia = nextToken.LeadingTrivia[triviaIndex];

                if (SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia)
                    || (SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                        && TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(nextToken, triviaIndex)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion // Methods
}