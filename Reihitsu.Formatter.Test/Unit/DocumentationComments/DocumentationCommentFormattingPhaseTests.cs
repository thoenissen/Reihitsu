using System;
using System.Text;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.DocumentationComments;
using Reihitsu.Formatter.Pipeline.DocumentationComments.Utilities;

namespace Reihitsu.Formatter.Test.Unit.DocumentationComments;

/// <summary>
/// Tests for <see cref="DocumentationCommentFormattingPhase"/>, the orchestrating half of the
/// documentation-comment phase. These pin the candidate location, the element splice and the
/// <c>///</c> line-prefix normalization on top of <see cref="DocCommentElementNormalizer"/>
/// </summary>
[TestClass]
public class DocumentationCommentFormattingPhaseTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a missing separator after a <c>///</c> line prefix is inserted
    /// </summary>
    [TestMethod]
    public void NormalizesLinePrefixSpacing()
    {
        // Arrange
        const string input = """
                             ///<summary>
                             ///Does a thing.
                             ///</summary>
                             public class C
                             {
                             }
                             """;
        const string expected = """
                                /// <summary>
                                /// Does a thing.
                                /// </summary>
                                public class C
                                {
                                }
                                """;

        // Act
        var actual = Format(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that whitespace after the canonical separator is preserved as content indentation
    /// </summary>
    [TestMethod]
    public void PreservesContentIndentationAfterCanonicalSeparator()
    {
        // Arrange
        const string input = """
                             ///   <summary>
                             ///   - Standard
                             ///     - Fast
                             ///   </summary>
                             public class C
                             {
                             }
                             """;

        // Act
        var actual = Format(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that a single-line summary is expanded across three lines
    /// </summary>
    [TestMethod]
    public void ExpandsSingleLineSummary()
    {
        // Arrange
        const string input = """
                             /// <summary> Does a thing. </summary>
                             public class C
                             {
                             }
                             """;
        const string expected = """
                                /// <summary>
                                /// Does a thing.
                                /// </summary>
                                public class C
                                {
                                }
                                """;

        // Act
        var actual = Format(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an already well-formed documentation comment is left unchanged
    /// </summary>
    [TestMethod]
    public void LeavesWellFormedCommentUnchanged()
    {
        // Arrange
        const string input = """
                             /// <summary>
                             /// Does a thing.
                             /// </summary>
                             public class C
                             {
                             }
                             """;

        // Act
        var actual = Format(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that every line terminator C# recognizes starts a new documentation line. This phase does not
    /// rewrite the separator itself, so the exotic terminators survive it and are normalized later by
    /// <c>LineEndingNormalizationPhase</c>
    /// </summary>
    /// <param name="lineTerminator">Terminator separating the two documentation lines</param>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void NormalizesEveryDocumentationLineTerminator(string lineTerminator)
    {
        // Arrange
        var input = string.Concat("///<summary>", lineTerminator, "///Does a thing.", lineTerminator, "///</summary>\npublic class C\n{\n}");
        var expected = string.Concat("/// <summary>", lineTerminator, "/// Does a thing.", lineTerminator, "/// </summary>\npublic class C\n{\n}");

        // Act
        var actual = Format(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that only the first exterior on a line is recognized, so a second <c>///</c> further along the
    /// line stays content
    /// </summary>
    [TestMethod]
    public void LeavesLaterSlashesOnTheLineAsContent()
    {
        // Arrange
        const string input = """
                             /// see /// here
                             public class C
                             {
                             }
                             """;

        // Act
        var actual = Format(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that a very large documentation comment is normalized completely, without an exception and
    /// without a wall-clock budget deciding the outcome. The comment is sized past the point at which the
    /// historic 100 ms match timeout aborted the pattern this phase used to run, so an implementation that
    /// reintroduced a tight budget would fail here rather than intermittently under CI load
    /// </summary>
    [TestMethod]
    public void NormalizesVeryLargeDocumentationComment()
    {
        // Arrange
        const int lineCount = 150000;

        var builder = new StringBuilder();

        for (var index = 0; index < lineCount; index++)
        {
            builder.Append("///Documentation line that carries no separator after the exterior marker.\n");
        }

        builder.Append("public class C\n{\n}");

        var input = builder.ToString();

        // Act
        var normalized = Format(input);
        var secondPass = Format(normalized);

        // Assert
        Assert.AreEqual(lineCount, CountOccurrences(normalized, "/// Documentation line"), "Every documentation line must carry the canonical separator.");
        Assert.AreEqual(0, CountOccurrences(normalized, "///Documentation line"), "No documentation line may keep a missing separator.");
        Assert.AreEqual(normalized, secondPass, "A second pass over the normalized comment must be a no-op.");
    }

    /// <summary>
    /// Counts the non-overlapping occurrences of a value in the given text
    /// </summary>
    /// <param name="text">Text to search</param>
    /// <param name="value">Value to count</param>
    /// <returns>The number of occurrences</returns>
    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = text.IndexOf(value, StringComparison.Ordinal);

        while (index >= 0)
        {
            count++;
            index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal);
        }

        return count;
    }

    /// <summary>
    /// Runs the documentation-comment phase over the given source and returns the result
    /// </summary>
    /// <param name="input">Source code to format</param>
    /// <returns>The formatted source code</returns>
    private string Format(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = new DocumentationCommentFormattingPhase().Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);

        return result.ToFullString();
    }

    #endregion // Methods
}