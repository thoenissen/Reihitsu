using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for <see cref="UnifiedDiffWriter"/>
/// </summary>
[TestClass]
public sealed class UnifiedDiffWriterTests
{
    #region Constants

    /// <summary>
    /// The internal no-newline sentinel text, written out literally so a test can plant it inside authored source
    /// without seeing <see cref="UnifiedDiffWriter"/>'s own private constant
    /// </summary>
    private const string SentinelLikeAuthoredText = "￼NO-NEWLINE-AT-END-OF-FILE￼";

    #endregion // Constants

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

    /// <summary>
    /// Verifies that an unchanged, newline-terminated final line whose authored text happens to equal the internal
    /// no-newline sentinel renders verbatim, without truncating the comment or emitting a false no-newline marker
    /// </summary>
    [TestMethod]
    public void GenerateRendersTerminatedFinalLineContainingSentinelLikeTextVerbatim()
    {
        // Act
        var result = UnifiedDiffWriter.Generate("x.cs",
                                                $"class A\n{{\n}}\n// {SentinelLikeAuthoredText}\n",
                                                $"class B\n{{\n}}\n// {SentinelLikeAuthoredText}\n");

        // Assert
        Assert.AreEqual("--- a/x.cs\n"
                        + "+++ b/x.cs\n"
                        + "@@ -1,4 +1,4 @@\n"
                        + "-class A\n"
                        + "+class B\n"
                        + " {\n"
                        + " }\n"
                        + $" // {SentinelLikeAuthoredText}\n",
                        result);
    }

    /// <summary>
    /// Verifies that a non-final line whose authored text equals the internal no-newline sentinel renders verbatim
    /// and never emits the no-newline marker, which only a genuinely unterminated final line may carry
    /// </summary>
    [TestMethod]
    public void GenerateRendersNonFinalLineContainingSentinelLikeTextVerbatim()
    {
        // Act
        var result = UnifiedDiffWriter.Generate("x.cs",
                                                $"a\n// {SentinelLikeAuthoredText}\nb\n",
                                                $"a\n// {SentinelLikeAuthoredText}\nc\n");

        // Assert
        Assert.AreEqual("--- a/x.cs\n"
                        + "+++ b/x.cs\n"
                        + "@@ -1,3 +1,3 @@\n"
                        + " a\n"
                        + $" // {SentinelLikeAuthoredText}\n"
                        + "-b\n"
                        + "+c\n",
                        result);
    }

    /// <summary>
    /// Verifies that an unterminated line and a terminated line whose authored text equals the internal no-newline
    /// sentinel never compare equal, so a real change is rendered as a change rather than collapsed into one
    /// context line. The comparison key has to carry termination state, not only line text
    /// </summary>
    [TestMethod]
    public void GenerateDistinguishesUnterminatedContentFromTerminatedContentEndingInSentinelLikeText()
    {
        // Act
        var result = UnifiedDiffWriter.Generate("x.cs", "}", $"}}{SentinelLikeAuthoredText}\n");

        // Assert
        Assert.AreEqual("--- a/x.cs\n"
                        + "+++ b/x.cs\n"
                        + "@@ -1,1 +1,1 @@\n"
                        + "-}\n"
                        + "\\ No newline at end of file\n"
                        + $"+}}{SentinelLikeAuthoredText}\n",
                        result);
    }

    #endregion // Methods
}