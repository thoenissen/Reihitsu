using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #314: expanding a single-line type that contains a member must place
/// the opening brace on its own line (Allman style) even when a leading <c>using</c> directive
/// precedes the type. The offset introduced by the using directive previously caused the
/// brace-placement edit to operate on a stale token, leaving the opening brace on the declaration line
/// </summary>
[TestClass]
public class SingleLineTypeBraceWithLeadingUsingTests
{
    #region Constants

    /// <summary>
    /// Single-line type containing a field member, preceded by a leading using directive and attribute
    /// </summary>
    private const string Input = """
                                 using System;

                                 [Serializable]
                                 internal class TestClass { private int _value; }
                                 """;

    /// <summary>
    /// Expected output with the opening brace on its own line and the member indented inside the braces
    /// </summary>
    private const string Expected = """
                                    using System;

                                    [Serializable]
                                    internal class TestClass
                                    {
                                        private int _value;
                                    }
                                    """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Formats the given source through the full pipeline using the requested end-of-line sequence
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="endOfLine">The end-of-line sequence to use</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input, string endOfLine, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(endOfLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Normalizes every line break in the given text to the requested end-of-line sequence
    /// </summary>
    /// <param name="text">The text to normalize</param>
    /// <param name="endOfLine">The end-of-line sequence to apply</param>
    /// <returns>The text using the requested line endings</returns>
    private static string Normalize(string text, string endOfLine)
    {
        return text.Replace("\r\n", "\n").Replace("\n", endOfLine);
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that a single-line type preceded by a leading using directive expands with the
    /// opening brace on its own line when the source uses LF line endings
    /// </summary>
    [TestMethod]
    public void OpeningBraceMovesToOwnLineWithLeadingUsingLf()
    {
        var input = Normalize(Input, "\n");
        var expected = Normalize(Expected, "\n");

        var actual = Format(input, "\n", TestContext.CancellationToken);

        Assert.AreEqual(expected, actual, "The opening brace of the expanded type must be placed on its own line.");
    }

    /// <summary>
    /// Verifies that a single-line type preceded by a leading using directive expands with the
    /// opening brace on its own line when the source uses CRLF line endings
    /// </summary>
    [TestMethod]
    public void OpeningBraceMovesToOwnLineWithLeadingUsingCrlf()
    {
        var input = Normalize(Input, "\r\n");
        var expected = Normalize(Expected, "\r\n");

        var actual = Format(input, "\r\n", TestContext.CancellationToken);

        Assert.AreEqual(expected, actual, "The opening brace of the expanded type must be placed on its own line.");
    }

    /// <summary>
    /// Verifies that formatting the already-expanded output is idempotent
    /// </summary>
    [TestMethod]
    public void ExpandedTypeRemainsStableOnSecondPass()
    {
        var expected = Normalize(Expected, "\n");

        var actual = Format(expected, "\n", TestContext.CancellationToken);

        Assert.AreEqual(expected, actual, "Formatting an already-expanded type must be idempotent.");
    }

    #endregion // Tests
}