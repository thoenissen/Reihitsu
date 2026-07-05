using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Diff;
using Reihitsu.Cli.Enumerations;

namespace Reihitsu.Cli.Test.Unit.Diff;

/// <summary>
/// Tests for <see cref="EditScriptBuilder.Build"/>
/// </summary>
[TestClass]
public class EditScriptBuilderTests
{
    #region Methods

    /// <summary>
    /// Verifies that identical arrays produce all <see cref="EditKind.Equal"/> operations
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
    /// Verifies that completely different arrays produce <see cref="EditKind.Delete"/> and <see cref="EditKind.Insert"/> operations
    /// </summary>
    [TestMethod]
    public void BuildCompletelyDifferentReturnsDeleteAndInsert()
    {
        var original = new[] { "a", "b" };
        var formatted = new[] { "x", "y" };

        var operations = EditScriptBuilder.Build(original, formatted);

        var deleteCount = operations.Count(operation => operation.Kind == EditKind.Delete);
        var insertCount = operations.Count(operation => operation.Kind == EditKind.Insert);
        var equalCount = operations.Count(operation => operation.Kind == EditKind.Equal);

        Assert.AreEqual(2, deleteCount);
        Assert.AreEqual(2, insertCount);
        Assert.AreEqual(0, equalCount);
    }

    /// <summary>
    /// Verifies that a single insertion produces the correct edit script with <see cref="EditKind.Equal"/> and <see cref="EditKind.Insert"/> operations
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
    /// Verifies that a single deletion produces the correct edit script with <see cref="EditKind.Equal"/> and <see cref="EditKind.Delete"/> operations
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
    /// Verifies that an empty original array produces all <see cref="EditKind.Insert"/> operations
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
    /// Verifies that an empty formatted array produces all <see cref="EditKind.Delete"/> operations
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

    /// <summary>
    /// Verifies that a localized change in a large input produces a minimal edit script without allocating a full O(n×m) table.
    /// Without trimming the common prefix and suffix, two 10 000-line inputs would allocate a ~400 MB table
    /// </summary>
    [TestMethod]
    public void BuildLargeInputWithLocalizedChangeReturnsMinimalScript()
    {
        const int lineCount = 10_000;
        const int changedLine = 5_000;

        var original = new string[lineCount];
        var formatted = new string[lineCount];

        for (var index = 0; index < lineCount; index++)
        {
            original[index] = $"line{index}";
            formatted[index] = index == changedLine ? "changed" : $"line{index}";
        }

        var operations = EditScriptBuilder.Build(original, formatted);

        var deleteCount = operations.Count(operation => operation.Kind == EditKind.Delete);
        var insertCount = operations.Count(operation => operation.Kind == EditKind.Insert);
        var equalCount = operations.Count(operation => operation.Kind == EditKind.Equal);

        Assert.AreEqual(1, deleteCount);
        Assert.AreEqual(1, insertCount);
        Assert.AreEqual(lineCount - 1, equalCount);
    }

    /// <summary>
    /// Verifies that a large input where every line differs is handled with bounded memory by falling back to a delete-then-insert script instead of allocating a full O(n×m) table
    /// </summary>
    [TestMethod]
    public void BuildLargeFullyDifferentInputProducesBoundedScript()
    {
        const int lineCount = 4_000;

        var original = new string[lineCount];
        var formatted = new string[lineCount];

        for (var index = 0; index < lineCount; index++)
        {
            original[index] = $"original{index}";
            formatted[index] = $"formatted{index}";
        }

        var operations = EditScriptBuilder.Build(original, formatted);

        var deleteCount = operations.Count(operation => operation.Kind == EditKind.Delete);
        var insertCount = operations.Count(operation => operation.Kind == EditKind.Insert);
        var equalCount = operations.Count(operation => operation.Kind == EditKind.Equal);

        Assert.AreEqual(lineCount, deleteCount);
        Assert.AreEqual(lineCount, insertCount);
        Assert.AreEqual(0, equalCount);
    }

    #endregion // Methods
}