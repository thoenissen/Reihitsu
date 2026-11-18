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
/// Tests for the unsupported shape of a comment, a directive, or disabled text between the arrow of an expression body and its
/// expression (FMT-UNSUP-012): every owner of an expression body, every kind of comment in every documentation mode, directives
/// and disabled text in both builds, every position in the gap with and without a line break, nested expression bodies, the
/// excepted places in front of the arrow, inside the expression, and behind it, the arrows of lambdas and switch-expression arms,
/// the shapes whose reason wins, every entry point, and the closure of the formatter's output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedExpressionBodyArrowCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> that holds a comment behind the arrow of its expression body, beside a method <c>N</c> that holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    int M() => // c
                                                                        1;

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: each expression body holds a comment behind its arrow, the input is refused, and the
    /// first comment, which starts in line 5, column 14, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    public void SpecExampleCommentsBehindTheArrowsOfExpressionBodiesAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 private int _v;

                                 int A => // c1
                                     1;

                                 int B =>
                                     // c2
                                     2;

                                 int this[int i] => /* c3 */ i;

                                 int Get() => /* c4 */ 42;

                                 int V
                                 {
                                     get => // c5
                                         _v;
                                 }

                                 void M()
                                 {
                                     int Local() =>
                                         /* c6 */ 1;

                                     Local();
                                 }
                             }
                             """;

        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, input, "// c1");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(4, 13), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies each expression body of the specification example on its own: the input is refused, and its comment is reported
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    int A => // c1\n        1;", "// c1", DisplayName = "Line comment behind the arrow of a property")]
    [DataRow("    int B =>\n        // c2\n        2;", "// c2", DisplayName = "Comment line between the arrow of a property and its expression")]
    [DataRow("    int this[int i] => /* c3 */ i;", "/* c3 */", DisplayName = "Block comment behind the arrow of an indexer on one line")]
    [DataRow("    int Get() => /* c4 */ 42;", "/* c4 */", DisplayName = "Block comment behind the arrow of a method on one line")]
    [DataRow("    int V\n    {\n        get => // c5\n            _v;\n    }", "// c5", DisplayName = "Line comment behind the arrow of a getter")]
    [DataRow("    void M()\n    {\n        int Local() =>\n            /* c6 */ 1;\n\n        Local();\n    }", "/* c6 */", DisplayName = "Block comment in front of the expression of a local function on its line")]
    public void EachExpressionBodyOfTheSpecExampleIsUnsupported(string members, string comment)
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass(members), comment);
    }

    /// <summary>
    /// Verifies the specification example: a conditional group behind the arrow refuses the input in every build, and its
    /// <c>#if DEBUG</c>, which starts in line 4, column 1, is reported
    /// </summary>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void SpecExampleDirectiveBehindTheArrowIsUnsupportedInEveryBuild(bool defineDebug)
    {
        const string input = """
                             class C
                             {
                                 int A =>
                             #if DEBUG
                                     1;
                             #else
                                     2;
                             #endif
                             }
                             """;
        var parseOptions = CreateOptions(defineDebug, "DEBUG");

        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, input, "#if DEBUG", parseOptions: parseOptions);

        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(3, 0), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies the specification example of the other side: comments above a member, inside an expression, behind an expression or
    /// its semicolon, and behind the arrow of a lambda or of a switch-expression arm do not refuse the input, and the input is
    /// formatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    public void SpecExampleCommentsOutsideTheGapAreSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 // c1
                                 int A => 1;

                                 int C2 => (/* c2 */ 1);

                                 int D => 1 /* c3 */;

                                 int E => 1; // c4

                                 void M(int n)
                                 {
                                     Func<int> f = () => /* c5 */ 1;
                                     var s = n switch
                                     {
                                         1 => /* c6 */ "one",
                                         _ => "other",
                                     };
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment behind the arrow of every owner of an expression body refuses the input: a method, also an
    /// <c>async</c> one, a constructor, a static constructor, a finalizer, an operator, a conversion operator, a property, an
    /// indexer, an explicit interface implementation, a <c>get</c>, <c>set</c>, <c>init</c>, <c>add</c>, or <c>remove</c>
    /// accessor, and a local function, also an <c>async</c> or a <c>static</c> one
    /// </summary>
    /// <param name="members">The members of the class</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    int M() => /* c */ 1;", DisplayName = "Method")]
    [DataRow("    async Task<int> M() => /* c */ await Task.FromResult(1);", DisplayName = "Async method returning Task<T>")]
    [DataRow("    async ValueTask<int> M() => /* c */ await new ValueTask<int>(1);", DisplayName = "Async method returning ValueTask<T>")]
    [DataRow("    async Task M() => /* c */ await Task.Yield();", DisplayName = "Async method returning Task")]
    [DataRow("    C() => /* c */ _v = 1;", DisplayName = "Constructor")]
    [DataRow("    static C() => /* c */ Console.WriteLine();", DisplayName = "Static constructor")]
    [DataRow("    ~C() => /* c */ _v = 0;", DisplayName = "Finalizer")]
    [DataRow("    public static C operator +(C a, C b) => /* c */ a;", DisplayName = "Binary operator")]
    [DataRow("    public static C operator !(C a) => /* c */ a;", DisplayName = "Unary operator")]
    [DataRow("    public static implicit operator int(C c) => /* c */ 1;", DisplayName = "Implicit conversion operator")]
    [DataRow("    public static explicit operator string(C c) => /* c */ \"\";", DisplayName = "Explicit conversion operator")]
    [DataRow("    int P => /* c */ 1;", DisplayName = "Property")]
    [DataRow("    static int S => /* c */ 1;", DisplayName = "Static property")]
    [DataRow("    int this[int i] => /* c */ i;", DisplayName = "Indexer")]
    [DataRow("    int I.P => /* c */ 1;", DisplayName = "Explicit interface implementation")]
    [DataRow("    int P { get => /* c */ _v; }", DisplayName = "Get accessor")]
    [DataRow("    int P { get => _v; set => /* c */ _v = value; }", DisplayName = "Set accessor")]
    [DataRow("    int P { get => _v; init => /* c */ _v = value; }", DisplayName = "Init accessor")]
    [DataRow("    int this[int i] { get => /* c */ i; }", DisplayName = "Get accessor of an indexer")]
    [DataRow("    event Action E { add => /* c */ Console.WriteLine(); remove => Console.WriteLine(); }", DisplayName = "Add accessor")]
    [DataRow("    event Action E { add => Console.WriteLine(); remove => /* c */ Console.WriteLine(); }", DisplayName = "Remove accessor")]
    [DataRow("    void M()\n    {\n        int L() => /* c */ 1;\n\n        L();\n    }", DisplayName = "Local function")]
    [DataRow("    async Task M()\n    {\n        async Task<int> L() => /* c */ await Task.FromResult(1);\n\n        await L();\n    }", DisplayName = "Async local function")]
    [DataRow("    void M()\n    {\n        static int L() => /* c */ 1;\n\n        L();\n    }", DisplayName = "Static local function")]
    public void CommentBehindTheArrowOfEveryExpressionBodyIsUnsupported(string members)
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass(members), "/* c */");
    }

    /// <summary>
    /// Verifies that the arrow of a lambda and of a switch-expression arm belongs to no expression body: a comment behind it does
    /// not refuse the input
    /// </summary>
    /// <param name="statement">The statement that holds the arrow</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("        Func<int, int> f = x => /* c */ x;", DisplayName = "Simple lambda")]
    [DataRow("        Func<int, int, int> f = (x, y) => /* c */ x + y;", DisplayName = "Parenthesized lambda")]
    [DataRow("        Func<Task> f = async () => /* c */ await Task.Yield();", DisplayName = "Async lambda")]
    [DataRow("        Func<int> f = static () => /* c */ 1;", DisplayName = "Static lambda")]
    [DataRow("        var s = _v switch\n        {\n            1 => /* c */ \"one\",\n            _ => \"other\",\n        };", DisplayName = "Switch-expression arm")]
    public void CommentBehindTheArrowOfALambdaOrASwitchArmIsSupported(string statement)
    {
        AssertSupported(WrapInClass($"    void M()\n    {{\n{statement}\n    }}"));
    }

    /// <summary>
    /// Verifies that every kind of comment refuses the input in each position of the gap: behind the arrow on its line, on a line
    /// of its own in front of the expression, behind a blank line, at the start of a line, as a multi-line block comment, without
    /// whitespace around it, behind a tab, and as two comments
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    int M() => // c\n        1;", "// c", DisplayName = "Line comment behind the arrow")]
    [DataRow("    int M() => /* c */\n        1;", "/* c */", DisplayName = "Block comment behind the arrow at the end of its line")]
    [DataRow("    int M() =>\n        // c\n        1;", "// c", DisplayName = "Comment line in front of the expression")]
    [DataRow("    int M() =>\n\n        // c\n        1;", "// c", DisplayName = "Comment line behind a blank line in front of the expression")]
    [DataRow("    int M() =>\n/* c */ 1;", "/* c */", DisplayName = "Block comment at the start of the expression's line")]
    [DataRow("    int M() => // c\n        // d\n        1;", "// c", DisplayName = "Line comment behind the arrow and a comment line")]
    [DataRow("    int M() => /* c\n        d */ 1;", "/* c\n        d */", DisplayName = "Multi-line block comment")]
    [DataRow("    int M() =>/* c */1;", "/* c */", DisplayName = "Block comment without whitespace")]
    [DataRow("    int M() =>\t/* c */ 1;", "/* c */", DisplayName = "Block comment behind a tab")]
    [DataRow("    int M() => /* c */ /* d */ 1;", "/* c */", DisplayName = "Two block comments")]
    [DataRow("    int P =>\n        // c\n\n        1;", "// c", DisplayName = "Comment line behind the arrow of a property followed by a blank line")]
    public void EveryCommentPositionInTheGapIsUnsupported(string members, string comment)
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass(members), comment);
    }

    /// <summary>
    /// Verifies that a documentation comment behind the arrow refuses the input in every documentation mode, in which it is a
    /// structured documentation comment or an ordinary comment
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    int M() => /** c */ 1;", "/** c */", DocumentationMode.Parse)]
    [DataRow("    int M() => /** c */ 1;", "/** c */", DocumentationMode.None)]
    [DataRow("    int M() => /** c */ 1;", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("    int M() =>\n        /** c */\n        1;", "/** c */", DocumentationMode.Parse)]
    [DataRow("    int M() =>\n        /** c */\n        1;", "/** c */", DocumentationMode.None)]
    [DataRow("    int M() =>\n        /** c */\n        1;", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("    int A =>\n        /// <summary>x</summary>\n        1;", "/// <summary>x</summary>", DocumentationMode.Parse)]
    [DataRow("    int A =>\n        /// <summary>x</summary>\n        1;", "/// <summary>x</summary>", DocumentationMode.None)]
    [DataRow("    int A =>\n        /// <summary>x</summary>\n        1;", "/// <summary>x</summary>", DocumentationMode.Diagnose)]
    [DataRow("    int A => /// x\n        1;", "/// x", DocumentationMode.Parse)]
    [DataRow("    int A => /// x\n        1;", "/// x", DocumentationMode.None)]
    [DataRow("    int A => /// x\n        1;", "/// x", DocumentationMode.Diagnose)]
    public void DocumentationCommentBehindTheArrowIsUnsupportedInEveryDocumentationMode(string members, string comment, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass(members), comment, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that every kind of directive behind the arrow refuses the input, also a group whose code is disabled in every
    /// build, and that a comment in front of a directive is reported as the first trivia of the gap
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="directive">The directive or comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    int M() =>\n#pragma warning disable CS0001\n        1;", "#pragma warning disable CS0001", DisplayName = "Pragma directive")]
    [DataRow("    int M() =>\n#region R\n        1;\n#endregion", "#region R", DisplayName = "Region directive")]
    [DataRow("    string M() =>\n#nullable disable\n        null;\n#nullable restore", "#nullable disable", DisplayName = "Nullable directive")]
    [DataRow("    int M() =>\n#line 200\n        1;\n#line default", "#line 200", DisplayName = "Line directive")]
    [DataRow("    int M() =>\n#if false\n        2 +\n#endif\n        1;", "#if false", DisplayName = "Fully inactive group")]
    [DataRow("    int M() =>\n#if X\n        1\n#elif Y\n        2\n#else\n        3\n#endif\n        ;", "#if X", DisplayName = "Group with elif and else branches")]
    [DataRow("    int M() => /* c */\n#pragma warning disable CS0001\n        1;", "/* c */", DisplayName = "Comment in front of a directive")]
    [DataRow("    int P =>\n#pragma warning disable CS0001\n        _v;", "#pragma warning disable CS0001", DisplayName = "Pragma directive behind the arrow of a property")]
    [DataRow("    int P\n    {\n        get =>\n#pragma warning disable CS0001\n            _v;\n    }", "#pragma warning disable CS0001", DisplayName = "Pragma directive behind the arrow of a getter")]
    public void EveryDirectiveBehindTheArrowIsUnsupported(string members, string directive)
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass(members), directive);
    }

    /// <summary>
    /// Verifies that a conditional group whose code is disabled in one build refuses the input in both builds: where <c>X</c> is
    /// defined, its <c>#if X</c> stands behind the arrow, and where it is not, the <c>#if X</c> and the disabled text do
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DisabledTextBehindTheArrowIsUnsupportedInEveryBuild(bool defineX)
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass("    int M() =>\n#if X\n        2 +\n#endif\n        1;"), "#if X", parseOptions: CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies that the gap ends at the expression's first token, whatever the expression starts with: a <c>ref</c>, a
    /// <c>throw</c>, a parenthesis, a unary operator, a <c>new</c>, an <c>await</c>, and a lambda
    /// </summary>
    /// <param name="members">The members of the class</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    ref int M() => /* c */ ref _v;", DisplayName = "In front of a ref")]
    [DataRow("    int M() => /* c */ throw new Exception();", DisplayName = "In front of a throw")]
    [DataRow("    int M() => /* c */ (1);", DisplayName = "In front of a parenthesis")]
    [DataRow("    int M() => /* c */ -1;", DisplayName = "In front of a unary operator")]
    [DataRow("    object M() => /* c */ new object();", DisplayName = "In front of a new")]
    [DataRow("    async Task<int> M() => /* c */ await Task.FromResult(1);", DisplayName = "In front of an await")]
    [DataRow("    Func<int> F => /* c */ () => 1;", DisplayName = "In front of a lambda")]
    [DataRow("    int M(int i) => /* c */ i + 1;", DisplayName = "In front of a binary expression")]
    public void CommentInFrontOfTheExpressionsFirstTokenIsUnsupported(string members)
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass(members), "/* c */");
    }

    /// <summary>
    /// Verifies that a comment outside the gap does not refuse the input: above the member; inside the expression behind its first
    /// token; and behind the expression or its semicolon. A comment or directive in front of the arrow is FMT-UNSUP-013's shape
    /// </summary>
    /// <param name="members">The members of the class</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    // c\n    int A => 1;", DisplayName = "Above the member")]
    [DataRow("    ref int M() => ref /* c */ _v;", DisplayName = "Behind a ref")]
    [DataRow("    int M() => throw /* c */ new Exception();", DisplayName = "Behind a throw")]
    [DataRow("    int M() => (/* c */ 1);", DisplayName = "Inside parentheses")]
    [DataRow("    int M() => N(/* c */ 1);\n\n    int N(int i) => i;", DisplayName = "Inside an argument list")]
    [DataRow("    int M() => -/* c */ 1;", DisplayName = "Behind a unary operator")]
    [DataRow("    int M() => 1 /* c */;", DisplayName = "In front of the semicolon")]
    [DataRow("    int M() => 1; // c", DisplayName = "Behind the semicolon")]
    [DataRow("    Func<int> F => () => /* c */ 1;", DisplayName = "Behind the arrow of a lambda that is the expression")]
    public void CommentOutsideTheGapIsSupported(string members)
    {
        AssertSupported(WrapInClass(members));
    }

    /// <summary>
    /// Verifies that each expression body is checked with its own arrow, also one nested in another construct: a local function
    /// in a lambda's body in a property's expression body or in a field's initializer, an accessor of a property in a nested type,
    /// a local function in a local function, and a property of a nested interface or record
    /// </summary>
    /// <param name="members">The members of the class</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    Func<int> F => () =>\n    {\n        int L() => /* c */ 1;\n\n        return L();\n    };", DisplayName = "Local function in a lambda in a property's expression body")]
    [DataRow("    private Func<int> _f = () =>\n    {\n        int L() => /* c */ 1;\n\n        return L();\n    };", DisplayName = "Local function in a lambda in a field with one declarator")]
    [DataRow("    private Func<int> _f = () =>\n    {\n        int L() => /* c */ 1;\n\n        return L();\n    }, _g;", DisplayName = "Local function in a lambda in a field with several declarators")]
    [DataRow("    class D\n    {\n        int P { get => /* c */ 1; }\n    }", DisplayName = "Accessor of a property in a nested type")]
    [DataRow("    void M()\n    {\n        void Outer()\n        {\n            int L() => /* c */ 1;\n\n            L();\n        }\n\n        Outer();\n    }", DisplayName = "Local function in a local function")]
    [DataRow("    interface J\n    {\n        int P => /* c */ 1;\n    }", DisplayName = "Property of a nested interface")]
    [DataRow("    record R\n    {\n        int P => /* c */ 1;\n    }", DisplayName = "Property of a nested record")]
    public void EveryNestedExpressionBodyIsCheckedWithItsOwnArrow(string members)
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass(members), "/* c */");
    }

    /// <summary>
    /// Verifies that an expression body inside disabled text is not checked: where <c>X</c> is not defined, the member is disabled
    /// text and the input is formatted, and where it is defined, the member is code and the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ExpressionBodyInDisabledTextIsNotChecked(bool defineX)
    {
        var input = WrapInClass("#if X\n    int A => /* c */ 1;\n#endif");
        var parseOptions = CreateOptions(defineX, "X");

        if (defineX)
        {
            AssertUnsupportedFor(ExpressionBodyArrowCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that a directive behind the arrow of a local function inside an initializer value of a field declaration with several
    /// declarators keeps the reason of FMT-UNSUP-010 in every build, and that the same directive in a field with one declarator gives
    /// the reason of FMT-UNSUP-012
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-012")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DirectiveBehindTheArrowInAFieldWithSeveralDeclaratorsKeepsTheReasonOfTheFieldDeclaration(bool defineX)
    {
        const string body = "    {\n        int L() =>\n#if X\n            1;\n#else\n            2;\n#endif\n\n        return L();\n    }";
        var parseOptions = CreateOptions(defineX, "X");

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass($"    private Func<int> _f = () =>\n{body}, _g;"), "#if X", parseOptions: parseOptions);
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass($"    private Func<int> _f = () =>\n{body};"), "#if X", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that of a comment behind the arrow of an expression body and a comment of another shape, the first in the text is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-012")]
    public void FirstShapeInTheTextIsReported()
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass("    int M(int i) => /* first */ i + /* second */ 1;"), "/* first */");
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInClass("    int M(int i) => i + /* first */ 1;\n\n    int N() => /* second */ 1;"), "/* first */");
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInClass("    int M(bool b) => b ? /* first */ 1 : 2;\n\n    int N() => /* second */ 1;"), "/* first */");
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInClass("    void M()\n    {\n        int /* first */ x = 1;\n    }\n\n    int N() => /* second */ 1;"), "/* first */");
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass("    int N() => /* first */ 1;\n\n    void M()\n    {\n        int /* second */ x = 1;\n    }"), "/* first */");
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, WrapInClass("    int N() => /* first */ 1;\n\n    int P => /* second */ 2;"), "/* first */");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment behind the arrow
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
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
    /// Verifies that the node-level entry points refuse a target or a context that holds the expression body, and format the
    /// method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
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

                var arrowResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, root.DescendantNodes().OfType<ArrowExpressionClauseSyntax>().Single(), TestContext.CancellationToken);

                Assert.AreEqual(text, (await arrowResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(arrowResult.Unsupported, text);

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
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for a node that holds the arrow
    /// clause, and nothing for the expression, whose leading trivia holds the comment; and that the node-level entry points do not
    /// refuse the expression, neither as a target in its document nor as a node of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [Requirement("FMT-UNSUP-003")]
    public async Task FindUnsupportedSyntaxAnswersForTheArrowClauseAndNotForTheExpressionAlone()
    {
        const string input = "class C\n{\n    int M() =>\n        // c\n        1;\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var arrow = root.DescendantNodes().OfType<ArrowExpressionClauseSyntax>().Single();

                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(arrow), text);
                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(FindMethod(root, "M")), text);
                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root), text);
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(arrow.Expression));
                Assert.IsNull(ReihitsuFormatter.FormatNode(arrow.Expression, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.IsNull((await ReihitsuFormatter.FormatNodeInDocumentAsync(document, arrow.Expression, TestContext.CancellationToken)).Unsupported);
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds, which stands in a
    /// tree of its own: a member built with a comment behind its arrow is refused, and an expression built with a comment in front
    /// of it is not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-012")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode()
    {
        var member = SyntaxFactory.ParseMemberDeclaration("int M() => /* c */ 1;");

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(member);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(ExpressionBodyArrowCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, member.ToFullString(), "/* c */", 0, "LF");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(SyntaxFactory.ParseExpression("/* c */ 1")));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same tree
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("class C\n{\n    int M() => /* c */ 1;\n}")]
    [DataRow("class C\n{\n    int P =>\n        // c\n        1;\n}")]
    [DataRow("class C\n{\n    int P\n    {\n        get =>\n#if DEBUG\n            1;\n#else\n            2;\n#endif\n    }\n}")]
    [DataRow("class C\n{\n    int P\n        // c\n        => 1;\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        System.Func<int> f = () => /* c */ 1;\n    }\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: inputs with a comment in front of the <c>get</c> keyword, with comments
    /// inside accessor blocks that become expression bodies, and with a blank line behind an arrow, are formatted, and their output,
    /// formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    int D { /* c */ get => _v; }", DocumentationMode.Parse, DisplayName = "Comment in front of the get keyword")]
    [DataRow("    int E { get { /* c */ return _v; } }", DocumentationMode.Parse, DisplayName = "Comment in front of the return of a getter block")]
    [DataRow("    int F { get { return _v; /* c */ } }", DocumentationMode.Parse, DisplayName = "Comment behind the return of a getter block")]
    [DataRow("    int G { get { return /* c */ _v; } }", DocumentationMode.Parse, DisplayName = "Comment behind the return keyword of a getter block")]
    [DataRow("    int H { get { return // c\n                _v; } }", DocumentationMode.Parse, DisplayName = "Line comment behind the return keyword of a getter block")]
    [DataRow("    int S { set { /* c */ _v = value; } }", DocumentationMode.Parse, DisplayName = "Comment in front of the statement of a setter block")]
    [DataRow("    int A =>\n\n        1;", DocumentationMode.Parse, DisplayName = "Blank line behind a property's arrow")]
    [DataRow("    int M() =>\n\n        1;", DocumentationMode.Parse, DisplayName = "Blank line behind a method's arrow")]
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
    /// Asserts that the outcome reports the reason of FMT-UNSUP-012 and the span of the comment <c>// c</c> in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(ExpressionBodyArrowCommentReason, unsupported.Reason);
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