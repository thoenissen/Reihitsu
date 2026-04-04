using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.BlankLines.Resources;

namespace Reihitsu.Formatter.Test.Integration.Rules.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.BlankLines.BlankLineBeforeStatementRule"/>
/// </summary>
[TestClass]
public class BlankLineBeforeStatementRuleTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the formatter inserts blank lines before statements where required.
    /// </summary>
    [TestMethod]
    public void InsertsBlankLinesBeforeStatements()
    {
        // Arrange
        var input = TestData.BlankLineBeforeStatementTestData;
        var expected = TestData.BlankLineBeforeStatementResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}