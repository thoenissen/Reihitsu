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
/// Tests for the unsupported shape of a comment, a directive, or disabled text between the arrow of an expression body and the
/// token in front of it (FMT-UNSUP-013): every owner of an expression body, every kind of comment in every documentation mode,
/// directives and disabled text in every build, conditional groups that close in front of the arrow or choose it, every position
/// in the gap with and without a line break, nested expression bodies, the excepted places above the member and inside its
/// signature, the arrows of lambdas and switch-expression arms, the shapes whose reason wins, every entry point, and the closure of
/// the formatter's output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedExpressionBodyArrowLeadingCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> that holds a comment line between its parameter list and its arrow, beside a method <c>N</c> that holds
    /// none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    int M()
                                                                        // c
                                                                        => 1;

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: each expression body holds a comment in front of its arrow, the input is refused, and
    /// the first comment, which starts in line 5, column 11, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    public void SpecExampleCommentsInFrontOfTheArrowsOfExpressionBodiesAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 private int _v;

                                 int A // c1
                                     => 1;

                                 int B
                                     // c2
                                     => 2;

                                 int this[int i] /* c3 */ => i;

                                 int Get() /* c4 */ => 42;

                                 int V
                                 {
                                     get // c5
                                         => _v;
                                 }

                                 void M()
                                 {
                                     int Local()
                                         /* c6 */ => 1;

                                     Local();
                                 }
                             }
                             """;

        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, input, "// c1");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(4, 10), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies each expression body of the specification example on its own: the input is refused, and its comment is reported
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    int A // c1\n        => 1;", "// c1", DisplayName = "Line comment behind the name of a property")]
    [DataRow("    int B\n        // c2\n        => 2;", "// c2", DisplayName = "Comment line between the name of a property and its arrow")]
    [DataRow("    int this[int i] /* c3 */ => i;", "/* c3 */", DisplayName = "Block comment in front of the arrow of an indexer on one line")]
    [DataRow("    int Get() /* c4 */ => 42;", "/* c4 */", DisplayName = "Block comment in front of the arrow of a method on one line")]
    [DataRow("    int V\n    {\n        get // c5\n            => _v;\n    }", "// c5", DisplayName = "Line comment behind the get keyword")]
    [DataRow("    void M()\n    {\n        int Local()\n            /* c6 */ => 1;\n\n        Local();\n    }", "/* c6 */", DisplayName = "Block comment in front of the arrow of a local function on its line")]
    public void EachExpressionBodyOfTheSpecExampleIsUnsupported(string members, string comment)
    {
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass(members), comment);
    }

    /// <summary>
    /// Verifies the specification example: a conditional group that chooses the arrow refuses the input in every build, and its
    /// <c>#if DEBUG</c>, which starts in line 4, column 1, is reported
    /// </summary>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void SpecExampleDirectiveInFrontOfTheArrowIsUnsupportedInEveryBuild(bool defineDebug)
    {
        const string input = """
                             class C
                             {
                                 int A
                             #if DEBUG
                                     => 1;
                             #else
                                     => 2;
                             #endif
                             }
                             """;
        var parseOptions = CreateOptions(defineDebug, "DEBUG");

        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, input, "#if DEBUG", parseOptions: parseOptions);

        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(3, 0), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies the specification example of the other side: comments above a member, inside a parameter list, behind an expression
    /// or its semicolon, inside an expression, and in front of the arrow of a lambda or of a switch-expression arm do not refuse the
    /// input, and the input is formatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [Requirement("FMT-UNSUP-012")]
    public void SpecExampleCommentsOutsideBothGapsAreSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 // c1
                                 int A => 1;

                                 int B(/* c2 */ int a) => a;

                                 int D => 1 /* c3 */;

                                 int E => 1; // c4

                                 void M(int n)
                                 {
                                     Func<int> f = () /* c5 */ => 1;
                                     var s = n switch
                                     {
                                         1 => "one", // c6
                                         _ => "other",
                                     };
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment in front of the arrow of every owner of an expression body refuses the input: a method, also an
    /// <c>async</c> one and one with a constraint clause, a constructor, also one with a constructor initializer, a static
    /// constructor, a finalizer, an operator, a conversion operator, a property, an indexer, an explicit interface implementation, a
    /// <c>get</c>, <c>set</c>, <c>init</c>, <c>add</c>, or <c>remove</c> accessor, and a local function, also an <c>async</c>, a
    /// <c>static</c>, or a generic one with a constraint clause
    /// </summary>
    /// <param name="members">The members of the class</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    int M() /* c */ => 1;", DisplayName = "Method")]
    [DataRow("    async Task<int> M() /* c */ => await Task.FromResult(1);", DisplayName = "Async method")]
    [DataRow("    T M<T>() where T : new() /* c */ => new T();", DisplayName = "Method with a constraint clause")]
    [DataRow("    T M<T>() where T : class /* c */ => null;", DisplayName = "Method with a class constraint")]
    [DataRow("    T M<T>() where T : I /* c */ => default;", DisplayName = "Method with a type constraint")]
    [DataRow("    C() /* c */ => _v = 1;", DisplayName = "Constructor")]
    [DataRow("    C() : this(1) /* c */ => _v = 2;\n\n    C(int v) => _v = v;", DisplayName = "Constructor with an initializer")]
    [DataRow("    static C() /* c */ => Console.WriteLine();", DisplayName = "Static constructor")]
    [DataRow("    ~C() /* c */ => _v = 0;", DisplayName = "Finalizer")]
    [DataRow("    public static C operator +(C a, C b) /* c */ => a;", DisplayName = "Binary operator")]
    [DataRow("    public static C operator !(C a) /* c */ => a;", DisplayName = "Unary operator")]
    [DataRow("    public static implicit operator int(C c) /* c */ => 1;", DisplayName = "Implicit conversion operator")]
    [DataRow("    public static explicit operator string(C c) /* c */ => \"\";", DisplayName = "Explicit conversion operator")]
    [DataRow("    int P /* c */ => 1;", DisplayName = "Property")]
    [DataRow("    static int S /* c */ => 1;", DisplayName = "Static property")]
    [DataRow("    int this[int i] /* c */ => i;", DisplayName = "Indexer")]
    [DataRow("    int I.P /* c */ => 1;", DisplayName = "Explicit interface implementation")]
    [DataRow("    int P { get /* c */ => _v; }", DisplayName = "Get accessor")]
    [DataRow("    int P { get => _v; set /* c */ => _v = value; }", DisplayName = "Set accessor")]
    [DataRow("    int P { get => _v; init /* c */ => _v = value; }", DisplayName = "Init accessor")]
    [DataRow("    int this[int i] { get /* c */ => i; }", DisplayName = "Get accessor of an indexer")]
    [DataRow("    event Action E { add /* c */ => Console.WriteLine(); remove => Console.WriteLine(); }", DisplayName = "Add accessor")]
    [DataRow("    event Action E { add => Console.WriteLine(); remove /* c */ => Console.WriteLine(); }", DisplayName = "Remove accessor")]
    [DataRow("    void M()\n    {\n        int L() /* c */ => 1;\n\n        L();\n    }", DisplayName = "Local function")]
    [DataRow("    async Task M()\n    {\n        async Task<int> L() /* c */ => await Task.FromResult(1);\n\n        await L();\n    }", DisplayName = "Async local function")]
    [DataRow("    void M()\n    {\n        static int L() /* c */ => 1;\n\n        L();\n    }", DisplayName = "Static local function")]
    [DataRow("    void M()\n    {\n        T L<T>() where T : new() /* c */ => new T();\n\n        L<C>();\n    }", DisplayName = "Local function with a constraint clause")]
    public void CommentInFrontOfTheArrowOfEveryExpressionBodyIsUnsupported(string members)
    {
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass(members), "/* c */");
    }

    /// <summary>
    /// Verifies that the arrow of a lambda belongs to no expression body: a comment in front of it does not refuse the input
    /// </summary>
    /// <param name="statement">The statement that holds the arrow</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("        Func<int, int> f = x /* c */ => x;", DisplayName = "Simple lambda")]
    [DataRow("        Func<int, int, int> f = (x, y) /* c */ => x + y;", DisplayName = "Parenthesized lambda")]
    [DataRow("        Func<int, int> f = x // c\n            => x;", DisplayName = "Line comment in front of the arrow of a simple lambda")]
    [DataRow("        Func<Task> f = async () /* c */ => await Task.Yield();", DisplayName = "Async lambda")]
    [DataRow("        Func<int> f = static () /* c */ => 1;", DisplayName = "Static lambda")]
    public void CommentInFrontOfTheArrowOfALambdaIsSupported(string statement)
    {
        AssertSupported(WrapInClass($"    void M()\n    {{\n{statement}\n    }}"));
    }

    /// <summary>
    /// Verifies that the arrow of a switch-expression arm belongs to no expression body: a comment in front of it is not this
    /// shape, and the input is refused with the reason of the gap in front of an arm's arrow
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [Requirement("FMT-UNSUP-017")]
    public void CommentInFrontOfTheArrowOfASwitchArmGivesTheReasonOfTheArm()
    {
        AssertUnsupportedFor(SwitchArmArrowCommentReason, WrapInClass("    void M()\n    {\n        var s = _v switch\n        {\n            1 /* c */ => \"one\",\n            _ => \"other\",\n        };\n    }"), "/* c */");
    }

    /// <summary>
    /// Verifies that every kind of comment refuses the input in each position of the gap: behind the token in front of the arrow on
    /// its line, on a line of its own, behind a blank line, in front of the arrow on the arrow's line, at the start of the arrow's
    /// line, on one line, as a multi-line block comment, without whitespace around it, behind a tab, and as two comments
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    int M() // c\n        => 1;", "// c", DisplayName = "Line comment behind the token in front of the arrow")]
    [DataRow("    int M() /* c */\n        => 1;", "/* c */", DisplayName = "Block comment at the end of the line in front of the arrow")]
    [DataRow("    int M()\n        // c\n        => 1;", "// c", DisplayName = "Comment line in front of the arrow")]
    [DataRow("    int M()\n\n        // c\n        => 1;", "// c", DisplayName = "Comment line behind a blank line in front of the arrow")]
    [DataRow("    int M()\n        // c\n\n        => 1;", "// c", DisplayName = "Comment line in front of a blank line in front of the arrow")]
    [DataRow("    int M()\n        /* c */ => 1;", "/* c */", DisplayName = "Block comment in front of the arrow on its line")]
    [DataRow("    int M()\n/* c */ => 1;", "/* c */", DisplayName = "Block comment at the start of the arrow's line")]
    [DataRow("    int M() /* c */ => 1;", "/* c */", DisplayName = "Block comment on one line")]
    [DataRow("    int M() // c\n        // d\n        => 1;", "// c", DisplayName = "Line comment behind the token and a comment line")]
    [DataRow("    int M()\n        // c\n        /* d */ => 1;", "// c", DisplayName = "Comment line and a block comment in front of the arrow on its line")]
    [DataRow("    int M() /* c\n        d */ => 1;", "/* c\n        d */", DisplayName = "Multi-line block comment")]
    [DataRow("    int M()/* c */=> 1;", "/* c */", DisplayName = "Block comment without whitespace")]
    [DataRow("    int M()\t/* c */ => 1;", "/* c */", DisplayName = "Block comment behind a tab")]
    [DataRow("    int M() /* c */ /* d */ => 1;", "/* c */", DisplayName = "Two block comments")]
    [DataRow("    int P\n        // c\n\n        => 1;", "// c", DisplayName = "Comment line in front of the arrow of a property followed by a blank line")]
    public void EveryCommentPositionInTheGapIsUnsupported(string members, string comment)
    {
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass(members), comment);
    }

    /// <summary>
    /// Verifies that a documentation comment in front of the arrow refuses the input in every documentation mode, in which it is a
    /// structured documentation comment or an ordinary comment, behind the token in front of the arrow and on a line of its own
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    int M() /** c */ => 1;", "/** c */", DocumentationMode.Parse)]
    [DataRow("    int M() /** c */ => 1;", "/** c */", DocumentationMode.None)]
    [DataRow("    int M() /** c */ => 1;", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("    int M()\n        /** c */\n        => 1;", "/** c */", DocumentationMode.Parse)]
    [DataRow("    int M()\n        /** c */\n        => 1;", "/** c */", DocumentationMode.None)]
    [DataRow("    int M()\n        /** c */\n        => 1;", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("    int A\n        /// <summary>x</summary>\n        => 1;", "/// <summary>x</summary>", DocumentationMode.Parse)]
    [DataRow("    int A\n        /// <summary>x</summary>\n        => 1;", "/// <summary>x</summary>", DocumentationMode.None)]
    [DataRow("    int A\n        /// <summary>x</summary>\n        => 1;", "/// <summary>x</summary>", DocumentationMode.Diagnose)]
    [DataRow("    int A /// x\n        => 1;", "/// x", DocumentationMode.Parse)]
    [DataRow("    int A /// x\n        => 1;", "/// x", DocumentationMode.None)]
    [DataRow("    int A /// x\n        => 1;", "/// x", DocumentationMode.Diagnose)]
    [DataRow("    int V\n    {\n        get /// x\n            => _v;\n    }", "/// x", DocumentationMode.Parse)]
    [DataRow("    int V\n    {\n        get\n            /// <summary>x</summary>\n            => _v;\n    }", "/// <summary>x</summary>", DocumentationMode.Parse)]
    public void DocumentationCommentInFrontOfTheArrowIsUnsupportedInEveryDocumentationMode(string members, string comment, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass(members), comment, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that every kind of directive in front of the arrow refuses the input, also a group whose code is disabled in every
    /// build, and that a comment in front of a directive is reported as the first trivia of the gap
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="directive">The directive or comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    int M()\n#pragma warning disable CS0001\n        => 1;", "#pragma warning disable CS0001", DisplayName = "Pragma directive")]
    [DataRow("    int M()\n        #region R\n        => 1;\n        #endregion", "#region R", DisplayName = "Region directive")]
    [DataRow("    string M()\n#nullable disable\n        => null;\n#nullable restore", "#nullable disable", DisplayName = "Nullable directive")]
    [DataRow("    int M()\n#line 200\n        => 1;\n#line default", "#line 200", DisplayName = "Line directive")]
    [DataRow("    int M()\n#if false\n        => 2;\n#endif\n        => 1;", "#if false", DisplayName = "Fully inactive group")]
    [DataRow("    int M() /* c */\n#pragma warning disable CS0001\n        => 1;", "/* c */", DisplayName = "Comment in front of a directive")]
    [DataRow("    int P\n#pragma warning disable CS0001\n        => 1;", "#pragma warning disable CS0001", DisplayName = "Pragma directive in front of the arrow of a property")]
    [DataRow("    int this[int i]\n#pragma warning disable CS0618\n        => i;", "#pragma warning disable CS0618", DisplayName = "Pragma directive in front of the arrow of an indexer")]
    [DataRow("    int P\n    {\n        get\n#pragma warning disable CS0001\n            => _v;\n    }", "#pragma warning disable CS0001", DisplayName = "Pragma directive in front of the arrow of a getter")]
    [DataRow("    void M()\n    {\n        int L()\n#pragma warning disable CS0001\n            => 1;\n\n        L();\n    }", "#pragma warning disable CS0001", DisplayName = "Pragma directive in front of the arrow of a local function")]
    public void EveryDirectiveInFrontOfTheArrowIsUnsupported(string members, string directive)
    {
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass(members), directive);
    }

    /// <summary>
    /// Verifies that a conditional group in front of the arrow refuses the input in every build: a group that chooses the arrow of a
    /// method, and a group whose code is disabled in one build, are reported at their <c>#if</c>
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    int M()\n#if X\n        => 1;\n#else\n        => 0;\n#endif", false, DisplayName = "Group that chooses the arrow of a method, X not defined")]
    [DataRow("    int M()\n#if X\n        => 1;\n#else\n        => 0;\n#endif", true, DisplayName = "Group that chooses the arrow of a method, X defined")]
    [DataRow("    int M()\n#if X\n        // only with X\n#endif\n        => 1;", false, DisplayName = "Group with a comment in front of the arrow, X not defined")]
    [DataRow("    int M()\n#if X\n        // only with X\n#endif\n        => 1;", true, DisplayName = "Group with a comment in front of the arrow, X defined")]
    public void ConditionalGroupInFrontOfTheArrowIsUnsupportedInEveryBuild(string members, bool defineX)
    {
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass(members), "#if X", parseOptions: CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies the change's example of a conditional group around the signature, around a constraint clause, and around a property's
    /// type, each closing in front of a shared arrow: the file is refused at the <c>#endif</c> in front of the first arrow, in line 9,
    /// column 1
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    public void ConditionalGroupsClosingInFrontOfASharedArrowAreUnsupported()
    {
        const string input = """
                             using System.Numerics;

                             class C
                             {
                             #if NET8_0
                                 public int M(int a)
                             #else
                                 public int M(long a)
                             #endif
                                     => 1;

                                 T Make<T>()
                             #if NET7_0_OR_GREATER
                                     where T : INumber<T>
                             #endif
                                     => default;

                             #if NET8_0
                                 public int P
                             #else
                                 public long P
                             #endif
                                     => 1;
                             }
                             """;

        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, input, "#endif");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(8, 0), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies each member of the change's example on its own and in both builds: a group around the signature is reported at the
    /// first of its directives in the arrow's leading trivia, <c>#endif</c> or, where the symbol is defined, <c>#else</c>; a group
    /// around a constraint clause at its <c>#if</c> or, where the symbol is defined, its <c>#endif</c>; and a group around a
    /// property's type like the one around a signature
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="symbol">The symbol that is defined, or <see langword="null"/></param>
    /// <param name="directive">The directive that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("#if NET8_0\n    public int M(int a)\n#else\n    public int M(long a)\n#endif\n        => 1;", null, "#endif", DisplayName = "Group around the signature, symbol not defined")]
    [DataRow("#if NET8_0\n    public int M(int a)\n#else\n    public int M(long a)\n#endif\n        => 1;", "NET8_0", "#else", DisplayName = "Group around the signature, symbol defined")]
    [DataRow("    T Make<T>()\n#if NET7_0_OR_GREATER\n        where T : INumber<T>\n#endif\n        => default;", null, "#if NET7_0_OR_GREATER", DisplayName = "Group around a constraint clause, symbol not defined")]
    [DataRow("    T Make<T>()\n#if NET7_0_OR_GREATER\n        where T : INumber<T>\n#endif\n        => default;", "NET7_0_OR_GREATER", "#endif", DisplayName = "Group around a constraint clause, symbol defined")]
    [DataRow("#if NET8_0\n    public int P\n#else\n    public long P\n#endif\n        => 1;", null, "#endif", DisplayName = "Group around the type of a property, symbol not defined")]
    [DataRow("#if NET8_0\n    public int P\n#else\n    public long P\n#endif\n        => 1;", "NET8_0", "#else", DisplayName = "Group around the type of a property, symbol defined")]
    public void ConditionalGroupClosingInFrontOfASharedArrowIsUnsupported(string members, string symbol, string directive)
    {
        var parseOptions = symbol == null ? CSharpParseOptions.Default : CSharpParseOptions.Default.WithPreprocessorSymbols(symbol);

        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, $"using System.Numerics;\n\nclass C\n{{\n{members}\n}}", directive, parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that the same choice written around the whole member, from in front of its first token through behind its
    /// semicolon, leaves the gap free of directives: the input is formatted in both builds
    /// </summary>
    /// <param name="defineSymbol">Whether <c>NET8_0</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow(false, DisplayName = "Symbol not defined")]
    [DataRow(true, DisplayName = "Symbol defined")]
    public void ConditionalGroupAroundTheWholeMemberIsSupported(bool defineSymbol)
    {
        AssertSupported("class C\n{\n#if NET8_0\n    public int M(int a) => 1;\n#else\n    public int M(long a) => 1;\n#endif\n\n#if NET8_0\n    public int P => 1;\n#else\n    public long P => 1;\n#endif\n}", CreateOptions(defineSymbol, "NET8_0"));
    }

    /// <summary>
    /// Verifies that a comment or directive outside the gap does not refuse the input: above the member, its documentation comment
    /// and a directive included; inside the signature, in front of its last token; and behind the expression or its semicolon
    /// </summary>
    /// <param name="members">The members of the class</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    // c\n    int A => 1;", DisplayName = "Comment above the member")]
    [DataRow("    /// <summary>x</summary>\n    int A => 1;", DisplayName = "Documentation comment above the member")]
    [DataRow("#pragma warning disable CS0001\n    int A => 1;", DisplayName = "Directive above the member")]
    [DataRow("    /* c */ int M() => 1;", DisplayName = "Block comment in front of the member's first token")]
    [DataRow("    int F(/* c */ int a) => a;", DisplayName = "Comment at the start of the parameter list")]
    [DataRow("    int F(int a /* c */) => a;", DisplayName = "Comment in front of the closing parenthesis of the parameter list")]
    [DataRow("    int this[int i /* c */] => i;", DisplayName = "Comment in front of the closing bracket of an indexer")]
    [DataRow("    T M<T>() where T : /* c */ new() => new T();", DisplayName = "Comment inside a constraint clause")]
    [DataRow("    int A => 1 /* c */;", DisplayName = "Comment in front of the semicolon")]
    [DataRow("    int A => 1; // c", DisplayName = "Comment behind the semicolon")]
    [DataRow("    Func<int> F => () /* c */ => 1;", DisplayName = "Comment in front of the arrow of a lambda that is the expression")]
    [DataRow("    int P\n    {\n        // c\n        get => _v;\n    }", DisplayName = "Comment in front of the get keyword")]
    public void CommentOutsideTheGapIsSupported(string members)
    {
        AssertSupported(WrapInClass(members));
    }

    /// <summary>
    /// Verifies that each expression body is checked with its own arrow and the token in front of it, also one nested in another
    /// construct: a local function in a lambda's body in a property's expression body or in a field's initializer, also one with
    /// several declarators, a local function in an accessor's body, an accessor of a property in a nested type, a local function in a
    /// local function, and a property of a nested interface or record
    /// </summary>
    /// <param name="members">The members of the class</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    Func<int> F => () =>\n    {\n        int L() /* c */ => 1;\n\n        return L();\n    };", DisplayName = "Local function in a lambda in a property's expression body")]
    [DataRow("    private Func<int> _f = () =>\n    {\n        int L() /* c */ => 1;\n\n        return L();\n    };", DisplayName = "Local function in a lambda in a field with one declarator")]
    [DataRow("    private Func<int> _f = () =>\n    {\n        int L() /* c */ => 1;\n\n        return L();\n    }, _g;", DisplayName = "Local function in a lambda in a field with several declarators")]
    [DataRow("    int P\n    {\n        get\n        {\n            int L() /* c */ => 1;\n\n            return L();\n        }\n    }", DisplayName = "Local function in an accessor's body")]
    [DataRow("    class D\n    {\n        int P { get /* c */ => 1; }\n    }", DisplayName = "Accessor of a property in a nested type")]
    [DataRow("    void M()\n    {\n        void Outer()\n        {\n            int L() /* c */ => 1;\n\n            L();\n        }\n\n        Outer();\n    }", DisplayName = "Local function in a local function")]
    [DataRow("    interface J\n    {\n        int P /* c */ => 1;\n    }", DisplayName = "Property of a nested interface")]
    [DataRow("    record R\n    {\n        int P /* c */ => 1;\n    }", DisplayName = "Property of a nested record")]
    public void EveryNestedExpressionBodyIsCheckedWithItsOwnArrow(string members)
    {
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass(members), "/* c */");
    }

    /// <summary>
    /// Verifies that an expression body inside disabled text is not checked: where <c>X</c> is not defined, the member is disabled
    /// text and the input is formatted, and where it is defined, the member is code and the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ExpressionBodyInDisabledTextIsNotChecked(bool defineX)
    {
        var input = WrapInClass("#if X\n    int A /* c */ => 1;\n#endif");
        var parseOptions = CreateOptions(defineX, "X");

        if (defineX)
        {
            AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that a directive in front of the arrow of a local function inside an initializer value of a field declaration with
    /// several declarators keeps the reason of FMT-UNSUP-010 in every build, and that the same directive in a field with one
    /// declarator gives the reason of FMT-UNSUP-013
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-013")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DirectiveInFrontOfTheArrowInAFieldWithSeveralDeclaratorsKeepsTheReasonOfTheFieldDeclaration(bool defineX)
    {
        const string body = "    {\n        int L()\n#if X\n            => 1;\n#else\n            => 2;\n#endif\n\n        return L();\n    }";
        var parseOptions = CreateOptions(defineX, "X");

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass($"    private Func<int> _f = () =>\n{body}, _g;"), "#if X", parseOptions: parseOptions);
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass($"    private Func<int> _f = () =>\n{body};"), "#if X", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that of a comment in front of the arrow of an expression body and a comment of another shape, the first in the text
    /// is reported: in front of and behind one arrow, the comment in front of it; and across members, each shape in its turn
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-013")]
    public void FirstShapeInTheTextIsReported()
    {
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass("    int A /* a */ => /* b */ 1;"), "/* a */");
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass("    int Both()\n        // before\n        =>\n        // after\n        42;"), "// before");
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass("    int Get3()\n        /* c */ =>\n        /* d */ 42;"), "/* c */");
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass("    int M() /* c */ =>\n        // a\n        42;"), "/* c */");
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass("    int A => /* first */ 1;\n\n    int B /* second */ => 2;"), "/* first */");
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass("    int B /* first */ => 2;\n\n    int A => /* second */ 1;"), "/* first */");
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInClass("    int M(int i) => i + /* first */ 1;\n\n    int N() /* second */ => 1;"), "/* first */");
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInClass("    void M()\n    {\n        int /* first */ x = 1;\n    }\n\n    int N() /* second */ => 1;"), "/* first */");
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, WrapInClass("    int N() /* first */ => 1;\n\n    void M()\n    {\n        int /* second */ x = 1;\n    }"), "/* first */");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment in front of the arrow
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
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
    /// Verifies that the node-level entry points refuse a target or a context that holds the method with the comment in front of
    /// its arrow, and format the method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
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

                var typeResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, type, TestContext.CancellationToken);

                Assert.AreEqual(text, (await typeResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(typeResult.Unsupported, text);

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
    /// Verifies that a node-level input holds the shape only when it holds both the arrow and the token in front of it:
    /// <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for the method and the root, and nothing for the arrow
    /// clause, whose leading trivia holds the comment; and the node-level entry points do not refuse the arrow clause, neither as a
    /// target in its document nor as a node of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [Requirement("FMT-UNSUP-003")]
    public async Task ArrowClauseAloneDoesNotHoldTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(RefusedMethodBesideFormattableMethod, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var arrow = FindMethod(root, "M").ExpressionBody;

                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(FindMethod(root, "M")), text);
                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root), text);
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(arrow));
                Assert.IsNull(ReihitsuFormatter.FormatNode(arrow, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.IsNull((await ReihitsuFormatter.FormatNodeInDocumentAsync(document, arrow, TestContext.CancellationToken)).Unsupported);
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for an accessor, which holds its keyword and its
    /// arrow: a getter with a comment behind its keyword is refused, and its arrow clause alone is not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [Requirement("FMT-UNSUP-003")]
    public void FindUnsupportedSyntaxAnswersForAnAccessor()
    {
        const string input = "class C\n{\n    private int _v;\n\n    int V\n    {\n        get // c\n            => _v;\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var accessor = root.DescendantNodes().OfType<AccessorDeclarationSyntax>().Single();

            AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(accessor), text);
            AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Single()), text);
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(accessor.ExpressionBody));
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds, which stands in a
    /// tree of its own: a member built with a comment in front of its arrow is refused, and its arrow clause is not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-013")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode()
    {
        var member = SyntaxFactory.ParseMemberDeclaration("int M() /* c */ => 1;");

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(member);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(ExpressionBodyArrowLeadingCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, member.ToFullString(), "/* c */", 0, "LF");

        var ownLine = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("int M()\n    // c\n    => 1;");

        Assert.IsNotNull(ReihitsuFormatter.FindUnsupportedSyntax(ownLine));
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(ownLine.ExpressionBody));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same tree
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("class C\n{\n    int M() /* c */ => 1;\n}")]
    [DataRow("class C\n{\n    int P // c\n        => 1;\n}")]
    [DataRow("class C\n{\n    int P\n#if DEBUG\n        => 1;\n#else\n        => 2;\n#endif\n}")]
    [DataRow("class C\n{\n    int P\n    {\n        get\n            // c\n            => 1;\n    }\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        System.Func<int> f = () /* c */ => 1;\n    }\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: get-only members whose accessor list is lifted or kept with comments at
    /// each seam the lift deletes or joins, block getters with comments behind <c>get</c> or in front of their block, properties whose
    /// arrow is joined onto the name's line, and members converted to a block body with a comment above them are formatted, and their
    /// output, formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    int V1\n    {\n        get // c1\n        {\n            return _v;\n        }\n    }", DocumentationMode.Parse, DisplayName = "Line comment behind the get keyword of a block getter")]
    [DataRow("    int V1b\n    {\n        get /* c1 */ { return _v; }\n    }", DocumentationMode.Parse, DisplayName = "Block comment behind the get keyword of a block getter")]
    [DataRow("    int V2 // c2\n    {\n        get\n        {\n            return 1;\n        }\n    }", DocumentationMode.Parse, DisplayName = "Line comment behind the name in front of the accessor list")]
    [DataRow("    int V3\n    {\n        // c3\n        get\n        {\n            return 1;\n        }\n    }", DocumentationMode.Parse, DisplayName = "Comment line in front of the get keyword")]
    [DataRow("    int this[int i] /* c4 */ { get { return i; } }", DocumentationMode.Parse, DisplayName = "Block comment behind the bracket of an indexer in front of its accessor list")]
    [DataRow("    int V5 { get { return 1; } } // c5", DocumentationMode.Parse, DisplayName = "Comment behind the accessor list")]
    [DataRow("    int V6\n    /* c6 */ { get => 1; }", DocumentationMode.Parse, DisplayName = "Block comment in front of the opening brace of the accessor list")]
    [DataRow("    int V7\n    {\n        get\n        // c7\n        {\n            return _v;\n        }\n        set /* c8 */ { _v = value; }\n    }", DocumentationMode.Parse, DisplayName = "Comment line between the get keyword and its block and a comment behind the set keyword")]
    [DataRow("    int V9\n    {\n        get\n        {\n            return _v;\n        } // c9\n    }", DocumentationMode.Parse, DisplayName = "Comment behind the closing brace of a getter block")]
    [DataRow("    int V10\n    {\n        get\n        {\n            return _v; // c10\n        }\n    }", DocumentationMode.Parse, DisplayName = "Comment behind the return statement of a getter block")]
    [DataRow("    int V11\n    {\n        get => _v; // c11\n    }", DocumentationMode.Parse, DisplayName = "Comment behind the semicolon of an expression-bodied getter")]
    [DataRow("    int V12\n    {\n        get\n        {\n            return _v;\n        }\n        /* c12 */\n    }", DocumentationMode.Parse, DisplayName = "Comment in front of the closing brace of the accessor list")]
    [DataRow("    int V13\n    {\n        /// <summary>doc</summary>\n        get { return 1; }\n    }", DocumentationMode.Parse, DisplayName = "Documentation comment in front of the get keyword")]
    [DataRow("    int V14 /// doc\n    {\n        get { return 1; }\n    }", DocumentationMode.Parse, DisplayName = "Single-line documentation comment behind the name in front of the accessor list")]
    [DataRow("    int V15 /// doc\n    {\n        get { return 1; }\n    }", DocumentationMode.None, DisplayName = "Single-line documentation comment behind the name in front of the accessor list, mode None")]
    [DataRow("    int V16\n    {\n        get\n#pragma warning disable CS0001\n        {\n            return 1;\n        }\n    }", DocumentationMode.Parse, DisplayName = "Directive between the get keyword and its block")]
    [DataRow("    int A\n        => 1;", DocumentationMode.Parse, DisplayName = "Property whose arrow starts the next line")]
    [DataRow("    int this[int i]\n        => i;", DocumentationMode.Parse, DisplayName = "Indexer whose arrow starts the next line")]
    [DataRow("    // c\n    int M()\n        => 1;", DocumentationMode.Parse, DisplayName = "Method with a comment above it whose arrow starts the next line")]
    [DataRow("    /// <summary>doc</summary>\n    int M() => 1;", DocumentationMode.Parse, DisplayName = "Method with a documentation comment above it")]
    public void OutputOfFormattedExpressionBodiesIsNotUnsupported(string members, DocumentationMode documentationMode)
    {
        var input = WrapInClass(members);
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
    /// Wraps members in a class <c>C</c> that implements an interface <c>I</c>, with the usings and the field its expression bodies
    /// use
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string members)
    {
        return $"using System;\nusing System.Threading.Tasks;\n\ninterface I\n{{\n    int P {{ get; }}\n}}\n\nclass C : I\n{{\n    private int _v;\n\n{members}\n}}";
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
    /// Asserts that the outcome reports the reason of FMT-UNSUP-013 and the span of the comment <c>// c</c> in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(ExpressionBodyArrowLeadingCommentReason, unsupported.Reason);
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