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
/// Tests that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> writes back the leading trivia of its target as it
/// stands in the document — its blank lines, comments, directives, and documentation comments, also one written behind the
/// code in front of the target — whatever the token in front of the target or the sibling in front of it in its list, both
/// for an unchanged target and when a phase before the blank-line phase replaced the target with a detached node
/// </summary>
[TestClass]
public class ReihitsuFormatterNodeBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the blank line above a property is kept when the property holds a region, whose directives keep their text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAbovePropertyHoldingARegion()
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
                                            #region assign

                                            _d = value;

                                            #endregion
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
    /// Verifies that node-level formatting keeps two blank lines above rewritten property, because the entry point
    /// writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsTwoBlankLinesAboveRewrittenProperty()
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

        await AssertFormatsTarget(input, BracedPropertyAfterField("\n\n"), SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps two blank lines above rewritten property holding a foreign line break,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsTwoBlankLinesAboveRewrittenPropertyHoldingAForeignLineBreak()
    {
        await AssertFormatsTargetWithForeignLineBreaks(UnbracedPropertyAfterField("\n\n").Replace("if (value != null)\n", "if (value != null)¶"),
                                                       BracedPropertyAfterField("\n\n"),
                                                       SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the blank line above a structurally rewritten property whose body holds a line break other than the
    /// document's line ending is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAboveRewrittenPropertyHoldingAForeignLineBreak()
    {
        await AssertFormatsTargetWithForeignLineBreaks(UnbracedPropertyAfterField("\n").Replace("if (value != null)\n", "if (value != null)¶"),
                                                       BracedPropertyAfterField("\n"),
                                                       SelectSingle<PropertyDeclarationSyntax>);
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
    /// Verifies that node-level formatting keeps blank line between opening brace and rewritten first member, because
    /// the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsBlankLineBetweenOpeningBraceAndRewrittenFirstMember()
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
    /// Verifies that node-level formatting keeps blank line between opening brace and documentation comment of
    /// rewritten first member, because the entry point writes back the target's leading trivia as it stands in the
    /// document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsBlankLineBetweenOpeningBraceAndDocumentationCommentOfRewrittenFirstMember()
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
    /// Verifies that node-level formatting keeps blank line before opening brace of rewritten block, because the entry
    /// point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsBlankLineBeforeOpeningBraceOfRewrittenBlock()
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
    /// Verifies that node-level formatting keeps blank line at start of file above rewritten top level statement,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsBlankLineAtStartOfFileAboveRewrittenTopLevelStatement()
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
    /// Verifies that node-level formatting keeps comment directly below previous member of rewritten property, because
    /// the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsCommentDirectlyBelowPreviousMemberOfRewrittenProperty()
    {
        await AssertFormatsTarget(UnbracedPropertyAfterField("    // Describes the instance\n"),
                                  BracedPropertyAfterField("    // Describes the instance\n"),
                                  SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps comment directly below multi line block comment for rewritten and
    /// unchanged property, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsCommentDirectlyBelowMultiLineBlockCommentForRewrittenAndUnchangedProperty()
    {
        const string separator = "    /* first\n    second */\n    // Describes the instance\n";
        var expected = BracedPropertyAfterField(separator).Replace("private string _d;\n    /* first", "private string _d; /* first");

        await AssertFormatsTarget(UnbracedPropertyAfterField(separator).Replace("private string _d;\n    /* first", "private string _d; /* first"),
                                  expected,
                                  SelectSingle<PropertyDeclarationSyntax>);
        await AssertFormatsTarget(BracedPropertyAfterField(separator).Replace("private string _d;\n    /* first", "private string _d; /* first"),
                                  expected,
                                  SelectSingle<PropertyDeclarationSyntax>);
        await AssertFormatsTarget(expected, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation behind previous member for rewritten and
    /// unchanged property, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationBehindPreviousMemberForRewrittenAndUnchangedProperty()
    {
        var expected = BracedPropertyAfterField("    /** doc */\n").Replace("private string _d;\n    /** doc */", "private string _d; /** doc */");

        await AssertFormatsTarget(UnbracedPropertyAfterField("    /** doc */\n").Replace("private string _d;\n    /** doc */", "private string _d; /** doc */"),
                                  expected,
                                  SelectSingle<PropertyDeclarationSyntax>);
        await AssertFormatsTarget(BracedPropertyAfterField("    /** doc */\n").Replace("private string _d;\n    /** doc */", "private string _d; /** doc */"),
                                  expected,
                                  SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation behind previous member for unchanged property,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationBehindPreviousMemberForUnchangedProperty()
    {
        const string input = "public class TestClass\n{\n    private string _d; /** Doc */\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that document-level formatting separates a delimited documentation comment behind the previous member
    /// with one blank line and no trailing whitespace in one pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DocumentLevelFormattingSeparatesDelimitedDocumentationWithoutTrailingWhitespace()
    {
        const string input = "public class TestClass\n{\n    private string _d; /** Doc */\n    public string Description { get; set; }\n}";
        const string expected = "public class TestClass\n{\n    private string _d;\n\n    /** Doc */\n    public string Description { get; set; }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(input, endOfLine)));
                var result = (await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken)).Output;

                Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), (await result.GetTextAsync(TestContext.CancellationToken)).ToString(), $"Unexpected document-level output under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation behind multi line block comment for rewritten
    /// and unchanged property, because the entry point writes back the target's leading trivia as it stands in the
    /// document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationBehindMultiLineBlockCommentForRewrittenAndUnchangedProperty()
    {
        const string trailing = "private string _d; /* a\n    b */ /** doc */";
        var expected = BracedPropertyAfterField(string.Empty).Replace("private string _d;", trailing);

        await AssertFormatsTarget(UnbracedPropertyAfterField(string.Empty).Replace("private string _d;", trailing),
                                  expected,
                                  SelectSingle<PropertyDeclarationSyntax>);
        await AssertFormatsTarget(BracedPropertyAfterField(string.Empty).Replace("private string _d;", trailing),
                                  expected,
                                  SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation behind block comment of previous member,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationBehindBlockCommentOfPreviousMember()
    {
        const string input = "public class TestClass\n{\n    private string _d; /* c */ /** Doc */\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps tab in front of delimited documentation behind previous member,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    [Requirement("FMT-PIPE-010")]
    public async Task KeepsTabInFrontOfDelimitedDocumentationBehindPreviousMember()
    {
        const string input = "public class TestClass\n{\n    private string _d;\t/** Doc */\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation behind previous statement, because the entry
    /// point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationBehindPreviousStatement()
    {
        const string input = "public class TestClass\n{\n    public void M()\n    {\n        var a = 0;\n        a = 1; /** Doc */\n        a = 2;\n    }\n}";

        await AssertFormatsTarget(input, input, SelectStatement("a = 2"));
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation behind previous member before conditional
    /// directive, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationBehindPreviousMemberBeforeConditionalDirective()
    {
        const string input = "public class TestClass\n{\n    private string _d; /** Doc */\n#if !RELEASE\n    public string Description { get; set; }\n#endif\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation behind previous member before disabled text,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationBehindPreviousMemberBeforeDisabledText()
    {
        const string input = "public class TestClass\n{\n    private string _d; /** Doc */\n#if false\n    int x;\n#endif\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind previous member followed by blank
    /// line, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindPreviousMemberFollowedByBlankLine()
    {
        const string input = "public class TestClass\n{\n    private string _d; /// Doc\n\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind previous member, because the entry
    /// point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    [Requirement("FMT-PIPE-010")]
    public async Task KeepsSingleLineDocumentationBehindPreviousMember()
    {
        const string input = "public class TestClass\n{\n    private string _d; /// Doc\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind previous statement, because the entry
    /// point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindPreviousStatement()
    {
        const string input = "public class TestClass\n{\n    public void M()\n    {\n        var a = 0;\n        a = 1; /// Doc\n        a = 2;\n    }\n}";

        await AssertFormatsTarget(input, input, SelectStatement("a = 2"));
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind case label, because the entry point
    /// writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindCaseLabel()
    {
        const string input = "public class TestClass\n{\n    public void M(int v)\n    {\n        switch (v)\n        {\n            case 1: /// Doc\n                M(2);\n                break;\n        }\n    }\n}";

        await AssertFormatsTarget(input, input, SelectStatement("M(2)"));
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind block comment of previous member,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindBlockCommentOfPreviousMember()
    {
        const string input = "public class TestClass\n{\n    private string _d; /* c */ /// Doc\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind opening brace of type, because the
    /// entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindOpeningBraceOfType()
    {
        const string input = "public class TestClass\n{ /// Doc\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind opening brace of method body, because
    /// the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindOpeningBraceOfMethodBody()
    {
        const string input = "public class TestClass\n{\n    public void M()\n    { /// Doc\n        M();\n    }\n}";

        await AssertFormatsTarget(input, input, SelectStatement("M()"));
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind opening brace of method body when the
    /// target holds a tab, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindOpeningBraceOfMethodBodyWhenTheTargetHoldsATab()
    {
        const string input = "public class TestClass\n{\n    public void M()\n    { /// Doc\n        M(\t);\n    }\n}";
        const string expected = "public class TestClass\n{\n    public void M()\n    { /// Doc\n        M();\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectStatement("M("));
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind opening brace of type when the target
    /// holds a tab, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    [Requirement("FMT-PIPE-010")]
    public async Task KeepsSingleLineDocumentationBehindOpeningBraceOfTypeWhenTheTargetHoldsATab()
    {
        const string input = "public class TestClass\n{ /// Doc\n    public string Description {\tget; set; }\n}";
        const string expected = "public class TestClass\n{ /// Doc\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind case label when the target holds a
    /// tab, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindCaseLabelWhenTheTargetHoldsATab()
    {
        const string input = "public class TestClass\n{\n    public void M(int v)\n    {\n        switch (v)\n        {\n            case 1: /// Doc\n                M(\t2);\n                break;\n        }\n    }\n}";
        const string expected = "public class TestClass\n{\n    public void M(int v)\n    {\n        switch (v)\n        {\n            case 1: /// Doc\n                M(2);\n                break;\n        }\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectStatement("M("));
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind opening brace of method body and the
    /// tabs of the target, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindOpeningBraceOfMethodBodyAndTheTabsOfTheTarget()
    {
        const string input = "public class TestClass\n{\n    public void M()\n    { /// Doc\n\t\tM();\n    }\n}";

        await AssertFormatsTarget(input, input, SelectStatement("M("));
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind previous statement of block target,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindPreviousStatementOfBlockTarget()
    {
        const string input = "public class TestClass\n{\n    public void M()\n    {\n        var a = 0; /// Doc\n        {\n            a = 2;\n        }\n    }\n}";

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation behind previous statement of block target,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationBehindPreviousStatementOfBlockTarget()
    {
        const string input = "public class TestClass\n{\n    public void M()\n    {\n        var a = 0; /** Doc */\n        {\n            a = 2;\n        }\n    }\n}";

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind previous statement of block target in
    /// switch section, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindPreviousStatementOfBlockTargetInSwitchSection()
    {
        const string input = "public class TestClass\n{\n    public void M(int v)\n    {\n        switch (v)\n        {\n            case 1:\n                M(2); /// Doc\n                {\n                    M(3);\n                }\n\n                break;\n        }\n    }\n}";

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps single line documentation behind method signature of body target,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsSingleLineDocumentationBehindMethodSignatureOfBodyTarget()
    {
        const string input = "public class TestClass\n{\n    public void M() /// Doc\n    {\n        M();\n    }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation behind method signature of body target,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationBehindMethodSignatureOfBodyTarget()
    {
        const string input = "public class TestClass\n{\n    public void M() /** Doc */\n    {\n        M();\n    }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that a single-line documentation comment behind the previous top-level type stays in place, because the
    /// leading trivia of a top-level target is kept as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task KeepsSingleLineDocumentationBehindPreviousTopLevelType()
    {
        const string input = "public class First\n{\n    private int _a;\n} /// Doc\npublic class Second\n{\n    private int _b;\n}";

        await AssertFormatsTarget(input, input, root => root.DescendantNodes().OfType<ClassDeclarationSyntax>().Last());
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the previous top-level type stays in place together with the
    /// whitespace that separates it from that type, because the leading trivia of a top-level target is kept as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsWhitespaceInFrontOfDelimitedDocumentationBehindPreviousTopLevelType()
    {
        const string input = "public class First\n{\n    private int _a;\n} /** Doc */\npublic class Second\n{\n    private int _b;\n}";

        await AssertFormatsTarget(input, input, root => root.DescendantNodes().OfType<ClassDeclarationSyntax>().Last());
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation written directly behind previous member,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationWrittenDirectlyBehindPreviousMember()
    {
        const string input = "public class TestClass\n{\n    private string _d;/** Doc */\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps delimited documentation written directly behind block comment, because
    /// the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsDelimitedDocumentationWrittenDirectlyBehindBlockComment()
    {
        const string input = "public class TestClass\n{\n    private string _d; /* c *//** Doc */\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps trailing whitespace of previous member and inserts no blank line above
    /// delimited documentation, because the entry point writes back the target's leading trivia as it stands in the
    /// document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsTrailingWhitespaceOfPreviousMemberAndInsertsNoBlankLineAboveDelimitedDocumentation()
    {
        const string input = "public class TestClass\n{\n    private string _d; \n    /** Doc */\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
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
    /// Verifies that node-level formatting keeps region directly below previous member of rewritten property, because
    /// the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsRegionDirectlyBelowPreviousMemberOfRewrittenProperty()
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
    /// Verifies that node-level formatting keeps region directly below commented opening brace of rewritten first
    /// member, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsRegionDirectlyBelowCommentedOpeningBraceOfRewrittenFirstMember()
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
    /// Verifies that node-level formatting keeps end region directly below previous member of rewritten property,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsEndRegionDirectlyBelowPreviousMemberOfRewrittenProperty()
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
    /// Verifies that node-level formatting keeps comment directly below file scoped namespace of rewritten type,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsCommentDirectlyBelowFileScopedNamespaceOfRewrittenType()
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
    /// Verifies that node-level formatting does not insert blank line above rewritten statement after declaration,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveRewrittenStatementAfterDeclaration()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var copy = value;
                                     if (copy > 0)
                                         copy++;

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
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
    /// Verifies that node-level formatting does not insert blank line above rewritten statement after closing brace,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveRewrittenStatementAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                     {
                                         value--;
                                     }
                                     if (value > 0)
                                         value++;

                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        if (value > 1)
                                        {
                                            value--;
                                        }
                                        if (value > 0)
                                        {
                                            value++;
                                        }

                                        return value;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectLast<IfStatementSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above unchanged statement after declaration,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveUnchangedStatementAfterDeclaration()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
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

        await AssertFormatsTarget(input, input, SelectStatement("if (copy"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above unchanged statement after closing brace,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveUnchangedStatementAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                     {
                                         value--;
                                     }
                                     Consume(value);

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectStatement("Consume(value)"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line below directive above statement after closing
    /// brace, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineBelowDirectiveAboveStatementAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                     {
                                         value--;
                                     }
                             #if !DEBUG
                                     Consume(value);
                             #endif

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectStatement("Consume(value)"));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a statement whose body a structural transform braces, when a directive directly precedes it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineBelowDirectiveAboveRewrittenStatementAfterDeclaration()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var copy = value;
                             #if !DEBUG
                                     if (copy > 0)
                                         copy++;
                             #endif

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        var copy = value;
                                #if !DEBUG
                                        if (copy > 0)
                                        {
                                            copy++;
                                        }
                                #endif

                                        return copy;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("if (copy"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above rewritten statement after trailing comment,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveRewrittenStatementAfterTrailingComment()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var copy = value; // Copy
                                     if (copy > 0)
                                         copy++;

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        var copy = value; // Copy
                                        if (copy > 0)
                                        {
                                            copy++;
                                        }

                                        return copy;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("if (copy"));
    }

    /// <summary>
    /// Verifies that node-level formatting keeps two blank lines above rewritten statement, because the entry point
    /// writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsTwoBlankLinesAboveRewrittenStatement()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var copy = value;


                                     if (copy > 0)
                                         copy++;

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
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

        await AssertFormatsTarget(input, expected, SelectStatement("if (copy"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above comment of rewritten statement after
    /// declaration, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveCommentOfRewrittenStatementAfterDeclaration()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var copy = value;
                                     // Increment
                                     if (copy > 0)
                                         copy++;

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        var copy = value;
                                        // Increment
                                        if (copy > 0)
                                        {
                                            copy++;
                                        }

                                        return copy;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("if (copy"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above declaration after expression, because the
    /// entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveDeclarationAfterExpression()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     value++;
                                     var copy = value;

                                     return copy;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectStatement("var copy"));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a declaration statement that directly follows another declaration statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveDeclarationAfterDeclaration()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var first = value;
                                     var copy = first;

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        var first = value;
                                        var copy = first;

                                        return copy;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("var copy"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above invocation after declaration, because the
    /// entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveInvocationAfterDeclaration()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var copy = value;
                                     Consume(copy);

                                     return copy;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectStatement("Consume(copy)"));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above an assignment statement that directly follows a declaration statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveAssignmentAfterDeclaration()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var copy = value;
                                     copy = copy + 1;

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        var copy = value;
                                        copy = copy + 1;

                                        return copy;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("copy = copy"));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above an invocation statement that directly follows another expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveInvocationAfterExpression()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     value++;
                                     Consume(value);

                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        value++;
                                        Consume(value);

                                        return value;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("Consume(value)"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above yield after expression, because the entry
    /// point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveYieldAfterExpression()
    {
        const string input = """
                             public class C
                             {
                                 public System.Collections.Generic.IEnumerable<int> M(int value)
                                 {
                                     value++;
                                     yield return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectStatement("yield return value"));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a yield statement that directly follows another yield statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveYieldAfterYield()
    {
        const string input = """
                             public class C
                             {
                                 public System.Collections.Generic.IEnumerable<int> M(int value)
                                 {
                                     yield return value;
                                     yield return value + 1;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public System.Collections.Generic.IEnumerable<int> M(int value)
                                    {
                                        yield return value;
                                        yield return value + 1;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("yield return value + 1"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above break in loop after expression, because the
    /// entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveBreakInLoopAfterExpression()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     while (value > 0)
                                     {
                                         value--;
                                         break;
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectStatement("break"));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above the terminal break statement of a switch section that directly follows an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveTerminalSwitchSectionBreakAfterExpression()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             value++;
                                             break;
                                     }

                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                value++;
                                                break;
                                        }

                                        return value;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("break"));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above the terminal break statement of a switch section that directly follows a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveTerminalSwitchSectionBreakAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
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
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
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

        await AssertFormatsTarget(input, expected, SelectStatement("break"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above statement after break, because the entry
    /// point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveStatementAfterBreak()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     while (value > 0)
                                     {
                                         break;
                                         Consume(value);
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectStatement("Consume(value)"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above switch section after break section, because
    /// the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveSwitchSectionAfterBreakSection()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             value++;
                                             break;
                                         case 2:
                                             value--;
                                             break;
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<SwitchSectionSyntax>);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a switch section whose preceding section ends in a return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveSwitchSectionAfterReturnSection()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             return value;
                                         case 2:
                                             value--;
                                             break;
                                     }

                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                return value;
                                            case 2:
                                                value--;
                                                break;
                                        }

                                        return value;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectLast<SwitchSectionSyntax>);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a statement whose body a structural transform braces, when it is the first statement of its block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveRewrittenFirstStatementInBlock()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 0)
                                         value++;

                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        if (value > 0)
                                        {
                                            value++;
                                        }

                                        return value;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("if (value"));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above an expression statement whose preceding statement ends in a semicolon, even when document-level formatting would brace that preceding statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveStatementAfterUnbracedStatement()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                         value--;
                                     Consume(value);

                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        if (value > 1)
                                            value--;
                                        Consume(value);

                                        return value;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("Consume(value)"));
    }

    /// <summary>
    /// Verifies that the gap above a statement that shares its line with the preceding statement stays unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveStatementOnSameLineAsPreviousStatement()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var copy = value; Consume(copy);

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        var copy = value; Consume(copy);

                                        return copy;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("Consume(copy)"));
    }

    /// <summary>
    /// Verifies that the blank line above the leading comment of a statement whose body a structural transform braces is kept, and that no second blank line is inserted between the comment and the statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsBlankLineAboveCommentOfRewrittenStatementAfterDeclaration()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     var copy = value;

                                     // Increment
                                     if (copy > 0)
                                         copy++;

                                     return copy;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        var copy = value;

                                        // Increment
                                        if (copy > 0)
                                        {
                                            copy++;
                                        }

                                        return copy;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectStatement("if (copy"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line below disabled text above statement after closing
    /// brace, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineBelowDisabledTextAboveStatementAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                     {
                                         value--;
                                     }
                             #if DEBUG
                                     value++;
                             #endif
                                     Consume(value);

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectStatement("Consume(value)"));
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above block statement after closing brace,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveBlockStatementAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                     {
                                         value--;
                                     }
                                     {
                                         Consume(value);
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above comment of block statement after closing
    /// brace, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveCommentOfBlockStatementAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                     {
                                         value--;
                                     }
                                     // Scope
                                     {
                                         value++;
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line below directive above comment of block statement
    /// after closing brace, because the entry point writes back the target's leading trivia as it stands in the
    /// document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineBelowDirectiveAboveCommentOfBlockStatementAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                     {
                                         value--;
                                     }
                             #if !DEBUG
                                     // Scope
                                     {
                                         value++;
                                     }
                             #endif

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line below directive above block statement after
    /// closing brace, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineBelowDirectiveAboveBlockStatementAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                     {
                                         value--;
                                     }
                             #if !DEBUG
                                     {
                                         value++;
                                     }
                             #endif

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above comment of switch section after break
    /// section, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveCommentOfSwitchSectionAfterBreakSection()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             value++;
                                             break;
                                         // Second
                                         case 2:
                                             value--;
                                             break;
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<SwitchSectionSyntax>);
    }

    /// <summary>
    /// Verifies that a switch section whose preceding section ends in a break statement gets no blank line when a directive directly precedes the section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    public async Task InsertsNoBlankLineAboveSwitchSectionDirectlyPrecededByDirectiveAfterBreakSection()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             value++;
                                             break;
                             #if !DEBUG
                                         case 2:
                                             value--;
                                             break;
                             #endif
                                     }

                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                value++;
                                                break;
                                #if !DEBUG
                                            case 2:
                                                value--;
                                                break;
                                #endif
                                        }

                                        return value;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectLast<SwitchSectionSyntax>);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above the first section of a switch statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task DoesNotInsertBlankLineAboveFirstSwitchSection()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             value++;
                                             break;
                                         case 2:
                                             value--;
                                             break;
                                     }

                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                value++;
                                                break;
                                            case 2:
                                                value--;
                                                break;
                                        }

                                        return value;
                                    }
                                }
                                """;

        await AssertFormatsTarget(input, expected, SelectFirst<SwitchSectionSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above block statement after break, because the
    /// entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveBlockStatementAfterBreak()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     while (value > 0)
                                     {
                                         break;
                                         {
                                             value++;
                                         }
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that the author's blank line above a block statement that follows a statement not ending with a closing brace is kept, as in document-level formatting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    [Requirement("FMT-BLANK-017")]
    public async Task KeepsAuthorBlankLineAboveBlockStatementAfterOtherStatement()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     value--;

                                     {
                                         Consume(value);
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a block statement that follows a statement not ending with a closing brace when the author wrote none, as in document-level formatting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    [Requirement("FMT-BLANK-017")]
    public async Task InsertsNoBlankLineAboveBlockStatementAfterOtherStatement()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     value--;
                                     {
                                         Consume(value);
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above glued comment of block statement after
    /// closing brace, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveGluedCommentOfBlockStatementAfterClosingBrace()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     if (value > 1)
                                     {
                                         value--;
                                     }
                                     /* Scope */ {
                                         Consume(value);
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line above comment of block statement after break,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineAboveCommentOfBlockStatementAfterBreak()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     while (value > 0)
                                     {
                                         break;
                                         // Scope
                                         {
                                             value++;
                                         }
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting does not insert blank line behind directive of commented block statement
    /// after break, because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DoesNotInsertBlankLineBehindDirectiveOfCommentedBlockStatementAfterBreak()
    {
        const string input = """
                             public class C
                             {
                                 public int M(int value)
                                 {
                                     while (value > 0)
                                     {
                                         break;
                             #if !DEBUG
                                         // Scope
                                         {
                                             value++;
                                         }
                             #endif
                                     }

                                     return value;
                                 }
                             }
                             """;

        await AssertFormatsTarget(input, input, SelectLast<BlockSyntax>);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written behind the opening brace of a type stays exactly where it is
    /// and that a second node-level pass does not change the result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsDelimitedDocumentationBehindOpeningBraceStable()
    {
        const string input = "public class TestClass\n{ /** Doc */\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written behind a switch label stays exactly where it is and that a
    /// second node-level pass does not change the result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsDelimitedDocumentationBehindCaseLabelStable()
    {
        const string input = "public class TestClass\n{\n    public void M(int v)\n    {\n        switch (v)\n        {\n            case 1: /** Doc */\n                M(2);\n                break;\n        }\n    }\n}";

        await AssertFormatsTarget(input, input, SelectStatement("M(2)"));
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written behind the opening brace of a method body stays exactly where
    /// it is and that a second node-level pass does not change the result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsDelimitedDocumentationBehindOpeningBraceOfMethodBodyStable()
    {
        const string input = "public class TestClass\n{\n    public void M()\n    { /** Doc */\n        M();\n    }\n}";

        await AssertFormatsTarget(input, input, SelectStatement("M()"));
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written behind a default label stays exactly where it is and that a
    /// second node-level pass does not change the result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsDelimitedDocumentationBehindDefaultLabelStable()
    {
        const string input = "public class TestClass\n{\n    public void M(int v)\n    {\n        switch (v)\n        {\n            default: /** Doc */\n                M(2);\n                break;\n        }\n    }\n}";

        await AssertFormatsTarget(input, input, SelectStatement("M(2)"));
    }

    /// <summary>
    /// Verifies that node-level formatting keeps column of delimited documentation on own line below opening brace,
    /// because the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsColumnOfDelimitedDocumentationOnOwnLineBelowOpeningBrace()
    {
        const string input = "public class TestClass\n{\n/** Doc */\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that node-level formatting keeps two delimited documentation comments behind previous member, because
    /// the entry point writes back the target's leading trivia as it stands in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsTwoDelimitedDocumentationCommentsBehindPreviousMember()
    {
        const string input = "public class TestClass\n{\n    private string _d; /** a */ /** b */\n    public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that two delimited documentation comments behind the previous member stay where they are when the target
    /// starts behind them on their line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsTwoDelimitedDocumentationCommentsBehindPreviousMemberWhenTargetSharesTheirLine()
    {
        const string input = "public class TestClass\n{\n    private string _d; /** a */ /** b */ public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the previous member stays where it is when the target starts
    /// behind it on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task KeepsDelimitedDocumentationBehindPreviousMemberWhenTargetSharesItsLine()
    {
        const string input = "public class TestClass\n{\n    private string _d; /** d */ public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the opening brace of a type stays where it is when the target
    /// starts behind it on the brace's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    public async Task KeepsDelimitedDocumentationBehindOpeningBraceWhenTargetSharesItsLine()
    {
        const string input = "public class TestClass\n{ /** d */ public string Description { get; set; }\n}";

        await AssertFormatsTarget(input, input, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies the spec example: with the if statement as target, no blank line is added above the comment that leads
    /// it, and none above the if statement without the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task SpecExampleKeepsCommentAboveStatementTargetBehindAssignment()
    {
        const string input = "class C\n{\n    void M(int a)\n    {\n        a = 1;\n        // c\n        if (a > 0) { a++; }\n    }\n}";
        const string expected = "class C\n{\n    void M(int a)\n    {\n        a = 1;\n        // c\n        if (a > 0)\n        {\n            a++;\n        }\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<IfStatementSyntax>);
        await AssertFormatsTarget(input.Replace("        // c\n", string.Empty), expected.Replace("        // c\n", string.Empty), SelectSingle<IfStatementSyntax>);
    }

    /// <summary>
    /// Verifies the spec example: with the property as target, the one-line documentation comment stays on its line and
    /// the blank line between the opening brace and the comment stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task SpecExampleKeepsOneLineDocumentationCommentAndBlankLineBehindOpeningBraceOfPropertyTarget()
    {
        const string input = "class C\n{\n\n    /// <summary>The value.</summary>\n    public int Value\n    {\n        get;\n        set;\n    } = 1;\n}";
        const string expected = "class C\n{\n\n    /// <summary>The value.</summary>\n    public int Value { get; set; } = 1;\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies the spec example: a delimited documentation comment written behind the code in front of the target, a
    /// property written over several lines, stays behind that code together with the whitespace in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task SpecExampleKeepsDelimitedDocumentationBehindPreviousMemberOfMultiLinePropertyTarget()
    {
        const string input = "class C\n{\n    int _a; /** Doc */\n    int P\n    {\n        get;\n        set;\n    }\n}";
        const string expected = "class C\n{\n    int _a; /** Doc */\n    int P { get; set; }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<PropertyDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that a blank line that holds whitespace in the leading trivia of a statement target is written back as
    /// it stands, whitespace included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsWhitespaceOnBlankLineAboveTarget()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = NormalizeLineEndings("class C\n{\n    void M(int a)\n    {\n        a = 1;\n    \n        if (a > 0) { a++; }\n    }\n}", endOfLine);
            var expected = NormalizeLineEndings("class C\n{\n    void M(int a)\n    {\n        a = 1;\n    \n        if (a > 0)\n        {\n            a++;\n        }\n    }\n}", endOfLine);

            Assert.AreEqual(expected, await FormatTarget(input, SelectSingle<IfStatementSyntax>), $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a line break other than the document's line ending in the leading trivia of a statement target is
    /// written back as it stands, while the lines of the target get the document's line ending
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task KeepsForeignLineBreakInLeadingTriviaOfTarget()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var foreignLineBreak = endOfLine == "\n" ? "\r\n" : "\n";
            var input = NormalizeLineEndings("class C\n{\n    void M(int a)\n    {\n        a = 1;\n        // c¶        if (a > 0) { a++; }\n    }\n}", endOfLine).Replace("¶", foreignLineBreak);
            var expected = NormalizeLineEndings("class C\n{\n    void M(int a)\n    {\n        a = 1;\n        // c¶        if (a > 0)\n        {\n            a++;\n        }\n    }\n}", endOfLine).Replace("¶", foreignLineBreak);

            Assert.AreEqual(expected, await FormatTarget(input, SelectSingle<IfStatementSyntax>), $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Selects the first node of the given type below the root
    /// </summary>
    /// <typeparam name="TNode">The node type</typeparam>
    /// <param name="root">The document root</param>
    /// <returns>The first matching node</returns>
    private static SyntaxNode SelectFirst<TNode>(SyntaxNode root)
        where TNode : SyntaxNode
    {
        return root.DescendantNodes().OfType<TNode>().First();
    }

    /// <summary>
    /// Selects the last node of the given type below the root
    /// </summary>
    /// <typeparam name="TNode">The node type</typeparam>
    /// <param name="root">The document root</param>
    /// <returns>The last matching node</returns>
    private static SyntaxNode SelectLast<TNode>(SyntaxNode root)
        where TNode : SyntaxNode
    {
        return root.DescendantNodes().OfType<TNode>().Last();
    }

    /// <summary>
    /// Creates a selector for the first statement below the root whose text starts with the given prefix
    /// </summary>
    /// <param name="prefix">The start of the statement text, without leading trivia</param>
    /// <returns>The selector</returns>
    private static Func<SyntaxNode, SyntaxNode> SelectStatement(string prefix)
    {
        return root => root.DescendantNodes().OfType<StatementSyntax>().First(statement => statement.ToString().StartsWith(prefix, StringComparison.Ordinal));
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

            var result = (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, selectTarget(root), TestContext.CancellationToken)).Output;

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

    /// <summary>
    /// Formats the selected target under LF and CRLF, with every <c>¶</c> of the input replaced by a line break other than the
    /// document's line ending, asserts the expected text, and asserts that formatting the same target of the result a second
    /// time changes nothing
    /// </summary>
    /// <param name="inputWithLf">The input with LF line endings, where <c>¶</c> marks the foreign line break</param>
    /// <param name="expectedWithLf">The expected output with LF line endings</param>
    /// <param name="selectTarget">Selects the target node from the document root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTargetWithForeignLineBreaks(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectTarget)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var expected = NormalizeLineEndings(expectedWithLf, endOfLine);
            var foreignLineBreaks = endOfLine == "\n" ? new[] { "\r\n", "\r", "\u2028" } : new[] { "\n", "\r", "\u2029" };

            foreach (var foreignLineBreak in foreignLineBreaks)
            {
                var description = $"{DescribeLineEnding(endOfLine)} line endings with {string.Join(" ", foreignLineBreak.Select(character => $"U+{(int)character:X4}"))} as the foreign line break";
                var firstPass = await FormatTarget(NormalizeLineEndings(inputWithLf, endOfLine).Replace("¶", foreignLineBreak), selectTarget);

                Assert.AreEqual(expected, firstPass, $"Unexpected first pass under {description}.");
                AssertUsesLineEnding(firstPass, endOfLine);

                var secondPass = await FormatTarget(firstPass, selectTarget);

                Assert.AreEqual(firstPass, secondPass, $"The second pass must not change the output under {description}.");
            }
        }
    }

    #endregion // Methods
}