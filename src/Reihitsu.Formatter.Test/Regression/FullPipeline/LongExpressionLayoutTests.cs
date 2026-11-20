using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline tests of the layout of the shapes whose formatting cost <see cref="LongExpressionCostTests"/> measures, at a
/// small size and with comments and directives near the rewritten tokens: chains of nested conditionals, long sibling lists,
/// enum members, class members, and block statements. They pin today's output, so a change that only lowers the cost of
/// formatting these shapes must keep every line of it
/// </summary>
[TestClass]
public class LongExpressionLayoutTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a chain of conditionals nested in the false branch, wrapped irregularly and with a comment behind a branch,
    /// puts every <c>?</c> and <c>:</c> at the start of its own line, one level deeper for each nested conditional, and keeps the
    /// comment behind its branch
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void FalseBranchChainWithCommentIsLaidOutOneBranchPerLine()
    {
        const string input = """
                             namespace N;

                             internal class C
                             {
                                 internal int M(int k)
                                 {
                                     return k == 0 ? 0
                                  : k == 1 ? 1 // one
                                         : k == 2 ? 2
                                            : -1;
                                 }
                             }
                             """;

        const string expected = """
                                namespace N;

                                internal class C
                                {
                                    internal int M(int k)
                                    {
                                        return k == 0
                                                   ? 0
                                                   : k == 1
                                                       ? 1 // one
                                                       : k == 2
                                                           ? 2
                                                           : -1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain of conditionals nested in the true branch, written on one line, is spread out with every
    /// <c>?</c> and <c>:</c> at the start of its own line, one level deeper for each nested conditional
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void TrueBranchChainOnOneLineIsLaidOutOneBranchPerLine()
    {
        const string input = """
                             namespace N;

                             internal class C
                             {
                                 internal int M(int k)
                                 {
                                     return k > 0 ? k > 1 ? k > 2 ? 0 : 2 : 1 : 0;
                                 }
                             }
                             """;

        const string expected = """
                                namespace N;

                                internal class C
                                {
                                    internal int M(int k)
                                    {
                                        return k > 0
                                                   ? k > 1
                                                       ? k > 2
                                                           ? 0
                                                           : 2
                                                       : 1
                                                   : 0;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain of conditionals in a field initializer, written on one line, is spread out under the column of the
    /// condition
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void FieldInitializerChainOnOneLineIsLaidOutOneBranchPerLine()
    {
        const string input = """
                             namespace N;

                             internal class C
                             {
                                 private static readonly int _value = Environment.TickCount == 0 ? 0 : Environment.TickCount == 1 ? 1 : Environment.TickCount == 2 ? 2 : -1;
                             }
                             """;

        const string expected = """
                                namespace N;

                                internal class C
                                {
                                    private static readonly int _value = Environment.TickCount == 0
                                                                             ? 0
                                                                             : Environment.TickCount == 1
                                                                                 ? 1
                                                                                 : Environment.TickCount == 2
                                                                                     ? 2
                                                                                     : -1;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain of conditionals each nested in parentheses, written on one line, stays on one line
    /// </summary>
    [TestMethod]
    public void ParenthesizedChainOnOneLineStaysOnOneLine()
    {
        const string input = """
                             namespace N;

                             internal class C
                             {
                                 internal int M(int k)
                                 {
                                     return k == 0 ? 0 : (k == 1 ? 1 : (k == 2 ? 2 : -1));
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a chain of twelve conditionals nested in the false branch, written one branch per line, is laid out with
    /// each nested conditional one level deeper than the one before, down to the last
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void DeepFalseBranchChainIsIndentedOneLevelPerConditional()
    {
        const int depth = 12;

        var branches = string.Concat(Enumerable.Range(1, depth - 1).Select(index => $"\n               : k == {index} ? {index}"));
        var input = CreateMethodReturning($"k == 0 ? 0{branches}\n               : -1");
        var expected = new StringBuilder("k == 0");

        for (var index = 0; index < depth; index++)
        {
            var indentation = new string(' ', 19 + (4 * index));

            expected.Append($"\n{indentation}? {index}");
            expected.Append(index + 1 < depth ? $"\n{indentation}: k == {index + 1}" : $"\n{indentation}: -1");
        }

        AssertRuleResult(input, CreateMethodReturning(expected.ToString()).TrimEnd('\n'));
    }

    /// <summary>
    /// Verifies that an array initializer with irregular spacing, a comment behind an element, and an element in an inactive
    /// <c>#if</c> branch gets one space behind each comma, keeps the comment and the disabled text, and aligns its continuation
    /// lines
    /// </summary>
    [TestMethod]
    public void ArrayInitializerWithCommentAndDirectiveKeepsBoth()
    {
        const string input = """
                             namespace N;

                             internal class C
                             {
                                 internal int[] M()
                                 {
                                     var a = new[] {   0,1, 2, // two
                                             3,
                             #if DEBUG
                                         4,
                             #endif
                               5 };
                                     return a;
                                 }
                             }
                             """;

        const string expected = """
                                namespace N;

                                internal class C
                                {
                                    internal int[] M()
                                    {
                                        var a = new[] { 0, 1, 2, // two
                                                    3,
                                #if DEBUG
                                            4,
                                #endif
                                                    5 };

                                        return a;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped collection expression with a comment behind a comma puts one element on each line and keeps the
    /// comment, and that a wrapped argument list puts one argument on each line
    /// </summary>
    [TestMethod]
    public void WrappedCollectionExpressionAndArgumentListPutOneElementPerLine()
    {
        const string input = """
                             namespace N;

                             internal class C
                             {
                                 internal void M(params int[] p)
                                 {
                                     int[] a = [0,1,
                                  2, /* two */ 3];
                                     M(0,1,
                                           2,  3);
                                 }
                             }
                             """;

        const string expected = """
                                namespace N;

                                internal class C
                                {
                                    internal void M(params int[] p)
                                    {
                                        int[] a = [
                                                      0,
                                                      1,
                                                      2, /* two */
                                                      3
                                                  ];

                                        M(0,
                                          1,
                                          2,
                                          3);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that enum members with irregular indentation, a comment line, and several blank lines are indented, get a blank
    /// line in front of the comment, and lose the extra blank lines
    /// </summary>
    [TestMethod]
    public void EnumMembersWithCommentAndBlankLinesAreNormalized()
    {
        const string input = """
                             namespace N;

                             internal enum E
                             {
                                   V0,
                               // one
                                 V1,


                                 V2, V3
                             }
                             """;

        const string expected = """
                                namespace N;

                                internal enum E
                                {
                                    V0,

                                    // one
                                    V1,

                                    V2, V3
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that fields and statements with irregular indentation, extra blank lines, and a block on one line are indented,
    /// lose the extra blank lines, and that the block is spread out with a blank line on each side
    /// </summary>
    [TestMethod]
    public void ClassMembersAndBlockStatementsAreNormalized()
    {
        const string input = """
                             namespace N;

                             internal class C
                             {
                                   private int _f0;
                                 private int _f1;


                                 // two
                                 private int _f2;
                                 internal void M()
                                 {
                                     M();
                                       M();
                                     if (true) { M(); }
                                     M();
                                 }
                             }
                             """;

        const string expected = """
                                namespace N;

                                internal class C
                                {
                                    private int _f0;
                                    private int _f1;

                                    // two
                                    private int _f2;
                                    internal void M()
                                    {
                                        M();
                                        M();

                                        if (true)
                                        {
                                            M();
                                        }

                                        M();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Creates a document whose class holds one method that returns the given expression
    /// </summary>
    /// <param name="expression">The expression, its continuation lines indented for the column of the <c>return</c> keyword</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateMethodReturning(string expression)
    {
        return $"namespace N;\n\ninternal class C\n{{\n    internal int M(int k)\n    {{\n        return {expression};\n    }}\n}}\n";
    }

    #endregion // Methods
}