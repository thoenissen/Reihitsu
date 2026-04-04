using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.Indentation.Resources;

namespace Reihitsu.Formatter.Test.Integration.Rules.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.Indentation.IndentationAndAlignmentRule"/> — method-chain alignment
/// </summary>
[TestClass]
public class MethodChainAlignmentIntegrationTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that method chains are aligned correctly.
    /// </summary>
    [TestMethod]
    public void FormatsMethodChainAlignment()
    {
        // Arrange
        var input = TestData.MethodChainAlignmentTestData;
        var expected = TestData.MethodChainAlignmentResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}