using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — chain line breaks inside
/// an argument or lambda body of a conditional access chain (issue #475)
/// </summary>
[TestClass]
public class ChainInsideConditionalAccessArgumentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a collapsible chain nested in a lambda argument of a conditional access chain
    /// is rejoined onto the root line
    /// </summary>
    [TestMethod]
    public void CollapsibleChainInsideLambdaArgumentOfConditionalAccessIsRejoined()
    {
        // Arrange
        const string input = """
                             public class Class
                             {
                                 public void M()
                                 {
                                     var value = a.List.Find(e => d
                                                                   .Equals(e.Number))
                                                       ?.Value;
                                 }
                             }
                             """;

        const string expected = """
                                public class Class
                                {
                                    public void M()
                                    {
                                        var value = a.List.Find(e => d.Equals(e.Number))
                                                          ?.Value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped chain nested in a lambda argument of a conditional access chain is
    /// normalized. The chain has an intermediate member access, so it stays wrapped instead of being
    /// rejoined, and the trailing link is moved onto its own line and aligned to the first dot
    /// </summary>
    [TestMethod]
    public void WrappedChainInsideLambdaArgumentOfConditionalAccessIsNormalized()
    {
        // Arrange
        const string input = """
                             public class Class
                             {
                                 public void M()
                                 {
                                     var value = a.List.Find(e => e.Number
                                                                   .ToString().Trim())
                                                       ?.Value;
                                 }
                             }
                             """;

        const string expected = """
                                public class Class
                                {
                                    public void M()
                                    {
                                        var value = a.List.Find(e => e.Number
                                                                      .ToString()
                                                                      .Trim())
                                                          ?.Value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}