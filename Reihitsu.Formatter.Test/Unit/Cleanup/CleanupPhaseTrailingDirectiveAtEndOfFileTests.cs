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

    /// <summary>
    /// Verifies that a non-<c>#pragma</c> directive family at end of file stays on its own line (CRLF),
    /// proving the fix is trivia-kind agnostic rather than <c>#pragma</c>-specific
    /// </summary>
    [TestMethod]
    public void NonPragmaDirectiveAtEndOfFileKeepsSeparateLineCrLf()
    {
        // Arrange
        const string input = "class Foo\r\n{\r\n}\r\n#nullable restore\r\n";
        const string expected = "class Foo\r\n{\r\n}\r\n#nullable restore\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing #nullable directive must stay on separate lines.");
    }

    /// <summary>
    /// Verifies the same non-<c>#pragma</c> directive scenario under LF line endings
    /// </summary>
    [TestMethod]
    public void NonPragmaDirectiveAtEndOfFileKeepsSeparateLineLf()
    {
        // Arrange
        const string input = "class Foo\n{\n}\n#nullable restore\n";
        const string expected = "class Foo\n{\n}\n#nullable restore\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing #nullable directive must stay on separate lines.");
    }

    /// <summary>
    /// Verifies that disabled text (an inactive <c>#if false</c> body) at end of file stays on its own
    /// line, separate from the preceding closing brace (CRLF)
    /// </summary>
    [TestMethod]
    public void DisabledTextAtEndOfFileKeepsSeparateLineCrLf()
    {
        // Arrange
        const string input = "class Foo\r\n{\r\n}\r\n#if false\r\nint ignored;\r\n#endif\r\n";
        const string expected = "class Foo\r\n{\r\n}\r\n#if false\r\nint ignored;\r\n#endif\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing disabled text must stay on separate lines.");
    }

    /// <summary>
    /// Verifies the same disabled-text scenario under LF line endings
    /// </summary>
    [TestMethod]
    public void DisabledTextAtEndOfFileKeepsSeparateLineLf()
    {
        // Arrange
        const string input = "class Foo\n{\n}\n#if false\nint ignored;\n#endif\n";
        const string expected = "class Foo\n{\n}\n#if false\nint ignored;\n#endif\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing disabled text must stay on separate lines.");
    }

    /// <summary>
    /// Verifies that a trailing block comment (rather than a single-line comment) at end of file stays
    /// on its own line — the predicate must treat every non-whitespace, non-end-of-line trivia kind as
    /// content, not only <c>#pragma</c> or single-line comments (CRLF)
    /// </summary>
    [TestMethod]
    public void TrailingBlockCommentAtEndOfFileKeepsSeparateLineCrLf()
    {
        // Arrange
        const string input = "class Foo\r\n{\r\n}\r\n/* trailing comment */";
        const string expected = "class Foo\r\n{\r\n}\r\n/* trailing comment */";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing block comment must stay on separate lines.");
    }

    /// <summary>
    /// Verifies the same trailing block-comment scenario under LF line endings
    /// </summary>
    [TestMethod]
    public void TrailingBlockCommentAtEndOfFileKeepsSeparateLineLf()
    {
        // Arrange
        const string input = "class Foo\n{\n}\n/* trailing comment */";
        const string expected = "class Foo\n{\n}\n/* trailing comment */";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "The closing brace and the trailing block comment must stay on separate lines.");
    }

    /// <summary>
    /// Verifies that a detached node (not part of any syntax tree, so its last token's
    /// <c>GetNextToken()</c> is <see cref="SyntaxKind.None"/>) still has its trailing end-of-file
    /// newline removed — the widened predicate must not affect the pre-existing detached-node
    /// stripping behaviour used when formatting a freshly generated node before insertion
    /// </summary>
    [TestMethod]
    public void DetachedNodeTrailingEndOfLineIsStillRemoved()
    {
        // Arrange — a standalone member declaration parsed without an enclosing tree; its last token's
        // GetNextToken() is SyntaxKind.None both with and without includeZeroWidth
        var node = SyntaxFactory.ParseMemberDeclaration("void M()\r\n{\r\n}\r\n");

        // Act
        var result = CleanupPhase.Execute(node, TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual("void M()\r\n{\r\n}", actual, "A detached node's trailing end-of-file newline should still be removed.");
    }

    /// <summary>
    /// Verifies that a file containing nothing but a directive (no real syntax token before
    /// <see cref="SyntaxKind.EndOfFileToken"/>) is left byte-identical — the else branch that inspects
    /// the previous real token's trailing trivia never runs when there is no such token
    /// </summary>
    [TestMethod]
    public void DirectiveOnlyFileIsUnchanged()
    {
        // Arrange
        const string input = "#pragma warning restore CS1591\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual, "A directive-only file with no real syntax token must be left unchanged.");
    }

    #endregion // Methods
}