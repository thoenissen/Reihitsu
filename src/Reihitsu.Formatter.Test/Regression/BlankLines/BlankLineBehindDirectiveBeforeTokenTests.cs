using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank lines next to a non-region directive in front of the <c>while</c> keyword and in front
/// of the opening brace of a collection initializer assigned to a member
/// </summary>
[TestClass]
public class BlankLineBehindDirectiveBeforeTokenTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the author's blank line between a trailing non-region directive and a <c>while</c> statement is kept,
    /// because a <c>while</c> statement continues no statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-008")]
    [Requirement("FMT-BLANK-009")]
    public void KeepsBlankLineBetweenDirectiveAndWhileStatement()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #pragma warning disable CS0168

                                     while (a > 0)
                                     {
                                         a--;
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the blank line between a trailing non-region directive and the <c>while</c> of a do statement is
    /// removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-008")]
    [Requirement("FMT-BLANK-009")]
    public void RemovesBlankLineBetweenDirectiveAndDoStatementWhile()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     do
                                     {
                                         a--;
                                     }
                             #pragma warning disable CS0168

                                     while (a > 0);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        do
                                        {
                                            a--;
                                        }
                                #pragma warning disable CS0168
                                        while (a > 0);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the author's blank line between the <c>=</c> of a collection initializer assigned to a member and a
    /// directive in front of its opening brace is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    [Requirement("FMT-BREAK-030")]
    public void KeepsBlankLineAboveDirectiveInFrontOfAssignedCollectionInitializerBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L =

                             #pragma warning disable CS0168
                                                 {
                                                     1
                                                 }
                                             };
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the blank line between a directive and the opening brace of a collection initializer assigned to a
    /// member is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    [Requirement("FMT-BREAK-030")]
    public void RemovesBlankLineBetweenDirectiveAndAssignedCollectionInitializerBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L =
                             #pragma warning disable CS0168

                                                 {
                                                     1
                                                 }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var p = new P
                                                {
                                                    L =
                                #pragma warning disable CS0168
                                                    {
                                                        1
                                                    }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted next to a directive between the <c>=</c> of a collection initializer
    /// assigned to a member and its opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    [Requirement("FMT-BREAK-030")]
    public void InsertsNoBlankLineNextToDirectiveInFrontOfAssignedCollectionInitializerBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L =
                             #pragma warning disable CS0168
                                                 {
                                                     1
                                                 }
                                             };
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Methods
}