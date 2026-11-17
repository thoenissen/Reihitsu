using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the unsupported shape of a comment, a directive, or disabled text between a conditional operator and its branch
/// (FMT-UNSUP-008): both operators, every kind of comment in both documentation modes, directives, nested conditionals, the
/// excepted places in front of the operator and inside a branch, the operators that are no conditional expression, every entry
/// point, and the closure of the formatter's output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedConditionalOperatorCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> that holds a conditional with a comment behind its <c>?</c>, beside a method <c>N</c> that holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    int M(bool a)
                                                                    {
                                                                        return a ? // c
                                                                            1
                                                                            : 2;
                                                                    }

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: a line comment behind the <c>?</c>, a line comment behind the <c>:</c>, and an inline
    /// block comment behind the <c>?</c> each refuse the input, and the first comment is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void SpecExampleCommentsBehindEitherOperatorAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     var x = a ? // c1
                                         1
                                         : 2;
                                     var y = a
                                                 ? 1 : // c2
                                                 2;
                                     var z = a ? /* c3 */ 1
                                             : 2;

                                     return x;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "// c1");
    }

    /// <summary>
    /// Verifies each conditional of the specification example on its own: the input is refused, and its comment is reported
    /// </summary>
    /// <param name="statement">The statement that holds the conditional</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    [DataRow("        var x = a ? // c1\n            1\n            : 2;", "// c1", DisplayName = "Line comment behind the question mark")]
    [DataRow("        var y = a\n                    ? 1 : // c2\n                    2;", "// c2", DisplayName = "Line comment behind the colon")]
    [DataRow("        var z = a ? /* c3 */ 1\n                : 2;", "/* c3 */", DisplayName = "Inline block comment behind the question mark")]
    [DataRow("        var w = a ? 1 : /* c4 */ 2;", "/* c4 */", DisplayName = "Inline block comment behind the colon")]
    [DataRow("        var v = a ?\n            // c5\n            1 : 2;", "// c5", DisplayName = "Comment line in front of the true branch")]
    [DataRow("        var u = a ? 1 :\n            // c6\n            2;", "// c6", DisplayName = "Comment line in front of the false branch")]
    [DataRow("        var t = a ?/* c7 */1 : 2;", "/* c7 */", DisplayName = "Block comment without whitespace")]
    public void EachCommentBetweenAnOperatorAndItsBranchIsUnsupported(string statement, string comment)
    {
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies the specification example of the one-line and the laid-out conditional with inline comments behind both operators,
    /// which the formatter kept as written before: the input is refused, and the comment in line 5, column 21 is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void InlineCommentsWithTheBranchOnTheOperatorsLineAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     var x = a ? /* yes */ 1 : /* no */ 2;
                                     var y = a
                                                 ? /* yes */ 1
                                                 : /* no */ 2;

                                     return x + y;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/* yes */");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(4, 20), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies the specification example: a directive and disabled text behind the <c>?</c> refuse the input, and the directive
    /// is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void SpecExampleDirectiveAndDisabledTextBehindTheQuestionMarkAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     return a ?
                             #if DEBUG
                                         1
                             #else
                                         2
                             #endif
                                         : 3;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "#if DEBUG");
    }

    /// <summary>
    /// Verifies that a directive behind the <c>:</c> refuses the input, also a directive that holds no disabled text
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DirectiveBehindTheColonIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     return a ? 1 :
                             #pragma warning disable CS0001
                                         2;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "#pragma warning disable CS0001");
    }

    /// <summary>
    /// Verifies that a documentation comment behind an operator refuses the input in every documentation mode, in which it is a
    /// documentation comment or an ordinary comment
    /// </summary>
    /// <param name="statement">The statement that holds the conditional</param>
    /// <param name="comment">The comment that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    [DataRow("        return a ? /** c */ 1 : 2;", "/** c */", DocumentationMode.Parse)]
    [DataRow("        return a ? /** c */ 1 : 2;", "/** c */", DocumentationMode.None)]
    [DataRow("        return a ? /** c */ 1 : 2;", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("        return a ? 1 :\n            /// <summary>c</summary>\n            2;", "/// <summary>c</summary>", DocumentationMode.Parse)]
    [DataRow("        return a ? 1 :\n            /// <summary>c</summary>\n            2;", "/// <summary>c</summary>", DocumentationMode.None)]
    [DataRow("        return a ? 1 :\n            /// <summary>c</summary>\n            2;", "/// <summary>c</summary>", DocumentationMode.Diagnose)]
    public void DocumentationCommentBehindAnOperatorIsUnsupportedInEveryDocumentationMode(string statement, string comment, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInMethod(statement), comment, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that of two comments behind operators the first in the text is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void FirstOfTwoCommentsIsReported()
    {
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInMethod("        return a ? /* first */ 1 : /* second */ 2;"), "/* first */");
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInMethod("        return a ? 1 : /* first */ 2;\n        return a ? /* second */ 1 : 2;"), "/* first */");
    }

    /// <summary>
    /// Verifies that a conditional nested in a branch of another, and one inside a lambda or an interpolation hole in a branch,
    /// is checked with its own operators
    /// </summary>
    /// <param name="statement">The statement that holds the conditionals</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    [DataRow("        return a ? (b ? /* c */ 1 : 2) : 3;", "/* c */", DisplayName = "Nested in the true branch")]
    [DataRow("        return a ? 1 : b ? 2 : // c\n            3;", "// c", DisplayName = "Nested in the false branch")]
    [DataRow("        return a ? F(() => b ? /* c */ 1 : 2) : 3;", "/* c */", DisplayName = "Inside a lambda in a branch")]
    [DataRow("        return $\"{(a ? /* c */ 1 : 2)}\".Length;", "/* c */", DisplayName = "Inside an interpolation hole")]
    public void NestedConditionalIsCheckedWithItsOwnOperators(string statement, string comment)
    {
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment on its own line in front of the <c>?</c>, a comment behind
    /// the condition, a comment behind the true branch, and a comment inside an argument of a branch do not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void SpecExampleCommentsInFrontOfTheOperatorsAreSupported()
    {
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     var x = a
                                                 // c4
                                                 ? 1
                                                 : 2;
                                     var y = a /* c5 */
                                                 ? 1 // c6
                                                 : N(/* c7 */ 2); // c8

                                     return x;
                                 }

                                 int N(int v) => v;
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that the excepted places and the operators that are no conditional expression do not refuse the input: a comment
    /// in front of an operator, inside a branch behind its first token, behind a <c>ref</c> of a branch, and behind a nullable
    /// type's <c>?</c> and a null-conditional operator, and in front of the null-coalescing operator, behind which a comment is
    /// refused as a comment behind a binary operator (FMT-UNSUP-011)
    /// </summary>
    /// <param name="statement">The statement</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    [DataRow("        var x = a /* c */ ? 1 : 2;", DisplayName = "Behind the condition")]
    [DataRow("        var x = a ? 1 /* c */ : 2;", DisplayName = "Behind the true branch")]
    [DataRow("        var x = a // c\n            ? 1\n            : 2;", DisplayName = "Line comment behind the condition")]
    [DataRow("        var x = a\n            ? 1\n            // c\n            : 2;", DisplayName = "Comment line in front of the colon")]
    [DataRow("        var x = a ? F(/* c */ 1) : 2;", DisplayName = "Inside the true branch behind its first token")]
    [DataRow("        var x = a ? (/* c */ 1) : 2;", DisplayName = "Inside parentheses of the branch")]
    [DataRow("        var n = (int? /* c */)null;", DisplayName = "Nullable type")]
    [DataRow("        var l = s?. /* c */ Length;", DisplayName = "Null-conditional member access")]
    [DataRow("        var e = s?[/* c */ 0];", DisplayName = "Null-conditional element access")]
    [DataRow("        var m = s /* c */ ?? \"\";", DisplayName = "Null-coalescing operator")]
    public void CommentOutsideTheShapeIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies that a comment behind the <c>ref</c> of a <c>ref</c> branch stands behind the branch's first token and does not
    /// refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void CommentBehindTheRefOfARefBranchIsSupported()
    {
        const string input = """
                             class C
                             {
                                 int _x;
                                 int _y;

                                 ref int M(bool a)
                                 {
                                     return ref a ? ref /* c */ _x : ref _y;
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment behind the <c>?</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public async Task FormatDocumentAsyncRefusesTheShape()
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
    /// Verifies that the node-level entry points refuse a target that holds the conditional, and format the method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public async Task NodeLevelEntryPointsRefuseATargetThatHoldsTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(RefusedMethodBesideFormattableMethod, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var type = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

                var nodeResult = ReihitsuFormatter.FormatNode(FindMethod(root, "M"), cancellationToken: TestContext.CancellationToken);

                Assert.AreEqual(FindMethod(root, "M").ToFullString(), nodeResult.Output.ToFullString());
                AssertReportsComment(nodeResult.Unsupported, text);

                var targetResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, FindMethod(root, "M"), TestContext.CancellationToken);

                Assert.AreEqual(text, (await targetResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(targetResult.Unsupported, text);

                var contextResult = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, FindMethod(root, "N"), type, TestContext.CancellationToken);

                Assert.AreEqual(text, (await contextResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(contextResult.Unsupported, text);

                var besideResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, FindMethod(root, "N"), TestContext.CancellationToken);

                Assert.IsNull(besideResult.Unsupported);
                Assert.AreNotEqual(text, (await besideResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString(), "The method beside the shape must be formatted.");
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for a node that holds the conditional,
    /// and nothing for a node that holds only part of it: the true branch, whose leading trivia holds the comment, and the
    /// condition
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    [Requirement("FMT-UNSUP-003")]
    public void FindUnsupportedSyntaxAnswersForTheConditionalAndNotForPartOfIt()
    {
        const string input = "class C\n{\n    int M(bool a)\n    {\n        return a ?\n            // c\n            1 : 2;\n    }\n}";
        var root = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
        var conditional = root.DescendantNodes().OfType<ConditionalExpressionSyntax>().Single();

        AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(conditional), input);
        AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root), input);
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(conditional.WhenTrue));
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(conditional.Condition));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same tree
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    [DataRow("class C\n{\n    int M(bool a) => a ? /* c */ 1 : 2;\n}")]
    [DataRow("class C\n{\n    int M(bool a) => a\n        // c\n        ? 1\n        : 2;\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: inputs with comments in front of the operators, which the line-break
    /// phase moves onto lines of their own or joins, are formatted, and their output, formatted again, is neither refused nor
    /// changed
    /// </summary>
    /// <param name="statement">The statement that holds the conditional</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-008")]
    [DataRow("        var x = a /* c1 */ ?\n            1 : 2;", DisplayName = "Comment in front of a question mark that ends its line")]
    [DataRow("        var x = a ? 1 /* c2 */ :\n            2;", DisplayName = "Comment in front of a colon that ends its line")]
    [DataRow("        var x = a ? b /* c3 */ ? 1 : 2 : 3;", DisplayName = "Comment in front of a nested question mark")]
    [DataRow("        var x = a ? 1\n            // c4\n            : 2;", DisplayName = "Comment line in front of the colon")]
    [DataRow("        var x = a // c5\n            ? 1 : 2;", DisplayName = "Line comment behind the condition")]
    [DataRow("        var x = a ? 1 // c6\n            : 2;", DisplayName = "Line comment behind the true branch")]
    [DataRow("        var x = a /** c7 */ ? 1 : 2;", DisplayName = "Delimited documentation comment in front of the question mark")]
    [DataRow("        var x = a ? 1 /** c8 */ : 2;", DisplayName = "Delimited documentation comment in front of the colon")]
    public void OutputOfAFormattedConditionalIsNotUnsupported(string statement)
    {
        var input = WrapInMethod(statement);

        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var first = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(first.Unsupported, $"The input must be formatted under {endingName} line endings.");

            var output = first.Output.ToString();
            var second = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(output, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(second.Unsupported, $"The formatted output must not be refused under {endingName} line endings:{endOfLine}{output}");
            Assert.AreEqual(output, second.Output.ToString(), $"The formatted output must be stable under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Wraps a statement in a method <c>M</c> of a class <c>C</c> with the parameters and members its conditionals use
    /// </summary>
    /// <param name="statement">The statement, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statement)
    {
        return $"class C\n{{\n    int M(bool a, bool b, string s)\n    {{\n{statement}\n\n        return 0;\n    }}\n\n    int F(System.Func<int> f) => f();\n\n    int F(int v) => v;\n}}";
    }

    /// <summary>
    /// Asserts that the outcome reports the reason of FMT-UNSUP-008 and the span of the comment <c>// c</c> in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(ConditionalOperatorCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, text, "// c", 0, "the document's");
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