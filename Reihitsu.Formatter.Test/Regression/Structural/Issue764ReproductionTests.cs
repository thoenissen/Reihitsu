using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Reproduction-gate test for issue #764: an unbraced if/else where both branches are unbraced must place
/// the <c>else</c> keyword on its own line after the inserted if-block closing brace
/// </summary>
[TestClass]
public class Issue764ReproductionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Reproduces the issue's literal example verbatim: both the if-body and the else-body are unbraced,
    /// single statements
    /// </summary>
    [TestMethod]
    public void Issue764LiteralExampleBothBranchesUnbraced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool value)
                                 {
                                     if (value)
                                         DoSomething();
                                     else
                                         Thread.Sleep(100);
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool value)
                                    {
                                        if (value)
                                        {
                                            DoSomething();
                                        }
                                        else
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}