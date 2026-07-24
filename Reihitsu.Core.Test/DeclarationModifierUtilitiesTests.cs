using System.Linq;

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
    /// Verifies that every accessibility modifier keyword, including the file-scoped <see langword="file"/>
    /// modifier, is recognized as declaring accessibility
    /// </summary>
    /// <param name="accessibilityModifier">Accessibility modifier kind</param>
    [TestMethod]
    [DataRow(SyntaxKind.PublicKeyword)]
    [DataRow(SyntaxKind.PrivateKeyword)]
    [DataRow(SyntaxKind.ProtectedKeyword)]
    [DataRow(SyntaxKind.InternalKeyword)]
    [DataRow(SyntaxKind.FileKeyword)]
    public void HasAccessibilityModifierReturnsTrueForEveryAccessibilityKeyword(SyntaxKind accessibilityModifier)
    {
        var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(accessibilityModifier));

        Assert.IsTrue(DeclarationModifierUtilities.HasAccessibilityModifier(modifiers));
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
    /// Verifies that adding an accessibility modifier to a documented member without modifiers keeps the
    /// leading trivia attached to the inserted modifier
    /// </summary>
    [TestMethod]
    public void AddAccessibilityModifierToDeclarationKeepsLeadingTriviaWhenNoModifiersExist()
    {
        var methodDeclaration = CoreSyntaxTestHelper.GetSingleMember<MethodDeclarationSyntax>("""
                                                                                              internal class Sample
                                                                                              {
                                                                                                  /// <summary>
                                                                                                  /// Doc.
                                                                                                  /// </summary>
                                                                                                  void DoWork()
                                                                                                  {
                                                                                                  }
                                                                                              }
                                                                                              """);

        var updatedDeclaration = (MethodDeclarationSyntax)DeclarationModifierUtilities.AddAccessibilityModifier(methodDeclaration, SyntaxKind.PrivateKeyword);

        Assert.AreEqual(SyntaxKind.PrivateKeyword, updatedDeclaration.Modifiers[0].Kind());
        Assert.Contains("/// <summary>", updatedDeclaration.Modifiers[0].LeadingTrivia.ToFullString());
        Assert.DoesNotContain("/// <summary>", updatedDeclaration.ReturnType.GetLeadingTrivia().ToFullString());
    }

    /// <summary>
    /// Verifies that adding an accessibility modifier to a documented member that already has another modifier
    /// keeps the leading trivia attached to the inserted modifier
    /// </summary>
    [TestMethod]
    public void AddAccessibilityModifierToDeclarationKeepsLeadingTriviaWhenAnotherModifierExists()
    {
        var methodDeclaration = CoreSyntaxTestHelper.GetSingleMember<MethodDeclarationSyntax>("""
                                                                                              internal class Sample
                                                                                              {
                                                                                                  /// <summary>
                                                                                                  /// Doc.
                                                                                                  /// </summary>
                                                                                                  static void DoWork()
                                                                                                  {
                                                                                                  }
                                                                                              }
                                                                                              """);

        var updatedDeclaration = (MethodDeclarationSyntax)DeclarationModifierUtilities.AddAccessibilityModifier(methodDeclaration, SyntaxKind.PrivateKeyword);

        Assert.AreEqual(SyntaxKind.PrivateKeyword, updatedDeclaration.Modifiers[0].Kind());
        Assert.AreEqual(SyntaxKind.StaticKeyword, updatedDeclaration.Modifiers[1].Kind());
        Assert.Contains("/// <summary>", updatedDeclaration.Modifiers[0].LeadingTrivia.ToFullString());
        Assert.DoesNotContain("/// <summary>", updatedDeclaration.Modifiers[1].LeadingTrivia.ToFullString());
    }

    /// <summary>
    /// Verifies that adding an accessibility modifier does not replace an existing file-scoped
    /// <see langword="file"/> modifier. <see langword="file"/> is not part of the replaceable accessibility set:
    /// silently replacing it would widen a file-local type's visibility (for example to <see langword="internal"/>)
    /// instead of leaving the combination in place
    /// </summary>
    [TestMethod]
    public void AddAccessibilityModifierDoesNotReplaceFileModifier()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          file class Sample
                                                                                          {
                                                                                          }
                                                                                          """);

        var updatedDeclaration = (ClassDeclarationSyntax)DeclarationModifierUtilities.AddAccessibilityModifier(classDeclaration, SyntaxKind.InternalKeyword);

        CollectionAssert.AreEqual(new[] { SyntaxKind.InternalKeyword, SyntaxKind.FileKeyword },
                                  updatedDeclaration.Modifiers.Select(obj => obj.Kind()).ToArray());
    }

    /// <summary>
    /// Verifies that a compound accessibility such as <see langword="protected"/> <see langword="internal"/> is
    /// inserted in the requested order, ahead of the existing modifiers and separated by single spaces
    /// </summary>
    [TestMethod]
    public void AddAccessibilityModifiersInsertsCompoundAccessibilityInOrder()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          partial class Sample
                                                                                          {
                                                                                          }
                                                                                          """);

        var updatedDeclaration = (ClassDeclarationSyntax)DeclarationModifierUtilities.AddAccessibilityModifiers(classDeclaration,
                                                                                                                [SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword]);

        CollectionAssert.AreEqual(new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword, SyntaxKind.PartialKeyword },
                                  updatedDeclaration.Modifiers.Select(obj => obj.Kind()).ToArray());
        Assert.AreEqual(" ", updatedDeclaration.Modifiers[0].TrailingTrivia.ToFullString());
        Assert.AreEqual(" ", updatedDeclaration.Modifiers[1].TrailingTrivia.ToFullString());
    }

    /// <summary>
    /// Verifies that adding a compound accessibility to a documented type keeps the leading trivia attached to
    /// the first inserted modifier
    /// </summary>
    [TestMethod]
    public void AddAccessibilityModifiersKeepsLeadingTriviaOnFirstModifier()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          /// <summary>
                                                                                          /// Doc.
                                                                                          /// </summary>
                                                                                          partial class Sample
                                                                                          {
                                                                                          }
                                                                                          """);

        var updatedDeclaration = (ClassDeclarationSyntax)DeclarationModifierUtilities.AddAccessibilityModifiers(classDeclaration,
                                                                                                                [SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword]);

        Assert.AreEqual(SyntaxKind.ProtectedKeyword, updatedDeclaration.Modifiers[0].Kind());
        Assert.Contains("/// <summary>", updatedDeclaration.Modifiers[0].LeadingTrivia.ToFullString());
        Assert.DoesNotContain("/// <summary>", updatedDeclaration.Modifiers[1].LeadingTrivia.ToFullString());
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
    /// Verifies that modifiers are returned for an incomplete member instead of throwing
    /// </summary>
    [TestMethod]
    public void GetModifiersReturnsModifiersForIncompleteMember()
    {
        var incompleteMember = CoreSyntaxTestHelper.GetSingleNode<IncompleteMemberSyntax>("""
                                                                                          public class Sample
                                                                                          {
                                                                                              public
                                                                                          }
                                                                                          """);

        var modifiers = DeclarationModifierUtilities.GetModifiers(incompleteMember);

        Assert.AreEqual(SyntaxKind.PublicKeyword, modifiers[0].Kind());
    }

    /// <summary>
    /// Verifies that modifiers are returned for an enum member instead of throwing
    /// </summary>
    [TestMethod]
    public void GetModifiersReturnsEmptyForEnumMember()
    {
        var enumMember = CoreSyntaxTestHelper.GetSingleNode<EnumMemberDeclarationSyntax>("""
                                                                                         internal enum Sample
                                                                                         {
                                                                                             Value
                                                                                         }
                                                                                         """);

        var modifiers = DeclarationModifierUtilities.GetModifiers(enumMember);

        Assert.AreEqual(0, modifiers.Count);
    }

    /// <summary>
    /// Verifies that modifiers can be applied to an incomplete member instead of throwing
    /// </summary>
    [TestMethod]
    public void WithModifiersAppliesTheProvidedModifiersToAnIncompleteMember()
    {
        var incompleteMember = CoreSyntaxTestHelper.GetSingleNode<IncompleteMemberSyntax>("""
                                                                                          public class Sample
                                                                                          {
                                                                                              public
                                                                                          }
                                                                                          """);
        var updatedModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

        var updatedDeclaration = DeclarationModifierUtilities.WithModifiers(incompleteMember, updatedModifiers);

        Assert.AreEqual(SyntaxKind.PrivateKeyword, DeclarationModifierUtilities.GetModifiers(updatedDeclaration)[0].Kind());
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