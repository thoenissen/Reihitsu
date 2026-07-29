using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — complex-element initializer alignment
/// </summary>
[TestClass]
public class ComplexElementInitializerAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a complex element with a single-line key and value remains on one line
    /// </summary>
    [TestMethod]
    public void SingleLineKeyAndValueRemainOnOneLine()
    {
        // Arrange
        const string input = """
                             var items = new Dictionary<Data, Data>()
                                         {
                                             { existingKey, existingValue }
                                         };
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line key is indented one level inside the complex element braces
    /// </summary>
    [TestMethod]
    public void MultiLineKeyAndSingleLineValueIndentRecursively()
    {
        // Arrange
        const string input = """
                             var items = new Dictionary<Data, Data>()
                                         {
                                             { new Data { A = 1, B = 2 }, existingValue }
                                         };
                             """;

        const string expected = """
                                var items = new Dictionary<Data, Data>()
                                            {
                                                {
                                                    new Data
                                                    {
                                                        A = 1,
                                                        B = 2
                                                    },
                                                    existingValue
                                                }
                                            };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line value is indented one level inside the complex element braces
    /// </summary>
    [TestMethod]
    public void SingleLineKeyAndMultiLineValueIndentRecursively()
    {
        // Arrange
        const string input = """
                             var items = new Dictionary<Data, Data>()
                                         {
                                             { existingKey, new Data { A = 3, B = 4 } }
                                         };
                             """;

        const string expected = """
                                var items = new Dictionary<Data, Data>()
                                            {
                                                {
                                                    existingKey,
                                                    new Data
                                                    {
                                                        A = 3,
                                                        B = 4
                                                    }
                                                }
                                            };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multi-line keys and values each retain their recursively nested indentation
    /// </summary>
    [TestMethod]
    public void MultiLineKeyAndValueIndentRecursively()
    {
        // Arrange
        const string input = """
                             var items = new Dictionary<Data, Data>()
                                         {
                                             { new Data { A = 5, B = 6 }, new Data { A = 7, B = 8 } }
                                         };
                             """;

        const string expected = """
                                var items = new Dictionary<Data, Data>()
                                            {
                                                {
                                                    new Data
                                                    {
                                                        A = 5,
                                                        B = 6
                                                    },
                                                    new Data
                                                    {
                                                        A = 7,
                                                        B = 8
                                                    }
                                                }
                                            };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}