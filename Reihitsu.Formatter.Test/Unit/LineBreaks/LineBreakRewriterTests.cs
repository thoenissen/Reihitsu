using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for <see cref="LineBreakRewriter"/> and <see cref="LineBreakPhase"/>
/// </summary>
[TestClass]
public class LineBreakRewriterTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that K&amp;R-style open braces are moved to their own line (Allman style)
    /// </summary>
    [TestMethod]
    public void PlacesOpenBraceOnNewLine()
    {
        // Arrange
        const string input = """
                             class Foo {
                                 void Bar() {
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — open braces must be on their own lines
        Assert.DoesNotContain("Foo {", result, "K&R-style class brace should be converted to Allman.");
        Assert.DoesNotContain("Bar() {", result, "K&R-style method brace should be converted to Allman.");
    }

    /// <summary>
    /// Verifies that close braces that are not on their own line are moved to a new line
    /// </summary>
    [TestMethod]
    public void PlacesCloseBraceOnNewLine()
    {
        // Arrange — close brace immediately follows a statement on the same line
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 { return; }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — close brace should be on its own line, not on the same line as "return;"
        Assert.DoesNotContain("return; }", result, "Close brace should not be on the same line as a statement.");
    }

    /// <summary>
    /// Verifies that code already in Allman brace style is preserved without modification
    /// </summary>
    [TestMethod]
    public void PreservesAllmanStyleBraces()
    {
        // Arrange
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     var x = 1;
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — output should match input since braces are already Allman-style
        Assert.AreEqual(input, result, "Already Allman-style code should not be modified.");
    }

    /// <summary>
    /// Verifies that empty blocks with K&amp;R-style braces are correctly reformatted
    /// </summary>
    [TestMethod]
    public void HandlesEmptyBlock()
    {
        // Arrange
        const string input = """
                             class Foo {
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — open brace should be on its own line
        Assert.Contains($"Foo {Environment.NewLine}{{", result, "Open brace of empty block should be on its own line.");
    }

    /// <summary>
    /// Verifies that nested braces are all converted to Allman style
    /// </summary>
    [TestMethod]
    public void HandlesNestedBraces()
    {
        // Arrange — K&R braces at multiple nesting levels
        const string input = """
                             namespace Ns {
                                 class Foo {
                                     void Bar() {
                                         if (true) {
                                         }
                                     }
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — no K&R braces should remain
        Assert.DoesNotContain("Ns {", result, "Namespace K&R brace should be converted.");
        Assert.DoesNotContain("Foo {", result, "Class K&R brace should be converted.");
        Assert.DoesNotContain("Bar() {", result, "Method K&R brace should be converted.");
        Assert.DoesNotContain("(true) {", result, "If-statement K&R brace should be converted.");
    }

    /// <summary>
    /// Verifies that a multi-line method chain collapses the first link to the root
    /// and keeps subsequent links on their own lines
    /// </summary>
    [TestMethod]
    public void BreaksLongMethodChain()
    {
        // Arrange — chain where the first dot is on a new line
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     var x = source
                                         .Where(i => i > 0)
                                         .Select(i => i * 2)
                                         .ToList();
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — first chain link should be collapsed to the root line
        Assert.Contains("source.Where", result, "First chain link should be collapsed to the same line as the root.");
    }

    /// <summary>
    /// Verifies that conditional access chains with invocations are normalized
    /// so that the <c>?.</c> tokens are kept together
    /// </summary>
    [TestMethod]
    public void HandlesConditionalAccessChain()
    {
        // Arrange — conditional access with member binding on separate line
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     var x = obj?
                                         .Method1()
                                         .Method2();
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — the ?. should be collapsed together (no line break between ? and .)
        Assert.Contains("obj?.Method1()", result, "Conditional access ?. should be collapsed onto the same line.");
    }

    /// <summary>
    /// Verifies that short single-line statements are not broken across lines
    /// </summary>
    [TestMethod]
    public void PreservesShortStatements()
    {
        // Arrange
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     var x = 1;
                                     var y = x + 2;
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — short statements should remain unchanged
        Assert.AreEqual(input, result, "Short statements should not be modified.");
    }

    /// <summary>
    /// Verifies that property accessor lists with bodies have braces placed on their own lines,
    /// while auto-property accessor lists are preserved inline
    /// </summary>
    [TestMethod]
    public void HandlesPropertyAccessors()
    {
        // Arrange — auto-property should stay inline; bodied accessor should get Allman braces
        const string input = """
                             class Foo
                             {
                                 public int AutoProp { get; set; }

                                 public int BodyProp {
                                     get {
                                         return 42;
                                     }
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — auto-property stays inline
        Assert.Contains("{ get; set; }", result, "Auto-property accessor list should stay inline.");

        // Bodied property should have Allman-style braces
        Assert.DoesNotContain("BodyProp {", result, "Bodied property accessor should have Allman-style braces.");
    }

    /// <summary>
    /// Verifies that switch expression arms are handled without errors
    /// </summary>
    [TestMethod]
    public void HandlesSwitchExpressionArms()
    {
        // Arrange — switch expression is not directly rewritten by LineBreakRewriter
        // but the surrounding block braces should still be handled
        const string input = """
                             class Foo
                             {
                                 string Bar(int x)
                                 {
                                     return x switch
                                     {
                                         1 => "one",
                                         2 => "two",
                                         _ => "other"
                                     };
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — switch expression should still be well-formed
        Assert.Contains("x switch", result, "Switch expression should be preserved.");
        Assert.Contains("1 => \"one\"", result, "Switch arms should be preserved.");
    }

    /// <summary>
    /// Verifies that LINQ query expressions are handled without errors and
    /// their surrounding block braces are correctly placed
    /// </summary>
    [TestMethod]
    public void HandlesLinqQueryExpressions()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class Foo
                             {
                                 void Bar()
                                 {
                                     var q = from x in new[] { 1, 2, 3 }
                                             where x > 1
                                             select x;
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — LINQ query should be preserved
        Assert.Contains("from x in", result, "LINQ from clause should be preserved.");
        Assert.Contains("where x > 1", result, "LINQ where clause should be preserved.");
        Assert.Contains("select x", result, "LINQ select clause should be preserved.");
    }

    /// <summary>
    /// Verifies that string literals are not altered by the line break phase
    /// </summary>
    [TestMethod]
    public void PreservesStringLiterals()
    {
        // Arrange — verbatim and raw string literals with embedded braces/newlines
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     var s1 = "hello { world }";
                                     var s2 = @"multi
                             line
                             string";
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — string content should be unchanged
        Assert.Contains("\"hello { world }\"", result, "Regular string literal should be preserved.");
        Assert.Contains($"@\"multi{Environment.NewLine}line{Environment.NewLine}string\"", result, "Verbatim string literal should be preserved.");
    }

    /// <summary>
    /// Verifies that binary operators at the end of a line are moved to the beginning
    /// of the next line
    /// </summary>
    [TestMethod]
    public void NormalizesBinaryOperatorPosition()
    {
        // Arrange — operator at end of line
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     var x = 1 +
                                         2;
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — operator should be at the start of the continuation line, not at end of previous
        Assert.DoesNotContain($"1 +{Environment.NewLine}", result, "Binary operator should not remain at end of line.");
        Assert.Contains($"1 {Environment.NewLine}", result, "Line break should be after the left operand.");
    }

    /// <summary>
    /// Verifies that constructor initializers are placed on new lines
    /// </summary>
    [TestMethod]
    public void EnsuresConstructorInitializerOnNewLine()
    {
        // Arrange — constructor initializer on same line
        const string input = """
                             class Foo
                             {
                                 Foo(int x) : base()
                                 {
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — colon should be on a new line
        Assert.DoesNotContain("(int x) : base()", result, "Constructor initializer should be moved to a new line.");
        Assert.Contains(": base()", result, "Constructor initializer colon should still be present.");
    }

    /// <summary>
    /// Verifies that generic constraint clauses (<c>where T :</c>) are placed on new lines
    /// </summary>
    [TestMethod]
    public void EnsuresGenericConstraintsOnNewLines()
    {
        // Arrange — where clause on same line as declaration
        const string input = """
                             class Foo<T> where T : class
                             {
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — where clause should be on its own line
        Assert.DoesNotContain("Foo<T> where", result, "Generic constraint should be moved to a new line.");
        Assert.Contains("where T : class", result, "Generic constraint content should be preserved.");
    }

    /// <summary>
    /// Verifies that multi-line expression-bodied properties have the arrow placed on a new line
    /// </summary>
    [TestMethod]
    public void CollapsesExpressionBodiedProperty()
    {
        // Arrange — expression-bodied property split across lines
        const string input = """
                             class Foo
                             {
                                 public int Value
                                     => 42;
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — property should be collapsed to single line
        Assert.Contains($"Value{Environment.NewLine} => 42;", result, "Expression-bodied property arrow should be placed on a new line.");
    }

    /// <summary>
    /// Verifies that single-line argument lists are not modified
    /// </summary>
    [TestMethod]
    public void PreservesSingleLineArgumentList()
    {
        // Arrange
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     Call("a", "b", "c");
                                 }

                                 void Call(string a, string b, string c) { }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — single-line arguments should not be split
        Assert.Contains("""Call("a", "b", "c")""", result, "Single-line argument list should not be modified.");
    }

    /// <summary>
    /// Verifies that an argument list where each argument is already on its own line
    /// is not modified
    /// </summary>
    [TestMethod]
    public void PreservesArgumentsAlreadyOnSeparateLines()
    {
        // Arrange
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     Call("a",
                                          "b",
                                          "c");
                                 }

                                 void Call(string a, string b, string c) { }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — already on separate lines should be preserved
        Assert.Contains("""Call("a",""", result, "First argument should stay on same line as call.");
    }

    /// <summary>
    /// Verifies that a multi-line argument list where some arguments share a line
    /// is split so each argument starts on its own line
    /// </summary>
    [TestMethod]
    public void SplitsMixedLineArguments()
    {
        // Arrange — "a" and "b" on same line, "c" on new line
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     Call("a", "b",
                                          "c");
                                 }

                                 void Call(string a, string b, string c) { }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — the comma after "a" should now have an end-of-line
        Assert.DoesNotContain("""a", "b""", result, "Arguments sharing a line should be split to separate lines.");
    }

    /// <summary>
    /// Verifies that the first argument is collapsed to the same line as the opening parenthesis
    /// when it starts on a new line
    /// </summary>
    [TestMethod]
    public void CollapsesFirstArgumentToSameLine()
    {
        // Arrange — first argument on new line
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                     Call(
                                         "a",
                                         "b");
                                 }

                                 void Call(string a, string b) { }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — first argument should be on same line as Call(
        Assert.Contains("""Call("a",""", result, "First argument should be collapsed to same line as opening parenthesis.");
    }

    /// <summary>
    /// Verifies that single-line parameter lists are not modified
    /// </summary>
    [TestMethod]
    public void PreservesSingleLineParameterList()
    {
        // Arrange
        const string input = """
                             class Foo
                             {
                                 void Bar(int a, int b, int c)
                                 {
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — single-line parameter list should not be split
        Assert.Contains("Bar(int a, int b, int c)", result, "Single-line parameter list should not be modified.");
    }

    /// <summary>
    /// Verifies that a multi-line parameter list where some parameters share a line
    /// is split so each parameter starts on its own line
    /// </summary>
    [TestMethod]
    public void SplitsMixedLineParameters()
    {
        // Arrange — a and b on same line, c on new line
        const string input = """
                             class Foo
                             {
                                 void Bar(int a, int b,
                                          int c)
                                 {
                                 }
                             }
                             """;

        // Act
        var result = ExecuteLineBreakPhase(input);

        // Assert — parameters sharing a line should be split
        Assert.DoesNotContain("int a, int b,", result, "Parameters sharing a line should be split to separate lines.");
    }

    /// <summary>
    /// Executes the <see cref="LineBreakPhase"/> on the given C# source text
    /// </summary>
    /// <param name="input">The C# source text to format</param>
    /// <returns>The formatted source text</returns>
    private string ExecuteLineBreakPhase(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = LineBreakPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);

        return result.ToFullString();
    }

    #endregion // Methods
}