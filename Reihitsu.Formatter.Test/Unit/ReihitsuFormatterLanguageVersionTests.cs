using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that every <see cref="ReihitsuFormatter"/> entry point formats for the language version of its input, even after
/// an earlier structural rewrite replaced the tree
/// </summary>
[TestClass]
public class ReihitsuFormatterLanguageVersionTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// An empty class next to an unbraced <c>if</c>, whose brace transform rewrites the tree before the empty-type transform runs
    /// </summary>
    private const string TopLevelInput = """
                                         public class A { }

                                         public class B
                                         {
                                             public void M(int x)
                                             {
                                                 if (x > 0)
                                                     x = 0;
                                             }
                                         }
                                         """;

    /// <summary>
    /// The formatted form of <see cref="TopLevelInput"/> below C# 12
    /// </summary>
    private const string TopLevelBracedExpected = """
                                                  public class A
                                                  {
                                                  }

                                                  public class B
                                                  {
                                                      public void M(int x)
                                                      {
                                                          if (x > 0)
                                                          {
                                                              x = 0;
                                                          }
                                                      }
                                                  }
                                                  """;

    /// <summary>
    /// The formatted form of <see cref="TopLevelInput"/> from C# 12 on
    /// </summary>
    private const string TopLevelSemicolonExpected = """
                                                     public class A;

                                                     public class B
                                                     {
                                                         public void M(int x)
                                                         {
                                                             if (x > 0)
                                                             {
                                                                 x = 0;
                                                             }
                                                         }
                                                     }
                                                     """;

    /// <summary>
    /// A containing class holding an empty nested class and an unbraced <c>if</c>, as a code fix would format it
    /// </summary>
    private const string NestedInput = """
                                       public class B
                                       {
                                           public class A { }

                                           public void M(int x)
                                           {
                                               if (x > 0)
                                                   x = 0;
                                           }
                                       }
                                       """;

    /// <summary>
    /// The formatted form of <see cref="NestedInput"/> below C# 12
    /// </summary>
    private const string NestedBracedExpected = """
                                                public class B
                                                {
                                                    public class A
                                                    {
                                                    }

                                                    public void M(int x)
                                                    {
                                                        if (x > 0)
                                                        {
                                                            x = 0;
                                                        }
                                                    }
                                                }
                                                """;

    /// <summary>
    /// The formatted form of <see cref="NestedInput"/> from C# 12 on
    /// </summary>
    private const string NestedSemicolonExpected = """
                                                   public class B
                                                   {
                                                       public class A;

                                                       public void M(int x)
                                                       {
                                                           if (x > 0)
                                                           {
                                                               x = 0;
                                                           }
                                                       }
                                                   }
                                                   """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> keeps an empty class braced below C# 12 after an
    /// earlier rewrite, and that a second pass is stable
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeKeepsEmptyClassBracedBelowCSharp12AfterEarlierTransform()
    {
        AssertFormatSyntaxTree(TopLevelInput, TopLevelBracedExpected, LanguageVersion.CSharp11);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> still converts an empty class from C# 12 on after an earlier rewrite
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeConvertsEmptyClassFromCSharp12AfterEarlierTransform()
    {
        AssertFormatSyntaxTree(TopLevelInput, TopLevelSemicolonExpected, LanguageVersion.CSharp12);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> keeps accessor blocks below C# 7 even after an earlier rewrite
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeKeepsAccessorBlockBelowCSharp7AfterEarlierTransform()
    {
        const string input = """
                             public class B
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         return _x;
                                     }
                                 }

                                 public void M(int x)
                                 {
                                     if (x > 0)
                                         x = 0;
                                 }
                             }
                             """;
        const string expected = """
                                public class B
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get
                                        {
                                            return _x;
                                        }
                                    }

                                    public void M(int x)
                                    {
                                        if (x > 0)
                                        {
                                            x = 0;
                                        }
                                    }
                                }
                                """;

        AssertFormatSyntaxTree(input, expected, LanguageVersion.CSharp6);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> returns a tree that keeps the input's parse options,
    /// including a symbolic language version the formatter itself clamps
    /// </summary>
    [TestMethod]
    public void FormatSyntaxTreeKeepsTheInputParseOptions()
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var tree = CSharpSyntaxTree.ParseText(TopLevelInput, parseOptions, cancellationToken: TestContext.CancellationToken);

        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(parseOptions, formattedTree.Options);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> keeps an empty class braced in a C# 11 project after an earlier rewrite
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task FormatDocumentAsyncKeepsEmptyClassBracedInCSharp11Project()
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(TopLevelInput, endOfLine), LanguageVersion.CSharp11);

                var result = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken);

                await AssertDocumentText(result, NormalizeLineEndings(TopLevelBracedExpected, endOfLine), endOfLine);
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> keeps an empty nested class braced in a
    /// C# 7.3 project when the formatted target also holds an earlier rewrite
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task FormatNodeInDocumentAsyncKeepsNestedEmptyClassBracedInCSharp73Project()
    {
        await AssertFormatNodeInDocument(LanguageVersion.CSharp7_3, NestedBracedExpected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> still converts an empty nested class in a C# 12 project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task FormatNodeInDocumentAsyncConvertsNestedEmptyClassInCSharp12Project()
    {
        await AssertFormatNodeInDocument(LanguageVersion.CSharp12, NestedSemicolonExpected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> keeps an empty class braced in a
    /// C# 11 project when its context node holds an earlier rewrite
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task FormatNodeInDocumentWithContextAsyncKeepsEmptyClassBracedInCSharp11Project()
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(TopLevelInput, endOfLine), LanguageVersion.CSharp11);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var target = root?.DescendantNodes().OfType<ClassDeclarationSyntax>().First(declaration => declaration.Identifier.Text == "A");

                Assert.IsNotNull(target);

                var result = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, target, root, TestContext.CancellationToken);
                var resultText = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.StartsWith(NormalizeLineEndings("public class A\n{\n}\n", endOfLine), resultText, $"The empty class must stay braced under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> follows the language version of the node's syntax tree
    /// </summary>
    [TestMethod]
    public void FormatNodeKeepsEmptyClassBracedForCSharp11Tree()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(TopLevelInput, endOfLine),
                                                  CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11),
                                                  cancellationToken: TestContext.CancellationToken);

            var result = ReihitsuFormatter.FormatNode(tree.GetRoot(TestContext.CancellationToken), cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(NormalizeLineEndings(TopLevelBracedExpected, endOfLine), result.ToFullString(), $"FormatNode output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> formats a detached node, which carries the default parse
    /// options, for the newest supported language version
    /// </summary>
    [TestMethod]
    public void FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion()
    {
        var detachedRoot = SyntaxFactory.ParseCompilationUnit(TopLevelInput);

        var result = ReihitsuFormatter.FormatNode(detachedRoot, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(TopLevelSemicolonExpected, result.ToFullString());
    }

    /// <summary>
    /// Creates a document in a project that parses at the given language version
    /// </summary>
    /// <param name="workspace">The workspace hosting the project</param>
    /// <param name="source">The document source</param>
    /// <param name="languageVersion">The project's language version</param>
    /// <returns>The created document</returns>
    private static Document CreateDocument(AdhocWorkspace workspace, string source, LanguageVersion languageVersion)
    {
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp)
                               .WithParseOptions(CSharpParseOptions.Default.WithLanguageVersion(languageVersion));

        return project.AddDocument("Test.cs", SourceText.From(source));
    }

    /// <summary>
    /// Formats the source through <see cref="ReihitsuFormatter.FormatSyntaxTree"/> at the given language version under
    /// every line ending, and asserts the expected output and a stable second pass
    /// </summary>
    /// <param name="input">The input source</param>
    /// <param name="expected">The expected formatted output</param>
    /// <param name="languageVersion">The language version to parse with</param>
    private void AssertFormatSyntaxTree(string input, string expected, LanguageVersion languageVersion)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), parseOptions, cancellationToken: TestContext.CancellationToken);

            var actual = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).GetRoot(TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(normalizedExpected, actual, $"FormatSyntaxTree output mismatch under {DescribeLineEnding(endOfLine)} line endings.");

            var secondTree = CSharpSyntaxTree.ParseText(actual, parseOptions, cancellationToken: TestContext.CancellationToken);
            var secondPass = ReihitsuFormatter.FormatSyntaxTree(secondTree, TestContext.CancellationToken).GetRoot(TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(normalizedExpected, secondPass, $"FormatSyntaxTree is not idempotent under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Formats the containing class of <see cref="NestedInput"/> through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>
    /// in a project at the given language version under every line ending
    /// </summary>
    /// <param name="languageVersion">The project's language version</param>
    /// <param name="expected">The expected document text</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatNodeInDocument(LanguageVersion languageVersion, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(NestedInput, endOfLine), languageVersion);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var target = root?.DescendantNodes().OfType<ClassDeclarationSyntax>().First(declaration => declaration.Identifier.Text == "B");

                Assert.IsNotNull(target);

                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken);

                await AssertDocumentText(result, NormalizeLineEndings(expected, endOfLine), endOfLine);
            }
        }
    }

    /// <summary>
    /// Asserts the text of a formatted document
    /// </summary>
    /// <param name="document">The formatted document</param>
    /// <param name="expected">The expected text</param>
    /// <param name="endOfLine">The line ending the text uses</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertDocumentText(Document document, string expected, string endOfLine)
    {
        var text = (await document.GetTextAsync(TestContext.CancellationToken)).ToString();

        Assert.AreEqual(expected, text, $"Document output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
    }

    #endregion // Methods
}