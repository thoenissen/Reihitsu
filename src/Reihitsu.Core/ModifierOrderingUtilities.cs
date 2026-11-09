using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Core;

/// <summary>
/// Helper methods for modifier ordering analyzers and code fixes
/// </summary>
public static class ModifierOrderingUtilities
{
    #region Methods

    /// <summary>
    /// Tries to find the token that violates RH7105
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <param name="diagnosticToken">Diagnostic token</param>
    /// <returns><see langword="true"/> if the modifier order is invalid</returns>
    public static bool TryGetRh7105Violation(SyntaxTokenList modifiers, out SyntaxToken diagnosticToken)
    {
        diagnosticToken = default;

        if (modifiers.Count < 2)
        {
            return false;
        }

        var orderedModifiers = OrderModifiersForRh7105(modifiers);

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
    /// Orders modifiers according to RH7105
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns>The ordered modifiers</returns>
    public static SyntaxTokenList OrderModifiersForRh7105(SyntaxTokenList modifiers)
    {
        return SyntaxFactory.TokenList(modifiers.Select((modifier, modifierIndex) => new
                                                                                     {
                                                                                         Modifier = modifier,
                                                                                         ModifierIndex = modifierIndex,
                                                                                     })
                                                .OrderBy(obj => GetRh7105Rank(obj.Modifier))
                                                .ThenBy(obj => obj.ModifierIndex)
                                                .Select(obj => obj.Modifier));
    }

    /// <summary>
    /// Writes a declaration's modifiers in a new order (SHARED-MODORDER-002). The trivia in front of the first written
    /// modifier stays in front of the new first modifier, and each modifier is followed by a single space, so the
    /// modifiers stand on one line. A comment, a directive, or code that a directive excludes between two modifiers
    /// keeps its place between the modifiers written in front of it and those written behind it, and one between the
    /// last modifier and the token behind them keeps its place in front of that token; a modifier that the new order
    /// adds at such a place is written behind it (RH2001-009). The modifiers cannot be written so when the new order
    /// puts a modifier written in front of such trivia behind one written behind it, because the trivia would have
    /// to move next to another keyword
    /// </summary>
    /// <param name="modifiers">Modifiers as written</param>
    /// <param name="nextToken">Token behind the modifiers</param>
    /// <param name="orderedModifiers">
    /// The modifiers in the new order: every written modifier, matched to the written one of its kind in the order
    /// they were written, and every modifier the new order adds
    /// </param>
    /// <param name="rewrittenModifiers">The rewritten modifiers</param>
    /// <param name="nextTokenLeadingTrivia">The leading trivia of the token behind the modifiers</param>
    /// <returns><see langword="true"/> if the modifiers could be written in the new order</returns>
    public static bool TryWriteModifiersInOrder(SyntaxTokenList modifiers, SyntaxToken nextToken, SyntaxTokenList orderedModifiers, out SyntaxTokenList rewrittenModifiers, out SyntaxTriviaList nextTokenLeadingTrivia)
    {
        rewrittenModifiers = default;
        nextTokenLeadingTrivia = nextToken.LeadingTrivia;

        if (TryGetWrittenIndices(modifiers, orderedModifiers, out var writtenIndices) == false)
        {
            return false;
        }

        // The gap whose trivia is written behind a modifier: gap g lies between the written modifiers g - 1 and g, and
        // the last gap between the last modifier and the token behind them
        var gapBehindModifier = new int?[orderedModifiers.Count];

        for (var gapIndex = 1; gapIndex <= modifiers.Count; gapIndex++)
        {
            if (HoldsCommentOrDirective(modifiers[gapIndex - 1].TrailingTrivia) == false
                && HoldsCommentOrDirective(GetLeadingTriviaBehindGap(modifiers, nextToken, gapIndex)) == false)
            {
                continue;
            }

            var lastIndexInFront = -1;
            var firstIndexBehind = orderedModifiers.Count;

            for (var orderedIndex = 0; orderedIndex < orderedModifiers.Count; orderedIndex++)
            {
                if (writtenIndices[orderedIndex] < 0)
                {
                    continue;
                }

                if (writtenIndices[orderedIndex] < gapIndex)
                {
                    lastIndexInFront = orderedIndex;
                }
                else if (orderedIndex < firstIndexBehind)
                {
                    firstIndexBehind = orderedIndex;
                }
            }

            if (firstIndexBehind < lastIndexInFront)
            {
                return false;
            }

            gapBehindModifier[lastIndexInFront] = gapIndex;
        }

        var rewrittenTokens = new List<SyntaxToken>(orderedModifiers.Count);
        var leadingTrivia = modifiers.Count > 0
                                ? modifiers[0].LeadingTrivia
                                : nextToken.LeadingTrivia;

        if (modifiers.Count == 0 && orderedModifiers.Count > 0)
        {
            nextTokenLeadingTrivia = default;
        }

        for (var orderedIndex = 0; orderedIndex < orderedModifiers.Count; orderedIndex++)
        {
            var trailingTrivia = SyntaxFactory.TriviaList(SyntaxFactory.Space);
            var nextLeadingTrivia = default(SyntaxTriviaList);

            if (gapBehindModifier[orderedIndex] is { } gapIndex)
            {
                trailingTrivia = modifiers[gapIndex - 1].TrailingTrivia;

                if (orderedIndex + 1 < orderedModifiers.Count)
                {
                    nextLeadingTrivia = GetLeadingTriviaBehindGap(modifiers, nextToken, gapIndex);

                    if (gapIndex == modifiers.Count)
                    {
                        nextTokenLeadingTrivia = default;
                    }
                }
            }

            rewrittenTokens.Add(orderedModifiers[orderedIndex].WithLeadingTrivia(leadingTrivia)
                                                              .WithTrailingTrivia(trailingTrivia));

            leadingTrivia = nextLeadingTrivia;
        }

        rewrittenModifiers = SyntaxFactory.TokenList(rewrittenTokens);

        return true;
    }

    /// <summary>
    /// Tries to find the token that violates RH7106
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <param name="diagnosticToken">Diagnostic token</param>
    /// <returns><see langword="true"/> if the compound accessibility order is invalid</returns>
    public static bool TryGetRh7106Violation(SyntaxTokenList modifiers, out SyntaxToken diagnosticToken)
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
    /// Reorders only the compound accessibility pair for RH7106
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns>The updated modifiers</returns>
    public static SyntaxTokenList OrderModifiersForRh7106(SyntaxTokenList modifiers)
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
    /// Gets the ranking used by RH7105
    /// </summary>
    /// <param name="modifier">Modifier token</param>
    /// <returns>The ordering rank</returns>
    /// <remarks>
    /// The two highest ranks are dictated by the C# grammar rather than by style: <see langword="ref"/> has to
    /// follow every other modifier and <see langword="partial"/> has to be last, so a ref struct reads
    /// <c>public readonly ref partial struct</c>.
    /// <para>
    /// The default rank sorts behind both of them, which is already correct for the two unranked modifiers that
    /// reach this switch: <see langword="const"/> and <see langword="fixed"/> also have to come last on the
    /// declarations that allow them (<c>public const int Value = 1</c>, <c>public unsafe fixed int Buffer[4]</c>),
    /// and neither can combine with <see langword="ref"/> or <see langword="partial"/>. A modifier that must
    /// precede any ranked one, on the other hand, would be reordered into a declaration that no longer compiles,
    /// so new modifiers have to be checked against the grammar and given an explicit rank where the default does
    /// not hold
    /// </para>
    /// </remarks>
    private static int GetRh7105Rank(SyntaxToken modifier)
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
                   SyntaxKind.ReadOnlyKeyword => 7,
                   SyntaxKind.VolatileKeyword => 8,
                   SyntaxKind.AsyncKeyword => 9,
                   SyntaxKind.ExternKeyword => 10,
                   SyntaxKind.UnsafeKeyword => 11,
                   SyntaxKind.RequiredKeyword => 12,
                   SyntaxKind.RefKeyword => 13,
                   SyntaxKind.PartialKeyword => 14,
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

    /// <summary>
    /// Matches every modifier of the new order to the written modifier of its kind, in the order they were written
    /// </summary>
    /// <param name="modifiers">Modifiers as written</param>
    /// <param name="orderedModifiers">Modifiers in the new order</param>
    /// <param name="writtenIndices">
    /// The index of the written modifier for each modifier of the new order, -1 for a modifier the new order adds
    /// </param>
    /// <returns><see langword="true"/> if the new order holds every written modifier</returns>
    private static bool TryGetWrittenIndices(SyntaxTokenList modifiers, SyntaxTokenList orderedModifiers, out int[] writtenIndices)
    {
        writtenIndices = new int[orderedModifiers.Count];

        var isMatched = new bool[modifiers.Count];

        for (var orderedIndex = 0; orderedIndex < orderedModifiers.Count; orderedIndex++)
        {
            writtenIndices[orderedIndex] = -1;

            for (var writtenIndex = 0; writtenIndex < modifiers.Count; writtenIndex++)
            {
                if (isMatched[writtenIndex] == false
                    && modifiers[writtenIndex].RawKind == orderedModifiers[orderedIndex].RawKind)
                {
                    isMatched[writtenIndex] = true;
                    writtenIndices[orderedIndex] = writtenIndex;

                    break;
                }
            }
        }

        return isMatched.All(obj => obj);
    }

    /// <summary>
    /// Gets the leading trivia of the token behind a gap between the written modifiers: the written modifier behind
    /// it, or the token behind the modifiers for the last gap
    /// </summary>
    /// <param name="modifiers">Modifiers as written</param>
    /// <param name="nextToken">Token behind the modifiers</param>
    /// <param name="gapIndex">Index of the gap, which lies in front of the written modifier of that index</param>
    /// <returns>The leading trivia</returns>
    private static SyntaxTriviaList GetLeadingTriviaBehindGap(SyntaxTokenList modifiers, SyntaxToken nextToken, int gapIndex)
    {
        return gapIndex < modifiers.Count
                   ? modifiers[gapIndex].LeadingTrivia
                   : nextToken.LeadingTrivia;
    }

    /// <summary>
    /// Determines whether the trivia holds a comment, a directive, or code that a directive excludes
    /// </summary>
    /// <param name="triviaList">Trivia</param>
    /// <returns><see langword="true"/> if the trivia holds a comment, a directive, or excluded code</returns>
    private static bool HoldsCommentOrDirective(SyntaxTriviaList triviaList)
    {
        return triviaList.Any(trivia => SyntaxNodeUtilities.IsCommentOrDirective(trivia)
                                        || trivia.IsKind(SyntaxKind.DisabledTextTrivia));
    }

    #endregion // Methods
}