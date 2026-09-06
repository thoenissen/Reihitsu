using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Reproduction test for issue #764's follow-up scenario: an <c>if</c>/<c>else</c> whose braces are
/// already hand-authored (not synthesized by <c>ControlFlowBraceTransform</c>), written with
/// <c>} else</c> glued together on the same line
/// </summary>
[TestClass]
public class AlreadyBracedElsePlacementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a hand-authored, already-fully-braced <c>if</c>/<c>else</c> with <c>} else</c>
    /// glued on the same line has <c>else</c> moved onto its own line
    /// </summary>
    [TestMethod]
    public void AlreadyBracedIfElseWithGluedElseMovesElseOntoItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool value)
                                 {
                                     if (value)
                                     {
                                         DoSomething();
                                     } else
                                     {
                                         Thread.Sleep(100);
                                     }
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