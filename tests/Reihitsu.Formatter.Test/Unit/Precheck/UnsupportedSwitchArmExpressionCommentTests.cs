using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the unsupported shape of a comment, a directive, or disabled text between the <c>=&gt;</c> of a switch-expression arm
/// and the arm's expression (FMT-UNSUP-021): every kind of trivia on one line and across lines, documentation comments in every
/// documentation mode, every arm wherever it stands, the arrows of other shapes, the gaps next to the arrow, node-level inputs, and
/// the closure of the formatter's output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedSwitchArmExpressionCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: each arm is refused, the input is returned unchanged, and the first comment, which
    /// starts in line 7, column 25, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    public void SpecExampleEachArmIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 int M(int v)
                                 {
                                     return v switch
                                            {
                                                1 => // one
                                                    2,
                                                2 => /* two */ 3,
                                                _ => 0
                                            };
                                 }
                             }
                             """;

        AssertUnsupportedFor(SwitchArmExpressionCommentReason, input, "// one");

        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(new LinePosition(6, 24), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
        }
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment behind the arm's expression on either side of its comma,
    /// one behind the arrow of a lambda with a block body, and one behind a <c>case</c> label do not refuse the input, and the line
    /// break alone behind <c>1 =&gt;</c> is formatted; the input is a fixed point of the formatter
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    public void SpecExampleCommentsOutsideTheShapeAreSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 int M(int v)
                                 {
                                     var r = v switch
                                             {
                                                 1 =>
                                                     2, // c1
                                                 2 => 3 /* c2 */,
                                                 _ => 0
                                             };
                                     Action a = () => // c3
                                                {
                                                };

                                     switch (v)
                                     {
                                         case 1: // c4
                                             break;
                                     }

                                     return r;
                                 }
                             }
                             """;

        AssertSupported(input);
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the change doc's example of a directive behind an arm's <c>=&gt;</c>: the input is returned unchanged, and the
    /// directive, which starts in line 8, column 1, is reported, in a build that defines <c>DEBUG</c> and in one that does not
    /// </summary>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    [DataRow(false, DisplayName = "DEBUG not defined")]
    [DataRow(true, DisplayName = "DEBUG defined")]
    public void DirectiveBehindTheArrowIsUnsupportedInEveryBuild(bool defineDebug)
    {
        const string input = """
                             class C
                             {
                                 int M(int v)
                                 {
                                     return v switch
                                            {
                                                1 =>
                             #if DEBUG
                                                    2,
                             #else
                                                    3,
                             #endif
                                                _ => 0
                                            };
                                 }
                             }
                             """;

        var parseOptions = defineDebug ? CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG") : CSharpParseOptions.Default;

        AssertUnsupportedFor(SwitchArmExpressionCommentReason, input, "#if DEBUG", parseOptions: parseOptions);

        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), parseOptions, cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(new LinePosition(7, 0), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
        }
    }

    /// <summary>
    /// Verifies that every kind of trivia behind the arrow refuses the input: a comment in the trailing trivia of the arrow and
    /// one in the leading trivia of the expression's first token, a documentation comment, and a directive
    /// </summary>
    /// <param name="arm">The arm in front of the default arm</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    [DataRow("1 => /* c */ 2,", "/* c */", DisplayName = "Delimited comment on one line")]
    [DataRow("1 => // c\n                        2,", "// c", DisplayName = "Line comment behind the arrow")]
    [DataRow("1 =>\n                        // c\n                        2,", "// c", DisplayName = "Line comment on its own line")]
    [DataRow("1 =>\n                        /* c */ 2,", "/* c */", DisplayName = "Delimited comment leading the expression's line")]
    [DataRow("1 => /** c */ 2,", "/** c */", DisplayName = "Delimited documentation comment")]
    [DataRow("1 =>\n#pragma warning disable CS0168\n                        2,", "#pragma warning disable CS0168", DisplayName = "Pragma")]
    [DataRow("int i when i > 0 => /* c */ 2,", "/* c */", DisplayName = "Arm with a when clause")]
    [DataRow("1 => /* c */ x = 2,", "/* c */", DisplayName = "Arm whose expression is an assignment")]
    public void EveryKindOfTriviaBehindTheArrowIsUnsupported(string arm, string reported)
    {
        AssertUnsupportedFor(SwitchArmExpressionCommentReason, WrapInSwitch(arm), reported);
    }

    /// <summary>
    /// Verifies that a documentation comment behind the arrow refuses the input in every documentation mode, in which it is a
    /// documentation comment or an ordinary comment
    /// </summary>
    /// <param name="arm">The arm in front of the default arm</param>
    /// <param name="comment">The comment that is reported</param>
    /// <param name="documentationMode">The documentation mode of the parse</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    [DataRow("1 => /** c */ 2,", "/** c */", DocumentationMode.Parse)]
    [DataRow("1 => /** c */ 2,", "/** c */", DocumentationMode.None)]
    [DataRow("1 => /** c */ 2,", "/** c */", DocumentationMode.Diagnose)]
    [DataRow("1 =>\n                        /// <summary>c</summary>\n                        2,", "/// <summary>c</summary>", DocumentationMode.Parse)]
    [DataRow("1 =>\n                        /// <summary>c</summary>\n                        2,", "/// <summary>c</summary>", DocumentationMode.None)]
    [DataRow("1 =>\n                        /// <summary>c</summary>\n                        2,", "/// <summary>c</summary>", DocumentationMode.Diagnose)]
    public void DocumentationCommentBehindTheArrowIsUnsupportedInEveryDocumentationMode(string arm, string comment, DocumentationMode documentationMode)
    {
        AssertUnsupportedFor(SwitchArmExpressionCommentReason, WrapInSwitch(arm), comment, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    /// <summary>
    /// Verifies that every arm is checked with its own arrow wherever it stands: the default arm, an arm of a nested switch
    /// expression, an arm inside a lambda, and an arm in a member's initializer
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    [DataRow("class C\n{\n    int M(int v)\n    {\n        return v switch\n               {\n                   1 => 2,\n                   _ => /* c */ 0\n               };\n    }\n}", "/* c */", DisplayName = "Default arm")]
    [DataRow("class C\n{\n    int M(int v)\n    {\n        return v switch\n               {\n                   1 => v switch\n                        {\n                            2 => /* c */ 3,\n                            _ => 4\n                        },\n                   _ => 0\n               };\n    }\n}", "/* c */", DisplayName = "Arm of a nested switch expression")]
    [DataRow("using System;\n\nclass C\n{\n    void M()\n    {\n        Func<int, int> f = v => v switch\n                                {\n                                    1 => /* c */ 2,\n                                    _ => 0\n                                };\n    }\n}", "/* c */", DisplayName = "Arm inside a lambda")]
    [DataRow("class C\n{\n    static int s_v;\n\n    int P { get; } = s_v switch\n                     {\n                         1 => /* c */ 2,\n                         _ => 0\n                     };\n}", "/* c */", DisplayName = "Arm in a property's initializer")]
    public void EveryArmIsUnsupportedWhereverItStands(string input, string reported)
    {
        AssertUnsupportedFor(SwitchArmExpressionCommentReason, input, reported);
    }

    /// <summary>
    /// Verifies the other side: a comment inside the arm's expression behind its first token, behind the expression, above the
    /// arm, and a line break alone behind the arrow do not refuse the input
    /// </summary>
    /// <param name="arm">The arm in front of the default arm</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    [DataRow("1 => M2(/* c */ 2),", DisplayName = "Inside the expression behind its first token")]
    [DataRow("1 => 2, // c", DisplayName = "Behind the comma")]
    [DataRow("1 => 2 /* c */,", DisplayName = "In front of the comma")]
    [DataRow("// c\n                    1 => 2,", DisplayName = "Above the arm")]
    [DataRow("1 =>\n                        2,", DisplayName = "Line break alone")]
    public void CommentOutsideTheGapIsSupported(string arm)
    {
        AssertSupported(WrapInSwitch(arm));
    }

    /// <summary>
    /// Verifies that the arrows of other shapes keep their reason: a comment in front of an arm's arrow (FMT-UNSUP-017), behind a
    /// lambda's arrow in front of an expression body (FMT-UNSUP-017), behind a member's expression-body arrow (FMT-UNSUP-012), and
    /// behind an assignment operator in an arm's expression (FMT-UNSUP-020)
    /// </summary>
    /// <param name="reason">The reason that is given</param>
    /// <param name="input">The source text</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    [DataRow(SwitchArmArrowCommentReason, "class C\n{\n    int M(int v)\n    {\n        return v switch\n               {\n                   1 /* c */ => 2,\n                   _ => 0\n               };\n    }\n}", "/* c */", DisplayName = "In front of an arm's arrow")]
    [DataRow(LambdaArrowCommentReason, "using System;\n\nclass C\n{\n    void M()\n    {\n        Func<int> f = () => /* c */ 1;\n    }\n}", "/* c */", DisplayName = "Behind a lambda's arrow")]
    [DataRow(ExpressionBodyArrowCommentReason, "class C\n{\n    int P => /* c */ 1;\n}", "/* c */", DisplayName = "Behind a member's expression-body arrow")]
    [DataRow(AssignmentCommentReason, "class C\n{\n    int M(int v, int x)\n    {\n        return v switch\n               {\n                   1 => x = /* c */ 2,\n                   _ => 0\n               };\n    }\n}", "/* c */", DisplayName = "Behind an assignment operator in the arm")]
    public void ArrowsOfOtherShapesKeepTheirReason(string reason, string input, string reported)
    {
        AssertUnsupportedFor(reason, input, reported);
    }

    /// <summary>
    /// Verifies that an arm inside disabled text is not checked: where <c>X</c> is not defined, the arm is disabled text and the
    /// input is formatted, and where it is defined, the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void ArmInDisabledTextIsNotChecked(bool defineX)
    {
        var input = "class C\n{\n    int M(int v)\n    {\n        return v switch\n               {\n#if X\n                   1 => /* c */ 2,\n#endif\n                   _ => 0\n               };\n    }\n}";
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;

        if (defineX)
        {
            AssertUnsupportedFor(SwitchArmExpressionCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that a node-level input that holds the arrow holds the shape — the arm and the switch expression —, while the arm's
    /// expression alone does not, also with the comment in the leading trivia of its first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-021")]
    public void NodeThatHoldsTheArrowHoldsTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(WrapInSwitch("1 =>\n                        // c\n                        2,"), endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var arm = root.DescendantNodes().OfType<SwitchExpressionArmSyntax>().First();

            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(arm.Expression));
            Assert.IsNull(ReihitsuFormatter.FormatNode(arm.Expression, cancellationToken: TestContext.CancellationToken).Unsupported);
            Assert.AreEqual(SwitchArmExpressionCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(arm)?.Reason);
            Assert.AreEqual(SwitchArmExpressionCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(arm.Parent)?.Reason);
            Assert.AreEqual(SwitchArmExpressionCommentReason, ReihitsuFormatter.FormatNode(arm.Parent, cancellationToken: TestContext.CancellationToken).Unsupported?.Reason);
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds: an expression built
    /// with a comment behind an arm's arrow is refused, and the same expression without the comment is not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-021")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode()
    {
        var node = SyntaxFactory.ParseExpression("v switch { 1 => /* c */ 2, _ => 0 }");

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(node);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(SwitchArmExpressionCommentReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, node.ToFullString(), "/* c */", 0, "LF");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(SyntaxFactory.ParseExpression("v switch { 1 => 2, _ => 0 }")));
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: the layout of a switch expression with a line break behind an arrow and
    /// a comment behind the arm, and a documentation comment behind an arm's expression, carry no comment into the gap behind the
    /// arrow, and the output, formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="arm">The arm in front of the default arm</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-021")]
    [DataRow("1 =>\n\n                        2, // c", DisplayName = "Line break and blank line behind the arrow")]
    [DataRow("1 => 2 /** e */,", DisplayName = "Documentation comment behind the expression")]
    public void OutputOfASwitchExpressionIsNotUnsupported(string arm)
    {
        var input = WrapInSwitch(arm);

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

    /// <summary>
    /// Wraps an arm in a switch expression in front of its default arm, in a method of a class with the parameters and helpers
    /// the arm uses
    /// </summary>
    /// <param name="arm">The arm, without the indentation of its first line</param>
    /// <returns>The source text</returns>
    private static string WrapInSwitch(string arm)
    {
        return $"class C\n{{\n    int M(int v, int x)\n    {{\n        return v switch\n               {{\n                   {arm}\n                   _ => 0\n               }};\n    }}\n\n    int M2(int value)\n    {{\n        return value;\n    }}\n}}";
    }

    #endregion // Methods
}