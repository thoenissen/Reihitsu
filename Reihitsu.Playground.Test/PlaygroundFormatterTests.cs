using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains unit tests for the <see cref="PlaygroundFormatter"/> class
/// </summary>
[TestClass]
public class PlaygroundFormatterTests
{
    #region Tests

    /// <summary>
    /// Verifies that misindented, syntactically valid input is reformatted
    /// </summary>
    [TestMethod]
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
    public void FormatLeavesSyntacticallyInvalidInputUnchanged()
    {
        var input = "public class Foo\n{\n";

        var result = PlaygroundFormatter.Format(input);

        Assert.IsFalse(result.Formatted);
        Assert.AreEqual(input, result.Text);
    }

    #endregion // Tests
}