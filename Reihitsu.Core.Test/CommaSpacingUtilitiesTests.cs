using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="CommaSpacingUtilities"/>
/// </summary>
[TestClass]
public class CommaSpacingUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that an ordinary argument-list comma requires one space after it
    /// </summary>
    [TestMethod]
    public void GetDesiredSpacesAfterReturnsOneForOrdinaryArgumentComma()
    {
        var token = GetCommaToken("class C { void M() { N(1,2); } }");

        Assert.AreEqual(1, CommaSpacingUtilities.GetDesiredSpacesAfter(token));
    }

    /// <summary>
    /// Verifies that a sized array-rank comma requires one space after it, unlike its rank-only sibling
    /// </summary>
    [TestMethod]
    public void GetDesiredSpacesAfterReturnsOneForSizedArrayRankSpecifierComma()
    {
        var token = GetCommaToken("class C { void M() { _ = new int[1,2]; } }");

        Assert.AreEqual(1, CommaSpacingUtilities.GetDesiredSpacesAfter(token));
    }

    /// <summary>
    /// Verifies that a rank-only array-specifier comma requires no space after it
    /// </summary>
    [TestMethod]
    public void GetDesiredSpacesAfterReturnsZeroForRankOnlyArraySpecifierComma()
    {
        var token = GetCommaToken("class C { int[,] M() { return null; } }");

        Assert.AreEqual(0, CommaSpacingUtilities.GetDesiredSpacesAfter(token));
    }

    /// <summary>
    /// Verifies that an unbound generic type comma requires no space after it
    /// </summary>
    [TestMethod]
    public void GetDesiredSpacesAfterReturnsZeroForUnboundGenericTypeComma()
    {
        var token = GetCommaToken("class C { System.Type M() { return typeof(System.Collections.Generic.Dictionary<,>); } }");

        Assert.AreEqual(0, CommaSpacingUtilities.GetDesiredSpacesAfter(token));
    }

    /// <summary>
    /// Verifies that the comma of an interpolated-string alignment component requires no space after it
    /// </summary>
    [TestMethod]
    public void GetDesiredSpacesAfterReturnsZeroForInterpolationAlignmentComma()
    {
        var token = GetCommaToken("""class C { string M(string value) { return $"{value,-10}"; } }""");

        Assert.AreEqual(0, CommaSpacingUtilities.GetDesiredSpacesAfter(token));
    }

    /// <summary>
    /// Verifies that <see cref="CommaSpacingUtilities.IsSpacingExempt"/> stays <see langword="false"/> for
    /// the interpolation-alignment comma. Unlike the rank-only array specifier and unbound generic type
    /// owners, this comma must stay subject to spacing analysis rather than being skipped entirely, so the
    /// analyzer keeps reporting a stray space around it (issue #696)
    /// </summary>
    [TestMethod]
    public void IsSpacingExemptReturnsFalseForInterpolationAlignmentComma()
    {
        var token = GetCommaToken("""class C { string M(string value) { return $"{value,-10}"; } }""");

        Assert.IsFalse(CommaSpacingUtilities.IsSpacingExempt(token));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the source and returns its single comma token
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The comma token</returns>
    private static SyntaxToken GetCommaToken(string source)
    {
        var root = CoreSyntaxTestHelper.ParseCompilationUnit(source);

        return root.DescendantTokens().Single(currentToken => currentToken.IsKind(SyntaxKind.CommaToken));
    }

    #endregion // Methods
}