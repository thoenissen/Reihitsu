using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="AdjacentTokenSpacingUtilities"/>
/// </summary>
[TestClass]
public class AdjacentTokenSpacingUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that a colon spaced on both sides is reported as spaced on both sides
    /// </summary>
    [TestMethod]
    public void DetermineLineBreakTolerantSpacingReturnsBothSidesSpacedWhenBothSidesHaveASpace()
    {
        var (token, sourceText) = GetColonToken("class C : System.IDisposable { }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineLineBreakTolerantSpacing(token, sourceText);

        Assert.IsTrue(hasLeadingSpace);
        Assert.IsTrue(hasTrailingSpace);
    }

    /// <summary>
    /// Verifies that a colon without a leading space is reported as unspaced on the leading side
    /// </summary>
    [TestMethod]
    public void DetermineLineBreakTolerantSpacingReturnsLeadingSideUnspacedWhenLeadingSpaceIsMissing()
    {
        var (token, sourceText) = GetColonToken("class C: System.IDisposable { }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineLineBreakTolerantSpacing(token, sourceText);

        Assert.IsFalse(hasLeadingSpace);
        Assert.IsTrue(hasTrailingSpace);
    }

    /// <summary>
    /// Verifies that a colon without a trailing space is reported as unspaced on the trailing side
    /// </summary>
    [TestMethod]
    public void DetermineLineBreakTolerantSpacingReturnsTrailingSideUnspacedWhenTrailingSpaceIsMissing()
    {
        var (token, sourceText) = GetColonToken("class C :System.IDisposable { }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineLineBreakTolerantSpacing(token, sourceText);

        Assert.IsTrue(hasLeadingSpace);
        Assert.IsFalse(hasTrailingSpace);
    }

    /// <summary>
    /// Verifies that a colon starting a continuation line is treated as spaced on the leading side, because a
    /// line break already satisfies the spacing requirement
    /// </summary>
    [TestMethod]
    public void DetermineLineBreakTolerantSpacingTreatsLeadingLineBreakAsSpaced()
    {
        var (token, sourceText) = GetColonToken("class C\n    : System.IDisposable { }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineLineBreakTolerantSpacing(token, sourceText);

        Assert.IsTrue(hasLeadingSpace);
        Assert.IsTrue(hasTrailingSpace);
    }

    /// <summary>
    /// Verifies that a colon whose neighbour starts on the next line is treated as spaced on the trailing
    /// side, because a line break already satisfies the spacing requirement
    /// </summary>
    [TestMethod]
    public void DetermineLineBreakTolerantSpacingTreatsTrailingLineBreakAsSpaced()
    {
        var (token, sourceText) = GetColonToken("class C :\n    System.IDisposable { }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineLineBreakTolerantSpacing(token, sourceText);

        Assert.IsTrue(hasLeadingSpace);
        Assert.IsTrue(hasTrailingSpace);
    }

    /// <summary>
    /// Verifies that an unspaced dot is reported as unspaced on both sides
    /// </summary>
    [TestMethod]
    public void DetermineSameLineAdjacentSpacingReturnsBothSidesUnspacedWhenNoSpaceIsPresent()
    {
        var (token, sourceText) = GetDotToken("class C { void M() { _ = string.Empty; } }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineSameLineAdjacentSpacing(token, sourceText);

        Assert.IsFalse(hasLeadingSpace);
        Assert.IsFalse(hasTrailingSpace);
    }

    /// <summary>
    /// Verifies that a dot with an extraneous leading space is reported as spaced on the leading side only
    /// </summary>
    [TestMethod]
    public void DetermineSameLineAdjacentSpacingReturnsLeadingSideSpacedWhenLeadingSpaceIsPresent()
    {
        var (token, sourceText) = GetDotToken("class C { void M() { _ = string .Empty; } }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineSameLineAdjacentSpacing(token, sourceText);

        Assert.IsTrue(hasLeadingSpace);
        Assert.IsFalse(hasTrailingSpace);
    }

    /// <summary>
    /// Verifies that a dot with an extraneous trailing space is reported as spaced on the trailing side only
    /// </summary>
    [TestMethod]
    public void DetermineSameLineAdjacentSpacingReturnsTrailingSideSpacedWhenTrailingSpaceIsPresent()
    {
        var (token, sourceText) = GetDotToken("class C { void M() { _ = string. Empty; } }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineSameLineAdjacentSpacing(token, sourceText);

        Assert.IsFalse(hasLeadingSpace);
        Assert.IsTrue(hasTrailingSpace);
    }

    /// <summary>
    /// Verifies that a dot whose leading side spans a line break is never reported as spaced on that side,
    /// even though the dot itself is unspaced, because a line-broken side can never carry the offending
    /// whitespace
    /// </summary>
    [TestMethod]
    public void DetermineSameLineAdjacentSpacingIgnoresLeadingLineBreak()
    {
        var (token, sourceText) = GetDotToken("class C { void M() { _ = string\n    .Empty; } }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineSameLineAdjacentSpacing(token, sourceText);

        Assert.IsFalse(hasLeadingSpace);
        Assert.IsFalse(hasTrailingSpace);
    }

    /// <summary>
    /// Verifies that a tab is recognized as adjacent spacing, matching the analyzer's tolerance for both
    /// spaces and tabs
    /// </summary>
    [TestMethod]
    public void DetermineSameLineAdjacentSpacingRecognizesTabs()
    {
        var (token, sourceText) = GetDotToken("class C { void M() { _ = string\t.Empty; } }");

        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineSameLineAdjacentSpacing(token, sourceText);

        Assert.IsTrue(hasLeadingSpace);
        Assert.IsFalse(hasTrailingSpace);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the source and returns its base-list colon token together with the backing source text
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The colon token and its backing source text</returns>
    private static (SyntaxToken Token, Microsoft.CodeAnalysis.Text.SourceText SourceText) GetColonToken(string source)
    {
        var root = CoreSyntaxTestHelper.ParseCompilationUnit(source);
        var token = root.DescendantTokens().Single(currentToken => currentToken.IsKind(SyntaxKind.ColonToken));

        return (token, root.SyntaxTree.GetText());
    }

    /// <summary>
    /// Parses the source and returns its dot token together with the backing source text
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The dot token and its backing source text</returns>
    private static (SyntaxToken Token, Microsoft.CodeAnalysis.Text.SourceText SourceText) GetDotToken(string source)
    {
        var root = CoreSyntaxTestHelper.ParseCompilationUnit(source);
        var token = root.DescendantTokens().Single(currentToken => currentToken.IsKind(SyntaxKind.DotToken));

        return (token, root.SyntaxTree.GetText());
    }

    #endregion // Methods
}