using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for <see cref="ReihitsuFormatter"/>
/// </summary>
[TestClass]
public class ReihitsuFormatterTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> returns the original tree when the input contains syntax errors.
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeWithSyntaxErrorsReturnsOriginalTree()
    {
        // Arrange
        const string input = """
            namespace Test;

            public class Foo
            {
                public void Bar(
                {
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreSame(tree, result, "Tree with syntax errors should be returned unchanged.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> applies formatting to valid input.
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeValidInputReturnsFormattedTree()
    {
        // Arrange — missing blank line before `return`
        const string input = "namespace Test;\r\n\r\npublic class Foo\r\n{\r\n    public int Bar()\r\n    {\r\n        var x = 1;\r\n        return x;\r\n    }\r\n}\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = result.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreNotEqual(input, actual, "Formatter should have modified the code.");
        Assert.Contains("\r\n\r\n        return x;", actual, "A blank line should be inserted before the return statement.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> handles an empty file without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeEmptyFileDoesNotThrow()
    {
        // Arrange
        var input = string.Empty;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = result.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert — the formatter may normalize the file (e.g. add a trailing newline),
        // but it must not throw or produce syntax errors.
        Assert.IsFalse(result.GetDiagnostics(TestContext.CancellationTokenSource.Token).Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error), "Formatted empty file should not have syntax errors.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> formats a given syntax node.
    /// </summary>
    [TestMethod]
    public void FormatNodeFormatsGivenNode()
    {
        // Arrange — method with missing blank line before return
        const string input = "namespace Test;\r\n\r\npublic class Foo\r\n{\r\n    public int Bar()\r\n    {\r\n        var x = 1;\r\n        return x;\r\n    }\r\n}\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var methodNode = root.DescendantNodes().First(n => n is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax);

        // Act
        var result = ReihitsuFormatter.FormatNode(methodNode, cancellationToken: TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        // Assert
        Assert.AreNotEqual(methodNode.ToFullString(), actual, "FormatNode should have modified the method node.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns the original document when it contains syntax errors.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task FormatDocumentAsyncWithSyntaxErrorsReturnsOriginalDocument()
    {
        // Arrange
        const string input = """
            namespace Test;

            public class Foo
            {
                public void Bar(
                {
                }
            }
            """;

        using var workspace = new AdhocWorkspace();

        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(input));

        // Act
        var result = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationTokenSource.Token);

        // Assert
        var originalText = await document.GetTextAsync(TestContext.CancellationTokenSource.Token);
        var resultText = await result.GetTextAsync(TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(originalText.ToString(), resultText.ToString(), "Document with syntax errors should be returned unchanged.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> applies formatting to a valid document.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task FormatDocumentAsyncValidInputReturnsFormattedDocument()
    {
        // Arrange — missing blank line before `return`
        const string input = "namespace Test;\r\n\r\npublic class Foo\r\n{\r\n    public int Bar()\r\n    {\r\n        var x = 1;\r\n        return x;\r\n    }\r\n}\r\n";

        using var workspace = new AdhocWorkspace();

        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(input));

        // Act
        var result = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationTokenSource.Token);
        var resultText = (await result.GetTextAsync(TestContext.CancellationTokenSource.Token)).ToString();

        // Assert
        Assert.AreNotEqual(input, resultText, "Formatter should have modified the document.");
        Assert.Contains("\r\n\r\n        return x;", resultText, "A blank line should be inserted before the return statement.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> preserves \r\n line endings.
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeDetectsCarriageReturnLineFeed()
    {
        // Arrange — input uses \r\n line endings
        const string input = "namespace Test;\r\n\r\npublic class Foo\r\n{\r\n    public int Bar()\r\n    {\r\n        var x = 1;\r\n        return x;\r\n    }\r\n}\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = result.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.Contains("\r\n", actual, "Output should contain \\r\\n line endings.");
        Assert.DoesNotContain("\n", actual.Replace("\r\n", string.Empty), "Output should not contain bare \\n line endings when input uses \\r\\n.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> preserves \n line endings.
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeDetectsLineFeed()
    {
        // Arrange — input uses \n line endings
        const string input = "namespace Test;\n\npublic class Foo\n{\n    public int Bar()\n    {\n        var x = 1;\n        return x;\n    }\n}\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        // Act
        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = result.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.Contains("\n", actual, "Output should contain \\n line endings.");
        Assert.DoesNotContain("\r\n", actual, "Output should not contain \\r\\n line endings when input uses \\n.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> throws <see cref="OperationCanceledException"/> when cancellation is requested.
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeCancellationRequestedThrowsOperationCanceled()
    {
        // Arrange
        const string input = "namespace Test;\r\n\r\npublic class Foo\r\n{\r\n    public int Bar()\r\n    {\r\n        var x = 1;\r\n        return x;\r\n    }\r\n}\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);

        using var cts = new System.Threading.CancellationTokenSource();

        cts.Cancel();

        // Act & Assert
        Assert.ThrowsExactly<OperationCanceledException>(() => ReihitsuFormatter.FormatSyntaxTree(tree, cts.Token));
    }

    #endregion // Methods
}