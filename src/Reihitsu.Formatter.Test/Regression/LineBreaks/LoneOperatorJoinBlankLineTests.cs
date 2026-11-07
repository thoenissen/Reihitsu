using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for an operator that the line-break phase moves to the start of the continuation line while the
/// operand in front of it already ends its line: the phase joins the operator with the operand below it and writes no
/// blank line into the gap it keeps (FMT-BREAK-030)
/// </summary>
[TestClass]
public class LoneOperatorJoinBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Wraps the given statements in a method body
    /// </summary>
    /// <param name="statements">The statements, indented for the method body</param>
    /// <returns>The source text</returns>
    private static string InMethod(string statements)
    {
        return $"class C\n{{\n    bool M(bool a, bool b, int i, object o, string s)\n    {{\n{statements}\n        return a;\n    }}\n}}";
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the reported example: a lone <c>&amp;&amp;</c> and a lone <c>?</c> are joined with their operands without a
    /// blank line inside either expression, while the blank line in front of <c>return</c> stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    [Requirement("FMT-BREAK-017")]
    [Requirement("FMT-BREAK-018")]
    public void LoneLogicalAndConditionalOperatorsAreJoinedWithoutBlankLine()
    {
        const string input = """
                             class C
                             {
                                 bool M(bool a, bool b)
                                 {
                                     var x = a
                                             &&
                                             b;
                                     var y = a
                                             ?
                                             1
                                             : 2;
                                     return x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        var x = a
                                                && b;
                                        var y = a
                                                    ? 1
                                                    : 2;

                                        return x;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone arithmetic operator is joined with its right operand without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void LoneArithmeticOperatorIsJoinedWithoutBlankLine()
    {
        var input = InMethod("        var p = i\n                +\n                1;");
        var expected = InMethod("        var p = i\n                + 1;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone null-coalescing operator is joined with its right operand without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void LoneCoalesceOperatorIsJoinedWithoutBlankLine()
    {
        var input = InMethod("        var p = s\n                ??\n                \"x\";");
        var expected = InMethod("        var p = s\n                ?? \"x\";\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone <c>as</c> operator is joined with its type without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void LoneAsOperatorIsJoinedWithoutBlankLine()
    {
        var input = InMethod("        var p = o\n                as\n                string;");
        var expected = InMethod("        var p = o\n                as string;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone operator behind an operand whose line ends with a comment is joined without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void LoneOperatorBehindEndOfLineCommentIsJoinedWithoutBlankLine()
    {
        var input = InMethod("        var p = a // c\n                &&\n                b;");
        var expected = InMethod("        var p = a // c\n                && b;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone operator below a comment line between the operands is joined without a blank line above the
    /// comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void LoneOperatorBelowCommentLineIsJoinedWithoutBlankLine()
    {
        var input = InMethod("        var p = a\n                // between\n                &&\n                b;");
        var expected = InMethod("        var p = a\n                // between\n                && b;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every lone operator of a chain of binary operators is joined without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void SeveralLoneOperatorsAreJoinedWithoutBlankLines()
    {
        var input = InMethod("        var p = a\n                ||\n                b\n                ||\n                a;");
        var expected = InMethod("        var p = a\n                || b\n                || a;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone conditional <c>:</c> is joined with the false branch without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    [Requirement("FMT-BREAK-018")]
    public void LoneConditionalColonIsJoinedWithoutBlankLine()
    {
        var input = InMethod("        var p = a\n                ? 1\n                :\n                2;");
        var expected = InMethod("        var p = a\n                    ? 1\n                    : 2;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone conditional <c>:</c> of a conditional written with its <c>?</c> on the condition line is joined
    /// without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    [Requirement("FMT-BREAK-018")]
    public void LoneConditionalColonBehindSameLineQuestionIsJoinedWithoutBlankLine()
    {
        var input = InMethod("        var p = a ? 1\n                :\n                2;");
        var expected = InMethod("        var p = a\n                    ? 1\n                    : 2;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone conditional <c>?</c> behind a condition whose line ends with a comment is joined without a blank
    /// line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    [Requirement("FMT-BREAK-018")]
    public void LoneConditionalQuestionBehindEndOfLineCommentIsJoinedWithoutBlankLine()
    {
        var input = InMethod("        var p = a // c\n                ?\n                1\n                : 2;");
        var expected = InMethod("        var p = a // c\n                    ? 1\n                    : 2;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone conditional <c>:</c> below a comment line is joined without a blank line above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    [Requirement("FMT-BREAK-018")]
    public void LoneConditionalColonBelowCommentLineIsJoinedWithoutBlankLine()
    {
        var input = InMethod("        var p = a\n                ? 1\n                // c\n                :\n                2;");
        var expected = InMethod("        var p = a\n                    ? 1\n                    // c\n                    : 2;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an operator written at the end of its operand's line is moved to the continuation line without a blank
    /// line, as before
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    [Requirement("FMT-BREAK-030")]
    public void OperatorAtEndOfOperandLineMovesWithoutBlankLine()
    {
        var input = InMethod("        var p = a &&\n                b;");
        var expected = InMethod("        var p = a\n                && b;\n");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone operator followed by a comment keeps its line and gains no blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void LoneOperatorFollowedByCommentGainsNoBlankLine()
    {
        var input = InMethod("        var p = a\n                && // c\n                b;");

        AssertBlankLinesAbove(input, ("&& // c", false), ("b;", false));
    }

    /// <summary>
    /// Verifies that a blank line the author wrote above an operator stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void AuthorBlankLineAboveOperatorStays()
    {
        var input = InMethod("        var p = a\n\n                && b;");
        var expected = InMethod("        var p = a\n\n                && b;\n");

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}