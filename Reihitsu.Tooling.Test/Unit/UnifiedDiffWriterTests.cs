using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for <see cref="UnifiedDiffWriter"/>
/// </summary>
[TestClass]
public sealed class UnifiedDiffWriterTests
{
    #region Methods

    /// <summary>
    /// Verifies that identical content is not reported as a change, whether or not it has a terminal newline
    /// </summary>
    [TestMethod]
    public void GenerateReturnsEmptyForIdenticalContentAndTermination()
    {
        // Act and assert
        Assert.AreEqual(string.Empty, UnifiedDiffWriter.Generate("x.cs", "class A\n", "class A\n"));
        Assert.AreEqual(string.Empty, UnifiedDiffWriter.Generate("x.cs", "class A", "class A"));
    }

    /// <summary>
    /// Verifies that removing the terminal newline emits a changed line and marks the added unterminated side
    /// </summary>
    [TestMethod]
    public void GenerateMarksRemovedTrailingNewline()
    {
        // Act
        var result = UnifiedDiffWriter.Generate("x.cs", "class A\n{\n}\n", "class A\n{\n}");

        // Assert
        Assert.AreEqual("--- a/x.cs\n"
                        + "+++ b/x.cs\n"
                        + "@@ -1,3 +1,3 @@\n"
                        + " class A\n"
                        + " {\n"
                        + "-}\n"
                        + "+}\n"
                        + "\\ No newline at end of file\n",
                        result);
    }

    /// <summary>
    /// Verifies that adding the terminal newline emits a changed line and marks the deleted unterminated side
    /// </summary>
    [TestMethod]
    public void GenerateMarksAddedTrailingNewline()
    {
        // Act
        var result = UnifiedDiffWriter.Generate("x.cs", "class A\n{\n}", "class A\n{\n}\n");

        // Assert
        Assert.AreEqual("--- a/x.cs\n"
                        + "+++ b/x.cs\n"
                        + "@@ -1,3 +1,3 @@\n"
                        + " class A\n"
                        + " {\n"
                        + "-}\n"
                        + "\\ No newline at end of file\n"
                        + "+}\n",
                        result);
    }

    /// <summary>
    /// Verifies that an empty source and a source containing one newline remain observably different
    /// </summary>
    [TestMethod]
    public void GenerateDistinguishesEmptySourceFromSingleNewline()
    {
        // Act
        var result = UnifiedDiffWriter.Generate("x.cs", string.Empty, "\n");

        // Assert
        Assert.AreEqual("--- a/x.cs\n+++ b/x.cs\n@@ -1,0 +1,1 @@\n+\n", result);
    }

    #endregion // Methods
}