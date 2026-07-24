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
    /// Adds (or replaces) the accessibility modifier on the specified declaration while keeping the
    /// declaration's leading trivia (such as XML documentation and indentation) attached to it
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="accessibilityModifier">Accessibility modifier</param>
    /// <returns>Updated declaration</returns>
    /// <remarks>
    /// The leading trivia is captured from the declaration's first token, detached while the modifier is
    /// inserted, and reattached to the new first token afterwards. This prevents the inserted modifier from
    /// being placed in front of leading trivia such as a doc comment, which would otherwise detach the XML
    /// documentation from the symbol (CS1587)
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
    /// The leading trivia is captured from the declaration's first token, detached while the modifiers are
    /// inserted, and reattached to the new first token afterwards. This prevents the inserted modifiers from
    /// being placed in front of leading trivia such as a doc comment, which would otherwise detach the XML
    /// documentation from the symbol (CS1587)
    /// </remarks>
    public static MemberDeclarationSyntax AddAccessibilityModifiers(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<SyntaxKind> accessibilityModifiers)
    {
        var leadingTrivia = memberDeclaration.GetLeadingTrivia();
        var declarationWithoutLeadingTrivia = memberDeclaration.WithLeadingTrivia(SyntaxFactory.TriviaList());
        var updatedModifiers = AddAccessibilityModifiers(declarationWithoutLeadingTrivia.Modifiers, accessibilityModifiers);

        return declarationWithoutLeadingTrivia.WithModifiers(updatedModifiers)
                                              .WithLeadingTrivia(leadingTrivia);
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