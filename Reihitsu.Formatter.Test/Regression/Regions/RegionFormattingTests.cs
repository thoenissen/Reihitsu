using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Regions;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class RegionFormattingTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a region name starting with a lowercase letter is capitalized
    /// </summary>
    [TestMethod]
    public void LowercaseRegionNameCapitalizesFirstLetter()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region methods

                                 void M()
                                 {
                                 }

                                 #endregion // methods
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Methods

                                    void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region name already starting with an uppercase letter is not changed
    /// </summary>
    [TestMethod]
    public void AlreadyUppercaseNoChange()
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

        const string expected = """
                                class C
                                {
                                    #region Methods

                                    void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a missing endregion comment is added to match the region name
    /// </summary>
    [TestMethod]
    public void EndRegionCommentSynchronizedWithRegionName()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Methods

                                 void M()
                                 {
                                 }

                                 #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Methods

                                    void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a correct endregion comment is not modified
    /// </summary>
    [TestMethod]
    public void EndRegionCommentAlreadyCorrectNoChange()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Constructor

                                 C()
                                 {
                                 }

                                 #endregion // Constructor
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    #region Constructor

                                    C()
                                    {
                                    }

                                    #endregion // Constructor
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a mismatched endregion comment is corrected to match the region name
    /// </summary>
    [TestMethod]
    public void MismatchedEndRegionCommentCorrected()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Methods

                                 void M()
                                 {
                                 }

                                 #endregion // Wrong
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Methods

                                    void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nested region pairs are both corrected
    /// </summary>
    [TestMethod]
    public void NestedRegionsBothCorrected()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region outer

                                 #region inner

                                 void M()
                                 {
                                 }

                                 #endregion

                                 #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Outer

                                    #region Inner

                                    void M()
                                    {
                                    }

                                    #endregion // Inner

                                    #endregion // Outer
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region directive without a name is skipped and not modified
    /// </summary>
    [TestMethod]
    public void RegionWithoutNameIsSkipped()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region

                                 void M()
                                 {
                                 }

                                 #endregion
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that multiple region pairs in one file are all corrected
    /// </summary>
    [TestMethod]
    public void MultipleRegionsAllCorrected()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region fields

                                 int _x;

                                 #endregion

                                 #region methods

                                 void M()
                                 {
                                 }

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

                                    void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that misindented region directives are aligned with their containing code
    /// </summary>
    [TestMethod]
    public void MisindentedRegionDirectivesAreAlignedWithContainingCode()
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
        const string expected = """
                                class C
                                {
                                    #region Methods

                                    void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that code without any region directives is not modified
    /// </summary>
    [TestMethod]
    public void NoRegionsNoChanges()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}