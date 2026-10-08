using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Regression tests for a documentation comment written behind a statement when a block statement follows it
/// </summary>
[TestClass]
public class DelimitedDocumentationBeforeBlockStatementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement is separated from the following block statement in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     var a = 0; /** Doc */
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M()
                                    {
                                        var a = 0;

                                        /** Doc */
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line documentation comment behind a statement is separated from the following block statement in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesSingleLineDocumentationBeforeBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     var a = 0; /// Doc
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M()
                                    {
                                        var a = 0;

                                        /// Doc
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement is separated from a following block statement inside a braced switch section in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeBlockStatementInSwitchSectionInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(int v)
                                 {
                                     switch (v)
                                     {
                                         case 1:
                                             {
                                                 M(2); /** Doc */
                                                 {
                                                     M(3);
                                                 }
                                             }

                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M(int v)
                                    {
                                        switch (v)
                                        {
                                            case 1:
                                                {
                                                    M(2);

                                                    /** Doc */
                                                    {
                                                        M(3);
                                                    }
                                                }

                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the layout the issue reports as the first-pass result is already stable
    /// </summary>
    [TestMethod]
    public void KeepsSecondPassLayoutOfDelimitedDocumentationBeforeBlockStatement()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     var a = 0;
                                     /** Doc */
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement is separated from a following if statement in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeIfStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(bool b)
                                 {
                                     var a = 0; /** Doc */
                                     if (b)
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M(bool b)
                                    {
                                        var a = 0;

                                        /** Doc */
                                        if (b)
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}