using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank lines next to a non-region directive in front of the <c>while</c> keyword, and for the
/// refusal of a directive in front of the opening brace of a collection initializer assigned to a member
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
    /// Verifies that a directive between the <c>=</c> of a collection initializer assigned to a member and its opening brace,
    /// behind the author's blank line, stands behind an assignment operator, so the input is refused and returned unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void DirectiveBehindAssignmentInFrontOfCollectionInitializerBraceBehindBlankLineIsUnsupported()
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

        AssertUnsupportedFor(AssignmentCommentReason, input, "#pragma warning disable CS0168");
    }

    /// <summary>
    /// Verifies that a directive between the <c>=</c> of a collection initializer assigned to a member and its opening brace,
    /// with a blank line in front of the brace, stands behind an assignment operator, so the input is refused and returned
    /// unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void DirectiveBehindAssignmentWithBlankLineInFrontOfCollectionInitializerBraceIsUnsupported()
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

        AssertUnsupportedFor(AssignmentCommentReason, input, "#pragma warning disable CS0168");
    }

    /// <summary>
    /// Verifies that a directive between the <c>=</c> of a collection initializer assigned to a member and its opening brace
    /// stands behind an assignment operator, so the input is refused and returned unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void DirectiveBehindAssignmentInFrontOfCollectionInitializerBraceIsUnsupported()
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

        AssertUnsupportedFor(AssignmentCommentReason, input, "#pragma warning disable CS0168");
    }

    #endregion // Methods
}