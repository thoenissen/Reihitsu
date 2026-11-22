using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the unsupported outcome at every formatting entry point of <see cref="ReihitsuFormatter"/>: the result object, the
/// reason and the location it carries, which node the decision is made on, and <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/>
/// </summary>
[TestClass]
public class UnsupportedSyntaxEntryPointTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The class of the FMT-UNSUP-002 example: the method <c>M</c> holds a declaration the formatter refuses, and the method
    /// <c>N</c> holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    void M()
                                                                    {
                                                                        int /* c */ a = 1;
                                                                    }

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: <see cref="ReihitsuFormatter.FormatSyntaxTree"/> returns the tree unchanged, also the
    /// brace and the spacing it would correct elsewhere, and reports the reason and the comment, which starts in line 3, column 13
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    public void SpecExampleFormatSyntaxTreeReturnsTheTreeUnchangedAndReportsTheComment()
    {
        const string input = """
                             class C{
                                 void M(){
                                     int /* c */ a=1;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");

        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);

            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            Assert.AreEqual(new LinePosition(2, 12), result.Unsupported.Location.GetLineSpan().StartLinePosition, $"The comment starts in line 3, column 13 under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies the specification example: of two unsupported shapes, the one that starts first in the text is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    public void SpecExampleShapeThatStartsFirstInTheTextIsReported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int b = /* second */ 1;
                                     int /* first */ a = 1;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* second */");
    }

    /// <summary>
    /// Verifies the specification example: a tree with a syntax error is returned unchanged and is not reported as unsupported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    public void SpecExampleSyntaxErrorIsNotReportedAsUnsupported()
    {
        var tree = CSharpSyntaxTree.ParseText("class C{ int x }", cancellationToken: TestContext.CancellationToken);

        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual("class C{ int x }", result.Output.ToString());
        Assert.IsNull(result.Unsupported);
    }

    /// <summary>
    /// Verifies that a tree with a syntax error that also holds an unsupported shape is returned unchanged and is not reported as
    /// unsupported: the syntax error decides first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-002")]
    public void SyntaxErrorBesideAnUnsupportedShapeIsNotReportedAsUnsupported()
    {
        const string input = "class C{ void M(){ int /* c */ a = 1 } }";
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(input, result.Output.ToString());
        Assert.IsNull(result.Unsupported);
    }

    /// <summary>
    /// Verifies that a file with a generated-code header that holds an unsupported shape is returned unchanged and is not
    /// reported as unsupported: the generated-code marker decides first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-002")]
    public void GeneratedCodeWithAnUnsupportedShapeIsNotReportedAsUnsupported()
    {
        const string input = "// <auto-generated />\nclass C{ void M(){ int /* c */ a=1; } }";
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(input, result.Output.ToString());
        Assert.IsNull(result.Unsupported);
    }

    /// <summary>
    /// Verifies that a formatted tree is reported as not unsupported, and its output is the formatted tree
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    public void FormattedTreeHasNoUnsupportedSyntax()
    {
        var tree = CSharpSyntaxTree.ParseText("class C{ void M(){ int a=1; } }", cancellationToken: TestContext.CancellationToken);

        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.IsNull(result.Unsupported);
        Assert.AreEqual("class C\n{\n    void M()\n    {\n        int a = 1;\n    }\n}", result.Output.ToString().ReplaceLineEndings("\n"));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment's span in the document's syntax tree
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-002")]
    public async Task FormatDocumentAsyncReturnsTheDocumentUnchangedAndReportsTheComment()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(RefusedMethodBesideFormattableMethod, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);

                var result = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken);

                Assert.AreEqual(text, (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(result.Unsupported, text);
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> returns a refused node unchanged and reports the comment's span in
    /// the node's syntax tree
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-002")]
    public void FormatNodeReturnsTheNodeUnchangedAndReportsTheComment()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(RefusedMethodBesideFormattableMethod, endOfLine);
            var method = FindMethod(CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken), "M");

            var result = ReihitsuFormatter.FormatNode(method, cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(method.ToFullString(), result.Output.ToFullString());
            AssertReportsComment(result.Unsupported, text);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> formats a node that holds no unsupported shape, although the tree
    /// around it holds one, and reports nothing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    [Requirement("FMT-UNSUP-003")]
    public void FormatNodeFormatsANodeBesideAnUnsupportedShape()
    {
        var method = FindMethod(CSharpSyntaxTree.ParseText(RefusedMethodBesideFormattableMethod, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken), "N");

        var result = ReihitsuFormatter.FormatNode(method, cancellationToken: TestContext.CancellationToken);

        Assert.IsNull(result.Unsupported);
        Assert.AreNotEqual(method.ToFullString(), result.Output.ToFullString(), "The node must be formatted.");
    }

    /// <summary>
    /// Verifies the specification example: <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> with the method <c>N</c> as
    /// target formats <c>N</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    public async Task SpecExampleFormatNodeInDocumentAsyncFormatsATargetWithoutTheShape()
    {
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int /* c */ a = 1;
                                    }

                                    void N()
                                    {
                                    }
                                }
                                """;

        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(RefusedMethodBesideFormattableMethod, endOfLine));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, FindMethod(root, "N"), TestContext.CancellationToken);

                Assert.IsNull(result.Unsupported);
                Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
            }
        }
    }

    /// <summary>
    /// Verifies the specification example: <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> with the method <c>M</c> as
    /// target returns the document unchanged and reports the comment's span in the document's syntax tree
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-002")]
    public async Task SpecExampleFormatNodeInDocumentAsyncRefusesATargetThatHoldsTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(RefusedMethodBesideFormattableMethod, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, FindMethod(root, "M"), TestContext.CancellationToken);

                Assert.AreEqual(text, (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(result.Unsupported, text);
            }
        }
    }

    /// <summary>
    /// Verifies the specification example: <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> with the method
    /// <c>N</c> as target and the class as context returns the document unchanged and reports the comment, because the context
    /// holds the declaration in <c>M</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-002")]
    public async Task SpecExampleContextEntryPointRefusesAContextThatHoldsTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(RefusedMethodBesideFormattableMethod, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var type = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

                var result = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, FindMethod(root, "N"), type, TestContext.CancellationToken);

                Assert.AreEqual(text, (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(result.Unsupported, text);
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> formats its target when the context holds
    /// no unsupported shape, although the document does elsewhere
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    [Requirement("FMT-UNSUP-003")]
    public async Task ContextEntryPointFormatsATargetWhoseContextHoldsNoShape()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int /* c */ a = 1;
                                 }
                             }

                             class D
                             {
                                 void N(){ }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int /* c */ a = 1;
                                    }
                                }

                                class D
                                {
                                    void N()
                                    {
                                    }
                                }
                                """;

        using (var workspace = new AdhocWorkspace())
        {
            var document = CreateDocument(workspace, input);
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
            var type = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single(candidate => candidate.Identifier.ValueText == "D");

            var result = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, FindMethod(root, "N"), type, TestContext.CancellationToken);

            Assert.IsNull(result.Unsupported);
            Assert.AreEqual(expected, (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
        }
    }

    /// <summary>
    /// Verifies the specification example: the equals-value clause as target holds the <c>=</c> and the comment but not the
    /// variable declaration, so it is not refused, while the local declaration statement as target and the document are refused
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    public async Task SpecExampleTargetBelowTheDeclarationIsFormattedWhileTheDeclarationIsRefused()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var d = /* a */ new List<int> { 1 };
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

                var clauseResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, root.DescendantNodes().OfType<EqualsValueClauseSyntax>().Single(), TestContext.CancellationToken);

                Assert.IsNull(clauseResult.Unsupported, "The equals-value clause holds no variable declaration.");

                var statementResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().Single(), TestContext.CancellationToken);

                Assert.AreEqual(text, (await statementResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(statementResult.Unsupported, text, "/* a */");

                var documentResult = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken);

                Assert.AreEqual(text, (await documentResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(documentResult.Unsupported, text, "/* a */");
            }
        }
    }

    /// <summary>
    /// Verifies the specification example: <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> returns the reason and the span of
    /// the comment for the class and for the method that hold the refused declaration, and null for the field declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-007")]
    public void SpecExampleFindUnsupportedSyntaxAnswersForTheNodeItIsGiven()
    {
        const string input = """
                             class C
                             {
                                 private int _a, _b;

                                 void M()
                                 {
                                     int /* c */ x = 1;
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

            AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single()), text);
            AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(FindMethod(root, "M")), text);
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(root.DescendantNodes().OfType<FieldDeclarationSyntax>().Single()));
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same node as
    /// the input of the phases: the same reason and location for a refused tree, and null for one the formatter formats
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-007")]
    [DataRow("class C\n{\n    int /* c */ _a;\n    int _b = /* d */ 1;\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        for (int i = 0, /* c */ j = 0; i < j; i++)\n        {\n        }\n    }\n}")]
    [DataRow("class C\n{\n    // c\n    int _a = 1; // d\n}")]
    [DataRow("class C\n{\n    int P { get; } = /* c */ 1;\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for the node alone: an equals-value clause that
    /// holds a comment behind the declaration's <c>=</c> holds no variable declaration and is not refused, and the local
    /// declaration statement around it is
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-007")]
    public void FindUnsupportedSyntaxAnswersForTheNodeAlone()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var d = /* a */ 1;\n    }\n}";
        var root = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(root.DescendantNodes().OfType<EqualsValueClauseSyntax>().Single()));
        AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().Single()), input, "/* a */");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds, which stands in a
    /// tree of its own: the field built from a property with a comment behind its type is refused
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-007")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode()
    {
        var field = SyntaxFactory.ParseMemberDeclaration("private int /* note */ _value;");

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(field);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(VariableDeclarationCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, field.ToFullString(), "/* note */", 0, "LF");
    }

    /// <summary>
    /// Asserts that the outcome reports the reason of FMT-UNSUP-004 and the span of the comment in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    /// <param name="comment">The comment that has to be reported</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text, string comment = "/* c */")
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(VariableDeclarationCommentReason, unsupported.Reason);
        Assert.AreEqual(text, unsupported.Location.SourceTree.GetText().ToString(), "The location must be in the caller's syntax tree.");
        AssertLocationIsShape(unsupported.Location, text, comment, 0, "the document's");
    }

    /// <summary>
    /// Creates a document with the given text in a new project of the workspace
    /// </summary>
    /// <param name="workspace">The workspace</param>
    /// <param name="text">The document's text</param>
    /// <returns>The document</returns>
    private static Document CreateDocument(AdhocWorkspace workspace, string text)
    {
        return workspace.AddProject("TestProject", LanguageNames.CSharp).AddDocument("Test.cs", SourceText.From(text));
    }

    /// <summary>
    /// Finds the method with the given name
    /// </summary>
    /// <param name="root">The root to search</param>
    /// <param name="name">The method's name</param>
    /// <returns>The method</returns>
    private static MethodDeclarationSyntax FindMethod(SyntaxNode root, string name)
    {
        return root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single(method => method.Identifier.ValueText == name);
    }

    #endregion // Methods
}