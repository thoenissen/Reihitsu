using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Regions;

/// <summary>
/// Regression tests for a region pair that straddles an element-body boundary: one directive sits
/// inside a body while its match does not. Removing only the half that sits inside the body left
/// the other half orphaned, so formatting turned source that parsed cleanly into source that no
/// longer compiles (CS1028). The pair is removed as a unit instead, which also matches what
/// <c>RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider</c> does for the same input
/// </summary>
[TestClass]
public class StraddlingRegionPairTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a region opened between an indexer declaration and its accessor list is
    /// removed together with its matching closing directive
    /// </summary>
    [TestMethod]
    public void RegionStraddlingAnAccessorListIsRemovedAsAPair()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region Members

                                 public int this[int index]
                             #region Inner
                                 {
                                     get { return 0; }
                                 }
                             #endregion

                             #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Members

                                    public int this[int index]
                                    {
                                        get
                                        {
                                            return 0;
                                        }
                                    }

                                    #endregion // Members
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region opened between a method declaration and its body is removed together
    /// with its matching closing directive
    /// </summary>
    [TestMethod]
    public void RegionStraddlingAMethodBodyIsRemovedAsAPair()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region Members

                                 public int M()
                             #region Inner
                                 {
                                     return 0;
                                 }
                             #endregion

                             #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Members

                                    public int M()
                                    {
                                        return 0;
                                    }

                                    #endregion // Members
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region whose closing directive sits inside a method body is removed together
    /// with its opening directive, so no directive the analyzer reports survives formatting
    /// </summary>
    [TestMethod]
    public void RegionClosingInsideAMethodBodyIsRemovedAsAPair()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Members

                                 public void M()
                                 {
                                     #endregion
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region fully contained in a method body is still removed
    /// </summary>
    [TestMethod]
    public void RegionFullyInsideAMethodBodyIsStillRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public void M()
                                 {
                             #region Inner
                                     var value = 1;
                             #endregion
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public void M()
                                    {
                                        var value = 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region pair fully outside any element body is preserved
    /// </summary>
    [TestMethod]
    public void RegionAroundMembersRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Members

                                 public void M()
                                 {
                                 }

                                 #endregion // Members
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}