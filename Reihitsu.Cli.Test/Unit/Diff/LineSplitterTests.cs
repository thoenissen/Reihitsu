using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Diff;

namespace Reihitsu.Cli.Test.Unit.Diff;

/// <summary>
/// Tests for <see cref="LineSplitter.Split"/>
/// </summary>
[TestClass]
public class LineSplitterTests
{
    #region Methods

    /// <summary>
    /// Verifies that <see cref="LineSplitter.Split"/> correctly splits content with Unix line endings
    /// </summary>
    [TestMethod]
    public void SplitUnixLineEndingsReturnsCorrectLines()
    {
        var result = LineSplitter.Split("a\nb\nc");

        Assert.AreEqual(3, result.Length);
        Assert.AreEqual("a", result[0]);
        Assert.AreEqual("b", result[1]);
        Assert.AreEqual("c", result[2]);
    }

    /// <summary>
    /// Verifies that <see cref="LineSplitter.Split"/> correctly splits content with Windows line endings
    /// </summary>
    [TestMethod]
    public void SplitWindowsLineEndingsReturnsCorrectLines()
    {
        var result = LineSplitter.Split("a\r\nb\r\nc");

        Assert.AreEqual(3, result.Length);
        Assert.AreEqual("a", result[0]);
        Assert.AreEqual("b", result[1]);
        Assert.AreEqual("c", result[2]);
    }

    /// <summary>
    /// Verifies that <see cref="LineSplitter.Split"/> correctly splits content with mixed line endings
    /// </summary>
    [TestMethod]
    public void SplitMixedLineEndingsReturnsCorrectLines()
    {
        var result = LineSplitter.Split("a\nb\r\nc");

        Assert.AreEqual(3, result.Length);
        Assert.AreEqual("a", result[0]);
        Assert.AreEqual("b", result[1]);
        Assert.AreEqual("c", result[2]);
    }

    /// <summary>
    /// Verifies that <see cref="LineSplitter.Split"/> returns a single empty element for an empty string
    /// </summary>
    [TestMethod]
    public void SplitEmptyStringReturnsSingleElement()
    {
        var result = LineSplitter.Split(string.Empty);

        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(string.Empty, result[0]);
    }

    /// <summary>
    /// Verifies that <see cref="LineSplitter.Split"/> returns a single element for a string with no line endings
    /// </summary>
    [TestMethod]
    public void SplitSingleLineWithoutLineEndingReturnsSingleElement()
    {
        var result = LineSplitter.Split("hello");

        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("hello", result[0]);
    }

    #endregion // Methods
}