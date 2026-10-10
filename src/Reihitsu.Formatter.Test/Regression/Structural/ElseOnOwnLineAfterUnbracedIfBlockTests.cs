using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Reproduction-gate test verifying that an unbraced if/else where both branches are unbraced places
/// the <c>else</c> keyword on its own line after the inserted if-block closing brace
/// </summary>
[TestClass]
public class ElseOnOwnLineAfterUnbracedIfBlockTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Reproduces the reported scenario verbatim: both the if-body and the else-body are unbraced,
    /// single statements
    /// </summary>
    [TestMethod]
    public void LiteralExampleBothBranchesUnbraced()
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