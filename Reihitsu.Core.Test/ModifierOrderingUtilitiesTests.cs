using System.Linq;

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
        CollectionAssert.AreEqual(new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword },
                                  orderedModifiers.Select(obj => obj.Kind()).ToArray());
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
        CollectionAssert.AreEqual(new[] { precedingModifier, SyntaxKind.PartialKeyword },
                                  reorderedModifiers.Select(token => token.Kind()).ToArray());
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
        CollectionAssert.AreEqual(new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword },
                                  orderedModifiers.Select(obj => obj.Kind()).ToArray());
    }

    #endregion // Tests
}