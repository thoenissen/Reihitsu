using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.BlankLines.Resources;

namespace Reihitsu.Formatter.Test.Integration.Rules.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.BlankLines.BlankLineAfterStatementRule"/>
/// </summary>
[TestClass]
public class BlankLineAfterStatementRuleTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the formatter inserts blank lines after break statements where required.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLinesAfterBreakStatements()
    {
        // Arrange
        var input = TestData.BlankLineAfterStatementTestData;
        var expected = TestData.BlankLineAfterStatementResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}