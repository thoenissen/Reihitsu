using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that the node-level entry points <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> and
/// <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> lay out the node they format at its own column, with
/// every line in the column the indentation phase gives it and no line moved after the last phase, apart from the leading
/// trivia <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> writes back as in the document
/// </summary>
[TestClass]
public class ReihitsuFormatterNodeColumnTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: an equals-value target whose <c>new</c> stands behind the last line of a multi-line comment
    /// is laid out from <c>new</c> where it stands, because the target holds no variable declaration, while document-level
    /// formatting refuses the document for the comment behind the declaration's <c>=</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-UNSUP-003")]
    public async Task EqualsValueTargetBehindTheLastLineOfAMultiLineCommentIsLaidOutFromNewWhereItStands()
    {
        const string input = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d = /* a\n                   b */ new List<int>\n          {\n            1\n          };\n    }\n}";
        const string expected = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d = /* a\n                   b */ new List<int>\n                        {\n                            1\n                        };\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<EqualsValueClauseSyntax>);
        AssertUnsupported(input, "/* a\n                   b */");
    }

    /// <summary>
    /// Verifies the spec example: an equals-value target that holds a multi-line raw string moves the content line and the
    /// closing marker under the opening quotes, as document-level formatting does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-RAW-001")]
    public async Task EqualsValueTargetHoldingRawStringMovesContentUnderTheOpeningQuotes()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var s = \"\"\"\n    a\n    \"\"\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var s = \"\"\"\n                a\n                \"\"\";\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<EqualsValueClauseSyntax>);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a raw string literal formatted as the target on its own, which reaches the indentation phase still attached
    /// to its document because no earlier phase rewrites it, moves its content line and closing marker under its opening quotes,
    /// as document-level formatting does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-RAW-001")]
    public async Task RawStringLiteralTargetMovesContentUnderTheOpeningQuotes()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var s = \"\"\"\n    a\n    \"\"\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var s = \"\"\"\n                a\n                \"\"\";\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<LiteralExpressionSyntax>);
    }

    /// <summary>
    /// Verifies the spec example: the tabs in the leading whitespace of a statement target count as four columns each, so the
    /// statement starts in column 8 and the wrapped argument aligns behind it, while the lines outside the target keep their
    /// tabs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-TAB-001")]
    public async Task StatementTargetIndentedWithTabsCountsEachOfItsTabsAsFourColumns()
    {
        const string input = "class C\n{\n\tvoid M()\n\t{\n\t\tCall(1,\n2);\n\t}\n}";
        const string expected = "class C\n{\n\tvoid M()\n\t{\n        Call(1,\n             2);\n\t}\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<ExpressionStatementSyntax>);
    }

    /// <summary>
    /// Verifies the spec example: two tabs in front of an equals-value target belong to the code in front of it and are kept,
    /// so they count as one column each, <c>new</c> stands in column 10, and the braces are written with ten spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task EqualsValueTargetBehindTabsCountsEachTabInFrontOfItAsOneColumn()
    {
        const string input = "using System.Collections.Generic;\n\nclass C\n{\n\tvoid M()\n\t{\n\t\tvar d = new List<int>\n\t\t  {\n\t\t\t1\n\t\t  };\n\t}\n}";
        const string expected = "using System.Collections.Generic;\n\nclass C\n{\n\tvoid M()\n\t{\n\t\tvar d = new List<int>\n          {\n              1\n          };\n\t}\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<EqualsValueClauseSyntax>);
    }

    /// <summary>
    /// Verifies that a tab inside a comment in front of an equals-value target is kept and counts as one column, so the braces
    /// align with <c>new</c> in its character column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-TAB-003")]
    public async Task EqualsValueTargetBehindTabInsideCommentCountsTheTabAsOneColumn()
    {
        const string input = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d /*\t*/ = new List<int>\n          {\n            1\n          };\n    }\n}";
        const string expected = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d /*\t*/ = new List<int>\n                      {\n                          1\n                      };\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<EqualsValueClauseSyntax>);
    }

    /// <summary>
    /// Verifies that a misindented statement target led by a one-line comment keeps its first token in its column, and the
    /// wrapped argument aligns under the first argument behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task MisindentedStatementTargetLedByCommentKeepsItsFirstTokenAndAlignsTheWrappedArgument()
    {
        const string input = "class C\n{\n    void M()\n    {\n     /* c */ Call(1,\n2);\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n     /* c */ Call(1,\n                  2);\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<ExpressionStatementSyntax>);
    }

    /// <summary>
    /// Verifies that a method target in a class indented with two spaces keeps its first token in column 2, and the lines the
    /// depth places keep their place relative to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task MethodTargetInTwoSpaceClassKeepsItsLinesRelativeToItsFirstToken()
    {
        const string input = "class C\n{\n  void M()\n  {\nCall();\n  }\n}";
        const string expected = "class C\n{\n  void M()\n  {\n      Call();\n  }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<MethodDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that the leading comment of a type target that is a direct child of the root is written back as it stands, while
    /// the type is laid out from its first token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-PIPE-010")]
    public async Task TypeTargetKeepsItsLeadingCommentAsItStands()
    {
        const string input = "// c\n  class D\n  {\nint x;\n  }";
        const string expected = "// c\n  class D\n  {\n      int x;\n  }";

        await AssertFormatsTarget(input, expected, SelectSingle<ClassDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that formatting a chain with an equals-value clause as context aligns the wrapped link under the first link of a
    /// chain behind the last line of a multi-line comment, because the context holds no variable declaration, while
    /// document-level formatting refuses the document for the comment behind the declaration's <c>=</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-INDENT-029")]
    public async Task ChainTargetWithContextBehindTheLastLineOfAMultiLineCommentAlignsTheWrappedLink()
    {
        const string input = "class C\n{\n    void M(string s)\n    {\n        var x = /* a\n                   b */ s.Trim()\n.ToUpper();\n    }\n}";
        const string expected = "class C\n{\n    void M(string s)\n    {\n        var x = /* a\n                   b */ s.Trim()\n                         .ToUpper();\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<InvocationExpressionSyntax>().First(),
                                             SelectSingle<EqualsValueClauseSyntax>);
        AssertUnsupported(input, "/* a\n                   b */");
    }

    /// <summary>
    /// Verifies that a one-line argument that starts a later line of its statement, formatted with the statement as context,
    /// keeps the whitespace in front of it as in the document, while the argument itself is formatted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task OneLineArgumentTargetOnALaterLineOfItsContextKeepsTheWhitespaceInFrontOfIt()
    {
        const string input = "class C\n{\n    void M()\n    {\n        Call(1,\n                 2  +  3);\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        Call(1,\n                 2 + 3);\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<ArgumentSyntax>().Last(),
                                             SelectSingle<ExpressionStatementSyntax>);
    }

    /// <summary>
    /// Verifies that a declarator that starts a later line of a declaration indented with tabs, formatted with the declaration
    /// as context, keeps the tabs and spaces in front of it as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task DeclaratorTargetOnALaterLineOfATabIndentedContextKeepsTheWhitespaceInFrontOfIt()
    {
        const string input = "class C\n{\n\tvoid M()\n\t{\n\t\tint a = 1,\n\t\t    b =\n\t\t        2;\n\t}\n}";
        const string expected = "class C\n{\n\tvoid M()\n\t{\n\t\tint a = 1,\n\t\t    b = 2;\n\t}\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Last(),
                                             SelectSingle<LocalDeclarationStatementSyntax>);
    }

    /// <summary>
    /// Verifies that a target that starts the first line of its context, whose first token the context's layout leaves in
    /// place, keeps the whitespace in front of it, and the phases lay out the target from that column, so the wrapped argument
    /// aligns behind the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task TargetStartingTheFirstLineOfItsContextIsLaidOutFromTheWhitespaceInFrontOfIt()
    {
        const string input = "class C\n{\n    void M()\n    {\n     Call(1,\n2);\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n     Call(1,\n          2);\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectSingle<InvocationExpressionSyntax>,
                                             SelectSingle<ExpressionStatementSyntax>);
    }

    /// <summary>
    /// Verifies that the tabs in front of a target that starts the first line of its context are kept and count as one column
    /// each, so the wrapped argument aligns behind the first argument in its character column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-PIPE-018")]
    public async Task TargetStartingTheFirstLineOfItsContextCountsEachTabInFrontOfItAsOneColumn()
    {
        const string input = "class C\n{\n\tvoid M()\n\t{\n\t\tCall(1,\n2);\n\t}\n}";
        const string expected = "class C\n{\n\tvoid M()\n\t{\n\t\tCall(1,\n       2);\n\t}\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectSingle<InvocationExpressionSyntax>,
                                             SelectSingle<ExpressionStatementSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind two blank lines, one of which document-level formatting would remove, keeps
    /// both blank lines and its column as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task EnumMemberTargetBehindTwoBlankLinesKeepsBothBlankLinesAndItsColumn()
    {
        const string input = "enum E\n{\n    A,\n\n\n  B =\n  2,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n\n\n  B = 2,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind a comment line, above which document-level formatting would add a blank line,
    /// gets no blank line, and the comment and the member keep their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task EnumMemberTargetBehindCommentLineKeepsTheCommentAndTheColumnsAsInTheDocument()
    {
        const string input = "enum E\n{\n    A,\n      // b\n  B =\n  2,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n      // b\n  B = 2,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind a conditional directive and a blank line keeps the directive in its column,
    /// the blank line behind it, and the member's column as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task EnumMemberTargetBehindConditionalDirectiveAndBlankLineKeepsBothAsInTheDocument()
    {
        const string input = "enum E\n{\n    A,\n  #if !DEBUG\n\n      B =\n      2,\n#endif\n    C\n}";
        const string expected = "enum E\n{\n    A,\n  #if !DEBUG\n\n      B = 2,\n#endif\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind a region directive, around which document-level formatting would add blank
    /// lines, gets no blank line, and the directive and the member keep their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task EnumMemberTargetBehindRegionDirectiveKeepsTheDirectiveAndTheColumnsAsInTheDocument()
    {
        const string input = "enum E\n{\n    A,\n  #region R\n  B =\n  2,\n  #endregion\n    C\n}";
        const string expected = "enum E\n{\n    A,\n  #region R\n  B = 2,\n  #endregion\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind a documentation comment, above which document-level formatting would add a
    /// blank line, gets no blank line, and every line of the comment and the member keep their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task EnumMemberTargetBehindDocumentationCommentKeepsTheCommentAndTheColumnsAsInTheDocument()
    {
        const string input = "enum E\n{\n    A,\n      /// <summary>\n      /// B\n      /// </summary>\n  B =\n  2,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n      /// <summary>\n      /// B\n      /// </summary>\n  B = 2,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind two blank lines and a documentation comment keeps both blank lines, the
    /// comment, and the columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task EnumMemberTargetBehindTwoBlankLinesAndDocumentationCommentKeepsThemAsInTheDocument()
    {
        const string input = "enum E\n{\n    A,\n\n\n      /// <summary>\n      /// B\n      /// </summary>\n  B =\n  2,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n\n\n      /// <summary>\n      /// B\n      /// </summary>\n  B = 2,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target behind two comments with a blank line between them keeps exactly that blank line,
    /// and both comments and the member keep their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task EnumMemberTargetBehindSeveralCommentsKeepsThemAsInTheDocument()
    {
        const string input = "enum E\n{\n    A,\n      // b\n\n  /* c */\n      B =\n      2,\n    C\n}";
        const string expected = "enum E\n{\n    A,\n      // b\n\n  /* c */\n      B = 2,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that a parameter target behind two blank lines and a comment keeps both blank lines, the comment, and the
    /// columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task ParameterTargetBehindTwoBlankLinesAndCommentKeepsThemAsInTheDocument()
    {
        const string input = "class C\n{\n    void M(int a,\n\n\n      // b\n               int b =\n               1)\n    {\n    }\n}";
        const string expected = "class C\n{\n    void M(int a,\n\n\n      // b\n               int b = 1)\n    {\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<ParameterSyntax>().Last(),
                                             SelectSingle<ParameterListSyntax>);
    }

    /// <summary>
    /// Verifies the spec example: with the enum member <c>B</c> as target and the enum as context, the two blank lines above
    /// <c>B</c> stay, and <c>B</c> keeps its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task SpecExampleTwoBlankLinesAboveEnumMemberStay()
    {
        const string input = "enum E\n{\n    A,\n\n\n  B =\n  2\n}";
        const string expected = "enum E\n{\n    A,\n\n\n  B = 2\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies the spec example: with a comment line above the enum member <c>B</c>, no blank line is added above the comment,
    /// and the comment and <c>B</c> keep their columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    public async Task SpecExampleNoBlankLineIsAddedAboveCommentInFrontOfEnumMember()
    {
        const string input = "enum E\n{\n    A,\n      // b\n  B =\n  2\n}";
        const string expected = "enum E\n{\n    A,\n      // b\n  B = 2\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that an enum member target whose documentation comment is written behind the member in front of it keeps that
    /// comment on the line of the previous member, with nothing inserted in front of it, and keeps its own column, as in the
    /// document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task EnumMemberTargetWithDocumentationCommentBehindThePreviousMemberKeepsThatLineAsWritten()
    {
        const string input = "enum E\n{\n    A, /// Doc\n    B =\n        2,\n    C\n}";
        const string expected = "enum E\n{\n    A, /// Doc\n    B = 2,\n    C\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectEnumMemberB,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies that a parameter target whose documentation comment is written behind the parameter in front of it keeps that
    /// comment on the line of the previous parameter, with nothing inserted in front of it, and keeps its own column, as in
    /// the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task ParameterTargetWithDocumentationCommentBehindThePreviousParameterKeepsThatLineAsWritten()
    {
        const string input = "class C\n{\n    void M(int a, /// Doc\n           int b =\n               1)\n    {\n    }\n}";
        const string expected = "class C\n{\n    void M(int a, /// Doc\n           int b = 1)\n    {\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<ParameterSyntax>().Last(),
                                             SelectSingle<ParameterListSyntax>);
    }

    /// <summary>
    /// Verifies that a chain target whose documentation comment is written behind the opening brace of its block, formatted
    /// with the block as context, keeps that comment on the brace's line, with nothing inserted in front of it, while the
    /// wrapped link is aligned
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task ChainTargetWithDocumentationCommentBehindTheOpeningBraceKeepsThatLineAsWritten()
    {
        const string input = "class C\n{\n    void M(string a)\n    { /// Doc\n        a.Trim()\n        .ToUpper();\n    }\n}";
        const string expected = "class C\n{\n    void M(string a)\n    { /// Doc\n        a.Trim()\n         .ToUpper();\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<InvocationExpressionSyntax>().First(),
                                             root => root.DescendantNodes().OfType<BlockSyntax>().Single());
    }

    /// <summary>
    /// Verifies that a chain target whose documentation comment is written behind the statement in front of it, formatted with
    /// the block as context, keeps that comment on the statement's line, with nothing inserted in front of it, while the
    /// wrapped link is aligned
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task ChainTargetWithDocumentationCommentBehindThePreviousStatementKeepsThatLineAsWritten()
    {
        const string input = "class C\n{\n    void M(string a)\n    {\n        var x = 1; /// Doc\n        a.Trim()\n        .ToUpper();\n    }\n}";
        const string expected = "class C\n{\n    void M(string a)\n    {\n        var x = 1; /// Doc\n        a.Trim()\n         .ToUpper();\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<InvocationExpressionSyntax>().First(),
                                             root => root.DescendantNodes().OfType<BlockSyntax>().Single());
    }

    /// <summary>
    /// Verifies the other side of the requirement's qualifier: a target that is a direct child of the root, whose leading
    /// trivia FMT-PIPE-010 keeps, has that trivia written back as it stands: the two blank lines, which the phases would
    /// collapse, and the comment and the first token in column 2, which the phases would move to column 0
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-018")]
    [Requirement("FMT-PIPE-010")]
    public async Task DirectChildOfRootTargetWithContextKeepsItsLeadingTriviaAsItStands()
    {
        const string input = "class A\n{\n}\n\n\n  // c\n  class D\n{\n    int x;\n}";
        const string expected = "class A\n{\n}\n\n\n  // c\n  class D\n{\n    int x;\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single(type => type.Identifier.Text == "D"),
                                             root => root);
    }

    /// <summary>
    /// Verifies that a statement target indented with tabs that holds a line break other than the document's line ending, which
    /// the preparation phase rewrites before every other phase, still counts each of its tabs as four columns, so the statement
    /// starts in column 8 and the wrapped argument aligns behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task StatementTargetIndentedWithTabsHoldingAForeignLineBreakCountsEachOfItsTabsAsFourColumns()
    {
        const string input = "class C\n{\n\tvoid M()\n\t{\n\t\tCall(1,¶2);\n\t}\n}";
        const string expected = "class C\n{\n\tvoid M()\n\t{\n        Call(1,\n             2);\n\t}\n}";

        await AssertFormatsTargetWithForeignLineBreaks(input, expected, SelectSingle<ExpressionStatementSyntax>, null);
    }

    /// <summary>
    /// Verifies that a misindented statement target led by a one-line comment that holds a line break other than the
    /// document's line ending keeps its first token in its column, and the wrapped argument aligns under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task MisindentedStatementTargetHoldingAForeignLineBreakKeepsItsFirstTokenAndAlignsTheWrappedArgument()
    {
        const string input = "class C\n{\n    void M()\n    {\n     /* c */ Call(1,¶2);\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n     /* c */ Call(1,\n                  2);\n    }\n}";

        await AssertFormatsTargetWithForeignLineBreaks(input, expected, SelectSingle<ExpressionStatementSyntax>, null);
    }

    /// <summary>
    /// Verifies that a chain target formatted with an equals-value clause as context, whose wrapped link follows a line break
    /// other than the document's line ending, aligns the wrapped link under the first link behind the last line of a
    /// multi-line comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task ChainTargetWithContextHoldingAForeignLineBreakAlignsTheWrappedLink()
    {
        const string input = "class C\n{\n    void M(string s)\n    {\n        var x = /* a\n                   b */ s.Trim()¶.ToUpper();\n    }\n}";
        const string expected = "class C\n{\n    void M(string s)\n    {\n        var x = /* a\n                   b */ s.Trim()\n                         .ToUpper();\n    }\n}";

        await AssertFormatsTargetWithForeignLineBreaks(input,
                                                       expected,
                                                       root => root.DescendantNodes().OfType<InvocationExpressionSyntax>().First(),
                                                       SelectSingle<EqualsValueClauseSyntax>);
    }

    /// <summary>
    /// Verifies that a block target whose closing brace starts its line behind a documentation comment written behind the
    /// block's last statement places that brace in the column of its method, as document-level formatting does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public async Task BlockTargetPlacesClosingBraceBehindStrayDocumentationComment()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var x = 1; /// x\n            }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var x = 1; /// x\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSingle<BlockSyntax>);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> on a detached type whose closing brace starts its line behind
    /// a documentation comment written behind the last field places that brace in the column of its type, as
    /// document-level formatting does
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void DetachedTypePlacesClosingBraceBehindStrayDocumentationComment()
    {
        const string input = "class D\n{\n    int A; /// <summary>end</summary>\n          }";
        const string expected = "class D\n{\n    int A; /// <summary>end</summary>\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var detachedRoot = SyntaxFactory.ParseCompilationUnit(NormalizeLineEndings(input, endOfLine));

            var firstPass = ReihitsuFormatter.FormatNode(detachedRoot, cancellationToken: TestContext.CancellationToken).Output.ToFullString();

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), firstPass, $"Unexpected first pass under {DescribeLineEnding(endOfLine)} line endings.");

            var secondPass = ReihitsuFormatter.FormatNode(SyntaxFactory.ParseCompilationUnit(firstPass), cancellationToken: TestContext.CancellationToken).Output.ToFullString();

            Assert.AreEqual(firstPass, secondPass, $"The second pass must not change the output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the interpolated raw string literal itself, as target with the local declaration as context, is found and
    /// formatted although the indentation phase moves its lines: its first token stays on the unmoved opening line, and its
    /// lines take the phases' columns under the opening quotes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-022")]
    public async Task RawStringLiteralTargetWhoseLinesMoveIsFormattedUnderItsOpeningQuotes()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var s = $\"\"\"\n            a {Call(1,\n            2)} b\n            \"\"\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var s = $\"\"\"\n                 a {Call(1,\n                         2)} b\n                 \"\"\";\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectSingle<InterpolatedStringExpressionSyntax>,
                                             SelectSingle<LocalDeclarationStatementSyntax>);
    }

    /// <summary>
    /// Verifies that with the literal already under its opening quotes, the phases change nothing in front of the invocation
    /// target on its line, and <c>2</c> is placed under <c>1</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task InvocationTargetInsideAnAlignedRawStringLiteralPlacesItsSecondLineUnderTheFirstArgument()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var s = $\"\"\"\n                 a {Call(1,\n            2)} b\n                 \"\"\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var s = $\"\"\"\n                 a {Call(1,\n                         2)} b\n                 \"\"\";\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectSingle<InvocationExpressionSyntax>,
                                             SelectSingle<LocalDeclarationStatementSyntax>);
    }

    /// <summary>
    /// Verifies the first way of FMT-PIPE-017: a declarator target that starts a later line of its declaration, which the
    /// context's layout puts in another column, keeps its column, and its wrapped argument stands under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task DeclaratorTargetOnALaterLineOfItsContextKeepsItsColumnAndAlignsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string input = "class C\n{\n    static int Other(int x, int y)\n    {\n        return x + y;\n    }\n\n    void M()\n    {\n        int a = 1,\n              b =\n                  Other(1,\n                        2);\n    }\n}";
        const string expected = "class C\n{\n    static int Other(int x, int y)\n    {\n        return x + y;\n    }\n\n    void M()\n    {\n        int a = 1,\n              b = Other(1,\n                        2);\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single(declarator => declarator.Identifier.Text == "b"),
                                             SelectSingle<LocalDeclarationStatementSyntax>);
    }

    /// <summary>
    /// Verifies that a declarator target that starts a later line of a tab-indented declaration keeps the tabs in front of
    /// it, and its wrapped argument stands under the first argument with each of those tabs counted as one column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-PIPE-018")]
    public async Task DeclaratorTargetOnALaterLineOfATabIndentedContextAlignsTheWrappedArgumentWithEachTabInFrontOfItAsOneColumn()
    {
        const string input = "class C\n{\n\tvoid M()\n\t{\n\t\tint a = 1,\n\t\t    b =\n\t\t        Other(1,\n\t\t              2);\n\t}\n}";
        const string expected = "class C\n{\n\tvoid M()\n\t{\n\t\tint a = 1,\n\t\t    b = Other(1,\n                2);\n\t}\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Last(),
                                             SelectSingle<LocalDeclarationStatementSyntax>);
    }

    /// <summary>
    /// Verifies the first way of FMT-PIPE-017 with a join: the first parameter target on the line after the opening
    /// parenthesis, which the context's layout joins to the parenthesis, stays on its line in its column, and its wrapped
    /// binary operator stands under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-PIPE-024")]
    public async Task FirstParameterTargetOnTheLineAfterTheOpeningParenthesisIsNotJoinedAndAlignsTheWrappedOperatorUnderTheValue()
    {
        const string input = "class C\n{\n    void M(\n        int a\n            = 1 +\n              2)\n    {\n    }\n}";
        const string expected = "class C\n{\n    void M(\n        int a = 1\n                + 2)\n    {\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectSingle<ParameterSyntax>,
                                             SelectSingle<ParameterListSyntax>);
    }

    /// <summary>
    /// Verifies the second way of FMT-PIPE-017 with a line break: a parameter target behind another parameter on its line,
    /// which the context's layout puts on a line of its own, stays on that line in its column, and its wrapped binary
    /// operator stands under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-PIPE-024")]
    public async Task ParameterTargetBehindAnotherParameterIsNotBrokenOffItsLineAndAlignsTheWrappedOperatorUnderTheValue()
    {
        const string input = "class C\n{\n    void M(int a,\n           int b, string c\n               = \"x\" +\n                 \"y\")\n    {\n    }\n}";
        const string expected = "class C\n{\n    void M(int a,\n           int b, string c = \"x\"\n                             + \"y\")\n    {\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<ParameterSyntax>().Last(),
                                             SelectSingle<ParameterListSyntax>);
    }

    /// <summary>
    /// Verifies the second way of FMT-PIPE-017 with a line break: an enum member target behind the opening brace, which the
    /// context's layout puts on a line of its own, stays on that line in its column, and its wrapped binary operator stands
    /// under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("FMT-PIPE-024")]
    public async Task EnumMemberTargetBehindTheOpeningBraceIsNotBrokenOffItsLineAndAlignsTheWrappedOperatorUnderTheValue()
    {
        const string input = "enum E { A =\n           1 +\n           2 }";
        const string expected = "enum E { A = 1\n             + 2 }";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectSingle<EnumMemberDeclarationSyntax>,
                                             SelectSingle<EnumDeclarationSyntax>);
    }

    /// <summary>
    /// Verifies the second way of FMT-PIPE-017 with spacing: a declarator target behind its type and several spaces, which
    /// the context's layout reduces to one, keeps them, and its wrapped argument stands under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task DeclaratorTargetBehindItsTypeAndSeveralSpacesKeepsThemAndAlignsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string input = "class C\n{\n    static int Other(int x, int y)\n    {\n        return x + y;\n    }\n\n    void M()\n    {\n        int    a =\n            Other(1,\n                  2);\n    }\n}";
        const string expected = "class C\n{\n    static int Other(int x, int y)\n    {\n        return x + y;\n    }\n\n    void M()\n    {\n        int    a = Other(1,\n                         2);\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectSingle<VariableDeclaratorSyntax>,
                                             SelectSingle<LocalDeclarationStatementSyntax>);
    }

    /// <summary>
    /// Verifies the second way of FMT-PIPE-017 with spacing: a local function target behind a label and several spaces,
    /// with the labeled statement as context, keeps those spaces, its wrapped parameter stands under the first parameter,
    /// and its block, which the depth places, starts in the column of the label's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task LocalFunctionTargetBehindALabelAndSeveralSpacesKeepsThemAndAlignsTheWrappedParameterUnderTheFirstParameter()
    {
        const string input = "class C\n{\n    void M()\n    {\n        L:    int F(int a,\n                 int b)\n        {\n            return a + b;\n        }\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        L:    int F(int a,\n                    int b)\n        {\n            return a + b;\n        }\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             SelectSingle<LocalFunctionStatementSyntax>,
                                             SelectSingle<LabeledStatementSyntax>);
    }

    /// <summary>
    /// Verifies the third way of FMT-PIPE-017: a declarator target behind the closing line of a raw string literal whose
    /// lines the indentation phase moves keeps the literal's lines and its own column as in the document, and its wrapped
    /// binary operator stands under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task DeclaratorTargetBehindTheClosingLineOfAMovingRawStringKeepsItsColumnAndAlignsTheWrappedOperatorUnderTheValue()
    {
        const string input = "class C\n{\n    void M()\n    {\n        string a = \"\"\"\n            x\n            \"\"\", b =\n            \"y\" +\n            \"z\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        string a = \"\"\"\n            x\n            \"\"\", b = \"y\"\n                     + \"z\";\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single(declarator => declarator.Identifier.Text == "b"),
                                             SelectSingle<LocalDeclarationStatementSyntax>);
    }

    /// <summary>
    /// Verifies the context's layout for a chain target that is the expression of its context statement, as the RH5201 and
    /// RH5112 fixes pass them, when the statement stands two columns deeper than its depth: the target starts the context's
    /// first line, so the anonymous method's block, which the depth places, is shifted by the context's offset and keeps its
    /// place relative to the statement's first token, and the wrapped link aligns under the first link
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task ChainTargetStartingItsStatementOffItsDepthShiftsTheBlockByTheContextsOffset()
    {
        const string input = "class C\n{\n    void M(B b)\n    {\n          b.A(delegate\n              {\n                  M(b);\n              })\n      .C() ;\n    }\n}";
        const string expected = "class C\n{\n    void M(B b)\n    {\n          b.A(delegate\n          {\n              M(b);\n          })\n           .C() ;\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<InvocationExpressionSyntax>().First(),
                                             root => root.DescendantNodes().OfType<ExpressionStatementSyntax>().First());
    }

    /// <summary>
    /// Verifies the context's layout for a chain target that is the expression of its context statement, as the RH5201 and
    /// RH5112 fixes pass them, in a file indented by two spaces: the anonymous method's block, which the depth places, is
    /// shifted by the context's offset and keeps its place relative to the statement's first token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task ChainTargetStartingItsStatementInTwoSpaceIndentedFileShiftsTheBlockByTheContextsOffset()
    {
        const string input = "class C\n{\n  void M(B b)\n  {\n    b.A(delegate\n        {\n          M(b);\n        })\n      .C() ;\n  }\n}";
        const string expected = "class C\n{\n  void M(B b)\n  {\n    b.A(delegate\n    {\n        M(b);\n    })\n     .C() ;\n  }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<InvocationExpressionSyntax>().First(),
                                             root => root.DescendantNodes().OfType<ExpressionStatementSyntax>().First());
    }

    /// <summary>
    /// Verifies the other side of the statement's indentation: for a chain target that is the expression of its context
    /// statement at its depth, the context's offset is zero and the anonymous method's block stands where the depth places it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    public async Task ChainTargetStartingItsStatementAtItsDepthPlacesTheBlockByTheDepth()
    {
        const string input = "class C\n{\n    void M(B b)\n    {\n        b.A(delegate\n            {\n                M(b);\n            })\n    .C() ;\n    }\n}";
        const string expected = "class C\n{\n    void M(B b)\n    {\n        b.A(delegate\n        {\n            M(b);\n        })\n         .C() ;\n    }\n}";

        await AssertFormatsTargetWithContext(input,
                                             expected,
                                             root => root.DescendantNodes().OfType<InvocationExpressionSyntax>().First(),
                                             root => root.DescendantNodes().OfType<ExpressionStatementSyntax>().First());
    }

    /// <summary>
    /// Selects the only node of the given type in the root
    /// </summary>
    /// <typeparam name="TNode">Type of the node</typeparam>
    /// <param name="root">Syntax root</param>
    /// <returns>The node</returns>
    private static SyntaxNode SelectSingle<TNode>(SyntaxNode root)
        where TNode : SyntaxNode
    {
        return root.DescendantNodes().OfType<TNode>().Single();
    }

    /// <summary>
    /// Selects the enum member <c>B</c> in the root
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The enum member</returns>
    private static SyntaxNode SelectEnumMemberB(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().Single(member => member.Identifier.Text == "B");
    }

    /// <summary>
    /// Formats the selected target through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> under LF and CRLF and
    /// asserts the document's text after one pass, and that a second pass leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTarget(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectTarget)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var expected = NormalizeLineEndings(expectedWithLf, endOfLine);
            var firstPass = await Format(NormalizeLineEndings(inputWithLf, endOfLine), selectTarget, null);

            Assert.AreEqual(expected, firstPass, $"Unexpected first pass under {DescribeLineEnding(endOfLine)} line endings.");

            var secondPass = await Format(firstPass, selectTarget, null);

            Assert.AreEqual(expected, secondPass, $"The second pass must not change the output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Formats the selected target through <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> under LF and
    /// CRLF and asserts the document's text after one pass, and that a second pass leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <param name="selectContext">Selects the context node in the document's root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTargetWithContext(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectTarget, Func<SyntaxNode, SyntaxNode> selectContext)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var expected = NormalizeLineEndings(expectedWithLf, endOfLine);
            var firstPass = await Format(NormalizeLineEndings(inputWithLf, endOfLine), selectTarget, selectContext);

            Assert.AreEqual(expected, firstPass, $"Unexpected first pass under {DescribeLineEnding(endOfLine)} line endings.");

            var secondPass = await Format(firstPass, selectTarget, selectContext);

            Assert.AreEqual(expected, secondPass, $"The second pass must not change the output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Formats the selected target under LF and CRLF, with every <c>¶</c> of the input replaced by a line break other than the
    /// document's line ending, and asserts the document's text after one pass, and that a second pass leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings, where <c>¶</c> marks the foreign line break</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <param name="selectContext">Selects the context node, or <see langword="null"/> to format without a context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTargetWithForeignLineBreaks(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectTarget, Func<SyntaxNode, SyntaxNode> selectContext)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var expected = NormalizeLineEndings(expectedWithLf, endOfLine);
            var foreignLineBreaks = endOfLine == "\n" ? new[] { "\r\n", "\r", "\u2028" } : new[] { "\n", "\r", "\u2029" };

            foreach (var foreignLineBreak in foreignLineBreaks)
            {
                var description = $"{DescribeLineEnding(endOfLine)} line endings with {string.Join(" ", foreignLineBreak.Select(character => $"U+{(int)character:X4}"))} as the foreign line break";
                var firstPass = await Format(NormalizeLineEndings(inputWithLf, endOfLine).Replace("¶", foreignLineBreak), selectTarget, selectContext);

                Assert.AreEqual(expected, firstPass, $"Unexpected first pass under {description}.");

                var secondPass = await Format(firstPass, selectTarget, selectContext);

                Assert.AreEqual(expected, secondPass, $"The second pass must not change the output under {description}.");
            }
        }
    }

    /// <summary>
    /// Formats the selected target of the given text through a node-level entry point
    /// </summary>
    /// <param name="text">Document text</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <param name="selectContext">Selects the context node, or <see langword="null"/> to format without a context</param>
    /// <returns>The document's text after formatting</returns>
    private async Task<string> Format(string text, Func<SyntaxNode, SyntaxNode> selectTarget, Func<SyntaxNode, SyntaxNode> selectContext)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(text));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

            if (root == null)
            {
                Assert.Fail("Expected a syntax root.");
            }

            var target = selectTarget(root);
            var result = selectContext == null
                             ? (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken)).Output
                             : (await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, target, selectContext(root), TestContext.CancellationToken)).Output;

            return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
        }
    }

    #endregion // Methods
}