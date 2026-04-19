using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.RegionFormatting;

namespace Reihitsu.Formatter.Test.Unit.RegionFormatting;

/// <summary>
/// Tests for <see cref="RegionFormattingPhase"/>
/// </summary>
[TestClass]
public class RegionFormattingPhaseTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a lowercase region description is capitalized.
    /// </summary>
    [TestMethod]
    public void CapitalizesRegionDescription()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region methods

                                 void M() { }

                             #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #region Methods

                                    void M() { }

                                #endregion // Methods
                                }
                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = RegionFormattingPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an endregion directive without a comment receives the matching region name as a comment.
    /// </summary>
    [TestMethod]
    public void SyncsEndregionWithRegion()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region Methods

                                 void M() { }

                             #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #region Methods

                                    void M() { }

                                #endregion // Methods
                                }
                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = RegionFormattingPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a region that is already correctly capitalized and has a matching endregion comment is not modified.
    /// </summary>
    [TestMethod]
    public void PreservesAlreadyCorrectRegion()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region Methods

                                 void M() { }

                             #endregion // Methods
                             }
                             """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = RegionFormattingPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that multiple sequential regions are each independently formatted.
    /// </summary>
    [TestMethod]
    public void HandlesMultipleRegions()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region fields

                                 int _x;

                             #endregion

                             #region methods

                                 void M() { }

                             #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #region Fields

                                    int _x;

                                #endregion // Fields

                                #region Methods

                                    void M() { }

                                #endregion // Methods
                                }
                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = RegionFormattingPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that nested regions are correctly paired and each receives the correct capitalization and endregion comment.
    /// </summary>
    [TestMethod]
    public void HandlesNestedRegions()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region outer

                             #region inner

                                 void M() { }

                             #endregion

                             #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #region Outer

                                #region Inner

                                    void M() { }

                                #endregion // Inner

                                #endregion // Outer
                                }
                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = RegionFormattingPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a region with no description is left unchanged.
    /// </summary>
    [TestMethod]
    public void HandlesRegionWithoutDescription()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region

                                 void M() { }

                             #endregion
                             }
                             """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = RegionFormattingPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that an endregion with an incorrect trailing comment is replaced with the correct one matching the region name.
    /// </summary>
    [TestMethod]
    public void HandlesEndregionWithMismatchedDescription()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region Methods

                                 void M() { }

                             #endregion // WrongName
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #region Methods

                                    void M() { }

                                #endregion // Methods
                                }
                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = RegionFormattingPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a region whose description already starts with an uppercase letter is not modified.
    /// </summary>
    [TestMethod]
    public void PreservesRegionWithCorrectCasing()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region Properties

                                 int X { get; set; }

                             #endregion // Properties
                             }
                             """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = RegionFormattingPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual);
    }

    #endregion // Methods
}