using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.RegionFormatting;

namespace Reihitsu.Formatter.Test.Unit.RegionFormatting;

/// <summary>
/// Tests for <see cref="RegionNamingRewriter"/>, the naming half of the region phase. These pin the
/// capitalization and endregion synchronization independently of the nested-region removal
/// </summary>
[TestClass]
public class RegionNamingRewriterTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a lowercase region description is capitalized
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
        var actual = Rewrite(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an endregion directive without a comment receives the matching region name
    /// </summary>
    [TestMethod]
    public void SyncsEndregionComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region Methods

                                 void M() { }

                             #endregion // Wrong
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
        var actual = Rewrite(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a region without a description is left unchanged
    /// </summary>
    [TestMethod]
    public void LeavesRegionWithoutDescriptionUnchanged()
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
        var actual = Rewrite(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Runs the region naming rewriter over the given source and returns the result
    /// </summary>
    /// <param name="input">Source code to rewrite</param>
    /// <returns>The rewritten source code</returns>
    private string Rewrite(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var result = RegionNamingRewriter.Rewrite(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);

        return result.ToFullString();
    }

    #endregion // Methods
}