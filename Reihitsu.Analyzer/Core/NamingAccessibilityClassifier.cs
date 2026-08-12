using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Classifies field and property declarations by their effective accessibility for casing-rule ownership
/// </summary>
internal static class NamingAccessibilityClassifier
{
    #region Methods

    /// <summary>
    /// Gets the single casing-rule accessibility owner for a declaration
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns>The effective accessibility owner, or <see cref="Accessibility.NotApplicable"/> when no rule owns it</returns>
    internal static Accessibility GetEffectiveAccessibility(MemberDeclarationSyntax declaration)
    {
        if (declaration is PropertyDeclarationSyntax { ExplicitInterfaceSpecifier: not null })
        {
            return Accessibility.NotApplicable;
        }

        var modifiers = declaration.Modifiers;

        if (modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return Accessibility.Public;
        }

        if (modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            return modifiers.Any(SyntaxKind.InternalKeyword)
                       ? Accessibility.Internal
                       : Accessibility.Protected;
        }

        if (modifiers.Any(SyntaxKind.InternalKeyword))
        {
            return Accessibility.Internal;
        }

        if (modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            return Accessibility.Private;
        }

        return declaration.Parent is InterfaceDeclarationSyntax
                   ? Accessibility.Public
                   : Accessibility.Private;
    }

    #endregion // Methods
}