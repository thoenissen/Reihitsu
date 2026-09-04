using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// End-to-end reproduction test for issue #769 via <see cref="ReihitsuFormatter.FormatSyntaxTree"/> —
/// the public entry point the issue itself names, as opposed to unit-level <c>CleanupPhase</c> access
/// </summary>
[TestClass]
public class ReihitsuFormatterTrailingDirectiveAtEndOfFileTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> keeps a trailing <c>#pragma</c>
    /// directive at end of file on its own line, separate from the preceding closing brace (issue #769)
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeKeepsTrailingPragmaOnOwnLineAtEndOfFile()
    {
        // Arrange — the issue's own reported scenario
        const string input = "namespace Test\r\n{\r\n    public class Foo\r\n    {\r\n        public void Bar()\r\n        {\r\n        }\r\n    }\r\n}\r\n#pragma warning restore CS1591\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);
        var actual = result.GetRoot(TestContext.CancellationToken).ToFullString();

        // Assert
        Assert.Contains("}\r\n#pragma warning restore CS1591", actual, "The closing brace and the trailing #pragma directive must stay on separate lines.");
        Assert.DoesNotContain("}#pragma", actual, "The closing brace and the #pragma directive must not be merged onto one line.");
    }

    /// <summary>
    /// Verifies that a syntax-invalid file (a <c>#undef</c> directive appearing after the first real
    /// token, which is a compile error) is returned untouched by
    /// <see cref="ReihitsuFormatter.FormatSyntaxTree"/> — the pre-existing syntax-error guard must keep
    /// behaving the same way once <see cref="Reihitsu.Formatter.Pipeline.Cleanup.CleanupPhase"/> stops
    /// unconditionally stripping the end-of-file newline
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeLeavesSyntaxInvalidTrailingDirectiveFileUntouched()
    {
        // Arrange — #undef after the first real token is CS1032, a syntax error
        const string input = "class Foo\r\n{\r\n}\r\n#undef X\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);
        var actual = result.GetRoot(TestContext.CancellationToken).ToFullString();

        // Assert
        Assert.AreEqual(input, actual, "Syntax-invalid source must be returned untouched.");
    }

    #endregion // Methods
}