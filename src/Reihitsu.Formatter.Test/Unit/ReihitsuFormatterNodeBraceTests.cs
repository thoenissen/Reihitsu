using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that a node-level entry point places the braces of a member body, a block, or an event's accessor list it is given as
/// target, whether the declaration that owns it lies inside the formatted node or not (FMT-BREAK-002)
/// </summary>
[TestClass]
public class ReihitsuFormatterNodeBraceTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the node-level example of FMT-BREAK-002: the body block of a method, given as target, gets its braces placed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public async Task MethodBodyTargetGetsItsBracesPlaced()
    {
        await AssertFormatsTarget("class C\n{\n    void M()\n    { Foo(); }\n}",
                                  "class C\n{\n    void M()\n    {\n        Foo();\n    }\n}",
                                  "    {\n        Foo();\n    }",
                                  root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body);
    }

    /// <summary>
    /// Verifies that the body block of a method whose closing brace stands behind its statement, given as target, gets that
    /// brace placed on a line of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public async Task MethodBodyTargetGetsItsClosingBracePlaced()
    {
        await AssertFormatsTarget("class C\n{\n    void M()\n    {\n        Foo(); }\n}",
                                  "class C\n{\n    void M()\n    {\n        Foo();\n    }\n}",
                                  "    {\n        Foo();\n    }",
                                  root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body);
    }

    /// <summary>
    /// Verifies that the body block of a constructor, given as target, gets its braces placed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public async Task ConstructorBodyTargetGetsItsBracesPlaced()
    {
        await AssertFormatsTarget("class C\n{\n    C()\n    { Foo(); }\n}",
                                  "class C\n{\n    C()\n    {\n        Foo();\n    }\n}",
                                  "    {\n        Foo();\n    }",
                                  root => root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().Single().Body);
    }

    /// <summary>
    /// Verifies that the body block of a finalizer, given as target, gets its braces placed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public async Task FinalizerBodyTargetGetsItsBracesPlaced()
    {
        await AssertFormatsTarget("class C\n{\n    ~C()\n    { Foo(); }\n}",
                                  "class C\n{\n    ~C()\n    {\n        Foo();\n    }\n}",
                                  "    {\n        Foo();\n    }",
                                  root => root.DescendantNodes().OfType<DestructorDeclarationSyntax>().Single().Body);
    }

    /// <summary>
    /// Verifies that the body block of an operator, given as target, gets its braces placed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public async Task OperatorBodyTargetGetsItsBracesPlaced()
    {
        await AssertFormatsTarget("class C\n{\n    public static C operator +(C a, C b)\n    { return a; }\n}",
                                  "class C\n{\n    public static C operator +(C a, C b)\n    {\n        return a;\n    }\n}",
                                  "    {\n        return a;\n    }",
                                  root => root.DescendantNodes().OfType<OperatorDeclarationSyntax>().Single().Body);
    }

    /// <summary>
    /// Verifies that the body block of a local function, given as target, gets its braces placed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public async Task LocalFunctionBodyTargetGetsItsBracesPlaced()
    {
        await AssertFormatsTarget("class C\n{\n    void M()\n    {\n        void L()\n        { Foo(); }\n    }\n}",
                                  "class C\n{\n    void M()\n    {\n        void L()\n        {\n            Foo();\n        }\n    }\n}",
                                  "        {\n            Foo();\n        }",
                                  root => root.DescendantNodes().OfType<LocalFunctionStatementSyntax>().Single().Body);
    }

    /// <summary>
    /// Verifies that the accessor list of an event, given as target, gets its braces and the braces of its accessors placed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public async Task EventAccessorListTargetGetsItsBracesPlaced()
    {
        await AssertFormatsTarget("class C\n{\n    event System.Action E\n    { add { } remove { } }\n}",
                                  "class C\n{\n    event System.Action E\n    {\n        add\n        {\n        }\n        remove\n        {\n        }\n    }\n}",
                                  "    {\n        add\n        {\n        }\n        remove\n        {\n        }\n    }",
                                  root => root.DescendantNodes().OfType<AccessorListSyntax>().Single());
    }

    /// <summary>
    /// Verifies that the body block of a property accessor, given as target, gets its braces placed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public async Task AccessorBodyTargetGetsItsBracesPlaced()
    {
        await AssertFormatsTarget("class C\n{\n    int P\n    {\n        get\n        { Foo(); return 1; }\n    }\n}",
                                  "class C\n{\n    int P\n    {\n        get\n        {\n            Foo();\n\n            return 1;\n        }\n    }\n}",
                                  "        {\n            Foo();\n\n            return 1;\n        }",
                                  root => root.DescendantNodes().OfType<AccessorDeclarationSyntax>().Single().Body);
    }

    /// <summary>
    /// Verifies that a statement block, given as target, gets its braces placed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public async Task StatementBlockTargetGetsItsBracesPlaced()
    {
        await AssertFormatsTarget("class C\n{\n    void M()\n    {\n        if (a)\n        { Foo(); }\n    }\n}",
                                  "class C\n{\n    void M()\n    {\n        if (a)\n        {\n            Foo();\n        }\n    }\n}",
                                  "        {\n            Foo();\n        }",
                                  root => root.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates a document with the given text in a new workspace
    /// </summary>
    /// <param name="text">Document text</param>
    /// <returns>The document</returns>
    private static Document CreateDocument(string text)
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);

        return project.AddDocument("Test.cs", SourceText.From(text));
    }

    /// <summary>
    /// Formats the selected target under LF and CRLF line endings through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>,
    /// asserting the document's text after one pass and that a second pass leaves it unchanged, and through
    /// <see cref="ReihitsuFormatter.FormatNode"/> on the attached target, asserting the node's text
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedDocumentWithLf">Expected document with LF line endings</param>
    /// <param name="expectedNodeWithLf">Expected text of the node <see cref="ReihitsuFormatter.FormatNode"/> returns, with LF line endings</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTarget(string inputWithLf, string expectedDocumentWithLf, string expectedNodeWithLf, Func<SyntaxNode, SyntaxNode> selectTarget)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var description = $"under {DescribeLineEnding(endOfLine)} line endings";
            var expected = NormalizeLineEndings(expectedDocumentWithLf, endOfLine);
            var input = NormalizeLineEndings(inputWithLf, endOfLine);
            var firstPass = await FormatInDocument(input, selectTarget);

            Assert.AreEqual(expected, firstPass, $"Unexpected first pass of FormatNodeInDocumentAsync {description}.");

            var secondPass = await FormatInDocument(firstPass, selectTarget);

            Assert.AreEqual(expected, secondPass, $"The second pass of FormatNodeInDocumentAsync must not change the output {description}.");

            var root = await CreateDocument(input).GetSyntaxRootAsync(TestContext.CancellationToken);
            var formattedNode = ReihitsuFormatter.FormatNode(selectTarget(root), cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(NormalizeLineEndings(expectedNodeWithLf, endOfLine), formattedNode.ToFullString(), $"Unexpected result of FormatNode on the attached target {description}.");
        }
    }

    /// <summary>
    /// Formats the selected target of the given text through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>
    /// </summary>
    /// <param name="text">Document text</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <returns>The document's text after formatting</returns>
    private async Task<string> FormatInDocument(string text, Func<SyntaxNode, SyntaxNode> selectTarget)
    {
        var document = CreateDocument(text);
        var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
        var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, selectTarget(root), TestContext.CancellationToken);

        return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
    }

    #endregion // Methods
}