using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Cleanup;

namespace Reihitsu.Formatter.Test.Unit.Cleanup;

/// <summary>
/// Reproduction tests for issue #769 — a trailing <c>#pragma</c> directive (or trailing single-line
/// comment) at end of file is merged onto the preceding closing brace line by <see cref="CleanupPhase"/>
/// </summary>
[TestClass]
public class CleanupPhaseTrailingDirectiveAtEndOfFileTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the line break between a closing brace and a trailing <c>#pragma</c> directive at
    /// end of file is preserved (CRLF), using the issue's own minimal repro verbatim (issue #769)
    /// </summary>
    [TestMethod]
    public void TrailingPragmaAtEndOfFileKeepsSeparateLineCrLf()
    {
        // Arrange — verbatim from issue #769's minimal repro
        const string input = "class Foo\r\n{\r\n}\r\n#pragma warning restore CS1591\r\n";
        const string expected = "class Foo\r\n{\r\n}\r\n#pragma warning restore CS1591\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing #pragma directive must stay on separate lines.");
    }

    /// <summary>
    /// Verifies the same scenario under LF line endings (issue #769)
    /// </summary>
    [TestMethod]
    public void TrailingPragmaAtEndOfFileKeepsSeparateLineLf()
    {
        // Arrange
        const string input = "class Foo\n{\n}\n#pragma warning restore CS1591\n";
        const string expected = "class Foo\n{\n}\n#pragma warning restore CS1591\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing #pragma directive must stay on separate lines.");
    }

    /// <summary>
    /// Verifies that the line break between a closing brace and a trailing single-line comment at end
    /// of file is preserved (CRLF) — the same-shape, lower-severity variant the issue also reports,
    /// since the merged result is still syntactically valid C# (issue #769)
    /// </summary>
    [TestMethod]
    public void TrailingCommentAtEndOfFileKeepsSeparateLineCrLf()
    {
        // Arrange
        const string input = "class Foo\r\n{\r\n}\r\n// trailing comment\r\n";
        const string expected = "class Foo\r\n{\r\n}\r\n// trailing comment";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing comment must stay on separate lines.");
    }

    /// <summary>
    /// Verifies the same trailing-comment scenario under LF line endings (issue #769)
    /// </summary>
    [TestMethod]
    public void TrailingCommentAtEndOfFileKeepsSeparateLineLf()
    {
        // Arrange
        const string input = "class Foo\n{\n}\n// trailing comment\n";
        const string expected = "class Foo\n{\n}\n// trailing comment";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing comment must stay on separate lines.");
    }

    /// <summary>
    /// Verifies that a blank line separating a closing brace from a trailing <c>#pragma</c> directive
    /// at end of file is preserved rather than collapsed (CRLF) — the chat-reported additional scenario
    /// tied to issue #769
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeTrailingPragmaAtEndOfFileIsPreservedCrLf()
    {
        // Arrange
        const string input = "class Foo\r\n{\r\n}\r\n\r\n#pragma warning restore CS1591\r\n";
        const string expected = "class Foo\r\n{\r\n}\r\n\r\n#pragma warning restore CS1591\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The blank line before the trailing #pragma directive must be preserved.");
    }

    /// <summary>
    /// Verifies the same blank-line scenario under LF line endings — the chat-reported additional
    /// scenario tied to issue #769
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeTrailingPragmaAtEndOfFileIsPreservedLf()
    {
        // Arrange
        const string input = "class Foo\n{\n}\n\n#pragma warning restore CS1591\n";
        const string expected = "class Foo\n{\n}\n\n#pragma warning restore CS1591\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The blank line before the trailing #pragma directive must be preserved.");
    }

    #endregion // Methods
}