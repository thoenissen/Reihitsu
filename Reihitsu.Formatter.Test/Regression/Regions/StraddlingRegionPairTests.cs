using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Regions;

/// <summary>
/// Regression tests for region pairs that straddle element-body boundaries. The formatter keeps
/// both endpoints so a formatting pass never orphans a directive or deletes user-authored regions
/// </summary>
[TestClass]
public class StraddlingRegionPairTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a region opened between an indexer declaration and its accessor list is
    /// preserved together with its matching closing directive
    /// </summary>
    [TestMethod]
    public void RegionStraddlingAnAccessorListIsPreservedAsAPair()
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

                                    #region Inner

                                    {
                                        get
                                        {
                                            return 0;
                                        }
                                    }

                                    #endregion // Inner

                                    #endregion // Members
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region opened between a method declaration and its body is preserved together
    /// with its matching closing directive
    /// </summary>
    [TestMethod]
    public void RegionStraddlingAMethodBodyIsPreservedAsAPair()
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

                                    #region Inner

                                    {
                                        return 0;
                                    }

                                    #endregion // Inner

                                    #endregion // Members
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region whose closing directive sits inside a method body remains paired with
    /// its opening directive
    /// </summary>
    [TestMethod]
    public void RegionClosingInsideAMethodBodyIsPreservedAsAPair()
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
                                    #region Members

                                    public void M()
                                    {
                                        #endregion // Members
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region fully contained in a method body is preserved and formatted
    /// </summary>
    [TestMethod]
    public void RegionFullyInsideAMethodBodyIsPreservedAndFormatted()
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
                                        #region Inner

                                        var value = 1;

                                        #endregion // Inner
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