using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Regression tests for a multi-line raw string literal whose lines are separated by a lone carriage return or a Unicode
/// line terminator: it is aligned as the same literal with <c>\n</c> line breaks is, and the line breaks inside it stay as
/// written (FMT-RAW-001, FMT-EOL-004)
/// </summary>
[TestClass]
public class RawStringOtherLineTerminatorAlignmentTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Makes line terminators visible, so an assertion message shows which one differs
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The text with its line terminators escaped</returns>
    private static string Escape(string text)
    {
        return text.Replace("\r", "\\r")
                   .Replace("\n", "\\n")
                   .Replace("\u0085", "\\u0085")
                   .Replace("\u2028", "\\u2028")
                   .Replace("\u2029", "\\u2029");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the reported example and its Unicode variants: the literal's content line and closing marker move under the
    /// opening quotes, a second pass changes nothing, and the terminators inside the literal stay
    /// </summary>
    /// <param name="terminator">The line terminator inside the literal</param>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void RawStringWithOtherLineTerminatorIsAligned(string terminator)
    {
        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var input = $"class C{endOfLine}{{{endOfLine}    void M(){endOfLine}    {{{endOfLine}        var a = \"\"\"{terminator}    Hello{terminator}    \"\"\";{endOfLine}    }}{endOfLine}}}";
            var expected = $"class C{endOfLine}{{{endOfLine}    void M(){endOfLine}    {{{endOfLine}        var a = \"\"\"{terminator}                Hello{terminator}                \"\"\";{endOfLine}    }}{endOfLine}}}";

            var actual = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken).Output.ToString();

            Assert.AreEqual(Escape(expected), Escape(actual));

            var secondPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(actual, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken).Output.ToString();

            Assert.AreEqual(Escape(actual), Escape(secondPass), "The formatter is not idempotent.");
        }
    }

    #endregion // Tests
}