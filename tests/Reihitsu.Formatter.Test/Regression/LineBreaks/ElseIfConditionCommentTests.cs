using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// A comment trailing the condition of an <c>if</c> or an <c>else if</c> stays behind the condition, so it neither splits an
/// <c>else if</c> across two lines nor leaves the line it was written on
/// </summary>
[TestClass]
public class ElseIfConditionCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a trailing comment on an <c>else if</c> condition is left in place instead of being
    /// moved between the <c>else</c> and <c>if</c> keywords
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public void TrailingCommentOnElseIfConditionDoesNotSplitElseIf()
    {
        var input = """
                    public class C
                    {
                        void M(bool x, bool y)
                        {
                            if (x)
                            {
                            }
                            else if (y) // note
                            {
                            }
                        }
                    }
                    """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a trailing comment on a plain (non-else) <c>if</c> condition stays behind the condition instead of
    /// moving onto its own line above the <c>if</c> keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public void TrailingCommentOnPlainIfConditionStaysBehindCondition()
    {
        var input = """
                    public class C
                    {
                        void M(bool x)
                        {
                            if (x) // note
                            {
                            }
                        }
                    }
                    """;

        AssertRuleResult(input);
    }

    #endregion // Tests
}