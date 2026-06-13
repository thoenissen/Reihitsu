using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for <see cref="LineBreakContainedBlockRewriter"/>
/// </summary>
[TestClass]
public class ContainedBlockRewriterTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that an <c>if</c> statement whose open brace is inline with the condition is fully normalized
    /// in a single rewriter pass, even though the open-brace edit shifts the positions of the first statement
    /// and the close brace (issue #241)
    /// </summary>
    [TestMethod]
    public void IfStatementWithInlineBracesIsNormalizedInSinglePass()
    {
        // Arrange — the open brace shares the condition line, so normalizing the gap before it shifts every
        //           following token; the first-content and close-brace steps must still apply
        const string input = "if (a) { B(); }";

        // Act
        var firstPass = ExecuteContainedBlockRewriter(input);
        var secondPass = ExecuteContainedBlockRewriter(firstPass);

        // Assert
        Assert.AreEqual(firstPass, secondPass, "A single contained-block rewriter pass must fully normalize the if block.");
        Assert.DoesNotContain("{ B();", firstPass, "The first statement should be moved onto its own line in a single pass.");
        Assert.DoesNotContain("B(); }", firstPass, "The close brace should be moved onto its own line in a single pass.");
    }

    /// <summary>
    /// Verifies that an <c>if</c>/<c>else</c> statement whose else open brace is inline is fully normalized in
    /// a single rewriter pass
    /// </summary>
    [TestMethod]
    public void IfElseStatementWithInlineElseBracesIsNormalizedInSinglePass()
    {
        // Arrange
        const string input = "if (a)\n{\n    B();\n}\nelse { C(); }";

        // Act
        var firstPass = ExecuteContainedBlockRewriter(input);
        var secondPass = ExecuteContainedBlockRewriter(firstPass);

        // Assert
        Assert.AreEqual(firstPass, secondPass, "A single contained-block rewriter pass must fully normalize the else block.");
        Assert.DoesNotContain("{ C();", firstPass, "The first statement of the else block should be moved onto its own line in a single pass.");
        Assert.DoesNotContain("C(); }", firstPass, "The close brace of the else block should be moved onto its own line in a single pass.");
    }

    /// <summary>
    /// Executes the <see cref="LineBreakContainedBlockRewriter"/> in isolation over the given C# statement
    /// </summary>
    /// <param name="input">The C# statement source text</param>
    /// <returns>The rewritten source text</returns>
    private string ExecuteContainedBlockRewriter(string input)
    {
        var statement = SyntaxFactory.ParseStatement(input);
        var context = new FormattingContext("\n");
        var gapNormalizer = new TokenGapNormalizer(context.EndOfLine);
        var bracePlacer = new BracePlacer(gapNormalizer, context.EndOfLine);
        var rewriter = new LineBreakContainedBlockRewriter(context, TestContext.CancellationToken, gapNormalizer, bracePlacer);

        return rewriter.Visit(statement).ToFullString();
    }

    #endregion // Methods
}