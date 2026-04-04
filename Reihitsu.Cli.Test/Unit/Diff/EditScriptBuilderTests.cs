using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Diff;

namespace Reihitsu.Cli.Test.Unit.Diff;

/// <summary>
/// Tests for <see cref="EditScriptBuilder.Build"/>.
/// </summary>
[TestClass]
public class EditScriptBuilderTests
{
    #region Methods

    /// <summary>
    /// Verifies that identical arrays produce all <see cref="EditKind.Equal"/> operations.
    /// </summary>
    [TestMethod]
    public void BuildIdenticalArraysReturnsAllEqual()
    {
        var lines = new[] { "a", "b", "c" };

        var operations = EditScriptBuilder.Build(lines, lines);

        Assert.HasCount(3, operations);

        foreach (var operation in operations)
        {
            Assert.AreEqual(EditKind.Equal, operation.Kind);
        }
    }

    /// <summary>
    /// Verifies that completely different arrays produce <see cref="EditKind.Delete"/> and <see cref="EditKind.Insert"/> operations.
    /// </summary>
    [TestMethod]
    public void BuildCompletelyDifferentReturnsDeleteAndInsert()
    {
        var original = new[] { "a", "b" };
        var formatted = new[] { "x", "y" };

        var operations = EditScriptBuilder.Build(original, formatted);

        var deleteCount = operations.Count(o => o.Kind == EditKind.Delete);
        var insertCount = operations.Count(o => o.Kind == EditKind.Insert);
        var equalCount = operations.Count(o => o.Kind == EditKind.Equal);

        Assert.AreEqual(2, deleteCount);
        Assert.AreEqual(2, insertCount);
        Assert.AreEqual(0, equalCount);
    }

    /// <summary>
    /// Verifies that a single insertion produces the correct edit script with <see cref="EditKind.Equal"/> and <see cref="EditKind.Insert"/> operations.
    /// </summary>
    [TestMethod]
    public void BuildSingleInsertionReturnsCorrectScript()
    {
        var original = new[] { "a", "c" };
        var formatted = new[] { "a", "b", "c" };

        var operations = EditScriptBuilder.Build(original, formatted);

        Assert.HasCount(3, operations);
        Assert.AreEqual(EditKind.Equal, operations[0].Kind);
        Assert.AreEqual(EditKind.Insert, operations[1].Kind);
        Assert.AreEqual(EditKind.Equal, operations[2].Kind);
    }

    /// <summary>
    /// Verifies that a single deletion produces the correct edit script with <see cref="EditKind.Equal"/> and <see cref="EditKind.Delete"/> operations.
    /// </summary>
    [TestMethod]
    public void BuildSingleDeletionReturnsCorrectScript()
    {
        var original = new[] { "a", "b", "c" };
        var formatted = new[] { "a", "c" };

        var operations = EditScriptBuilder.Build(original, formatted);

        Assert.HasCount(3, operations);
        Assert.AreEqual(EditKind.Equal, operations[0].Kind);
        Assert.AreEqual(EditKind.Delete, operations[1].Kind);
        Assert.AreEqual(EditKind.Equal, operations[2].Kind);
    }

    /// <summary>
    /// Verifies that an empty original array produces all <see cref="EditKind.Insert"/> operations.
    /// </summary>
    [TestMethod]
    public void BuildEmptyOriginalReturnsAllInserts()
    {
        var original = Array.Empty<string>();
        var formatted = new[] { "a", "b" };

        var operations = EditScriptBuilder.Build(original, formatted);

        Assert.HasCount(2, operations);

        foreach (var operation in operations)
        {
            Assert.AreEqual(EditKind.Insert, operation.Kind);
        }
    }

    /// <summary>
    /// Verifies that an empty formatted array produces all <see cref="EditKind.Delete"/> operations.
    /// </summary>
    [TestMethod]
    public void BuildEmptyFormattedReturnsAllDeletes()
    {
        var original = new[] { "a", "b" };
        var formatted = Array.Empty<string>();

        var operations = EditScriptBuilder.Build(original, formatted);

        Assert.HasCount(2, operations);

        foreach (var operation in operations)
        {
            Assert.AreEqual(EditKind.Delete, operation.Kind);
        }
    }

    #endregion // Methods
}