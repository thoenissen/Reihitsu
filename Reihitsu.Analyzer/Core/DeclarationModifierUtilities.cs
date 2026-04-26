using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Helper methods for declaration modifiers.
/// </summary>
public static class DeclarationModifierUtilities
{
    #region Methods

    /// <summary>
    /// Checks whether the given modifiers contain an accessibility modifier.
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns><see langword="true"/> if an accessibility modifier is present</returns>
    public static bool HasAccessibilityModifier(SyntaxTokenList modifiers)
    {
        return modifiers.Any(modifier => IsAccessibilityModifier(modifier.Kind()));
    }

    /// <summary>
    /// Checks whether the given modifier kind is an accessibility modifier.
    /// </summary>
    /// <param name="syntaxKind">Syntax kind</param>
    /// <returns><see langword="true"/> if the kind is an accessibility modifier</returns>
    private static bool IsAccessibilityModifier(SyntaxKind syntaxKind)
    {
        return syntaxKind is SyntaxKind.PublicKeyword
                          or SyntaxKind.PrivateKeyword
                          or SyntaxKind.ProtectedKeyword
                          or SyntaxKind.InternalKeyword;
    }

    /// <summary>
    /// Removes all accessibility modifiers from the specified list.
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns>Modifiers without accessibility modifiers</returns>
    private static SyntaxTokenList RemoveAccessibilityModifiers(SyntaxTokenList modifiers)
    {
        return SyntaxFactory.TokenList(modifiers.Where(obj => IsAccessibilityModifier(obj.Kind()) == false));
    }

    /// <summary>
    /// Replaces the accessibility modifiers with the specified modifier.
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <param name="accessibilityModifier">Accessibility modifier</param>
    /// <returns>Updated modifiers</returns>
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
    /// Gets the modifiers for the specified declaration.
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>Modifiers</returns>
    public static SyntaxTokenList GetModifiers(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.Modifiers,
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Modifiers,
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.Modifiers,
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Modifiers,
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Modifiers,
                   EventDeclarationSyntax eventDeclaration => eventDeclaration.Modifiers,
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.Modifiers,
                   ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.Modifiers,
                   DestructorDeclarationSyntax destructorDeclaration => destructorDeclaration.Modifiers,
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.Modifiers,
                   OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.Modifiers,
                   ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => conversionOperatorDeclaration.Modifiers,
                   _ => throw new ArgumentOutOfRangeException(nameof(memberDeclaration))
               };
    }

    /// <summary>
    /// Applies the modifiers to the specified declaration.
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="modifiers">Modifiers</param>
    /// <returns>Updated declaration</returns>
    public static MemberDeclarationSyntax WithModifiers(MemberDeclarationSyntax memberDeclaration, SyntaxTokenList modifiers)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.WithModifiers(modifiers),
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.WithModifiers(modifiers),
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.WithModifiers(modifiers),
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.WithModifiers(modifiers),
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.WithModifiers(modifiers),
                   EventDeclarationSyntax eventDeclaration => eventDeclaration.WithModifiers(modifiers),
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.WithModifiers(modifiers),
                   ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.WithModifiers(modifiers),
                   DestructorDeclarationSyntax destructorDeclaration => destructorDeclaration.WithModifiers(modifiers),
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.WithModifiers(modifiers),
                   OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.WithModifiers(modifiers),
                   ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => conversionOperatorDeclaration.WithModifiers(modifiers),
                   _ => throw new ArgumentOutOfRangeException(nameof(memberDeclaration))
               };
    }

    #endregion // Methods
}