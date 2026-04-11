using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — switch-case brace insertion
/// </summary>
[TestClass]
public class SwitchCaseBracesTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that braces are correctly added around multi-statement type-pattern switch sections.
    /// </summary>
    [TestMethod]
    public void TypePatternCaseWithMultipleStatementsAddsBracesCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
                bool M(object value)
                {
                    var current = value;

                    while (current != null)
                    {
                        switch (current)
                        {
                            case string:
                            case int:
                                current = current.ToString();
                                continue;
                            case bool:
                            case double:
                                return true;
                        }

                        break;
                    }

                    return false;
                }
            }
            """;

        const string expected = """
            class C
            {
                bool M(object value)
                {
                    var current = value;

                    while (current != null)
                    {
                        switch (current)
                        {
                            case string:
                            case int:
                                {
                                    current = current.ToString();

                                    continue;
                                }
                            case bool:
                            case double:
                                {
                                    return true;
                                }
                        }

                        break;
                    }

                    return false;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}