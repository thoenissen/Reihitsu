using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Helper methods for declaration modifiers
/// </summary>
public static class DeclarationModifierUtilities
{
    #region Methods

    /// <summary>
    /// Checks whether the given modifiers contain an accessibility modifier
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns><see langword="true"/> if an accessibility modifier is present</returns>
    public static bool HasAccessibilityModifier(SyntaxTokenList modifiers)
    {
        return modifiers.Any(modifier => IsExplicitAccessibilityModifier(modifier.Kind()));
    }

    /// <summary>
    /// Checks whether a comment, a directive, or disabled text stands next to an accessibility modifier that is not the
    /// first modifier: in its leading trivia, in its trailing trivia, or in the leading trivia of the token that follows
    /// it. These are the gaps that <see cref="AddAccessibilityModifiers(MemberDeclarationSyntax, IReadOnlyList{SyntaxKind})"/>
    /// merges when it removes such a modifier, and the trivia it discards from them is whitespace and line breaks only
    /// when this check returns <see langword="false"/>
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns><see langword="true"/> if such a comment, directive, or disabled text exists</returns>
    public static bool HasCommentOrDirectiveNextToLaterAccessibilityModifier(SyntaxTokenList modifiers)
    {
        return modifiers.Skip(1)
                        .Where(static modifier => IsAccessibilityModifier(modifier.Kind()))
                        .SelectMany(static modifier => modifier.LeadingTrivia
                                                               .Concat(modifier.TrailingTrivia)
                                                               .Concat(modifier.GetNextToken().LeadingTrivia))
                        .Any(static trivia => SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                                              || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Maps a declared accessibility to the modifier keywords that express it, using the canonical keyword order
    /// for the compound accessibilities (<see langword="protected"/> <see langword="internal"/> and
    /// <see langword="private"/> <see langword="protected"/>). This is the single accessibility-keyword policy
    /// shared by the code fixes that materialize an accessibility
    /// </summary>
    /// <param name="accessibility">Declared accessibility</param>
    /// <returns>
    /// The modifier keywords in declaration order, or an empty list for <see cref="Accessibility.NotApplicable"/>.
    /// An empty result leaves the fallback to the caller, which knows whether a nested default
    /// (<see langword="private"/>) or a namespace-level default (<see langword="internal"/>) applies
    /// </returns>
    public static IReadOnlyList<SyntaxKind> GetAccessibilityModifierKinds(Accessibility accessibility)
    {
        return accessibility switch
               {
                   Accessibility.Public => [SyntaxKind.PublicKeyword],
                   Accessibility.Internal => [SyntaxKind.InternalKeyword],
                   Accessibility.Protected => [SyntaxKind.ProtectedKeyword],
                   Accessibility.Private => [SyntaxKind.PrivateKeyword],
                   Accessibility.ProtectedOrInternal => [SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword],
                   Accessibility.ProtectedAndInternal => [SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword],
                   _ => []
               };
    }

    /// <summary>
    /// Adds (or replaces) the accessibility modifier on the specified declaration while keeping the
    /// declaration's leading trivia (such as XML documentation and indentation) attached to it
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="accessibilityModifier">Accessibility modifier</param>
    /// <returns>Updated declaration</returns>
    /// <remarks>
    /// The modifier is written where the first token behind the declaration's attribute lists stood, behind
    /// that token's leading trivia; see <see cref="AddAccessibilityModifiers(MemberDeclarationSyntax, IReadOnlyList{SyntaxKind})"/>
    /// </remarks>
    public static MemberDeclarationSyntax AddAccessibilityModifier(MemberDeclarationSyntax memberDeclaration, SyntaxKind accessibilityModifier)
    {
        return AddAccessibilityModifiers(memberDeclaration, [accessibilityModifier]);
    }

    /// <summary>
    /// Adds (or replaces) the accessibility modifiers on the specified declaration while keeping the
    /// declaration's leading trivia (such as XML documentation and indentation) attached to it. Passing more
    /// than one modifier expresses a compound accessibility such as <see langword="protected"/>
    /// <see langword="internal"/> or <see langword="private"/> <see langword="protected"/>
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="accessibilityModifiers">Accessibility modifiers in declaration order</param>
    /// <returns>Updated declaration</returns>
    /// <remarks>
    /// The modifiers are written where the first token behind the declaration's attribute lists stood: that
    /// token's leading trivia is captured, detached while the modifiers are inserted, and reattached to the new
    /// first token behind the attribute lists afterwards. Without attribute lists, that trivia is the
    /// declaration's leading trivia, so a doc comment stays in front of the inserted modifiers instead of being
    /// detached from the symbol (CS1587). With attribute lists on their own line, the indentation and every
    /// comment and directive line in front of the declaration stay in front of the inserted modifiers, which
    /// therefore stand on the declaration's line and in its column. An existing accessibility modifier is replaced, first
    /// or not; one private method decides where the trivia of every removed accessibility modifier goes, and it also sees
    /// the token behind the modifier list. The file-scoped <see langword="file"/> modifier is never replaced
    /// </remarks>
    public static MemberDeclarationSyntax AddAccessibilityModifiers(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<SyntaxKind> accessibilityModifiers)
    {
        var firstToken = GetFirstTokenBehindAttributeLists(memberDeclaration);
        var leadingTrivia = firstToken.LeadingTrivia;
        var declarationWithoutLeadingTrivia = memberDeclaration.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(SyntaxFactory.TriviaList()));
        var modifiers = declarationWithoutLeadingTrivia.Modifiers;
        var originalFollowingToken = modifiers.Count > 0
                                         ? modifiers[modifiers.Count - 1].GetNextToken()
                                         : GetFirstTokenBehindAttributeLists(declarationWithoutLeadingTrivia);
        var followingToken = originalFollowingToken;
        var updatedModifiers = ReplaceAccessibilityModifiers(modifiers, accessibilityModifiers, ref followingToken);

        declarationWithoutLeadingTrivia = declarationWithoutLeadingTrivia.ReplaceToken(originalFollowingToken, followingToken);

        var updatedDeclaration = declarationWithoutLeadingTrivia.WithModifiers(updatedModifiers);
        var updatedFirstToken = GetFirstTokenBehindAttributeLists(updatedDeclaration);

        return updatedDeclaration.ReplaceToken(updatedFirstToken, updatedFirstToken.WithLeadingTrivia(leadingTrivia));
    }

    /// <summary>
    /// Gets the modifiers for the specified declaration
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>Modifiers</returns>
    public static SyntaxTokenList GetModifiers(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration.Modifiers;
    }

    /// <summary>
    /// Applies the modifiers to the specified declaration
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="modifiers">Modifiers</param>
    /// <returns>Updated declaration</returns>
    public static MemberDeclarationSyntax WithModifiers(MemberDeclarationSyntax memberDeclaration, SyntaxTokenList modifiers)
    {
        return memberDeclaration.WithModifiers(modifiers);
    }

    /// <summary>
    /// Replaces the accessibility modifiers with the specified sequence of modifiers and decides, for every removed
    /// accessibility modifier, where its leading and trailing trivia go
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <param name="accessibilityModifiers">Accessibility modifiers in declaration order</param>
    /// <param name="followingToken">
    /// The token behind the modifier list, updated when trivia of a removed modifier goes to it
    /// </param>
    /// <returns>Updated modifiers</returns>
    /// <remarks>
    /// The inserted modifiers stand first. When the first modifier is an accessibility modifier, they take its place: the
    /// first inserted modifier takes its leading trivia and the last one its trailing trivia; otherwise they are separated
    /// from the kept modifiers by a single space. An accessibility modifier behind another modifier is removed together
    /// with one of the two gaps around it, so the other gap keeps its place:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// When the modifier starts its line, the gap in front of it stays: the token that follows it takes
    /// the modifier's leading trivia instead of its own and stands on that line, in the modifier's column. The modifier's
    /// trailing trivia and the following token's own leading trivia are dropped
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// When the modifier ends its line, the gap behind it stays: the token in front of it ends with the modifier's
    /// trailing trivia instead of the whitespace at its own end, so the line break that ended the modifier ends that
    /// token's line, and the following token keeps its line and column
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// When the modifier neither starts nor ends its line, the gap in front of it stays as written: the modifier is
    /// removed together with the whitespace behind it, and the following token stands directly behind that gap
    /// </description>
    /// </item>
    /// </list>
    /// The dropped trivia is whitespace and line breaks only when
    /// <see cref="HasCommentOrDirectiveNextToLaterAccessibilityModifier"/> returns <see langword="false"/>; callers that
    /// must not drop a comment or directive check it first
    /// </remarks>
    private static SyntaxTokenList ReplaceAccessibilityModifiers(SyntaxTokenList modifiers, IReadOnlyList<SyntaxKind> accessibilityModifiers, ref SyntaxToken followingToken)
    {
        var replacesFirstModifier = modifiers.Count > 0 && IsAccessibilityModifier(modifiers[0].Kind());
        var leadingTrivia = replacesFirstModifier ? modifiers[0].LeadingTrivia : default;
        var trailingTrivia = replacesFirstModifier && modifiers[0].TrailingTrivia.Count > 0
                                 ? modifiers[0].TrailingTrivia
                                 : [SyntaxFactory.Space];
        var tokens = new List<SyntaxToken>(modifiers.Count + accessibilityModifiers.Count);

        for (var index = 0; index < accessibilityModifiers.Count; index++)
        {
            tokens.Add(SyntaxFactory.Token(index == 0 ? leadingTrivia : default,
                                           accessibilityModifiers[index],
                                           index == accessibilityModifiers.Count - 1 ? trailingTrivia : [SyntaxFactory.Space]));
        }

        SyntaxTriviaList? carriedLeadingTrivia = null;

        for (var index = replacesFirstModifier ? 1 : 0; index < modifiers.Count; index++)
        {
            var modifier = modifiers[index];

            if (IsAccessibilityModifier(modifier.Kind()) == false)
            {
                tokens.Add(carriedLeadingTrivia == null ? modifier : modifier.WithLeadingTrivia(carriedLeadingTrivia.Value));
                carriedLeadingTrivia = null;
            }
            else if (ContainsEndOfLine(modifier.LeadingTrivia)
                     || (carriedLeadingTrivia == null && ContainsEndOfLine(tokens[tokens.Count - 1].TrailingTrivia)))
            {
                carriedLeadingTrivia = modifier.LeadingTrivia;
            }
            else if (carriedLeadingTrivia == null
                     && ContainsEndOfLine(modifier.TrailingTrivia))
            {
                var previousToken = tokens[tokens.Count - 1];

                tokens[tokens.Count - 1] = previousToken.WithTrailingTrivia(TrimTrailingWhitespace(previousToken.TrailingTrivia.AddRange(modifier.LeadingTrivia)).AddRange(modifier.TrailingTrivia));
            }
        }

        if (carriedLeadingTrivia != null)
        {
            followingToken = followingToken.WithLeadingTrivia(carriedLeadingTrivia.Value);
        }

        return SyntaxFactory.TokenList(tokens);
    }

    /// <summary>
    /// Checks whether the trivia contains a line break
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia contains a line break</returns>
    private static bool ContainsEndOfLine(SyntaxTriviaList trivia)
    {
        return trivia.Any(SyntaxKind.EndOfLineTrivia);
    }

    /// <summary>
    /// Removes the whitespace at the end of the trivia
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns>The trivia without its trailing whitespace</returns>
    private static SyntaxTriviaList TrimTrailingWhitespace(SyntaxTriviaList trivia)
    {
        var count = trivia.Count;

        while (count > 0
               && trivia[count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            count--;
        }

        return SyntaxFactory.TriviaList(trivia.Take(count));
    }

    /// <summary>
    /// Gets the first token of the declaration behind its attribute lists: its first modifier, or, without
    /// modifiers, the token that starts the rest of the declaration (such as its type or its keyword)
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>The first token behind the attribute lists</returns>
    private static SyntaxToken GetFirstTokenBehindAttributeLists(MemberDeclarationSyntax memberDeclaration)
    {
        if (memberDeclaration.Modifiers.Count > 0)
        {
            return memberDeclaration.Modifiers[0];
        }

        return memberDeclaration.AttributeLists.Count > 0
                   ? memberDeclaration.AttributeLists[memberDeclaration.AttributeLists.Count - 1].GetLastToken().GetNextToken()
                   : memberDeclaration.GetFirstToken();
    }

    /// <summary>
    /// Checks whether the given modifier kind is one of the four accessibility keywords that
    /// <see cref="AddAccessibilityModifiers(MemberDeclarationSyntax, IReadOnlyList{SyntaxKind})"/> may replace. The
    /// file-scoped <see langword="file"/> modifier is intentionally excluded: unlike the four accessibility keywords, it
    /// cannot be combined with another accessibility modifier, so silently replacing it would widen a file-local type's
    /// visibility instead of leaving the (already invalid) combination for the caller to detect
    /// </summary>
    /// <param name="syntaxKind">Syntax kind</param>
    /// <returns><see langword="true"/> if the kind is a replaceable accessibility modifier</returns>
    private static bool IsAccessibilityModifier(SyntaxKind syntaxKind)
    {
        return syntaxKind is SyntaxKind.PublicKeyword
                          or SyntaxKind.PrivateKeyword
                          or SyntaxKind.ProtectedKeyword
                          or SyntaxKind.InternalKeyword;
    }

    /// <summary>
    /// Checks whether the given modifier kind declares a type or member's accessibility, including the
    /// file-scoped <see langword="file"/> modifier, which is the sole and mandatory visibility declaration for
    /// a file-local type
    /// </summary>
    /// <param name="syntaxKind">Syntax kind</param>
    /// <returns><see langword="true"/> if the kind declares accessibility</returns>
    private static bool IsExplicitAccessibilityModifier(SyntaxKind syntaxKind)
    {
        return IsAccessibilityModifier(syntaxKind)
               || syntaxKind == SyntaxKind.FileKeyword;
    }

    #endregion // Methods
}