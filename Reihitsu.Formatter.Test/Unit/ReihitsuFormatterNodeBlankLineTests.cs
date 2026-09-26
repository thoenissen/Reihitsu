using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> decides the blank lines above a nested, line-starting
/// target from the token that precedes it in the document, even when a phase before the blank-line phase replaced the target
/// with a detached node
/// </summary>
[TestClass]
public class ReihitsuFormatterNodeBlankLineTests : FormatterTestsBase
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the blank line above a property is kept when the region phase rewrites a region inside the property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAbovePropertyWhenRegionInsideIsRewritten()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d;

                                 public string Description
                                 {
                                     set
                                     {
                                         #region assign

                                         _d = value;

                                         #endregion
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d;

                                    public string Description
                                    {
                                        set
                                        {
                                            #region Assign

                                            _d = value;

                                            #endregion // Assign
                                        }
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line above a property is kept when the documentation-comment phase rewrites a comment inside the property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAbovePropertyWhenDocumentationCommentInsideIsRewritten()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d;

                                 public string Description
                                 {
                                     set
                                     {
                                         /// <summary>Assigns the value</summary>
                                         _d = value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d;

                                    public string Description
                                    {
                                        set
                                        {
                                            /// <summary>
                                            /// Assigns the value
                                            /// </summary>
                                            _d = value;
                                        }
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line above a nested namespace is kept when the using-ordering phase rebuilds its already ordered usings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAboveNamespaceWhenOrderedUsingsAreRebuilt()
    {
        const string source = """
                              namespace Outer
                              {
                                  internal class A
                                  {
                                      private int _a;
                                  }

                                  namespace Inner
                                  {
                                      using System.IO;
                                      using System.Text;

                                      internal class B
                                      {
                                          private int _b;
                                      }
                                  }
                              }
                              """;

        await AssertFormatsTarget(source, source, root => root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single(namespaceDeclaration => namespaceDeclaration.Name.ToString() == "Inner"));
    }

    /// <summary>
    /// Verifies that two blank lines above a structurally rewritten property collapse to exactly one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task CollapsesTwoBlankLinesAboveRewrittenPropertyToOne()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d;


                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             _d = value;
                                     }
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, BracedPropertyAfterField("\n"), SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line above a line comment that precedes a structurally rewritten property is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAboveCommentOfRewrittenProperty()
    {
        await AssertFormatsTarget(UnbracedPropertyAfterField("\n    // Describes the instance\n"),
                                  BracedPropertyAfterField("\n    // Describes the instance\n"),
                                  SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line above a region directive that precedes a structurally rewritten property is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAboveRegionOfRewrittenProperty()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d;

                                 #region Properties

                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             _d = value;
                                     }
                                 }

                                 #endregion // Properties
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d;

                                    #region Properties

                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                            {
                                                _d = value;
                                            }
                                        }
                                    }

                                    #endregion // Properties
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line above a documentation comment of a structurally rewritten property is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAboveDocumentationCommentOfRewrittenProperty()
    {
        const string documentation = "\n    /// <summary>\n    /// The description\n    /// </summary>\n";

        await AssertFormatsTarget(UnbracedPropertyAfterField(documentation),
                                  BracedPropertyAfterField(documentation),
                                  SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line between an opening brace and a structurally rewritten first member is removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task RemovesBlankLineBetweenOpeningBraceAndRewrittenFirstMember()
    {
        const string input = """
                             public class TestClass
                             {

                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             Value = value;
                                     }
                                 }

                                 public string Value { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                            {
                                                Value = value;
                                            }
                                        }
                                    }

                                    public string Value { get; set; }
                                }
                                """;

        await AssertFormatsTarget(input, expected, root => root.DescendantNodes().OfType<PropertyDeclarationSyntax>().First());
    }

    /// <summary>
    /// Verifies that the blank line between an opening brace and the documentation comment of a structurally rewritten first member is removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task RemovesBlankLineBetweenOpeningBraceAndDocumentationCommentOfRewrittenFirstMember()
    {
        const string input = """
                             public class TestClass
                             {

                                 /// <summary>
                                 /// The description
                                 /// </summary>
                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             Value = value;
                                     }
                                 }

                                 public string Value { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    /// <summary>
                                    /// The description
                                    /// </summary>
                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                            {
                                                Value = value;
                                            }
                                        }
                                    }

                                    public string Value { get; set; }
                                }
                                """;

        await AssertFormatsTarget(input, expected, root => root.DescendantNodes().OfType<PropertyDeclarationSyntax>().First());
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a structurally rewritten property that directly follows its sibling
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveRewrittenPropertyWithoutOne()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d;
                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             _d = value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d;
                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                            {
                                                _d = value;
                                            }
                                        }
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line above a statement whose body a structural transform braces is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAboveRewrittenStatement()
    {
        const string input = """
                             public class TestClass
                             {
                                 public int Method(int value)
                                 {
                                     var copy = value;

                                     if (copy > 0)
                                         copy++;

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public int Method(int value)
                                    {
                                        var copy = value;

                                        if (copy > 0)
                                        {
                                            copy++;
                                        }

                                        return copy;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<IfStatementSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line between a file-scoped namespace and a type that a structural transform rewrites is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAboveRewrittenTypeInFileScopedNamespace()
    {
        const string input = """
                             namespace Sample;

                             internal class Empty
                             {
                             }
                             """;
        const string expected = """
                                namespace Sample;

                                internal class Empty;
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<ClassDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line before the opening brace of a rewritten block target is removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task RemovesBlankLineBeforeOpeningBraceOfRewrittenBlock()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void Method(int value)

                                 {
                                     if (value > 0)
                                         value++;
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void Method(int value)
                                    {
                                        if (value > 0)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body);
    }

    /// <summary>
    /// Verifies that blank lines at the start of the file above a rewritten top-level statement are still removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task RemovesBlankLineAtStartOfFileAboveRewrittenTopLevelStatement()
    {
        const string input = """

                             if (args.Length > 0)
                                 System.Console.WriteLine();
                             """;
        const string expected = """
                                if (args.Length > 0)
                                {
                                    System.Console.WriteLine();
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<IfStatementSyntax>);
    }

    /// <summary>
    /// Verifies that a blank line is inserted above a line comment that directly follows the previous member when a structural
    /// transform rewrites the property the comment belongs to
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task InsertsBlankLineAboveCommentOfRewrittenProperty()
    {
        await AssertFormatsTarget(UnbracedPropertyAfterField("    // Describes the instance\n"),
                                  BracedPropertyAfterField("\n    // Describes the instance\n"),
                                  SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that a line comment below a multi-line block comment behind the previous member is measured the same way
    /// whether or not a structural transform rewrites the property the line comment belongs to
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task MeasuresCommentGapAfterMultiLineBlockCommentAlikeForRewrittenAndUnchangedProperty()
    {
        const string separator = "    /* first\n    second */\n    // Describes the instance\n";
        var expected = BracedPropertyAfterField(separator).Replace("private string _d;\n    /* first", "private string _d; /* first");

        await AssertFormatsTarget(UnbracedPropertyAfterField(separator).Replace("private string _d;\n    /* first", "private string _d; /* first"),
                                  expected,
                                  SelectSingle<PropertyDeclarationSyntax>);
        await AssertFormatsTarget(expected, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a line comment that directly follows the opening brace when a structural
    /// transform rewrites the member the comment belongs to
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveCommentOfRewrittenFirstMember()
    {
        const string input = """
                             public class TestClass
                             {
                                 // Describes the instance
                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             Value = value;
                                     }
                                 }

                                 public string Value { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    // Describes the instance
                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                            {
                                                Value = value;
                                            }
                                        }
                                    }

                                    public string Value { get; set; }
                                }
                                """;

        await AssertFormatsTarget(input, expected, root => root.DescendantNodes().OfType<PropertyDeclarationSyntax>().First());
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a line comment that directly follows a switch label when a structural
    /// transform rewrites the statement the comment belongs to
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveCommentOfRewrittenStatementAfterSwitchLabel()
    {
        const string input = """
                             public class TestClass
                             {
                                 public int Method(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             // Handles one
                                             if (value > 0)
                                                 value++;

                                             break;
                                     }

                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public int Method(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                // Handles one
                                                if (value > 0)
                                                {
                                                    value++;
                                                }

                                                break;
                                        }

                                        return value;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<IfStatementSyntax>);
    }

    /// <summary>
    /// Verifies that a blank line is inserted above a region directive that directly follows the previous member when a
    /// structural transform rewrites the property the directive precedes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task InsertsBlankLineAboveRegionOfRewrittenProperty()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d;
                                 #region Properties

                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             _d = value;
                                     }
                                 }

                                 #endregion // Properties
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d;

                                    #region Properties

                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                            {
                                                _d = value;
                                            }
                                        }
                                    }

                                    #endregion // Properties
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a region directive that directly follows the opening brace when a
    /// structural transform rewrites the property the directive precedes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveRegionOfRewrittenFirstMember()
    {
        const string input = """
                             public class TestClass
                             {
                                 #region Properties

                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             Value = value;
                                     }
                                 }

                                 public string Value { get; set; }

                                 #endregion // Properties
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    #region Properties

                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                            {
                                                Value = value;
                                            }
                                        }
                                    }

                                    public string Value { get; set; }

                                    #endregion // Properties
                                }
                                """;

        await AssertFormatsTarget(input, expected, root => root.DescendantNodes().OfType<PropertyDeclarationSyntax>().First());
    }

    /// <summary>
    /// Verifies that a blank line is inserted above a region directive that follows an opening brace carrying a trailing
    /// comment when a structural transform rewrites the property the directive precedes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task InsertsBlankLineAboveRegionOfRewrittenFirstMemberAfterCommentedOpeningBrace()
    {
        const string input = """
                             public class TestClass
                             { // Members
                                 #region Properties

                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             Value = value;
                                     }
                                 }

                                 public string Value { get; set; }

                                 #endregion // Properties
                             }
                             """;
        const string expected = """
                                public class TestClass
                                { // Members

                                    #region Properties

                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                            {
                                                Value = value;
                                            }
                                        }
                                    }

                                    public string Value { get; set; }

                                    #endregion // Properties
                                }
                                """;

        await AssertFormatsTarget(input, expected, root => root.DescendantNodes().OfType<PropertyDeclarationSyntax>().First());
    }

    /// <summary>
    /// Verifies that a blank line is inserted above an end region directive that directly follows the previous member when a
    /// structural transform rewrites the property the directive precedes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task InsertsBlankLineAboveEndRegionOfRewrittenProperty()
    {
        const string input = """
                             public class TestClass
                             {
                                 #region Fields

                                 private string _d;
                                 #endregion // Fields

                                 public string Description
                                 {
                                     set
                                     {
                                         if (value != null)
                                             _d = value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    #region Fields

                                    private string _d;

                                    #endregion // Fields

                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                            {
                                                _d = value;
                                            }
                                        }
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that a blank line is inserted above a line comment between a file-scoped namespace and a type that a structural
    /// transform rewrites
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task InsertsBlankLineAboveCommentOfRewrittenTypeInFileScopedNamespace()
    {
        const string input = """
                             namespace Sample;
                             // Describes the type
                             internal class Empty
                             {
                             }
                             """;
        const string expected = """
                                namespace Sample;

                                // Describes the type
                                internal class Empty;
                                """;

        await AssertFormatsTarget(input, expected, SelectSingle<ClassDeclarationSyntax>);
    }

    /// <summary>
    /// Selects the single node of the given type below the root
    /// </summary>
    /// <typeparam name="TNode">The node type</typeparam>
    /// <param name="root">The document root</param>
    /// <returns>The single matching node</returns>
    private static SyntaxNode SelectSingle<TNode>(SyntaxNode root)
        where TNode : SyntaxNode
    {
        return root.DescendantNodes().OfType<TNode>().Single();
    }

    /// <summary>
    /// Builds a class with a field, a blank line, the given text, and a property whose setter contains an unbraced <c>if</c>
    /// </summary>
    /// <param name="separator">The text between the field line and the property line, starting with the blank line</param>
    /// <returns>The source text with LF line endings</returns>
    private static string UnbracedPropertyAfterField(string separator)
    {
        return $"public class TestClass\n{{\n    private string _d;\n{separator}    public string Description\n    {{\n        set\n        {{\n            if (value != null)\n                _d = value;\n        }}\n    }}\n}}";
    }

    /// <summary>
    /// Builds the formatted counterpart of <see cref="UnbracedPropertyAfterField"/>
    /// </summary>
    /// <param name="separator">The text between the field line and the property line, starting with the blank line</param>
    /// <returns>The source text with LF line endings</returns>
    private static string BracedPropertyAfterField(string separator)
    {
        return $"public class TestClass\n{{\n    private string _d;\n{separator}    public string Description\n    {{\n        set\n        {{\n            if (value != null)\n            {{\n                _d = value;\n            }}\n        }}\n    }}\n}}";
    }

    /// <summary>
    /// Formats the selected target of the given source through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>
    /// </summary>
    /// <param name="source">The source text</param>
    /// <param name="selectTarget">Selects the target node from the document root</param>
    /// <returns>The resulting document text</returns>
    private async Task<string> FormatTarget(string source, Func<SyntaxNode, SyntaxNode> selectTarget)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(source));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

            Assert.IsNotNull(root, "Expected a syntax root in the test document.");

            var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, selectTarget(root), TestContext.CancellationToken);

            return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
        }
    }

    /// <summary>
    /// Formats the selected target under every line ending, asserts the expected text, and asserts that formatting the
    /// same target of the result a second time changes nothing
    /// </summary>
    /// <param name="inputWithLf">The input with LF line endings</param>
    /// <param name="expectedWithLf">The expected output with LF line endings</param>
    /// <param name="selectTarget">Selects the target node from the document root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTarget(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectTarget)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = NormalizeLineEndings(inputWithLf, endOfLine);
            var expected = NormalizeLineEndings(expectedWithLf, endOfLine);

            var firstPass = await FormatTarget(input, selectTarget);

            Assert.AreEqual(expected, firstPass, $"Unexpected first pass under {DescribeLineEnding(endOfLine)} line endings.");
            AssertUsesLineEnding(firstPass, endOfLine);

            var secondPass = await FormatTarget(firstPass, selectTarget);

            Assert.AreEqual(firstPass, secondPass, $"The second pass must not change the output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    #endregion // Methods
}