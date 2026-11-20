using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineEndings;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for <see cref="LineEndingNormalizationPhase"/> executed directly
/// </summary>
[TestClass]
public class LineEndingNormalizationPhaseTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Placeholder for a line break inside a literal, a comment, or disabled text, which keeps its characters
    /// </summary>
    private const string KeptLineBreakPlaceholder = "¤";

    /// <summary>
    /// Placeholder for a line break that the phase rewrites and that the test replaces with a foreign terminator
    /// </summary>
    private const string ForeignLineBreakPlaceholder = "¶";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that the phase returns its input node itself when every line break between tokens, at the end of a
    /// directive, and inside a documentation comment is the detected line ending, also when a literal, a comment, or
    /// disabled text holds another line break
    /// </summary>
    /// <param name="endOfLine">The detected line ending, which every line break of the input uses</param>
    /// <param name="otherLineEnding">The line break inside the literal, the comment, and the disabled text</param>
    [TestMethod]
    [DataRow("\n", "\r\n")]
    [DataRow("\r\n", "\n")]
    [DataRow("\n", "\u2028")]
    public void ExecuteReturnsItsInputWhenNoLineBreakDiffersFromTheDetectedLineEnding(string endOfLine, string otherLineEnding)
    {
        // Arrange
        var input = "class C\n"
                    + "{\n"
                    + "#pragma warning disable CS0168\n"
                    + "    /// <summary>\n"
                    + "    /// Doc\n"
                    + "    /// </summary>\n"
                    + "    private string _s = @\"a¤b\";\n"
                    + "    private int _x; /* a¤b */\n"
                    + "#if false\n"
                    + "    a¤b\n"
                    + "#endif\n"
                    + "}";

        input = input.Replace("\n", endOfLine).Replace(KeptLineBreakPlaceholder, otherLineEnding);

        var root = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        // Act
        var result = new LineEndingNormalizationPhase().Execute(root, new FormattingContext(endOfLine), TestContext.CancellationToken);

        // Assert
        Assert.AreSame(root, result);
    }

    /// <summary>
    /// Verifies that the phase rewrites a foreign line break that only a documentation comment holds, while every end-of-line
    /// trivia already is the detected line ending, and leaves every other character of the input as it was
    /// </summary>
    /// <param name="endOfLine">The detected line ending</param>
    /// <param name="foreignLineEnding">The line break inside the documentation comment</param>
    [TestMethod]
    [Requirement("FMT-EOL-003")]
    [DataRow("\n", "\r\n")]
    [DataRow("\r\n", "\n")]
    public void ExecuteRewritesAForeignLineBreakThatOnlyADocumentationCommentHolds(string endOfLine, string foreignLineEnding)
    {
        // Arrange
        var input = "class C\n"
                    + "{\n"
                    + "    /// <summary>¶"
                    + "    /// Doc\n"
                    + "    /// </summary>\n"
                    + "    private int _x;\n"
                    + "}";
        var expected = "class C\n"
                       + "{\n"
                       + "    /// <summary>\n"
                       + "    /// Doc\n"
                       + "    /// </summary>\n"
                       + "    private int _x;\n"
                       + "}";

        input = input.Replace("\n", endOfLine).Replace(ForeignLineBreakPlaceholder, foreignLineEnding);
        expected = expected.Replace("\n", endOfLine);

        var root = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        // Act
        var result = new LineEndingNormalizationPhase().Execute(root, new FormattingContext(endOfLine), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(expected, result.ToFullString());
    }

    #endregion // Methods
}