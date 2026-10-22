using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains unit tests for the <see cref="PlaygroundFormatter"/> class
/// </summary>
[TestClass]
public class PlaygroundFormatterTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that misindented, syntactically valid input is reformatted
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-005")]
    public void FormatReformatsMisindentedInput()
    {
        var input = "public class Foo\n{\npublic void Bar()\n{\nSystem.Console.WriteLine(\"hi\");\n}\n}\n";
        var expected = "public class Foo\n{\n    public void Bar()\n    {\n        System.Console.WriteLine(\"hi\");\n    }\n}";

        var result = PlaygroundFormatter.Format(input);

        Assert.IsTrue(result.Formatted);
        Assert.AreEqual(expected, result.Text);
    }

    /// <summary>
    /// Verifies that already-formatted input is returned unchanged, matching the formatter's idempotency guarantee
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-005")]
    public void FormatIsIdempotentForAlreadyFormattedInput()
    {
        var input = "public class Foo\n{\n    public void Bar()\n    {\n    }\n}";

        var result = PlaygroundFormatter.Format(input);

        Assert.IsTrue(result.Formatted);
        Assert.AreEqual(input, result.Text);
    }

    /// <summary>
    /// Verifies that syntactically invalid input is left unchanged, matching <see cref="Reihitsu.Formatter.ReihitsuFormatter"/>'s
    /// documented behavior for source with syntax errors
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void FormatLeavesSyntacticallyInvalidInputUnchanged()
    {
        var input = "public class Foo\n{\n";

        var result = PlaygroundFormatter.Format(input);

        Assert.IsFalse(result.Formatted);
        Assert.AreEqual(input, result.Text);
    }

    /// <summary>
    /// Verifies that the result equals the formatter's tree-level output for the same text parsed with Roslyn's default
    /// parse options
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-005")]
    public void FormatEqualsTreeLevelFormattingWithDefaultParseOptions()
    {
        var input = "using System;\nnamespace N{\nclass C{int P{get{return 1;}}\n#if DEBUG\n   int   d;\n#endif\nvoid M(){Console.WriteLine( 1 );}}}\n";
        var expected = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken).ToString();

        var result = PlaygroundFormatter.Format(input);

        Assert.IsTrue(result.Formatted);
        Assert.AreEqual(expected, result.Text);
    }

    /// <summary>
    /// Verifies that the input is parsed with the default language version, so a version-dependent transform applies
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-005")]
    public void FormatUsesTheDefaultLanguageVersion()
    {
        var input = "class C\n{\n    int P { get { return 1; } }\n}";

        var result = PlaygroundFormatter.Format(input);

        Assert.IsTrue(result.Formatted);
        Assert.AreEqual("class C\n{\n    int P => 1;\n}", result.Text);
    }

    /// <summary>
    /// Verifies that the input is parsed without preprocessor symbols, so a <c>#if DEBUG</c> branch is disabled text and
    /// is kept as written
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-005")]
    public void FormatDefinesNoPreprocessorSymbols()
    {
        var input = "class C\n{\n#if DEBUG\n   int   d;\n#endif\n    int x;\n}";

        var result = PlaygroundFormatter.Format(input);

        Assert.IsTrue(result.Formatted);
        Assert.AreEqual(input, result.Text);
    }

    /// <summary>
    /// Verifies that the line breaks of the input decide the line ending of the output
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-005")]
    public void FormatKeepsTheLineEndingOfTheInput()
    {
        var input = "class C\r\n{\r\nint x;\r\n}\r\n";

        var result = PlaygroundFormatter.Format(input);

        Assert.IsTrue(result.Formatted);
        Assert.AreEqual("class C\r\n{\r\n    int x;\r\n}", result.Text);
    }

    /// <summary>
    /// Verifies that formatting the result again returns it unchanged
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-005")]
    public void FormatIsIdempotentOnItsOwnResult()
    {
        var input = "public class Sample{public int Add(int a,int b){return a+b;}}";

        var first = PlaygroundFormatter.Format(input);
        var second = PlaygroundFormatter.Format(first.Text);

        Assert.IsTrue(second.Formatted);
        Assert.AreEqual(first.Text, second.Text);
    }

    #endregion // Tests
}