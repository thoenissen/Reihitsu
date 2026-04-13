using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Diff;

namespace Reihitsu.Cli.Test.Unit.Diff;

/// <summary>
/// Tests for <see cref="HunkBuilder.Build"/>.
/// </summary>
[TestClass]
public class HunkBuilderTests
{
    #region Methods

    /// <summary>
    /// Verifies that a list of all <see cref="EditKind.Equal"/> operations returns an empty list of hunks.
    /// </summary>
    [TestMethod]
    public void BuildNoChangesReturnsEmptyList()
    {
        var operations = new List<EditOperation>
                         {
                             new(EditKind.Equal, 0, 0),
                             new(EditKind.Equal, 1, 1),
                             new(EditKind.Equal, 2, 2)
                         };

        var hunks = HunkBuilder.Build(operations, 3, 3);

        Assert.IsEmpty(hunks);
    }

    /// <summary>
    /// Verifies that a single change produces exactly one hunk.
    /// </summary>
    [TestMethod]
    public void BuildSingleChangeReturnsSingleHunk()
    {
        var operations = new List<EditOperation>
                         {
                             new(EditKind.Equal, 0, 0),
                             new(EditKind.Delete, 1, -1),
                             new(EditKind.Insert, -1, 1),
                             new(EditKind.Equal, 2, 2)
                         };

        var hunks = HunkBuilder.Build(operations, 3, 3);

        Assert.HasCount(1, hunks);
    }

    /// <summary>
    /// Verifies that changes separated by more than twice <see cref="HunkBuilder.ContextLines"/> produce separate hunks.
    /// </summary>
    [TestMethod]
    public void BuildFarApartChangesReturnsSeparateHunks()
    {
        var operations = new List<EditOperation>
                         {
                             // First change at index 0
                             new(EditKind.Delete, 0, -1),
                             new(EditKind.Insert, -1, 0)
                         };

        // Add enough Equal operations to separate hunks (> 2 × ContextLines)
        for (var i = 1; i <= 8; i++)
        {
            operations.Add(new EditOperation(EditKind.Equal, i, i));
        }

        // Second change
        operations.Add(new EditOperation(EditKind.Delete, 9, -1));
        operations.Add(new EditOperation(EditKind.Insert, -1, 9));

        var hunks = HunkBuilder.Build(operations, 10, 10);

        Assert.HasCount(2, hunks);
    }

    /// <summary>
    /// Verifies that changes within<see cref="HunkBuilder.ContextLines"/> distance are merged into a single hunk.
    /// </summary>
    [TestMethod]
    public void BuildNearbyChangesMergesIntoSingleHunk()
    {
        var operations = new List<EditOperation>
                         {
                             new(EditKind.Delete, 0, -1),
                             new(EditKind.Insert, -1, 0),
                             new(EditKind.Equal, 1, 1),
                             new(EditKind.Equal, 2, 2),
                             new(EditKind.Delete, 3, -1),
                             new(EditKind.Insert, -1, 3)
                         };

        var hunks = HunkBuilder.Build(operations, 4, 4);

        Assert.HasCount(1, hunks);
    }

    /// <summary>
    /// Verifies that hunks contain the correct number of context lines around changes.
    /// </summary>
    [TestMethod]
    public void BuildHunkContainsCorrectContextLines()
    {
        var operations = new List<EditOperation>();

        // 5 equal lines before the change
        for (var i = 0; i < 5; i++)
        {
            operations.Add(new EditOperation(EditKind.Equal, i, i));
        }

        // One change
        operations.Add(new EditOperation(EditKind.Delete, 5, -1));
        operations.Add(new EditOperation(EditKind.Insert, -1, 5));

        // 5 equal lines after the change
        for (var i = 6; i < 11; i++)
        {
            operations.Add(new EditOperation(EditKind.Equal, i, i));
        }

        var hunks = HunkBuilder.Build(operations, 11, 11);

        Assert.HasCount(1, hunks);

        var hunk = hunks[0];
        var equalBefore = 0;
        var equalAfter = 0;
        var foundChange = false;

        foreach (var op in hunk.Operations)
        {
            if (op.Kind == EditKind.Equal && foundChange == false)
            {
                equalBefore++;
            }
            else if (op.Kind != EditKind.Equal)
            {
                foundChange = true;
            }
            else
            {
                equalAfter++;
            }
        }

        Assert.AreEqual(HunkBuilder.ContextLines, equalBefore);
        Assert.AreEqual(HunkBuilder.ContextLines, equalAfter);
    }

    /// <summary>
    /// Verifies that <see cref="DiffHunk.OriginalCount"/> and <see cref="DiffHunk.FormattedCount"/> match the operations.
    /// </summary>
    [TestMethod]
    public void BuildHunkCountsAreCorrect()
    {
        var operations = new List<EditOperation>
                         {
                             new(EditKind.Equal, 0, 0),
                             new(EditKind.Delete, 1, -1),
                             new(EditKind.Insert, -1, 1),
                             new(EditKind.Insert, -1, 2),
                             new(EditKind.Equal, 2, 3)
                         };

        var hunks = HunkBuilder.Build(operations, 3, 4);

        Assert.HasCount(1, hunks);

        var hunk = hunks[0];
        var expectedOriginalCount = hunk.Operations.Count(o => o.Kind is EditKind.Equal or EditKind.Delete);
        var expectedFormattedCount = hunk.Operations.Count(o => o.Kind is EditKind.Equal or EditKind.Insert);

        Assert.AreEqual(expectedOriginalCount, hunk.OriginalCount);
        Assert.AreEqual(expectedFormattedCount, hunk.FormattedCount);
    }

    #endregion // Methods
}