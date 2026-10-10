using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;

namespace Reihitsu.Formatter.Test.Unit.BlankLines;

/// <summary>
/// Tests for <see cref="BlankLineEditor.HasBlankLineBeforeIndex"/>, in particular the region-directive arm
/// that treats a <c>#region</c>/<c>#endregion</c> directive as resetting line-start status. These pin the
/// boundary of that arm — including that it deliberately does not distinguish an active directive from one
/// inside a skipped conditional branch — so a future change to the arm's predicate cannot narrow it silently
/// </summary>
[TestClass]
public class BlankLineEditorTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that an active <c>#region</c> directive between two line breaks is treated as line-start,
    /// satisfying the blank-line check
    /// </summary>
    [TestMethod]
    public void HasBlankLineBeforeIndexTreatsActiveRegionDirectiveAsLineStart()
    {
        // Arrange
        var directive = GetFirstTrivia("class C\n{\n    #region R\n    #endregion\n}\n", SyntaxKind.RegionDirectiveTrivia);
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n"), directive, SyntaxFactory.EndOfLine("\n"));

        // Act
        var actual = BlankLineEditor.HasBlankLineBeforeIndex(trivia, trivia.Count);

        // Assert
        Assert.IsTrue(actual);
    }

    /// <summary>
    /// Verifies that an active <c>#endregion</c> directive between two line breaks is treated as line-start,
    /// satisfying the blank-line check
    /// </summary>
    [TestMethod]
    public void HasBlankLineBeforeIndexTreatsEndRegionDirectiveAsLineStart()
    {
        // Arrange
        var directive = GetFirstTrivia("class C\n{\n    #region R\n    #endregion\n}\n", SyntaxKind.EndRegionDirectiveTrivia);
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n"), directive, SyntaxFactory.EndOfLine("\n"));

        // Act
        var actual = BlankLineEditor.HasBlankLineBeforeIndex(trivia, trivia.Count);

        // Assert
        Assert.IsTrue(actual);
    }

    /// <summary>
    /// Verifies that a <c>#region</c> directive inside a skipped conditional branch is still treated as
    /// line-start, because the predicate this arm mirrors, <see cref="Reihitsu.Core.SyntaxTriviaUtilities.IsRegionDirective"/>,
    /// deliberately does not inspect directive activity. This is the boundary a narrower "active region only"
    /// rewrite would silently break
    /// </summary>
    [TestMethod]
    public void HasBlankLineBeforeIndexTreatsInactiveRegionDirectiveAsLineStart()
    {
        // Arrange
        var directive = GetFirstTrivia("class C\n{\n#if false\n    #region R\n    #endregion // R\n#endif\n}\n", SyntaxKind.RegionDirectiveTrivia);
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n"), directive, SyntaxFactory.EndOfLine("\n"));

        // Act
        var actual = BlankLineEditor.HasBlankLineBeforeIndex(trivia, trivia.Count);

        // Assert
        Assert.IsTrue(actual);
    }

    /// <summary>
    /// Verifies that a non-region directive between two line breaks is not treated as line-start, so the
    /// region arm is not mistaken for "any directive resets line-start"
    /// </summary>
    [TestMethod]
    public void HasBlankLineBeforeIndexDoesNotTreatPragmaDirectiveAsLineStart()
    {
        // Arrange
        var directive = GetFirstTrivia("class C\n{\n#pragma warning disable CS0108\n    int A;\n}\n", SyntaxKind.PragmaWarningDirectiveTrivia);
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n"), directive, SyntaxFactory.EndOfLine("\n"));

        // Act
        var actual = BlankLineEditor.HasBlankLineBeforeIndex(trivia, trivia.Count);

        // Assert
        Assert.IsFalse(actual);
    }

    /// <summary>
    /// Parses the given source and returns the first descendant trivia of the specified kind
    /// </summary>
    /// <param name="source">The source text to parse</param>
    /// <param name="kind">The trivia kind to look for</param>
    /// <returns>The first matching trivia</returns>
    private static SyntaxTrivia GetFirstTrivia(string source, SyntaxKind kind)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();

        return root.DescendantTrivia(descendIntoTrivia: true).First(trivia => trivia.IsKind(kind));
    }

    #endregion // Methods
}