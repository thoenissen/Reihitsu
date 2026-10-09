using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — conditional access chains whose
/// source wraps between the <c>?</c> and the <c>.</c> of the operator
/// </summary>
[TestClass]
public class ConditionalAccessWrapBetweenOperatorTokensTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a chain wrapped between <c>?</c> and <c>.</c> on every link moves the <c>?</c> in front of
    /// the break so each conditional link starts its own aligned line with <c>?.</c>
    /// </summary>
    [TestMethod]
    public void ChainWrappedBetweenQuestionMarkAndDotStartsEachLinkWithConditionalAccess()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public int Add(List<string> list)
                                 {
                                     return list.Select(o => o)?
                                                .Select(o => o)?
                                                .Select(o => o);
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public int Add(List<string> list)
                                    {
                                        return list.Select(o => o)
                                                   ?.Select(o => o)
                                                   ?.Select(o => o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}