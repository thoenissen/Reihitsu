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

        Assert.HasCount(3, result);
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

        Assert.HasCount(3, result);
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

        Assert.HasCount(3, result);
        Assert.AreEqual("a", result[0]);
        Assert.AreEqual("b", result[1]);
        Assert.AreEqual("c", result[2]);
    }

    /// <summary>
    /// Verifies that <see cref="LineSplitter.Split"/> treats an empty string as zero lines
    /// </summary>
    [TestMethod]
    public void SplitEmptyStringReturnsNoLines()
    {
        var result = LineSplitter.Split(string.Empty);

        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Verifies that <see cref="LineSplitter.Split"/> correctly splits content with lone carriage-return line endings
    /// </summary>
    [TestMethod]
    public void SplitCarriageReturnLineEndingsReturnsCorrectLines()
    {
        var result = LineSplitter.Split("a\rb\rc");

        Assert.HasCount(3, result);
        Assert.AreEqual("a", result[0]);
        Assert.AreEqual("b", result[1]);
        Assert.AreEqual("c", result[2]);
    }

    /// <summary>
    /// Verifies that <see cref="LineSplitter.Split"/> returns a single element for a string with no line endings
    /// </summary>
    [TestMethod]
    public void SplitSingleLineWithoutLineEndingReturnsSingleElement()
    {
        var result = LineSplitter.Split("hello");

        Assert.HasCount(1, result);
        Assert.AreEqual("hello", result[0]);
    }

    #endregion // Methods
}