using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Helper methods for modifier ordering analyzers and code fixes
/// </summary>
internal static class ModifierOrderingUtilities
{
    #region Methods

    /// <summary>
    /// Tries to find the token that violates RH0604
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <param name="diagnosticToken">Diagnostic token</param>
    /// <returns><see langword="true"/> if the modifier order is invalid</returns>
    internal static bool TryGetRh0604Violation(SyntaxTokenList modifiers, out SyntaxToken diagnosticToken)
    {
        diagnosticToken = default;

        if (modifiers.Count < 2)
        {
            return false;
        }

        var orderedModifiers = OrderModifiersForRh0604(modifiers);

        for (var modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
        {
            if (modifiers[modifierIndex].RawKind != orderedModifiers[modifierIndex].RawKind)
            {
                diagnosticToken = modifiers[modifierIndex];

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Orders modifiers according to RH0604
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns>The ordered modifiers</returns>
    internal static SyntaxTokenList OrderModifiersForRh0604(SyntaxTokenList modifiers)
    {
        return SyntaxFactory.TokenList(modifiers.Select((modifier, modifierIndex) => new
                                                                                     {
                                                                                         Modifier = modifier,
                                                                                         ModifierIndex = modifierIndex,
                                                                                     })
                                                .OrderBy(obj => GetRh0604Rank(obj.Modifier))
                                                .ThenBy(obj => obj.ModifierIndex)
                                                .Select(obj => obj.Modifier));
    }

    /// <summary>
    /// Tries to find the token that violates RH0605
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <param name="diagnosticToken">Diagnostic token</param>
    /// <returns><see langword="true"/> if the compound accessibility order is invalid</returns>
    internal static bool TryGetRh0605Violation(SyntaxTokenList modifiers, out SyntaxToken diagnosticToken)
    {
        diagnosticToken = default;

        var protectedTokenIndex = GetModifierIndex(modifiers, SyntaxKind.ProtectedKeyword);
        var internalTokenIndex = GetModifierIndex(modifiers, SyntaxKind.InternalKeyword);
        var privateTokenIndex = GetModifierIndex(modifiers, SyntaxKind.PrivateKeyword);

        if (internalTokenIndex >= 0
            && protectedTokenIndex >= 0
            && internalTokenIndex < protectedTokenIndex)
        {
            diagnosticToken = modifiers[protectedTokenIndex];

            return true;
        }

        if (privateTokenIndex >= 0
            && protectedTokenIndex >= 0
            && protectedTokenIndex < privateTokenIndex)
        {
            diagnosticToken = modifiers[privateTokenIndex];

            return true;
        }

        return false;
    }

    /// <summary>
    /// Reorders only the compound accessibility pair for RH0605
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns>The updated modifiers</returns>
    internal static SyntaxTokenList OrderModifiersForRh0605(SyntaxTokenList modifiers)
    {
        var updatedModifiers = modifiers.ToList();
        var protectedTokenIndex = GetModifierIndex(modifiers, SyntaxKind.ProtectedKeyword);
        var internalTokenIndex = GetModifierIndex(modifiers, SyntaxKind.InternalKeyword);
        var privateTokenIndex = GetModifierIndex(modifiers, SyntaxKind.PrivateKeyword);

        if (internalTokenIndex >= 0
            && protectedTokenIndex >= 0
            && internalTokenIndex < protectedTokenIndex)
        {
            (updatedModifiers[internalTokenIndex], updatedModifiers[protectedTokenIndex]) = (updatedModifiers[protectedTokenIndex], updatedModifiers[internalTokenIndex]);
        }
        else if (privateTokenIndex >= 0
                 && protectedTokenIndex >= 0
                 && protectedTokenIndex < privateTokenIndex)
        {
            (updatedModifiers[protectedTokenIndex], updatedModifiers[privateTokenIndex]) = (updatedModifiers[privateTokenIndex], updatedModifiers[protectedTokenIndex]);
        }

        return SyntaxFactory.TokenList(updatedModifiers);
    }

    /// <summary>
    /// Gets the ranking used by RH0604
    /// </summary>
    /// <param name="modifier">Modifier token</param>
    /// <returns>The ordering rank</returns>
    private static int GetRh0604Rank(SyntaxToken modifier)
    {
        return modifier.Kind() switch
               {
                   SyntaxKind.FileKeyword => 0,
                   SyntaxKind.PublicKeyword => 0,
                   SyntaxKind.PrivateKeyword => 0,
                   SyntaxKind.ProtectedKeyword => 0,
                   SyntaxKind.InternalKeyword => 0,
                   SyntaxKind.StaticKeyword => 1,
                   SyntaxKind.AbstractKeyword => 2,
                   SyntaxKind.VirtualKeyword => 3,
                   SyntaxKind.SealedKeyword => 4,
                   SyntaxKind.OverrideKeyword => 5,
                   SyntaxKind.NewKeyword => 6,
                   SyntaxKind.PartialKeyword => 7,
                   SyntaxKind.ReadOnlyKeyword => 8,
                   SyntaxKind.VolatileKeyword => 9,
                   SyntaxKind.AsyncKeyword => 10,
                   SyntaxKind.ExternKeyword => 11,
                   SyntaxKind.UnsafeKeyword => 12,
                   SyntaxKind.RequiredKeyword => 13,
                   _ => 100,
               };
    }

    /// <summary>
    /// Gets the index of the given modifier
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <param name="syntaxKind">Modifier kind</param>
    /// <returns>The index, or -1 if not present</returns>
    private static int GetModifierIndex(SyntaxTokenList modifiers, SyntaxKind syntaxKind)
    {
        for (var modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
        {
            if (modifiers[modifierIndex].IsKind(syntaxKind))
            {
                return modifierIndex;
            }
        }

        return -1;
    }

    #endregion // Methods
}