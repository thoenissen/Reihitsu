using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — blank lines around region directives
/// </summary>
[TestClass]
public class BlankLineRegionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the blank line before #endregion is not inserted after the following #region instead.
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeEndRegionNotMovedToAfterNextRegion()
    {
        // Arrange
        const string input = """
            class C
            {
                #region Constructor

                public C(string value)
                {
                    Value = value;
                }

                public C(int value)
                {
                    Value = value.ToString();
                }
                #endregion // Constructor

                #region Properties

                public string Value { get; }

                #endregion // Properties
            }
            """;

        const string expected = """
            class C
            {
                #region Constructor

                public C(string value)
                {
                    Value = value;
                }

                public C(int value)
                {
                    Value = value.ToString();
                }

                #endregion // Constructor

                #region Properties

                public string Value { get; }

                #endregion // Properties
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}