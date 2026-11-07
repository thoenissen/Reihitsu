using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the bracketed parameter list of an indexer, which the formatter lays out as the parameter list
/// of a method (FMT-PIPE-014, FMT-BREAK-011 to FMT-BREAK-013)
/// </summary>
[TestClass]
public class IndexerParameterListLayoutTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the specification example: the opening bracket joins <c>this</c>, a leading comma moves to the end of the
    /// previous parameter's line, and a closing bracket on its own line joins the last parameter
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    [Requirement("FMT-PIPE-014")]
    public void IndexerParameterListHugsItsDeclaration()
    {
        const string input = """
                             class C
                             {
                                 int this
                                     [int a] => a;

                                 int this[int a
                                          , int b] => a;

                                 int this[string a, string b
                                     ] => 0;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int a] => a;

                                    int this[int a,
                                             int b] => a;

                                    int this[string a, string b] => 0;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported example: a comma at the start of the second parameter's line moves to the end of the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void LeadingCommaOfIndexerParameterMovesToPreviousLine()
    {
        const string input = """
                             class C
                             {
                                 int this[int a
                                          , int b] => a;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int a,
                                             int b] => a;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: an indexer parameter list wrapped between two parameters gets one parameter per
    /// line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void WrappedIndexerParameterListGetsOneParameterPerLine()
    {
        const string input = """
                             class C
                             {
                                 int this[int a, int b,
                                          int c] => a;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int a,
                                             int b,
                                             int c] => a;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a break behind the opening bracket moves the first parameter onto the line of the bracket
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void FirstIndexerParameterJoinsOpeningBracket()
    {
        const string input = """
                             class C
                             {
                                 int this[
                                     int a, int b] => a;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int a,
                                             int b] => a;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an indexer parameter list written on one line stays on it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void OneLineIndexerParameterListStays()
    {
        const string input = """
                             class C
                             {
                                 int this[int a, int b] => a;
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the opening bracket of an explicit interface indexer joins <c>this</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void ExplicitInterfaceIndexerOpeningBracketJoinsThis()
    {
        const string input = """
                             interface I
                             {
                                 int this[int a] { get; }
                             }

                             class C : I
                             {
                                 int I.this
                                     [int a] => a;
                             }
                             """;
        const string expected = """
                                interface I
                                {
                                    int this[int a] { get; }
                                }

                                class C : I
                                {
                                    int I.this[int a] => a;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the parameter list of an indexer with an accessor block is laid out like any other
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void IndexerWithAccessorBlockLeadingCommaMovesToPreviousLine()
    {
        const string input = """
                             class C
                             {
                                 private int _v;

                                 int this[int a
                                          , int b]
                                 {
                                     get
                                     {
                                         return a;
                                     }
                                     set
                                     {
                                         _v = value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _v;

                                    int this[int a,
                                             int b]
                                    {
                                        get => a;
                                        set => _v = value;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter spanning several lines splits an indexer parameter list as it splits a method's
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-013")]
    public void MultiLineParameterSplitsIndexerParameterList()
    {
        const string input = """
                             class A : System.Attribute
                             {
                                 public A(string a, string b)
                                 {
                                 }
                             }

                             class C
                             {
                                 void N(int a, [A("a",
                                                  "b")] int b)
                                 {
                                 }

                                 int this[int a, [A("a",
                                                    "b")] int b] => a;
                             }
                             """;
        const string expected = """
                                class A : System.Attribute
                                {
                                    public A(string a, string b)
                                    {
                                    }
                                }

                                class C
                                {
                                    void N(int a,
                                           [A("a",
                                              "b")] int b)
                                    {
                                    }

                                    int this[int a,
                                             [A("a",
                                                "b")] int b] => a;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrap inside an indexer parameter, such as one between the type and the name, is joined as in a
    /// method's parameter list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-015")]
    public void WrapInsideIndexerParameterIsJoined()
    {
        const string input = """
                             class C
                             {
                                 int this[int
                                          a, int b] => a;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int a, int b] => a;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the default value of an indexer parameter wrapped at its <c>=</c> is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-015")]
    public void WrappedIndexerParameterDefaultIsJoined()
    {
        const string input = """
                             class C
                             {
                                 int this[int a, int b
                                              = 1] => a;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int a, int b = 1] => a;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between two indexer parameters keeps the list one parameter per line without losing the
    /// comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void CommentBehindIndexerParameterKeepsCommaOnItsLine()
    {
        const string input = """
                             class C
                             {
                                 int this[int a, // c
                                          int b] => a;
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Tests
}