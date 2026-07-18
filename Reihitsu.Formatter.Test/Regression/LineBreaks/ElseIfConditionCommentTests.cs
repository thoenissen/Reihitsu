using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #424: moving a trailing <c>if</c>-condition comment onto its own line must
/// not split an <c>else if</c> across two lines by inserting the comment between the <c>else</c> and
/// <c>if</c> keywords
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
    /// Verifies that a trailing comment on a plain (non-else) <c>if</c> condition still moves onto its own
    /// line above the <c>if</c> keyword
    /// </summary>
    [TestMethod]
    public void TrailingCommentOnPlainIfConditionMovesToOwnLine()
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

        var expected = """
                       public class C
                       {
                           void M(bool x)
                           {
                               // note
                               if (x)
                               {
                               }
                           }
                       }
                       """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}