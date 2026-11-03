using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that the node-level entry points of <see cref="ReihitsuFormatter"/> keep the text of region directives, while the
/// document and syntax-tree entry points apply the region requirements
/// </summary>
[TestClass]
public class ReihitsuFormatterNodeRegionTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The spec example input: a class with a lower-case region description behind several spaces, a bare
    /// <c>#endregion</c>, and a property whose accessor the node-level entry points expand
    /// </summary>
    private const string SpecExampleInput = """
                                            class C
                                            {
                                                #region    fields

                                                int P { get { return 1; } }

                                                #endregion
                                            }
                                            """;

    /// <summary>
    /// The spec example output
    /// </summary>
    private const string SpecExampleExpected = """
                                               class C
                                               {
                                                   #region    fields

                                                   int P
                                                   {
                                                       get
                                                       {
                                                           return 1;
                                                       }
                                                   }

                                                   #endregion
                                               }
                                               """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the spec example: <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> with the class as target formats
    /// the property and keeps the text of both region directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatNodeInDocumentAsyncKeepsRegionDirectivesOfTheSpecExample()
    {
        await AssertNodeInDocument(SpecExampleInput, SpecExampleExpected, root => FindType(root, "C"), contextSelector: null);
    }

    /// <summary>
    /// Verifies the spec example through <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/>, with the class
    /// as target and its namespace as context
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatNodeInDocumentWithContextAsyncKeepsRegionDirectives()
    {
        const string input = """
                             namespace N
                             {
                                 class C
                                 {
                                     #region    fields

                                     int P { get { return 1; } }

                                     #endregion
                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    class C
                                    {
                                        #region    fields

                                        int P
                                        {
                                            get
                                            {
                                                return 1;
                                            }
                                        }

                                        #endregion
                                    }
                                }
                                """;

        await AssertNodeInDocument(input, expected, root => FindType(root, "C"), root => root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().First());
    }

    /// <summary>
    /// Verifies the spec example through <see cref="ReihitsuFormatter.FormatNode"/> on the detached class
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public void FormatNodeKeepsRegionDirectives()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var expected = NormalizeLineEndings(SpecExampleExpected, endOfLine);
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(SpecExampleInput, endOfLine), cancellationToken: TestContext.CancellationToken);
            var target = FindType(tree.GetRoot(TestContext.CancellationToken), "C");

            var firstPass = ReihitsuFormatter.FormatNode(target, cancellationToken: TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(expected, firstPass, $"First pass mismatch under {DescribeLineEnding(endOfLine)} line endings.");

            var secondTree = CSharpSyntaxTree.ParseText(firstPass, cancellationToken: TestContext.CancellationToken);
            var secondPass = ReihitsuFormatter.FormatNode(FindType(secondTree.GetRoot(TestContext.CancellationToken), "C"), cancellationToken: TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(expected, secondPass, $"Second pass mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that several spaces in front of an upper-case description and the <c>#endregion</c> comment behind it are kept
    /// by a node-level entry point
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatNodeInDocumentAsyncKeepsSeveralSpacesBeforeUpperCaseDescription()
    {
        const string input = """
                             class C
                             {
                                 #region    Bar

                                 int a;

                                 #endregion    Bar
                             }
                             """;

        await AssertNodeInDocument(input, input, root => FindType(root, "C"), contextSelector: null);
    }

    /// <summary>
    /// Verifies that whitespace behind a region description is kept by a node-level entry point
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatNodeInDocumentAsyncKeepsWhitespaceBehindRegionDescription()
    {
        const string input = "class C\n{\n    #region Baz   \n\n    int b;\n\n    #endregion // Baz\n}";

        await AssertNodeInDocument(input, input, root => FindType(root, "C"), contextSelector: null);
    }

    /// <summary>
    /// Verifies that the whitespace around the text of an <c>#endregion</c> of an undescribed region is kept by a node-level
    /// entry point
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatNodeInDocumentAsyncKeepsWhitespaceAroundTextOfEndRegionOfUndescribedRegion()
    {
        const string input = "class C\n{\n    #region\n\n    int a;\n\n    #endregion    // Stale   \n}";

        await AssertNodeInDocument(input, input, root => FindType(root, "C"), contextSelector: null);
    }

    /// <summary>
    /// Verifies that the whitespace behind a <c>#region</c> without a description and behind a bare <c>#endregion</c> is kept
    /// by a node-level entry point
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatNodeInDocumentAsyncKeepsWhitespaceBehindBareRegionAndEndRegion()
    {
        const string input = "class C\n{\n    #region   \n\n    int a;\n\n    #endregion   \n}";

        await AssertNodeInDocument(input, input, root => FindType(root, "C"), contextSelector: null);
    }

    /// <summary>
    /// Verifies that a region inside a method body is kept by a node-level entry point that formats the method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatNodeInDocumentAsyncKeepsRegionInsideFormattedMethod()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     #region    setup

                                     var x=1;

                                     #endregion
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        #region    setup

                                        var x = 1;

                                        #endregion
                                    }
                                }
                                """;

        await AssertNodeInDocument(input, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First(), contextSelector: null);
    }

    /// <summary>
    /// Verifies that a region opened in front of a top-level type and closed inside it is kept as a whole by a node-level entry
    /// point that formats the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatNodeInDocumentAsyncKeepsBothHalvesOfRegionOpenedInFrontOfTopLevelType()
    {
        const string input = """
                             #region    bar
                             partial class C
                             {
                                 int a;

                                 #endregion
                             }
                             """;

        await AssertNodeInDocument(input, input, root => FindType(root, "C"), contextSelector: null);
    }

    /// <summary>
    /// Verifies that a region whose <c>#region</c> stands in the leading trivia of the formatted member and whose
    /// <c>#endregion</c> lies outside it is kept by a node-level entry point
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatNodeInDocumentAsyncKeepsRegionWhoseEndRegionLiesOutsideTheTarget()
    {
        const string input = """
                             class C
                             {
                                 #region    bar

                                 int M()
                                 {
                                     return 1;
                                 }

                                 #endregion
                             }
                             """;

        await AssertNodeInDocument(input, input, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First(), contextSelector: null);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> applies the region requirements that the node-level
    /// entry points skip
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public async Task FormatDocumentAsyncAppliesRegionRequirements()
    {
        const string input = "class C\n{\n    #region    fields   \n\n    int a;\n\n    #endregion\n}";
        const string expected = "class C\n{\n    #region Fields\n\n    int a;\n\n    #endregion // Fields\n}";

        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(input, endOfLine)));

                var result = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken);
                var text = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), text, $"Document output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> applies the region requirements that the node-level entry
    /// points skip
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    public void FormatSyntaxTreeAppliesRegionRequirements()
    {
        const string input = "class C\n{\n    #region    Bar   \n\n    int a;\n\n    #endregion\n}";
        const string expected = "class C\n{\n    #region Bar\n\n    int a;\n\n    #endregion // Bar\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);

            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), result.GetText(TestContext.CancellationToken).ToString(), $"Tree output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Finds the type declaration with the given name
    /// </summary>
    /// <param name="root">The root to search</param>
    /// <param name="name">The declared name</param>
    /// <returns>The type declaration</returns>
    private static SyntaxNode FindType(SyntaxNode root, string name)
    {
        return root.DescendantNodes()
                   .OfType<BaseTypeDeclarationSyntax>()
                   .First(type => type.Identifier.Text == name);
    }

    /// <summary>
    /// Formats the selected target through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>, or through
    /// <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> when a context selector is given, under every line
    /// ending, and verifies the document text after a first and a second pass
    /// </summary>
    /// <param name="input">The document source</param>
    /// <param name="expected">The expected document text with LF line endings</param>
    /// <param name="targetSelector">Selects the target node in a document root</param>
    /// <param name="contextSelector">Selects the context node in a document root, or <see langword="null"/> for no context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertNodeInDocument(string input, string expected, Func<SyntaxNode, SyntaxNode> targetSelector, Func<SyntaxNode, SyntaxNode> contextSelector)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var endingName = DescribeLineEnding(endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(input, endOfLine)));

                var firstPass = await FormatOnce(document, targetSelector, contextSelector);
                var firstText = (await firstPass.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(normalizedExpected, firstText, $"First pass mismatch under {endingName} line endings.");

                var secondPass = await FormatOnce(firstPass, targetSelector, contextSelector);
                var secondText = (await secondPass.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(normalizedExpected, secondText, $"Second pass mismatch under {endingName} line endings.");
            }
        }
    }

    /// <summary>
    /// Formats the selected target of the document once
    /// </summary>
    /// <param name="document">The document</param>
    /// <param name="targetSelector">Selects the target node in a document root</param>
    /// <param name="contextSelector">Selects the context node in a document root, or <see langword="null"/> for no context</param>
    /// <returns>The formatted document</returns>
    private async Task<Document> FormatOnce(Document document, Func<SyntaxNode, SyntaxNode> targetSelector, Func<SyntaxNode, SyntaxNode> contextSelector)
    {
        var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

        Assert.IsNotNull(root);

        var target = targetSelector(root);

        return contextSelector == null
                   ? await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken)
                   : await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, target, contextSelector(root), TestContext.CancellationToken);
    }

    #endregion // Methods
}