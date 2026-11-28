using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Regions;

/// <summary>
/// Regression tests for region pairs that straddle type-body boundaries. The formatter keeps
/// both endpoints so a formatting pass never orphans a directive or deletes user-authored regions; a pair
/// that straddles a member body holds a region directive inside a member, which the formatter refuses (FMT-UNSUP-023)
/// </summary>
[TestClass]
public class StraddlingRegionPairTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a region opened among the members of a nested type and closed among the members of the outer type is
    /// preserved together with its matching closing directive and named
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-005")]
    public void RegionOpenedInsideANestedTypeBodyAndClosedOutsideIsPreservedAsAPair()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 private class D
                                 {
                                     #region members
                                     private int _x;
                                 }

                                 #endregion
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    private class D
                                    {
                                        #region Members

                                        private int _x;
                                    }

                                    #endregion // Members
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region pair fully outside any element body is preserved
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-005")]
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