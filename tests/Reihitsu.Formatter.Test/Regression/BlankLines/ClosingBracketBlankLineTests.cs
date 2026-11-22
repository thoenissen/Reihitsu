using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank lines before the closing bracket of a collection expression or a list pattern and
/// before the closing parenthesis of a parenthesized pattern
/// </summary>
[TestClass]
public class ClosingBracketBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the blank line before the closing bracket of a collection expression and of a list pattern is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesBlankLineBeforeClosingBracketOfCollectionExpressionAndListPattern()
    {
        const string input = """
                             class C
                             {
                                 int[] F = [
                                     1,
                                     2

                                 ];

                                 void M(object o)
                                 {
                                     if (o is [
                                             1,
                                             2

                                         ])
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] F = [
                                                  1,
                                                  2
                                              ];

                                    void M(object o)
                                    {
                                        if (o is [
                                                     1,
                                                     2
                                                 ])
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line behind the opening bracket of a collection expression is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void KeepsBlankLineBehindOpeningBracketOfCollectionExpression()
    {
        const string input = """
                             class C
                             {
                                 int[] F = [

                                     1,
                                     2
                                 ];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] F = [

                                                  1,
                                                  2
                                              ];
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line before the closing parenthesis of a parenthesized pattern is removed. Only the blank
    /// lines are asserted: no requirement decides the columns of a parenthesized pattern's lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesBlankLineBeforeClosingParenthesisOfParenthesizedPattern()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (
                                         1 or 2

                                         );
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, (");", false));
    }

    /// <summary>
    /// Verifies that the blank line directly below an <c>#endregion</c> in front of the closing bracket of a collection
    /// expression is kept, and that a missing one is inserted. Only the blank lines are asserted: no requirement decides
    /// the column of a region directive inside a collection expression
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void KeepsAndInsertsBlankLineBelowEndRegionBeforeClosingBracket()
    {
        const string input = """
                             class C
                             {
                                 int[] G = [
                                     1,
                                     #region R
                                     2,
                                     #endregion // R

                                 ];

                                 void M()
                                 {
                                     Use([
                                         1,
                                         #region S
                                         2,
                                         #endregion // S
                                     ]);
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("];", true), ("]);", true));
    }

    /// <summary>
    /// Verifies that the blank line directly below an <c>#endregion</c> in front of the closing parenthesis of a
    /// parenthesized pattern is kept. Only the blank lines are asserted: no requirement decides the columns of a
    /// parenthesized pattern's lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void KeepsBlankLineBelowEndRegionBeforeClosingParenthesisOfParenthesizedPattern()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (
                                         1
                                         #region R
                                         or 2
                                         #endregion // R

                                         );
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, (");", true));
    }

    /// <summary>
    /// Verifies that before the closing bracket of a collection expression the blank line above an <c>#if</c> stays and
    /// the one below its <c>#endif</c> is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void RemovesOnlyBlankLineBelowDirectiveBeforeClosingBracket()
    {
        const string input = """
                             class C
                             {
                                 int[] F = [
                                     1,

                             #if DEBUG
                                     2,
                             #endif

                                 ];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] F = [
                                                  1,

                                #if DEBUG
                                        2,
                                #endif
                                              ];
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that before the closing parenthesis of a parenthesized pattern the blank line below an <c>#endif</c> is
    /// removed. Only the blank lines are asserted: no requirement decides the columns of a parenthesized pattern's lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void RemovesBlankLineBelowDirectiveBeforeClosingParenthesisOfParenthesizedPattern()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (
                                         1 or 2
                             #if DEBUG
                                         or 3
                             #endif

                                         );
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, (");", false));
    }

    /// <summary>
    /// Verifies that the blank line between a directive and a comment behind it, in front of a closing brace, is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void KeepsBlankLineBetweenDirectiveAndCommentBehindIt()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                             #if NEVER
                                     a++;
                             #endif

                                     // c
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Methods
}