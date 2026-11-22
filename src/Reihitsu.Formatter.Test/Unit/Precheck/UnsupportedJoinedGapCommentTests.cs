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
/// Tests for the unsupported shape of a comment, a directive, or disabled text in a gap the line-break phase joins (FMT-UNSUP-017):
/// each gap of the table with its reason, on one line and across lines, directives in every build, documentation comments in
/// every documentation mode, the gaps next to the same tokens that are no such gap, the shapes that keep their own reason, every
/// entry point, node-level inputs that hold only the token behind a gap, and the closure of the formatter's output, each under LF
/// and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedJoinedGapCommentTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method <c>M</c> that holds a line comment between <c>return</c> and its value, beside a method <c>N</c> that holds none
    /// </summary>
    private const string RefusedMethodBesideFormattableMethod = """
                                                                class C
                                                                {
                                                                    int M(int x)
                                                                    {
                                                                        return // c
                                                                            x;
                                                                    }

                                                                    void N(){ }
                                                                }
                                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: each gap is refused, the input is returned unchanged, and the first comment, which
    /// starts in line 5, column 9, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    public void SpecExampleEachGapIsUnsupported()
    {
        const string input = """
                             using System;

                             class B;

                             class D // c1
                                 : B
                             {
                                 int M(int x, object o, int[] a)
                                 {
                                     var p = o is /* c2 */ not null;
                                     var r = x switch
                                             {
                                                 1 // c3
                                                     => 2,
                                                 _ => 0,
                                             };
                                     Func<int> u = () => /* c4 */ a.Length;
                                     var w = new { A /* c5 */ = 1 };

                                     for (var i = 0; // c6
                                         i < 3; i++)
                                     {
                                     }

                                     return // c7
                                         x;
                                 }
                             }
                             """;

        AssertUnsupportedFor(BaseListCommentReason, input, "// c1");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(4, 8), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies each gap of the first specification example on its own: the input is refused with the gap's reason, and its
    /// comment is reported
    /// </summary>
    /// <param name="reason">The reason of the gap</param>
    /// <param name="statement">The statement that holds the gap</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(PatternCombinatorCommentReason, "        var p = o is not /* c2 */ null;", "/* c2 */", DisplayName = "Behind not")]
    [DataRow(IsPatternCommentReason, "        var p = o is /* c2 */ not null;", "/* c2 */", DisplayName = "Behind is")]
    [DataRow(SwitchArmArrowCommentReason, "        var r = x switch\n                {\n                    1 // c3\n                        => 2,\n                    _ => 0,\n                };", "// c3", DisplayName = "In front of the arrow of an arm")]
    [DataRow(LambdaArrowCommentReason, "        Func<int> u = () => /* c4 */ a.Length;", "/* c4 */", DisplayName = "Behind the arrow of a lambda")]
    [DataRow(AnonymousMemberEqualsCommentReason, "        var w = new { A /* c5 */ = 1 };", "/* c5 */", DisplayName = "In front of the equals of an anonymous-object member")]
    [DataRow(ForHeaderCommentReason, "        for (var i = 0; // c6\n            i < 3; i++)\n        {\n        }", "// c6", DisplayName = "Behind the first semicolon of a for header")]
    [DataRow(KeywordOperandCommentReason, "        return // c7\n            x;", "// c7", DisplayName = "Behind return")]
    public void EachGapOfTheSpecExampleIsUnsupported(string reason, string statement, string comment)
    {
        AssertUnsupportedFor(reason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies the second specification example: the gaps behind a combinator, a cast, the <c>in</c> of a <c>foreach</c>, the
    /// name of a <c>let</c> clause, <c>yield return</c>, and a conditional group of modifiers refuse the input; where <c>NET</c>
    /// is defined, the <c>#else</c> in line 7, column 1 is reported, and where it is not, the <c>#endif</c> in line 9, column 1
    /// </summary>
    /// <param name="defineNet">Whether <c>NET</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(false, DisplayName = "NET not defined")]
    [DataRow(true, DisplayName = "NET defined")]
    public void SpecExampleConditionalModifiersAndFurtherGapsAreUnsupported(bool defineNet)
    {
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Linq;

                             #if NET
                             public
                             #else
                             internal
                             #endif
                             class N
                             {
                                 IEnumerable<int> M(int x, object o, int[] a)
                                 {
                                     var k = x is 1 or // c1
                                         2;
                                     var c = (long) /* c2 */ x;

                                     foreach (var y in /* c3 */ a)
                                     {
                                     }

                                     var q = from v in a
                                             let u /* c4 */ = v
                                             select u;

                                     yield return /* c5 */ x;
                                 }
                             }
                             """;
        var parseOptions = defineNet ? CSharpParseOptions.Default.WithPreprocessorSymbols("NET") : CSharpParseOptions.Default;
        var reported = defineNet ? "#else" : "#endif";

        AssertUnsupportedFor(ModifierCommentReason, input, reported, parseOptions: parseOptions);

        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(defineNet ? 6 : 8, 0), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies each further gap of the second specification example on its own: the input is refused with the gap's reason
    /// </summary>
    /// <param name="reason">The reason of the gap</param>
    /// <param name="statement">The statement that holds the gap</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(PatternCombinatorCommentReason, "        var k = x is 1 or // c1\n            2;", "// c1", DisplayName = "Behind or")]
    [DataRow(CastCommentReason, "        var c = (long) /* c2 */ x;", "/* c2 */", DisplayName = "Behind a cast")]
    [DataRow(ForEachExpressionCommentReason, "        foreach (var y in /* c3 */ a)\n        {\n        }", "/* c3 */", DisplayName = "Behind the in of a foreach statement")]
    [DataRow(LetClauseEqualsCommentReason, "        var q = from v in a\n                let u /* c4 */ = v\n                select u;", "/* c4 */", DisplayName = "In front of the equals of a let clause")]
    [DataRow(KeywordOperandCommentReason, "        yield return /* c5 */ x;", "/* c5 */", DisplayName = "Behind yield return")]
    public void EachFurtherGapOfTheSpecExampleIsUnsupported(string reason, string statement, string comment)
    {
        AssertUnsupportedFor(reason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies the specification example: a conditional group between <c>return</c> and its value refuses the input in every
    /// build, and its <c>#if DEBUG</c>, which starts in line 6, column 1, is reported
    /// </summary>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void SpecExampleDirectiveBetweenReturnAndItsValueIsUnsupportedInEveryBuild(bool defineDebug)
    {
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return
                             #if DEBUG
                                         1;
                             #else
                                         2;
                             #endif
                                 }
                             }
                             """;
        var parseOptions = defineDebug ? CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG") : CSharpParseOptions.Default;

        AssertUnsupportedFor(KeywordOperandCommentReason, input, "#if DEBUG", parseOptions: parseOptions);

        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(5, 0), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies the specification example of the other side: comments next to the gaps but outside them do not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    public void SpecExampleCommentsOutsideTheGapsAreSupported()
    {
        const string input = """
                             using System;

                             class B;

                             // c1
                             class D : /* c2 */ B
                             {
                                 int M(int x, object o, int[] a)
                                 {
                                     // c3
                                     var p = o is string { Length: /* c4 */ 2 };
                                     var r = x switch
                                             {
                                                 1 => /* c5 */ 2,
                                                 _ => 0,
                                             };
                                     Func<int> u = () /* c6 */ => a.Length;
                                     var w = new
                                             {
                                                 A = 1 /* c7 */
                                             };

                                     return x; // c8
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies the second specification example of the other side: a combinator, a modifier, a cast, and a <c>let</c> clause
    /// with a comment outside their gaps are formatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    public void SpecExampleCommentsOutsideTheGapsOfCombinatorModifierCastAndLetAreSupported()
    {
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             /* c1 */ internal class N
                             {
                                 IEnumerable<int> M(int x, int[] a)
                                 {
                                     var k = x is 1 /* c2 */ or 2;
                                     var c = (long)x; // c3
                                     var q = from v in a
                                             let u = v // c4
                                             select u;

                                     yield return x; // c5
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies the refused example of every row of the table: the input is refused with the row's reason, and the comment is
    /// reported
    /// </summary>
    /// <param name="reason">The reason of the row</param>
    /// <param name="statement">The statement that holds the gap</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(KeywordOperandCommentReason, "        return // c\n            x;", "// c", DisplayName = "Return, line comment")]
    [DataRow(KeywordOperandCommentReason, "        return /* c */ x;", "/* c */", DisplayName = "Return, one line")]
    [DataRow(KeywordOperandCommentReason, "        var v = o ?? throw /* c */ new Exception();", "/* c */", DisplayName = "Throw expression")]
    [DataRow(KeywordOperandCommentReason, "        throw /* c */ new Exception();", "/* c */", DisplayName = "Throw statement")]
    [DataRow(KeywordOperandCommentReason, "        var q = await /* c */ t;", "/* c */", DisplayName = "Await expression")]
    [DataRow(KeywordOperandCommentReason, "        await // c\n            foreach (var z in s)\n        {\n        }", "// c", DisplayName = "Await foreach")]
    [DataRow(KeywordOperandCommentReason, "        await /* c */ using (d)\n        {\n        }", "/* c */", DisplayName = "Await using statement")]
    [DataRow(KeywordOperandCommentReason, "        var n = new // c\n            List<int>();", "// c", DisplayName = "Object creation")]
    [DataRow(KeywordOperandCommentReason, "        int[] m = new /* c */ [] { 1 };", "/* c */", DisplayName = "Implicitly typed array creation")]
    [DataRow(KeywordOperandCommentReason, "        var m = new /* c */ int[1];", "/* c */", DisplayName = "Array creation")]
    [DataRow(IsPatternCommentReason, "        var p = o is /* c */ not null;", "/* c */", DisplayName = "Is pattern")]
    [DataRow(IsPatternCommentReason, "        var p = o is // c\n            (int or long);", "// c", DisplayName = "Is pattern with a parenthesized pattern")]
    [DataRow(PatternCombinatorCommentReason, "        var k = x is 1 or // c\n            2;", "// c", DisplayName = "Or")]
    [DataRow(PatternCombinatorCommentReason, "        var k = x is > 1 and /* c */ < 3;", "/* c */", DisplayName = "And")]
    [DataRow(PatternCombinatorCommentReason, "        var p = o is not /* c */ null;", "/* c */", DisplayName = "Not")]
    [DataRow(SwitchArmWhenCommentReason, "        var r = o switch\n                {\n                    int i /* c */ when i > 0 => 1,\n                    _ => 0,\n                };", "/* c */", DisplayName = "When of a switch arm")]
    [DataRow(SwitchArmArrowCommentReason, "        var r = x switch\n                {\n                    1 /* c */ => 2,\n                    _ => 0,\n                };", "/* c */", DisplayName = "Arrow of a switch arm behind the pattern")]
    [DataRow(SwitchArmArrowCommentReason, "        var r = o switch\n                {\n                    int i when i > 0 // c\n                        => 1,\n                    _ => 0,\n                };", "// c", DisplayName = "Arrow of a switch arm behind the when clause")]
    [DataRow(CatchFilterCommentReason, "        try\n        {\n        }\n        catch (Exception e) /* c */ when (e != null)\n        {\n        }", "/* c */", DisplayName = "Catch filter")]
    [DataRow(ForHeaderCommentReason, "        for (var i = 0; // c\n            i < 3; i++)\n        {\n        }", "// c", DisplayName = "First semicolon of a for header")]
    [DataRow(ForHeaderCommentReason, "        for (var i = 0; i < 3; /* c */ i++)\n        {\n        }", "/* c */", DisplayName = "Second semicolon of a for header")]
    [DataRow(ForEachExpressionCommentReason, "        foreach (var y in /* c */ a)\n        {\n        }", "/* c */", DisplayName = "Foreach expression")]
    [DataRow(CastCommentReason, "        var c = (int) /* c */ x;", "/* c */", DisplayName = "Cast, one line")]
    [DataRow(CastCommentReason, "        var c = (int) // c\n            x;", "// c", DisplayName = "Cast, line comment")]
    [DataRow(LambdaArrowCommentReason, "        Func<int> u = () => /* c */ 1;", "/* c */", DisplayName = "Lambda, one line")]
    [DataRow(LambdaArrowCommentReason, "        Func<int, int> f = y => // c\n            y + 1;", "// c", DisplayName = "Simple lambda, line comment")]
    [DataRow(WithCommentReason, "        var w = r /* c */ with { };", "/* c */", DisplayName = "With, one line")]
    [DataRow(WithCommentReason, "        var w = new R() // c\n            with { };", "// c", DisplayName = "With, line comment")]
    [DataRow(AnonymousMemberEqualsCommentReason, "        var w = new { A /* c */ = 1 };", "/* c */", DisplayName = "In front of the equals of an anonymous-object member")]
    [DataRow(AnonymousMemberEqualsCommentReason, "        var w = new { A = // c\n            1 };", "// c", DisplayName = "Behind the equals of an anonymous-object member")]
    [DataRow(LetClauseEqualsCommentReason, "        var q = from v in a\n                let u /* c */ = v\n                select u;", "/* c */", DisplayName = "In front of the equals of a let clause")]
    [DataRow(LetClauseEqualsCommentReason, "        var q = from v in a\n                let u = // c\n                    v\n                select u;", "// c", DisplayName = "Behind the equals of a let clause")]
    [DataRow(ModifierCommentReason, "        static // c\n            int Local()\n        {\n            return 1;\n        }", "// c", DisplayName = "Modifier of a local function")]
    [DataRow(ModifierCommentReason, "        Func<int, int> f = static /* c */ y => y;", "/* c */", DisplayName = "Modifier of a lambda")]
    [DataRow(ModifierCommentReason, "        Func<int> g = static /* c */ delegate { return 1; };", "/* c */", DisplayName = "Modifier of an anonymous method")]
    public void EveryRowOfTheTableIsUnsupported(string reason, string statement, string comment)
    {
        AssertUnsupportedFor(reason, WrapInMethod(statement), comment);
    }

    /// <summary>
    /// Verifies the modifier and base-list rows for declarations: a comment behind a modifier of a method, a type, a constructor,
    /// a delegate, an accessor, a parameter, and a function-pointer parameter outside a variable declaration, and a comment in front of the <c>:</c> of a base list of a class and an enum,
    /// each refuses the input with the row's reason
    /// </summary>
    /// <param name="reason">The reason of the row</param>
    /// <param name="member">The member that holds the gap</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(ModifierCommentReason, "    private /* c */ int A()\n    {\n        return 1;\n    }", "/* c */", DisplayName = "Modifier of a method")]
    [DataRow(ModifierCommentReason, "    public // c\n    static void B()\n    {\n    }", "// c", DisplayName = "Between two modifiers")]
    [DataRow(ModifierCommentReason, "    public /* c */ class N\n    {\n    }", "/* c */", DisplayName = "Modifier of a nested type")]
    [DataRow(ModifierCommentReason, "    public /* c */ C()\n    {\n    }", "/* c */", DisplayName = "Modifier of a constructor")]
    [DataRow(ModifierCommentReason, "    public /* c */ delegate void Dl();", "/* c */", DisplayName = "Modifier of a delegate")]
    [DataRow(ModifierCommentReason, "    public int P { get; private /* c */ set; }", "/* c */", DisplayName = "Modifier of an accessor")]
    [DataRow(ModifierCommentReason, "    static void E(this /* c */ int x)\n    {\n    }", "/* c */", DisplayName = "Modifier of a parameter")]
    [DataRow(ModifierCommentReason, "    public static /* c */ implicit operator int(C c)\n    {\n        return 1;\n    }", "/* c */", DisplayName = "Modifier of a conversion operator")]
    [DataRow(ModifierCommentReason, "    static unsafe delegate*<ref // c\n    int, void> P => null;", "// c", DisplayName = "Line comment behind a function-pointer parameter's modifier in a property's type")]
    [DataRow(ModifierCommentReason, "    static unsafe delegate*<ref /* c */ int, void> Q => null;", "/* c */", DisplayName = "Block comment behind a function-pointer parameter's modifier in a property's type")]
    [DataRow(ModifierCommentReason, "    static unsafe void F(delegate*<ref /* c */ int, void> p)\n    {\n    }", "/* c */", DisplayName = "Block comment behind a function-pointer parameter's modifier in a method's parameter")]
    [DataRow(ModifierCommentReason, "    static unsafe void G(delegate*<in // c\n    int, void> p)\n    {\n    }", "// c", DisplayName = "Line comment behind a function-pointer parameter's modifier in a method's parameter")]
    [DataRow(BaseListCommentReason, "    class D2 // c\n        : B\n    {\n    }", "// c", DisplayName = "Base list of a class")]
    [DataRow(BaseListCommentReason, "    enum E /* c */ : byte\n    {\n    }", "/* c */", DisplayName = "Base list of an enum")]
    public void EveryRowOfTheTableIsUnsupportedInDeclarations(string reason, string member, string comment)
    {
        AssertUnsupportedFor(reason, WrapInClass(member), comment);
    }

    /// <summary>
    /// Verifies that a directive, a conditional group with disabled text, and a documentation comment in a gap refuse the input
    /// like a comment
    /// </summary>
    /// <param name="statement">The statement that holds the gap</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow("        return\n#pragma warning disable CS0168\n            x;", "#pragma warning disable CS0168", DisplayName = "Pragma directive")]
    [DataRow("        return\n#if false\n            junk\n#endif\n            x;", "#if false", DisplayName = "Fully inactive group with disabled text")]
    [DataRow("        return\n            #region R\n            #endregion\n            x;", "#region R", DisplayName = "Region directives")]
    [DataRow("        return /** c */ x;", "/** c */", DisplayName = "Delimited documentation comment")]
    [DataRow("        return /// c\n            x;", "/// c", DisplayName = "Single-line documentation comment")]
    public void DirectivesAndDocumentationCommentsInAGapAreUnsupported(string statement, string reported)
    {
        foreach (var mode in new[] { DocumentationMode.None, DocumentationMode.Parse, DocumentationMode.Diagnose })
        {
            AssertUnsupportedFor(KeywordOperandCommentReason, WrapInMethod(statement), reported, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(mode));
        }
    }

    /// <summary>
    /// Verifies the conditional modifiers: a group that chooses between two accessibilities in front of a member's other modifier
    /// or of a type refuses the input in every build, and a group around a single modifier refuses it only where the symbol is
    /// defined
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ConditionalModifiersAreUnsupportedInTheBuildsThatPutADirectiveBehindAModifier(bool defineX)
    {
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;
        var reported = defineX ? "#else" : "#endif";

        AssertUnsupportedFor(ModifierCommentReason, WrapInClass("#if X\n    public\n#else\n    internal\n#endif\n    static int M2()\n    {\n        return 1;\n    }"), reported, parseOptions: parseOptions);
        AssertUnsupportedFor(ModifierCommentReason, "#if X\npublic\n#else\ninternal\n#endif\nclass Foo\n{\n}", reported, parseOptions: parseOptions);

        var single = WrapInClass("#if X\n    public\n#endif\n    int M2()\n    {\n        return 1;\n    }");

        if (defineX)
        {
            AssertUnsupportedFor(ModifierCommentReason, single, "#endif", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(single, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that the same choice written around the whole member leaves the modifier gaps free of directives: the input is
    /// formatted in both builds
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ConditionalGroupAroundTheWholeMemberIsSupported(bool defineX)
    {
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;

        AssertSupported(WrapInClass("#if X\n    public static int M2()\n    {\n        return 1;\n    }\n#else\n    internal static int M2()\n    {\n        return 1;\n    }\n#endif"), parseOptions);
    }

    /// <summary>
    /// Verifies the first condition's other side: a comment, a directive, or disabled text next to the same tokens but outside
    /// the gaps does not refuse the input
    /// </summary>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow("        // c\n        return x;", DisplayName = "In front of return")]
    [DataRow("        return x; // c", DisplayName = "Behind the statement")]
    [DataRow("        await t /* c */;", DisplayName = "Behind the operand")]
    [DataRow("        return M2(/* c */ 1);", DisplayName = "Inside the expression behind its first token")]
    [DataRow("        Use(o is { } /* c */);", DisplayName = "Behind the pattern")]
    [DataRow("        var p = o is string { Length: /* c */ 2 };", DisplayName = "Inside the pattern")]
    [DataRow("        var k = x is 1 /* c */ or 2;", DisplayName = "In front of a combinator")]
    [DataRow("        var r = x switch\n                {\n                    1 => /* c */ 2,\n                    _ => 0,\n                };", DisplayName = "Behind the arrow of a switch arm")]
    [DataRow("        try\n        {\n        }\n        catch (Exception e) when /* c */ (e != null)\n        {\n        }", DisplayName = "Behind the when of a catch filter")]
    [DataRow("        for (/* c */ ;;)\n        {\n        }", DisplayName = "Behind the opening parenthesis of a for header")]
    [DataRow("        foreach (var y /* c */ in a)\n        {\n        }", DisplayName = "In front of the in of a foreach statement")]
    [DataRow("        var c = (int /* c */) x;", DisplayName = "Inside the parentheses of a cast")]
    [DataRow("        Func<int> u = () /* c */ => 1;", DisplayName = "In front of the arrow of a lambda")]
    [DataRow("        Action b = () => // c\n        {\n        };", DisplayName = "Behind the arrow of a lambda with a block body")]
    [DataRow("        var w = r with /* c */ { };", DisplayName = "Behind with")]
    [DataRow("        var n = new /* c */ { A = 1 };", DisplayName = "Behind the new of an anonymous object")]
    [DataRow("        var w = new { A = 1 /* c */ };", DisplayName = "Behind the value of an anonymous-object member")]
    public void CommentOutsideTheGapsIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies the other side for statements without an expression: a comment between <c>return</c> or <c>throw</c> and the
    /// <c>;</c> does not refuse the input
    /// </summary>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow("        return /* c */;", DisplayName = "Return without an expression")]
    [DataRow("        try\n        {\n        }\n        catch\n        {\n            throw /* c */;\n        }", DisplayName = "Rethrow")]
    public void CommentBetweenTheKeywordAndTheSemicolonOfAStatementWithoutAnExpressionIsSupported(string statement)
    {
        AssertSupported($"class C\n{{\n    void M()\n    {{\n{statement}\n    }}\n}}");
    }

    /// <summary>
    /// Verifies the other side for declarations: a comment in front of the first modifier or behind the <c>:</c> of a base list
    /// does not refuse the input
    /// </summary>
    /// <param name="member">The member that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow("    /* c */ public int A()\n    {\n        return 1;\n    }", DisplayName = "In front of the first modifier")]
    [DataRow("    class D2 : /* c */ B\n    {\n    }", DisplayName = "Behind the colon of a base list")]
    public void CommentOutsideTheGapsOfADeclarationIsSupported(string member)
    {
        AssertSupported(WrapInClass(member));
    }

    /// <summary>
    /// Verifies that the gaps other requirements decide keep their reasons: the <c>new</c> of an implicit object creation gives
    /// the reason of FMT-UNSUP-014, the <c>is</c> of a type test the reason of FMT-UNSUP-011, and the arrow of a member's
    /// expression body the reason of FMT-UNSUP-012
    /// </summary>
    /// <param name="reason">The reason that is reported</param>
    /// <param name="statement">The statement that holds the comment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(AttachedPartCommentReason, "        List<int> n = new /* c */ ();", DisplayName = "New of an implicit object creation")]
    [DataRow(BinaryOperatorCommentReason, "        var p = o is /* c */ string;", DisplayName = "Is of a type test")]
    [DataRow(ExpressionBodyArrowCommentReason, "        int Local() => /* c */ 1;", DisplayName = "Arrow of a local function's expression body")]
    public void GapsOfOtherRequirementsKeepTheirReason(string reason, string statement)
    {
        AssertUnsupportedFor(reason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies the overlap with FMT-UNSUP-004 and FMT-UNSUP-010: a comment behind a field's modifier, between a local's
    /// <c>const</c> and its type, and between the <c>await</c> and the <c>using</c> of a using declaration give the reason of
    /// FMT-UNSUP-004, a directive behind a modifier of a field with several
    /// declarators gives the reason of FMT-UNSUP-010, a gap inside an initializer value and behind the first <c>;</c> of a
    /// <c>for</c> header give this requirement's reason, and a comment behind a function-pointer parameter's modifier inside the
    /// type of a field or a local gives the reason of FMT-UNSUP-004
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-017")]
    public void OverlapWithTheVariableDeclarationShapesGivesTheirReason()
    {
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInClass("    private /* c */ int _f;"), "/* c */");
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInClass("    private /* c */ static int _g;"), "/* c */");
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInMethod("        const /* c */ int k = 1;"), "/* c */");
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass("    private\n#pragma warning disable CS0168\n    int _f, _g;"), "#pragma warning disable CS0168");
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInMethod("        await /* c */ using var e = d;"), "/* c */");
        AssertUnsupportedFor(KeywordOperandCommentReason, WrapInMethod("        int v = await /* c */ t;"), "/* c */");
        AssertUnsupportedFor(ForHeaderCommentReason, WrapInMethod("        for (var i = 0; /* c */ i < 3; i++)\n        {\n        }"), "/* c */");
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInClass("    static unsafe delegate*<ref /* c */ int, void> _p;"), "/* c */");
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInMethod("        delegate*<ref // c\n            int, void> p = null;"), "// c");
    }

    /// <summary>
    /// Verifies that each gap is checked wherever it stands: inside a list element, an interpolation hole, a lambda, a switch
    /// arm's expression, and a local function inside a lambda
    /// </summary>
    /// <param name="statement">The statement that holds the gap</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow("        Use(await /* c */ t, 1);", DisplayName = "List element")]
    [DataRow("        var h = $\"{await /* c */ t}\";", DisplayName = "Interpolation hole")]
    [DataRow("        Func<Task<int>> l = async () => await /* c */ t;", DisplayName = "Lambda body")]
    [DataRow("        var r = x switch\n                {\n                    1 => new /* c */ List<int>(),\n                    _ => null,\n                };", DisplayName = "Switch arm expression")]
    [DataRow("        Func<int> l = () =>\n        {\n            int L()\n            {\n                return /* c */ 1;\n            }\n\n            return L();\n        };", DisplayName = "Local function in a lambda")]
    public void EveryGapIsCheckedWhereverItStands(string statement)
    {
        AssertUnsupportedFor(KeywordOperandCommentReason, WrapInMethod(statement), "/* c */");
    }

    /// <summary>
    /// Verifies that a gap inside disabled text is not checked: where <c>X</c> is not defined, the statement is disabled text and
    /// the input is formatted, and where it is defined, the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void GapInDisabledTextIsNotChecked(bool defineX)
    {
        var input = WrapInMethod("#if X\n        return /* c */ x;\n#endif\n        return 0;");
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;

        if (defineX)
        {
            AssertUnsupportedFor(KeywordOperandCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// comment behind <c>return</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
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
    /// Verifies that the node-level entry points refuse a target or a context that holds the method with the comment behind
    /// <c>return</c>, and format the method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
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

                var nodeResult = ReihitsuFormatter.FormatNode(statement, cancellationToken: TestContext.CancellationToken);

                Assert.AreEqual(statement.ToFullString(), nodeResult.Output.ToFullString());
                AssertReportsComment(nodeResult.Unsupported, text, "// c");

                var targetResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, statement, TestContext.CancellationToken);

                Assert.AreEqual(text, (await targetResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsComment(targetResult.Unsupported, text, "// c");

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
    /// Verifies that a node-level input holds a gap only when it holds both tokens around it: the expression behind <c>return</c>,
    /// the pattern behind <c>is</c>, and a base list without its type do not hold the shape, while the statement, the pattern
    /// expression, and the type declaration do
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-017")]
    public void NodeThatHoldsOnlyTheTokenBehindTheGapDoesNotHoldTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings("class B;\n\nclass D // c1\n    : B\n{\n    int M(int x, object o)\n    {\n        var p = o is /* c2 */ not null;\n\n        return // c3\n            x;\n    }\n}", endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var returnStatement = root.DescendantNodes().OfType<ReturnStatementSyntax>().Single();
            var isPattern = root.DescendantNodes().OfType<IsPatternExpressionSyntax>().Single();
            var type = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single(declaration => declaration.Identifier.ValueText == "D");

            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(returnStatement.Expression));
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(isPattern.Pattern));
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(type.BaseList));
            Assert.IsNull(ReihitsuFormatter.FormatNode(returnStatement.Expression, cancellationToken: TestContext.CancellationToken).Unsupported);
            Assert.IsNull(ReihitsuFormatter.FormatNode(isPattern.Pattern, cancellationToken: TestContext.CancellationToken).Unsupported);

            Assert.AreEqual(KeywordOperandCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(returnStatement)?.Reason);
            Assert.AreEqual(IsPatternCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(isPattern)?.Reason);
            Assert.AreEqual(BaseListCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(type)?.Reason);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds: a statement built
    /// with a comment behind <c>return</c> is refused, and the same statement without the comment is not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-017")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode()
    {
        var node = SyntaxFactory.ParseStatement("return /* c */ 1;");

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(node);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(KeywordOperandCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, node.ToFullString(), "/* c */", 0, "LF");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(SyntaxFactory.ParseStatement("return 1;")));
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: the output of every join, formatted again, is neither refused nor
    /// changed, also next to comments outside the gaps
    /// </summary>
    /// <param name="statement">The statement whose gap is joined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-017")]
    [DataRow("        return\n\n            x; // c", DisplayName = "Return behind a blank line, comment behind the statement")]
    [DataRow("        // c\n        var p = o is\n            not\n            null;", DisplayName = "Is and not, comment above the statement")]
    [DataRow("        var k = x is 1 /* c */ or\n            2;", DisplayName = "Combinator, comment in front of it")]
    [DataRow("        var c = (long)\n            x; // c", DisplayName = "Cast")]
    [DataRow("        Func<int> u = () /* c */ =>\n            1;", DisplayName = "Lambda, comment in front of the arrow")]
    [DataRow("        var n = new\n            List<int>(); // c", DisplayName = "Object creation")]
    [DataRow("        var w = new\n                {\n                    A\n                        = 1 // c\n                };", DisplayName = "Anonymous-object member")]
    public void OutputOfJoinedGapsIsNotUnsupported(string statement)
    {
        AssertFormattedOutputIsNotRefused(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies that the output of an expression body converted to a block, whose created <c>return</c> joins its value, formatted
    /// again, is neither refused nor changed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-017")]
    public void OutputOfAConvertedExpressionBodyIsNotUnsupported()
    {
        AssertFormattedOutputIsNotRefused("class C\n{\n    int M() =>\n\n        42; // c\n}");
    }

    /// <summary>
    /// Wraps statements in an async method of a class, with the parameters and helpers the statements use
    /// </summary>
    /// <param name="statements">The statements, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statements)
    {
        return WrapInClass($"    async Task<int> M(int x, object o, Task<int> t, int[] a, IAsyncEnumerable<int> s, IAsyncDisposable d, R r)\n    {{\n{statements}\n    }}");
    }

    /// <summary>
    /// Wraps members in a class <c>C</c> with the usings and types its members use
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string members)
    {
        return $"using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing System.Threading.Tasks;\n\nclass B;\n\nrecord R;\n\nstatic class C\n{{\n{members}\n\n    static int M2(int v)\n    {{\n        return v;\n    }}\n\n    static void Use(params object[] values)\n    {{\n    }}\n}}";
    }

    /// <summary>
    /// Asserts that the outcome reports this shape's keyword reason and the span of the given comment in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    /// <param name="comment">The comment that is reported</param>
    private static void AssertReportsComment(UnsupportedSyntax unsupported, string text, string comment)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(KeywordOperandCommentReason, unsupported.Reason);
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