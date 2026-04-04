using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.BlankLines;

namespace Reihitsu.Formatter.Test.Unit.Rules.BlankLines;

/// <summary>
/// Tests for <see cref="BlankLineBeforeStatementRule"/>
/// </summary>
[TestClass]
public class BlankLineBeforeStatementRuleTests
{
    #region Methods

    /// <summary>
    /// Verifies that a blank line is inserted before an <c>if</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void IfStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    if (x > 0) { }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    if (x > 0) { }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted when an <c>if</c> statement
    /// is the first statement in a block.
    /// </summary>
    [TestMethod]
    public void IfStatementFirstInBlockNoBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    if (true) { }
                }
            }
            """;

        AssertRule(input, input);
    }

    /// <summary>
    /// Verifies that no additional blank line is inserted before an
    /// <c>else if</c> clause.
    /// </summary>
    [TestMethod]
    public void ElseIfNoBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    if (true) { }
                    else if (false) { }
                }
            }
            """;

        AssertRule(input, input);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>try</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void TryStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    try { } catch { }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    try { } catch { }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>while</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void WhileStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    while (x > 0) { x--; }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    while (x > 0) { x--; }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>do</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void DoStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    do { x--; } while (x > 0);
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    do { x--; } while (x > 0);
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>for</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ForStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    for (var i = 0; i < x; i++) { }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    for (var i = 0; i < x; i++) { }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>foreach</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ForEachStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var items = new int[0];
                    foreach (var item in items) { }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var items = new int[0];

                    foreach (var item in items) { }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>return</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ReturnStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                int M()
                {
                    var x = 1;
                    return x;
                }
            }
            """;

        const string expected = """
            class C
            {
                int M()
                {
                    var x = 1;

                    return x;
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted when a <c>return</c> statement
    /// is the first statement in a block.
    /// </summary>
    [TestMethod]
    public void ReturnStatementFirstInBlockNoBlankLine()
    {
        const string input = """
            class C
            {
                int M()
                {
                    return 1;
                }
            }
            """;

        AssertRule(input, input);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>throw</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ThrowStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    throw new System.Exception();
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    throw new System.Exception();
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>break</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void BreakStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    while (true)
                    {
                        var x = 1;
                        break;
                    }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    while (true)
                    {
                        var x = 1;

                        break;
                    }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>continue</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ContinueStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    while (true)
                    {
                        var x = 1;
                        continue;
                    }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    while (true)
                    {
                        var x = 1;

                        continue;
                    }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>goto</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void GotoStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    goto end;
                    end:
                    return;
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    goto end;
                    end:
                    return;
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>switch</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void SwitchStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    switch (x) { default: break; }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    switch (x) { default: break; }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>checked</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void CheckedStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    checked { x++; }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    checked { x++; }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>fixed</c> statement
    /// that follows another statement inside an <c>unsafe</c> context.
    /// </summary>
    [TestMethod]
    public void FixedStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                unsafe void M()
                {
                    var arr = new int[1];
                    fixed (int* p = arr) { }
                }
            }
            """;

        const string expected = """
            class C
            {
                unsafe void M()
                {
                    var arr = new int[1];

                    fixed (int* p = arr) { }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>lock</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void LockStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var obj = new object();
                    lock (obj) { }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var obj = new object();

                    lock (obj) { }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>using</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void UsingStatementInsertsBlankLine()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                    using (var s = new System.IO.MemoryStream()) { }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;

                    using (var s = new System.IO.MemoryStream()) { }
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>yield return</c>
    /// statement that is the first yield in a method.
    /// </summary>
    [TestMethod]
    public void YieldReturnInsertsBlankLine()
    {
        const string input = """
            class C
            {
                System.Collections.Generic.IEnumerable<int> M()
                {
                    var x = 1;
                    yield return x;
                }
            }
            """;

        const string expected = """
            class C
            {
                System.Collections.Generic.IEnumerable<int> M()
                {
                    var x = 1;

                    yield return x;
                }
            }
            """;

        AssertRule(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted between consecutive
    /// <c>yield return</c> statements.
    /// </summary>
    [TestMethod]
    public void YieldReturnAfterYieldReturnNoBlankLine()
    {
        const string input = """
            class C
            {
                System.Collections.Generic.IEnumerable<int> M()
                {
                    yield return 1;
                    yield return 2;
                    yield return 3;
                }
            }
            """;

        AssertRule(input, input);
    }

    /// <summary>
    /// Verifies that no duplicate blank line is inserted when a blank line
    /// already exists before the statement.
    /// </summary>
    [TestMethod]
    public void AlreadyHasBlankLineNoDoubleInsert()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;

                    if (x > 0) { }
                }
            }
            """;

        AssertRule(input, input);
    }

    /// <summary>
    /// Verifies that <see cref="BlankLineBeforeStatementRule.Phase"/> returns
    /// <see cref="FormattingPhase.BlankLineManagement"/>.
    /// </summary>
    [TestMethod]
    public void PhaseReturnsBlankLineManagement()
    {
        var context = new FormattingContext("\n");
        var rule = new BlankLineBeforeStatementRule(context, CancellationToken.None);

        Assert.AreEqual(FormattingPhase.BlankLineManagement, rule.Phase);
    }

    /// <summary>
    /// Applies the <see cref="BlankLineBeforeStatementRule"/> to the given input
    /// and asserts the result matches the expected output.
    /// </summary>
    /// <param name="input">The input source code.</param>
    /// <param name="expected">The expected output source code.</param>
    private static void AssertRule(string input, string expected)
    {
        input = input.Replace("\r\n", "\n");
        expected = expected.Replace("\r\n", "\n");

        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext("\n");
        var rule = new BlankLineBeforeStatementRule(context, CancellationToken.None);
        var result = rule.Apply(tree.GetRoot());
        var actual = result.ToFullString();

        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}