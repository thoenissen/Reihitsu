using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests that the line-break phase leaves the whitespace in front of a line break it inserts to the cleanup phase
/// (FMT-BREAK-029)
/// </summary>
[TestClass]
public class LineBreakResidueWhitespaceTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The example of FMT-BREAK-029: a method body brace and an <c>if</c> body brace on the lines of their headers
    /// </summary>
    private const string Input = """
                                 class C
                                 {
                                     void M() {
                                         if (true) {
                                             M();
                                         }
                                     }
                                 }
                                 """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that the line-break phase's output keeps the space behind <c>void M()</c> and the space behind
    /// <c>if (true)</c>, in front of the line breaks it writes in front of the two moved braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-029")]
    public void LineBreakPhaseKeepsTheSpacesInFrontOfTheLineBreaksItInserts()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = ObservedPipeline.GetPhaseOutput(NormalizeLineEndings(Input, endOfLine), endOfLine, "LineBreakPhase").ToFullString();
            var description = $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{text}";

            Assert.Contains("void M() " + endOfLine, text, description);
            Assert.Contains("if (true) " + endOfLine, text, description);
        }
    }

    /// <summary>
    /// Verifies that the final output of the example of FMT-BREAK-029 holds neither space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-029")]
    public void FinalOutputHoldsNoWhitespaceInFrontOfTheInsertedLineBreaks()
    {
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        if (true)
                                        {
                                            M();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(Input, expected);
    }

    /// <summary>
    /// Verifies the reach FMT-BREAK-029 states: where the phase normalizes the gap in front of a <c>}</c> it places behind
    /// <c>M();</c>, the whitespace in front of that line break may go with the rewrite, and the final output holds none
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-029")]
    public void FinalOutputHoldsNoWhitespaceInFrontOfANormalizedBraceGap()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     if (true)
                                     {
                                         M();   }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        if (true)
                                        {
                                            M();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}