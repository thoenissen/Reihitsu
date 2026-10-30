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

    /// <summary>
    /// The property of the FMT-PIPE-008 example, whose accessor block only C# 6 can write as an expression body
    /// </summary>
    private const string SpecPropertyInput = """
                                             class C
                                             {
                                                 int P { get { return 1; } }
                                             }
                                             """;

    /// <summary>
    /// The empty class of the node-level FMT-PIPE-008 example, which C# 12 writes with a semicolon body
    /// </summary>
    private const string SpecEmptyClassInput = """
                                               class Empty
                                               {
                                               }
                                               """;

    #endregion // Constants

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
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> keeps the accessor blocks of a property with a getter and a
    /// setter below C# 7 even after an earlier rewrite
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
                                     set
                                     {
                                         _x = value;
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
                                        set
                                        {
                                            _x = value;
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
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> keeps the accessor list of a get-only property below C# 6,
    /// which introduced expression-bodied properties
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public void FormatSyntaxTreeKeepsGetOnlyAccessorListBelowCSharp6()
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
                             }
                             """;

        AssertFormatSyntaxTree(input, input, LanguageVersion.CSharp5);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> converts a get-only property to an expression-bodied
    /// property in C# 6
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public void FormatSyntaxTreeConvertsGetOnlyPropertyInCSharp6()
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
                             }
                             """;
        const string expected = """
                                public class B
                                {
                                    private int _x;

                                    public int X => _x;
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
    [Requirement("FMT-PIPE-008")]
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
    [Requirement("FMT-PIPE-008")]
    public async Task FormatNodeInDocumentAsyncKeepsNestedEmptyClassBracedInCSharp73Project()
    {
        await AssertFormatNodeInDocument(LanguageVersion.CSharp7_3, NestedBracedExpected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> still converts an empty nested class in a C# 12 project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
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
    [Requirement("FMT-PIPE-008")]
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
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> still converts an empty class in a
    /// C# 12 project when its context node holds an earlier rewrite
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public async Task FormatNodeInDocumentWithContextAsyncConvertsEmptyClassInCSharp12Project()
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(TopLevelInput, endOfLine), LanguageVersion.CSharp12);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var target = root?.DescendantNodes().OfType<ClassDeclarationSyntax>().First(declaration => declaration.Identifier.Text == "A");

                Assert.IsNotNull(target);

                var result = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, target, root, TestContext.CancellationToken);
                var resultText = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.StartsWith(NormalizeLineEndings("public class A;\n", endOfLine), resultText, $"The empty class must convert under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> follows the language version of the node's syntax tree
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
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
    [Requirement("FMT-PIPE-008")]
    public void FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion()
    {
        var detachedRoot = SyntaxFactory.ParseCompilationUnit(TopLevelInput);

        var result = ReihitsuFormatter.FormatNode(detachedRoot, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(TopLevelSemicolonExpected, result.ToFullString());
    }

    /// <summary>
    /// Verifies the spec example through <see cref="ReihitsuFormatter.FormatDocumentAsync"/>: a C# 5 project keeps the accessor
    /// blocks of a get-only property, which only C# 6 can write as an expression body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public async Task FormatDocumentAsyncKeepsGetOnlyAccessorListInCSharp5Project()
    {
        const string expected = """
                                class C
                                {
                                    int P
                                    {
                                        get
                                        {
                                            return 1;
                                        }
                                    }
                                }
                                """;

        await AssertFormatDocument(SpecPropertyInput, expected, LanguageVersion.CSharp5);
    }

    /// <summary>
    /// Verifies the spec example through <see cref="ReihitsuFormatter.FormatDocumentAsync"/>: a C# 6 project converts the get-only
    /// property to an expression body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public async Task FormatDocumentAsyncConvertsGetOnlyPropertyInCSharp6Project()
    {
        const string expected = """
                                class C
                                {
                                    int P => 1;
                                }
                                """;

        await AssertFormatDocument(SpecPropertyInput, expected, LanguageVersion.CSharp6);
    }

    /// <summary>
    /// Verifies the spec example through <see cref="ReihitsuFormatter.FormatSyntaxTree"/>: a tree parsed as C# 5 keeps the accessor
    /// blocks of a get-only property and a tree parsed as C# 6 converts it to an expression body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public void FormatSyntaxTreeFollowsTheTreeVersionForTheSpecPropertyExample()
    {
        const string bracedExpected = """
                                      class C
                                      {
                                          int P
                                          {
                                              get
                                              {
                                                  return 1;
                                              }
                                          }
                                      }
                                      """;
        const string expressionExpected = """
                                          class C
                                          {
                                              int P => 1;
                                          }
                                          """;

        AssertFormatSyntaxTree(SpecPropertyInput, bracedExpected, LanguageVersion.CSharp5);
        AssertFormatSyntaxTree(SpecPropertyInput, expressionExpected, LanguageVersion.CSharp6);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> converts an empty class in a C# 12 project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public async Task FormatDocumentAsyncConvertsEmptyClassInCSharp12Project()
    {
        await AssertFormatDocument(TopLevelInput, TopLevelSemicolonExpected, LanguageVersion.CSharp12);
    }

    /// <summary>
    /// Verifies the node-level spec example: <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> keeps an empty top-level
    /// class braced in a C# 11 project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public async Task FormatNodeInDocumentAsyncKeepsSpecEmptyClassBracedInCSharp11Project()
    {
        await AssertFormatEmptyClassInDocument(LanguageVersion.CSharp11, false, SpecEmptyClassInput);
    }

    /// <summary>
    /// Verifies the node-level spec example: <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> converts an empty top-level
    /// class in a C# 12 project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public async Task FormatNodeInDocumentAsyncConvertsSpecEmptyClassInCSharp12Project()
    {
        await AssertFormatEmptyClassInDocument(LanguageVersion.CSharp12, false, "class Empty;");
    }

    /// <summary>
    /// Verifies the node-level spec example: <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> with the
    /// compilation unit as context keeps an empty class braced in a C# 11 project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public async Task FormatNodeInDocumentWithContextAsyncKeepsSpecEmptyClassBracedInCSharp11Project()
    {
        await AssertFormatEmptyClassInDocument(LanguageVersion.CSharp11, true, SpecEmptyClassInput);
    }

    /// <summary>
    /// Verifies the node-level spec example: <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> with the
    /// compilation unit as context converts an empty class in a C# 12 project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public async Task FormatNodeInDocumentWithContextAsyncConvertsSpecEmptyClassInCSharp12Project()
    {
        await AssertFormatEmptyClassInDocument(LanguageVersion.CSharp12, true, "class Empty;");
    }

    /// <summary>
    /// Verifies the node-level spec example: <see cref="ReihitsuFormatter.FormatNode"/> keeps an empty class braced when its
    /// tree is parsed as C# 11 and converts it when its tree is parsed as C# 12
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public void FormatNodeFollowsTheTreeVersionForTheSpecEmptyClass()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var source = NormalizeLineEndings(SpecEmptyClassInput, endOfLine);
            var csharp11Tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11), cancellationToken: TestContext.CancellationToken);
            var csharp12Tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12), cancellationToken: TestContext.CancellationToken);
            var csharp11Target = csharp11Tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            var csharp12Target = csharp12Tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

            var csharp11Result = ReihitsuFormatter.FormatNode(csharp11Target, cancellationToken: TestContext.CancellationToken);
            var csharp12Result = ReihitsuFormatter.FormatNode(csharp12Target, cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(source, csharp11Result.ToFullString(), $"The C# 11 class must stay braced under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.AreEqual("class Empty;", csharp12Result.ToFullString(), $"The C# 12 class must convert under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> converts an empty class whose tree is parsed as C# 12
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public void FormatNodeConvertsEmptyClassForCSharp12Tree()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(TopLevelInput, endOfLine),
                                                  CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12),
                                                  cancellationToken: TestContext.CancellationToken);

            var result = ReihitsuFormatter.FormatNode(tree.GetRoot(TestContext.CancellationToken), cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(NormalizeLineEndings(TopLevelSemicolonExpected, endOfLine), result.ToFullString(), $"FormatNode output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a node parsed with C# 11 parse options, which has no tree of its own, keeps those parse options:
    /// <see cref="ReihitsuFormatter.FormatNode"/> leaves the empty class braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public void FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var source = NormalizeLineEndings(SpecEmptyClassInput, endOfLine);
            var detachedRoot = SyntaxFactory.ParseCompilationUnit(source, options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11));

            var result = ReihitsuFormatter.FormatNode(detachedRoot, cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(source, result.ToFullString(), $"The detached C# 11 node must stay braced under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a member declaration parsed with parse options, which has no tree of its own, keeps those parse
    /// options: <see cref="ReihitsuFormatter.FormatNode"/> leaves the empty class braced for C# 11 and converts it for C# 12
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public void FormatNodeFollowsTheParseOptionsOfDetachedMemberDeclaration()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var source = NormalizeLineEndings(SpecEmptyClassInput, endOfLine);
            var csharp11Member = SyntaxFactory.ParseMemberDeclaration(source, options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11));
            var csharp12Member = SyntaxFactory.ParseMemberDeclaration(source, options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12));

            Assert.IsNotNull(csharp11Member);
            Assert.IsNotNull(csharp12Member);

            var csharp11Result = ReihitsuFormatter.FormatNode(csharp11Member, cancellationToken: TestContext.CancellationToken);
            var csharp12Result = ReihitsuFormatter.FormatNode(csharp12Member, cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(source, csharp11Result.ToFullString(), $"The detached C# 11 member must stay braced under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.AreEqual("class Empty;", csharp12Result.ToFullString(), $"The detached C# 12 member must convert under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a node built with <see cref="SyntaxFactory"/>, without parse options, belongs to a tree with the
    /// default parse options: <see cref="ReihitsuFormatter.FormatNode"/> converts the empty class for the newest supported version
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-008")]
    public void FormatNodeFormatsNodeBuiltWithSyntaxFactoryForTheNewestSupportedVersion()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var builtRoot = SyntaxFactory.CompilationUnit()
                                         .AddMembers(SyntaxFactory.ClassDeclaration("Empty"))
                                         .NormalizeWhitespace(eol: endOfLine);

            Assert.AreEqual(NormalizeLineEndings(SpecEmptyClassInput, endOfLine), builtRoot.ToFullString(), $"The built node must match the spec input under {DescribeLineEnding(endOfLine)} line endings.");

            var result = ReihitsuFormatter.FormatNode(builtRoot, cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual("class Empty;", result.ToFullString(), $"The built node must convert under {DescribeLineEnding(endOfLine)} line endings.");
        }
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
    /// Formats the source through <see cref="ReihitsuFormatter.FormatDocumentAsync"/> in a project at the given language
    /// version under every line ending
    /// </summary>
    /// <param name="input">The input source</param>
    /// <param name="expected">The expected document text</param>
    /// <param name="languageVersion">The project's language version</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatDocument(string input, string expected, LanguageVersion languageVersion)
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(input, endOfLine), languageVersion);

                var result = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken);

                await AssertDocumentText(result, NormalizeLineEndings(expected, endOfLine), endOfLine);
            }
        }
    }

    /// <summary>
    /// Formats the class of <see cref="SpecEmptyClassInput"/> through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>,
    /// or through <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> with the compilation unit as context, in
    /// a project at the given language version under every line ending
    /// </summary>
    /// <param name="languageVersion">The project's language version</param>
    /// <param name="withContext">Whether to format with the compilation unit as context</param>
    /// <param name="expected">The expected document text</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatEmptyClassInDocument(LanguageVersion languageVersion, bool withContext, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(SpecEmptyClassInput, endOfLine), languageVersion);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var target = root?.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

                Assert.IsNotNull(target);

                var result = withContext
                                 ? await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, target, root, TestContext.CancellationToken)
                                 : await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken);

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