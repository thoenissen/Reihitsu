using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the separator the line-break phase writes where it joins two tokens: two tokens that would fuse keep
/// one space, so the output compiles, and a joined pair no spacing rule covers keeps the minimal separator, like the same pair
/// written on one line
/// </summary>
[TestClass]
public class JoinSeparatorTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the final output of the requirement example
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void RequirementExampleIsSpacedByTheSpacingPhase()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int P
                                     => 1;

                                 public int Q
                                 {
                                     get;
                                     protected
                                     internal set;
                                 }

                                 unsafe int M(bool b, int a, int* p)
                                 {
                                     N(name:
                                       a);

                                     var x = a -
                                             -a;
                                     var y = a /
                                             *p;

                                     return b ?
                                            1 :
                                            2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int P => 1;

                                    public int Q { get; protected internal set; }

                                    unsafe int M(bool b, int a, int* p)
                                    {
                                        N(name: a);

                                        var x = a
                                                - -a;
                                        var y = a
                                                / *p;

                                        return b
                                                   ? 1
                                                   : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>protected</c> and <c>internal</c> on two lines of a collapsed accessor list stay two keywords
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void ProtectedInternalOnTwoLinesOfAnAccessorListStaysTwoKeywords()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int P
                                 {
                                     get;
                                     protected
                                     internal set;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int P { get; protected internal set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>internal</c> and <c>protected</c> on two lines of a collapsed accessor list stay two keywords
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void InternalProtectedOnTwoLinesOfAnAccessorListStaysTwoKeywords()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int P
                                 {
                                     get;
                                     internal
                                     protected set;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int P { get; internal protected set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>private</c> and <c>protected</c> on two lines of a collapsed accessor list stay two keywords
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void PrivateProtectedOnTwoLinesOfAnAccessorListStaysTwoKeywords()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int Q
                                 {
                                     get;
                                     private
                                     protected set;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int Q { get; private protected set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>protected</c> and <c>private</c> on two lines of a collapsed accessor list stay two keywords
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void ProtectedPrivateOnTwoLinesOfAnAccessorListStaysTwoKeywords()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int Q
                                 {
                                     get;
                                     protected
                                     private set;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int Q { get; protected private set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that accessor modifiers split over lines in front of the first accessor stay two keywords, also in an indexer's
    /// accessor list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void AccessorModifiersInFrontOfTheFirstAccessorOfAnIndexerStayTwoKeywords()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int this[int i]
                                 {
                                     protected
                                     internal get;
                                     set;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int this[int i] { protected internal get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that binary operators moved to the start of the next line in front of an operand they would fuse with are
    /// spaced in the final output
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void BinaryOperatorsMovedInFrontOfAFusingOperandAreSpaced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(int a, int* p, object o)
                                 {
                                     var b = a +
                                             +a;
                                     var c = a +
                                             ++a;
                                     var d = o as
                                             string;
                                     var e = o is
                                             int;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M(int a, int* p, object o)
                                    {
                                        var b = a
                                                + +a;
                                        var c = a
                                                + ++a;
                                        var d = o
                                                as string;
                                        var e = o
                                                is int;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a joined pair no spacing rule covers keeps the minimal separator, and the same pair written on one line keeps
    /// the author's gap, collapsed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-002")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedPairWithoutRuleKeepsTheMinimalSeparator()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     N(Create
                                       (1), (int)
                                       a);
                                     N(Create(1), (int)a);
                                     N(Create(1), (int)  a);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        N(Create(1), (int)a);
                                        N(Create(1), (int)a);
                                        N(Create(1), (int) a);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}