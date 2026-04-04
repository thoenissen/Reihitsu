using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.Indentation.Resources;

namespace Reihitsu.Formatter.Test.Integration.Rules.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.Indentation.IndentationAndAlignmentRule"/>
/// </summary>
[TestClass]
public class IndentationAndAlignmentRuleTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that indentation is normalized correctly.
    /// </summary>
    [TestMethod]
    public void NormalizesIndentation()
    {
        // Arrange
        var input = TestData.IndentationTestData;
        var expected = TestData.IndentationResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}