using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for compound assignment operators, which the formatter lays out as an assignment with <c>=</c>
/// (FMT-PIPE-014, FMT-BREAK-016)
/// </summary>
[TestClass]
public class CompoundAssignmentLayoutTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Wraps the given statements in a method body
    /// </summary>
    /// <param name="statements">The statements, indented for the method body</param>
    /// <returns>The source text</returns>
    private static string InMethod(string statements)
    {
        return $"class C\n{{\n    event System.EventHandler E;\n\n    void M(int x, int y, string s, int? n, System.EventHandler h)\n    {{\n{statements}\n    }}\n}}";
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the specification example: a compound assignment wrapped in front of its operator and one wrapped behind it
    /// are joined, while a wrapped binary operator keeps its wrap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    [Requirement("FMT-PIPE-014")]
    public void CompoundAssignmentsAreJoinedWhileBinaryOperatorKeepsItsWrap()
    {
        var input = InMethod("        x\n            += 1;\n        s ??=\n            \"x\";\n        x = x\n            + 1;");
        var expected = InMethod("        x += 1;\n        s ??= \"x\";\n        x = x\n            + 1;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a subtraction assignment wrapped in front of its operator is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void SubtractionAssignmentWrappedBeforeOperatorIsJoined()
    {
        var input = InMethod("        x\n            -= 1;");
        var expected = InMethod("        x -= 1;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a shift assignment wrapped behind its operator is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void ShiftAssignmentWrappedAfterOperatorIsJoined()
    {
        var input = InMethod("        x <<=\n            2;");
        var expected = InMethod("        x <<= 2;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lone unsigned-shift assignment operator is joined with its target and its value
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void LoneUnsignedShiftAssignmentOperatorIsJoined()
    {
        var input = InMethod("        x\n            >>>=\n            3;");
        var expected = InMethod("        x >>>= 3;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an event subscription wrapped in front of its operator is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void EventSubscriptionWrappedBeforeOperatorIsJoined()
    {
        var input = InMethod("        E\n            += h;");
        var expected = InMethod("        E += h;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a compound assignment in the incrementor of a <c>for</c> statement is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void CompoundAssignmentInForIncrementorIsJoined()
    {
        var input = InMethod("        for (var i = 0; i < 3; i\n                                   += 1)\n        {\n        }");
        var expected = InMethod("        for (var i = 0; i < 3; i += 1)\n        {\n        }");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a compound assignment that forms a lambda body is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void CompoundAssignmentAsLambdaBodyIsJoined()
    {
        var input = InMethod("        System.Action f = () => x\n                                    |= 1;");
        var expected = InMethod("        System.Action f = () => x |= 1;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a coalescing assignment nested in the value of a simple assignment is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void NestedCoalesceAssignmentIsJoined()
    {
        var input = InMethod("        var z = n = n\n                        ??= 5;");
        var expected = InMethod("        var z = n = n ??= 5;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a compound assignment inside an argument is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void CompoundAssignmentInsideArgumentIsJoined()
    {
        var input = InMethod("        System.Math.Abs(x\n                        += 1);");
        var expected = InMethod("        System.Math.Abs(x += 1);");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a compound assignment whose target line ends with a comment keeps its wrap, so the comment is not lost
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void CompoundAssignmentBehindEndOfLineCommentKeepsItsWrap()
    {
        var input = InMethod("        n // c\n            ??= 4;");

        AssertBlankLinesAbove(input, ("n // c", false), ("??= 4;", false));
    }

    /// <summary>
    /// Verifies that a compound assignment wrapped behind a directive keeps its wrap and the directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void CompoundAssignmentWrappedAroundDirectiveKeepsItsWrap()
    {
        var input = InMethod("        x\n#pragma warning disable CS0168\n            += 1;");

        AssertBlankLinesAbove(input, ("#pragma warning disable CS0168", false), ("+= 1;", false));
    }

    /// <summary>
    /// Verifies that a compound assignment on one line stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void OneLineCompoundAssignmentStays()
    {
        var input = InMethod("        x += 1;\n        s ??= \"x\";");

        AssertRuleResult(input);
    }

    #endregion // Tests
}