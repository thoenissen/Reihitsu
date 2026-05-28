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