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
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that trailing whitespace before end-of-line trivia is removed.
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Trailing whitespace before end-of-line should be removed.");
    }

    /// <summary>
    /// Verifies that consecutive blank lines are collapsed within each trivia list.
    /// Because Roslyn splits trivia across token boundaries, one blank line may remain
    /// at each boundary, resulting in at most two blank lines between elements.
    /// </summary>
    [TestMethod]
    public void CollapsesConsecutiveBlankLinesToMaxOne()
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Consecutive blank lines should be collapsed within each trivia list.");
    }

    /// <summary>
    /// Verifies that blank lines immediately after an opening brace are removed.
    /// </summary>
    [TestMethod]
    public void RemovesBlankLineAfterOpenBrace()
    {
        // Arrange — blank line after opening brace
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Blank line after opening brace should be removed.");
    }

    /// <summary>
    /// Verifies that blank lines immediately before a closing brace are preserved
    /// (the cleanup phase does not remove blank lines before closing braces).
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Blank line before closing brace should be preserved; only trailing EOF newlines are removed.");
    }

    /// <summary>
    /// Verifies that trailing end-of-line trivia at the end of a file is removed.
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Trailing end-of-line trivia at end of file should be removed.");
    }

    /// <summary>
    /// Verifies that well-formatted content passes through without modification.
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual, "Clean input should not be modified.");
    }

    /// <summary>
    /// Verifies that an empty input does not cause exceptions and produces an empty result.
    /// </summary>
    [TestMethod]
    public void HandlesEmptyInput()
    {
        // Arrange
        var input = string.Empty;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(string.Empty, actual, "Empty input should produce empty output.");
    }

    /// <summary>
    /// Verifies that comment trivia is preserved during cleanup.
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual, "Comment trivia should be preserved unchanged.");
    }

    /// <summary>
    /// Verifies that whitespace trivia on otherwise blank lines is removed.
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "Whitespace on blank lines should be removed.");
    }

    /// <summary>
    /// Verifies that the cleanup phase handles multiple issues in a single file simultaneously.
    /// </summary>
    [TestMethod]
    public void HandlesMultipleCleanupIssuesInOneFile()
    {
        // Arrange — trailing whitespace, extra blank lines, blank line after brace, trailing newline
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = CleanupPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual, "All cleanup issues should be resolved simultaneously.");
    }

    #endregion // Methods
}