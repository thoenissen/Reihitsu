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
/// Tests for the unsupported shape of a comment, a directive, or disabled text between a binary operator and its right operand
/// (FMT-UNSUP-011): every binary operator, every kind of comment in every documentation mode, directives and disabled text in
/// both builds, every enclosing construct, the excepted places in front of the operator and inside the right operand, the
/// operators that are no binary operator, the shapes that keep their own reason, every entry point, and the closure of the
/// formatter's output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedBinaryOperatorCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> that holds a binary expression with a comment behind its operator, beside a method <c>N</c> that holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    int M(int i)
                                                                    {
                                                                        return i + // c
                                                                            1;
                                                                    }

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: each binary expression holds a comment behind its operator, the input is refused, and
    /// the first comment, which starts in line 5, column 22, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    public void SpecExampleCommentsBehindBinaryOperatorsAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 bool M(bool a, bool b, int n, object o)
                                 {
                                     var x = a && // c1
                                             b;
                                     var y = n
                                             + // c2
                                             1;
                                     var z = n * /* c3 */ 2;
                                     var w = a
                                             && b
                                             ||
                                             // c4
                                             o is string;
                                     var v = o as /* c5 */ string ?? /* c6 */ "";

                                     return x;
                                 }
                             }
                             """;

        AssertUnsupportedFor(BinaryOperatorCommentReason, input, "// c1");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(4, 21), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies each binary expression of the specification example on its own: the input is refused, and its first comment is
    /// reported
    /// </summary>
    /// <param name="statement">The statement that holds the binary expression</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("        var x = a && // c1\n                b;", "// c1", DisplayName = "Line comment behind a logical operator at the end of its line")]
    [DataRow("        var y = i\n                + // c2\n                1;", "// c2", DisplayName = "Line comment behind an operator at the start of its line")]
    [DataRow("        var z = i * /* c3 */ 2;", "/* c3 */", DisplayName = "Block comment behind an operator on one line")]
    [DataRow("        var w = a\n                && b\n                ||\n                // c4\n                o is string;", "// c4", DisplayName = "Comment line behind a lone operator")]
    [DataRow("        var v = o as /* c5 */ string ?? /* c6 */ \"\";", "/* c5 */", DisplayName = "Comments behind an as and a null-coalescing operator")]
    public void EachBinaryExpressionOfTheSpecExampleIsUnsupported(string statement, string comment)
    {
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies that a comment behind every binary operator refuses the input: arithmetic, shift, relational, equality, bitwise,
    /// logical, and null-coalescing operators, the <c>is</c> of a type test, and the <c>as</c> of a conversion
    /// </summary>
    /// <param name="expression">The binary expression</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("i + /* c */ j", DisplayName = "Addition")]
    [DataRow("i - /* c */ j", DisplayName = "Subtraction")]
    [DataRow("i * /* c */ j", DisplayName = "Multiplication")]
    [DataRow("i / /* c */ j", DisplayName = "Division")]
    [DataRow("i % /* c */ j", DisplayName = "Modulo")]
    [DataRow("i << /* c */ j", DisplayName = "Left shift")]
    [DataRow("i >> /* c */ j", DisplayName = "Right shift")]
    [DataRow("i >>> /* c */ j", DisplayName = "Unsigned right shift")]
    [DataRow("i < /* c */ j", DisplayName = "Less than")]
    [DataRow("i <= /* c */ j", DisplayName = "Less than or equal")]
    [DataRow("i > /* c */ j", DisplayName = "Greater than")]
    [DataRow("i >= /* c */ j", DisplayName = "Greater than or equal")]
    [DataRow("i == /* c */ j", DisplayName = "Equals")]
    [DataRow("i != /* c */ j", DisplayName = "Not equals")]
    [DataRow("i & /* c */ j", DisplayName = "Bitwise and")]
    [DataRow("i | /* c */ j", DisplayName = "Bitwise or")]
    [DataRow("i ^ /* c */ j", DisplayName = "Exclusive or")]
    [DataRow("a && /* c */ b", DisplayName = "Logical and")]
    [DataRow("a || /* c */ b", DisplayName = "Logical or")]
    [DataRow("n ?? /* c */ i", DisplayName = "Null-coalescing")]
    [DataRow("o is /* c */ string", DisplayName = "Type test")]
    [DataRow("o as /* c */ string", DisplayName = "Conversion")]
    public void CommentBehindEveryBinaryOperatorIsUnsupported(string expression)
    {
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod($"        var r = {expression};"), "/* c */");
    }

    /// <summary>
    /// Verifies the specification example: an <c>is</c> followed by an enum member is read as the operator of a binary expression,
    /// the input is refused, and the comment, which starts in line 10, column 21, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    public void SpecExampleIsFollowedByAnEnumMemberIsUnsupported()
    {
        const string input = """
                             enum Color
                             {
                                 Red
                             }

                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is /* c */ Color.Red;
                                 }
                             }
                             """;

        AssertUnsupportedFor(BinaryOperatorCommentReason, input, "/* c */");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(9, 20), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies that a relational operator inside a pattern is no binary operator, and a comment in front of a combinator stands
    /// outside every refused gap: neither refuses the input
    /// </summary>
    /// <param name="expression">The pattern match</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("i is > /* c */ 1", DisplayName = "Relational pattern")]
    [DataRow("i is > 1 /* c */ and < 3", DisplayName = "In front of a combinator")]
    public void CommentBehindAPatternOperatorIsSupported(string expression)
    {
        AssertSupported(WrapInMethod($"        var r = {expression};"));
    }

    /// <summary>
    /// Verifies that the <c>is</c> of a pattern match and the pattern combinators are no binary operators: a comment behind them is
    /// not this shape, and the input is refused with the reason of the gap the line-break phase joins
    /// </summary>
    /// <param name="reason">The reason that is reported</param>
    /// <param name="expression">The pattern match</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(IsPatternCommentReason, "o is /* c */ 5", DisplayName = "Constant pattern")]
    [DataRow(IsPatternCommentReason, "o is /* c */ null", DisplayName = "Null pattern")]
    [DataRow(IsPatternCommentReason, "o is /* c */ not null", DisplayName = "Negated pattern")]
    [DataRow(IsPatternCommentReason, "o is /* c */ string t", DisplayName = "Declaration pattern")]
    [DataRow(IsPatternCommentReason, "o is /* c */ { }", DisplayName = "Property pattern")]
    [DataRow(PatternCombinatorCommentReason, "i is > 1 or /* c */ 3", DisplayName = "Or combinator")]
    [DataRow(PatternCombinatorCommentReason, "i is > 1 and /* c */ < 3", DisplayName = "And combinator")]
    public void CommentBehindIsOfAPatternMatchOrACombinatorGivesTheReasonOfTheJoinedGap(string reason, string expression)
    {
        AssertUnsupportedFor(reason, WrapInMethod($"        var r = {expression};"), "/* c */");
    }

    /// <summary>
    /// Verifies that every kind of comment refuses the input in each position of the gap: behind the operator on its line, on a
    /// line of its own in front of the right operand, as a multi-line block comment, without whitespace around it, and behind a
    /// tab
    /// </summary>
    /// <param name="statement">The statement that holds the binary expression</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("        var r = a && // c\n            b;", "// c", DisplayName = "Line comment behind the operator")]
    [DataRow("        var r = a &&\n            // c\n            b;", "// c", DisplayName = "Comment line in front of the right operand")]
    [DataRow("        var r = a &&\n\n            // c\n            b;", "// c", DisplayName = "Comment line behind a blank line in front of the right operand")]
    [DataRow("        var r = a && /* c\n               d */ b;", "/* c\n               d */", DisplayName = "Multi-line block comment")]
    [DataRow("        var r = a &&/* c */b;", "/* c */", DisplayName = "Block comment without whitespace")]
    [DataRow("        var r = a &&\t/* c */ b;", "/* c */", DisplayName = "Block comment behind a tab")]
    [DataRow("        var r = a && /* c */ /* d */ b;", "/* c */", DisplayName = "Two block comments")]
    public void EveryCommentPositionInTheGapIsUnsupported(string statement, string comment)
    {
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies that a documentation comment behind an operator refuses the input in every documentation mode, in which it is a
    /// documentation comment or an ordinary comment
    /// </summary>
    /// <param name="statement">The statement that holds the binary expression</param>
    /// <param name="comment">The comment that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("        var r = a && /** c */ b;", "/** c */", DocumentationMode.Parse)]
    [DataRow("        var r = a && /** c */ b;", "/** c */", DocumentationMode.None)]
    [DataRow("        var r = a && /** c */ b;", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("        var r = a &&\n            /** c */\n            b;", "/** c */", DocumentationMode.Parse)]
    [DataRow("        var r = a &&\n            /** c */\n            b;", "/** c */", DocumentationMode.None)]
    [DataRow("        var r = a &&\n            /** c */\n            b;", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("        var r = a &&\n            /// <summary>c</summary>\n            b;", "/// <summary>c</summary>", DocumentationMode.Parse)]
    [DataRow("        var r = a &&\n            /// <summary>c</summary>\n            b;", "/// <summary>c</summary>", DocumentationMode.None)]
    [DataRow("        var r = a &&\n            /// <summary>c</summary>\n            b;", "/// <summary>c</summary>", DocumentationMode.Diagnose)]
    [DataRow("        var r = a && /// c\n            b;", "/// c", DocumentationMode.Parse)]
    [DataRow("        var r = a && /// c\n            b;", "/// c", DocumentationMode.None)]
    [DataRow("        var r = a && /// c\n            b;", "/// c", DocumentationMode.Diagnose)]
    public void DocumentationCommentBehindAnOperatorIsUnsupportedInEveryDocumentationMode(string statement, string comment, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod(statement), comment, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies the specification example: directives and disabled text behind an operator refuse the input in every build, and
    /// the first directive is reported
    /// </summary>
    /// <param name="defineDebug">Whether the build defines <c>DEBUG</c></param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void SpecExampleConditionalOperandBehindATrailingOperatorIsUnsupported(bool defineDebug)
    {
        const string input = """
                             using System.Text.RegularExpressions;

                             class C
                             {
                                 RegexOptions M()
                                 {
                                     return RegexOptions.Compiled |
                             #if DEBUG
                                            RegexOptions.IgnoreCase |
                             #endif
                                            RegexOptions.Multiline;
                                 }
                             }
                             """;

        AssertUnsupportedFor(BinaryOperatorCommentReason, input, "#if DEBUG", parseOptions: CreateOptions(defineDebug, "DEBUG"));
    }

    /// <summary>
    /// Verifies that every kind of directive behind an operator refuses the input, also one that holds no disabled text, and a
    /// fully inactive group
    /// </summary>
    /// <param name="statement">The statement that holds the binary expression</param>
    /// <param name="directive">The directive that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("        var r = a &&\n#pragma warning disable CS0001\n            b;", "#pragma warning disable CS0001", DisplayName = "Pragma directive")]
    [DataRow("        var r = a &&\n#region R\n            b;\n#endregion", "#region R", DisplayName = "Region directive")]
    [DataRow("        var r = a &&\n#nullable disable\n            b;", "#nullable disable", DisplayName = "Nullable directive")]
    [DataRow("        var r = a &&\n#if false\n            c &&\n#endif\n            b;", "#if false", DisplayName = "Fully inactive group")]
    [DataRow("        var r = i +\n#if X\n            1\n#elif Y\n            2\n#else\n            3\n#endif\n            ;", "#if X", DisplayName = "Group with elif and else branches")]
    [DataRow("        var r = i + /* c */\n#pragma warning disable CS0001\n            j;", "/* c */", DisplayName = "Comment in front of a directive")]
    public void EveryDirectiveBehindAnOperatorIsUnsupported(string statement, string directive)
    {
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod(statement), directive);
    }

    /// <summary>
    /// Verifies that a conditional operand behind an operator refuses the input in both builds: in the build that defines <c>X</c>
    /// the directive stands behind the first operator, and in the other the directive and the disabled text stand behind it
    /// </summary>
    /// <param name="defineX">Whether the build defines <c>X</c></param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DisabledTextBehindAnOperatorIsUnsupportedInEveryBuild(bool defineX)
    {
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod("        var r = a &&\n#if X\n            b &&\n#endif\n            c;"), "#if X", parseOptions: CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies the condition about disabled text: in the build that does not define <c>X</c>, the <c>|</c> of the disabled text
    /// is not checked, and in both builds the directives stand in front of an operator, so the input is not refused
    /// </summary>
    /// <param name="defineX">Whether the build defines <c>X</c></param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DirectivesInFrontOfLeadingOperatorsAreSupportedInEveryBuild(bool defineX)
    {
        AssertSupported(WrapInMethod("        var r = i\n#if X\n                | j\n#endif\n                | 1;"), CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies that each binary expression is checked with its own operator and right operand: the second operator of a chain, one
    /// nested in an operand of another, one inside a lambda, an argument, an interpolation hole, a query clause, a collection, a
    /// switch expression arm, and a branch of a conditional expression
    /// </summary>
    /// <param name="statement">The statement that holds the binary expression</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("        var r = a && b && /* c */ c;", DisplayName = "Second operator of a chain")]
    [DataRow("        var r = a && /* c */ b && c;", DisplayName = "First operator of a chain")]
    [DataRow("        var r = i * (j + /* c */ 1);", DisplayName = "Nested in the right operand")]
    [DataRow("        var r = (i + /* c */ j) * 2;", DisplayName = "Nested in the left operand")]
    [DataRow("        var r = a && o is /* c */ string;", DisplayName = "Type test as the right operand of a logical operator")]
    [DataRow("        var r = F(() => i + /* c */ j);", DisplayName = "Inside a lambda")]
    [DataRow("        var r = F(i + /* c */ j);", DisplayName = "Inside an argument")]
    [DataRow("        var r = $\"{i + /* c */ j}\";", DisplayName = "Inside an interpolation hole")]
    [DataRow("        var r = from x in xs where x > /* c */ 1 select x;", DisplayName = "Inside a query clause")]
    [DataRow("        var r = new[] { i + /* c */ j };", DisplayName = "Inside a collection")]
    [DataRow("        var r = i switch { 1 => j + /* c */ 1, _ => 0 };", DisplayName = "Inside a switch expression arm")]
    [DataRow("        var r = a ? i + /* c */ j : 0;", DisplayName = "Inside a branch of a conditional expression")]
    [DataRow("        F(i + /* c */ j);", DisplayName = "Inside an expression statement")]
    [DataRow("        i = j + /* c */ 1;", DisplayName = "Inside an assigned value")]
    public void EveryBinaryExpressionIsCheckedWithItsOwnOperator(string statement)
    {
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies the specification example of the other side: no comment and no directive of the example stands behind a binary
    /// operator, and the input is formatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    public void SpecExampleCommentsInFrontOfTheOperatorsAreSupported()
    {
        const string input = """
                             using System.Text.RegularExpressions;

                             class C
                             {
                                 bool M(bool a, bool b, int n)
                                 {
                                     var x = a
                                             // c1
                                             && b;
                                     var y = a /* c2 */ && b;
                                     var z = n + M2(1) // c3
                                             + 2;
                                     var w = n is > /* c4 */ 1 or 3;
                                     var u = M3() /* c7 */ is 5;
                                     n += /* c6 */ 1;

                                     return x && y && z > 0 && w && u;
                                 }

                                 object M3()
                                 {
                                     return 5;
                                 }

                                 int M2(int v)
                                 {
                                     return v;
                                 }

                                 RegexOptions Options()
                                 {
                                     return RegexOptions.Compiled
                             #if DEBUG
                                            | RegexOptions.IgnoreCase
                             #endif
                                            | RegexOptions.Multiline;
                                 }
                             }
                             """;

        AssertSupported(input);
        AssertSupported(input, CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG"));
    }

    /// <summary>
    /// Verifies that the excepted places and the tokens that are no binary operator do not refuse the input: a comment in front of
    /// the operator, inside the right operand behind its first token, behind the right operand, behind an assignment, a compound
    /// assignment, a range, and a unary operator
    /// </summary>
    /// <param name="statement">The statement</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("        var r = a /* c */ && b;", DisplayName = "Behind the left operand")]
    [DataRow("        var r = a/* c */&& b;", DisplayName = "Behind the left operand without whitespace")]
    [DataRow("        var r = a // c\n            && b;", DisplayName = "Line comment behind the left operand")]
    [DataRow("        var r = a\n            // c\n            && b;", DisplayName = "Comment line above the operator's line")]
    [DataRow("        var r = a\n#pragma warning disable CS0001\n            && b;", DisplayName = "Directive above the operator's line")]
    [DataRow("        var r = a && (/* c */ b);", DisplayName = "Inside parentheses of the right operand")]
    [DataRow("        var r = i + F(/* c */ 1);", DisplayName = "Inside the right operand behind its first token")]
    [DataRow("        var r = i + -/* c */ 1;", DisplayName = "Behind a unary operator that starts the right operand")]
    [DataRow("        F(i + j /* c */);", DisplayName = "Behind the right operand")]
    [DataRow("        i = /* c */ j;", DisplayName = "Behind an assignment operator")]
    [DataRow("        i += /* c */ 1;", DisplayName = "Behind a compound assignment operator")]
    [DataRow("        n ??= /* c */ 1;", DisplayName = "Behind a null-coalescing assignment operator")]
    [DataRow("        var r = xs[1.. /* c */ 2];", DisplayName = "Behind a range operator")]
    [DataRow("        var r = -/* c */ i;", DisplayName = "Behind a unary operator")]
    public void CommentOutsideTheShapeIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies that a comment behind a conditional's <c>?</c> in front of a binary expression keeps the reason of FMT-UNSUP-008:
    /// it stands behind no binary operator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    public void CommentBehindAConditionalOperatorInFrontOfABinaryExpressionKeepsTheReasonOfAConditional()
    {
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInMethod("        var r = a ? /* c */ i + j : 0;"), "/* c */");
    }

    /// <summary>
    /// Verifies the specification's statement about FMT-UNSUP-004: a comment behind an operator in an initializer value of a local
    /// declaration or a field gives this requirement's reason, and one in an attribute argument of a declaration gives FMT-UNSUP-004's
    /// reason, while one in an attribute argument of a method gives this requirement's
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="reason">The reason that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("class C\n{\n    void M()\n    {\n        int c = 1 + /* c */ 2;\n    }\n}", BinaryOperatorCommentReason, DisplayName = "Local declaration value")]
    [DataRow("class C\n{\n    private int _a = 1 + /* c */ 2;\n}", BinaryOperatorCommentReason, DisplayName = "Field with one declarator")]
    [DataRow("class C\n{\n    private int _a = 1 + /* c */ 2, _b;\n}", BinaryOperatorCommentReason, DisplayName = "Field with several declarators")]
    [DataRow("using System;\n\nclass C\n{\n    [Obsolete(\"old\" + /* c */ \" API\")] private int _e;\n}", VariableDeclarationCommentReason, DisplayName = "Attribute argument of a field")]
    [DataRow("using System;\n\nclass C\n{\n    [Obsolete(\"old\" + /* c */ \" API\")]\n    void M()\n    {\n    }\n}", BinaryOperatorCommentReason, DisplayName = "Attribute argument of a method")]
    public void CommentBehindAnOperatorOfADeclarationKeepsTheReasonOfItsDeclaration(string input, string reason)
    {
        AssertUnsupportedFor(reason, input, "/* c */");
    }

    /// <summary>
    /// Verifies the specification's statement about FMT-UNSUP-010: a directive behind an operator inside a value of a field
    /// declaration with several declarators gives FMT-UNSUP-010's reason, and in a field with one declarator this requirement's, in
    /// both builds
    /// </summary>
    /// <param name="defineX">Whether the build defines <c>X</c></param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DirectiveBehindAnOperatorOfAFieldValueKeepsTheReasonOfItsDeclaration(bool defineX)
    {
        const string several = """
                               class C
                               {
                                   private int _a = 1 +
                               #if X
                                       2
                               #else
                                       3
                               #endif
                                       , _b;
                               }
                               """;
        const string one = """
                           class C
                           {
                               private int _a = 1 +
                           #if X
                                   2
                           #else
                                   3
                           #endif
                                   ;
                           }
                           """;

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, several, "#if X", parseOptions: CreateOptions(defineX, "X"));
        AssertUnsupportedFor(BinaryOperatorCommentReason, one, "#if X", parseOptions: CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies that of a comment behind a binary operator and a comment of another shape, the first in the text is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-011")]
    public void FirstShapeInTheTextIsReported()
    {
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod("        var x = i + /* first */ j;\n        int /* second */ y = 1;"), "/* first */");
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInMethod("        int /* first */ y = 1;\n        var x = i + /* second */ j;"), "/* first */");
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInMethod("        var x = a ? /* first */ 1 : 2;\n        var y = i + /* second */ j;"), "/* first */");
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod("        var x = i + /* first */ j;\n        var y = i + /* second */ j;"), "/* first */");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment behind the operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
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
    /// Verifies that the node-level entry points refuse a target or a context that holds the binary expression, and format the
    /// method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
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

                var binaryResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, root.DescendantNodes().OfType<BinaryExpressionSyntax>().Single(), TestContext.CancellationToken);

                Assert.AreEqual(text, (await binaryResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(binaryResult.Unsupported, text);

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
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for a node that holds the binary
    /// expression, and nothing for a node that holds only part of it: the right operand, whose leading trivia holds the comment,
    /// and the left operand; and that <see cref="ReihitsuFormatter.FormatNode"/> does not refuse the right operand
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [Requirement("FMT-UNSUP-003")]
    public void FindUnsupportedSyntaxAnswersForTheBinaryExpressionAndNotForPartOfIt()
    {
        const string input = "class C\n{\n    int M(int i)\n    {\n        return i +\n            // c\n            1;\n    }\n}";
        var root = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
        var binary = root.DescendantNodes().OfType<BinaryExpressionSyntax>().Single();

        AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(binary), input);
        AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root), input);
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(binary.Right));
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(binary.Left));
        Assert.IsNull(ReihitsuFormatter.FormatNode(binary.Right, cancellationToken: TestContext.CancellationToken).Unsupported);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same tree
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("class C\n{\n    int M(int i) => i + /* c */ 1;\n}")]
    [DataRow("class C\n{\n    bool M(bool a, bool b) => a &&\n        // c\n        b;\n}")]
    [DataRow("class C\n{\n    bool M(bool a, bool b) => a\n        // c\n        && b;\n}")]
    [DataRow("class C\n{\n    private int _a = 1 + /* c */ 2, _b;\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: inputs with comments in front of operators, which the line-break phase
    /// moves to the start of the next line and whose right operand it joins behind them, are formatted, and their output, formatted
    /// again, is neither refused nor changed
    /// </summary>
    /// <param name="statement">The statement that holds the binary expression</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-011")]
    [DataRow("        c = a /* c1 */ &&\n            b;", DocumentationMode.Parse, DisplayName = "Block comment in front of an operator that ends its line")]
    [DataRow("        c = a // c2\n            &&\n            b;", DocumentationMode.Parse, DisplayName = "Line comment in front of a lone operator")]
    [DataRow("        c = a &&\n\n            b;", DocumentationMode.Parse, DisplayName = "Blank line in front of the right operand")]
    [DataRow("        c = a &&\n            b /** d1 */ && c;", DocumentationMode.Parse, DisplayName = "Delimited documentation comment in front of a later operator")]
    [DataRow("        c = a &&\n            b /** d1 */ && c;", DocumentationMode.None, DisplayName = "Delimited documentation comment in front of a later operator, mode None")]
    [DataRow("        c = a &&\n            b /// d2\n            && c;", DocumentationMode.Parse, DisplayName = "Single-line documentation comment in front of a later operator")]
    [DataRow("        c = a &&\n            b /// d2\n            && c;", DocumentationMode.None, DisplayName = "Single-line documentation comment in front of a later operator, mode None")]
    [DataRow("        c = a\n            && b /// d3\n            && c;", DocumentationMode.Parse, DisplayName = "Single-line documentation comment behind a leading operator's operand")]
    [DataRow("        c = i + i /** d4 */ * i > 0;", DocumentationMode.Parse, DisplayName = "Delimited documentation comment in front of an operator on one line")]
    [DataRow("        c = a &&\n            b /** d5 */;", DocumentationMode.Parse, DisplayName = "Delimited documentation comment behind the last operand")]
    [DataRow("        c = a /** d6 */\n            && b;", DocumentationMode.Parse, DisplayName = "Delimited documentation comment behind the left operand")]
    [DataRow("        c = a\n            /// d7\n            && b;", DocumentationMode.Parse, DisplayName = "Single-line documentation comment above the operator's line")]
    [DataRow("        c = a /** d8 */ &&\n            b;", DocumentationMode.Diagnose, DisplayName = "Delimited documentation comment in front of an operator that ends its line, mode Diagnose")]
    public void OutputOfAFormattedBinaryExpressionIsNotUnsupported(string statement, DocumentationMode documentationMode)
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
    /// Verifies that the line-break phase joins the right operand behind an operator it moves to the start of the next line when
    /// no comment stands behind the operator, which is the join a comment behind the operator used to refuse
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void RightOperandIsJoinedBehindTheMovedOperator()
    {
        const string input = """
                             class C
                             {
                                 bool M(bool a, bool b)
                                 {
                                     var x = a /* note */ &&
                                             b;

                                     return x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        var x = a /* note */
                                                && b;

                                        return x;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Wraps a statement in a method <c>M</c> of a class <c>C</c> with the parameters and members its binary expressions use
    /// </summary>
    /// <param name="statement">The statement, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statement)
    {
        return $"class C\n{{\n    int M(int i, int j, bool a, bool b, bool c, object o, string s, int? n, int[] xs)\n    {{\n{statement}\n\n        return 0;\n    }}\n\n    int F(System.Func<int> f)\n    {{\n        return f();\n    }}\n\n    int F(int v)\n    {{\n        return v;\n    }}\n}}";
    }

    /// <summary>
    /// Creates the parse options of a build that defines the given symbol or not
    /// </summary>
    /// <param name="define">Whether the build defines the symbol</param>
    /// <param name="symbol">The symbol</param>
    /// <returns>The parse options</returns>
    private static CSharpParseOptions CreateOptions(bool define, string symbol)
    {
        return define ? CSharpParseOptions.Default.WithPreprocessorSymbols(symbol) : CSharpParseOptions.Default;
    }

    /// <summary>
    /// Asserts that the outcome reports the reason of FMT-UNSUP-011 and the span of the comment <c>// c</c> in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(BinaryOperatorCommentReason, unsupported.Reason);
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