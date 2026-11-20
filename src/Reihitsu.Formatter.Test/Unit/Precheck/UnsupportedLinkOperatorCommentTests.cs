using System.Collections.Generic;
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
/// Tests for the unsupported shape of a comment, a directive, or disabled text between two tokens of a link operator <c>?.</c>,
/// <c>!.</c>, or <c>!?.</c> (FMT-UNSUP-015): every gap of every operator with the trivia behind the earlier token and in front
/// of the later one, every kind of comment in every documentation mode, directives and disabled text in every build, every
/// enclosing construct, the gaps in front of an operator and behind it, the tokens that are no link operator, the shapes that keep
/// their own reason, every entry point, node-level inputs that hold one token of an operator, and the closure of the formatter's
/// output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedLinkOperatorCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> that holds a <c>?.</c> with a line comment between its <c>?</c> and its <c>.</c>, beside a method <c>N</c>
    /// that holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    object M(dynamic a)
                                                                    {
                                                                        return a? // c
                                                                            .B();
                                                                    }

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The four gaps inside a link operator, each as the text in front of the gap, the text behind it, and a name:
    /// <c>?</c>–<c>.</c> of <c>?.</c>, <c>!</c>–<c>.</c> of <c>!.</c>, <c>!</c>–<c>?</c> of <c>!?.</c>, and <c>?</c>–<c>.</c> of
    /// <c>!?.</c>
    /// </summary>
    private static readonly string[][] _operatorGaps = [
                                                           ["s?", ".Length", "question mark and dot of ?."],
                                                           ["t!", ".Length", "exclamation mark and dot of !."],
                                                           ["u!", "?.Length", "exclamation mark and question mark of !?."],
                                                           ["u!?", ".Length", "question mark and dot of !?."]
                                                       ];

    /// <summary>
    /// The comments and directives placed into a gap, each as the text that replaces the gap, the text that is reported, and a
    /// name: behind the earlier token on its line, in front of the later token on its line, on a line of their own, and without
    /// whitespace
    /// </summary>
    private static readonly string[][] _gapTrivia = [
                                                        [" /* c */ ", "/* c */", "block comment on one line"],
                                                        ["/* c */", "/* c */", "block comment without whitespace"],
                                                        [" // c\n            ", "// c", "line comment behind the earlier token"],
                                                        [" /* c */\n            ", "/* c */", "block comment behind the earlier token at the end of its line"],
                                                        ["\n            // c\n            ", "// c", "comment line between the tokens"],
                                                        ["\n\n            // c\n            ", "// c", "comment line behind a blank line"],
                                                        ["\n            /* c */ ", "/* c */", "block comment in front of the later token on its line"],
                                                        [" /* c\n               d */\n            ", "/* c\n               d */", "multi-line block comment"],
                                                        ["\n#pragma warning disable CS0001\n            ", "#pragma warning disable CS0001", "pragma directive"],
                                                        ["\n            #region R\n            #endregion\n            ", "#region R", "region directives"],
                                                        ["\n#if false\n            junk\n#endif\n            ", "#if false", "fully inactive group with disabled text"],
                                                        [" /* c */\n#pragma warning disable CS0001\n            ", "/* c */", "comment in front of a directive"],
                                                        ["\n#nullable disable\n            ", "#nullable disable", "nullable directive"],
                                                        ["\n#line 200\n            ", "#line 200", "line directive"],
                                                        ["\n#if X\n#elif Y\n#endif\n            ", "#if X", "empty conditional group with an elif directive"]
                                                    ];

    /// <summary>
    /// The documentation comments placed into a gap, each as the text that replaces the gap and the text that is reported: behind
    /// the earlier token and on a line of their own
    /// </summary>
    private static readonly string[][] _documentationComments = [
                                                                    [" /** c */ ", "/** c */"],
                                                                    ["\n            /** c */\n            ", "/** c */"],
                                                                    [" /// x\n            ", "/// x"],
                                                                    ["\n            /// <summary>x</summary>\n            ", "/// <summary>x</summary>"]
                                                                ];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Every gap of every link operator with every comment and directive of <see cref="_gapTrivia"/>
    /// </summary>
    public static IEnumerable<object[]> GapTriviaCases
    {
        get
        {
            foreach (var gap in _operatorGaps)
            {
                foreach (var trivia in _gapTrivia)
                {
                    yield return [gap[0], gap[1], trivia[0], trivia[1], $"{trivia[2]} between {gap[2]}"];
                }
            }
        }
    }

    /// <summary>
    /// Every gap of every link operator with every documentation comment of <see cref="_documentationComments"/> in every
    /// documentation mode
    /// </summary>
    public static IEnumerable<object[]> DocumentationCommentCases
    {
        get
        {
            foreach (var gap in _operatorGaps)
            {
                foreach (var comment in _documentationComments)
                {
                    foreach (var mode in new[] { DocumentationMode.None, DocumentationMode.Parse, DocumentationMode.Diagnose })
                    {
                        yield return [gap[0], gap[1], comment[0], comment[1], mode];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Every gap of every link operator with a conditional group that chooses between two later tokens, in a build that defines
    /// the group's symbol and in one that does not
    /// </summary>
    public static IEnumerable<object[]> ConditionalGroupCases
    {
        get
        {
            foreach (var gap in _operatorGaps)
            {
                yield return [gap[0], gap[1], false];
                yield return [gap[0], gap[1], true];
            }
        }
    }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the specification example: each link operator is refused, the input is returned unchanged, and the first comment,
    /// which starts in line 7, column 20, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    public void SpecExampleEachLinkOperatorIsUnsupported()
    {
        const string input = """
                             #nullable enable

                             class C
                             {
                                 int? M(string? s, string? t, string? u, string? w)
                                 {
                                     var p = s? // c1
                                         .Length;
                                     var q = t! /* c2 */ .Length;
                                     var r = u!
                                         // c3
                                         ?.Length;
                                     var v = w!? /* c4 */ .Length;

                                     return p + q + r + v;
                                 }
                             }
                             """;

        AssertUnsupportedFor(LinkOperatorCommentReason, input, "// c1");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(6, 19), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies each link operator of the specification example on its own: the input is refused, and its comment is reported
    /// </summary>
    /// <param name="statement">The statement that holds the link operator</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("        var p = s? // c1\n            .Length;", "// c1", DisplayName = "Line comment between the question mark and the dot")]
    [DataRow("        var q = t! /* c2 */ .Length;", "/* c2 */", DisplayName = "Block comment between the exclamation mark and the dot on one line")]
    [DataRow("        var r = u!\n            // c3\n            ?.Length;", "// c3", DisplayName = "Comment line between the exclamation mark and the question mark")]
    [DataRow("        var v = w!? /* c4 */ .Length;", "/* c4 */", DisplayName = "Block comment between the question mark and the dot of !?.")]
    public void EachLinkOperatorOfTheSpecExampleIsUnsupported(string statement, string comment)
    {
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies the specification example: a conditional group inside a <c>!.</c> refuses the input in every build, and its
    /// <c>#if DEBUG</c>, which starts in line 8, column 1, is reported
    /// </summary>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void SpecExampleDirectiveInsideANullForgivingLinkOperatorIsUnsupportedInEveryBuild(bool defineDebug)
    {
        const string input = """
                             #nullable enable

                             class C
                             {
                                 string M()
                                 {
                                     return Get()!
                             #if DEBUG
                                         .Trim();
                             #else
                                         .ToUpper();
                             #endif
                                 }

                                 string? Get()
                                 {
                                     return null;
                                 }
                             }
                             """;
        var parseOptions = CreateOptions(defineDebug, "DEBUG");

        AssertUnsupportedFor(LinkOperatorCommentReason, input, "#if DEBUG", parseOptions: parseOptions);

        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(7, 0), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies the specification example of the other side: comments above a statement, in front of a link operator, inside a
    /// link's argument list, behind a link, inside a conditional element access, and behind a statement do not refuse the input,
    /// and the input is formatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    public void SpecExampleCommentsOutsideTheLinkOperatorsAreSupported()
    {
        const string input = """
                             #nullable enable

                             class C
                             {
                                 int? M(string? s, string? t, C? c, int[]? n)
                                 {
                                     // c1
                                     var p = s // c2
                                             ?.Length;
                                     var q = t /* c3 */ !.Length;
                                     var r = c?.N(/* c4 */ 1) // c5
                                              ?.N(2);
                                     var x = n?[/* c6 */ 0];

                                     return p + q + (r == null ? 0 : 1) + x; // c7
                                 }

                                 C? N(int value)
                                 {
                                     return value > 0 ? this : null;
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that every comment and directive in every gap of every link operator refuses the input — behind the earlier token
    /// and in front of the later one, on one line and across lines — and that the first of them is reported
    /// </summary>
    /// <param name="left">The text in front of the gap</param>
    /// <param name="right">The text behind the gap</param>
    /// <param name="gap">The text of the gap</param>
    /// <param name="reported">The text that is reported</param>
    /// <param name="name">The name of the case</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DynamicData(nameof(GapTriviaCases))]
    public void EveryCommentAndDirectiveInEveryGapOfEveryLinkOperatorIsUnsupported(string left, string right, string gap, string reported, string name)
    {
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInMethod($"        Use({left}{gap}{right});"), reported);
    }

    /// <summary>
    /// Verifies that a documentation comment in every gap of every link operator refuses the input in every documentation mode, in
    /// which it is a structured documentation comment or an ordinary comment
    /// </summary>
    /// <param name="left">The text in front of the gap</param>
    /// <param name="right">The text behind the gap</param>
    /// <param name="gap">The text of the gap</param>
    /// <param name="reported">The text that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DynamicData(nameof(DocumentationCommentCases))]
    public void DocumentationCommentInEveryGapIsUnsupportedInEveryDocumentationMode(string left, string right, string gap, string reported, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInMethod($"        Use({left}{gap}{right});"), reported, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that a conditional group that chooses between two later tokens of a link operator refuses the input in every build,
    /// for every gap of every link operator, and is reported at its <c>#if</c>
    /// </summary>
    /// <param name="left">The text in front of the gap</param>
    /// <param name="right">The text behind the gap</param>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DynamicData(nameof(ConditionalGroupCases))]
    public void ConditionalGroupInEveryGapIsUnsupportedInEveryBuild(string left, string right, bool defineX)
    {
        var statement = $"        Use({left}\n#if X\n            {right});\n#else\n            {right});\n#endif";

        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInMethod(statement), "#if X", parseOptions: CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies that the same choice written around the whole statement leaves the link operator free of directives: the input is
    /// formatted in both builds
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ConditionalGroupAroundTheWholeStatementIsSupported(bool defineX)
    {
        AssertSupported(WrapInMethod("#if X\n        Use(Get()!.Trim());\n#else\n        Use(Get()!.ToUpper());\n#endif"), CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies that each link operator is checked with its own tokens wherever it stands: a later link of a wrapped chain, an
    /// invoked link, a link of a chain without calls, the target of a null-conditional assignment, a list element, an interpolation
    /// hole, a lambda, a switch-expression arm, another link's argument list, a conditional element access's index, a local
    /// initializer, and a local function in a lambda
    /// </summary>
    /// <param name="statement">The statement that holds the link operator</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("        Use(list.Where(z => z != null)\n            .Select(z => z)\n            .FirstOrDefault()? /* c */ .Trim());", DisplayName = "Later link of a wrapped chain")]
    [DataRow("        Use(list.Where(z => z != null)! /* c */ .Select(z => z));", DisplayName = "Later link of a chain behind a null-forgiving operator")]
    [DataRow("        Use(c? /* c */ .N(1));", DisplayName = "Invoked link")]
    [DataRow("        Use(c.Next? /* c */ .Next.Name);", DisplayName = "Link of a chain without calls")]
    [DataRow("        c? /* c */ .Name = \"x\";", DisplayName = "Target of a null-conditional assignment")]
    [DataRow("        Use(1, s? /* c */ .Length, 2);", DisplayName = "List element")]
    [DataRow("        Use($\"{s? /* c */ .Length}\");", DisplayName = "Interpolation hole")]
    [DataRow("        Func<string?, int?> l = y => y? /* c */ .Length;", DisplayName = "Lambda")]
    [DataRow("        Use(b switch\n            {\n                true => s? /* c */ .Length,\n                _ => 0\n            });", DisplayName = "Switch-expression arm")]
    [DataRow("        Use(c?.N(s? /* c */ .Length ?? 0));", DisplayName = "Argument list of another link")]
    [DataRow("        Use(n?[s? /* c */ .Length ?? 0]);", DisplayName = "Index of a conditional element access")]
    [DataRow("        var v = s? /* c */ .Length;", DisplayName = "Local initializer")]
    [DataRow("        Func<int?> l = () =>\n        {\n            int? L() => s? /* c */ .Length;\n\n            return L();\n        };", DisplayName = "Local function in a lambda")]
    public void EveryLinkOperatorIsCheckedWithItsOwnTokens(string statement)
    {
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies that a link operator in a member's expression body, a property initializer, and a field with one declarator refuses
    /// the input with this shape's reason
    /// </summary>
    /// <param name="member">The member that holds the link operator</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("    int? P => Name? /* c */ .Length;", DisplayName = "Expression body of a property")]
    [DataRow("    int? Q { get; } = Source? /* c */ .Length;", DisplayName = "Property initializer")]
    [DataRow("    private int? _f = Source? /* c */ .Length;", DisplayName = "Field with one declarator")]
    public void LinkOperatorInAMemberIsUnsupported(string member)
    {
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInClass(member), "/* c */");
    }

    /// <summary>
    /// Verifies that a link operator inside disabled text is not checked: where <c>X</c> is not defined, the statement is disabled
    /// text and the input is formatted, and where it is defined, the statement is code and the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void LinkOperatorInDisabledTextIsNotChecked(bool defineX)
    {
        var input = WrapInMethod("#if X\n        Use(s? /* c */ .Length);\n#endif");
        var parseOptions = CreateOptions(defineX, "X");

        if (defineX)
        {
            AssertUnsupportedFor(LinkOperatorCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies the first condition's other side: a comment, a directive, or a documentation comment in front of a link operator's
    /// first token, between the operator's <c>.</c> and the member name, inside the link's argument list, behind the link, behind
    /// and above the statement, and in front of the token in front of the operator does not refuse the input
    /// </summary>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("        Use(s /* c */ ?.Length);", DisplayName = "Block comment in front of ?.")]
    [DataRow("        Use(s // c\n            ?.Length);", DisplayName = "Line comment in front of ?.")]
    [DataRow("        Use(t /* c */ !.Length);", DisplayName = "Block comment in front of !.")]
    [DataRow("        Use(t // c\n            !.Length);", DisplayName = "Line comment in front of !.")]
    [DataRow("        Use(u /* c */ !?.Length);", DisplayName = "Block comment in front of !?.")]
    [DataRow("        Use(u\n            // c\n            !?.Length);", DisplayName = "Comment line in front of !?.")]
    [DataRow("        Use(w! /* c */ !.Length);", DisplayName = "Block comment in front of the !. behind another null-forgiving operator")]
    [DataRow("        Use(list.Where(z => z != null)\n#pragma warning disable CS0001\n            .FirstOrDefault()?.Trim());", DisplayName = "Directive in front of a later link")]
    [DataRow("        Use(c.N(1) /// d\n            ?.N(2));", DisplayName = "Single-line documentation comment in front of ?.")]
    [DataRow("        Use(c.N(1) /** d */\n            ?.N(2));", DisplayName = "Delimited documentation comment in front of ?.")]
    [DataRow("        Use(s?. /* c */ Length);", DisplayName = "Between the dot of ?. and the member name")]
    [DataRow("        Use(t!. /* c */ Length);", DisplayName = "Between the dot of !. and the member name")]
    [DataRow("        Use(u!?. /* c */ Length);", DisplayName = "Between the dot of !?. and the member name")]
    [DataRow("        Use(c?.N(/* c */ 1));", DisplayName = "Inside the link's argument list")]
    [DataRow("        Use(c?.N(1) /* c */);", DisplayName = "Behind the link")]
    [DataRow("        Use(s?.Length); // c", DisplayName = "Behind the statement")]
    [DataRow("        // c\n        Use(s?.Length);", DisplayName = "Above the statement")]
    [DataRow("        Use(/* c */ s?.Length);", DisplayName = "In front of the token in front of the operator")]
    public void CommentOutsideTheLinkOperatorIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies the second condition's other side: a comment next to a token pair that is no link operator does not refuse the
    /// input under this requirement — a plain <c>.</c>, the <c>!</c> of a logical negation, the <c>..</c> of a range, the
    /// <c>-&gt;</c> of a pointer member access, and the <c>::</c> of an alias-qualified name
    /// </summary>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("        Use(c /* c */ .Name);", DisplayName = "In front of a plain dot")]
    [DataRow("        Use(c. /* c */ Name);", DisplayName = "Behind a plain dot")]
    [DataRow("        Use(! /* c */ b);", DisplayName = "Behind a logical negation")]
    [DataRow("        Use(1 /* c */ ..2);", DisplayName = "In front of a range")]
    [DataRow("        Use(1.. /* c */ 2);", DisplayName = "Behind a range")]
    [DataRow("        Use(p /* c */ ->X);", DisplayName = "In front of a pointer member access")]
    [DataRow("        Use(p-> /* c */ X);", DisplayName = "Behind a pointer member access")]
    [DataRow("        Use(global:: /* c */ System.String.Empty);", DisplayName = "Behind an alias qualifier")]
    public void CommentNextToATokenThatIsNoLinkOperatorIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies the second condition's other side for the shapes of other requirements: the gaps of a conditional element access
    /// keep the reason of FMT-UNSUP-014, and a comment behind a conditional expression's <c>?</c> in front of a <c>.</c> keeps the
    /// reason of FMT-UNSUP-008
    /// </summary>
    /// <param name="reason">The reason that is reported</param>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow(AttachedPartCommentReason, "        Use(n? /* c */ [0]);", DisplayName = "Between the question mark and the bracket of ?[")]
    [DataRow(AttachedPartCommentReason, "        Use(n! /* c */ ?[0]);", DisplayName = "Between the exclamation mark and the question mark of !?[")]
    [DataRow(AttachedPartCommentReason, "        Use(n!? /* c */ [0]);", DisplayName = "Between the question mark and the bracket of !?[")]
    [DataRow(ConditionalOperatorCommentReason, "        Use(b ? /* c */ .5 : 1);", DisplayName = "Behind the question mark of a conditional expression")]
    public void CommentInTheGapOfAnotherShapeKeepsItsReason(string reason, string statement)
    {
        AssertUnsupportedFor(reason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies that a comment inside a link operator in an initializer value of a field declaration with several declarators gives
    /// the reason of FMT-UNSUP-015, which FMT-UNSUP-004 does not refuse inside a value
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    [Requirement("FMT-UNSUP-015")]
    public void CommentInsideALinkOperatorInAFieldWithSeveralDeclaratorsGivesTheReasonOfTheLinkOperator()
    {
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInClass("    private int? _a = Source? /* c */ .Length, _b;"), "/* c */");
    }

    /// <summary>
    /// Verifies that a directive inside a link operator in an initializer value of a field declaration with several declarators
    /// keeps the reason of FMT-UNSUP-010 in every build, and that the same directive in a field with one declarator gives the
    /// reason of FMT-UNSUP-015
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-015")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DirectiveInsideALinkOperatorInAFieldWithSeveralDeclaratorsKeepsTheReasonOfTheFieldDeclaration(bool defineX)
    {
        const string value = " = Source?\n#if X\n        .Length\n#else\n        .Length\n#endif\n        ";
        var parseOptions = CreateOptions(defineX, "X");

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass($"    private int? _a{value}, _b;"), "#if X", parseOptions: parseOptions);
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInClass($"    private int? _a{value};"), "#if X", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that of a comment inside a link operator and a comment of another shape, the first in the text is reported, and
    /// that a comment in front of a <c>!</c> that starts a <c>!?.</c> is neither shape
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-015")]
    public void FirstShapeInTheTextIsReported()
    {
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInMethod("        Use(s? /* first */ .Length + /* second */ 1);"), "/* first */");
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod("        Use(1 + /* first */ s? /* second */ .Length);"), "/* first */");
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod("        Use(n? /* first */ [0]);\n        Use(s? /* second */ .Length);"), "/* first */");
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInMethod("        Use(s? /* first */ .Length);\n        Use(n? /* second */ [0]);"), "/* first */");
        AssertUnsupportedFor(ConditionalOperatorCommentReason, WrapInMethod("        Use(b ? /* first */ s? /* second */ .Length : 0);"), "/* first */");
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInMethod("        Use(u /* first */ !? /* second */ .Length);"), "/* second */");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment inside the link operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
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
    /// Verifies that the node-level entry points refuse a target or a context that holds the method with the comment inside its
    /// link operator, and format the method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
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
                var statement = root.DescendantNodes().OfType<ReturnStatementSyntax>().Single();

                var nodeResult = ReihitsuFormatter.FormatNode(FindMethod(root, "M"), cancellationToken: TestContext.CancellationToken);

                Assert.AreEqual(FindMethod(root, "M").ToFullString(), nodeResult.Output.ToFullString());
                AssertReportsComment(nodeResult.Unsupported, text, "// c");

                var statementResult = ReihitsuFormatter.FormatNode(statement, cancellationToken: TestContext.CancellationToken);

                Assert.AreEqual(statement.ToFullString(), statementResult.Output.ToFullString());
                AssertReportsComment(statementResult.Unsupported, text, "// c");

                var targetResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, statement, TestContext.CancellationToken);

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
    /// Verifies that a node-level input holds a <c>?.</c> only when it holds its conditional access:
    /// <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for the conditional access and the root, and nothing
    /// for the conditional access's branch, whose <c>.</c> holds the comment in its leading trivia; and the node-level entry points
    /// do not refuse the branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [Requirement("FMT-UNSUP-003")]
    public async Task BranchOfAConditionalAccessAloneDoesNotHoldTheShape()
    {
        const string input = "#nullable enable\n\nclass C\n{\n    int? M(string? s)\n    {\n        return s?\n            // c\n            .Length;\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var access = root.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().Single();

                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(access), text, "// c");
                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root), text, "// c");
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(access.WhenNotNull));
                Assert.IsNull(ReihitsuFormatter.FormatNode(access.WhenNotNull, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.IsNull((await ReihitsuFormatter.FormatNodeInDocumentAsync(document, access.WhenNotNull, TestContext.CancellationToken)).Unsupported);
            }
        }
    }

    /// <summary>
    /// Verifies that a node-level input holds a <c>!.</c> only when it holds its member access:
    /// <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for the member access, and nothing for the
    /// null-forgiving operator alone, whose <c>!</c> holds the comment in its trailing trivia; and the node-level entry points do
    /// not refuse the null-forgiving operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [Requirement("FMT-UNSUP-003")]
    public async Task NullForgivingOperatorAloneDoesNotHoldTheShapeOfItsMemberAccess()
    {
        const string input = "#nullable enable\n\nclass C\n{\n    int M(string? t)\n    {\n        return t! /* c */ .Length;\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var access = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Single();

                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(access), text, "/* c */");
                Assert.IsInstanceOfType<PostfixUnaryExpressionSyntax>(access.Expression);
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(access.Expression));
                Assert.IsNull(ReihitsuFormatter.FormatNode(access.Expression, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.IsNull((await ReihitsuFormatter.FormatNodeInDocumentAsync(document, access.Expression, TestContext.CancellationToken)).Unsupported);
            }
        }
    }

    /// <summary>
    /// Verifies that a node-level input holds a <c>!?.</c> only when it holds its conditional access:
    /// <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for the conditional access, and nothing for the
    /// null-forgiving operator alone, whose <c>!</c> holds the comment in its trailing trivia, nor for the branch, whose <c>.</c>
    /// holds a comment in its leading trivia
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [Requirement("FMT-UNSUP-003")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? u)\n    {\n        return u! /* c */ ?.Length;\n    }\n}", "/* c */", DisplayName = "Comment between the exclamation mark and the question mark")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? u)\n    {\n        return u!?\n            // c\n            .Length;\n    }\n}", "// c", DisplayName = "Comment between the question mark and the dot")]
    public void PartsOfANullForgivingConditionalAccessAloneDoNotHoldTheShape(string input, string comment)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var access = root.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().Single();

            AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(access), text, comment);
            Assert.IsInstanceOfType<PostfixUnaryExpressionSyntax>(access.Expression);
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(access.Expression));
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(access.WhenNotNull));
            Assert.IsNull(ReihitsuFormatter.FormatNode(access.Expression, cancellationToken: TestContext.CancellationToken).Unsupported);
            Assert.IsNull(ReihitsuFormatter.FormatNode(access.WhenNotNull, cancellationToken: TestContext.CancellationToken).Unsupported);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds, which stands in a
    /// tree of its own: an expression built with a comment inside a link operator is refused, and the same expression without the
    /// comment is not
    /// </summary>
    /// <param name="expression">The expression with the comment</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("s? /* c */ .Length", "/* c */")]
    [DataRow("t! // c\n    .Length", "// c")]
    [DataRow("u! /* c */ ?.Length", "/* c */")]
    [DataRow("u!? /* c */ .Length", "/* c */")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode(string expression, string comment)
    {
        var node = SyntaxFactory.ParseExpression(expression);

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(node);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(LinkOperatorCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, node.ToFullString(), comment, 0, "LF");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(SyntaxFactory.ParseExpression(expression.Replace(comment, string.Empty))));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same tree
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("class C\n{\n    object M(dynamic a)\n    {\n        return a? // c\n            .B();\n    }\n}")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int M(string? t)\n    {\n        return t!\n#if DEBUG\n            .Length;\n#else\n            .Length;\n#endif\n    }\n}")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? u)\n    {\n        return u!\n            // c\n            ?.Length;\n    }\n}")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? s)\n    {\n        return s // c\n            ?.Length;\n    }\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: link operators split by whitespace alone, which the line-break phase
    /// joins, next to comments in front of the operator, behind its <c>.</c>, inside and behind the link, and above the statement,
    /// in every documentation mode, are formatted, and their output, formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="statement">The statement that holds the link operator</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("        Use(s?\n            .Length); // c", DocumentationMode.Parse, DisplayName = "Comment behind a joined ?.")]
    [DataRow("        Use(t!\n            .Length /* c */);", DocumentationMode.Parse, DisplayName = "Comment behind a joined !.")]
    [DataRow("        Use(u!\n            ?\n            .Length); // c", DocumentationMode.Parse, DisplayName = "Comment behind a joined !?.")]
    [DataRow("        Use(s // c\n            ?\n            .Length);", DocumentationMode.Parse, DisplayName = "Comment in front of a joined ?.")]
    [DataRow("        Use(t /* c */\n            !\n            .Length);", DocumentationMode.Parse, DisplayName = "Comment in front of a joined !.")]
    [DataRow("        Use(u // c\n            !\n            ?\n            .Length);", DocumentationMode.Parse, DisplayName = "Comment in front of a joined !?.")]
    [DataRow("        Use(s?\n            . // c\n            Length);", DocumentationMode.Parse, DisplayName = "Comment behind the dot of a joined ?.")]
    [DataRow("        Use(c?\n            .N( // c\n                1));", DocumentationMode.Parse, DisplayName = "Comment inside the argument list of a joined link")]
    [DataRow("        Use(list.Where(z => z != null)\n            .FirstOrDefault()?\n            .Trim() // c\n            .Length);", DocumentationMode.Parse, DisplayName = "Comment behind a joined later link")]
    [DataRow("        /// <summary>x</summary>\n        Use(s?\n            .Length);", DocumentationMode.Parse, DisplayName = "Documentation comment above a statement with a joined ?.")]
    [DataRow("        Use(c.N(1) /// d\n            ?\n            .N(2));", DocumentationMode.Parse, DisplayName = "Single-line documentation comment in front of a joined ?., mode Parse")]
    [DataRow("        Use(c.N(1) /// d\n            ?\n            .N(2));", DocumentationMode.None, DisplayName = "Single-line documentation comment in front of a joined ?., mode None")]
    [DataRow("        Use(c.N(1) /** d */\n            !\n            .N(2));", DocumentationMode.Diagnose, DisplayName = "Delimited documentation comment in front of a joined !., mode Diagnose")]
    public void OutputOfFormattedLinkOperatorsIsNotUnsupported(string statement, DocumentationMode documentationMode)
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
    /// Wraps statements in a method <c>M</c> of a class <c>C</c>, with the parameters its link operators use
    /// </summary>
    /// <param name="statements">The statements, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statements)
    {
        return WrapInClass($"    unsafe void M(string? s, string? t, string? u, string? w, C? c, int[]? n, List<string?> list, bool b, S* p)\n    {{\n{statements}\n    }}");
    }

    /// <summary>
    /// Wraps members in a class <c>C</c> with the usings and the helper members its link operators use
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string members)
    {
        return $"#nullable enable\n\nusing System;\nusing System.Collections.Generic;\nusing System.Linq;\n\nstruct S\n{{\n    public int X;\n}}\n\nclass C\n{{\n{members}\n\n    private static string? Source = null;\n\n    public string? Name {{ get; set; }}\n\n    public C? Next {{ get; set; }}\n\n    C? N(int value)\n    {{\n        return value > 0 ? this : null;\n    }}\n\n    string? Get()\n    {{\n        return null;\n    }}\n\n    void Use(params object?[] values)\n    {{\n    }}\n}}";
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
    /// Asserts that the outcome reports the reason of FMT-UNSUP-015 and the span of the given comment in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    /// <param name="comment">The comment that is reported</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text, string comment)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(LinkOperatorCommentReason, unsupported.Reason);
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