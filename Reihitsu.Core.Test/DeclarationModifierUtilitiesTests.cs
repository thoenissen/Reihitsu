using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="DeclarationModifierUtilities"/>
/// </summary>
[TestClass]
public class DeclarationModifierUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that accessibility modifiers are detected correctly
    /// </summary>
    [TestMethod]
    public void HasAccessibilityModifierReturnsExpectedResult()
    {
        var withAccessibility = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                                        SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        var withoutAccessibility = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

        Assert.IsTrue(DeclarationModifierUtilities.HasAccessibilityModifier(withAccessibility));
        Assert.IsFalse(DeclarationModifierUtilities.HasAccessibilityModifier(withoutAccessibility));
    }

    /// <summary>
    /// Verifies that replacing the accessibility modifier preserves its trailing trivia
    /// </summary>
    [TestMethod]
    public void AddAccessibilityModifierReplacesExistingAccessibilityAndPreservesTrivia()
    {
        var fieldDeclaration = CoreSyntaxTestHelper.GetSingleMember<FieldDeclarationSyntax>("""
                                                                                            internal class Sample
                                                                                            {
                                                                                                private static int _value;
                                                                                            }
                                                                                            """);

        var updatedModifiers = DeclarationModifierUtilities.AddAccessibilityModifier(fieldDeclaration.Modifiers, SyntaxKind.PublicKeyword);

        Assert.AreEqual(SyntaxKind.PublicKeyword, updatedModifiers[0].Kind());
        Assert.AreEqual(" ", updatedModifiers[0].TrailingTrivia.ToFullString());
        Assert.AreEqual(SyntaxKind.StaticKeyword, updatedModifiers[1].Kind());
    }

    /// <summary>
    /// Verifies that modifiers can be extracted from supported declarations
    /// </summary>
    [TestMethod]
    public void GetModifiersReturnsDeclarationModifiers()
    {
        var propertyDeclaration = CoreSyntaxTestHelper.GetSingleMember<PropertyDeclarationSyntax>("""
                                                                                                  internal class Sample
                                                                                                  {
                                                                                                      protected internal int Value { get; }
                                                                                                  }
                                                                                                  """);

        var modifiers = DeclarationModifierUtilities.GetModifiers(propertyDeclaration);

        Assert.AreEqual(SyntaxKind.ProtectedKeyword, modifiers[0].Kind());
        Assert.AreEqual(SyntaxKind.InternalKeyword, modifiers[1].Kind());
    }

    /// <summary>
    /// Verifies that updated modifiers are applied to the requested declaration
    /// </summary>
    [TestMethod]
    public void WithModifiersAppliesTheProvidedModifiersToTheDeclaration()
    {
        var propertyDeclaration = CoreSyntaxTestHelper.GetSingleMember<PropertyDeclarationSyntax>("""
                                                                                                  internal class Sample
                                                                                                  {
                                                                                                      private int Value { get; }
                                                                                                  }
                                                                                                  """);
        var updatedModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxFactory.Space),
                                                       SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTrailingTrivia(SyntaxFactory.Space));

        var updatedDeclaration = (PropertyDeclarationSyntax)DeclarationModifierUtilities.WithModifiers(propertyDeclaration, updatedModifiers);

        Assert.AreEqual("public static int Value { get; }", updatedDeclaration.NormalizeWhitespace().ToFullString());
    }

    #endregion // Tests
}