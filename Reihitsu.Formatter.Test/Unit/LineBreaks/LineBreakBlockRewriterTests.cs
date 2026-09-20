using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for <see cref="LineBreakBlockRewriter"/>
/// </summary>
[TestClass]
public class LineBreakBlockRewriterTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that two statements on one line inside a block are split onto separate lines, in isolation from
    /// every later phase (in particular <c>SwitchCaseBracePhase</c>, which would otherwise wrap a multi-statement
    /// switch section in a block before this rewriter's own switch-section behavior could be observed)
    /// </summary>
    [TestMethod]
    public void VisitBlockSplitsTwoStatementsOnOneLine()
    {
        // Arrange
        const string input = "{ N(); N(); }";

        // Act
        var result = ExecuteBlockRewriter(input);

        // Assert
        Assert.Contains("N(); \nN();", result, "Both statements should start on their own line.");
    }

    /// <summary>
    /// Verifies that two statements on one line inside a switch section are split onto separate lines by the same
    /// shared implementation used for blocks — the one generic <see cref="LineBreakBlockRewriter"/> method that
    /// replaced two near-identical <c>EnsureStatementsStartOnSeparateLines</c> overloads (one per statement-list
    /// owner)
    /// </summary>
    [TestMethod]
    public void VisitSwitchSectionSplitsTwoStatementsOnOneLine()
    {
        // Arrange
        const string input = "switch (value) { case 1: N(); N(); break; }";

        // Act
        var result = ExecuteBlockRewriter(input);

        // Assert
        Assert.Contains("N(); \nN();", result, "Both statements inside the switch section should start on their own line.");
    }

    /// <summary>
    /// Executes the <see cref="LineBreakBlockRewriter"/> in isolation over the given C# statement
    /// </summary>
    /// <param name="input">The C# statement source text</param>
    /// <returns>The rewritten source text</returns>
    private string ExecuteBlockRewriter(string input)
    {
        var statement = SyntaxFactory.ParseStatement(input);
        var gapNormalizer = new TokenGapNormalizer("\n");
        var bracePlacer = new BracePlacer(gapNormalizer, "\n");
        var rewriter = new LineBreakBlockRewriter(gapNormalizer, bracePlacer, TestContext.CancellationToken);

        return rewriter.Visit(statement).ToFullString();
    }

    #endregion // Methods
}