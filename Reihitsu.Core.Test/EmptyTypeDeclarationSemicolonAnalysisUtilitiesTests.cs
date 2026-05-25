using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core;

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

    #endregion // Tests
}