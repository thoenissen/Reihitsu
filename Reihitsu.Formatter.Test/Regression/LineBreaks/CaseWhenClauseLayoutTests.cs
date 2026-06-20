using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #322: layout rules for <c>case ... when</c> guard clauses. A guard on a
/// single-line pattern stays inline with the <c>case</c> label, while a guard on a multi-line pattern
/// wraps onto its own line indented four spaces past the <c>case</c> keyword
/// </summary>
[TestClass]
public class CaseWhenClauseLayoutTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a guard clause whose pattern fits on one line is collapsed onto the case label line
    /// </summary>
    [TestMethod]
    public void GuardOnSingleLinePatternIsCollapsedInline()
    {
        var input = """
                    internal class TestClass
                    {
                        void M(object value)
                        {
                            switch (value)
                            {
                                case int n
                                when n > 0:
                                    break;
                            }
                        }
                    }
                    """;

        var expected = """
                       internal class TestClass
                       {
                           void M(object value)
                           {
                               switch (value)
                               {
                                   case int n when n > 0:
                                       break;
                               }
                           }
                       }
                       """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an already inline guard clause is left unchanged
    /// </summary>
    [TestMethod]
    public void GuardThatIsAlreadyInlineIsLeftUnchanged()
    {
        var input = """
                    internal class TestClass
                    {
                        void M(object value)
                        {
                            switch (value)
                            {
                                case int n when n > 0:
                                    break;
                            }
                        }
                    }
                    """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a case pattern without a guard clause is left unchanged
    /// </summary>
    [TestMethod]
    public void CasePatternWithoutGuardIsLeftUnchanged()
    {
        var input = """
                    internal class TestClass
                    {
                        void M(object value)
                        {
                            switch (value)
                            {
                                case int n:
                                    break;
                            }
                        }
                    }
                    """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a guard clause on a multi-line pattern wraps onto its own line indented four
    /// spaces past the case keyword
    /// </summary>
    [TestMethod]
    public void GuardOnMultiLinePatternWrapsOntoOwnLine()
    {
        var input = """
                    internal class TestClass
                    {
                        void M(int value)
                        {
                            switch (value)
                            {
                                case 1
                                    or 2
                                    or 3 when value > 0:
                                    break;
                            }
                        }
                    }
                    """;

        var expected = """
                       internal class TestClass
                       {
                           void M(int value)
                           {
                               switch (value)
                               {
                                   case 1
                                        or 2
                                        or 3
                                       when value > 0:
                                       break;
                               }
                           }
                       }
                       """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}