using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the gaps the line-break phase joins behind a keyword, <c>is</c>, <c>not</c>, a modifier, a cast, a
/// switch arm, a base list, a catch filter, a loop header, a lambda, or <c>with</c>: the line breaks and the blank lines of each
/// gap are removed, so the token behind the gap stands on the line of the token in front of it, while the gaps next to the same
/// tokens but outside the list keep their line breaks
/// </summary>
[TestClass]
public class ContinuationJoinTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The input of the specification example in which every gap of the list holds a line break
    /// </summary>
    private const string SpecExampleInput = """
                                            using System;
                                            using System.Collections.Generic;
                                            using System.Threading.Tasks;

                                            class B;

                                            public
                                            class D
                                            : B
                                            {
                                                private static
                                                readonly int _f = 1;

                                                public
                                                event Action E;

                                                public
                                                D()
                                                {
                                                }

                                                async Task<int> M(int x, object o, Task<int> t, int[] a, IAsyncEnumerable<int> s)
                                                {
                                                    var p = o is
                                                    not
                                                    null;
                                                    var r = o switch
                                                            {
                                                                int i
                                                    when i > 0
                                                    => 2,
                                                                _ => 0,
                                                            };

                                                    try
                                                    {
                                                    }
                                                    catch (Exception e)
                                                    when (e != null)
                                                    {
                                                    }

                                                    for (var i = 0;
                                                    i < 3;
                                                    i++)
                                                    {
                                                    }

                                                    foreach (var y in
                                                    a)
                                                    {
                                                    }

                                                    await
                                                    foreach (var z in s)
                                                    {
                                                    }

                                                    Func<int> u = () =>
                                                    a.Length;
                                                    var n = new
                                                    List<int>();
                                                    int[] m = new
                                                    [] { 1 };
                                                    var c = (long)
                                                    x;
                                                    var q = await
                                                    t;

                                                    if (x > 0)
                                                    {
                                                        throw
                                                        new Exception();
                                                    }

                                                    return
                                                    x;
                                                }

                                                IEnumerable<int> Y(int x)
                                                {
                                                    yield return
                                                    x;
                                                }
                                            }

                                            record R
                                            {
                                                void M(R r)
                                                {
                                                    var w = r
                                                    with
                                                    {
                                                    };
                                                }
                                            }
                                            """;

    /// <summary>
    /// The output of the specification example, in which every gap of the list is joined
    /// </summary>
    private const string SpecExampleExpected = """
                                               using System;
                                               using System.Collections.Generic;
                                               using System.Threading.Tasks;

                                               class B;

                                               public class D : B
                                               {
                                                   private static readonly int _f = 1;

                                                   public event Action E;

                                                   public D()
                                                   {
                                                   }

                                                   async Task<int> M(int x, object o, Task<int> t, int[] a, IAsyncEnumerable<int> s)
                                                   {
                                                       var p = o is not null;
                                                       var r = o switch
                                                               {
                                                                   int i when i > 0 => 2,
                                                                   _ => 0,
                                                               };

                                                       try
                                                       {
                                                       }
                                                       catch (Exception e) when (e != null)
                                                       {
                                                       }

                                                       for (var i = 0; i < 3; i++)
                                                       {
                                                       }

                                                       foreach (var y in a)
                                                       {
                                                       }

                                                       await foreach (var z in s)
                                                       {
                                                       }

                                                       Func<int> u = () => a.Length;
                                                       var n = new List<int>();
                                                       int[] m = new[] { 1 };
                                                       var c = (long)x;
                                                       var q = await t;

                                                       if (x > 0)
                                                       {
                                                           throw new Exception();
                                                       }

                                                       return x;
                                                   }

                                                   IEnumerable<int> Y(int x)
                                                   {
                                                       yield return x;
                                                   }
                                               }

                                               record R
                                               {
                                                   void M(R r)
                                                   {
                                                       var w = r with
                                                                 {
                                                                 };
                                                   }
                                               }
                                               """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: every gap of the list is joined in one pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void SpecExampleEveryGapIsJoined()
    {
        // Act & Assert
        AssertRuleResult(SpecExampleInput, SpecExampleExpected);
    }

    /// <summary>
    /// Verifies the specification example: a blank line between <c>return</c> and its value goes with the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void SpecExampleBlankLineInTheGapGoesWithTheJoin()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     return

                                         x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported scenario: a <c>return</c> value written on the next line joins the <c>return</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void ReturnValueOnTheNextLineJoinsTheReturn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return
                                         2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the opening parenthesis of a parenthesized pattern that spans several lines keeps its
    /// own line below the <c>is</c>, and its content is indented four columns right of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    [Requirement("FMT-INDENT-026")]
    public void SpecExampleMultiLineParenthesizedPatternKeepsItsOpeningParenthesisBelowIs()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (
                                         int or long
                                     );
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o)
                                    {
                                        return o is
                                                 (
                                                     int or long
                                                 );
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the pattern exception: the opening brace of a multi-line property pattern without a type keeps
    /// its own line below the <c>is</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void MultiLinePropertyPatternKeepsItsOpeningBraceBelowIs()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(string s)
                                 {
                                     return s is
                                              {
                                                  Length: 2
                                              };
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: the <c>=</c> of an anonymous-object member and of a <c>let</c> clause joins the name
    /// in front of it and the value behind it, and the query's clauses align under <c>from</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void SpecExampleEqualsOfAnAnonymousObjectMemberAndOfALetClauseJoinsBothSides()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 object M(int[] arr)
                                 {
                                     var w = new
                                             {
                                                 A
                                                     = 1,
                                                 B =
                                                     2
                                             };
                                     var q = from x in arr
                                             let y
                                                 = x
                                             let z =
                                                 y
                                             select z;

                                     return q;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    object M(int[] arr)
                                    {
                                        var w = new
                                                {
                                                    A = 1,
                                                    B = 2
                                                };
                                        var q = from x in arr
                                                let y = x
                                                let z = y
                                                select z;

                                        return q;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the gaps of the list one at a time: each statement written with a line break in its gap is joined, and then takes
    /// the layout the same statement written on one line gets, such as the initializer behind <c>with</c> on its own line and the
    /// block of an anonymous method on lines of its own
    /// </summary>
    /// <param name="statement">The statement with the line break in the gap</param>
    /// <param name="joined">The joined statement</param>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    [DataRow("        return\n            x;", "        return x;", DisplayName = "Return with an expression")]
    [DataRow("        var v = o ?? throw\n            new Exception();", "        var v = o ?? throw new Exception();", DisplayName = "Throw expression")]
    [DataRow("        var q = await\n            t;", "        var q = await t;", DisplayName = "Await expression")]
    [DataRow("        await\n            using var e = d;", "        await using var e = d;", DisplayName = "Await using declaration")]
    [DataRow("        var n = new\n            List<int>();", "        var n = new List<int>();", DisplayName = "Object creation")]
    [DataRow("        var m = new\n            int[1];", "        var m = new int[1];", DisplayName = "Array creation")]
    [DataRow("        var k = new\n            [] { 1 };", "        var k = new[] { 1 };", DisplayName = "Implicitly typed array creation")]
    [DataRow("        var p = o is\n            1;", "        var p = o is 1;", DisplayName = "Is pattern with a constant pattern")]
    [DataRow("        var p = o is\n            string;", "        var p = o is string;", DisplayName = "Type test")]
    [DataRow("        var p = o is\n            string v;", "        var p = o is string v;", DisplayName = "Is pattern with a declaration pattern")]
    [DataRow("        var p = o is not\n            null;", "        var p = o is not null;", DisplayName = "Negated pattern")]
    [DataRow("        var c = (long)\n            x;", "        var c = (long)x;", DisplayName = "Cast")]
    [DataRow("        Func<int> u = () =>\n            1;", "        Func<int> u = () => 1;", DisplayName = "Parenthesized lambda with an expression body")]
    [DataRow("        Func<int, int> f = y =>\n            y + 1;", "        Func<int, int> f = y => y + 1;", DisplayName = "Simple lambda with an expression body")]
    [DataRow("        Func<int, int> f = static\n            y => y;", "        Func<int, int> f = static y => y;", DisplayName = "Modifier of a lambda")]
    [DataRow("        Func<int> g = static\n            delegate { return 1; };", "        Func<int> g = static delegate\n        {\n            return 1;\n        };", DisplayName = "Modifier of an anonymous method")]
    [DataRow("        const\n            int k = 1;", "        const int k = 1;", DisplayName = "Modifier of a local declaration")]
    [DataRow("        var w = r\n            with { };", "        var w = r with\n                  {\n                  };", DisplayName = "With expression")]
    [DataRow("        Use(await\n            t, 1);", "        Use(await t, 1);", DisplayName = "Await inside a list element")]
    [DataRow("        Use(o is\n            not null);", "        Use(o is not null);", DisplayName = "Is pattern inside a list element")]
    public void EachGapIsJoined(string statement, string joined)
    {
        AssertRuleResult(WrapInMethod(statement), WrapInMethod(joined));
    }

    /// <summary>
    /// Verifies the gaps of the list that need a statement of several lines: each is joined, and the statement keeps its block
    /// </summary>
    /// <param name="statement">The statement with the line break in the gap</param>
    /// <param name="joined">The joined statement</param>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    [DataRow("        await\n            using (d)\n        {\n        }", "        await using (d)\n        {\n        }", DisplayName = "Await using statement")]
    [DataRow("        foreach (var y in\n            a)\n        {\n        }", "        foreach (var y in a)\n        {\n        }", DisplayName = "Foreach expression")]
    [DataRow("        for (var i = 0;\n            i < 3;\n            i++)\n        {\n        }", "        for (var i = 0; i < 3; i++)\n        {\n        }", DisplayName = "For header")]
    [DataRow("        static\n            int L()\n        {\n            return 1;\n        }", "        static int L()\n        {\n            return 1;\n        }", DisplayName = "Modifier of a local function")]
    public void EachGapOfABlockStatementIsJoined(string statement, string joined)
    {
        AssertRuleResult(WrapInMethod(statement), WrapInMethod(joined));
    }

    /// <summary>
    /// Verifies the arm gaps one at a time: the <c>=&gt;</c> of an arm without a <c>when</c> clause, and the <c>when</c> of an arm,
    /// each joined onto the arm's line
    /// </summary>
    /// <param name="arm">The arm with the line break in the gap</param>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    [DataRow("                    1\n                        => 2,", DisplayName = "Arrow behind the pattern")]
    [DataRow("                    int i\n                        when i > 0 => 2,", DisplayName = "When behind the pattern")]
    [DataRow("                    int i when i > 0\n                        => 2,", DisplayName = "Arrow behind the when clause")]
    public void EachArmGapIsJoined(string arm)
    {
        var joined = arm.StartsWith("                    1") ? "                    1 => 2," : "                    int i when i > 0 => 2,";

        AssertRuleResult(WrapInMethod($"        var w = x switch\n                {{\n{arm}\n                    _ => 0,\n                }};"), WrapInMethod($"        var w = x switch\n                {{\n{joined}\n                    _ => 0,\n                }};"));
    }

    /// <summary>
    /// Verifies the modifier gaps of declarations the specification example does not show: a method, a property's accessor, a
    /// conversion operator, a delegate, a nested type, and a parameter, each joined; the empty nested type then becomes a semicolon
    /// declaration, as it does when written on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void ModifierGapsOfOtherDeclarationsAreJoined()
    {
        // Arrange
        const string input = """
                             public
                             delegate void D();

                             static class C
                             {
                                 public static
                                 implicit operator int(N n)
                                 {
                                     return 1;
                                 }

                                 public
                                 static void M()
                                 {
                                 }

                                 static void E(this
                                               int x)
                                 {
                                 }

                                 public
                                 class N
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public delegate void D();

                                static class C
                                {
                                    public static implicit operator int(N n)
                                    {
                                        return 1;
                                    }

                                    public static void M()
                                    {
                                    }

                                    static void E(this int x)
                                    {
                                    }

                                    public class N;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the join removes only the line breaks of the gap, and the requirements that lay the
    /// construct out on one line then decide its shape: the initializer behind <c>with</c> starts its own line, the block of the
    /// anonymous method spans several lines, and the empty type becomes a semicolon declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void SpecExampleJoinedConstructsAreLaidOutAsWhenWrittenOnOneLine()
    {
        // Arrange
        const string input = """
                             using System;

                             record R;

                             static class C
                             {
                                 public
                                 class N
                                 {
                                 }

                                 static void M(R r)
                                 {
                                     Func<int> g = static
                                         delegate { return 1; };
                                     var w = r
                                         with { };
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                record R;

                                static class C
                                {
                                    public class N;

                                    static void M(R r)
                                    {
                                        Func<int> g = static delegate
                                        {
                                            return 1;
                                        };
                                        var w = r with
                                                  {
                                                  };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the modifier gap of an accessor: the line break between <c>private</c> and <c>set</c> is removed, whatever layout
    /// the accessor list takes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void ModifierGapOfAnAccessorIsJoined()
    {
        const string input = """
                             class C
                             {
                                 public int P
                                 {
                                     get;
                                     private
                                     set;
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var output = FormatWithPipeline(input, endOfLine);

            Assert.Contains("private set;", output, $"The accessor's modifier must be joined under {DescribeLineEnding(endOfLine)} line endings:{endOfLine}{output}");
        }

        AssertSecondPassIsStable(input);
    }

    /// <summary>
    /// Verifies the other side: a gap next to the same tokens but outside the list keeps its line break, under LF and CRLF —
    /// behind a switch arm's <c>=&gt;</c>, behind a base list's <c>:</c>, behind the <c>(</c> of a <c>for</c> header, in front of
    /// the <c>in</c> of a <c>foreach</c>, behind the <c>=&gt;</c> of a lambda with a block body, behind <c>with</c>, between
    /// <c>return</c> or <c>throw</c> and the <c>;</c> of a statement without an expression, and behind the <c>new</c> of an
    /// anonymous object
    /// </summary>
    /// <param name="statement">The statement with the line break</param>
    /// <param name="front">The text in front of the line break that must stay at the end of its line</param>
    /// <param name="behind">The text behind the line break that must start the next line</param>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    [DataRow("        var w = x switch\n                {\n                    1 =>\n                        2,\n                    _ => 0,\n                };", "1 =>", "2,", DisplayName = "Behind the arrow of a switch arm")]
    [DataRow("        for (\n            var i = 0; i < 3; i++)\n        {\n        }", "for (", "var i = 0;", DisplayName = "Behind the opening parenthesis of a for header")]
    [DataRow("        foreach (var y\n            in a)\n        {\n        }", "foreach (var y", "in a)", DisplayName = "In front of the in of a foreach statement")]
    [DataRow("        Action b = () =>\n        {\n        };", "() =>", "{", DisplayName = "Behind the arrow of a lambda with a block body")]
    [DataRow("        var v = r with\n                {\n                };", "r with", "{", DisplayName = "Behind with")]
    [DataRow("        var n = new\n                {\n                    A = 1\n                };", "new", "{", DisplayName = "Behind the new of an anonymous object")]
    public void GapsOutsideTheListKeepTheirLineBreak(string statement, string front, string behind)
    {
        AssertLineBreakIsKept(WrapInMethod(statement), front, behind);
    }

    /// <summary>
    /// Verifies the other side of the base list gap: a line break behind the <c>:</c> of a base list is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void LineBreakBehindTheColonOfABaseListIsKept()
    {
        AssertLineBreakIsKept("class B;\n\nclass D :\n    B\n{\n}", "class D :", "B");
    }

    /// <summary>
    /// Verifies the other side of the keyword gap: a <c>return</c> or a <c>throw</c> without an expression keeps the line break
    /// in front of its <c>;</c>
    /// </summary>
    /// <param name="statement">The statement with the line break</param>
    /// <param name="keyword">The keyword</param>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    [DataRow("        return\n            ;", "return", DisplayName = "Return without an expression")]
    [DataRow("        try\n        {\n        }\n        catch\n        {\n            throw\n                ;\n        }", "throw", DisplayName = "Rethrow")]
    public void StatementWithoutAnExpressionKeepsTheLineBreakInFrontOfItsSemicolon(string statement, string keyword)
    {
        AssertLineBreakIsKept(WrapInVoidMethod(statement), keyword, ";");
    }

    /// <summary>
    /// Verifies that the join also applies in a node-level root: <see cref="ReihitsuFormatter.FormatNode"/> of a return statement
    /// joins its value onto the <c>return</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void JoinAppliesInANodeLevelRoot()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(WrapInMethod("        return\n            x;"), endOfLine), cancellationToken: TestContext.CancellationToken);
            var statement = tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<ReturnStatementSyntax>().Single();

            var result = ReihitsuFormatter.FormatNode(statement, cancellationToken: TestContext.CancellationToken);

            Assert.IsNull(result.Unsupported);
            Assert.AreEqual("return x;", result.Output.ToString().Trim(), $"The node-level root must be joined under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Wraps statements in an async method of a class, with the parameters the statements use
    /// </summary>
    /// <param name="statements">The statements, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statements)
    {
        return $"using System;\nusing System.Collections.Generic;\nusing System.Threading.Tasks;\n\nclass C\n{{\n    async Task<int> M(int x, object o, Task<int> t, int[] a, IAsyncDisposable d, R r)\n    {{\n{statements}\n    }}\n}}\n\nrecord R;";
    }

    /// <summary>
    /// Wraps statements in a void method of a class
    /// </summary>
    /// <param name="statements">The statements, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInVoidMethod(string statements)
    {
        return $"class C\n{{\n    void M()\n    {{\n{statements}\n    }}\n}}";
    }

    /// <summary>
    /// Formats the input under LF and CRLF line endings and verifies that the output is a fixed point of the formatter and still
    /// holds a line break between the given texts: one line of the output ends with <paramref name="front"/>, and the next one
    /// starts with <paramref name="behind"/>
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="front">The text at the end of the line in front of the line break</param>
    /// <param name="behind">The text at the start of the line behind the line break</param>
    private static void AssertLineBreakIsKept(string input, string front, string behind)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = FormatWithPipeline(input, endOfLine);
            var lines = output.Split(endOfLine);
            var index = System.Array.FindIndex(lines, line => line.TrimEnd().EndsWith(front, System.StringComparison.Ordinal));

            Assert.IsGreaterThanOrEqualTo(0, index, $"A line must end with '{front}' under {DescribeLineEnding(endOfLine)} line endings:{endOfLine}{output}");
            Assert.IsLessThan(lines.Length - 1, index);
            Assert.IsTrue(lines[index + 1].TrimStart().StartsWith(behind, System.StringComparison.Ordinal), $"The line behind '{front}' must start with '{behind}' under {DescribeLineEnding(endOfLine)} line endings:{endOfLine}{output}");
        }

        AssertSecondPassIsStable(input);
    }

    #endregion // Methods
}