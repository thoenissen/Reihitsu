using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for block indentation by scope depth: namespaces, types, blocks, switch sections, labels, and unbraced embedded statements
/// </summary>
[TestClass]
public class ScopeIndentationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the members of a file-scoped namespace are moved back to column 0, because the namespace adds no level
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void FileScopedNamespaceAddsNoIndentationLevel()
    {
        const string input = """
                             namespace N;

                                 class C
                                 {
                                     void M()
                                     {
                                     }
                                 }
                             """;
        const string expected = """
                                namespace N;

                                class C
                                {
                                    void M()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a label is indented like any other statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void LabeledStatementIsIndentedToTheStatementColumn()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                 retry:
                                     x--;

                                     if (x > 0)
                                     {
                                         goto retry;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        retry:
                                        x--;

                                        if (x > 0)
                                        {
                                            goto retry;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that top-level statements are moved to column 0 while their blocks still add a level
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void TopLevelStatementsAddNoIndentationLevel()
    {
        const string input = """
                               var x = 1;
                                 if (x > 0)
                                 {
                               System.Console.WriteLine(x);
                                 }
                             """;
        const string expected = """
                                var x = 1;
                                if (x > 0)
                                {
                                    System.Console.WriteLine(x);
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: each enclosing namespace, type, and block adds one level and braces stay at their owner's level
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void NestedScopesAddOneLevelEach()
    {
        const string input = """
                             namespace N
                             {
                             class C
                             {
                                   int _value;

                               void M()
                               {
                                       {
                                 M();
                                       }
                               }
                             }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    class C
                                    {
                                        int _value;

                                        void M()
                                        {
                                            {
                                                M();
                                            }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: labels are one level deeper than the switch keyword and statements one level deeper than their labels
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-003")]
    public void SwitchLabelsAndStatementsAreIndentedOneLevelEach()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                     case 1:
                                     M(2);
                                     break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                M(2);
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block forming a switch section's body is indented like a statement of the section
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-003")]
    public void BracedSwitchSectionBodyIsIndentedLikeAStatement()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                     {
                                     int y = x;
                                     M(y);
                                     break;
                                     }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = x;

                                                    M(y);

                                                    break;
                                                }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: an unbraced loop body is indented one level deeper than its owner
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-004")]
    public void UnbracedWhileBodyIsIndentedOneLevel()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     while (x < 10)
                                     x++;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        while (x < 10)
                                            x++;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the unbraced body of a do statement is indented one level deeper than the do keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-004")]
    public void UnbracedDoBodyIsIndentedOneLevel()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     do
                                     x++;
                                     while (x < 10);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        do
                                            x++;
                                        while (x < 10);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}