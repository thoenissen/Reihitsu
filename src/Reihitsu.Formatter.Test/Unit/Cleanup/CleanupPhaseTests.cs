using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Cleanup;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Cleanup;

/// <summary>
/// Tests for <see cref="CleanupPhase"/>
/// </summary>
[TestClass]
public class CleanupPhaseTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that trailing whitespace before end-of-line trivia is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    public void RemovesTrailingWhitespaceBeforeEndOfLine()
    {
        // Arrange — lines have trailing spaces before the line break
        const string input = """
                             class Foo   
                             {
                                 int x;   
                             }
                             """;
        const string expected = """
                                class Foo
                                {
                                    int x;
                                }
                                """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Trailing whitespace before end-of-line should be removed.");
    }

    /// <summary>
    /// Verifies that cleanup does not own blank-line collapsing. Collapsing excessive blank lines
    /// is owned solely by <c>BlankLineCollapser</c>, so the cleanup phase preserves consecutive
    /// blank lines and only strips the end-of-file newline
    /// </summary>
    [TestMethod]
    public void PreservesConsecutiveBlankLinesForStructuralPhases()
    {
        // Arrange — three blank lines between members
        const string input = """
                             class Foo
                             {
                                 int x;



                                 int y;
                             }

                             """;
        const string expected = """
                                class Foo
                                {
                                    int x;



                                    int y;
                                }
                                """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Cleanup should preserve consecutive blank lines; collapsing belongs to BlankLineCollapser.");
    }

    /// <summary>
    /// Verifies that cleanup does not own structural blank-line removal after an opening brace
    /// </summary>
    [TestMethod]
    public void PreservesBlankLineAfterOpenBraceForStructuralPhases()
    {
        // Arrange — structural blank-line policy belongs to BlankLinePhase, not CleanupPhase
        const string input = """
                             class Foo
                             {
                             
                                 int x;
                             }
                             
                             """;
        const string expected = """
                                class Foo
                                {
                                
                                    int x;
                                }
                                """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Cleanup should preserve structural blank-line decisions for BlankLinePhase.");
    }

    /// <summary>
    /// Verifies that blank lines immediately before a closing brace are preserved
    /// (the cleanup phase does not remove blank lines before closing braces)
    /// </summary>
    [TestMethod]
    public void RemovesBlankLineBeforeCloseBrace()
    {
        // Arrange — blank line before closing brace
        const string input = """
                             class Foo
                             {
                                 int x;

                             }

                             """;
        const string expected = """
                                class Foo
                                {
                                    int x;

                                }
                                """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Blank line before closing brace should be preserved; only trailing EOF newlines are removed.");
    }

    /// <summary>
    /// Verifies that trailing end-of-line trivia at the end of a file is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void RemovesTrailingEndOfLineAtEndOfFile()
    {
        // Arrange — multiple trailing newlines at end of file
        const string input = """
                             class Foo
                             {
                             }



                             """;
        const string expected = """
                                class Foo
                                {
                                }
                                """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Trailing end-of-line trivia at end of file should be removed.");
    }

    /// <summary>
    /// Verifies that well-formatted content passes through without modification
    /// </summary>
    [TestMethod]
    public void PreservesContentWithNoCleanupNeeded()
    {
        // Arrange — already clean code with no trailing whitespace, no extra blank lines
        const string input = """
                             class Foo
                             {
                                 int x;
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual, "Clean input should not be modified.");
    }

    /// <summary>
    /// Verifies that cleanup does not own blank-line insertion before <c>#endregion</c>
    /// </summary>
    [TestMethod]
    public void DoesNotInsertBlankLineBeforeEndRegionDirective()
    {
        // Arrange — structural region spacing belongs to BlankLinePhase, not CleanupPhase
        const string input = """
                             class Foo
                             {
                                 #region Methods

                                 void M()
                                 {
                                 }
                                 #endregion // Methods
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual, "Cleanup should not enforce structural blank-line policy before #endregion.");
    }

    /// <summary>
    /// Verifies that an empty input does not cause exceptions and produces an empty result
    /// </summary>
    [TestMethod]
    public void HandlesEmptyInput()
    {
        // Arrange
        var input = string.Empty;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(string.Empty, actual, "Empty input should produce empty output.");
    }

    /// <summary>
    /// Verifies that comment trivia is preserved during cleanup
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-004")]
    public void PreservesCommentTrivia()
    {
        // Arrange — code with single-line and multi-line comments
        const string input = """
                             // Header comment
                             class Foo
                             {
                                 /* inline */ int x;
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual, "Comment trivia should be preserved unchanged.");
    }

    /// <summary>
    /// Verifies that whitespace trivia on otherwise blank lines is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    public void RemovesWhitespaceBetweenEndOfLines()
    {
        // Arrange — whitespace on blank lines (spaces on an otherwise empty line)
        const string input = """
                             class Foo
                             {
                                 int x;
                                 
                                 int y;
                             }

                             """;
        const string expected = """
                                class Foo
                                {
                                    int x;

                                    int y;
                                }
                                """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Whitespace on blank lines should be removed.");
    }

    /// <summary>
    /// Verifies that the cleanup phase handles multiple trivia-noise issues in a single file
    /// simultaneously. Blank-line collapsing is intentionally not one of them — that is owned by
    /// <c>BlankLineCollapser</c> — so consecutive blank lines are preserved
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    public void HandlesMultipleCleanupIssuesInOneFile()
    {
        // Arrange — trailing whitespace, blank line after brace, trailing newline
        const string input = """
                             class Foo
                             {

                                 int x;   



                                 int y;
                             }



                             """;
        const string expected = """
                                class Foo
                                {

                                    int x;



                                    int y;
                                }
                                """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "All cleanup issues should be resolved simultaneously.");
    }

    /// <summary>
    /// Verifies that a final blank line made of whitespace-then-newline is still trimmed away entirely —
    /// the strip-side boundary of the widened end-of-file predicate: end-of-file leading
    /// trivia that holds only whitespace and end-of-line trivia must still be treated as "nothing left
    /// to print", not as content that blocks the strip
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void RemovesTrailingWhitespaceOnlyBlankLineAtEndOfFile()
    {
        // Arrange — a final blank line consisting of trailing whitespace followed by a newline
        const string input = "class Foo\r\n{\r\n}\r\n \r\n";
        const string expected = "class Foo\r\n{\r\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "A trailing whitespace-only blank line at end of file should be fully trimmed.");
    }

    /// <summary>
    /// Verifies that the whitespace behind a token is removed when the line break that ends the line is the first leading
    /// trivia of the following token, as it is after an earlier phase moved the line break there
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    public void RemovesTrailingWhitespaceBeforeLineBreakInFollowingTokenLeadingTrivia()
    {
        // Arrange — move the line break behind "int x;" from its trailing trivia into the leading trivia of "}"
        var tree = CSharpSyntaxTree.ParseText("class Foo\n{\n    int x;   \n}", cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var semicolon = root.DescendantTokens().Single(token => token.IsKind(SyntaxKind.SemicolonToken));
        var closeBrace = semicolon.GetNextToken();
        var rewritten = root.ReplaceTokens([semicolon, closeBrace],
                                           (original, _) => original == semicolon
                                                                ? original.WithTrailingTrivia(SyntaxFactory.Whitespace("   "))
                                                                : original.WithLeadingTrivia(SyntaxFactory.LineFeed));

        Assert.AreEqual("class Foo\n{\n    int x;   \n}", rewritten.ToFullString());

        // Act
        var result = CleanupPhase.Execute(rewritten, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual("class Foo\n{\n    int x;\n}", result.ToFullString(), "Whitespace in front of a line break in the next token's leading trivia should be removed.");
    }

    /// <summary>
    /// Verifies that a byte order mark directly in front of the line break that ends the first line is kept, although
    /// the parser reads it as white-space trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    public void KeepsByteOrderMarkBeforeLineBreakOfTheFirstLine()
    {
        // Arrange
        const string input = "\uFEFF\nclass Foo\n{\n}";
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual, "The byte order mark in front of the first line break should be kept.");
    }

    /// <summary>
    /// Verifies that the white space after a byte order mark in front of the line break that ends the first line is
    /// removed while the byte order mark is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    [Requirement("FMT-CLEAN-001")]
    public void RemovesWhiteSpaceAfterByteOrderMarkBeforeLineBreakOfTheFirstLine()
    {
        // Arrange
        const string input = "\uFEFF   \nclass Foo\n{\n}";
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual("\uFEFF\nclass Foo\n{\n}", actual, "The byte order mark should be kept and the white space after it removed.");
    }

    /// <summary>
    /// Verifies the other side of the boundary: a white-space trivia that starts with the byte order mark character on
    /// a line that holds nothing else, in the middle of the text, is removed like any other white space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-009")]
    [Requirement("FMT-CLEAN-001")]
    public void RemovesByteOrderMarkCharacterOnBlankLineInTheMiddleOfTheText()
    {
        // Arrange
        const string input = "class Foo\n{\n    int x;\n\uFEFF  \n}";
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual("class Foo\n{\n    int x;\n\n}", actual, "White space that starts with the byte order mark character in the middle of the text should be removed.");
    }

    #endregion // Methods
}