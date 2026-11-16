using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="ModifierOrderingUtilities"/>
/// </summary>
[TestClass]
public class ModifierOrderingUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that RH7105 detects and fixes misplaced modifiers
    /// </summary>
    [TestMethod]
    public void Rh7105HelpersDetectAndReorderMisplacedModifiers()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          static public class Sample
                                                                                          {
                                                                                          }
                                                                                          """);

        var hasViolation = ModifierOrderingUtilities.TryGetRh7105Violation(classDeclaration.Modifiers, out var diagnosticToken);
        var orderedModifiers = ModifierOrderingUtilities.OrderModifiersForRh7105(classDeclaration.Modifiers);

        Assert.IsTrue(hasViolation);
        Assert.AreEqual(SyntaxKind.StaticKeyword, diagnosticToken.Kind());
        Assert.AreSequenceEqual([SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword], orderedModifiers.Select(obj => obj.Kind()).ToArray());
    }

    /// <summary>
    /// Verifies that RH7105 ignores already ordered modifiers
    /// </summary>
    [TestMethod]
    public void TryGetRh7105ViolationReturnsFalseForOrderedModifiers()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          public static class Sample
                                                                                          {
                                                                                          }
                                                                                          """);

        var hasViolation = ModifierOrderingUtilities.TryGetRh7105Violation(classDeclaration.Modifiers, out _);

        Assert.IsFalse(hasViolation);
    }

    /// <summary>
    /// Verifies that RH7105 ranks <see langword="partial"/> after the modifiers that must precede it, so idiomatic
    /// declarations such as <c>readonly partial</c> or <c>async partial</c> are neither flagged nor reordered into a
    /// non-compiling sequence (C# requires <see langword="partial"/> to be the last modifier)
    /// </summary>
    /// <param name="precedingModifier">Modifier that must appear before <see langword="partial"/></param>
    [TestMethod]
    [DataRow(SyntaxKind.ReadOnlyKeyword)]
    [DataRow(SyntaxKind.VolatileKeyword)]
    [DataRow(SyntaxKind.AsyncKeyword)]
    [DataRow(SyntaxKind.ExternKeyword)]
    [DataRow(SyntaxKind.UnsafeKeyword)]
    [DataRow(SyntaxKind.RequiredKeyword)]
    public void Rh7105RanksPartialAfterOtherModifiers(SyntaxKind precedingModifier)
    {
        var orderedModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(precedingModifier),
                                                       SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        Assert.IsFalse(ModifierOrderingUtilities.TryGetRh7105Violation(orderedModifiers, out _));

        var misorderedModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword),
                                                          SyntaxFactory.Token(precedingModifier));

        var hasViolation = ModifierOrderingUtilities.TryGetRh7105Violation(misorderedModifiers, out var diagnosticToken);
        var reorderedModifiers = ModifierOrderingUtilities.OrderModifiersForRh7105(misorderedModifiers);

        Assert.IsTrue(hasViolation);
        Assert.AreEqual(SyntaxKind.PartialKeyword, diagnosticToken.Kind());
        Assert.AreSequenceEqual([precedingModifier, SyntaxKind.PartialKeyword], reorderedModifiers.Select(token => token.Kind()).ToArray());
    }

    /// <summary>
    /// Verifies that RH7105 ranks <see langword="ref"/> before <see langword="partial"/>, so the idiomatic
    /// declaration <c>ref partial struct</c> is neither flagged nor reordered into the non-compiling sequence
    /// <c>partial ref struct</c> (C# requires <see langword="ref"/> to precede <see langword="partial"/>)
    /// </summary>
    [TestMethod]
    public void Rh7105RanksRefBeforePartial()
    {
        var orderedModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword),
                                                       SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        Assert.IsFalse(ModifierOrderingUtilities.TryGetRh7105Violation(orderedModifiers, out _));

        var misorderedModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword),
                                                          SyntaxFactory.Token(SyntaxKind.RefKeyword));

        var hasViolation = ModifierOrderingUtilities.TryGetRh7105Violation(misorderedModifiers, out var diagnosticToken);
        var reorderedModifiers = ModifierOrderingUtilities.OrderModifiersForRh7105(misorderedModifiers);

        Assert.IsTrue(hasViolation);
        Assert.AreEqual(SyntaxKind.PartialKeyword, diagnosticToken.Kind());
        Assert.AreSequenceEqual([SyntaxKind.RefKeyword, SyntaxKind.PartialKeyword], reorderedModifiers.Select(token => token.Kind()).ToArray());
    }

    /// <summary>
    /// Verifies that RH7105 ranks <see langword="ref"/> after the modifiers that must precede it, because C# only
    /// accepts <see langword="ref"/> as the last modifier before <see langword="partial"/> and the type keyword
    /// </summary>
    /// <param name="precedingModifier">Modifier that must appear before <see langword="ref"/></param>
    [TestMethod]
    [DataRow(SyntaxKind.PublicKeyword)]
    [DataRow(SyntaxKind.FileKeyword)]
    [DataRow(SyntaxKind.ReadOnlyKeyword)]
    [DataRow(SyntaxKind.UnsafeKeyword)]
    public void Rh7105RanksRefAfterOtherModifiers(SyntaxKind precedingModifier)
    {
        var orderedModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(precedingModifier),
                                                       SyntaxFactory.Token(SyntaxKind.RefKeyword));

        Assert.IsFalse(ModifierOrderingUtilities.TryGetRh7105Violation(orderedModifiers, out _));

        var misorderedModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword),
                                                          SyntaxFactory.Token(precedingModifier));

        var hasViolation = ModifierOrderingUtilities.TryGetRh7105Violation(misorderedModifiers, out var diagnosticToken);
        var reorderedModifiers = ModifierOrderingUtilities.OrderModifiersForRh7105(misorderedModifiers);

        Assert.IsTrue(hasViolation);
        Assert.AreEqual(SyntaxKind.RefKeyword, diagnosticToken.Kind());
        Assert.AreSequenceEqual([precedingModifier, SyntaxKind.RefKeyword], reorderedModifiers.Select(token => token.Kind()).ToArray());
    }

    /// <summary>
    /// Verifies that RH7105 accepts the full modifier sequence of a readonly ref partial struct, which is the only
    /// order the C# parser accepts for such a declaration
    /// </summary>
    [TestMethod]
    public void TryGetRh7105ViolationReturnsFalseForReadonlyRefPartialStruct()
    {
        var structDeclaration = CoreSyntaxTestHelper.GetSingleNode<StructDeclarationSyntax>("""
                                                                                            public readonly ref partial struct Sample
                                                                                            {
                                                                                            }
                                                                                            """);

        var hasViolation = ModifierOrderingUtilities.TryGetRh7105Violation(structDeclaration.Modifiers, out _);

        Assert.IsFalse(hasViolation);
    }

    /// <summary>
    /// Verifies that RH7106 detects and fixes internal protected ordering
    /// </summary>
    [TestMethod]
    public void Rh7106HelpersDetectAndReorderInternalProtectedModifiers()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          internal protected class Sample
                                                                                          {
                                                                                          }
                                                                                          """);

        var hasViolation = ModifierOrderingUtilities.TryGetRh7106Violation(classDeclaration.Modifiers, out var diagnosticToken);
        var orderedModifiers = ModifierOrderingUtilities.OrderModifiersForRh7106(classDeclaration.Modifiers);

        Assert.IsTrue(hasViolation);
        Assert.AreEqual(SyntaxKind.ProtectedKeyword, diagnosticToken.Kind());
        Assert.AreSequenceEqual([SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword], orderedModifiers.Select(obj => obj.Kind()).ToArray());
    }

    /// <summary>
    /// Verifies that RH7105 ranks <see langword="closed"/> in the position of <see langword="sealed"/>: behind the
    /// accessibility and <see langword="static"/>, and in front of <see langword="new"/>, <see langword="unsafe"/>, and
    /// <see langword="partial"/>
    /// </summary>
    /// <param name="first">Modifier the canonical order puts first</param>
    /// <param name="second">Modifier the canonical order puts second</param>
    [TestMethod]
    [Requirement("RH7105-001")]
    [DataRow(SyntaxKind.PublicKeyword, SyntaxKind.ClosedKeyword)]
    [DataRow(SyntaxKind.FileKeyword, SyntaxKind.ClosedKeyword)]
    [DataRow(SyntaxKind.StaticKeyword, SyntaxKind.ClosedKeyword)]
    [DataRow(SyntaxKind.ClosedKeyword, SyntaxKind.NewKeyword)]
    [DataRow(SyntaxKind.ClosedKeyword, SyntaxKind.UnsafeKeyword)]
    [DataRow(SyntaxKind.ClosedKeyword, SyntaxKind.RefKeyword)]
    [DataRow(SyntaxKind.ClosedKeyword, SyntaxKind.PartialKeyword)]
    public void Rh7105RanksClosedInThePositionOfSealed(SyntaxKind first, SyntaxKind second)
    {
        AssertRh7105Order(SyntaxFactory.Token(first), SyntaxFactory.Token(second));
    }

    /// <summary>
    /// Verifies that RH7105 ranks the preview <see langword="safe"/> in the position of <see langword="unsafe"/>: behind
    /// the accessibility, <see langword="static"/>, <see langword="new"/>, <see langword="readonly"/>,
    /// <see langword="async"/>, and <see langword="extern"/>, and in front of <see langword="required"/>,
    /// <see langword="ref"/>, and <see langword="partial"/>. The <see langword="safe"/> token is parsed from preview
    /// source, because its syntax kind is an experimental API
    /// </summary>
    /// <param name="otherModifier">Modifier paired with <see langword="safe"/></param>
    /// <param name="safeIsFirst">Whether the canonical order puts <see langword="safe"/> in front of the other modifier</param>
    [TestMethod]
    [Requirement("RH7105-001")]
    [DataRow(SyntaxKind.PublicKeyword, false)]
    [DataRow(SyntaxKind.StaticKeyword, false)]
    [DataRow(SyntaxKind.NewKeyword, false)]
    [DataRow(SyntaxKind.ReadOnlyKeyword, false)]
    [DataRow(SyntaxKind.AsyncKeyword, false)]
    [DataRow(SyntaxKind.ExternKeyword, false)]
    [DataRow(SyntaxKind.RequiredKeyword, true)]
    [DataRow(SyntaxKind.RefKeyword, true)]
    [DataRow(SyntaxKind.PartialKeyword, true)]
    public void Rh7105RanksSafeInThePositionOfUnsafe(SyntaxKind otherModifier, bool safeIsFirst)
    {
        var safeModifier = CoreSyntaxTestHelper.GetSingleMember<MethodDeclarationSyntax>("""
                                                                                         class Sample
                                                                                         {
                                                                                             safe void M()
                                                                                             {
                                                                                             }
                                                                                         }
                                                                                         """,
                                                                                         LanguageVersion.Preview)
                                               .Modifiers
                                               .Single();
        var other = SyntaxFactory.Token(otherModifier);

        Assert.AreEqual("safe", safeModifier.Text);

        if (safeIsFirst)
        {
            AssertRh7105Order(safeModifier, other);
        }
        else
        {
            AssertRh7105Order(other, safeModifier);
        }
    }

    /// <summary>
    /// Verifies that RH7105 keeps ranking <see langword="const"/> and <see langword="fixed"/>, which the order does not
    /// name, behind every named modifier
    /// </summary>
    /// <param name="first">Modifier the canonical order puts first</param>
    /// <param name="second">Modifier the canonical order puts second</param>
    [TestMethod]
    [Requirement("RH7105-001")]
    [DataRow(SyntaxKind.PartialKeyword, SyntaxKind.ConstKeyword)]
    [DataRow(SyntaxKind.RefKeyword, SyntaxKind.ConstKeyword)]
    [DataRow(SyntaxKind.PartialKeyword, SyntaxKind.FixedKeyword)]
    [DataRow(SyntaxKind.RefKeyword, SyntaxKind.FixedKeyword)]
    public void Rh7105RanksUnnamedModifiersLast(SyntaxKind first, SyntaxKind second)
    {
        AssertRh7105Order(SyntaxFactory.Token(first), SyntaxFactory.Token(second));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Asserts that RH7105 accepts the two modifiers in the given order, and that it reports the reverse order on its
    /// first modifier and reorders it to the given order
    /// </summary>
    /// <param name="first">Modifier the canonical order puts first</param>
    /// <param name="second">Modifier the canonical order puts second</param>
    private static void AssertRh7105Order(SyntaxToken first, SyntaxToken second)
    {
        var orderedModifiers = SyntaxFactory.TokenList(first, second);

        Assert.IsFalse(ModifierOrderingUtilities.TryGetRh7105Violation(orderedModifiers, out _));

        var misorderedModifiers = SyntaxFactory.TokenList(second, first);

        var hasViolation = ModifierOrderingUtilities.TryGetRh7105Violation(misorderedModifiers, out var diagnosticToken);
        var reorderedModifiers = ModifierOrderingUtilities.OrderModifiersForRh7105(misorderedModifiers);

        Assert.IsTrue(hasViolation);
        Assert.AreEqual(second.Text, diagnosticToken.Text);
        Assert.AreSequenceEqual([first.Text, second.Text], reorderedModifiers.Select(token => token.Text).ToArray());
    }

    #endregion // Methods
}