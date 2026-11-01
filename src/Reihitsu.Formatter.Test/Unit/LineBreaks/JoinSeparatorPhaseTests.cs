using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests the separator <see cref="LineBreakPhase"/> writes where it joins two tokens onto one line, by running the phase alone:
/// no whitespace unless the two tokens would not read back as written, and no whitespace written, removed, or changed in a gap
/// that is on one line before and after the phase
/// </summary>
[TestClass]
public class JoinSeparatorPhaseTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the example of the requirement: the phase's text holds the minimal separator at every join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void RequirementExampleHoldsTheMinimalSeparatorAtEveryJoin()
    {
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

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var result = ExecuteLineBreakPhase(input, endOfLine);

            Assert.Contains("public int P=> 1;", result);
            Assert.Contains("public int Q{get;protected internal set;}", result);
            Assert.Contains("N(name:a);", result);

            var lines = result.Split(endOfLine).Select(line => line.TrimStart()).ToList();

            foreach (var lineStart in new[] { "- -a;", "/ *p;", "?1", ":2;" })
            {
                Assert.Contains(line => line.StartsWith(lineStart, StringComparison.Ordinal), lines, $"No line of the phase's text starts with '{lineStart}':{Environment.NewLine}{result}");
            }

            AssertSameTokens(input, result);
        }
    }

    /// <summary>
    /// Verifies that every pair of accessor modifiers the phase joins in a collapsed accessor list is separated by one space
    /// </summary>
    /// <param name="first">The modifier on the first line</param>
    /// <param name="second">The modifier on the second line</param>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    [DataRow("protected", "internal")]
    [DataRow("internal", "protected")]
    [DataRow("private", "protected")]
    [DataRow("protected", "private")]
    public void AccessorModifiersJoinedInACollapsedListAreSeparatedByOneSpace(string first, string second)
    {
        var input = $$"""
                      class C
                      {
                          public int P
                          {
                              get;
                              {{first}}
                              {{second}} set;
                          }
                      }
                      """;

        var result = ExecuteLineBreakPhase(input, "\n");

        Assert.Contains($"public int P{{get;{first} {second} set;}}", result);
        AssertSameTokens(input, result);
    }

    /// <summary>
    /// Verifies that a binary operator the phase moves to the start of the next line is separated by one space from an operand
    /// it would otherwise fuse with or open a comment with
    /// </summary>
    /// <param name="firstLine">The line that ends in the operator</param>
    /// <param name="secondLine">The line that holds the right operand</param>
    /// <param name="joined">The operator and operand as the phase's text must hold them</param>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    [DataRow("var x = a -", "-a;", "- -a;")]
    [DataRow("var x = a +", "+a;", "+ +a;")]
    [DataRow("var x = a +", "++a;", "+ ++a;")]
    [DataRow("var x = a -", "--a;", "- --a;")]
    [DataRow("var x = a &", "&a == null;", "& &a == null;")]
    [DataRow("var x = a /", "*p;", "/ *p;")]
    [DataRow("var x = o as", "string;", "as string;")]
    [DataRow("var x = o is", "int;", "is int;")]
    public void BinaryOperatorMovedToTheNextLineIsSeparatedFromAFusingOperand(string firstLine, string secondLine, string joined)
    {
        var input = $$"""
                      class C
                      {
                          unsafe void M(int a, int* p, object o)
                          {
                              {{firstLine}}
                                      {{secondLine}}
                          }
                      }
                      """;

        var result = ExecuteLineBreakPhase(input, "\n");

        Assert.Contains(joined, result);
        AssertSameTokens(input, result);
    }

    /// <summary>
    /// Verifies that the joins of an assignment write no whitespace where the two tokens stay two tokens without it, and keep the
    /// whitespace of the gap that was already on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void AssignmentJoinsWriteNoWhitespace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x =
                                         1;
                                     var y
                                         = 2;
                                 }
                             }
                             """;

        var result = ExecuteLineBreakPhase(input, "\n");

        Assert.Contains("var x =1;", result);
        Assert.Contains("var y= 2;", result);
        AssertSameTokens(input, result);
    }

    /// <summary>
    /// Verifies that the join of a <c>case</c> guard writes no whitespace in front of <c>when</c> behind a parenthesis, and one
    /// space behind an identifier, which would otherwise fuse with it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void CaseGuardJoinWritesTheMinimalSeparator()
    {
        const string input = """
                             class C
                             {
                                 void M(object o, bool b)
                                 {
                                     switch (o)
                                     {
                                         case (1, 2)
                                             when b:
                                             break;

                                         case int i
                                             when b:
                                             break;
                                     }
                                 }
                             }
                             """;

        var result = ExecuteLineBreakPhase(input, "\n");

        Assert.Contains("case (1, 2)when b:", result);
        Assert.Contains("case int i when b:", result);
        AssertSameTokens(input, result);
    }

    /// <summary>
    /// Verifies that the joins of a list element's interior wraps write no whitespace where the two tokens stay two tokens without
    /// it, and one space where they would fuse
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void ListElementJoinsWriteTheMinimalSeparator()
    {
        const string input = """
                             class C
                             {
                                 void M(object o, int x)
                                 {
                                     N(name:
                                       "x");
                                     N(out
                                       var v);
                                     N(static
                                       (int i) => i);
                                     N(o is not
                                       "x");
                                     N(x is >
                                       1);
                                     N(z =>
                                       z);
                                 }

                                 void P(int?
                                        a, [param
                                        :System.Obsolete] int b)
                                 {
                                 }
                             }
                             """;

        var result = ExecuteLineBreakPhase(input, "\n");

        Assert.Contains("N(name:\"x\");", result);
        Assert.Contains("N(out var v);", result);
        Assert.Contains("N(static(int i) => i);", result);
        Assert.Contains("N(o is not\"x\");", result);
        Assert.Contains("N(x is >1);", result);
        Assert.Contains("N(z =>z);", result);
        Assert.Contains("void P(int?a, [param:System.Obsolete] int b)", result);
        AssertSameTokens(input, result);
    }

    /// <summary>
    /// Verifies that the joins of a property arrow write no whitespace on the joined side and leave the side that was already on
    /// one line as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    [Requirement("FMT-BREAK-026")]
    public void PropertyArrowJoinsWriteNoWhitespace()
    {
        const string input = """
                             class C
                             {
                                 int P
                                     => 1;

                                 int Q =>
                                     1;

                                 int this[int i]
                                     =>i;
                             }
                             """;

        var result = ExecuteLineBreakPhase(input, "\n");

        Assert.Contains("int P=> 1;", result);
        Assert.Contains("int Q =>1;", result);
        Assert.Contains("int this[int i]=>i;", result);
        AssertSameTokens(input, result);
    }

    /// <summary>
    /// Verifies the last sentence of the requirement example: a property arrow and an accessor list already on one line leave the
    /// phase with the whitespace the author wrote
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    [Requirement("FMT-BREAK-026")]
    [Requirement("FMT-BREAK-024")]
    public void GapsTheJoinDoesNotTouchKeepTheirWhitespace()
    {
        const string input = """
                             class C
                             {
                                 public int R =>1;

                                 public int S {get;set;}

                                 public int T  =>  1;

                                 public int U {[System.Obsolete] get;set;}
                             }
                             """;

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var normalized = input.Replace("\r\n", "\n").Replace("\n", endOfLine);

            Assert.AreEqual(normalized, ExecuteLineBreakPhase(normalized, endOfLine));
        }
    }

    /// <summary>
    /// Verifies that an accessor list the phase collapses onto the declaration's line is joined without whitespace at every join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    [Requirement("FMT-BREAK-024")]
    public void CollapsedAccessorListIsJoinedWithoutWhitespace()
    {
        const string input = """
                             class C
                             {
                                 public int P
                                 {
                                     get;
                                     private set;
                                 }
                             }
                             """;

        var result = ExecuteLineBreakPhase(input, "\n");

        Assert.Contains("public int P{get;private set;}", result);
        AssertSameTokens(input, result);
    }

    /// <summary>
    /// Verifies that the joins of a declaration's parameter list, a field's semicolon, and an invocation inside an argument write
    /// no whitespace, because the joined tokens stay two tokens without it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void JoinsOfTokensThatStayApartWriteNoWhitespace()
    {
        const string input = """
                             class C
                             {
                                 int _f
                                     ;

                                 void M
                                     (int a)
                                 {
                                     N(Create
                                       (1));
                                 }
                             }
                             """;

        var result = ExecuteLineBreakPhase(input, "\n");

        Assert.Contains("int _f;", result);
        Assert.Contains("void M(int a)", result);
        Assert.Contains("N(Create(1));", result);
        AssertSameTokens(input, result);
    }

    /// <summary>
    /// Verifies that a comment in the gap still refuses the join of a property arrow, a conditional branch, a <c>case</c> guard,
    /// an assignment value, a binary operand, and a named argument's value, after the separator change
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void CommentInTheGapStillRefusesEveryJoin()
    {
        const string input = """
                             class C
                             {
                                 int P // p
                                     => 1;

                                 int M(bool b, int a, object o)
                                 {
                                     switch (o)
                                     {
                                         case (1, 2) // w
                                             when b:
                                             break;
                                     }

                                     var x = // x
                                         1;
                                     var y = a - // y
                                             -a;
                                     N(name: // n
                                       a);

                                     return b ? // r
                                            1 : 2;
                                 }
                             }
                             """;

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var normalized = input.Replace("\r\n", "\n").Replace("\n", endOfLine);
            var result = ExecuteLineBreakPhase(normalized, endOfLine);

            foreach (var comment in new[] { "// p", "// w", "// x", "// y", "// n", "// r" })
            {
                Assert.Contains($"{comment}{endOfLine}", result, $"The comment '{comment}' must still end its line.");
            }

            AssertSameTokens(normalized, result);
        }
    }

    /// <summary>
    /// Verifies that a directive in the gap still refuses the join of a property arrow, accessor modifiers in an accessor list, a
    /// conditional branch, a <c>case</c> guard, and a binary operand, after the separator change
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void DirectiveInTheGapStillRefusesEveryJoin()
    {
        const string input = """
                             class C
                             {
                                 int P
                             #pragma warning disable CS0001
                                     => 1;

                                 public int Q
                                 {
                                     get;
                                     protected
                             #pragma warning disable CS0002
                                     internal set;
                                 }

                                 int M(bool b, int a, object o)
                                 {
                                     switch (o)
                                     {
                                         case (1, 2)
                             #pragma warning disable CS0003
                                             when b:
                                             break;
                                     }

                                     var y = a -
                             #pragma warning disable CS0004
                                             -a;

                                     return b ?
                             #pragma warning disable CS0005
                                            1 : 2;
                                 }
                             }
                             """;

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var normalized = input.Replace("\r\n", "\n").Replace("\n", endOfLine);
            var result = ExecuteLineBreakPhase(normalized, endOfLine);
            var lines = result.Split(endOfLine).Select(line => line.TrimStart()).ToList();

            foreach (var code in new[] { "CS0001", "CS0002", "CS0003", "CS0004", "CS0005" })
            {
                Assert.Contains($"#pragma warning disable {code}", lines, $"The directive disabling {code} must still stand on a line of its own:{Environment.NewLine}{result}");
            }

            AssertSameTokens(normalized, result);
        }
    }

    /// <summary>
    /// Executes the <see cref="LineBreakPhase"/> on the given C# source text
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="endOfLine">The end-of-line sequence for the formatting context</param>
    /// <returns>The text of the phase's tree</returns>
    private string ExecuteLineBreakPhase(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input.Replace("\r\n", "\n").Replace("\n", endOfLine), cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(endOfLine);
        var result = new LineBreakPhase().Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Asserts that the text of the phase's tree reads back as the same tokens as the input, so no join fused two tokens or opened
    /// a comment
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="result">The text of the phase's tree</param>
    private void AssertSameTokens(string input, string result)
    {
        var expectedTokens = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken)
                                             .GetRoot(TestContext.CancellationToken)
                                             .DescendantTokens()
                                             .Select(token => token.Text)
                                             .ToList();
        var actualTokens = CSharpSyntaxTree.ParseText(result, cancellationToken: TestContext.CancellationToken)
                                           .GetRoot(TestContext.CancellationToken)
                                           .DescendantTokens()
                                           .Select(token => token.Text)
                                           .ToList();

        Assert.AreSequenceEqual(expectedTokens, actualTokens, $"The phase's text must read back as the input's tokens:{Environment.NewLine}{result}");
    }

    #endregion // Methods
}