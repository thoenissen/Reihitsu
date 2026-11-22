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
/// Tests for the unsupported shape of a comment, a directive, or disabled text between the <c>.</c> of a link — a simple member
/// access or a member binding — and its member name (FMT-UNSUP-016): every kind of link, with the trivia behind the dot and in
/// front of the name, every kind of comment in every documentation mode, directives and disabled text in every build, every
/// enclosing construct, the gaps in front of the link, inside its operator, inside and behind its name, the dots that are no link,
/// the shapes that keep their own reason, every entry point, node-level inputs that hold the name without its dot, and the closure
/// of the formatter's output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedLinkDotNameCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> that holds a member access with a line comment between its <c>.</c> and its member name, beside a method
    /// <c>N</c> that holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    object M(dynamic a)
                                                                    {
                                                                        return a.B(). // c
                                                                            C();
                                                                    }

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The links whose dot-to-name gap is filled, each as the text in front of the gap, which ends with the dot, the text behind
    /// it, which starts with the member name, and a name
    /// </summary>
    private static readonly string[][] _linkGaps = [
                                                       ["t.", "Length", "dot of a member access"],
                                                       ["s?.", "Length", "dot of ?."],
                                                       ["t!.", "Length", "dot of !."],
                                                       ["u!?.", "Length", "dot of !?."],
                                                       ["c.N(1).", "Name", "dot behind an invocation"],
                                                       ["this.", "Name", "dot behind this"],
                                                       ["base.", "ToString()", "dot behind base"],
                                                       ["global::System.String.", "Empty", "dot behind an alias-qualified name"]
                                                   ];

    /// <summary>
    /// The comments and directives placed into a gap, each as the text that replaces the gap, the text that is reported, and a
    /// name: behind the dot on its line, in front of the name on its line, on a line of their own, and without whitespace
    /// </summary>
    private static readonly string[][] _gapTrivia = [
                                                        [" /* c */ ", "/* c */", "block comment on one line"],
                                                        ["/* c */", "/* c */", "block comment without whitespace"],
                                                        [" // c\n            ", "// c", "line comment behind the dot"],
                                                        [" /* c */\n            ", "/* c */", "block comment behind the dot at the end of its line"],
                                                        ["\n            // c\n            ", "// c", "comment line between the dot and the name"],
                                                        ["\n\n            // c\n            ", "// c", "comment line behind a blank line"],
                                                        ["\n            /* c */ ", "/* c */", "block comment in front of the name on its line"],
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
    /// the dot and on a line of their own
    /// </summary>
    private static readonly string[][] _documentationComments = [
                                                                    [" /** c */ ", "/** c */"],
                                                                    ["\n            /** c */\n            ", "/** c */"],
                                                                    [" /// x\n            ", "/// x"],
                                                                    ["\n            /// <summary>x</summary>\n            ", "/// <summary>x</summary>"]
                                                                ];

    /// <summary>
    /// The two comments placed into a gap inside an attribute argument, each as the text that replaces the gap, the text that is
    /// reported, and a name
    /// </summary>
    private static readonly string[][] _attributeGapTrivia = [
                                                                 [" /* c */ ", "/* c */", "block comment"],
                                                                 [" // c\n            ", "// c", "line comment"]
                                                             ];

    /// <summary>
    /// Members whose span is a variable declaration (FMT-UNSUP-004) and that carry an attribute, each as the member with the
    /// placeholder <c>ARGUMENT</c> for the attribute's argument and a name
    /// </summary>
    private static readonly string[][] _attributedVariableDeclarations = [
                                                                             ["    [Obsolete(nameof(ARGUMENT))]\n    private int _f;", "field with one declarator"],
                                                                             ["    [Obsolete(nameof(ARGUMENT))]\n    private int _f, _g;", "field with several declarators"],
                                                                             ["    [Obsolete(nameof(ARGUMENT))]\n    public event Action? E;", "event field"]
                                                                         ];

    /// <summary>
    /// Members that are no variable declaration and that carry an attribute, each as the member with the placeholder
    /// <c>ARGUMENT</c> for the attribute's argument and a name
    /// </summary>
    private static readonly string[][] _attributedOtherDeclarations = [
                                                                          ["    [Obsolete(nameof(ARGUMENT))]\n    void A()\n    {\n    }", "method"],
                                                                          ["    [Obsolete(nameof(ARGUMENT))]\n    int P { get; set; }", "property"],
                                                                          ["    [Obsolete(nameof(ARGUMENT))]\n    class D\n    {\n    }", "nested class"],
                                                                          ["    void A()\n    {\n        [Obsolete(nameof(ARGUMENT))]\n        void L()\n        {\n        }\n\n        L();\n    }", "local function"]
                                                                      ];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Every link of <see cref="_linkGaps"/> with every comment and directive of <see cref="_gapTrivia"/>
    /// </summary>
    public static IEnumerable<object[]> GapTriviaCases
    {
        get
        {
            foreach (var gap in _linkGaps)
            {
                foreach (var trivia in _gapTrivia)
                {
                    yield return [gap[0], gap[1], trivia[0], trivia[1], $"{trivia[2]} behind the {gap[2]}"];
                }
            }
        }
    }

    /// <summary>
    /// Every link of <see cref="_linkGaps"/> with every documentation comment of <see cref="_documentationComments"/> in every
    /// documentation mode
    /// </summary>
    public static IEnumerable<object[]> DocumentationCommentCases
    {
        get
        {
            foreach (var gap in _linkGaps)
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
    /// Every link of <see cref="_linkGaps"/> with a conditional group that chooses between two member names, in a build that
    /// defines the group's symbol and in one that does not
    /// </summary>
    public static IEnumerable<object[]> ConditionalGroupCases
    {
        get
        {
            foreach (var gap in _linkGaps)
            {
                yield return [gap[0], gap[1], false];
                yield return [gap[0], gap[1], true];
            }
        }
    }

    /// <summary>
    /// Every comment of <see cref="_attributeGapTrivia"/> between a dot and its member name in an attribute argument of every
    /// member of <see cref="_attributedVariableDeclarations"/>
    /// </summary>
    public static IEnumerable<object[]> AttributedVariableDeclarationCases => CreateAttributeCases(_attributedVariableDeclarations);

    /// <summary>
    /// Every comment of <see cref="_attributeGapTrivia"/> between a dot and its member name in an attribute argument of every
    /// member of <see cref="_attributedOtherDeclarations"/>
    /// </summary>
    public static IEnumerable<object[]> AttributedOtherDeclarationCases => CreateAttributeCases(_attributedOtherDeclarations);

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the specification example: the gap behind each link's <c>.</c> is refused, the input is returned unchanged, and
    /// the first comment, which starts in line 7, column 24, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    public void SpecExampleTheGapBehindEachLinksDotIsUnsupported()
    {
        const string input = """
                             #nullable enable

                             class C
                             {
                                 object M(dynamic a, string? s, string? t, string? u)
                                 {
                                     var p = a.B(). // c1
                                         C();
                                     var q = s?. /* c2 */ Length;
                                     var r = t!.
                                         // c3
                                         Length;
                                     var v = u?.Trim(). /* c4 */ Length;

                                     return (p, q, r, v);
                                 }
                             }
                             """;

        AssertUnsupportedFor(LinkDotNameCommentReason, input, "// c1");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(6, 23), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies each link of the specification example on its own: the input is refused, and its comment is reported
    /// </summary>
    /// <param name="statement">The statement that holds the link</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("        var p = a.B(). // c1\n            C();", "// c1", DisplayName = "Line comment behind the dot of an invoked link")]
    [DataRow("        var q = s?. /* c2 */ Length;", "/* c2 */", DisplayName = "Block comment behind the dot of ?. on one line")]
    [DataRow("        var r = t!.\n            // c3\n            Length;", "// c3", DisplayName = "Comment line between the dot of !. and the name")]
    [DataRow("        var v = u?.Trim(). /* c4 */ Length;", "/* c4 */", DisplayName = "Block comment behind the dot of a later link")]
    public void EachLinkOfTheSpecExampleIsUnsupported(string statement, string comment)
    {
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies the specification example: a conditional group between a <c>.</c> and its member name refuses the input in every
    /// build, and its <c>#if DEBUG</c>, which starts in line 8, column 1, is reported
    /// </summary>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void SpecExampleDirectiveBetweenTheDotAndTheMemberNameIsUnsupportedInEveryBuild(bool defineDebug)
    {
        const string input = """
                             #nullable enable

                             class C
                             {
                                 string M(string s)
                                 {
                                     return s.
                             #if DEBUG
                                         Trim();
                             #else
                                         ToUpper();
                             #endif
                                 }
                             }
                             """;
        var parseOptions = CreateOptions(defineDebug, "DEBUG");

        AssertUnsupportedFor(LinkDotNameCommentReason, input, "#if DEBUG", parseOptions: parseOptions);

        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(7, 0), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment behind a qualified name's dot in a base list, above a
    /// statement, in front of a link, inside a link's argument list, and behind a link or a statement does not refuse the input,
    /// and the input is formatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    public void SpecExampleCommentsOutsideTheDotToNameGapsAreSupported()
    {
        const string input = """
                             #nullable enable

                             class C : System. // c1
                                       IDisposable
                             {
                                 public void Dispose()
                                 {
                                 }

                                 int? M(string? s, string t)
                                 {
                                     // c2
                                     var p = s // c3
                                             ?.Length;
                                     var q = t.Substring(/* c4 */1) // c5
                                              .Length;
                                     var r = t.Trim(); // c6

                                     return p + q + r.Length; // c7
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that every comment and directive between the dot and the member name of every kind of link refuses the input —
    /// behind the dot and in front of the name, on one line and across lines — and that the first of them is reported
    /// </summary>
    /// <param name="left">The text in front of the gap, which ends with the dot</param>
    /// <param name="right">The text behind the gap, which starts with the member name</param>
    /// <param name="gap">The text of the gap</param>
    /// <param name="reported">The text that is reported</param>
    /// <param name="name">The name of the case</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DynamicData(nameof(GapTriviaCases))]
    public void EveryCommentAndDirectiveBetweenTheDotAndTheNameOfEveryLinkIsUnsupported(string left, string right, string gap, string reported, string name)
    {
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInMethod($"        Use({left}{gap}{right});"), reported);
    }

    /// <summary>
    /// Verifies that a documentation comment between the dot and the member name of every kind of link refuses the input in every
    /// documentation mode, in which it is a structured documentation comment or an ordinary comment
    /// </summary>
    /// <param name="left">The text in front of the gap, which ends with the dot</param>
    /// <param name="right">The text behind the gap, which starts with the member name</param>
    /// <param name="gap">The text of the gap</param>
    /// <param name="reported">The text that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DynamicData(nameof(DocumentationCommentCases))]
    public void DocumentationCommentBetweenTheDotAndTheNameIsUnsupportedInEveryDocumentationMode(string left, string right, string gap, string reported, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInMethod($"        Use({left}{gap}{right});"), reported, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that a conditional group that chooses between two member names behind the dot of every kind of link refuses the
    /// input in every build, and is reported at its <c>#if</c>
    /// </summary>
    /// <param name="left">The text in front of the gap, which ends with the dot</param>
    /// <param name="right">The text behind the gap, which starts with the member name</param>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DynamicData(nameof(ConditionalGroupCases))]
    public void ConditionalGroupBetweenTheDotAndTheNameIsUnsupportedInEveryBuild(string left, string right, bool defineX)
    {
        var statement = $"        Use({left}\n#if X\n            {right});\n#else\n            {right});\n#endif";

        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInMethod(statement), "#if X", parseOptions: CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies that the same choice written around the whole statement leaves the gap behind the dot free of directives: the
    /// input is formatted in both builds
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ConditionalGroupAroundTheWholeStatementIsSupported(bool defineX)
    {
        AssertSupported(WrapInMethod("#if X\n        Use(t.Trim());\n#else\n        Use(t.ToUpper());\n#endif"), CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies that each simple member access and member binding is checked with its own gap wherever it stands: a later link of
    /// a wrapped chain, an invoked member, a member read, a chain without calls, an assignment target, the target of a
    /// null-conditional assignment, a list element, an interpolation hole, a lambda, a switch-expression arm, another link's
    /// argument list, a conditional element access's index, a local initializer, a <c>nameof</c> argument, an extended property
    /// pattern, a query, an object initializer, a collection expression, and a local function in a lambda
    /// </summary>
    /// <param name="statement">The statement that holds the link</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("        Use(list.Where(z => z != null)\n            .Select(z => z)\n            . /* c */ FirstOrDefault());", DisplayName = "Later link of a wrapped chain")]
    [DataRow("        Use(c. /* c */ N(1));", DisplayName = "Invoked member")]
    [DataRow("        Use(c. /* c */ Name);", DisplayName = "Member read")]
    [DataRow("        Use(c.Next. /* c */ Next.Name);", DisplayName = "Link of a chain without calls")]
    [DataRow("        this. /* c */ Name = \"x\";", DisplayName = "Assignment target")]
    [DataRow("        c?. /* c */ Name = \"x\";", DisplayName = "Target of a null-conditional assignment")]
    [DataRow("        Use(1, t. /* c */ Length, 2);", DisplayName = "List element")]
    [DataRow("        Use($\"{t. /* c */ Length}\");", DisplayName = "Interpolation hole")]
    [DataRow("        Func<string, int> l = y => y. /* c */ Length;", DisplayName = "Lambda")]
    [DataRow("        Use(b switch\n            {\n                true => t. /* c */ Length,\n                _ => 0\n            });", DisplayName = "Switch-expression arm")]
    [DataRow("        Use(c?.N(t. /* c */ Length));", DisplayName = "Argument list of another link")]
    [DataRow("        Use(n?[t. /* c */ Length]);", DisplayName = "Index of a conditional element access")]
    [DataRow("        var v = t. /* c */ Length;", DisplayName = "Local initializer")]
    [DataRow("        Use(nameof(C. /* c */ Name));", DisplayName = "Member in a nameof argument")]
    [DataRow("        Use(nameof(System. /* c */ String));", DisplayName = "Type in a nameof argument")]
    [DataRow("        Use(o is C { Next. /* c */ Name: null });", DisplayName = "Extended property pattern")]
    [DataRow("        Use(from z in list select z. /* c */ Length);", DisplayName = "Query")]
    [DataRow("        Use(new C { Name = t. /* c */ Trim() });", DisplayName = "Object initializer")]
    [DataRow("        int[] e = [t. /* c */ Length];", DisplayName = "Collection expression")]
    [DataRow("        Func<int> l = () =>\n        {\n            int L() => t. /* c */ Length;\n\n            return L();\n        };", DisplayName = "Local function in a lambda")]
    public void EveryLinkIsCheckedWithItsOwnGap(string statement)
    {
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies that a link in a member's expression body, a property initializer, and a field with one or several declarators
    /// refuses the input with this shape's reason
    /// </summary>
    /// <param name="member">The member that holds the link</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("    int? P => Name?. /* c */ Length;", DisplayName = "Expression body of a property")]
    [DataRow("    int? Q { get; } = Source?. /* c */ Length;", DisplayName = "Property initializer")]
    [DataRow("    private int? _f = Source?. /* c */ Length;", DisplayName = "Field with one declarator")]
    [DataRow("    private int? _f = Source?. /* c */ Length, _g;", DisplayName = "Value of a field with several declarators")]
    [DataRow("    private int _h = Source!. // c\n                     Length;", DisplayName = "Line comment in the value of a field")]
    public void LinkInAMemberIsUnsupported(string member)
    {
        var comment = member.Contains("// c") ? "// c" : "/* c */";

        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInClass(member), comment);
    }

    /// <summary>
    /// Verifies that a member access inside disabled text is not checked: where <c>X</c> is not defined, the statement is disabled
    /// text and the input is formatted, and where it is defined, the statement is code and the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void MemberAccessInDisabledTextIsNotChecked(bool defineX)
    {
        var input = WrapInMethod("#if X\n        Use(t. /* c */ Length);\n#endif");
        var parseOptions = CreateOptions(defineX, "X");

        if (defineX)
        {
            AssertUnsupportedFor(LinkDotNameCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies the first condition's other side: a comment, a directive, or a documentation comment in front of a link's dot,
    /// inside the member name's type-argument list, inside the link's argument list, behind the link, behind and above the
    /// statement, and in front of the expression's first token does not refuse the input
    /// </summary>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("        Use(t /* c */ .Length);", DisplayName = "Block comment in front of the dot")]
    [DataRow("        Use(t // c\n            .Length);", DisplayName = "Line comment in front of the dot")]
    [DataRow("        Use(s /* c */ ?.Length);", DisplayName = "Block comment in front of ?.")]
    [DataRow("        Use(c.Next /* c */ .Name);", DisplayName = "Block comment in front of a later dot")]
    [DataRow("        Use(list.Where(z => z != null)\n            // c\n            .FirstOrDefault());", DisplayName = "Comment line in front of a later link")]
    [DataRow("        Use(list.Where(z => z != null)\n#pragma warning disable CS0001\n            .FirstOrDefault());", DisplayName = "Directive in front of a later link")]
    [DataRow("        Use(c.N(1) /// d\n            .N(2));", DisplayName = "Single-line documentation comment in front of the dot")]
    [DataRow("        Use(c.N(1) /** d */\n            .N(2));", DisplayName = "Delimited documentation comment in front of the dot")]
    [DataRow("        Use(c.G</* c */ int>());", DisplayName = "Inside the name's type-argument list")]
    [DataRow("        Use(c.N(/* c */ 1));", DisplayName = "Inside the link's argument list")]
    [DataRow("        Use(c.N(1) /* c */);", DisplayName = "Behind the link")]
    [DataRow("        Use(t.Length); // c", DisplayName = "Behind the statement")]
    [DataRow("        // c\n        Use(t.Length);", DisplayName = "Above the statement")]
    [DataRow("        Use(/* c */ t.Length);", DisplayName = "In front of the expression's first token")]
    public void CommentOutsideTheGapBehindTheDotIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies the second condition's other side: a comment behind a <c>.</c> that is no operator token of a member access or a
    /// member binding does not refuse the input — the dot of a qualified name in a type, in an object creation, in a cast, and in
    /// a <c>default</c> expression, the <c>::</c> of an alias-qualified name, the <c>-&gt;</c> of a pointer member access, and
    /// the <c>..</c> of a range
    /// </summary>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("        Use(typeof(System. /* c */ String));", DisplayName = "Qualified name in typeof")]
    [DataRow("        Use(new System. /* c */ Text.StringBuilder());", DisplayName = "Qualified name in an object creation")]
    [DataRow("        Use((System. /* c */ String?)null);", DisplayName = "Qualified name in a cast")]
    [DataRow("        Use(default(System. /* c */ String));", DisplayName = "Qualified name in a default expression")]
    [DataRow("        Use(global:: /* c */ System.String.Empty);", DisplayName = "Behind an alias qualifier")]
    [DataRow("        Use(p-> /* c */ X);", DisplayName = "Behind a pointer member access")]
    [DataRow("        Use(1.. /* c */ 2);", DisplayName = "Behind a range")]
    public void CommentBehindADotThatIsNoLinkIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies the second condition's other side for the dots of qualified names outside a method: a using directive and a base
    /// list, which FMT-INDENT-013 lays out
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("using System. /* c */ Text;\n\nclass C\n{\n    StringBuilder? M()\n    {\n        return null;\n    }\n}", DisplayName = "Qualified name of a using directive")]
    [DataRow("class B : System. // c\n          IDisposable\n{\n    public void Dispose()\n    {\n    }\n}", DisplayName = "Qualified name in a base list")]
    public void CommentBehindTheDotOfAQualifiedNameIsSupported(string input)
    {
        AssertSupported(input);
    }

    /// <summary>
    /// Verifies the first condition's other side for the shapes of other requirements: a comment between the tokens of a link
    /// operator keeps the reason of FMT-UNSUP-015, and a comment between the member name and its argument list keeps the reason of
    /// FMT-UNSUP-014
    /// </summary>
    /// <param name="reason">The reason that is reported</param>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow(LinkOperatorCommentReason, "        Use(s? /* c */ .Length);", DisplayName = "Between the question mark and the dot of ?.")]
    [DataRow(LinkOperatorCommentReason, "        Use(t! /* c */ .Length);", DisplayName = "Between the exclamation mark and the dot of !.")]
    [DataRow(LinkOperatorCommentReason, "        Use(u!? /* c */ .Length);", DisplayName = "Between the question mark and the dot of !?.")]
    [DataRow(AttachedPartCommentReason, "        Use(c.N /* c */ (1));", DisplayName = "Between the member name and its argument list")]
    public void CommentInTheGapOfAnotherShapeKeepsItsReason(string reason, string statement)
    {
        AssertUnsupportedFor(reason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies that of a comment between a dot and its member name and a comment of another shape, the first in the text is
    /// reported, also when both stand in one link
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-016")]
    public void FirstShapeInTheTextIsReported()
    {
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInMethod("        Use(t. /* first */ Length + /* second */ 1);"), "/* first */");
        AssertUnsupportedFor(BinaryOperatorCommentReason, WrapInMethod("        Use(1 + /* first */ t. /* second */ Length);"), "/* first */");
        AssertUnsupportedFor(LinkOperatorCommentReason, WrapInMethod("        Use(u!? /* first */ . /* second */ Length);"), "/* first */");
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInMethod("        Use(s?. /* first */ Length);\n        Use(s? /* second */ .Length);"), "/* first */");
        AssertUnsupportedFor(AttachedPartCommentReason, WrapInMethod("        Use(c.N /* first */ (1). /* second */ Name);"), "/* first */");
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInMethod("        Use(c. /* first */ N /* second */ (1));"), "/* first */");
    }

    /// <summary>
    /// Verifies the specification's example of the overlap with FMT-UNSUP-004 outside an initializer value: a comment between a
    /// dot and its member name in an attribute argument of a field gives the reason of FMT-UNSUP-004, and the same comment in a
    /// local's initializer value gives the reason of FMT-UNSUP-016
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    [Requirement("FMT-UNSUP-016")]
    public void SpecExampleOverlapWithTheVariableDeclarationShape()
    {
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInClass("    [Obsolete(nameof(C. /* c */ M))] private int _e;\n\n    void M()\n    {\n    }"), "/* c */");
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInMethod("        int x = t. /* c */ Length;"), "/* c */");
    }

    /// <summary>
    /// Verifies the overlap with FMT-UNSUP-004 outside an initializer value: a comment between a dot and its member name in an
    /// attribute argument of a field or an event field stands in the span of the variable declaration, so the input is reported
    /// with the reason of FMT-UNSUP-004 at the comment, with one or several declarators
    /// </summary>
    /// <param name="member">The member that holds the link</param>
    /// <param name="reported">The text that is reported</param>
    /// <param name="name">The name of the case</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    [Requirement("FMT-UNSUP-016")]
    [DynamicData(nameof(AttributedVariableDeclarationCases))]
    public void CommentBetweenTheDotAndTheNameInAnAttributeArgumentOfAVariableDeclarationGivesTheReasonOfTheVariableDeclaration(string member, string reported, string name)
    {
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInClass(member), reported);
    }

    /// <summary>
    /// Verifies the other side of the overlap with FMT-UNSUP-004: the same comment in an attribute argument of a member that is no
    /// variable declaration — a method, a property, a nested class, or a local function — gives the reason of FMT-UNSUP-016 at the
    /// comment
    /// </summary>
    /// <param name="member">The member that holds the link</param>
    /// <param name="reported">The text that is reported</param>
    /// <param name="name">The name of the case</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    [Requirement("FMT-UNSUP-016")]
    [DynamicData(nameof(AttributedOtherDeclarationCases))]
    public void CommentBetweenTheDotAndTheNameInAnAttributeArgumentOfAnotherMemberGivesTheReasonOfTheLink(string member, string reported, string name)
    {
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInClass(member), reported);
    }

    /// <summary>
    /// Verifies that a directive between a dot and its member name in an initializer value of a field declaration with several
    /// declarators keeps the reason of FMT-UNSUP-010 in every build, and that the same directive in a field with one declarator
    /// gives the reason of FMT-UNSUP-016
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-016")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DirectiveBetweenTheDotAndTheNameInAFieldValueKeepsTheReasonOfTheFieldDeclaration(bool defineX)
    {
        const string value = " = Source!.\n#if X\n        Length\n#else\n        Length\n#endif\n        ";
        var parseOptions = CreateOptions(defineX, "X");

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass($"    private int _a{value}, _b;"), "#if X", parseOptions: parseOptions);
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInClass($"    private int _a{value};"), "#if X", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that a directive between a dot and its member name in an attribute argument of a field declaration, which
    /// FMT-UNSUP-004 does not refuse, gives the reason of FMT-UNSUP-010 with several declarators and the reason of FMT-UNSUP-016
    /// with one, in every build
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-016")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void DirectiveBetweenTheDotAndTheNameInAnAttributeArgumentOfAFieldKeepsTheReasonOfTheFieldDeclaration(bool defineX)
    {
        const string attribute = "    [Obsolete(nameof(Source.\n#if X\n        Length\n#else\n        Length\n#endif\n        ))]\n";
        var parseOptions = CreateOptions(defineX, "X");

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass($"{attribute}    private int _f, _g;"), "#if X", parseOptions: parseOptions);
        AssertUnsupportedFor(LinkDotNameCommentReason, WrapInClass($"{attribute}    private int _f;"), "#if X", parseOptions: parseOptions);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment between the dot and the member name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
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
    /// Verifies that the node-level entry points refuse a target or a context that holds the method with the comment between a dot
    /// and its member name, and format the method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
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
    /// Verifies that a node-level input holds the gap of a simple member access only when it holds the member access:
    /// <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for the member access and the root, and nothing for
    /// the member access's expression nor for its name, which holds the comment in its leading trivia when the comment stands on a
    /// line of its own; and the node-level entry points do not refuse the name or the expression
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="comment">The comment that is reported</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [Requirement("FMT-UNSUP-003")]
    [DataRow("class C\n{\n    int M(string t)\n    {\n        return t. /* c */ Length;\n    }\n}", "/* c */", DisplayName = "Comment behind the dot")]
    [DataRow("class C\n{\n    int M(string t)\n    {\n        return t.\n            // c\n            Length;\n    }\n}", "// c", DisplayName = "Comment in front of the name")]
    public async Task NameOrExpressionOfAMemberAccessAloneDoesNotHoldTheShape(string input, string comment)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var access = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Single();

                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(access), text, comment);
                AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(root), text, comment);
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(access.Name));
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(access.Expression));
                Assert.IsNull(ReihitsuFormatter.FormatNode(access.Name, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.IsNull(ReihitsuFormatter.FormatNode(access.Expression, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.IsNull((await ReihitsuFormatter.FormatNodeInDocumentAsync(document, access.Name, TestContext.CancellationToken)).Unsupported);
                Assert.IsNull((await ReihitsuFormatter.FormatNodeInDocumentAsync(document, access.Expression, TestContext.CancellationToken)).Unsupported);
            }
        }
    }

    /// <summary>
    /// Verifies that a node-level input holds the gap of a member binding only when it holds the member binding:
    /// <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for the member binding and its conditional access,
    /// and nothing for the binding's name nor for the conditional access's expression; and the node-level entry points do not
    /// refuse the name
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [Requirement("FMT-UNSUP-003")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? s)\n    {\n        return s?. /* c */ Length;\n    }\n}", "/* c */", DisplayName = "Comment behind the dot")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? s)\n    {\n        return s?.\n            // c\n            Length;\n    }\n}", "// c", DisplayName = "Comment in front of the name")]
    public void NameOfAMemberBindingAloneDoesNotHoldTheShape(string input, string comment)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var access = root.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().Single();
            var binding = root.DescendantNodes().OfType<MemberBindingExpressionSyntax>().Single();

            AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(binding), text, comment);
            AssertReportsComment(ReihitsuFormatter.FindUnsupportedSyntax(access), text, comment);
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(binding.Name));
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(access.Expression));
            Assert.IsNull(ReihitsuFormatter.FormatNode(binding.Name, cancellationToken: TestContext.CancellationToken).Unsupported);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds, which stands in a
    /// tree of its own: an expression built with a comment between a dot and its member name is refused, and the same expression
    /// without the comment is not
    /// </summary>
    /// <param name="expression">The expression with the comment</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("t. /* c */ Length", "/* c */")]
    [DataRow("a.B(). // c\n    C()", "// c")]
    [DataRow("s?. /* c */ Length", "/* c */")]
    [DataRow("t!.\n    // c\n    Length", "// c")]
    [DataRow("u!?. /* c */ Length", "/* c */")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode(string expression, string comment)
    {
        var node = SyntaxFactory.ParseExpression(expression);

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(node);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(LinkDotNameCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, node.ToFullString(), comment, 0, "LF");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(SyntaxFactory.ParseExpression(expression.Replace(comment, string.Empty))));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same tree
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("class C\n{\n    object M(dynamic a)\n    {\n        return a.B(). // c\n            C();\n    }\n}")]
    [DataRow("class C\n{\n    string M(string s)\n    {\n        return s.\n#if DEBUG\n            Trim();\n#else\n            ToUpper();\n#endif\n    }\n}")]
    [DataRow("#nullable enable\n\nclass C\n{\n    int? M(string? s)\n    {\n        return s?.\n            // c\n            Length;\n    }\n}")]
    [DataRow("class C\n{\n    int M(string s)\n    {\n        return s // c\n            .Length;\n    }\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: member names split from their dot by whitespace alone, which the
    /// line-break phase joins, next to comments in front of the dot, behind the name, inside and behind the link, and above the
    /// statement, in every documentation mode and in every enclosing construct the join reaches, are formatted, and their output,
    /// formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="statement">The statement that holds the link</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("        Use(t.\n            Length); // c", DocumentationMode.Parse, DisplayName = "Comment behind a joined member access")]
    [DataRow("        Use(s?.\n            Length /* c */);", DocumentationMode.Parse, DisplayName = "Comment behind a joined member binding")]
    [DataRow("        Use(t // c\n            .\n            Length);", DocumentationMode.Parse, DisplayName = "Comment in front of a dot whose name is joined")]
    [DataRow("        Use(c.N(1)\n            .\n            Name); // c", DocumentationMode.Parse, DisplayName = "Comment behind a joined later link")]
    [DataRow("        Use(list.Where(z => z != null)\n            // c\n            .\n            FirstOrDefault());", DocumentationMode.Parse, DisplayName = "Comment line in front of a later dot whose name is joined")]
    [DataRow("        Use(c.\n            N( // c\n                1));", DocumentationMode.Parse, DisplayName = "Comment inside the argument list of a joined link")]
    [DataRow("        this.\n            Name = \"x\"; // c", DocumentationMode.Parse, DisplayName = "Comment behind a joined assignment target")]
    [DataRow("        Use(nameof(C.\n            Name)); // c", DocumentationMode.Parse, DisplayName = "Comment behind a joined nameof argument")]
    [DataRow("        Func<string, int> l = y => y.\n            Length; // c", DocumentationMode.Parse, DisplayName = "Comment behind a joined lambda body")]
    [DataRow("        /// <summary>x</summary>\n        Use(t.\n            Length);", DocumentationMode.Parse, DisplayName = "Documentation comment above a statement with a joined member access")]
    [DataRow("        Use(c.N(1) /// d\n            .\n            N(2));", DocumentationMode.Parse, DisplayName = "Single-line documentation comment in front of a dot whose name is joined, mode Parse")]
    [DataRow("        Use(c.N(1) /// d\n            .\n            N(2));", DocumentationMode.None, DisplayName = "Single-line documentation comment in front of a dot whose name is joined, mode None")]
    [DataRow("        Use(c.N(1) /** d */\n            ?.\n            N(2));", DocumentationMode.Diagnose, DisplayName = "Delimited documentation comment in front of ?. whose name is joined, mode Diagnose")]
    public void OutputOfFormattedLinksIsNotUnsupported(string statement, DocumentationMode documentationMode)
    {
        AssertFormattedOutputIsNotRefused(WrapInMethod(statement), CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that the output of the specification example of the other side, formatted again, is neither refused nor changed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-016")]
    public void OutputOfTheOtherSideExampleIsNotUnsupported()
    {
        const string input = "#nullable enable\n\nclass C : System. // c1\n          IDisposable\n{\n    public void Dispose()\n    {\n    }\n\n    int? M(string? s, string t)\n    {\n        // c2\n        var p = s // c3\n                ?.Length;\n        var q = t.Substring(/* c4 */1) // c5\n                 .Length;\n        var r = t.Trim(); // c6\n\n        return p + q + r.Length; // c7\n    }\n}";

        AssertFormattedOutputIsNotRefused(input, CSharpParseOptions.Default);
    }

    /// <summary>
    /// Creates the cases of a comment between a dot and its member name in an attribute argument of every member given
    /// </summary>
    /// <param name="members">The members, each with the placeholder <c>ARGUMENT</c> for the attribute's argument and a name</param>
    /// <returns>The member, the text that is reported, and the name of each case</returns>
    private static IEnumerable<object[]> CreateAttributeCases(string[][] members)
    {
        foreach (var member in members)
        {
            foreach (var trivia in _attributeGapTrivia)
            {
                var argument = $"Source.{trivia[0]}Length";

                yield return [member[0].Replace("ARGUMENT", argument), trivia[1], $"{trivia[2]} in an attribute argument of a {member[1]}"];
            }
        }
    }

    /// <summary>
    /// Wraps statements in a method <c>M</c> of a class <c>C</c>, with the parameters its links use
    /// </summary>
    /// <param name="statements">The statements, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statements)
    {
        return WrapInClass($"    unsafe void M(string? s, string t, string? u, C? c, int[]? n, List<string> list, object o, bool b, S* p, dynamic a)\n    {{\n{statements}\n    }}");
    }

    /// <summary>
    /// Wraps members in a class <c>C</c> with the usings and the helper members its links use
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string members)
    {
        return $"#nullable enable\n\nusing System;\nusing System.Collections.Generic;\nusing System.Linq;\n\nstruct S\n{{\n    public int X;\n}}\n\nclass C\n{{\n{members}\n\n    private static string? Source = null;\n\n    public string? Name {{ get; set; }}\n\n    public C? Next {{ get; set; }}\n\n    C? N(int value)\n    {{\n        return value > 0 ? this : null;\n    }}\n\n    T? G<T>()\n    {{\n        return default;\n    }}\n\n    void Use(params object?[] values)\n    {{\n    }}\n}}";
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
    /// Asserts that the outcome reports the reason of FMT-UNSUP-016 and the span of the given comment in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    /// <param name="comment">The comment that is reported</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text, string comment)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(LinkDotNameCommentReason, unsupported.Reason);
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
    /// Formats the input, then formats its output again, under LF and CRLF line endings, and verifies that neither pass refuses its
    /// input and that the second pass changes nothing
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="parseOptions">The parse options</param>
    private void AssertFormattedOutputIsNotRefused(string input, CSharpParseOptions parseOptions)
    {
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

    #endregion // Methods
}