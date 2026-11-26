using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the unsupported shape of a comment, a directive, or disabled text behind the <c>=</c> of an attribute argument or of
/// a using alias (FMT-UNSUP-022): every kind of trivia on one line and across lines, documentation comments in every documentation
/// mode, every attribute and using alias wherever it stands, the <c>=</c> tokens of other shapes, the gaps next to the <c>=</c>,
/// the shapes that keep their own reason, node-level inputs, and the closure of the formatter's output, each under LF and CRLF line
/// endings
/// </summary>
[TestClass]
public class UnsupportedNameEqualsCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: the <c>=</c> of a using alias and of an attribute argument are refused, the input is
    /// returned unchanged, and the first comment, which starts in line 2, column 14, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    public void SpecExampleUsingAliasAndAttributeArgumentAreUnsupported()
    {
        const string input = """
                             using System;
                             using Text = // alias
                             System.Text;

                             class NAttribute : Attribute
                             {
                                 public int Name { get; set; }
                             }

                             class C
                             {
                                 [N(Name = /* name */ 1)]
                                 void M()
                                 {
                                 }
                             }
                             """;

        AssertUnsupportedFor(NameEqualsCommentReason, input, "// alias");
        AssertStartsAt(input, 1, 13);
    }

    /// <summary>
    /// Verifies the specification example: an attribute argument alone is refused as well, the input is returned unchanged, and
    /// the comment, which starts in line 10, column 15, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    public void SpecExampleAttributeArgumentAloneIsUnsupported()
    {
        const string input = """
                             using System;

                             class NAttribute : Attribute
                             {
                                 public int Name { get; set; }
                             }

                             class C
                             {
                                 [N(Name = // name
                                 1)]
                                 void M()
                                 {
                                 }
                             }
                             """;

        AssertUnsupportedFor(NameEqualsCommentReason, input, "// name");
        AssertStartsAt(input, 9, 14);
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment in front of the <c>=</c> of a using alias and of an
    /// attribute argument, one behind a named argument's colon, and one behind the value do not refuse the input, which is a fixed
    /// point of the formatter
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    public void SpecExampleCommentsOutsideTheShapeAreSupported()
    {
        const string input = """
                             using System;

                             using Text /* c1 */ = System.Text;

                             class NAttribute : Attribute
                             {
                                 public int Name { get; set; }
                             }

                             class C
                             {
                                 [Obsolete(message: /* c2 */ "old")]
                                 [N(Name /* c3 */ = 1)]
                                 [N(Name = 1 /* c4 */)]
                                 void M()
                                 {
                                 }
                             }
                             """;

        AssertSupported(input);
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that every kind of trivia behind the <c>=</c> of a using alias refuses the input, and that a using alias refuses
    /// it wherever it stands: with <c>global</c>, inside a block-scoped and a file-scoped namespace
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    [DataRow("using Text = /* c */ System.Text;\n\nclass C\n{\n}", "/* c */", DisplayName = "One line")]
    [DataRow("using Text =\n    // c\n    System.Text;\n\nclass C\n{\n}", "// c", DisplayName = "Comment on its own line")]
    [DataRow("using Text = /** c */ System.Text;\n\nclass C\n{\n}", "/** c */", DisplayName = "Delimited documentation comment")]
    [DataRow("using Text =\n#if X\n    System.Text;\n#else\n    System.IO;\n#endif\n\nclass C\n{\n}", "#if X", DisplayName = "Conditional group")]
    [DataRow("using Text =\n#pragma warning disable CS0168\n    System.Text;\n\nclass C\n{\n}", "#pragma warning disable CS0168", DisplayName = "Pragma")]
    [DataRow("global using Text = /* c */ System.Text;\n\nclass C\n{\n}", "/* c */", DisplayName = "Global using alias")]
    [DataRow("namespace N\n{\n    using Text = /* c */ System.Text;\n\n    class C\n    {\n    }\n}", "/* c */", DisplayName = "Inside a block-scoped namespace")]
    [DataRow("namespace N;\n\nusing Text = /* c */ System.Text;\n\nclass C\n{\n}", "/* c */", DisplayName = "Inside a file-scoped namespace")]
    [DataRow("using Pair = /* c */ (int, int);\n\nclass C\n{\n}", "/* c */", DisplayName = "Alias of a tuple type")]
    public void UsingAliasIsUnsupported(string input, string reported)
    {
        AssertUnsupportedFor(NameEqualsCommentReason, input, reported);
    }

    /// <summary>
    /// Verifies that every kind of trivia behind the <c>=</c> of an attribute argument refuses the input, and that an attribute
    /// refuses it wherever it stands: on a type, a method, a parameter, a property, an accessor, a return value, and the assembly
    /// </summary>
    /// <param name="member">The member of <c>C</c> that holds the attribute</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    [DataRow("    [N(Name = /* c */ 1)]\n    void M()\n    {\n    }", "/* c */", DisplayName = "Method, one line")]
    [DataRow("    [N(Name =\n       // c\n       1)]\n    void M()\n    {\n    }", "// c", DisplayName = "Method, comment on its own line")]
    [DataRow("    [N(Name = /** c */ 1)]\n    void M()\n    {\n    }", "/** c */", DisplayName = "Delimited documentation comment")]
    [DataRow("    [N(Name =\n#if X\n       1)]\n#else\n       2)]\n#endif\n    void M()\n    {\n    }", "#if X", DisplayName = "Conditional group")]
    [DataRow("    [N(Name =\n#pragma warning disable CS0168\n       1)]\n    void M()\n    {\n    }", "#pragma warning disable CS0168", DisplayName = "Pragma")]
    [DataRow("    void M([N(Name = /* c */ 1)] int y)\n    {\n    }", "/* c */", DisplayName = "Parameter")]
    [DataRow("    [N(Name = /* c */ 1)]\n    int P { get; set; }", "/* c */", DisplayName = "Property")]
    [DataRow("    int P\n    {\n        [N(Name = /* c */ 1)]\n        get;\n    }", "/* c */", DisplayName = "Accessor")]
    [DataRow("    [return: N(Name = /* c */ 1)]\n    int M()\n    {\n        return 0;\n    }", "/* c */", DisplayName = "Return value")]
    [DataRow("    [N(1, Name = /* c */ 2)]\n    void M()\n    {\n    }", "/* c */", DisplayName = "Behind a positional argument")]
    public void AttributeArgumentIsUnsupported(string member, string reported)
    {
        AssertUnsupportedFor(NameEqualsCommentReason, WrapInClass(member), reported);
    }

    /// <summary>
    /// Verifies that an attribute on a type and on the assembly refuses the input
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    [DataRow("using System;\n\nclass NAttribute : Attribute\n{\n    public int Name { get; set; }\n}\n\n[N(Name = /* c */ 1)]\nclass C\n{\n}", DisplayName = "Type")]
    [DataRow("using System;\n\n[assembly: N(Name = /* c */ 1)]\n\nclass NAttribute : Attribute\n{\n    public int Name { get; set; }\n}", DisplayName = "Assembly")]
    public void AttributeOnATypeOrTheAssemblyIsUnsupported(string input)
    {
        AssertUnsupportedFor(NameEqualsCommentReason, input, "/* c */");
    }

    /// <summary>
    /// Verifies that a documentation comment behind the <c>=</c> refuses the input in every documentation mode, in which it is a
    /// documentation comment or an ordinary comment
    /// </summary>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.None)]
    [DataRow(DocumentationMode.Diagnose)]
    public void DocumentationCommentBehindTheEqualsIsUnsupportedInEveryDocumentationMode(DocumentationMode documentationMode)
    {
        var parseOptions = CSharpParseOptions.Default.WithDocumentationMode(documentationMode);

        AssertUnsupportedFor(NameEqualsCommentReason, WrapInClass("    [N(Name = /** c */ 1)]\n    void M()\n    {\n    }"), "/** c */", parseOptions: parseOptions);
        AssertUnsupportedFor(NameEqualsCommentReason, WrapInClass("    [N(Name =\n       /// <summary>c</summary>\n       1)]\n    void M()\n    {\n    }"), "/// <summary>c</summary>", parseOptions: parseOptions);
        AssertUnsupportedFor(NameEqualsCommentReason, "using Text =\n    /// <summary>c</summary>\n    System.Text;\n\nclass C\n{\n}", "/// <summary>c</summary>", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies the other side: a comment in front of the <c>=</c>, behind the value's first token, behind a named argument's
    /// colon, inside a positional argument, and above the attribute or the directive do not refuse the input
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    [DataRow("using Text /* c */ = System.Text;\n\nclass C\n{\n}", DisplayName = "In front of a using alias's =")]
    [DataRow("using Text = System /* c */ .Text;\n\nclass C\n{\n}", DisplayName = "Inside the alias's target behind its first token")]
    [DataRow("// c\nusing Text = System.Text;\n\nclass C\n{\n}", DisplayName = "Above the using alias")]
    [DataRow("using Text = System.Text; // c\n\nclass C\n{\n}", DisplayName = "Behind the using alias")]
    [DataRow("using System;\n\nclass NAttribute : Attribute\n{\n    public int Name { get; set; }\n}\n\nclass C\n{\n    [N(Name /* c */ = 1)]\n    void M()\n    {\n    }\n}", DisplayName = "In front of an attribute argument's =")]
    [DataRow("using System;\n\nclass NAttribute : Attribute\n{\n    public int Name { get; set; }\n}\n\nclass C\n{\n    [N(Name = 1 /* c */)]\n    void M()\n    {\n    }\n}", DisplayName = "Behind an attribute argument's value")]
    [DataRow("using System;\n\nclass C\n{\n    [Obsolete(message: /* c */ \"old\")]\n    void M()\n    {\n    }\n}", DisplayName = "Behind a named argument's colon")]
    [DataRow("using System;\n\nclass NAttribute : Attribute\n{\n    public int Name { get; set; }\n}\n\nclass C\n{\n    // c\n    [N(Name = 1)]\n    void M()\n    {\n    }\n}", DisplayName = "Above the attribute")]
    [DataRow("using Text =\n    System.Text;\n\nclass C\n{\n}", DisplayName = "Line break alone behind a using alias's =")]
    public void CommentOutsideTheGapIsSupported(string input)
    {
        AssertSupported(input);
    }

    /// <summary>
    /// Verifies the order and the other <c>=</c> tokens: a comment behind the <c>=</c> of an attribute argument of a field keeps
    /// FMT-UNSUP-004's reason, a directive there in a field with several declarators keeps FMT-UNSUP-010's, the <c>=</c> of an
    /// anonymous-object member keeps FMT-UNSUP-017's, and the <c>=</c> of an assignment and of a property is FMT-UNSUP-020's
    /// </summary>
    /// <param name="reason">The reason that is given</param>
    /// <param name="member">The member of <c>C</c></param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    [DataRow(VariableDeclarationCommentReason, "    [N(Name = /* c */ 1)] private int _e;", "/* c */", DisplayName = "Attribute argument of a field")]
    [DataRow(FieldDeclarationDirectiveReason, "    [N(Name =\n#pragma warning disable CS0168\n       1)]\n    private int _e, _f;", "#pragma warning disable CS0168", DisplayName = "Directive in an attribute argument of a field with several declarators")]
    [DataRow(AnonymousMemberEqualsCommentReason, "    object M()\n    {\n        return new { A = /* c */ 1 };\n    }", "/* c */", DisplayName = "Anonymous-object member")]
    [DataRow(AssignmentCommentReason, "    int P { get; } = /* c */ 1;", "/* c */", DisplayName = "Property")]
    public void OtherShapesKeepTheirReason(string reason, string member, string reported)
    {
        AssertUnsupportedFor(reason, WrapInClass(member), reported);
    }

    /// <summary>
    /// Verifies that a using alias and an attribute inside disabled text are not checked: where <c>X</c> is not defined, they are
    /// disabled text and the input is formatted, and where it is defined, the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ShapeInDisabledTextIsNotChecked(bool defineX)
    {
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;
        var usingInput = "#if X\nusing Text = /* c */ System.Text;\n#endif\n\nclass C\n{\n}";
        var attributeInput = WrapInClass("#if X\n    [N(Name = /* c */ 1)]\n#endif\n    void M()\n    {\n    }");

        if (defineX)
        {
            AssertUnsupportedFor(NameEqualsCommentReason, usingInput, "/* c */", parseOptions: parseOptions);
            AssertUnsupportedFor(NameEqualsCommentReason, attributeInput, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(usingInput, parseOptions);
            AssertSupported(attributeInput, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that a node-level input holds the shape when it holds the <c>=</c> and the comment — the attribute argument, the
    /// attribute, the attribute list, the method, and the using directive —, while the value alone does not, also with the
    /// comment in the leading trivia of its first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-022")]
    public void NodeThatHoldsTheEqualsHoldsTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings("using Text =\n    // t\n    System.Text;\n" + WrapInClass("    [N(Name =\n       // c\n       1)]\n    void M()\n    {\n    }"), endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var argument = root.DescendantNodes().OfType<AttributeArgumentSyntax>().Single();
            var alias = root.DescendantNodes().OfType<UsingDirectiveSyntax>().Single(directive => directive.Alias != null);

            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(argument.Expression));
            Assert.IsNull(ReihitsuFormatter.FormatNode(argument.Expression, cancellationToken: TestContext.CancellationToken).Unsupported);
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(alias.NamespaceOrType));

            foreach (var node in new SyntaxNode[] { argument, argument.Parent.Parent, argument.Parent.Parent.Parent, argument.Parent.Parent.Parent.Parent, alias })
            {
                Assert.AreEqual(NameEqualsCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(node)?.Reason, $"The node '{node.Kind()}' holds the = and the comment.");
            }

            Assert.AreEqual(NameEqualsCommentReason, ReihitsuFormatter.FormatNode(argument.Parent.Parent, cancellationToken: TestContext.CancellationToken).Unsupported?.Reason);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds: a using directive
    /// built with a comment behind its alias's <c>=</c> is refused, and the same directive without the comment is not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-022")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode()
    {
        var node = SyntaxFactory.ParseCompilationUnit("using Text = /* c */ System.Text;").Usings.Single();

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(node);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(NameEqualsCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, node.SyntaxTree.GetText(TestContext.CancellationToken).ToString(), "/* c */", 0, "LF");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(SyntaxFactory.ParseCompilationUnit("using Text = System.Text;").Usings.Single()));
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: the join behind an attribute argument's <c>=</c>, the merge and split of
    /// attribute lists, and the ordering of using directives carry no comment into the gap, and the output, formatted again, is
    /// neither refused nor changed
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-022")]
    [DataRow("using System;\n\nclass NAttribute : Attribute\n{\n    public int Name { get; set; }\n}\n\nclass C\n{\n    [N(Name =\n       1)] // c\n    void M()\n    {\n    }\n}", DisplayName = "Join behind an attribute argument's =")]
    [DataRow("using System;\n\nclass NAttribute : Attribute\n{\n    public int Name { get; set; }\n}\n\nclass C\n{\n    [N(Name = 1), /* c */ Obsolete]\n    void M()\n    {\n    }\n}", DisplayName = "Split of an attribute list")]
    [DataRow("using Text = System.Text; // t\nusing System; // s\nusing IO = System.IO; // i\n\nclass C\n{\n}", DisplayName = "Ordering of using aliases")]
    public void OutputOfTheFormatterIsNotUnsupported(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var first = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(first.Unsupported, $"The input must be formatted under {endingName} line endings: {first.Unsupported?.Reason}");

            var output = first.Output.ToString();
            var second = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(output, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(second.Unsupported, $"The formatted output must not be refused under {endingName} line endings:{endOfLine}{output}");
            Assert.AreEqual(output, second.Output.ToString(), $"The formatted output must be stable under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Wraps members in a class <c>C</c>, beside the attribute <c>N</c> with its property <c>Name</c>
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string members)
    {
        return $"using System;\n\nclass NAttribute : Attribute\n{{\n    public NAttribute()\n    {{\n    }}\n\n    public NAttribute(int value)\n    {{\n    }}\n\n    public int Name {{ get; set; }}\n}}\n\nclass C\n{{\n{members}\n}}";
    }

    /// <summary>
    /// Asserts that the formatter reports the shape of the input in the given line and column, under LF and CRLF line endings
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="line">The zero-based line</param>
    /// <param name="character">The zero-based column</param>
    private void AssertStartsAt(string input, int line, int character)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(new LinePosition(line, character), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
        }
    }

    #endregion // Methods
}