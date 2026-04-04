using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.Spacing.Resources;

namespace Reihitsu.Formatter.Test.Integration.Rules.Spacing;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.Spacing.HorizontalSpacingRule"/>
/// </summary>
[TestClass]
public class HorizontalSpacingRuleTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that horizontal spacing is normalized correctly.
    /// </summary>
    [TestMethod]
    public void NormalizesHorizontalSpacing()
    {
        // Arrange
        var input = TestData.HorizontalSpacingTestData;
        var expected = TestData.HorizontalSpacingResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}