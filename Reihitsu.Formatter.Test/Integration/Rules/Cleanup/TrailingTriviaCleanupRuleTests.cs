using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.Cleanup.Resources;

namespace Reihitsu.Formatter.Test.Integration.Rules.Cleanup;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.Cleanup.TrailingTriviaCleanupRule"/>
/// </summary>
[TestClass]
public class TrailingTriviaCleanupRuleTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that trailing trivia and blank lines are cleaned up.
    /// </summary>
    [TestMethod]
    public void CleansTrailingTriviaAndBlankLines()
    {
        // Arrange
        var input = TestData.TrailingTriviaCleanupTestData;
        var expected = TestData.TrailingTriviaCleanupResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}