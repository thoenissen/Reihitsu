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
/// Tests for the unsupported shape of a comment, a directive, or disabled text behind an assignment operator or the <c>=</c> of an
/// equals-value clause (FMT-UNSUP-020): every assignment operator wherever the assignment stands, every owner of an equals-value
/// clause, every kind of trivia on one line and across lines, documentation comments in every documentation mode, the gaps in
/// front of the operator and inside the value, the <c>=</c> tokens of other shapes, the shapes that keep their own reason, every
/// entry point, node-level inputs that hold only the value, and the closure of the formatter's output, each under LF and CRLF line
/// endings
/// </summary>
[TestClass]
public class UnsupportedAssignmentCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> that holds a comment behind the <c>=</c> of an assignment, beside a method <c>N</c> that holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    int M(int x)
                                                                    {
                                                                        x = // c
                                                                            1;

                                                                        return x;
                                                                    }

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: the <c>=</c> of a property initializer and of an assignment statement are refused, the
    /// input is returned unchanged, and the first comment, which starts in line 3, column 24, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void SpecExampleStatementAndPropertyInitializerAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 int[] G { get; } = // values
                                     { 1, 2 };

                                 int M(int x, int y)
                                 {
                                     x = // reset
                                         y + 1;

                                     return x;
                                 }
                             }
                             """;

        AssertUnsupportedFor(AssignmentCommentReason, input, "// values");
        AssertStartsAt(input, 2, 23);
    }

    /// <summary>
    /// Verifies the specification example: the members of an object initializer, of a <c>with</c> initializer, and of an index
    /// initializer are assignments, a comment on one line is refused as well, the input is returned unchanged, and the first
    /// comment, which starts in line 12, column 13, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void SpecExampleInitializerMembersAndACommentOnOneLineAreUnsupported()
    {
        const string input = """
                             using System.Collections.Generic;

                             record R
                             {
                                 public int A { get; set; }
                             }

                             class C
                             {
                                 object M(int x, R r)
                                 {
                                     x = /* one */ 1;

                                     var o = new R
                                             {
                                                 A = /* c1 */ 3
                                             };
                                     var p = r with
                                               {
                                                   A = /* c2 */ 4
                                               };
                                     var d = new Dictionary<int, int>
                                             {
                                                 [0] = /* c3 */ 1
                                             };

                                     return x + o.A + p.A + d.Count;
                                 }
                             }
                             """;

        AssertUnsupportedFor(AssignmentCommentReason, input, "/* one */");
        AssertStartsAt(input, 11, 12);
    }

    /// <summary>
    /// Verifies the specification example: a compound assignment operator and a deconstruction, with and without declared
    /// variables, are refused, the input is returned unchanged, and the first comment, which starts in line 5, column 14, is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void SpecExampleCompoundOperatorAndDeconstructionAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 int M(int x, int? y, (int, int) t)
                                 {
                                     x += // add
                                         3;
                                     y ??= /* fallback */ 0;
                                     var (a, b) = // pair
                                         t;
                                     (x, a) = /* swap */ (a, x);

                                     return x + a + b;
                                 }
                             }
                             """;

        AssertUnsupportedFor(AssignmentCommentReason, input, "// add");
        AssertStartsAt(input, 4, 13);
    }

    /// <summary>
    /// Verifies the specification example: an assignment inside a switch arm or an anonymous-object member is refused like any
    /// other, with this requirement's reason, the input is returned unchanged, and the first comment, which starts in line 7,
    /// column 30, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void SpecExampleAssignmentInsideAnArmOrAMemberIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 object M(int x, int y)
                                 {
                                     var w = new
                                             {
                                                 A = x += // c1
                                                     3
                                             };

                                     return y switch
                                            {
                                                1 => x -= /* c2 */ 2,
                                                _ => w,
                                            };
                                 }
                             }
                             """;

        AssertUnsupportedFor(AssignmentCommentReason, input, "// c1");
        AssertStartsAt(input, 6, 29);
    }

    /// <summary>
    /// Verifies the specification example: the <c>=</c> of an enum member, of a property, of a parameter's default value, and of a
    /// variable declarator are refused, the input is returned unchanged, and the first comment, which starts in line 3, column 9,
    /// is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void SpecExampleEqualsOfEnumMemberPropertyParameterAndDeclaratorAreUnsupported()
    {
        const string input = """
                             enum E
                             {
                                 A = // one
                                     1
                             }

                             class C
                             {
                                 int P { get; } =
                             #if DEBUG
                                     1;
                             #else
                                     2;
                             #endif

                                 int M(int y = /* default */ 1)
                                 {
                                     int x =
                             #if DEBUG
                                         1;
                             #else
                                         2;
                             #endif

                                     return x + y;
                                 }
                             }
                             """;

        AssertUnsupportedFor(AssignmentCommentReason, input, "// one");
        AssertStartsAt(input, 2, 8);
    }

    /// <summary>
    /// Verifies the specification example: a directive behind the <c>=</c> of an assignment is refused, the input is returned
    /// unchanged, and the directive, which starts in line 6, column 1, is reported, in a build that defines <c>DEBUG</c> and in
    /// one that does not
    /// </summary>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void SpecExampleDirectiveBehindAnAssignmentIsUnsupported(bool defineDebug)
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     x =
                             #if DEBUG
                                         1;
                             #else
                                         2;
                             #endif

                                     return x;
                                 }
                             }
                             """;

        var parseOptions = defineDebug ? CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG") : CSharpParseOptions.Default;

        AssertUnsupportedFor(AssignmentCommentReason, input, "#if DEBUG", parseOptions: parseOptions);
        AssertStartsAt(input, 5, 0, parseOptions);
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment in front of the <c>=</c> of a property, of a parameter, and
    /// of an assignment, and one inside the value behind its first token do not refuse the input, which is formatted as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void SpecExampleCommentsOutsideTheShapeAreSupported()
    {
        const string input = """
                             class C
                             {
                                 int P { get; } /* c1 */ = 1;

                                 int M(int x, int y /* c2 */ = 1)
                                 {
                                     x /* c3 */ = 1;
                                     x = y /* c4 */ + 1;

                                     return x;
                                 }
                             }
                             """;

        AssertSupported(input);
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that every assignment operator refuses the input, on one line and across lines
    /// </summary>
    /// <param name="statement">The statement that holds the assignment</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("        x = /* c */ 1;", "/* c */", DisplayName = "=")]
    [DataRow("        x += /* c */ 1;", "/* c */", DisplayName = "+=")]
    [DataRow("        x -= /* c */ 1;", "/* c */", DisplayName = "-=")]
    [DataRow("        x *= /* c */ 1;", "/* c */", DisplayName = "*=")]
    [DataRow("        x /= /* c */ 1;", "/* c */", DisplayName = "/=")]
    [DataRow("        x %= /* c */ 1;", "/* c */", DisplayName = "%=")]
    [DataRow("        x &= /* c */ 1;", "/* c */", DisplayName = "&=")]
    [DataRow("        x |= /* c */ 1;", "/* c */", DisplayName = "|=")]
    [DataRow("        x ^= /* c */ 1;", "/* c */", DisplayName = "^=")]
    [DataRow("        x <<= /* c */ 1;", "/* c */", DisplayName = "<<=")]
    [DataRow("        x >>= /* c */ 1;", "/* c */", DisplayName = ">>=")]
    [DataRow("        x >>>= /* c */ 1;", "/* c */", DisplayName = ">>>=")]
    [DataRow("        n ??= /* c */ 1;", "/* c */", DisplayName = "??=")]
    [DataRow("        x = // c\n            1;", "// c", DisplayName = "= across lines")]
    [DataRow("        x -= // c\n            1;", "// c", DisplayName = "-= across lines")]
    [DataRow("        n ??= // c\n            1;", "// c", DisplayName = "??= across lines")]
    public void EveryAssignmentOperatorIsUnsupported(string statement, string reported)
    {
        AssertUnsupportedFor(AssignmentCommentReason, WrapInMethod(statement), reported);
    }

    /// <summary>
    /// Verifies that an assignment refuses the input wherever it stands: in an argument, a lambda, a <c>for</c> incrementor, a
    /// chain of assignments, a ref assignment, every kind of deconstruction, the member of an initializer, a declaration's
    /// initializer value, a switch arm and its <c>when</c> clause, an anonymous-object member, and a lambda inside a member
    /// </summary>
    /// <param name="statement">The statement that holds the assignment</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("        M2(x = /* c */ 1);", "/* c */", DisplayName = "Argument")]
    [DataRow("        Action f = () => x = /* c */ 1;", "/* c */", DisplayName = "Lambda with an expression body")]
    [DataRow("        Action g = () =>\n                   {\n                       x += // c\n                           3;\n                   };", "// c", DisplayName = "Statement in a lambda's block")]
    [DataRow("        for (var i = 0; i < 3; i = /* c */ i + 1)\n        {\n        }", "/* c */", DisplayName = "For incrementor")]
    [DataRow("        x = y = /* c */ 1;", "/* c */", DisplayName = "Inner assignment of a chain")]
    [DataRow("        x = /* c */ y = 1;", "/* c */", DisplayName = "Outer assignment of a chain")]
    [DataRow("        ref int k = ref x;\n        k = /* c */ ref y;", "/* c */", DisplayName = "Ref assignment")]
    [DataRow("        (x, y) = /* c */ t;", "/* c */", DisplayName = "Deconstruction")]
    [DataRow("        var (a, b) = // c\n            t;", "// c", DisplayName = "Deconstruction declaration")]
    [DataRow("        (var a, y) = /* c */ t;", "/* c */", DisplayName = "Mixed deconstruction")]
    [DataRow("        var q = new R { A = /* c */ 3 };", "/* c */", DisplayName = "Object-initializer member in a declaration's initializer value")]
    [DataRow("        var q = r with { A = /* c */ 3 };", "/* c */", DisplayName = "With-initializer member in a declaration's initializer value")]
    [DataRow("        var q = new Dictionary<int, int> { [0] = /* c */ 1 };", "/* c */", DisplayName = "Index-initializer member in a declaration's initializer value")]
    [DataRow("        o = new R { A = // c\n            3 };", "// c", DisplayName = "Object-initializer member across lines")]
    [DataRow("        o = new R\n            {\n                A =\n                    // c\n                    3\n            };", "// c", DisplayName = "Object-initializer member, comment on its own line")]
    [DataRow("        var q = y switch\n                {\n                    1 => x = /* c */ 2,\n                    _ => 0,\n                };", "/* c */", DisplayName = "Arm expression")]
    [DataRow("        var q = y switch\n                {\n                    int i when (x += /* c */ i) > 0 => 1,\n                    _ => 0,\n                };", "/* c */", DisplayName = "When clause of an arm")]
    [DataRow("        var q = y switch\n                {\n                    1 => y switch\n                         {\n                             2 => x = /* c */ 2,\n                             _ => 0,\n                         },\n                    _ => 0,\n                };", "/* c */", DisplayName = "Arm of a nested switch expression")]
    [DataRow("        var q = new { A = x = /* c */ 3 };", "/* c */", DisplayName = "Anonymous-object member")]
    [DataRow("        var q = new { A = (Func<int>)(() => x += /* c */ 1) };", "/* c */", DisplayName = "Lambda inside an anonymous-object member")]
    [DataRow("        int[] q = [y -= // c\n            2];", "// c", DisplayName = "Collection element")]
    [DataRow("        var q = $\"{(x = /* c */ 1)}\";", "/* c */", DisplayName = "Interpolation hole")]
    public void AssignmentIsUnsupportedWhereverItStands(string statement, string reported)
    {
        AssertUnsupportedFor(AssignmentCommentReason, WrapInMethod(statement), reported);
    }

    /// <summary>
    /// Verifies that an assignment inside the initializer value of a field refuses the input with this requirement's reason,
    /// although FMT-UNSUP-004 excepts the initializer value of a declaration
    /// </summary>
    /// <param name="member">The member that holds the assignment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("    private R _r = new R { A = /* c */ 1 };", DisplayName = "Field")]
    [DataRow("    private R _r = new R { A = /* c */ 1 }, _s;", DisplayName = "Field with several declarators")]
    [DataRow("    R P { get; } = new R { A = /* c */ 1 };", DisplayName = "Property")]
    public void AssignmentInsideTheInitializerValueOfAMemberIsUnsupported(string member)
    {
        AssertUnsupportedFor(AssignmentCommentReason, WrapInClass(member), "/* c */");
    }

    /// <summary>
    /// Verifies that every kind of trivia behind an assignment operator refuses the input: a comment in the trailing trivia of the
    /// operator and one in the leading trivia of the right operand's first token, a documentation comment, a directive, and a
    /// region
    /// </summary>
    /// <param name="statement">The statement that holds the assignment</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("        x =\n            // c\n            1;", "// c", DisplayName = "Line comment on its own line")]
    [DataRow("        x =\n            /* c */ 1;", "/* c */", DisplayName = "Delimited comment leading the operand's line")]
    [DataRow("        x = /** c */ 1;", "/** c */", DisplayName = "Delimited documentation comment")]
    [DataRow("        x =\n#pragma warning disable CS0168\n            1;", "#pragma warning disable CS0168", DisplayName = "Pragma")]
    [DataRow("        x =\n#region r\n            1;\n#endregion", "#region r", DisplayName = "Region")]
    [DataRow("        x +=\n#if X\n            1;\n#else\n            2;\n#endif", "#if X", DisplayName = "Conditional group behind a compound operator")]
    public void EveryKindOfTriviaBehindTheOperatorIsUnsupported(string statement, string reported)
    {
        AssertUnsupportedFor(AssignmentCommentReason, WrapInMethod(statement), reported);
    }

    /// <summary>
    /// Verifies that disabled text behind an assignment operator refuses the input in the build that disables it: where <c>X</c>
    /// is defined, the <c>#else</c> branch is disabled text, and the input is still refused at the <c>#if</c>
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ConditionalGroupBehindTheOperatorIsUnsupportedInEveryBuild(bool defineX)
    {
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;

        AssertUnsupportedFor(AssignmentCommentReason, WrapInMethod("        x =\n#if X\n            1;\n#else\n            2;\n#endif"), "#if X", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that a documentation comment behind an assignment operator or an equals-value clause's <c>=</c> refuses the input
    /// in every documentation mode, in which it is a documentation comment or an ordinary comment
    /// </summary>
    /// <param name="statement">The statement that holds the operator</param>
    /// <param name="comment">The comment that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("        x = /** c */ 1;", "/** c */", DocumentationMode.Parse)]
    [DataRow("        x = /** c */ 1;", "/** c */", DocumentationMode.None)]
    [DataRow("        x = /** c */ 1;", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("        x = /// d\n            1;", "/// d", DocumentationMode.Parse)]
    [DataRow("        x = /// d\n            1;", "/// d", DocumentationMode.None)]
    [DataRow("        x = /// d\n            1;", "/// d", DocumentationMode.Diagnose)]
    [DataRow("        x =\n            /// <summary>c</summary>\n            1;", "/// <summary>c</summary>", DocumentationMode.Parse)]
    [DataRow("        x =\n            /// <summary>c</summary>\n            1;", "/// <summary>c</summary>", DocumentationMode.None)]
    [DataRow("        x =\n            /// <summary>c</summary>\n            1;", "/// <summary>c</summary>", DocumentationMode.Diagnose)]
    public void DocumentationCommentBehindTheOperatorIsUnsupportedInEveryDocumentationMode(string statement, string comment, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(AssignmentCommentReason, WrapInMethod(statement), comment, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that a documentation comment behind the <c>=</c> of a property and of an enum member refuses the input in every
    /// documentation mode
    /// </summary>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.None)]
    [DataRow(DocumentationMode.Diagnose)]
    public void DocumentationCommentBehindTheEqualsOfADeclarationIsUnsupportedInEveryDocumentationMode(DocumentationMode documentationMode)
    {
        var parseOptions = CSharpParseOptions.Default.WithDocumentationMode(documentationMode);

        AssertUnsupportedFor(AssignmentCommentReason, WrapInClass("    int P { get; } =\n        /// <summary>c</summary>\n        1;"), "/// <summary>c</summary>", parseOptions: parseOptions);
        AssertUnsupportedFor(AssignmentCommentReason, "enum E\n{\n    A = /** c */ 1\n}", "/** c */", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that the <c>=</c> of every owner of an equals-value clause refuses the input: a property, an enum member, the
    /// default value of a parameter of a method, a local function, a lambda, an indexer, a constructor, and a record, and a
    /// directive behind the <c>=</c> of a variable declarator of a local, a field, an event field, a constant, a <c>using</c>
    /// statement, a <c>fixed</c> statement, and a <c>for</c> initializer
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("class C\n{\n    int P { get; } = /* c */ 1;\n}", "/* c */", DisplayName = "Property, one line")]
    [DataRow("class C\n{\n    int P { get; } =\n        // c\n        1;\n}", "// c", DisplayName = "Property, comment on its own line")]
    [DataRow("class C\n{\n    int P { get; } =\n#pragma warning disable CS0168\n        1;\n}", "#pragma warning disable CS0168", DisplayName = "Property, directive")]
    [DataRow("enum E\n{\n    A = /* c */ 1,\n    B = 2\n}", "/* c */", DisplayName = "Enum member, one line")]
    [DataRow("enum E\n{\n    A = 1,\n    B =\n#if X\n        2\n#else\n        3\n#endif\n}", "#if X", DisplayName = "Enum member, directive")]
    [DataRow("class C\n{\n    void M(int y = // c\n               1)\n    {\n    }\n}", "// c", DisplayName = "Parameter of a method")]
    [DataRow("class C\n{\n    void M()\n    {\n        void L(int y = /* c */ 1)\n        {\n        }\n    }\n}", "/* c */", DisplayName = "Parameter of a local function")]
    [DataRow("class C\n{\n    void M()\n    {\n        var f = (int y = /* c */ 1) => y;\n    }\n}", "/* c */", DisplayName = "Parameter of a lambda")]
    [DataRow("class C\n{\n    int this[int i = /* c */ 0] => i;\n}", "/* c */", DisplayName = "Parameter of an indexer")]
    [DataRow("class C\n{\n    C(int y = /* c */ 1)\n    {\n    }\n}", "/* c */", DisplayName = "Parameter of a constructor")]
    [DataRow("record R(int Y = /* c */ 1);", "/* c */", DisplayName = "Parameter of a record")]
    [DataRow("class C\n{\n    void M()\n    {\n        int x =\n#if X\n            1;\n#else\n            2;\n#endif\n    }\n}", "#if X", DisplayName = "Local declarator, directive")]
    [DataRow("class C\n{\n    int _x =\n#pragma warning disable CS0168\n        1;\n}", "#pragma warning disable CS0168", DisplayName = "Field declarator, directive")]
    [DataRow("using System;\n\nclass C\n{\n    event Action E =\n#pragma warning disable CS0168\n        null;\n}", "#pragma warning disable CS0168", DisplayName = "Event field declarator, directive")]
    [DataRow("class C\n{\n    const int K =\n#pragma warning disable CS0168\n        1;\n}", "#pragma warning disable CS0168", DisplayName = "Constant declarator, directive")]
    [DataRow("using System.IO;\n\nclass C\n{\n    void M()\n    {\n        using (var s =\n#pragma warning disable CS0168\n               new MemoryStream())\n        {\n        }\n    }\n}", "#pragma warning disable CS0168", DisplayName = "Using statement declarator, directive")]
    [DataRow("class C\n{\n    unsafe void M(int[] a)\n    {\n        fixed (int* p =\n#pragma warning disable CS0168\n               a)\n        {\n        }\n    }\n}", "#pragma warning disable CS0168", DisplayName = "Fixed statement declarator, directive")]
    [DataRow("class C\n{\n    void M()\n    {\n        for (var i =\n#pragma warning disable CS0168\n                 0; i < 3; i++)\n        {\n        }\n    }\n}", "#pragma warning disable CS0168", DisplayName = "For initializer declarator, directive")]
    public void EqualsOfEveryOwnerOfAnEqualsValueClauseIsUnsupported(string input, string reported)
    {
        AssertUnsupportedFor(AssignmentCommentReason, input, reported);
    }

    /// <summary>
    /// Verifies the other side: a comment in front of the operator or the <c>=</c>, one inside the right operand or the value
    /// behind its first token, one behind the <c>ref</c> of a ref assignment's value, and one behind the statement do not refuse
    /// the input
    /// </summary>
    /// <param name="statement">The statement that holds the assignment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("        x /* c */ = 1;", DisplayName = "In front of =")]
    [DataRow("        x /* c */ += 1;", DisplayName = "In front of a compound operator")]
    [DataRow("        x = y /* c */ + 1;", DisplayName = "Inside the value behind its first token")]
    [DataRow("        x = M2(/* c */ 1);", DisplayName = "Inside an argument of the value")]
    [DataRow("        ref int k = ref x;\n        k = ref /* c */ y;", DisplayName = "Behind the ref of the value")]
    [DataRow("        x = 1; // c", DisplayName = "Behind the statement")]
    [DataRow("        // c\n        x = 1;", DisplayName = "Above the statement")]
    [DataRow("        var q = new R { A /* c */ = 3 };", DisplayName = "In front of the = of an initializer member")]
    [DataRow("        x =\n            1;", DisplayName = "Line break alone")]
    public void CommentOutsideTheGapIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies the other side for the equals-value clauses: a comment in front of the <c>=</c> of a property, a parameter, an enum
    /// member, and a variable declarator, and one behind the value do not refuse the input
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("class C\n{\n    int P { get; } /* c */ = 1;\n}", DisplayName = "In front of a property's =")]
    [DataRow("class C\n{\n    void M(int y /* c */ = 1)\n    {\n    }\n}", DisplayName = "In front of a parameter's =")]
    [DataRow("enum E\n{\n    A /* c */ = 1\n}", DisplayName = "In front of an enum member's =")]
    [DataRow("enum E\n{\n    A = 1 // c\n}", DisplayName = "Behind an enum member's value")]
    [DataRow("class C\n{\n    int P { get; } = 1; // c\n}", DisplayName = "Behind a property's initializer")]
    public void CommentOutsideTheGapOfAnEqualsValueClauseIsSupported(string input)
    {
        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that the <c>=</c> tokens of other shapes keep their reason: the <c>=</c> of an anonymous-object member declarator
    /// and of a <c>let</c> clause (FMT-UNSUP-017), of an attribute argument and of a using alias (FMT-UNSUP-022), and a comparison,
    /// which is a binary operator (FMT-UNSUP-011)
    /// </summary>
    /// <param name="reason">The reason that is given</param>
    /// <param name="input">The source text</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow(AnonymousMemberEqualsCommentReason, "class C\n{\n    object M()\n    {\n        return new { A = /* c */ 1 };\n    }\n}", "/* c */", DisplayName = "Anonymous-object member")]
    [DataRow(LetClauseEqualsCommentReason, "using System.Linq;\n\nclass C\n{\n    object M(int[] a)\n    {\n        return from v in a\n               let w = /* c */ v\n               select w;\n    }\n}", "/* c */", DisplayName = "Let clause")]
    [DataRow(NameEqualsCommentReason, "using System;\n\nclass NAttribute : Attribute\n{\n    public int Name { get; set; }\n}\n\n[N(Name = /* c */ 1)]\nclass C\n{\n}", "/* c */", DisplayName = "Attribute argument")]
    [DataRow(NameEqualsCommentReason, "using Text = /* c */ System.Text;\n\nclass C\n{\n}", "/* c */", DisplayName = "Using alias")]
    [DataRow(BinaryOperatorCommentReason, "class C\n{\n    bool M(int x)\n    {\n        return x == /* c */ 1;\n    }\n}", "/* c */", DisplayName = "Comparison")]
    [DataRow(BinaryOperatorCommentReason, "class C\n{\n    bool M(int x)\n    {\n        return x <= /* c */ 1;\n    }\n}", "/* c */", DisplayName = "Less than or equal")]
    public void EqualsOfOtherShapesKeepsItsReason(string reason, string input, string reported)
    {
        AssertUnsupportedFor(reason, input, reported);
    }

    /// <summary>
    /// Verifies the order of the shapes: a comment behind a variable declarator's <c>=</c> keeps FMT-UNSUP-004's reason, and a
    /// directive behind the <c>=</c> of a declarator in a field declaration with several declarators keeps FMT-UNSUP-010's
    /// </summary>
    /// <param name="reason">The reason that is given</param>
    /// <param name="input">The source text</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow(VariableDeclarationCommentReason, "class C\n{\n    void M()\n    {\n        int c = // c\n            1;\n    }\n}", "// c", DisplayName = "Comment behind a local declarator's =")]
    [DataRow(VariableDeclarationCommentReason, "class C\n{\n    int _c = /* c */ 1;\n}", "/* c */", DisplayName = "Comment behind a field declarator's =")]
    [DataRow(FieldDeclarationDirectiveReason, "class C\n{\n    int _a = 1, _b =\n#pragma warning disable CS0168\n        2;\n}", "#pragma warning disable CS0168", DisplayName = "Directive behind a declarator's = in a field with several declarators")]
    public void OverlapWithTheVariableDeclarationShapesGivesTheirReason(string reason, string input, string reported)
    {
        AssertUnsupportedFor(reason, input, reported);
    }

    /// <summary>
    /// Verifies that an assignment inside disabled text is not checked: where <c>X</c> is not defined, the arm is disabled text and
    /// the input is formatted, and where it is defined, the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void AssignmentInDisabledTextIsNotChecked(bool defineX)
    {
        var input = WrapInMethod("        var q = y switch\n                {\n#if X\n                    1 => x = /* c */ 2,\n#endif\n                    _ => 0,\n                };");
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;

        if (defineX)
        {
            AssertUnsupportedFor(AssignmentCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that of two comments behind assignment operators the first in the text is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void FirstOfTwoCommentsIsReported()
    {
        AssertUnsupportedFor(AssignmentCommentReason, WrapInMethod("        x = /* first */ 1;\n        y = /* second */ 2;"), "/* first */");
        AssertUnsupportedFor(AssignmentCommentReason, WrapInMethod("        x = /* first */ y = /* second */ 1;"), "/* first */");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment behind the <c>=</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
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
                AssertReportsShape(result.Unsupported, text, "// c");
            }
        }
    }

    /// <summary>
    /// Verifies that the node-level entry points refuse a target or a context that holds the method with the comment behind the
    /// <c>=</c>, and format the method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
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
                var statement = root.DescendantNodes().OfType<ExpressionStatementSyntax>().Single();

                var nodeResult = ReihitsuFormatter.FormatNode(statement, cancellationToken: TestContext.CancellationToken);

                Assert.AreEqual(statement.ToFullString(), nodeResult.Output.ToFullString());
                AssertReportsShape(nodeResult.Unsupported, text, "// c");

                var targetResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, statement, TestContext.CancellationToken);

                Assert.AreEqual(text, (await targetResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsShape(targetResult.Unsupported, text, "// c");

                var contextResult = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, FindMethod(root, "N"), type, TestContext.CancellationToken);

                Assert.AreEqual(text, (await contextResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsShape(contextResult.Unsupported, text, "// c");

                var besideResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, FindMethod(root, "N"), TestContext.CancellationToken);

                Assert.IsNull(besideResult.Unsupported);
                Assert.AreNotEqual(text, (await besideResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString(), "The method beside the shape must be formatted.");
            }
        }
    }

    /// <summary>
    /// Verifies that a node-level input that holds the operator holds the shape, whatever it stands in: the assignment, the arm and
    /// the switch expression around it, and the anonymous-object member and the object around it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void NodeThatHoldsTheOperatorHoldsTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(WrapInMethod("        var q = y switch\n                {\n                    1 => x = /* c */ 2,\n                    _ => 0,\n                };\n        var w = new { A = x += /* d */ 3 };"), endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var assignments = root.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();
            var arm = root.DescendantNodes().OfType<SwitchExpressionArmSyntax>().First();
            var member = root.DescendantNodes().OfType<AnonymousObjectMemberDeclaratorSyntax>().Single();

            Assert.AreEqual(AssignmentCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(assignments[0])?.Reason);
            Assert.AreEqual(AssignmentCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(arm)?.Reason);
            Assert.AreEqual(AssignmentCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(arm.Parent)?.Reason);
            Assert.AreEqual(AssignmentCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(assignments[1])?.Reason);
            Assert.AreEqual(AssignmentCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(member)?.Reason);
            Assert.AreEqual(AssignmentCommentReason, ReihitsuFormatter.FormatNode(assignments[1], cancellationToken: TestContext.CancellationToken).Unsupported?.Reason);
        }
    }

    /// <summary>
    /// Verifies that an equals-value clause formatted without its owner holds the shape: the clause of a property, of an enum
    /// member, of a parameter, and of a variable declarator with a comment behind its <c>=</c> are refused with this requirement's
    /// reason, while the local declaration statement around the declarator keeps FMT-UNSUP-004's
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-020")]
    public void EqualsValueClauseWithoutItsOwnerHoldsTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings("enum E\n{\n    A = /* e */ 1\n}\n\nclass C\n{\n    int P { get; } = /* p */ 1;\n\n    void M(int y = /* y */ 1)\n    {\n        var d = /* d */ 1;\n    }\n}", endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var clauses = root.DescendantNodes().OfType<EqualsValueClauseSyntax>().ToList();

            Assert.HasCount(4, clauses);

            foreach (var clause in clauses)
            {
                var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(clause);

                Assert.AreEqual(AssignmentCommentReason, unsupported?.Reason, $"The clause '{clause}' holds the = and the comment.");
                Assert.AreEqual(clause.ToFullString(), ReihitsuFormatter.FormatNode(clause, cancellationToken: TestContext.CancellationToken).Output.ToFullString());
            }

            Assert.AreEqual(VariableDeclarationCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().Single())?.Reason);
        }
    }

    /// <summary>
    /// Verifies that a node-level input that holds the right operand or the value but neither the operator nor the <c>=</c> does
    /// not hold the shape, also with the comment in the leading trivia of its first token: the object creation behind a commented
    /// <c>=</c>, which the RH5301 fix formats, and the value of a property
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-020")]
    public void NodeThatHoldsOnlyTheValueDoesNotHoldTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(WrapInClass("    int P { get; } =\n        // p\n        1;\n\n    object M(object o)\n    {\n        o = // c\n            new R { A = 1 };\n        o =\n            // d\n            new R { A = 2 };\n\n        return o;\n    }"), endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var creations = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().ToList();
            var propertyValue = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Single(property => property.Identifier.ValueText == "P").Initializer.Value;

            Assert.HasCount(2, creations);

            foreach (var creation in creations)
            {
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(creation));
                Assert.IsNull(ReihitsuFormatter.FormatNode(creation, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.AreEqual(AssignmentCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(creation.Parent)?.Reason);
            }

            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(propertyValue));
            Assert.AreEqual(AssignmentCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(propertyValue.Parent.Parent)?.Reason);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds: a statement built
    /// with a comment behind <c>=</c> is refused, and the same statement without the comment is not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode()
    {
        var node = SyntaxFactory.ParseStatement("x += /* c */ 1;");

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(node);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(AssignmentCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, node.ToFullString(), "/* c */", 0, "LF");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(SyntaxFactory.ParseStatement("x += 1;")));
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: the join behind an assignment operator and an equals-value clause's
    /// <c>=</c>, and the documentation comments in front of an <c>=</c> and behind the value carry no comment into the
    /// gap, and the output, formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="statement">The statements whose gaps are joined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("        x =\n            1; // c", DisplayName = "Join behind =")]
    [DataRow("        x +=\n\n            1; // c", DisplayName = "Join behind a compound operator across a blank line")]
    [DataRow("        int[] z;\n        z =\n            [1, 2]; // c", DisplayName = "Join behind = in front of a collection expression")]
    [DataRow("        var v =\n            1; // c", DisplayName = "Join behind a declarator's =")]
    [DataRow("        var w = new { A = x +=\n            3 }; // c\n        var q = y switch\n                {\n                    1 => x =\n                        2,\n                    _ => 0,\n                };", DisplayName = "Join inside an arm and a member")]
    [DataRow("        x /** d */ = 1;", DisplayName = "Documentation comment in front of =")]
    [DataRow("        x = 1 /** d */;", DisplayName = "Documentation comment behind the value")]
    public void OutputOfJoinsAroundTheOperatorIsNotUnsupported(string statement)
    {
        AssertFormattedOutputIsNotRefused(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape for an equals-value clause: the join behind the <c>=</c> of a property,
    /// an enum member, and a parameter, and the relocation of a documentation comment written in front of a property's <c>=</c>,
    /// carry no comment into the gap, and the output, formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-020")]
    [DataRow("class C\n{\n    int[] G { get; } =\n        { 1, 2 }; // c\n}", DisplayName = "Property with an array initializer")]
    [DataRow("class C\n{\n    int[] G { get; } =\n        [1, 2]; // c\n}", DisplayName = "Property with a collection expression")]
    [DataRow("enum E\n{\n    A =\n        1, // c\n    B = 2\n}", DisplayName = "Enum member")]
    [DataRow("class C\n{\n    void M(int y =\n               1) // c\n    {\n    }\n}", DisplayName = "Parameter")]
    [DataRow("class C\n{\n    int P { get; } /** c */ = 1;\n}", DisplayName = "Documentation comment in front of a property's =")]
    public void OutputOfJoinsBehindTheEqualsOfADeclarationIsNotUnsupported(string input)
    {
        AssertFormattedOutputIsNotRefused(input);
    }

    /// <summary>
    /// Wraps statements in a method of a class, with the parameters and helpers they use
    /// </summary>
    /// <param name="statements">The statements, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statements)
    {
        return WrapInClass($"    object M(int x, int y, int? n, object o, (int, int) t, R r)\n    {{\n{statements}\n\n        return null;\n    }}");
    }

    /// <summary>
    /// Wraps members in a class <c>C</c>, with the usings, the record <c>R</c>, and the helper <c>M2</c> its members use
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string members)
    {
        return $"using System;\nusing System.Collections.Generic;\n\nrecord R\n{{\n    public int A {{ get; set; }}\n}}\n\nclass C\n{{\n{members}\n\n    int M2(int v)\n    {{\n        return v;\n    }}\n}}";
    }

    /// <summary>
    /// Asserts that the outcome reports this requirement's reason and the span of the given comment in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    /// <param name="comment">The comment that is reported</param>
    private static void AssertReportsShape(UnsupportedSyntax unsupported, string text, string comment)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(AssignmentCommentReason, unsupported.Reason);
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

    /// <summary>
    /// Asserts that the formatter reports the shape of the input in the given line and column, under LF and CRLF line endings
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="line">The zero-based line</param>
    /// <param name="character">The zero-based column</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    private void AssertStartsAt(string input, int line, int character, CSharpParseOptions parseOptions = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), parseOptions, cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(new LinePosition(line, character), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
        }
    }

    /// <summary>
    /// Formats the input, then formats its output again, under LF and CRLF line endings, and verifies that neither pass refuses its
    /// input and that the second pass changes nothing
    /// </summary>
    /// <param name="input">The source text</param>
    private void AssertFormattedOutputIsNotRefused(string input)
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

    #endregion // Methods
}