using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests structural blank-line ownership in the full formatting pipeline
/// </summary>
[TestClass]
public class BlankLineStructureFullPipelineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that blank-line removal after an opening brace still happens in the full pipeline
    /// after cleanup stops owning that policy
    /// </summary>
    [TestMethod]
    public void RemovesBlankLineAfterOpenBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {

                                     var x = 1;
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single blank line before a multi-line block comment whose closing token sits on its own line
    /// is preserved instead of being doubled (see issue #307)
    /// </summary>
    [TestMethod]
    public void PreservesSingleBlankLineBeforeMultiLineBlockComment()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public void M()
                                 {
                                     var x = 1;

                                     /* line one
                                     line two
                                     */

                                     System.Console.WriteLine();
                                 }
                             }
                             """;
        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}