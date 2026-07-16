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
        return modifiers.Any(modifier => IsAccessibilityModifier(modifier.Kind()));
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
        var leadingTrivia = memberDeclaration.GetLeadingTrivia();
        var declarationWithoutLeadingTrivia = memberDeclaration.WithLeadingTrivia(SyntaxFactory.TriviaList());
        var updatedModifiers = AddAccessibilityModifier(declarationWithoutLeadingTrivia.Modifiers, accessibilityModifier);

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
    /// example doc comments and indentation) is moved onto the inserted modifier
    /// </remarks>
    public static SyntaxTokenList AddAccessibilityModifier(SyntaxTokenList modifiers, SyntaxKind accessibilityModifier)
    {
        var cleanedModifiers = RemoveAccessibilityModifiers(modifiers);
        var accessibilityToken = modifiers.FirstOrDefault(obj => IsAccessibilityModifier(obj.Kind()));

        if (accessibilityToken.RawKind != 0)
        {
            var trailingTrivia = accessibilityToken.TrailingTrivia.Count > 0
                                     ? accessibilityToken.TrailingTrivia
                                     : [SyntaxFactory.Space];
            var updatedToken = SyntaxFactory.Token(accessibilityToken.LeadingTrivia, accessibilityModifier, trailingTrivia);

            return cleanedModifiers.Insert(0, updatedToken);
        }

        return cleanedModifiers.Insert(0, SyntaxFactory.Token(accessibilityModifier));
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
    /// Checks whether the given modifier kind is an accessibility modifier
    /// </summary>
    /// <param name="syntaxKind">Syntax kind</param>
    /// <returns><see langword="true"/> if the kind is an accessibility modifier</returns>
    private static bool IsAccessibilityModifier(SyntaxKind syntaxKind)
    {
        return syntaxKind is SyntaxKind.PublicKeyword
                          or SyntaxKind.PrivateKeyword
                          or SyntaxKind.ProtectedKeyword
                          or SyntaxKind.InternalKeyword
                          or SyntaxKind.FileKeyword;
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