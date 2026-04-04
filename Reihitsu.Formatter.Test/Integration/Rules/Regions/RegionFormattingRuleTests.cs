using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.Regions.Resources;

namespace Reihitsu.Formatter.Test.Integration.Rules.Regions;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.Regions.RegionFormattingRule"/>
/// </summary>
[TestClass]
public class RegionFormattingRuleTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that region directives are formatted correctly.
    /// </summary>
    [TestMethod]
    public void FormatsRegionDirectives()
    {
        // Arrange
        var input = TestData.RegionFormattingTestData;
        var expected = TestData.RegionFormattingResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}