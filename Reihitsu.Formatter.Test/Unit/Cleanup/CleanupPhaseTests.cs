using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Cleanup;

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
    /// Verifies that tab characters within whitespace trivia are replaced with an equivalent run of spaces
    /// </summary>
    [TestMethod]
    public void ReplacesTabsInWhitespaceTriviaWithSpaces()
    {
        // Arrange — a tab used as the gap before a trailing comment, which no earlier phase normalizes
        const string input = "class Foo\r\n{\r\n    int x;\t// comment\r\n}";
        const string expected = "class Foo\r\n{\r\n    int x;    // comment\r\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Tab characters in whitespace trivia should be replaced with spaces.");
    }

    /// <summary>
    /// Verifies that tabs inside comment trivia are preserved, since only whitespace trivia is normalized
    /// </summary>
    [TestMethod]
    public void PreservesTabsInsideComments()
    {
        // Arrange — a tab inside a comment body is content, not indentation
        const string input = "class Foo\r\n{\r\n    /* col\tumn */\r\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual, "Tabs inside comment trivia should be preserved unchanged.");
    }

    /// <summary>
    /// Verifies that a tab inside a non-region preprocessor directive's own interior (for example the gap
    /// between "pragma" and "warning" in a #pragma directive) is replaced with spaces. Directive trivia is
    /// structured, so the token-level cleanup pass never visits tokens inside it directly
    /// </summary>
    [TestMethod]
    public void ReplacesTabsInsideDirectiveInteriorWithSpaces()
    {
        // Arrange — a tab used as the keyword gap inside a #pragma directive
        const string input = "class Foo\r\n{\r\n#pragma\twarning disable CS0168\r\n    int x;\r\n#pragma warning restore CS0168\r\n}";
        const string expected = "class Foo\r\n{\r\n#pragma    warning disable CS0168\r\n    int x;\r\n#pragma warning restore CS0168\r\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Tabs inside a directive's own interior should be replaced with spaces.");
    }

    #endregion // Methods
}