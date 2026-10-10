using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for <see cref="FixtureLineEndings"/>
/// </summary>
[TestClass]
public sealed class FixtureLineEndingsTests
{
    #region Methods

    /// <summary>
    /// Verifies that every supported input separator is rewritten to the requested line ending
    /// </summary>
    [TestMethod]
    public void NormalizeRewritesMixedSeparators()
    {
        // Arrange
        const string source = "first\r\nsecond\rthird\n";

        // Act and assert
        Assert.AreEqual("first\nsecond\nthird\n",
                        FixtureLineEndings.Normalize(source, FixtureLineEndings.LineFeed));
        Assert.AreEqual("first\r\nsecond\r\nthird\r\n",
                        FixtureLineEndings.Normalize(source, FixtureLineEndings.CarriageReturnLineFeed));
    }

    /// <summary>
    /// Verifies that separator validation distinguishes LF, CRLF, lone CR, and separator-free text
    /// </summary>
    [TestMethod]
    public void UsesOnlyDistinguishesRequestedSeparator()
    {
        // Act and assert
        Assert.IsTrue(FixtureLineEndings.UsesOnly("first\nsecond\n", FixtureLineEndings.LineFeed));
        Assert.IsFalse(FixtureLineEndings.UsesOnly("first\r\nsecond\r\n", FixtureLineEndings.LineFeed));
        Assert.IsTrue(FixtureLineEndings.UsesOnly("first\r\nsecond\r\n", FixtureLineEndings.CarriageReturnLineFeed));
        Assert.IsFalse(FixtureLineEndings.UsesOnly("first\nsecond\n", FixtureLineEndings.CarriageReturnLineFeed));
        Assert.IsFalse(FixtureLineEndings.UsesOnly("first\rsecond", FixtureLineEndings.LineFeed));
        Assert.IsFalse(FixtureLineEndings.UsesOnly("first\rsecond", FixtureLineEndings.CarriageReturnLineFeed));
        Assert.IsTrue(FixtureLineEndings.UsesOnly("single line", FixtureLineEndings.LineFeed));
        Assert.IsTrue(FixtureLineEndings.UsesOnly("single line", FixtureLineEndings.CarriageReturnLineFeed));
    }

    /// <summary>
    /// Verifies that the line-ending description counts CRLF, lone LF, and lone CR independently
    /// </summary>
    [TestMethod]
    public void DescribeCountsEverySeparatorKind()
    {
        // Act
        var result = FixtureLineEndings.Describe("first\r\nsecond\nthird\rfourth");

        // Assert
        Assert.AreEqual("CRLF=1, LF=1, CR=1", result);
    }

    /// <summary>
    /// Verifies that splitting preserves blank content lines while discarding separators and a trailing terminator
    /// </summary>
    [TestMethod]
    public void SplitLinesPreservesContentLines()
    {
        // Act and assert
        Assert.AreSequenceEqual([], FixtureLineEndings.SplitLines(string.Empty));
        Assert.AreSequenceEqual([string.Empty], FixtureLineEndings.SplitLines("\n"));
        Assert.AreSequenceEqual(["first", string.Empty, "second"], FixtureLineEndings.SplitLines("first\r\n\rsecond\n"));
        Assert.AreSequenceEqual(["first"], FixtureLineEndings.SplitLines("first\n"));
    }

    #endregion // Methods
}