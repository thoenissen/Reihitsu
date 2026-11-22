using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the unsupported shape of a comment, a directive, or disabled text between a list element and the comma behind it,
/// with the comma on a later line of the text (FMT-UNSUP-009): every kind of list, every kind of comment in every documentation
/// mode, directives and disabled text in both builds, a multi-line comment, an omitted element, the excepted places behind the
/// comma and on the element's line, commas that are no token of the code, the overlap with a variable declaration, every entry
/// point, and the closure of the formatter's output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedListSeparatorCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> whose argument list holds a comment in front of a comma on a later line, beside a method <c>N</c> that
    /// holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    void M()
                                                                    {
                                                                        Foo(1 // c
                                                                            , 2);
                                                                    }

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: a parameter list, a constraint clause, an argument list with a comment line, an
    /// object initializer, and an argument list with a multi-line comment each hold a comment in front of a comma on a later
    /// line; the input is refused, and the first comment is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    public void SpecExampleCommentsInFrontOfCommasOnLaterLinesAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M<T>(int a // c1
                                           , int b)
                                     where T : class /* c2 */
                                               , new()
                                 {
                                     Foo(1
                                         // c3
                                         , 2);
                                     var e = new D { X = 3 // c4
                                         , Y = 4 };
                                     Foo(1 /* a
                                         b */, 2);
                                 }
                             }
                             """;

        AssertUnsupportedFor(ListSeparatorCommentReason, input, "// c1");
    }

    /// <summary>
    /// Verifies each list of the specification example on its own: the input is refused, and its comment is reported
    /// </summary>
    /// <param name="member">The member that holds the list, indented by four spaces</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [DataRow("    void M(int a // c1\n           , int b)\n    {\n    }", "// c1", DisplayName = "Parameter list")]
    [DataRow("    void M<T>()\n        where T : class /* c2 */\n                  , new()\n    {\n    }", "/* c2 */", DisplayName = "Constraint clause")]
    [DataRow("    void M()\n    {\n        Foo(1\n            // c3\n            , 2);\n    }", "// c3", DisplayName = "Comment line in front of an argument comma")]
    [DataRow("    void M()\n    {\n        var e = new D { X = 3 // c4\n            , Y = 4 };\n    }", "// c4", DisplayName = "Object initializer")]
    [DataRow("    void M()\n    {\n        Foo(1 /* a\n            b */, 2);\n    }", "/* a\n            b */", DisplayName = "Multi-line comment that puts the comma on a later line")]
    public void EachListOfTheSpecExampleIsUnsupported(string member, string comment)
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInClass(member), comment);
    }

    /// <summary>
    /// Verifies that the shape is refused in every kind of list a statement can hold, separators and trailing commas alike
    /// </summary>
    /// <param name="statement">The statement that holds the list, indented by eight spaces</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [DataRow("        Foo(1, 2 // c\n            , 3);", "// c", DisplayName = "Argument list, last comma")]
    [DataRow("        Foo(1 /* c */\n            , 2);", "/* c */", DisplayName = "Argument list, block comment behind the element")]
    [DataRow("        Foo(1\n            /* c */\n            , 2);", "/* c */", DisplayName = "Argument list, block comment on its own line")]
    [DataRow("        var o = new D(1 // c\n            , 2);", "// c", DisplayName = "Object creation arguments")]
    [DataRow("        var e = x[1 // c\n            , 2];", "// c", DisplayName = "Element access")]
    [DataRow("        var t = (1 // c\n            , 2);", "// c", DisplayName = "Tuple expression")]
    [DataRow("        (int A // c\n            , int B) t = default;", "// c", DisplayName = "Tuple type")]
    [DataRow("        var k = new List<int> { 1 // c\n            , 2 };", "// c", DisplayName = "Collection initializer")]
    [DataRow("        var n = new D { Items = { 1 // c\n            , 2 } };", "// c", DisplayName = "Collection initializer assigned to a member")]
    [DataRow("        var g = new Dictionary<int, int> { { 1 // c\n            , 2 } };", "// c", DisplayName = "Complex element initializer")]
    [DataRow("        var g = new Grid { [1 // c\n            , 2] = 3 };", "// c", DisplayName = "Dictionary key")]
    [DataRow("        int[] a = { 1 // c\n            , 2 };", "// c", DisplayName = "Array initializer")]
    [DataRow("        var w = new[] { 1 // c\n            , 2 };", "// c", DisplayName = "Implicitly typed array initializer")]
    [DataRow("        var h = new { A = 1 // c\n            , B = 2 };", "// c", DisplayName = "Anonymous object")]
    [DataRow("        var q = p with { X = 1 // c\n            , Y = 2 };", "// c", DisplayName = "With initializer")]
    [DataRow("        int[] d = [1 // c\n            , 2];", "// c", DisplayName = "Collection expression")]
    [DataRow("        var b = o is [1 // c\n            , 2];", "// c", DisplayName = "List pattern")]
    [DataRow("        var b = o is D { X: 1 // c\n            , Y: 2 };", "// c", DisplayName = "Property pattern")]
    [DataRow("        var b = o is (1 // c\n            , 2);", "// c", DisplayName = "Positional pattern")]
    [DataRow("        var s = o switch { 1 => 2 // c\n            , _ => 3 };", "// c", DisplayName = "Switch expression arms")]
    [DataRow("        for (i = 0 // c\n            , j = 0; i < 1; i++)\n        {\n        }", "// c", DisplayName = "For expression list")]
    [DataRow("        Func<int, int, int> f = (int a // c\n            , int b) => a;", "// c", DisplayName = "Parenthesized lambda parameters")]
    [DataRow("        Func<int, int, int> f = (a // c\n            , b) => a;", "// c", DisplayName = "Implicitly typed lambda parameters")]
    [DataRow("        Func<int, int, int> f = delegate(int a // c\n            , int b) { return a; };", "// c", DisplayName = "Anonymous method parameters")]
    [DataRow("        void L(int a // c\n            , int b)\n        {\n        }", "// c", DisplayName = "Local function parameters")]
    [DataRow("        var t = typeof(Dictionary<int // c\n            , string>);", "// c", DisplayName = "Type argument list")]
    [DataRow("        var t = typeof(Dictionary<\n// c\n,>);", "// c", DisplayName = "Omitted type argument behind a comment line")]
    [DataRow("        var t = typeof(Dictionary< // omitted\n                ,>);", "// omitted", DisplayName = "Omitted type argument behind a comment on the opening line")]
    [DataRow("        int[] a = new[] { 1,\n            2 /* two */\n            , };", "/* two */", DisplayName = "Trailing comma of an array initializer")]
    [DataRow("        var e = new D { X = 3 // k\n            , };", "// k", DisplayName = "Trailing comma of an object initializer")]
    [DataRow("        Foo(1\n#pragma warning disable CS0168\n            , 2);", "#pragma warning disable CS0168", DisplayName = "Pragma directive in front of an argument comma")]
    public void ShapeIsUnsupportedInEveryListOfAStatement(string statement, string comment)
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies that the shape is refused in every kind of list a member or a type declaration can hold
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [DataRow("class C\n{\n    C(int a // c\n      , int b)\n    {\n    }\n}", "// c", DisplayName = "Constructor parameters")]
    [DataRow("class C\n{\n    int this[int a // c\n             , int b] => a;\n}", "// c", DisplayName = "Indexer parameters")]
    [DataRow("record R(int A // c\n         , int B);", "// c", DisplayName = "Primary constructor parameters")]
    [DataRow("class C : B\n{\n    C()\n        : base(1 // c\n               , 2)\n    {\n    }\n}", "// c", DisplayName = "Constructor initializer arguments")]
    [DataRow("class C\n{\n    void M<T // c\n           , U>()\n    {\n    }\n}", "// c", DisplayName = "Type parameter list")]
    [DataRow("class C\n{\n    [A(1 // c\n       , 2)]\n    void M()\n    {\n    }\n}", "// c", DisplayName = "Attribute argument list")]
    [DataRow("class C\n{\n    [A // c\n     , B]\n    void M()\n    {\n    }\n}", "// c", DisplayName = "Attribute list")]
    [DataRow("unsafe class C\n{\n    delegate*<int // c\n              , void> _f;\n}", "// c", DisplayName = "Function pointer parameters")]
    [DataRow("class K : IA // c\n    , IB\n{\n}", "// c", DisplayName = "Base list")]
    [DataRow("enum E\n{\n    A // c\n    , B\n}", "// c", DisplayName = "Enum members")]
    [DataRow("enum E\n{\n    A,\n    B /* about B */\n    ,\n}", "/* about B */", DisplayName = "Trailing comma of an enum")]
    public void ShapeIsUnsupportedInEveryListOfADeclaration(string input, string comment)
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, input, comment);
    }

    /// <summary>
    /// Verifies that a documentation comment in front of a comma on a later line refuses the input in every documentation mode,
    /// in which it is a documentation comment or an ordinary comment
    /// </summary>
    /// <param name="statement">The statement that holds the list</param>
    /// <param name="comment">The comment that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [DataRow("        Foo(1\n            /// c\n            , 2);", "/// c", DocumentationMode.Parse)]
    [DataRow("        Foo(1\n            /// c\n            , 2);", "/// c", DocumentationMode.None)]
    [DataRow("        Foo(1\n            /// c\n            , 2);", "/// c", DocumentationMode.Diagnose)]
    [DataRow("        Foo(1\n            /** c */\n            , 2);", "/** c */", DocumentationMode.Parse)]
    [DataRow("        Foo(1\n            /** c */\n            , 2);", "/** c */", DocumentationMode.None)]
    [DataRow("        Foo(1\n            /** c */\n            , 2);", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("        Foo(1 /** c */\n            , 2);", "/** c */", DocumentationMode.Parse)]
    [DataRow("        Foo(1 /** c */\n            , 2);", "/** c */", DocumentationMode.None)]
    [DataRow("        Foo(1 /** c */\n            , 2);", "/** c */", DocumentationMode.Diagnose)]
    public void DocumentationCommentInFrontOfACommaIsUnsupportedInEveryDocumentationMode(string statement, string comment, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInMethod(statement), comment, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies the specification example of a directive: the input is refused in both builds of <c>DEBUG</c>, and the
    /// <c>#if DEBUG</c> directive is reported, which stands in front of the comma of <c>, c</c> where <c>DEBUG</c> is defined and
    /// in front of the comma of <c>, d</c> where it is not
    /// </summary>
    /// <param name="defineDebug">Whether the build defines <c>DEBUG</c></param>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    public void SpecExampleDirectiveInFrontOfACommaIsUnsupportedInEveryBuild(bool defineDebug)
    {
        const string input = """
                             class C
                             {
                                 void M(int a, int b, int c, int d)
                                 {
                                     M(a, b
                             #if DEBUG
                                         , c
                             #else
                                         , d
                             #endif
                                         );
                                 }
                             }
                             """;

        var parseOptions = defineDebug ? CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG") : CSharpParseOptions.Default;

        AssertUnsupportedFor(ListSeparatorCommentReason, input, "#if DEBUG", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that a comma whose branch the build defines is code, so the directive in front of it refuses the input, while the
    /// same text is formatted in a build that does not define the symbol
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    public void CommaInADefinedBranchIsUnsupportedAndInDisabledTextIsNot()
    {
        var input = WrapInMethod("        Foo(1\n#if NEVER\n            , 2\n#endif\n            );");

        AssertUnsupportedFor(ListSeparatorCommentReason, input, "#if NEVER", parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("NEVER"));
        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that disabled text and the directives around it in front of a comma refuse the input, and the first directive is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    public void DisabledTextInFrontOfACommaIsUnsupported()
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInClass("    void M(int a\n#if NEVER\n           , int unused\n#endif\n           , int b)\n    {\n    }"), "#if NEVER");
    }

    /// <summary>
    /// Verifies that a directive that holds the element in front of a comma, with the comma behind its <c>#endif</c>, refuses the
    /// input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    public void EndIfDirectiveInFrontOfACommaIsUnsupported()
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInClass("    void M(\n#if true\n           int a\n#endif\n           ,\n           int b)\n    {\n    }"), "#endif");
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment behind a comma, a comment in front of a comma on the
    /// element's line, comma-first lists without comments, a comma at the end of its line in front of a directive, and a comma
    /// in disabled text do not refuse the input, in both builds of <c>X</c>
    /// </summary>
    /// <param name="defineX">Whether the build defines <c>X</c></param>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [DataRow(true, DisplayName = "X defined")]
    [DataRow(false, DisplayName = "X not defined")]
    public void SpecExampleCommentsAndDirectivesOutsideTheShapeAreSupported(bool defineX)
    {
        const string input = """
                             enum E
                             {
                                 A,
                             #if X
                                 B,
                             #endif
                                 C,
                             }

                             class C
                             {
                                 void M()
                                 {
                                     Foo(true /* force */, false);
                                     Foo(1
                                         , /* c */ 2
                                         , // c
                                         3);
                                     Foo(1, // c
                                         2);
                                     int[] d = [1
                                         , 2];
                                     Foo(1
                             #if NEVER
                                         , 2
                             #endif
                                         );
                                 }
                             }
                             """;

        AssertSupported(input, defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : null);
    }

    /// <summary>
    /// Verifies that comments and directives outside the gap in front of a comma on a later line do not refuse the input
    /// </summary>
    /// <param name="statement">The statement</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [DataRow("        var a = new[] { 0x01 /* SOH */, 0x02 /* STX */ };", DisplayName = "Block comments in front of commas on the elements' line")]
    [DataRow("        Foo(1 /* one */, 2,\n            3);", DisplayName = "Block comment in front of a comma on the element's line of a wrapped list")]
    [DataRow("        Foo(1\n            , 2);", DisplayName = "Comma-first argument list without a comment")]
    [DataRow("        Foo(1\n            ,\n            // c\n            2);", DisplayName = "Comment line behind a comma alone on its line")]
    [DataRow("        Foo(1, /* a\n            b */ 2);", DisplayName = "Multi-line comment behind a comma")]
    [DataRow("        var t = typeof(Dictionary<\n,>);", DisplayName = "Omitted type argument without a comment")]
    [DataRow("        int[] a = { 1,\n#if X\n            2,\n#endif\n            3 };", DisplayName = "Comma at the end of its line in front of a directive")]
    [DataRow("        Foo(a, // c\n            b);", DisplayName = "Line comment behind a comma")]
    public void CommentsOutsideTheShapeAreSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
        AssertSupported(WrapInMethod(statement), CSharpParseOptions.Default.WithPreprocessorSymbols("X"));
    }

    /// <summary>
    /// Verifies that a comment behind the comma of an omitted type argument is not this shape: the input is refused under the
    /// shape of a comment inside a list of omitted type arguments, with that shape's reason
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [Requirement("FMT-UNSUP-018")]
    public void CommentBehindTheCommaOfAnOmittedTypeArgumentGivesTheReasonOfTheOmittedTypeArgumentList()
    {
        AssertUnsupportedFor(OmittedTypeArgumentCommentReason, WrapInMethod("        var t = typeof(Dictionary<, // c\n>);"), "// c");
    }

    /// <summary>
    /// Verifies that a comma inside a documentation comment's <c>cref</c> and a comma inside a directive are no tokens of the code
    /// and do not refuse the input, also behind a line break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    public void CommasInsideDocumentationCommentsAndDirectivesAreSupported()
    {
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// See <see cref="M(int,
                                 /// int)"/>
                                 /// </summary>
                                 void M(int a, int b)
                                 {
                             #pragma warning disable CS0168
                             #pragma warning restore CS0168
                                 }

                             #pragma warning disable CS0168, CS0219
                                 void N()
                                 {
                                 }
                             #pragma warning restore CS0168, CS0219
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that of two comments in front of commas on later lines the first in the text is reported, also when both stand
    /// in one gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    public void FirstOfTwoCommentsIsReported()
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInMethod("        Foo(1 /* first */\n            , 2);\n        Foo(1 /* second */\n            , 2);"), "/* first */");
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInMethod("        Foo(1 /* first */ // second\n            , 2);"), "/* first */");
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInMethod("        Foo(1 // first\n            /* second */ , 2);"), "// first");
    }

    /// <summary>
    /// Verifies that a comment in front of a comma between two declarators keeps the reason and the location of FMT-UNSUP-004,
    /// while a directive there, which FMT-UNSUP-004 does not cover, is refused with the reason of FMT-UNSUP-009
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [Requirement("FMT-UNSUP-004")]
    public void DeclaratorCommaKeepsTheReasonOfAVariableDeclarationForAComment()
    {
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInMethod("        int a = 1 // c\n            , b = 2;"), "// c");
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInClass("    private int _a = 1 // c\n        , _b = 2;"), "// c");
        AssertUnsupportedFor(ListSeparatorCommentReason,
                             WrapInMethod("        int a = 1\n#if X\n            , b = 2\n#endif\n            ;"),
                             "#if X",
                             parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("X"));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment in front of the comma
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [Requirement("FMT-UNSUP-002")]
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
    /// Verifies that the node-level entry points refuse a target that holds the comma, and format the method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [Requirement("FMT-UNSUP-002")]
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
                var argumentList = root.DescendantNodes().OfType<ArgumentListSyntax>().Single();

                var nodeResult = ReihitsuFormatter.FormatNode(FindMethod(root, "M"), cancellationToken: TestContext.CancellationToken);

                Assert.AreEqual(FindMethod(root, "M").ToFullString(), nodeResult.Output.ToFullString());
                AssertReportsComment(nodeResult.Unsupported, text);

                var targetResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, FindMethod(root, "M"), TestContext.CancellationToken);

                Assert.AreEqual(text, (await targetResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(targetResult.Unsupported, text);

                var listResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, argumentList, TestContext.CancellationToken);

                Assert.AreEqual(text, (await listResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(listResult.Unsupported, text);

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
    /// Verifies that a node-level target that holds the element in front of the comma, with the comment in its trivia, but not
    /// the comma, is not refused
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [Requirement("FMT-UNSUP-003")]
    public async Task ElementTargetWithoutTheCommaIsNotRefused()
    {
        using (var workspace = new AdhocWorkspace())
        {
            var document = CreateDocument(workspace, RefusedMethodBesideFormattableMethod);
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
            var firstArgument = root.DescendantNodes().OfType<ArgumentSyntax>().First();

            var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, firstArgument, TestContext.CancellationToken);

            Assert.IsNull(result.Unsupported);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for a node that holds the comma,
    /// and nothing for a node that holds only part of the gap: the element whose trailing trivia holds the comment, and the
    /// element behind the comma
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-007")]
    public void FindUnsupportedSyntaxAnswersForTheListAndNotForPartOfIt()
    {
        var root = CSharpSyntaxTree.ParseText(RefusedMethodBesideFormattableMethod, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
        var argumentList = root.DescendantNodes().OfType<ArgumentListSyntax>().Single();

        AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(argumentList), RefusedMethodBesideFormattableMethod);
        AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root), RefusedMethodBesideFormattableMethod);
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(argumentList.Arguments[0]));
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(argumentList.Arguments[1]));
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(FindMethod(root, "N")));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same tree
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-009")]
    [Requirement("FMT-UNSUP-007")]
    [DataRow("class C\n{\n    void M(int a // c\n           , int b)\n    {\n    }\n}")]
    [DataRow("enum E\n{\n    A\n    /* c */\n    , B\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        Foo(1\n            , /* c */ 2);\n    }\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: inputs whose commas the phases move, with and without a comment
    /// behind them, and whose trailing commas they remove, are formatted, and their output, formatted again, is neither refused
    /// nor changed
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-009")]
    [DataRow("class C\n{\n    void M()\n    {\n        Foo(1\n            , /* c */ 2, 3);\n        Foo(1\n            , // c\n            2, 3);\n    }\n}", DisplayName = "Argument commas with comments behind them")]
    [DataRow("class C\n{\n    void M(int a\n           , /* c */ int b\n           , int c)\n    {\n    }\n}", DisplayName = "Parameter commas")]
    [DataRow("using System;\n\nclass C\n{\n    void M<T>()\n        where T : class\n            , /* c */ new()\n            , IDisposable\n    {\n    }\n}", DisplayName = "Constraint commas")]
    [DataRow("class C\n{\n    void M()\n    {\n        var b = new D { X = 3\n            , /* c */ Y = 4\n            , // d\n            Z = 5 };\n        int[] y = { 1\n            , 2 };\n    }\n}", DisplayName = "Initializer commas")]
    [DataRow("enum E { A,\nB\n    , // comma\n}\n\nclass C\n{\n    int[] _a = new[] { 1,\n2\n, // comma\n};\n}", DisplayName = "Removed trailing commas with comments behind them")]
    [DataRow("class C\n{\n    void M()\n    {\n        var e = new D\n                {\n                    X = 3\n                    ,\n                };\n    }\n}", DisplayName = "Removed trailing comma alone on its line")]
    public void OutputOfFormattedCommasIsNotUnsupported(string input)
    {
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
    /// Wraps a member in a class <c>C</c>
    /// </summary>
    /// <param name="member">The member, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string member)
    {
        return $"using System;\nusing System.Collections.Generic;\n\nclass C\n{{\n{member}\n}}";
    }

    /// <summary>
    /// Wraps a statement in a method <c>M</c> of a class <c>C</c>
    /// </summary>
    /// <param name="statement">The statement, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statement)
    {
        return WrapInClass($"    void M(object o, P p, int[,] x, int i, int j)\n    {{\n{statement}\n    }}");
    }

    /// <summary>
    /// Asserts that the outcome reports the reason of FMT-UNSUP-009 and the span of the comment <c>// c</c> in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(ListSeparatorCommentReason, unsupported.Reason);
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