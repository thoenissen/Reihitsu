using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the unsupported shape of a comment, a directive, or disabled text in front of an argument list, an element access, a
/// conditional element access, or a null-forgiving operator, or between the <c>?</c> and the <c>[</c> of a conditional element
/// access (FMT-UNSUP-014): every owner of such a part, every kind of comment in every documentation mode, directives and disabled
/// text in every build, every position in the gap with and without a line break, every enclosing construct, the parentheses,
/// brackets, question marks, and exclamation marks that start no such part, the places inside and behind a part, the shapes that
/// keep their own reason, every entry point, node-level inputs that hold one side of a gap, and the closure of the formatter's
/// output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedAttachedPartCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> that holds an element access below a comment behind its element, beside a method <c>N</c> that holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    int M(int[] arr)
                                                                    {
                                                                        return arr // c
                                                                            [0];
                                                                    }

                                                                    void N(){ }
                                                                }
                                                                """;

    /// <summary>
    /// A method <c>M</c> that holds an invocation with a block comment between the invoked name and its argument list
    /// </summary>
    private const string InvocationWithCommentInFrontOfItsArgumentList = """
                                                                         class C
                                                                         {
                                                                             int M()
                                                                             {
                                                                                 return Foo /* c */ (1);
                                                                             }

                                                                             int Foo(int x)
                                                                             {
                                                                                 return x;
                                                                             }
                                                                         }
                                                                         """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: each part is refused, the input is returned unchanged, and the first comment, which
    /// starts in line 7, column 21, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void SpecExampleEachPartIsUnsupported()
    {
        const string input = """
                             #nullable enable

                             class C
                             {
                                 int M(int[] arr, int[]? n, string? s)
                                 {
                                     var a = arr // c1
                                         [0];
                                     var b = Foo /* c2 */ (1);
                                     var c = n
                                         // c3
                                         ?[0];
                                     var d = s /* c4 */ !;
                                     var e = n? // c5
                                         [0];

                                     return a + b + (c ?? 0) + d.Length + (e ?? 0);
                                 }

                                 int Foo(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        AssertUnsupportedFor(AttachedPartCommentReason, input, "// c1");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(6, 20), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies each part of the specification example on its own: the input is refused, and its comment is reported
    /// </summary>
    /// <param name="statement">The statement that holds the part</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        var a = arr // c1\n            [0];", "// c1", DisplayName = "Line comment in front of an element access")]
    [DataRow("        var b = Foo /* c2 */ (1);", "/* c2 */", DisplayName = "Block comment in front of an argument list on one line")]
    [DataRow("        var c = n\n            // c3\n            ?[0];", "// c3", DisplayName = "Comment line in front of a conditional element access")]
    [DataRow("        var d = s /* c4 */ !;", "/* c4 */", DisplayName = "Block comment in front of a null-forgiving operator on one line")]
    [DataRow("        var e = n? // c5\n            [0];", "// c5", DisplayName = "Line comment between the question mark and the bracket")]
    public void EachPartOfTheSpecExampleIsUnsupported(string statement, string comment)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies the specification example: a conditional group that chooses between two argument lists refuses the input in every
    /// build, and its <c>#if DEBUG</c>, which starts in line 6, column 1, is reported
    /// </summary>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void SpecExampleDirectiveInFrontOfAnArgumentListIsUnsupportedInEveryBuild(bool defineDebug)
    {
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return Foo
                             #if DEBUG
                                         (1);
                             #else
                                         (2);
                             #endif
                                 }

                                 int Foo(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;
        var parseOptions = CreateOptions(defineDebug, "DEBUG");

        AssertUnsupportedFor(AttachedPartCommentReason, input, "#if DEBUG", parseOptions: parseOptions);

        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(5, 0), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies the specification example of the other side: comments in front of an attribute's argument list, above a statement,
    /// inside brackets and parentheses, behind an argument list, in front of a <c>!</c> that starts a link operator, inside a
    /// parenthesized expression and a collection expression, and in front of a parameter list do not refuse the input, and the
    /// input is formatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void SpecExampleCommentsOutsideTheGapsAreSupported()
    {
        const string input = """
                             #nullable enable

                             using System;

                             class C
                             {
                                 [Obsolete /* c1 */ ("x")]
                                 int M(int[] arr, int[]? n, string? s)
                                 {
                                     // c2
                                     var a = arr[/* c3 */ 0];
                                     var b = Foo(/* c4 */ 1); // c5
                                     var c = n?[/* c6 */ 0];
                                     var d = s // c7
                                             !.Length;
                                     var e = (/* c8 */ 1);
                                     int[] f = [/* c9 */ 1];

                                     return a + b + (c ?? 0) + d + e + f[0];
                                 }

                                 int Foo /* c10 */ (int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment in front of the argument list of every owner FMT-BREAK-039 names refuses the input: an invocation of a
    /// name, of a generic name, of a parenthesized expression, of an invocation, and of <c>nameof</c>, an object creation, an
    /// implicit object creation, a <c>base</c> and a <c>this</c> constructor initializer, a primary-constructor base type, and the
    /// <c>with(…)</c> element of a collection expression
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("class C\n{\n    void M()\n    {\n        Foo /* c */ (1);\n    }\n\n    void Foo(int x)\n    {\n    }\n}", DisplayName = "Invocation of a name")]
    [DataRow("class C\n{\n    void M()\n    {\n        Foo<int> /* c */ (1);\n    }\n\n    void Foo<T>(T x)\n    {\n    }\n}", DisplayName = "Invocation of a generic name")]
    [DataRow("using System;\n\nclass C\n{\n    void M(Func<int, int> f, Func<int, int> g)\n    {\n        (f ?? g) /* c */ (1);\n    }\n}", DisplayName = "Invocation of a parenthesized expression")]
    [DataRow("using System;\n\nclass C\n{\n    void M()\n    {\n        Make(1) /* c */ (2);\n    }\n\n    Func<int, int> Make(int x)\n    {\n        return y => y;\n    }\n}", DisplayName = "Invocation of an invocation")]
    [DataRow("class C\n{\n    void M()\n    {\n        Use(nameof /* c */ (C));\n    }\n\n    void Use(string s)\n    {\n    }\n}", DisplayName = "Invocation of nameof")]
    [DataRow("using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        Use(new List<int> /* c */ (4));\n    }\n\n    void Use(object o)\n    {\n    }\n}", DisplayName = "Object creation")]
    [DataRow("using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        List<int> k;\n\n        k = new /* c */ (4);\n    }\n}", DisplayName = "Implicit object creation")]
    [DataRow("class B\n{\n    public B(int x)\n    {\n    }\n}\n\nclass D : B\n{\n    public D()\n        : base /* c */ (1)\n    {\n    }\n}", DisplayName = "Base constructor initializer")]
    [DataRow("class D\n{\n    public D()\n        : this /* c */ (1)\n    {\n    }\n\n    public D(int x)\n    {\n    }\n}", DisplayName = "This constructor initializer")]
    [DataRow("class B(int x)\n{\n}\n\nclass E() : B /* c */ (2);", DisplayName = "Primary-constructor base type")]
    [DataRow("using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        Use([with /* c */ (4), 1]);\n    }\n\n    void Use(HashSet<int> s)\n    {\n    }\n}", DisplayName = "With element of a collection expression")]
    public void CommentInFrontOfTheArgumentListOfEveryOwnerIsUnsupported(string input)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment in front of the <c>[</c> of every element access refuses the input: behind a name, <c>this</c>,
    /// <c>base</c>, a parenthesized expression, another element access, an invocation, and a pointer
    /// </summary>
    /// <param name="statement">The statement that holds the element access</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(arr /* c */ [0]);", DisplayName = "Behind a name")]
    [DataRow("        Use(this /* c */ [0]);", DisplayName = "Behind this")]
    [DataRow("        Use(base /* c */ [0]);", DisplayName = "Behind base")]
    [DataRow("        Use((arr) /* c */ [0]);", DisplayName = "Behind a parenthesized expression")]
    [DataRow("        Use(d[\"k\"] /* c */ [0]);", DisplayName = "Behind an element access")]
    [DataRow("        Use(Get() /* c */ [0]);", DisplayName = "Behind an invocation")]
    [DataRow("        Use(p /* c */ [0]);", DisplayName = "Behind a pointer")]
    public void CommentInFrontOfEveryElementAccessIsUnsupported(string statement)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies that a comment in front of the <c>?</c> of a conditional element access, also behind a null-forgiving operator, and
    /// a comment between its <c>?</c> and its <c>[</c>, also behind a null-forgiving operator and on a line of its own, refuse the
    /// input
    /// </summary>
    /// <param name="statement">The statement that holds the conditional element access</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(n /* c */ ?[0]);", "/* c */", DisplayName = "In front of the question mark")]
    [DataRow("        Use(x! /* c */ ?[0]);", "/* c */", DisplayName = "In front of the question mark behind a null-forgiving operator")]
    [DataRow("        Use(n /* c */ ? [0]);", "/* c */", DisplayName = "In front of a question mark that is apart from its bracket")]
    [DataRow("        Use(n? /* c */ [0]);", "/* c */", DisplayName = "Between the question mark and the bracket")]
    [DataRow("        Use(x!? /* c */ [0]);", "/* c */", DisplayName = "Between the question mark and the bracket behind a null-forgiving operator")]
    [DataRow("        Use(n? // c\n            [0]);", "// c", DisplayName = "Line comment behind the question mark")]
    [DataRow("        Use(n?\n            // c\n            [0]);", "// c", DisplayName = "Comment line between the question mark and the bracket")]
    [DataRow("        Use(n?\n            /* c */ [0]);", "/* c */", DisplayName = "Block comment in front of the bracket on its line")]
    [DataRow("        Use(d[\"k\"]?[0] /* c */ ?[1]);", "/* c */", DisplayName = "In front of the second of two conditional element accesses")]
    public void CommentInAGapOfAConditionalElementAccessIsUnsupported(string statement, string comment)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies that a comment in front of a null-forgiving operator refuses the input whatever follows the operator: a semicolon,
    /// a binary operator, a conditional element access, an element access, an argument list, a closing parenthesis, and another
    /// null-forgiving operator, also behind an invocation in a chain
    /// </summary>
    /// <param name="statement">The statement that holds the null-forgiving operator</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(Get() /* c */ !);", "/* c */", DisplayName = "Behind an invocation")]
    [DataRow("        Use(s // c\n            ! + \"\");", "// c", DisplayName = "In front of a binary operator")]
    [DataRow("        Use(x /* c */ !?[0]);", "/* c */", DisplayName = "In front of a conditional element access")]
    [DataRow("        Use(x /* c */ ![0]);", "/* c */", DisplayName = "In front of an element access")]
    [DataRow("        Use(f /* c */ !(1));", "/* c */", DisplayName = "In front of an argument list")]
    [DataRow("        Use((s /* c */ !).Length);", "/* c */", DisplayName = "In front of a closing parenthesis")]
    [DataRow("        Use(s! /* c */ !);", "/* c */", DisplayName = "In front of a second null-forgiving operator")]
    [DataRow("        Use(s.ToString() /* c */ !);", "/* c */", DisplayName = "Behind an invocation in a chain")]
    public void CommentInFrontOfANullForgivingOperatorIsUnsupported(string statement, string comment)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies that every kind of comment refuses the input in each position of the gap: behind the token in front of the part on
    /// its line, as a comment line, behind a blank line, in front of the part on its line, at the start of the part's line, on one
    /// line, as a multi-line block comment, without whitespace around it, behind a tab, and as two comments
    /// </summary>
    /// <param name="statement">The statement that holds the part</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(arr // c\n            [0]);", "// c", DisplayName = "Line comment behind the token in front of the part")]
    [DataRow("        Use(arr /* c */\n            [0]);", "/* c */", DisplayName = "Block comment at the end of the line in front of the part")]
    [DataRow("        Use(arr\n            // c\n            [0]);", "// c", DisplayName = "Comment line in front of the part")]
    [DataRow("        Use(arr\n\n            // c\n            [0]);", "// c", DisplayName = "Comment line behind a blank line")]
    [DataRow("        Use(arr\n            // c\n\n            [0]);", "// c", DisplayName = "Comment line in front of a blank line")]
    [DataRow("        Use(arr\n            /* c */ [0]);", "/* c */", DisplayName = "Block comment in front of the part on its line")]
    [DataRow("        Use(arr\n/* c */ [0]);", "/* c */", DisplayName = "Block comment at the start of the part's line")]
    [DataRow("        Use(arr /* c */ [0]);", "/* c */", DisplayName = "Block comment on one line")]
    [DataRow("        Use(arr /* c\n            d */ [0]);", "/* c\n            d */", DisplayName = "Multi-line block comment")]
    [DataRow("        Use(arr/* c */[0]);", "/* c */", DisplayName = "Block comment without whitespace")]
    [DataRow("        Use(arr\t/* c */ [0]);", "/* c */", DisplayName = "Block comment behind a tab")]
    [DataRow("        Use(arr /* c */ /* d */ [0]);", "/* c */", DisplayName = "Two block comments")]
    [DataRow("        Use(arr // c\n            // d\n            [0]);", "// c", DisplayName = "Line comment behind the token and a comment line")]
    public void EveryCommentPositionInTheGapIsUnsupported(string statement, string comment)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies that a documentation comment in a gap refuses the input in every documentation mode, in which it is a structured
    /// documentation comment or an ordinary comment, behind the token in front of the part and on a line of its own
    /// </summary>
    /// <param name="statement">The statement that holds the part</param>
    /// <param name="comment">The comment that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(arr /** c */ [0]);", "/** c */", DocumentationMode.Parse)]
    [DataRow("        Use(arr /** c */ [0]);", "/** c */", DocumentationMode.None)]
    [DataRow("        Use(arr /** c */ [0]);", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("        Use(Foo\n            /** c */\n            (1));", "/** c */", DocumentationMode.Parse)]
    [DataRow("        Use(Foo\n            /** c */\n            (1));", "/** c */", DocumentationMode.None)]
    [DataRow("        Use(Foo\n            /** c */\n            (1));", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("        Use(s\n            /// <summary>x</summary>\n            !);", "/// <summary>x</summary>", DocumentationMode.Parse)]
    [DataRow("        Use(s\n            /// <summary>x</summary>\n            !);", "/// <summary>x</summary>", DocumentationMode.None)]
    [DataRow("        Use(s\n            /// <summary>x</summary>\n            !);", "/// <summary>x</summary>", DocumentationMode.Diagnose)]
    [DataRow("        Use(n /// x\n            ?[0]);", "/// x", DocumentationMode.Parse)]
    [DataRow("        Use(n /// x\n            ?[0]);", "/// x", DocumentationMode.None)]
    [DataRow("        Use(n /// x\n            ?[0]);", "/// x", DocumentationMode.Diagnose)]
    [DataRow("        Use(n?\n            /// x\n            [0]);", "/// x", DocumentationMode.Parse)]
    [DataRow("        Use(n?\n            /// x\n            [0]);", "/// x", DocumentationMode.None)]
    public void DocumentationCommentInAGapIsUnsupportedInEveryDocumentationMode(string statement, string comment, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod(statement), comment, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that every kind of directive in a gap refuses the input, also a group whose code is disabled in every build, and
    /// that a comment in front of a directive is reported as the first trivia of the gap
    /// </summary>
    /// <param name="statement">The statement that holds the part</param>
    /// <param name="directive">The directive or comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(arr\n#pragma warning disable CS0001\n            [0]);", "#pragma warning disable CS0001", DisplayName = "Pragma directive in front of an element access")]
    [DataRow("        Use(Foo\n            #region R\n            (1));\n            #endregion", "#region R", DisplayName = "Region directive in front of an argument list")]
    [DataRow("        Use(s\n#nullable disable\n            !);\n#nullable restore", "#nullable disable", DisplayName = "Nullable directive in front of a null-forgiving operator")]
    [DataRow("        Use(n\n#line 200\n            ?[0]);\n#line default", "#line 200", DisplayName = "Line directive in front of a conditional element access")]
    [DataRow("        Use(n?\n#pragma warning disable CS0001\n            [0]);", "#pragma warning disable CS0001", DisplayName = "Pragma directive between the question mark and the bracket")]
    [DataRow("        Use(Foo\n#if false\n            (2)\n#endif\n            (1));", "#if false", DisplayName = "Fully inactive group in front of an argument list")]
    [DataRow("        Use(arr /* c */\n#pragma warning disable CS0001\n            [0]);", "/* c */", DisplayName = "Comment in front of a directive")]
    public void EveryDirectiveInAGapIsUnsupported(string statement, string directive)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod(statement), directive);
    }

    /// <summary>
    /// Verifies that a conditional group in a gap refuses the input in every build: a group that chooses between two element
    /// accesses, between two null-forgiving operators' lines, and between two brackets behind a <c>?</c>, and a group whose code is
    /// disabled in one build, are reported at their <c>#if</c>
    /// </summary>
    /// <param name="statement">The statement that holds the part</param>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(arr\n#if X\n            [0]);\n#else\n            [1]);\n#endif", false, DisplayName = "Group that chooses an element access, X not defined")]
    [DataRow("        Use(arr\n#if X\n            [0]);\n#else\n            [1]);\n#endif", true, DisplayName = "Group that chooses an element access, X defined")]
    [DataRow("        Use(s\n#if X\n            !);\n#else\n            !);\n#endif", false, DisplayName = "Group that chooses a null-forgiving operator, X not defined")]
    [DataRow("        Use(s\n#if X\n            !);\n#else\n            !);\n#endif", true, DisplayName = "Group that chooses a null-forgiving operator, X defined")]
    [DataRow("        Use(n?\n#if X\n            [0]);\n#else\n            [1]);\n#endif", false, DisplayName = "Group between the question mark and the bracket, X not defined")]
    [DataRow("        Use(n?\n#if X\n            [0]);\n#else\n            [1]);\n#endif", true, DisplayName = "Group between the question mark and the bracket, X defined")]
    [DataRow("        Use(Foo\n#if X\n            // only with X\n#endif\n            (1));", false, DisplayName = "Group with a comment in front of an argument list, X not defined")]
    [DataRow("        Use(Foo\n#if X\n            // only with X\n#endif\n            (1));", true, DisplayName = "Group with a comment in front of an argument list, X defined")]
    public void ConditionalGroupInAGapIsUnsupportedInEveryBuild(string statement, bool defineX)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod(statement), "#if X", parseOptions: CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies that the same choice written around the whole statement leaves the gap free of directives: the input is formatted in
    /// both builds
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ConditionalGroupAroundTheWholeStatementIsSupported(bool defineX)
    {
        AssertSupported(WrapInMethod("#if X\n        Use(Foo(1));\n#else\n        Use(Foo(2));\n#endif"), CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies the first condition's other side: a comment in front of a parenthesis, a bracket, a question mark, or an
    /// exclamation mark that starts no part FMT-BREAK-039 or FMT-BREAK-040 joins does not refuse the input — a parameter list, an
    /// attribute's argument list, a parenthesized expression, a cast, a tuple, <c>typeof</c>, <c>default</c>, a statement's
    /// parenthesis, an implicit element access, an array rank specifier, <c>new[]</c>, <c>stackalloc []</c>, a collection
    /// expression, a list pattern, an attribute list, an indexer's parameter list, a conditional member access, a conditional
    /// expression, a <c>!</c> followed by <c>.</c> or <c>?.</c>, and a logical negation
    /// </summary>
    /// <param name="statement">The statement or member that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Func<int, int> g = delegate /* c */ (int y) { return y; };", DisplayName = "Parameter list of an anonymous method")]
    [DataRow("        int L /* c */ (int y)\n        {\n            return y;\n        }\n\n        Use(L(1));", DisplayName = "Parameter list of a local function")]
    [DataRow("        Use(/* c */ (1));", DisplayName = "Parenthesized expression")]
    [DataRow("        Use(/* c */ (object)1);", DisplayName = "Cast")]
    [DataRow("        Use(/* c */ (1, 2));", DisplayName = "Tuple")]
    [DataRow("        Use(typeof /* c */ (int));", DisplayName = "Typeof")]
    [DataRow("        Use(default /* c */ (int));", DisplayName = "Default")]
    [DataRow("        if /* c */ (b)\n        {\n            Use(1);\n        }", DisplayName = "Parenthesis of an if statement")]
    [DataRow("        Use(new Dictionary<string, int> { /* c */ [\"a\"] = 1 });", DisplayName = "Implicit element access")]
    [DataRow("        Use(new int /* c */ [3]);", DisplayName = "Array rank specifier")]
    [DataRow("        Span<int> sp = stackalloc /* c */ [] { 1 };", DisplayName = "Implicit stackalloc array creation")]
    [DataRow("        Use(/* c */ [1]);", DisplayName = "Collection expression")]
    [DataRow("        Use(n /* c */ ?.Length);", DisplayName = "Conditional member access")]
    [DataRow("        Use(b /* c */ ? 1 : 2);", DisplayName = "Conditional expression")]
    [DataRow("        Use(s // c\n            !.Length);", DisplayName = "Null-forgiving operator followed by a dot")]
    [DataRow("        Use(s /* c */ !?.Length);", DisplayName = "Null-forgiving operator followed by a conditional member access")]
    [DataRow("        if (/* c */ !b)\n        {\n            Use(1);\n        }", DisplayName = "Logical negation")]
    public void CommentInFrontOfATokenThatStartsNoSuchPartIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies that a comment in front of the <c>[</c> of an implicitly typed array creation or of a list pattern behind
    /// <c>is</c> is not this shape: it stands in a gap the line-break phase joins, and the input is refused with that gap's reason
    /// </summary>
    /// <param name="reason">The reason that is reported</param>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(KeywordOperandCommentReason, "        Use(new /* c */ [] { 1 });", DisplayName = "Implicit array creation")]
    [DataRow(IsPatternCommentReason, "        if (o is /* c */ [1])\n        {\n            Use(1);\n        }", DisplayName = "List pattern")]
    public void CommentInFrontOfABracketBehindNewOrIsGivesTheReasonOfTheJoinedGap(string reason, string statement)
    {
        AssertUnsupportedFor(reason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies the first condition's other side for members: a comment in front of an attribute's argument list, between two
    /// attribute lists, in front of an indexer's parameter list, and in front of a method's parameter list does not refuse the
    /// input
    /// </summary>
    /// <param name="member">The member that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("    [Obsolete /* c */ (\"x\")]\n    void A()\n    {\n    }", DisplayName = "Attribute argument list")]
    [DataRow("    [Obsolete] /* c */ [CLSCompliant(false)]\n    void A()\n    {\n    }", DisplayName = "Attribute list behind another")]
    [DataRow("    int this /* c */ [string k] => 0;", DisplayName = "Indexer parameter list")]
    [DataRow("    void A /* c */ (int y)\n    {\n    }", DisplayName = "Method parameter list")]
    public void CommentInFrontOfAMemberTokenThatStartsNoSuchPartIsSupported(string member)
    {
        AssertSupported(WrapInClass(member));
    }

    /// <summary>
    /// Verifies the second condition's other side: a comment inside a part's brackets or parentheses, behind a part, in front of
    /// the token in front of the part, and above the statement does not refuse the input
    /// </summary>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(Foo(/* c */ 1));", DisplayName = "Inside an argument list")]
    [DataRow("        Use(arr[/* c */ 0]);", DisplayName = "Inside an element access")]
    [DataRow("        Use(n?[/* c */ 0]);", DisplayName = "Inside a conditional element access")]
    [DataRow("        Use(Foo(1 /* c */));", DisplayName = "In front of the closing parenthesis of an argument list")]
    [DataRow("        Use(arr[0] /* c */);", DisplayName = "Behind an element access")]
    [DataRow("        Use(s! /* c */);", DisplayName = "Behind a null-forgiving operator")]
    [DataRow("        Use(/* c */ arr[0]);", DisplayName = "In front of the token in front of the part")]
    [DataRow("        // c\n        Use(arr[0]);", DisplayName = "Above the statement")]
    [DataRow("        Use(1); // c\n        Use(arr[0]);", DisplayName = "Behind the statement above")]
    public void CommentOutsideTheGapsIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies that each part is checked with its own gap wherever it stands: in a chain with links, inside a list element, an
    /// interpolation hole, a lambda, a switch-expression arm, an object initializer, a nested element access, and a local function
    /// in a lambda
    /// </summary>
    /// <param name="statement">The statement that holds the part</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(list.Where(z => z > 0)\n            .ToArray() /* c */ [0]);", DisplayName = "In a chain with links")]
    [DataRow("        Use(list.Where(z => z > 0)\n            .ToList() /* c */ (0));", DisplayName = "Argument list in a chain with links")]
    [DataRow("        Use(1, arr /* c */ [0], 2);", DisplayName = "Inside a list element")]
    [DataRow("        Use($\"{arr /* c */ [0]}\");", DisplayName = "Inside an interpolation hole")]
    [DataRow("        Func<int> l = () => arr /* c */ [0];", DisplayName = "Inside a lambda")]
    [DataRow("        Use(o switch\n            {\n                int[] y => y /* c */ [0],\n                _ => 0\n            });", DisplayName = "Inside a switch-expression arm")]
    [DataRow("        Use(new List<int> { arr /* c */ [0] });", DisplayName = "Inside a collection initializer")]
    [DataRow("        Use(d[\"k\"] /* c */ [0]);", DisplayName = "Behind a nested element access")]
    [DataRow("        Func<int> l = () =>\n        {\n            int L() => arr /* c */ [0];\n\n            return L();\n        };", DisplayName = "Inside a local function in a lambda")]
    public void EveryPartIsCheckedWithItsOwnGap(string statement)
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies that a part inside disabled text is not checked: where <c>X</c> is not defined, the statement is disabled text and
    /// the input is formatted, and where it is defined, the statement is code and the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void PartInDisabledTextIsNotChecked(bool defineX)
    {
        var input = WrapInMethod("#if X\n        Use(arr /* c */ [0]);\n#endif");
        var parseOptions = CreateOptions(defineX, "X");

        if (defineX)
        {
            AssertUnsupportedFor(AttachedPartCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that a comment in front of an argument list in an attribute argument of a field declaration keeps the reason of
    /// FMT-UNSUP-004, and that the same comment inside the initializer value of a local declaration gives the reason of FMT-UNSUP-014
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    [Requirement("FMT-UNSUP-014")]
    public void CommentInAnAttributeArgumentOfADeclarationKeepsTheReasonOfTheDeclaration()
    {
        AssertUnsupportedFor(VariableDeclarationCommentReason, "using System;\n\nclass C\n{\n    [Obsolete(nameof /* c */ (C))] private int _e;\n}", "/* c */");
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod("        int c = Foo /* c */ (1);"), "/* c */");
    }

    /// <summary>
    /// Verifies that a directive in front of a part inside an initializer value of a field declaration with several declarators
    /// keeps the reason of FMT-UNSUP-010 in every build, and that the same directive in a field with one declarator gives the reason
    /// of FMT-UNSUP-014
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-014")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DirectiveInFrontOfAPartInAFieldWithSeveralDeclaratorsKeepsTheReasonOfTheFieldDeclaration(bool defineX)
    {
        const string value = " = Offset\n#if X\n        (1)\n#else\n        (2)\n#endif\n        ";
        var parseOptions = CreateOptions(defineX, "X");

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass($"    private int _a{value}, _b;"), "#if X", parseOptions: parseOptions);
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInClass($"    private int _a{value};"), "#if X", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that of a comment in a gap of an attached part and a comment of another shape, the first in the text is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-014")]
    public void FirstShapeInTheTextIsReported()
    {
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod("        Use(arr /* first */ [0] + /* second */ 1);"), "/* first */");
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod("        Use(1 + /* first */ arr /* second */ [0]);"), "/* first */");
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod("        Use(x /* first */ !? /* second */ [0]);"), "/* first */");
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod("        Use(n? /* first */ [0]);\n        Use(Foo /* second */ (1));"), "/* first */");
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInMethod("        Use(b ? /* first */ 1 : 2);\n        Use(Foo /* second */ (1));"), "/* first */");
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod("        Use(Foo /* first */ (1));\n        Use(b ? /* second */ 1 : 2);"), "/* first */");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment in front of the element access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
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
                AssertReportsComment(result.Unsupported, text, "// c");
            }
        }
    }

    /// <summary>
    /// Verifies that the node-level entry points refuse a target or a context that holds the method with the comment in front of
    /// its element access, and format the method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
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
                AssertReportsComment(nodeResult.Unsupported, text, "// c");

                var targetResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, FindMethod(root, "M"), TestContext.CancellationToken);

                Assert.AreEqual(text, (await targetResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(targetResult.Unsupported, text, "// c");

                var typeResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, type, TestContext.CancellationToken);

                Assert.AreEqual(text, (await typeResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(typeResult.Unsupported, text, "// c");

                var contextResult = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, FindMethod(root, "N"), type, TestContext.CancellationToken);

                Assert.AreEqual(text, (await contextResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(contextResult.Unsupported, text, "// c");

                var besideResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, FindMethod(root, "N"), TestContext.CancellationToken);

                Assert.IsNull(besideResult.Unsupported);
                Assert.AreNotEqual(text, (await besideResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString(), "The method beside the shape must be formatted.");
            }
        }
    }

    /// <summary>
    /// Verifies that a node-level input holds the shape only when it holds both the argument list and the token in front of it:
    /// <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for the invocation and the root, and nothing for the
    /// argument list, whose leading trivia holds no comment of the shape on its own, nor for the invoked name, whose trailing trivia
    /// holds the comment; and the node-level entry points refuse neither of them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [Requirement("FMT-UNSUP-003")]
    public async Task ArgumentListAloneAndInvokedExpressionAloneDoNotHoldTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(InvocationWithCommentInFrontOfItsArgumentList, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(invocation), text, "/* c */");
                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root), text, "/* c */");
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(invocation.ArgumentList));
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(invocation.Expression));
                Assert.IsNull(ReihitsuFormatter.FormatNode(invocation.ArgumentList, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.IsNull(ReihitsuFormatter.FormatNode(invocation.Expression, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.IsNull((await ReihitsuFormatter.FormatNodeInDocumentAsync(document, invocation.ArgumentList, TestContext.CancellationToken)).Unsupported);
                Assert.IsNull((await ReihitsuFormatter.FormatNodeInDocumentAsync(document, invocation.Expression, TestContext.CancellationToken)).Unsupported);
            }
        }
    }

    /// <summary>
    /// Verifies that the gap between the <c>?</c> and the <c>[</c> of a conditional element access is held only by an input that
    /// holds the <c>?</c>: <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for the conditional access, and
    /// nothing for its element binding, which holds only the <c>[</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [Requirement("FMT-UNSUP-003")]
    public void ElementBindingAloneDoesNotHoldTheGapBetweenQuestionMarkAndBracket()
    {
        const string input = "class C\n{\n    int? M(int[]? n)\n    {\n        return n? /* c */ [0];\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var access = root.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().Single();

            AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(access), text, "/* c */");
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(access.WhenNotNull));
            Assert.IsNull(ReihitsuFormatter.FormatNode(access.WhenNotNull, cancellationToken: TestContext.CancellationToken).Unsupported);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds, which stands in a
    /// tree of its own: an expression built with a comment in front of an argument list, an element access, or a null-forgiving
    /// operator is refused, and the same expression without the comment is not
    /// </summary>
    /// <param name="expression">The expression with the comment</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("Foo /* c */ (1)", "/* c */")]
    [DataRow("arr // c\n    [0]", "// c")]
    [DataRow("s /* c */ !", "/* c */")]
    [DataRow("n? /* c */ [0]", "/* c */")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode(string expression, string comment)
    {
        var node = SyntaxFactory.ParseExpression(expression);

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(node);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(AttachedPartCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, node.ToFullString(), comment, 0, "LF");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(SyntaxFactory.ParseExpression(expression.Replace(comment, string.Empty))));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same tree
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("class C\n{\n    int M(int[] arr)\n    {\n        return arr /* c */ [0];\n    }\n}")]
    [DataRow("class C\n{\n    int M()\n    {\n        return Foo // c\n            (1);\n    }\n\n    int Foo(int x)\n    {\n        return x;\n    }\n}")]
    [DataRow("class C\n{\n    int? M(int[]? n)\n    {\n        return n?\n#if DEBUG\n            [0];\n#else\n            [1];\n#endif\n    }\n}")]
    [DataRow("class C\n{\n    string M(string? s)\n    {\n        return s\n            // c\n            !;\n    }\n}")]
    [DataRow("class C\n{\n    int M(string? s)\n    {\n        return s // c\n            !.Length;\n    }\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: parts joined onto the line in front of them next to comments behind
    /// the part, behind the opening parenthesis or bracket, inside the part, above the statement, and behind a link operator's
    /// <c>!</c>, in every documentation mode, are formatted, and their output, formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="statement">The statement that holds the part</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("        Use(Foo\n            (1)); // c", DocumentationMode.Parse, DisplayName = "Comment behind a joined argument list")]
    [DataRow("        Use(arr\n            [0] // c\n            + 1);", DocumentationMode.Parse, DisplayName = "Comment behind a joined element access")]
    [DataRow("        Use(Foo\n            ( // c\n                1));", DocumentationMode.Parse, DisplayName = "Comment behind the opening parenthesis of a joined argument list")]
    [DataRow("        Use(arr\n            [ // c\n                0]);", DocumentationMode.Parse, DisplayName = "Comment behind the opening bracket of a joined element access")]
    [DataRow("        Use(Foo\n            (\n                // c\n                1));", DocumentationMode.Parse, DisplayName = "Comment line inside a joined argument list")]
    [DataRow("        Use(n\n            ?[/* c */ 0]);", DocumentationMode.Parse, DisplayName = "Comment inside a joined conditional element access")]
    [DataRow("        // c\n        Use(s\n            !);", DocumentationMode.Parse, DisplayName = "Comment above a statement with a joined null-forgiving operator")]
    [DataRow("        Use(s\n            !); /* c */", DocumentationMode.Parse, DisplayName = "Comment behind a statement with a joined null-forgiving operator")]
    [DataRow("        Use(s // c\n            !.Length);", DocumentationMode.Parse, DisplayName = "Comment in front of a link operator's null-forgiving operator")]
    [DataRow("        /// <summary>x</summary>\n        Use(arr\n            [0]);", DocumentationMode.Parse, DisplayName = "Documentation comment above a statement with a joined element access")]
    [DataRow("        Use(arr\n            [0]); /** c */", DocumentationMode.None, DisplayName = "Delimited documentation comment behind a statement, mode None")]
    [DataRow("        Use(arr\n            [0]); /// c", DocumentationMode.Diagnose, DisplayName = "Single-line documentation comment behind a statement, mode Diagnose")]
    public void OutputOfFormattedAttachedPartsIsNotUnsupported(string statement, DocumentationMode documentationMode)
    {
        var input = WrapInMethod(statement);
        var parseOptions = CSharpParseOptions.Default.WithDocumentationMode(documentationMode);

        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var first = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), parseOptions, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(first.Unsupported, $"The input must be formatted under {endingName} line endings: {first.Unsupported?.Reason}");

            var output = first.Output.ToString();
            var second = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(output, parseOptions, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(second.Unsupported, $"The formatted output must not be refused under {endingName} line endings:{endOfLine}{output}");
            Assert.AreEqual(output, second.Output.ToString(), $"The formatted output must be stable under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Wraps statements in a method <c>M</c> of a class <c>C</c> that derives from a class with an indexer, with the usings, the
    /// parameters, and the helper methods its parts use
    /// </summary>
    /// <param name="statements">The statements, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statements)
    {
        return WrapInClass($"    unsafe void M(int[] arr, int[]? n, string? s, object o, bool b, Func<int, int>? f, int[]? x, int* p, Dictionary<string, int[]> d, List<int> list)\n    {{\n{statements}\n    }}");
    }

    /// <summary>
    /// Wraps members in a class <c>C</c> that derives from a class with an indexer, with the usings and the helper members its
    /// parts use
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string members)
    {
        return $"#nullable enable\n\nusing System;\nusing System.Collections.Generic;\nusing System.Linq;\n\nclass B\n{{\n    public int this[int i] => i;\n}}\n\nclass C : B\n{{\n{members}\n\n    new int this[int i] => i;\n\n    int Foo(int x)\n    {{\n        return x;\n    }}\n\n    static int Offset(int x)\n    {{\n        return x;\n    }}\n\n    int[] Get()\n    {{\n        return [];\n    }}\n\n    void Use(object? value)\n    {{\n    }}\n}}";
    }

    /// <summary>
    /// Creates parse options that define the given symbol or define nothing
    /// </summary>
    /// <param name="define">Whether the symbol is defined</param>
    /// <param name="symbol">The symbol</param>
    /// <returns>The parse options</returns>
    private static CSharpParseOptions CreateOptions(bool define, string symbol)
    {
        return define ? CSharpParseOptions.Default.WithPreprocessorSymbols(symbol) : CSharpParseOptions.Default;
    }

    /// <summary>
    /// Asserts that the outcome reports the reason of FMT-UNSUP-014 and the span of the given comment in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    /// <param name="comment">The comment that is reported</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text, string comment)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(AttachedPartCommentReason, unsupported.Reason);
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