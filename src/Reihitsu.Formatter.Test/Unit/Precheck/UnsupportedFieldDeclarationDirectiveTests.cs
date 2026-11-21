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
/// Tests for the unsupported shape of a directive or disabled text inside a field declaration with several declarators
/// (FMT-UNSUP-010): every place in the declaration outside the leading trivia of its first token, every kind of directive, the
/// declarator count of the build being formatted, every kind of type that can hold the declaration, the excepted leading trivia,
/// event fields and local declarations, the ties with FMT-UNSUP-009 and FMT-UNSUP-008, the order against FMT-UNSUP-004, every documentation mode,
/// every entry point, node-level inputs that hold only part of the declaration, and the closure of the formatter's output, each
/// under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedFieldDeclarationDirectiveTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A class whose combined field holds a conditional group inside the statement lambda of its first value, beside a method
    /// <c>N</c> the formatter changes
    /// </summary>
    private const string RefusedFieldBesideFormattableMethod = """
                                                               using System;

                                                               class C
                                                               {
                                                                   private Func<int> _f = () =>
                                                                   {
                                                               #if DEBUG
                                                                       return 1;
                                                               #else
                                                                       return 2;
                                                               #endif
                                                                   }, _g;

                                                                   void N(){ }
                                                               }
                                                               """;

    /// <summary>
    /// A class whose combined field holds a pragma directive between its declarators, behind the separating comma
    /// </summary>
    private const string PragmaBetweenDeclarators = """
                                                    class C
                                                    {
                                                        private int _a,
                                                    #pragma warning disable CS0169
                                                            _b;
                                                    #pragma warning restore CS0169

                                                        void N(){ }
                                                    }
                                                    """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: each of the four declarations is refused, in both builds of <c>X</c>, and the first
    /// directive, <c>#if X</c> in front of <c>b,</c>, is reported
    /// </summary>
    /// <param name="defineX">Whether the build defines <c>X</c></param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void SpecExampleDirectivesInsideCombinedFieldsAreUnsupported(bool defineX)
    {
        const string input = """
                             using System;

                             class C
                             {
                                 int a,
                             #if X
                                     b,
                             #endif
                                     c;

                                 private int _a,
                             #pragma warning disable CS0169
                                     _b;
                             #pragma warning restore CS0169

                                 [Obsolete]
                             #if X
                                 public
                             #endif
                                 int _c, _d;

                                 private Func<int> _f = () =>
                                 {
                             #if DEBUG
                                     return 1;
                             #else
                                     return 2;
                             #endif
                                 }, _g;
                             }
                             """;

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, input, "#if X", parseOptions: CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies each declaration of the specification example on its own: the input is refused, and its first directive is
    /// reported
    /// </summary>
    /// <param name="member">The declaration, indented by four spaces</param>
    /// <param name="shape">The directive that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [DataRow("    int a,\n#if X\n        b,\n#endif\n        c;", "#if X", DisplayName = "Conditional group between the declarators")]
    [DataRow("    private int _a,\n#pragma warning disable CS0169\n        _b;\n#pragma warning restore CS0169", "#pragma warning disable CS0169", DisplayName = "Pragma directive between the declarators")]
    [DataRow("    [Obsolete]\n#if X\n    public\n#endif\n    int _c, _d;", "#if X", DisplayName = "Conditional group between the attribute list and the type")]
    [DataRow("    private Func<int> _f = () =>\n    {\n#if DEBUG\n        return 1;\n#else\n        return 2;\n#endif\n    }, _g;", "#if DEBUG", DisplayName = "Conditional group inside a lambda value")]
    public void EachDeclarationOfTheSpecExampleIsUnsupported(string member, string shape)
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass(member), shape);
    }

    /// <summary>
    /// Verifies that a directive or disabled text refuses the input at every place of the declaration outside the leading trivia
    /// of its first token, and with every kind of directive
    /// </summary>
    /// <param name="member">The declaration, indented by four spaces</param>
    /// <param name="shape">The directive that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [DataRow("    [Obsolete]\n#pragma warning disable CS0618\n    [NonSerialized]\n    private int _a, _b;", "#pragma warning disable CS0618", DisplayName = "Between two attribute lists")]
    [DataRow("    [Obsolete]\n#region Fields\n    private int _a, _b;\n#endregion", "#region Fields", DisplayName = "Region between the attribute list and the modifier")]
    [DataRow("    private\n#nullable disable\n    static int _a, _b;", "#nullable disable", DisplayName = "Nullable directive between two modifiers")]
    [DataRow("    private\n#if X\n    static\n#endif\n    int _a, _b;", "#if X", DisplayName = "Disabled modifier between a modifier and the type")]
    [DataRow("    private List<\n#if X\n    int\n#else\n    long\n#endif\n    > _a, _b;", "#if X", DisplayName = "Conditional group inside the type")]
    [DataRow("    private int\n#pragma warning disable CS0169\n        _a, _b;", "#pragma warning disable CS0169", DisplayName = "Between the type and the first name")]
    [DataRow("    private int _a\n#pragma warning disable CS0414\n        = 1, _b;", "#pragma warning disable CS0414", DisplayName = "Between a name and its equals sign")]
    [DataRow("    private int _a =\n#if false\n        2 +\n#endif\n        1, _b;", "#if false", DisplayName = "Fully inactive group between the equals sign and the value")]
    [DataRow("    private Func<int> _f = () =>\n    {\n        if (true)\n        {\n#pragma warning disable CS0162\n            return 1;\n        }\n\n        return 2;\n    }, _g;", "#pragma warning disable CS0162", DisplayName = "Pragma deep inside a statement lambda")]
    [DataRow("    private List<Func<int>> _l = [() =>\n    {\n#if DEBUG\n        return 1;\n#else\n        return 2;\n#endif\n    }], _m;", "#if DEBUG", DisplayName = "Conditional group in a lambda nested in a collection expression")]
    [DataRow("    private int _a = 1, _b = 2 +\n#if X\n        3 +\n#endif\n        4;", "#if X", DisplayName = "Conditional group inside the value of the last declarator")]
    [DataRow("    private int _a,\n#region Fields\n        _b;\n#endregion", "#region Fields", DisplayName = "Region between the comma and the next declarator")]
    [DataRow("    private int _a, _b\n#pragma warning disable CS0169\n        ;", "#pragma warning disable CS0169", DisplayName = "In front of the semicolon")]
    [DataRow("    [Obsolete(\n#pragma warning disable CS1030\n        \"m\")]\n    private int _a, _b;", "#pragma warning disable CS1030", DisplayName = "Inside an attribute argument list")]
    [DataRow("    private int _a,\n#if X\n        _b,\n#elif Y\n        _c,\n#else\n        _d,\n#endif\n        _e;", "#if X", DisplayName = "Group with elif and else branches between the declarators")]
    [DataRow("    private int _a,\n#if X\n#endif\n        _b;", "#if X", DisplayName = "Empty conditional group between the declarators")]
    public void DirectiveAtEveryPlaceOfTheDeclarationIsUnsupported(string member, string shape)
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass(member), shape);
    }

    /// <summary>
    /// Verifies that a conditional group whose code is all active, through a symbol the build defines or one the file defines,
    /// refuses the input although it holds no disabled text
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    public void ActiveConditionalGroupBetweenTheDeclaratorsIsUnsupported()
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason,
                             WrapInClass("    private int _a,\n#if X\n        _b,\n#endif\n        _c;"),
                             "#if X",
                             parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("X"));

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, "#define D\nclass C\n{\n    private int _a,\n#if D\n        _b,\n#endif\n        _c;\n}", "#if D");
    }

    /// <summary>
    /// Verifies that the shape is refused in every kind of type that can hold a field declaration, for constants and fixed-size
    /// buffers, and in a type nested in another type, where the document is refused
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="shape">The directive that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [DataRow("struct S\n{\n    private int _a,\n#pragma warning disable CS0169\n        _b;\n}", "#pragma warning disable CS0169", DisplayName = "Struct")]
    [DataRow("record R\n{\n    private int _a,\n#pragma warning disable CS0169\n        _b;\n}", "#pragma warning disable CS0169", DisplayName = "Record")]
    [DataRow("record struct R\n{\n    private int _a,\n#pragma warning disable CS0169\n        _b;\n}", "#pragma warning disable CS0169", DisplayName = "Record struct")]
    [DataRow("interface I\n{\n    static int _a,\n#pragma warning disable CS0169\n        _b;\n}", "#pragma warning disable CS0169", DisplayName = "Static fields of an interface")]
    [DataRow("class C\n{\n    private const int A = 1,\n#pragma warning disable CS0169\n        B = 2;\n}", "#pragma warning disable CS0169", DisplayName = "Constants")]
    [DataRow("unsafe struct S\n{\n    private fixed int _a[2],\n#pragma warning disable CS0169\n        _b[3];\n}", "#pragma warning disable CS0169", DisplayName = "Fixed-size buffers")]
    [DataRow("class C\n{\n    private int _p;\n\n    class D\n    {\n        private int _a,\n#pragma warning disable CS0169\n            _b;\n    }\n}", "#pragma warning disable CS0169", DisplayName = "Nested type")]
    public void ShapeIsUnsupportedInEveryKindOfType(string input, string shape)
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, input, shape);
    }

    /// <summary>
    /// Verifies that the shape is refused in a union declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    public void ShapeIsUnsupportedInAUnion()
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason,
                             "union U(int, string)\n{\n    public static int A = 1,\n#pragma warning disable CS0169\n        B = 2;\n}",
                             "#pragma warning disable CS0169",
                             parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp15));
    }

    /// <summary>
    /// Verifies that the declaration's own tokens decide, whatever declares it: a field declaration with several declarators in
    /// an extension block, which the field split does not split, is refused as well
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    public void ShapeIsUnsupportedInAnExtensionBlock()
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason,
                             "static class E\n{\n    extension(int i)\n    {\n        private static int _x,\n#pragma warning disable CS0169\n            _y;\n    }\n}",
                             "#pragma warning disable CS0169",
                             parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
    }

    /// <summary>
    /// Verifies that a field declaration with one declarator is not refused under this requirement: a directive in front of its
    /// equals sign is formatted in both builds of <c>X</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    public void DirectiveInsideAFieldWithOneDeclaratorIsSupported()
    {
        var member = "    private int _a\n#pragma warning disable CS0414\n        = 1;";

        AssertSupported(WrapInClass(member));
        AssertSupported(WrapInClass(member), CSharpParseOptions.Default.WithPreprocessorSymbols("X"));
    }

    /// <summary>
    /// Verifies that a directive behind a modifier of a field with one declarator is not this shape but the shape of a gap the
    /// line-break phase joins: a directive between two modifiers refuses the input in both builds, and a conditional modifier
    /// refuses it only in the build that compiles the modifier, where its <c>#endif</c> stands behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-017")]
    public void DirectiveBehindAModifierOfAFieldWithOneDeclaratorGivesTheReasonOfTheModifier()
    {
        var between = WrapInClass("    private\n#nullable disable\n    static int _a;");
        var conditional = WrapInClass("    [Obsolete]\n#if X\n    public\n#endif\n    int _a;");

        AssertUnsupportedFor(ModifierCommentReason, between, "#nullable disable");
        AssertUnsupportedFor(ModifierCommentReason, between, "#nullable disable", parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("X"));
        AssertSupported(conditional);
        AssertUnsupportedFor(ModifierCommentReason, conditional, "#endif", parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("X"));
    }

    /// <summary>
    /// Verifies that the declarators are those of the build being formatted: a declarator in disabled text in front of the
    /// semicolon leaves one declarator, and the input is not refused, while in a build that defines the symbol the directive
    /// stands in front of a comma on a later line, and the reason and the location are those of FMT-UNSUP-009
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-009")]
    public void DeclaratorInDisabledTextDoesNotCountAndAtTheCommaTheReasonOfAListSeparatorWins()
    {
        var input = WrapInClass("    private int _a\n#if X\n        , _b\n#endif\n        ;");

        AssertSupported(input);
        AssertUnsupportedFor(ListSeparatorCommentReason, input, "#if X", parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("X"));
    }

    /// <summary>
    /// Verifies that a declaration with two declarators in every build and a third in a conditional group in front of the
    /// semicolon is refused under this requirement where the group is disabled text, and with the reason of FMT-UNSUP-009 where
    /// the group's comma is code
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-009")]
    public void ThirdDeclaratorInAConditionalGroupIsUnsupportedInEveryBuild()
    {
        var input = WrapInClass("    private int _a, _b\n#if X\n        , _c\n#endif\n        ;");

        AssertUnsupportedFor(FieldDeclarationDirectiveReason, input, "#if X");
        AssertUnsupportedFor(ListSeparatorCommentReason, input, "#if X", parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("X"));
    }

    /// <summary>
    /// Verifies that a directive in front of a declarator comma on a later line keeps the reason and the location of FMT-UNSUP-009
    /// also when it holds a value and the declaration has several declarators in every build
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-009")]
    public void DirectiveInFrontOfADeclaratorCommaOnALaterLineKeepsTheReasonOfAListSeparator()
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInClass("    private int _a = 1\n#pragma warning disable CS0414\n        , _b = 2;"), "#pragma warning disable CS0414");
        AssertUnsupportedFor(ListSeparatorCommentReason,
                             WrapInClass("    private int _a = 1\n#if X\n        , _b = 2\n#endif\n        , _c;"),
                             "#if X",
                             parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("X"));
    }

    /// <summary>
    /// Verifies that a directive in front of a comma on a later line that separates no declarators — in an attribute argument
    /// list, in the declaration's type, or inside a value — is refused under this requirement and under FMT-UNSUP-009, starting
    /// at the same directive, and gives this requirement's reason and location
    /// </summary>
    /// <param name="member">The field declaration, indented by four spaces</param>
    /// <param name="shape">The text of the first directive</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-009")]
    [DataRow("    [Obsolete(\"m\"\n#pragma warning disable CS1030\n        , false)]\n    private int _a, _b;", "#pragma warning disable CS1030", DisplayName = "Attribute argument list")]
    [DataRow("    private (int A\n#pragma warning disable CS0169\n        , int B) _a, _b;", "#pragma warning disable CS0169", DisplayName = "Tuple type")]
    [DataRow("    private int _a = Math.Max(1\n#pragma warning disable CS0414\n        , 2), _b;", "#pragma warning disable CS0414", DisplayName = "Call in a value")]
    [DataRow("    private int _a = Math.Max(1\n#if DEBUG\n        , 2\n#else\n        , 3\n#endif\n        ), _b;", "#if DEBUG", DisplayName = "Conditional group in a value")]
    public void DirectiveInFrontOfAnotherCommaOnALaterLineGivesTheReasonOfThisRequirement(string member, string shape)
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass(member), shape);
    }

    /// <summary>
    /// Verifies that a directive between an operator of a conditional expression in a value and its branch is refused under this
    /// requirement and under FMT-UNSUP-008, starting at the same directive, and gives this requirement's reason and location
    /// </summary>
    /// <param name="member">The field declaration, indented by four spaces</param>
    /// <param name="shape">The text of the first directive</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-008")]
    [DataRow("    private int _a = true ?\n#pragma warning disable CS0162\n        1 : 2, _b;", "#pragma warning disable CS0162", DisplayName = "Behind the question mark")]
    [DataRow("    private int _a = true ? 1 :\n#pragma warning disable CS0162\n        2, _b;", "#pragma warning disable CS0162", DisplayName = "Behind the colon")]
    [DataRow("    private int _a = true ?\n#if DEBUG\n        1\n#else\n        2\n#endif\n        : 3, _b;", "#if DEBUG", DisplayName = "Conditional group behind the question mark")]
    [DataRow("    private int _a = true ?\n#pragma warning disable CS0162\n        1 : Math.Max(2\n        // c\n        , 3), _b;", "#pragma warning disable CS0162", DisplayName = "Behind the question mark, with a comment in front of a later comma of a call")]
    public void DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirement(string member, string shape)
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass(member), shape);
    }

    /// <summary>
    /// Verifies that a directive between an operator of a conditional expression in a value and its branch gives this
    /// requirement's reason and location also when a comment, a directive, or disabled text stands later in the text in front of a
    /// comma between two declarators on a later line — of the same field, of another field, or of a local declaration inside the
    /// value: the directive is refused under this requirement and under FMT-UNSUP-008, starting at the same directive, and it is
    /// no directive in front of a declarator comma
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <param name="shape">The text of the first directive</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-008")]
    [DataRow("    private int _a = true ?\n#pragma warning disable CS0162\n        1 : 2\n#pragma warning disable CS0414\n        , _b;", "#pragma warning disable CS0162", DisplayName = "Behind the question mark, directive in front of the declarator comma")]
    [DataRow("    private int _a = true ? 1 :\n#pragma warning disable CS0162\n        2\n#pragma warning disable CS0414\n        , _b;", "#pragma warning disable CS0162", DisplayName = "Behind the colon, directive in front of the declarator comma")]
    [DataRow("    private int _a = true ?\n#if DEBUG\n        1\n#else\n        2\n#endif\n        : 3\n#pragma warning disable CS0414\n        , _b;", "#if DEBUG", DisplayName = "Conditional group behind the question mark, directive in front of the declarator comma")]
    [DataRow("    private int _a = true ?\n#pragma warning disable CS0162\n        1 : 2\n        // c\n        , _b;", "#pragma warning disable CS0162", DisplayName = "Behind the question mark, comment in front of the declarator comma")]
    [DataRow("    private int _a = true ?\n#pragma warning disable CS0162\n        1 : 2, _b;\n\n    private int _c\n        // c\n        , _d;", "#pragma warning disable CS0162", DisplayName = "Behind the question mark, comment in front of the declarator comma of a later field")]
    [DataRow("    private Func<int> _f = () =>\n    {\n        var a = true ?\n#pragma warning disable CS0162\n            1 : 2;\n        int x = a\n            // c\n            , y = 3;\n\n        return x + y;\n    }, _g;", "#pragma warning disable CS0162", DisplayName = "Behind the question mark in a lambda value, comment in front of the declarator comma of a later local")]
    public void DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirementBeforeALaterDeclaratorComma(string members, string shape)
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass(members), shape);
    }

    /// <summary>
    /// Verifies that a directive in front of a comma on a later line between two declarators of a local declaration inside a
    /// value of the field is refused under this requirement and under FMT-UNSUP-009, starting at the same directive, and keeps
    /// the reason and the location of FMT-UNSUP-009: it stands between a declarator and a comma on a later line
    /// </summary>
    /// <param name="member">The field declaration, indented by four spaces</param>
    /// <param name="shape">The text of the first directive</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-009")]
    [DataRow("    private Func<int> _f = () =>\n    {\n        int x = 1\n#pragma warning disable CS0168\n            , y = 2;\n\n        return x + y;\n    }, _g;", "#pragma warning disable CS0168", DisplayName = "Pragma directive")]
    [DataRow("    private Func<int> _f = () =>\n    {\n        int x = 1\n#if DEBUG\n            , y = 2\n#else\n            , y = 3\n#endif\n            ;\n\n        return x + y;\n    }, _g;", "#if DEBUG", DisplayName = "Conditional group")]
    public void DirectiveInFrontOfADeclaratorCommaOfALocalInAValueKeepsTheReasonOfAListSeparator(string member, string shape)
    {
        AssertUnsupportedFor(ListSeparatorCommentReason, WrapInClass(member), shape);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives this requirement's reason and location for a
    /// directive between a conditional operator in a value and its branch both for the whole document, which also holds a later
    /// field with a comment in front of its declarator comma, and for the field declaration alone, which does not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-008")]
    public void FindUnsupportedSyntaxGivesThisRequirementsReasonBehindAConditionalOperatorForTheDocumentAndTheField()
    {
        const string shape = "#pragma warning disable CS0162";

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var source = WrapInClass("    private int _a = true ?\n#pragma warning disable CS0162\n        1 : 2, _b;\n\n    private int _c\n        // c\n        , _d;").Replace("\n", endOfLine);
            var root = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var field = root.DescendantNodes().OfType<FieldDeclarationSyntax>().First();

            AssertReportsShape(ReihitsuFormatter.FindUnsupportedSyntax(root), source, shape);
            AssertReportsShape(ReihitsuFormatter.FindUnsupportedSyntax(field), source, shape);
        }
    }

    /// <summary>
    /// Verifies the other side of the two ties above: in a field with one declarator, this requirement does not apply, and the
    /// same directive gives the reason and the location of FMT-UNSUP-009 or FMT-UNSUP-008
    /// </summary>
    /// <param name="member">The field declaration, indented by four spaces</param>
    /// <param name="shape">The text of the first directive</param>
    /// <param name="reason">The expected reason</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-009")]
    [Requirement("FMT-UNSUP-008")]
    [DataRow("    [Obsolete(\"m\"\n#pragma warning disable CS1030\n        , false)]\n    private int _a;", "#pragma warning disable CS1030", ListSeparatorCommentReason, DisplayName = "Attribute argument list")]
    [DataRow("    private int _a = Math.Max(1\n#pragma warning disable CS0414\n        , 2);", "#pragma warning disable CS0414", ListSeparatorCommentReason, DisplayName = "Call in a value")]
    [DataRow("    private int _a = true ?\n#pragma warning disable CS0162\n        1 : 2;", "#pragma warning disable CS0162", ConditionalOperatorCommentReason, DisplayName = "Behind the question mark")]
    [DataRow("    private int _a = true ? 1 :\n#pragma warning disable CS0162\n        2;", "#pragma warning disable CS0162", ConditionalOperatorCommentReason, DisplayName = "Behind the colon")]
    public void DirectiveAtTheSamePlaceOfAFieldWithOneDeclaratorGivesTheReasonOfTheOtherShape(string member, string shape, string reason)
    {
        AssertUnsupportedFor(reason, WrapInClass(member), shape);
    }

    /// <summary>
    /// Verifies that an event field declaration and a local declaration statement with a conditional group between their
    /// declarators are not refused, in both builds of <c>X</c>
    /// </summary>
    /// <param name="defineX">Whether the build defines <c>X</c></param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void EventFieldAndLocalDeclarationAreSupported(bool defineX)
    {
        const string input = """
                             using System;

                             class C
                             {
                                 public event Action A,
                             #if X
                                     B,
                             #endif
                                     D;

                                 void M()
                                 {
                                     int a,
                             #if X
                                         b,
                             #endif
                                         c;
                                 }
                             }
                             """;

        AssertSupported(input, CreateOptions(defineX, "X"));
    }

    /// <summary>
    /// Verifies that a directive or disabled text in the leading trivia of the declaration's first token is not refused: a region,
    /// a conditional group around the declaration, a pragma directive and a nullable directive in front of it, the end of a group
    /// with disabled text in front of it, and a directive behind its semicolon
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [DataRow("    #region Fields\n\n    private int _x, _y;\n\n    #endregion // Fields", DisplayName = "Region around the declaration")]
    [DataRow("#if !RELEASE\n    private int _p, _q = 1;\n#endif", DisplayName = "Conditional group around the declaration")]
    [DataRow("#if !X\n    private int _p, _q;\n#else\n    private long _p, _q;\n#endif", DisplayName = "Declaration in the active branch with an else branch behind it")]
    [DataRow("#pragma warning disable CS0169\n    private int _a, _b;\n#pragma warning restore CS0169", DisplayName = "Pragma directives in front of and behind the declaration")]
    [DataRow("#nullable disable\n    private string _a, _b;", DisplayName = "Nullable directive in front of the declaration")]
    [DataRow("#if DEBUG\n    [Obsolete]\n#endif\n    private int _a, _b;", DisplayName = "Disabled attribute list in front of the declaration")]
    [DataRow("#if NETSTANDARD\n    static\n#endif\n    private int _a, _b;", DisplayName = "Disabled modifier in front of the declaration")]
    [DataRow("    [Obsolete]\n    private int _a, _b;\n#if X\n    private int _c;\n#endif", DisplayName = "Group behind the semicolon")]
    public void DirectiveInTheLeadingTriviaOfTheFirstTokenIsSupported(string members)
    {
        AssertSupported(WrapInClass(members));
    }

    /// <summary>
    /// Verifies that of a comment that FMT-UNSUP-004 refuses and a directive in the same declaration the one first in the text
    /// gives the reason and the location
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-001")]
    public void FirstOfACommentAndADirectiveInTheDeclarationIsReported()
    {
        AssertUnsupportedFor(VariableDeclarationCommentReason, WrapInClass("    private int /* c */ _a,\n#pragma warning disable CS0169\n        _b;"), "/* c */");
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, WrapInClass("    private int _a,\n#pragma warning disable CS0169\n        _b /* c */;"), "#pragma warning disable CS0169");
    }

    /// <summary>
    /// Verifies that the outcome does not depend on the documentation mode: a documented declaration with a directive between its
    /// declarators is refused under this requirement in every mode
    /// </summary>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.None)]
    [DataRow(DocumentationMode.Diagnose)]
    public void DocumentedDeclarationWithADirectiveIsUnsupportedInEveryDocumentationMode(DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(FieldDeclarationDirectiveReason,
                             WrapInClass("    /// <summary>The fields</summary>\n    private int _a,\n#pragma warning disable CS0169\n        _b;"),
                             "#pragma warning disable CS0169",
                             parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> returns a refused document unchanged and reports the
    /// directive inside the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-002")]
    public async Task FormatDocumentAsyncRefusesTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(RefusedFieldBesideFormattableMethod, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);

                var result = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken);

                Assert.AreEqual(text, (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsShape(result.Unsupported, text, "#if DEBUG");
            }
        }
    }

    /// <summary>
    /// Verifies that the node-level entry points refuse the field declaration, as a detached node, as a target, and inside the
    /// context of another target, and format the method beside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-002")]
    public async Task NodeLevelEntryPointsRefuseATargetThatHoldsTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(RefusedFieldBesideFormattableMethod, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var type = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
                var field = root.DescendantNodes().OfType<FieldDeclarationSyntax>().Single();
                var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

                var nodeResult = ReihitsuFormatter.FormatNode(field, cancellationToken: TestContext.CancellationToken);

                Assert.AreEqual(field.ToFullString(), nodeResult.Output.ToFullString());
                AssertReportsShape(nodeResult.Unsupported, text, "#if DEBUG");

                var targetResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, field, TestContext.CancellationToken);

                Assert.AreEqual(text, (await targetResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsShape(targetResult.Unsupported, text, "#if DEBUG");

                var contextResult = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, method, type, TestContext.CancellationToken);

                Assert.AreEqual(text, (await contextResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                AssertReportsShape(contextResult.Unsupported, text, "#if DEBUG");

                var besideResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, method, TestContext.CancellationToken);

                Assert.IsNull(besideResult.Unsupported);
                Assert.AreNotEqual(text, (await besideResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString(), "The method beside the shape must be formatted.");
            }
        }
    }

    /// <summary>
    /// Verifies that a node-level target that holds only part of the declaration — its variable declaration without the field, a
    /// declarator, or the value that holds the directive — is not refused, as a target and as a detached node
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-003")]
    public async Task TargetThatHoldsOnlyPartOfTheDeclarationIsNotRefused()
    {
        foreach (var source in new[] { RefusedFieldBesideFormattableMethod, PragmaBetweenDeclarators })
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, source);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var field = root.DescendantNodes().OfType<FieldDeclarationSyntax>().Single();
                var parts = new SyntaxNode[] { field.Declaration, field.Declaration.Variables[0], field.Declaration.Variables[1] }.Concat(field.Declaration.Variables.Select(variable => variable.Initializer).Where(initializer => initializer != null));

                foreach (var part in parts)
                {
                    var targetResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, part, TestContext.CancellationToken);

                    Assert.IsNull(targetResult.Unsupported, $"The {part.Kind()} target must not be refused: {targetResult.Unsupported?.Reason}");

                    var nodeResult = ReihitsuFormatter.FormatNode(part, cancellationToken: TestContext.CancellationToken);

                    Assert.IsNull(nodeResult.Unsupported, $"The detached {part.Kind()} must not be refused: {nodeResult.Unsupported?.Reason}");
                }
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports the shape for the field declaration, its type,
    /// and the root, and nothing for a node that holds only part of the declaration or for the method beside it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-007")]
    public void FindUnsupportedSyntaxAnswersForTheDeclarationAndNotForPartOfIt()
    {
        foreach (var (source, shape) in new[] { (RefusedFieldBesideFormattableMethod, "#if DEBUG"), (PragmaBetweenDeclarators, "#pragma warning disable CS0169") })
        {
            var root = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var field = root.DescendantNodes().OfType<FieldDeclarationSyntax>().Single();

            AssertReportsShape(ReihitsuFormatter.FindUnsupportedSyntax(field), source, shape);
            AssertReportsShape(ReihitsuFormatter.FindUnsupportedSyntax(root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single()), source, shape);
            AssertReportsShape(ReihitsuFormatter.FindUnsupportedSyntax(root), source, shape);
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(field.Declaration));
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(field.Declaration.Variables[0]));
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(field.Declaration.Variables[1]));
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single()));
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> reports a nested type that holds the shape, and the
    /// outer type that holds the nested type, and nothing for a combined field behind a region in the outer type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-007")]
    public void FindUnsupportedSyntaxReportsTheNestedTypeAndTheOuterType()
    {
        const string source = """
                              class C
                              {
                                  #region Fields

                                  private int _p, _q;

                                  #endregion // Fields

                                  class D
                                  {
                                      private int _a,
                              #pragma warning disable CS0169
                                          _b;
                                  }
                              }
                              """;

        var root = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
        var types = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        var outerField = root.DescendantNodes().OfType<FieldDeclarationSyntax>().First();

        AssertReportsShape(ReihitsuFormatter.FindUnsupportedSyntax(types[0]), source, "#pragma warning disable CS0169");
        AssertReportsShape(ReihitsuFormatter.FindUnsupportedSyntax(types[1]), source, "#pragma warning disable CS0169");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(outerField));
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> gives the answer the precheck gives for the same tree
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    [Requirement("FMT-UNSUP-007")]
    [DataRow("class C\n{\n    private int _a,\n#pragma warning disable CS0169\n        _b;\n}")]
    [DataRow("class C\n{\n    [System.Obsolete]\n#if X\n    public\n#endif\n    int _c, _d;\n}")]
    [DataRow("class C\n{\n    #region Fields\n    private int _x, _y;\n    #endregion\n}")]
    [DataRow("class C\n{\n#if DEBUG\n    [System.Obsolete]\n#endif\n    private int _a, _b;\n}")]
    [DataRow("class C\n{\n    private int _a\n#if X\n        , _b\n#endif\n        ;\n}")]
    public void FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var found = ReihitsuFormatter.FindUnsupportedSyntax(tree.GetRoot(TestContext.CancellationToken));
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(formatted.Unsupported?.Reason, found?.Reason);
        Assert.AreEqual(formatted.Unsupported?.Location.SourceSpan, found?.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: combined fields with a directive or disabled text in front of them are
    /// formatted, the split ones and the ones kept whole, and their output, formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-010")]
    [DataRow("class C\n{\n    #region Fields\n\n    private int _x, _y;\n\n    #endregion // Fields\n\n#if !RELEASE\n    private int _p, _q = 1;\n#endif\n}", DisplayName = "Region and conditional group around the declarations")]
    [DataRow("class C\n{\n#pragma warning disable CS0169\n    private int _a, _b;\n#pragma warning restore CS0169\n}", DisplayName = "Pragma directives around the declaration")]
    [DataRow("#define D\nclass C\n{\n#if D\n    private int _x;\n#endif\n    private int _m, _n;\n}", DisplayName = "End of an active group in front of the declaration")]
    [DataRow("using System;\n\nclass C\n{\n    #region Fields\n\n    [NonSerialized, Obsolete]\n    private int _a = 1, _b;\n\n    #endregion\n}", DisplayName = "Region in front of an attribute list with two attributes")]
    [DataRow("class C\n{\n#if DEBUG\n    [System.Obsolete]\n#endif\n    private int _a, _b;\n}", DisplayName = "Disabled attribute list in front of the declaration")]
    [DataRow("class C {\n    #region Fields\n    private int _a,_b; private int _c;\n    #endregion\n}", DisplayName = "Unformatted declarations behind a region")]
    public void OutputOfFormattedDeclarationsIsNotUnsupported(string input)
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
    /// Wraps members in a class <c>C</c>
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string members)
    {
        return $"using System;\nusing System.Collections.Generic;\n\nclass C\n{{\n{members}\n}}";
    }

    /// <summary>
    /// Creates parse options that define the given symbol or none
    /// </summary>
    /// <param name="define">Whether the symbol is defined</param>
    /// <param name="symbol">The symbol</param>
    /// <returns>The parse options</returns>
    private static CSharpParseOptions CreateOptions(bool define, string symbol)
    {
        return define ? CSharpParseOptions.Default.WithPreprocessorSymbols(symbol) : CSharpParseOptions.Default;
    }

    /// <summary>
    /// Asserts that the outcome reports the reason of FMT-UNSUP-010 and the span of the given directive in the given text
    /// </summary>
    /// <param name="unsupported">The reported outcome</param>
    /// <param name="text">The text of the caller's syntax tree</param>
    /// <param name="shape">The directive that is reported</param>
    private static void AssertReportsShape(UnsupportedSyntax unsupported, string text, string shape)
    {
        Assert.IsNotNull(unsupported, "The input must be reported as unsupported.");
        Assert.AreEqual(FieldDeclarationDirectiveReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, text, shape, 0, "the document's");
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

    #endregion // Methods
}