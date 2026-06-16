using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.DocumentationComments;

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
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that multiple spaces after a <c>///</c> line prefix are collapsed to a single space
    /// </summary>
    [TestMethod]
    public void NormalizesLinePrefixSpacing()
    {
        // Arrange
        const string input = """
                             ///   <summary>
                             ///   Does a thing.
                             ///   </summary>
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