using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Diff;

namespace Reihitsu.Cli.Test.Unit.Diff;

/// <summary>
/// Tests for <see cref="LcsComputer.ComputeTable"/>
/// </summary>
[TestClass]
public class LcsComputerTests
{
    #region Methods

    /// <summary>
    /// Verifies that identical lines produce a table where diagonal values increment sequentially
    /// </summary>
    [TestMethod]
    public void ComputeTableIdenticalLinesReturnsCorrectTable()
    {
        var lines = ToDiffLines("a", "b", "c");

        var table = LcsComputer.ComputeTable(lines, lines);

        Assert.AreEqual(1, table[1, 1]);
        Assert.AreEqual(2, table[2, 2]);
        Assert.AreEqual(3, table[3, 3]);
    }

    /// <summary>
    /// Verifies that completely different lines produce a table with zero on the diagonal
    /// </summary>
    [TestMethod]
    public void ComputeTableCompletelyDifferentLinesReturnsZeroDiagonal()
    {
        var original = ToDiffLines("a", "b", "c");
        var formatted = ToDiffLines("x", "y", "z");

        var table = LcsComputer.ComputeTable(original, formatted);

        Assert.AreEqual(0, table[1, 1]);
        Assert.AreEqual(0, table[2, 2]);
        Assert.AreEqual(0, table[3, 3]);
    }

    /// <summary>
    /// Verifies that an empty original array produces a table with all zeros in the first row
    /// </summary>
    [TestMethod]
    public void ComputeTableEmptyOriginalReturnsZeroRow()
    {
        var original = Array.Empty<DiffLine>();
        var formatted = ToDiffLines("a", "b", "c");

        var table = LcsComputer.ComputeTable(original, formatted);

        Assert.AreEqual(1, table.GetLength(0));
        Assert.AreEqual(4, table.GetLength(1));
        Assert.AreEqual(0, table[0, 0]);
        Assert.AreEqual(0, table[0, 1]);
        Assert.AreEqual(0, table[0, 2]);
        Assert.AreEqual(0, table[0, 3]);
    }

    /// <summary>
    /// Verifies that an empty formatted array produces a table with all zeros in the first column
    /// </summary>
    [TestMethod]
    public void ComputeTableEmptyFormattedReturnsZeroColumn()
    {
        var original = ToDiffLines("a", "b", "c");
        var formatted = Array.Empty<DiffLine>();

        var table = LcsComputer.ComputeTable(original, formatted);

        Assert.AreEqual(4, table.GetLength(0));
        Assert.AreEqual(1, table.GetLength(1));
        Assert.AreEqual(0, table[0, 0]);
        Assert.AreEqual(0, table[1, 0]);
        Assert.AreEqual(0, table[2, 0]);
        Assert.AreEqual(0, table[3, 0]);
    }

    /// <summary>
    /// Verifies that partially overlapping lines produce correct LCS lengths in the table
    /// </summary>
    [TestMethod]
    public void ComputeTablePartialOverlapReturnsCorrectLengths()
    {
        var original = ToDiffLines("a", "b", "c", "d");
        var formatted = ToDiffLines("a", "x", "c", "d");

        var table = LcsComputer.ComputeTable(original, formatted);

        // LCS is "a", "c", "d" → length 3
        Assert.AreEqual(3, table[4, 4]);

        // After matching "a" at [1,1]
        Assert.AreEqual(1, table[1, 1]);
    }

    /// <summary>
    /// Converts plain line text into terminated <see cref="DiffLine"/> values for tests that only exercise LCS table
    /// computation and are indifferent to termination state
    /// </summary>
    /// <param name="texts">The line texts</param>
    /// <returns>A <see cref="DiffLine"/> array with every line marked terminated</returns>
    private static DiffLine[] ToDiffLines(params string[] texts)
    {
        return Array.ConvertAll(texts, text => new DiffLine(text, true));
    }

    #endregion // Methods
}