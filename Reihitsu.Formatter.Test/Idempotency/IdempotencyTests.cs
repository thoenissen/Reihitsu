using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.BlankLines.Resources;

using CleanupTestData = Reihitsu.Formatter.Test.Integration.Rules.Cleanup.Resources.TestData;
using IndentationTestData = Reihitsu.Formatter.Test.Integration.Rules.Indentation.Resources.TestData;
using RegionsTestData = Reihitsu.Formatter.Test.Integration.Rules.Regions.Resources.TestData;
using SpacingTestData = Reihitsu.Formatter.Test.Integration.Rules.Spacing.Resources.TestData;
using StructuralTestData = Reihitsu.Formatter.Test.Integration.Rules.Structural.Resources.TestData;

namespace Reihitsu.Formatter.Test.Idempotency;

/// <summary>
/// Verifies that the formatter produces stable output when applied multiple times.
/// </summary>
[TestClass]
public class IdempotencyTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that applying the formatter twice to BlankLineBeforeStatement test data produces the same result.
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeStatementIsIdempotent()
    {
        // Arrange
        var input = TestData.BlankLineBeforeStatementTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to BlankLineAfterStatement test data produces the same result.
    /// </summary>
    [TestMethod]
    public void BlankLineAfterStatementIsIdempotent()
    {
        // Arrange
        var input = TestData.BlankLineAfterStatementTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that already-formatted code produces no changes when formatted again.
    /// </summary>
    [TestMethod]
    public void AlreadyFormattedCodeProducesNoChanges()
    {
        // Arrange
        var input = TestData.BlankLineBeforeStatementResultData;

        // Act
        var formatted = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = formatted.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ExpressionBodiedMethod test data produces the same result.
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedMethodIsIdempotent()
    {
        // Arrange
        var input = StructuralTestData.ExpressionBodiedMethodTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ExpressionBodiedConstructor test data produces the same result.
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedConstructorIsIdempotent()
    {
        // Arrange
        var input = StructuralTestData.ExpressionBodiedConstructorTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to RegionFormatting test data produces the same result.
    /// </summary>
    [TestMethod]
    public void RegionFormattingIsIdempotent()
    {
        // Arrange
        var input = RegionsTestData.RegionFormattingTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to TrailingTriviaCleanup test data produces the same result.
    /// </summary>
    [TestMethod]
    public void TrailingTriviaCleanupIsIdempotent()
    {
        // Arrange
        var input = CleanupTestData.TrailingTriviaCleanupTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to Indentation test data produces the same result.
    /// </summary>
    [TestMethod]
    public void IndentationIsIdempotent()
    {
        // Arrange
        var input = IndentationTestData.IndentationTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to HorizontalSpacing test data produces the same result.
    /// </summary>
    [TestMethod]
    public void HorizontalSpacingIsIdempotent()
    {
        // Arrange
        var input = SpacingTestData.HorizontalSpacingTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ObjectInitializerLayout test data produces the same result.
    /// </summary>
    [TestMethod]
    public void ObjectInitializerLayoutIsIdempotent()
    {
        // Arrange
        var input = IndentationTestData.ObjectInitializerLayoutTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to MethodChainAlignment test data produces the same result.
    /// </summary>
    [TestMethod]
    public void MethodChainAlignmentIsIdempotent()
    {
        // Arrange
        var input = IndentationTestData.MethodChainAlignmentTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to LogicalExpressionLayout test data produces the same result.
    /// </summary>
    [TestMethod]
    public void LogicalExpressionLayoutIsIdempotent()
    {
        // Arrange
        var input = IndentationTestData.LogicalExpressionLayoutTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    #endregion // Methods
}