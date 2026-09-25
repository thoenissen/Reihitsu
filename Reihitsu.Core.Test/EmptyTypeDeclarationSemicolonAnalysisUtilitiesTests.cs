using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="EmptyTypeDeclarationSemicolonAnalysisUtilities"/>
/// </summary>
[TestClass]
public class EmptyTypeDeclarationSemicolonAnalysisUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that an empty declaration reports when the kind and language version match
    /// </summary>
    [TestMethod]
    public void ShouldReportReturnsTrueForEmptyMatchingDeclaration()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          {
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp1);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport(classDeclaration,
                                                                                 SyntaxKind.ClassDeclaration,
                                                                                 LanguageVersion.CSharp1);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that declarations with members are ignored
    /// </summary>
    [TestMethod]
    public void ShouldReportReturnsFalseWhenDeclarationContainsMembers()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          {
                                                                                              private int _value;
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp1);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport(classDeclaration,
                                                                                 SyntaxKind.ClassDeclaration,
                                                                                 LanguageVersion.CSharp1);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that unsupported language versions are ignored
    /// </summary>
    [TestMethod]
    public void ShouldReportReturnsFalseWhenLanguageVersionIsTooOld()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          {
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp1);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport(classDeclaration,
                                                                                 SyntaxKind.ClassDeclaration,
                                                                                 LanguageVersion.CSharp2);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that comments inside the declaration body prevent safe conversion
    /// </summary>
    [TestMethod]
    public void CanConvertSafelyReturnsFalseWhenBodyContainsCommentTrivia()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          { /* keep */ }
                                                                                          """,
                                                                                          LanguageVersion.CSharp1);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(classDeclaration,
                                                                                     SyntaxKind.ClassDeclaration,
                                                                                     LanguageVersion.CSharp1);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a comment between the type header and the open brace prevents safe conversion
    /// </summary>
    [TestMethod]
    public void CanConvertSafelyReturnsFalseWhenCommentPrecedesOpenBrace()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          // why this type is empty
                                                                                          {
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp1);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(classDeclaration,
                                                                                     SyntaxKind.ClassDeclaration,
                                                                                     LanguageVersion.CSharp1);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a comment between the type header and the open brace is treated as meaningful body trivia
    /// </summary>
    [TestMethod]
    public void HasMeaningfulBodyTriviaReturnsTrueWhenCommentPrecedesOpenBrace()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          // why this type is empty
                                                                                          {
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp1);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.HasMeaningfulBodyTrivia(classDeclaration);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a comment trailing the type header line prevents safe conversion
    /// </summary>
    [TestMethod]
    public void CanConvertSafelyReturnsFalseWhenCommentTrailsHeaderLine()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample // why this type is empty
                                                                                          {
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp1);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(classDeclaration,
                                                                                     SyntaxKind.ClassDeclaration,
                                                                                     LanguageVersion.CSharp1);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a comment trailing the type header line is treated as meaningful body trivia
    /// </summary>
    [TestMethod]
    public void HasMeaningfulBodyTriviaReturnsTrueWhenCommentTrailsHeaderLine()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample // why this type is empty
                                                                                          {
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp1);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.HasMeaningfulBodyTrivia(classDeclaration);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that an explicit language version below the minimum wins over a syntax tree that supports the conversion
    /// </summary>
    [TestMethod]
    public void ShouldReportWithExplicitVersionReturnsFalseBelowMinimumEvenWhenTreeSupportsIt()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          {
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp12);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport(classDeclaration,
                                                                                 SyntaxKind.ClassDeclaration,
                                                                                 LanguageVersion.CSharp12,
                                                                                 LanguageVersion.CSharp11);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that an explicit language version at the minimum wins over a syntax tree that predates the conversion
    /// </summary>
    [TestMethod]
    public void ShouldReportWithExplicitVersionReturnsTrueAtMinimumEvenWhenTreePredatesIt()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          {
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp11);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport(classDeclaration,
                                                                                 SyntaxKind.ClassDeclaration,
                                                                                 LanguageVersion.CSharp12,
                                                                                 LanguageVersion.CSharp12);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that an explicit language version below the record minimum prevents the conversion
    /// </summary>
    [TestMethod]
    public void CanConvertSafelyWithExplicitVersionReturnsFalseBelowRecordMinimum()
    {
        var recordDeclaration = CoreSyntaxTestHelper.GetSingleNode<RecordDeclarationSyntax>("""
                                                                                            record Sample
                                                                                            {
                                                                                            }
                                                                                            """,
                                                                                            LanguageVersion.CSharp9);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(recordDeclaration,
                                                                                     SyntaxKind.RecordDeclaration,
                                                                                     LanguageVersion.CSharp9,
                                                                                     LanguageVersion.CSharp8);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that an explicit language version at the minimum still honors the meaningful-trivia guard
    /// </summary>
    [TestMethod]
    public void CanConvertSafelyWithExplicitVersionReturnsFalseWhenBodyContainsCommentTrivia()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          {
                                                                                              // keep
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp12);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(classDeclaration,
                                                                                     SyntaxKind.ClassDeclaration,
                                                                                     LanguageVersion.CSharp12,
                                                                                     LanguageVersion.CSharp12);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that an explicit language version at the minimum converts an empty declaration without trivia
    /// </summary>
    [TestMethod]
    public void CanConvertSafelyWithExplicitVersionReturnsTrueAtMinimum()
    {
        var classDeclaration = CoreSyntaxTestHelper.GetSingleNode<ClassDeclarationSyntax>("""
                                                                                          class Sample
                                                                                          {
                                                                                          }
                                                                                          """,
                                                                                          LanguageVersion.CSharp12);

        var result = EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(classDeclaration,
                                                                                     SyntaxKind.ClassDeclaration,
                                                                                     LanguageVersion.CSharp12,
                                                                                     LanguageVersion.CSharp12);

        Assert.IsTrue(result);
    }

    #endregion // Tests
}