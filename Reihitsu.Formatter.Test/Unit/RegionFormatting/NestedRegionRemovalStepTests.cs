using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.RegionFormatting;

namespace Reihitsu.Formatter.Test.Unit.RegionFormatting;

/// <summary>
/// Tests for <see cref="NestedRegionRemovalStep"/>, the removal half of the region phase. These pin
/// the reparse-based removal of region directives inside element bodies independently of the naming
/// </summary>
[TestClass]
public class NestedRegionRemovalStepTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a region directive placed inside a method body is removed
    /// </summary>
    [TestMethod]
    public void RemovesRegionInsideMethodBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     #region Inner
                                     var x = 1;
                                     #endregion
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;
                                    }
                                }
                                """;

        // Act
        var actual = Remove(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a region directive at member level is left untouched
    /// </summary>
    [TestMethod]
    public void KeepsMemberLevelRegion()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Methods

                                 void M()
                                 {
                                 }

                                 #endregion // Methods
                             }
                             """;

        // Act
        var actual = Remove(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Runs the nested-region removal step over the given source and returns the result
    /// </summary>
    /// <param name="input">Source code to process</param>
    /// <returns>The processed source code</returns>
    private string Remove(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var result = NestedRegionRemovalStep.Remove(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);

        return result.ToFullString();
    }

    #endregion // Methods
}