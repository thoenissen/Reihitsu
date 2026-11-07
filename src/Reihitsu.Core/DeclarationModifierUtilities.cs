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
    /// Checks whether a comment, a directive, or disabled text stands in front of an accessibility modifier that is not
    /// the first modifier: the trivia that <see cref="AddAccessibilityModifiers(MemberDeclarationSyntax, IReadOnlyList{SyntaxKind})"/>
    /// carries onto the token behind such a modifier when it removes the modifier
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns><see langword="true"/> if such a comment, directive, or disabled text exists</returns>
    public static bool HasCommentOrDirectiveInFrontOfLaterAccessibilityModifier(SyntaxTokenList modifiers)
    {
        return modifiers.Skip(1)
                        .Where(static modifier => IsAccessibilityModifier(modifier.Kind()))
                        .SelectMany(static modifier => modifier.LeadingTrivia)
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
    /// Replaces the accessibility modifiers with the specified modifier
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <param name="accessibilityModifier">Accessibility modifier</param>
    /// <returns>Updated modifiers</returns>
    /// <remarks>
    /// This overload operates on the modifier list in isolation and does not relocate any leading trivia that
    /// is attached to a declaration's first token. Callers that add a modifier to a declaration should use
    /// <see cref="AddAccessibilityModifier(MemberDeclarationSyntax, SyntaxKind)"/> so leading trivia (for
    /// example doc comments and indentation) is moved onto the inserted modifier. The file-scoped
    /// <see langword="file"/> modifier is never replaced by this method: unlike the four accessibility
    /// keywords, <see langword="file"/> cannot be combined with another accessibility modifier, so silently
    /// replacing it would widen a file-local type's visibility instead of leaving the (already invalid)
    /// combination for the caller to detect
    /// </remarks>
    public static SyntaxTokenList AddAccessibilityModifier(SyntaxTokenList modifiers, SyntaxKind accessibilityModifier)
    {
        return AddAccessibilityModifiers(modifiers, [accessibilityModifier]);
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
    /// therefore stand on the declaration's line and in its column. An accessibility modifier that is removed from behind
    /// another modifier hands its leading trivia to the token that followed it, so a line that modifier started keeps its
    /// place and that token stands on it, in the modifier's column
    /// </remarks>
    public static MemberDeclarationSyntax AddAccessibilityModifiers(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<SyntaxKind> accessibilityModifiers)
    {
        var firstToken = GetFirstTokenBehindAttributeLists(memberDeclaration);
        var leadingTrivia = firstToken.LeadingTrivia;
        var declarationWithoutLeadingTrivia = CarryLeadingTriviaOfLaterAccessibilityModifiers(memberDeclaration.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(SyntaxFactory.TriviaList())));
        var updatedModifiers = AddAccessibilityModifiers(declarationWithoutLeadingTrivia.Modifiers, accessibilityModifiers);
        var updatedDeclaration = declarationWithoutLeadingTrivia.WithModifiers(updatedModifiers);
        var updatedFirstToken = GetFirstTokenBehindAttributeLists(updatedDeclaration);

        return updatedDeclaration.ReplaceToken(updatedFirstToken, updatedFirstToken.WithLeadingTrivia(leadingTrivia));
    }

    /// <summary>
    /// Replaces the accessibility modifiers with the specified sequence of modifiers. Passing more than one
    /// modifier expresses a compound accessibility such as <see langword="protected"/> <see langword="internal"/>
    /// or <see langword="private"/> <see langword="protected"/>
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <param name="accessibilityModifiers">Accessibility modifiers in declaration order</param>
    /// <returns>Updated modifiers</returns>
    /// <remarks>
    /// This overload operates on the modifier list in isolation and does not relocate any leading trivia that is
    /// attached to a declaration's first token. Callers that add modifiers to a declaration should use
    /// <see cref="AddAccessibilityModifiers(MemberDeclarationSyntax, IReadOnlyList{SyntaxKind})"/> so leading
    /// trivia (for example doc comments and indentation) is moved onto the inserted modifiers. When an
    /// accessibility modifier already exists its trivia is reused, otherwise the inserted modifiers are separated
    /// by a single space. The file-scoped <see langword="file"/> modifier is never replaced by this method; see
    /// <see cref="AddAccessibilityModifier(SyntaxTokenList, SyntaxKind)"/>
    /// </remarks>
    public static SyntaxTokenList AddAccessibilityModifiers(SyntaxTokenList modifiers, IReadOnlyList<SyntaxKind> accessibilityModifiers)
    {
        var cleanedModifiers = RemoveAccessibilityModifiers(modifiers);
        var accessibilityToken = modifiers.FirstOrDefault(obj => IsAccessibilityModifier(obj.Kind()));
        var hasAccessibilityToken = accessibilityToken.RawKind != 0;
        var leadingTrivia = hasAccessibilityToken ? accessibilityToken.LeadingTrivia : default;
        var trailingTrivia = hasAccessibilityToken && accessibilityToken.TrailingTrivia.Count > 0
                                 ? accessibilityToken.TrailingTrivia
                                 : [SyntaxFactory.Space];
        var tokens = new List<SyntaxToken>(accessibilityModifiers.Count);

        for (var index = 0; index < accessibilityModifiers.Count; index++)
        {
            tokens.Add(SyntaxFactory.Token(index == 0 ? leadingTrivia : default,
                                           accessibilityModifiers[index],
                                           index == accessibilityModifiers.Count - 1 ? trailingTrivia : [SyntaxFactory.Space]));
        }

        return cleanedModifiers.InsertRange(0, tokens);
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
    /// Moves the leading trivia of every accessibility modifier that is not the declaration's first modifier onto the
    /// token that follows it and is kept: the next modifier that is no accessibility modifier, or the token behind the
    /// modifier list. The accessibility modifiers are removed afterwards, and the trivia in front of them would go with
    /// them
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>The declaration with the leading trivia of its later accessibility modifiers carried onward</returns>
    private static MemberDeclarationSyntax CarryLeadingTriviaOfLaterAccessibilityModifiers(MemberDeclarationSyntax memberDeclaration)
    {
        var modifiers = memberDeclaration.Modifiers;
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();
        var carriedTrivia = new List<SyntaxTrivia>();

        for (var index = 1; index < modifiers.Count; index++)
        {
            var modifier = modifiers[index];

            if (IsAccessibilityModifier(modifier.Kind()))
            {
                carriedTrivia.AddRange(modifier.LeadingTrivia);
                replacements[modifier] = modifier.WithLeadingTrivia(SyntaxFactory.TriviaList());
            }
            else if (carriedTrivia.Count > 0)
            {
                replacements[modifier] = modifier.WithLeadingTrivia(carriedTrivia.Concat(modifier.LeadingTrivia));
                carriedTrivia.Clear();
            }
        }

        if (carriedTrivia.Count > 0)
        {
            var nextToken = modifiers[modifiers.Count - 1].GetNextToken();

            replacements[nextToken] = nextToken.WithLeadingTrivia(carriedTrivia.Concat(nextToken.LeadingTrivia));
        }

        return replacements.Count == 0
                   ? memberDeclaration
                   : memberDeclaration.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
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
    /// <see cref="AddAccessibilityModifier(SyntaxTokenList, SyntaxKind)"/> may replace. The file-scoped
    /// <see langword="file"/> modifier is intentionally excluded; see that method's remarks
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

    /// <summary>
    /// Removes all accessibility modifiers from the specified list
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns>Modifiers without accessibility modifiers</returns>
    private static SyntaxTokenList RemoveAccessibilityModifiers(SyntaxTokenList modifiers)
    {
        return SyntaxFactory.TokenList(modifiers.Where(obj => IsAccessibilityModifier(obj.Kind()) == false));
    }

    #endregion // Methods
}