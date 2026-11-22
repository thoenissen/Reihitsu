using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the unsupported shape of a comment, a directive, or disabled text inside a list of omitted type arguments
/// (FMT-UNSUP-018): behind the <c>&lt;</c> and a comma and in front of a comma and the <c>&gt;</c>, on one line and across lines,
/// in every documentation mode and build, the lists that hold a type, the trivia outside the list, the shapes that keep their own
/// reason, and node-level inputs, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedOmittedTypeArgumentCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: each list is refused, the input is returned unchanged, and the first comment, which
    /// starts in line 5, column 64, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-018")]
    public void SpecExampleEachListIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a = typeof(System.Collections.Generic.Dictionary<, // c1
                                         >);
                                     var b = typeof(System.Collections.Generic.List</* c2 */>);
                                 }
                             }
                             """;

        AssertUnsupportedFor(OmittedTypeArgumentCommentReason, input, "// c1");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(4, 63), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment behind the list and a comment in a list that holds a type
    /// do not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-018")]
    public void SpecExampleCommentsOutsideAListOfOmittedTypeArgumentsAreSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a = typeof(System.Collections.Generic.Dictionary<,>); // c1
                                     var b = typeof(System.Collections.Generic.Dictionary<int, /* c2 */ string>);
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that every place inside the list refuses the input — behind the <c>&lt;</c>, behind a comma, in front of the
    /// <c>&gt;</c> — on one line and across lines, with comments, documentation comments, and directives
    /// </summary>
    /// <param name="expression">The <c>typeof</c> or <c>nameof</c> expression</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-018")]
    [DataRow("typeof(Dictionary</* c */,>)", "/* c */", DisplayName = "Behind the opening angle bracket, one line")]
    [DataRow("typeof(Dictionary<, /* c */>)", "/* c */", DisplayName = "Behind the comma, one line")]
    [DataRow("typeof(Dictionary<, // c\n            >)", "// c", DisplayName = "Behind the comma, line comment")]
    [DataRow("typeof(List</* c */>)", "/* c */", DisplayName = "Single omitted argument")]
    [DataRow("typeof(Func<,, /* c */>)", "/* c */", DisplayName = "Behind the second comma")]
    [DataRow("typeof(Dictionary<,\n#pragma warning disable CS0168\n            >)", "#pragma warning disable CS0168", DisplayName = "Directive in front of the closing angle bracket")]
    [DataRow("typeof(Dictionary<,\n#if false\n            junk\n#endif\n            >)", "#if false", DisplayName = "Conditional group with disabled text")]
    [DataRow("typeof(Dictionary<, /** c */>)", "/** c */", DisplayName = "Delimited documentation comment")]
    [DataRow("typeof(Dictionary<, /// c\n            >)", "/// c", DisplayName = "Single-line documentation comment")]
    [DataRow("nameof(Dictionary<, /* c */>)", "/* c */", DisplayName = "In nameof")]
    [DataRow("typeof(List<Dictionary<, /* c */>>)", "/* c */", DisplayName = "Nested list")]
    public void CommentOrDirectiveInsideAListOfOmittedTypeArgumentsIsUnsupported(string expression, string reported)
    {
        foreach (var mode in new[] { DocumentationMode.None, DocumentationMode.Parse, DocumentationMode.Diagnose })
        {
            AssertUnsupportedFor(OmittedTypeArgumentCommentReason, WrapInMethod($"        var t = {expression};"), reported, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(mode));
        }
    }

    /// <summary>
    /// Verifies the other side: a comment in a type argument list that holds a type, in front of the <c>&lt;</c>, and behind the
    /// <c>&gt;</c> does not refuse the input under this shape
    /// </summary>
    /// <param name="expression">The expression</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-018")]
    [DataRow("typeof(List<int /* c */>)", DisplayName = "Comment in a list with a type")]
    [DataRow("typeof(Dictionary<int, // c\n            string>)", DisplayName = "Line comment behind the comma of a list with types")]
    [DataRow("typeof(Dictionary /* c */ <,>)", DisplayName = "In front of the opening angle bracket")]
    [DataRow("typeof(Dictionary<,> /* c */)", DisplayName = "Behind the closing angle bracket")]
    public void CommentOutsideAListOfOmittedTypeArgumentsIsSupported(string expression)
    {
        AssertSupported(WrapInMethod($"        var t = {expression};"));
    }

    /// <summary>
    /// Verifies that a list inside disabled text is not checked: where <c>X</c> is not defined, the statement is disabled text and
    /// the input is formatted, and where it is defined, the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-018")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ListInDisabledTextIsNotChecked(bool defineX)
    {
        var input = WrapInMethod("#if X\n        var t = typeof(Dictionary</* c */,>);\n#endif");
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;

        if (defineX)
        {
            AssertUnsupportedFor(OmittedTypeArgumentCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies the overlap with FMT-UNSUP-009: a comment in front of a comma on a later line than the <c>&lt;</c> gives the reason
    /// of FMT-UNSUP-009, and the overlap with FMT-UNSUP-004: a comment inside the list in an attribute argument of a field gives
    /// the reason of FMT-UNSUP-004
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [Requirement("FMT-UNSUP-004")]
    [Requirement("FMT-UNSUP-018")]
    public void OverlapWithOtherShapesGivesTheirReason()
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInMethod("        var t = typeof(Dictionary<\n// c\n,>);"), "// c");
        AssertUnsupportedFor(VariableDeclarationCommentReason, "using System;\nusing System.Collections.Generic;\n\nclass C\n{\n    [Obsolete(nameof(Dictionary</* c */,>))]\n    private int _f;\n}", "/* c */");
    }

    /// <summary>
    /// Verifies that the list is one node: a node-level input that holds the list holds the shape, and the type around it does
    /// as well, while an omitted argument alone holds no trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-018")]
    public void NodeThatHoldsTheListHoldsTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(WrapInMethod("        var t = typeof(Dictionary<, // c\n            >);"), endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var list = root.DescendantNodes().OfType<TypeArgumentListSyntax>().Single();

            Assert.AreEqual(OmittedTypeArgumentCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(list)?.Reason);
            Assert.AreEqual(OmittedTypeArgumentCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(list.Parent)?.Reason);
            Assert.AreEqual(OmittedTypeArgumentCommentReason, ReihitsuFormatter.FormatNode(list.Parent, cancellationToken: TestContext.CancellationToken).Unsupported?.Reason);
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(list.Arguments[0]));
        }
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: a wrapped list of omitted type arguments without a comment is joined,
    /// and the output next to a comment behind the list, formatted again, is neither refused nor changed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-018")]
    public void OutputOfAJoinedListIsNotUnsupported()
    {
        AssertRuleResult(WrapInMethod("        var t = typeof(Dictionary<,\n            >); // c"), WrapInMethod("        var t = typeof(Dictionary<,>); // c"));
    }

    /// <summary>
    /// Wraps statements in a method of a class with the usings its lists use
    /// </summary>
    /// <param name="statements">The statements, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statements)
    {
        return $"using System;\nusing System.Collections.Generic;\n\nclass C\n{{\n    void M()\n    {{\n{statements}\n    }}\n}}";
    }

    #endregion // Methods
}