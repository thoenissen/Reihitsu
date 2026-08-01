using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for the contained-block brace placement: a statement whose brace anchor lives
/// outside the block must still be normalized by the first formatting pass, even when the block's
/// contents are re-flowed during the same pass
/// </summary>
[TestClass]
public class ContainedBlockBraceTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an unsafe statement block gets its opening brace moved to its own line in the
    /// same pass that re-flows its contents, matching the lock statement counterpart
    /// </summary>
    [TestMethod]
    public void UnsafeBlockBraceMovesWhenContentsAreReflowed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M()
                                 {
                                     unsafe {
                                         if (true) { int x = 1; }
                                     }

                                     lock (this) {
                                         if (true) { int y = 1; }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M()
                                    {
                                        unsafe
                                        {
                                            if (true)
                                            {
                                                int x = 1;
                                            }
                                        }

                                        lock (this)
                                        {
                                            if (true)
                                            {
                                                int y = 1;
                                            }
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a labeled statement block gets its opening brace moved to its own line in the
    /// same pass that re-flows its contents
    /// </summary>
    [TestMethod]
    public void LabeledBlockBraceMovesWhenContentsAreReflowed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     label: {
                                         if (true) { int x = 1; }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        label:
                                        {
                                            if (true)
                                            {
                                                int x = 1;
                                            }
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an unsafe statement block that is already laid out correctly stays untouched
    /// </summary>
    [TestMethod]
    public void FormattedUnsafeBlockRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M()
                                 {
                                     unsafe
                                     {
                                         int x = 1;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}