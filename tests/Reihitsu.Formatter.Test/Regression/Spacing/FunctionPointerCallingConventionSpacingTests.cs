using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the space behind a function pointer's <c>*</c> in front of its calling convention: one space, whether
/// the author wrote the pair on one line or the line-break phase joined it
/// </summary>
[TestClass]
public class FunctionPointerCallingConventionSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that <c>managed</c> and <c>unmanaged</c> get one space behind the <c>*</c>, with and without a calling-convention
    /// list, however the author spaced them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-027")]
    public void CallingConventionGetsOneSpaceBehindTheAsterisk()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 void M(delegate*unmanaged<int> a, delegate*managed<int> b, delegate*unmanaged[Cdecl, SuppressGCTransition]<int, void> c, delegate*  managed<int> d)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    void M(delegate* unmanaged<int> a, delegate* managed<int> b, delegate* unmanaged[Cdecl, SuppressGCTransition]<int, void> c, delegate* managed<int> d)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the rule: a function pointer without a calling convention keeps its <c>*</c> directly in front
    /// of its type-argument list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-027")]
    public void FunctionPointerWithoutCallingConventionKeepsItsGap()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 void M(delegate*<int, void> a)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a function pointer wrapped behind its <c>*</c> is joined and spaced like the same function pointer on one
    /// line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-027")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedCallingConventionIsSpacedLikeTheSameCallingConventionOnOneLine()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 void M(delegate*
                                        unmanaged[Cdecl]<int, void> f)
                                 {
                                 }

                                 void N(delegate*unmanaged[Cdecl]<int, void> f)
                                 {
                                 }

                                 void O(delegate*
                                        managed<int> g)
                                 {
                                 }

                                 void P(delegate*managed<int> g)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    void M(delegate* unmanaged[Cdecl]<int, void> f)
                                    {
                                    }

                                    void N(delegate* unmanaged[Cdecl]<int, void> f)
                                    {
                                    }

                                    void O(delegate* managed<int> g)
                                    {
                                    }

                                    void P(delegate* managed<int> g)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the <c>*</c> carries keeps its place, and the calling convention follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-027")]
    [Requirement("FMT-SPACE-013")]
    public void CommentBehindTheAsteriskKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 void M(delegate*/* a */unmanaged<int> f)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    void M(delegate*/* a */ unmanaged<int> f)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}