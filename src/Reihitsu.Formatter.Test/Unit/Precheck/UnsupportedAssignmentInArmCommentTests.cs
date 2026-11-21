using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the unsupported shape of a comment, a directive, or disabled text behind an assignment operator inside a
/// switch-expression arm or an anonymous-object member declarator (FMT-UNSUP-019): every assignment operator, on one line and
/// across lines, the places inside an arm or a member, the assignments outside them, the gaps in front of the operator and
/// inside the value, the <c>=</c> that belongs to no assignment expression, and node-level inputs, each under LF and CRLF line
/// endings
/// </summary>
[TestClass]
public class UnsupportedAssignmentInArmCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: each assignment is refused, the input is returned unchanged, and the first comment,
    /// which starts in line 7, column 30, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-019")]
    public void SpecExampleEachAssignmentIsUnsupported()
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

        AssertUnsupportedFor(AssignmentInArmCommentReason, input, "// c1");

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(new LinePosition(6, 29), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment behind an assignment operator in a statement and in a
    /// collection element, a comment in front of the operator, and one inside the value of an anonymous-object member do not
    /// refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-019")]
    public void SpecExampleCommentsOutsideTheShapeAreSupported()
    {
        const string input = """
                             class C
                             {
                                 object M(int x, int y)
                                 {
                                     x += // c1
                                         3;
                                     int[] z = [
                                                   y -= // c2
                                                       2
                                               ];
                                     var w = new
                                             {
                                                 A = x /* c3 */ += 3,
                                                 B = x += M2(/* c4 */ 1)
                                             };

                                     return z.Length + w.A;
                                 }

                                 int M2(int v)
                                 {
                                     return v;
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that every assignment operator inside an arm or a member refuses the input, on one line and across lines, with
    /// comments, documentation comments, and directives, in the arm's expression, its <c>when</c> clause, and nested in an
    /// argument, a collection expression, or a lambda with an expression body inside it
    /// </summary>
    /// <param name="statement">The statement that holds the assignment</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-019")]
    [DataRow("        var w = new { A = x = /* c */ 3 };", "/* c */", DisplayName = "Simple assignment in a member, one line")]
    [DataRow("        var w = new { A = x += /* c */ 3 };", "/* c */", DisplayName = "Compound assignment in a member, one line")]
    [DataRow("        var w = new { A = o ??= // c\n            3 };", "// c", DisplayName = "Null-coalescing assignment in a member, line comment")]
    [DataRow("        var w = new { A = x <<= /** c */ 1 };", "/** c */", DisplayName = "Delimited documentation comment")]
    [DataRow("        var w = new { A = x |=\n#pragma warning disable CS0168\n            1 };", "#pragma warning disable CS0168", DisplayName = "Directive")]
    [DataRow("        var r = y switch\n                {\n                    1 => x = /* c */ 2,\n                    _ => 0,\n                };", "/* c */", DisplayName = "Arm expression")]
    [DataRow("        var r = y switch\n                {\n                    int i when (x += /* c */ i) > 0 => 1,\n                    _ => 0,\n                };", "/* c */", DisplayName = "When clause of an arm")]
    [DataRow("        var r = y switch\n                {\n                    1 => M2(x += /* c */ 2),\n                    _ => 0,\n                };", "/* c */", DisplayName = "Argument inside an arm")]
    [DataRow("        var w = new { A = new[] { x -= /* c */ 1 } };", "/* c */", DisplayName = "Array initializer inside a member")]
    [DataRow("        var w = new { A = (System.Func<int>)(() => x += /* c */ 1) };", "/* c */", DisplayName = "Lambda with an expression body inside a member")]
    public void EveryAssignmentInsideAnArmOrAMemberIsUnsupported(string statement, string reported)
    {
        AssertUnsupportedFor(AssignmentInArmCommentReason, WrapInMethod(statement), reported);
    }

    /// <summary>
    /// Verifies the other side: an assignment in a statement, also in a lambda's block inside an arm, an assignment outside an arm
    /// or a member, and a comment in front of the operator or inside the value behind its first token do not refuse the input
    /// </summary>
    /// <param name="statement">The statement that holds the assignment</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-019")]
    [DataRow("        x += // c\n            3;", DisplayName = "Statement")]
    [DataRow("        M2(x += // c\n            3);", DisplayName = "Argument outside an arm")]
    [DataRow("        int[] z = [y += // c\n            2];", DisplayName = "Collection expression outside an arm")]
    [DataRow("        var r = y switch\n                {\n                    1 => () =>\n                    {\n                        x += // c\n                            3;\n                    },\n                    _ => null,\n                };", DisplayName = "Statement in a lambda's block inside an arm")]
    [DataRow("        var w = new { A = x /* c */ += 3 };", DisplayName = "In front of the operator")]
    [DataRow("        var w = new { A = x += M2(/* c */ 1) };", DisplayName = "Inside the value behind its first token")]
    public void AssignmentOutsideTheShapeIsSupported(string statement)
    {
        AssertSupported(WrapInMethod(statement));
    }

    /// <summary>
    /// Verifies that the <c>=</c> of an anonymous-object member declarator is no assignment expression: a comment behind it keeps
    /// the reason of FMT-UNSUP-017
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    [Requirement("FMT-UNSUP-019")]
    public void EqualsOfTheMemberDeclaratorKeepsItsOwnReason()
    {
        AssertUnsupportedFor(AnonymousMemberEqualsCommentReason, WrapInMethod("        var w = new { A = /* c */ 1 };"), "/* c */");
    }

    /// <summary>
    /// Verifies that an assignment inside disabled text is not checked: where <c>X</c> is not defined, the arm is disabled text and
    /// the input is formatted, and where it is defined, the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-019")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void AssignmentInDisabledTextIsNotChecked(bool defineX)
    {
        var input = WrapInMethod("        var r = y switch\n                {\n#if X\n                    1 => x = /* c */ 2,\n#endif\n                    _ => 0,\n                };");
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;

        if (defineX)
        {
            AssertUnsupportedFor(AssignmentInArmCommentReason, input, "/* c */", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that a node-level input that holds the assignment but not the arm around it does not hold the shape, while the arm
    /// and the switch expression do
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-019")]
    public void AssignmentWithoutItsArmDoesNotHoldTheShape()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(WrapInMethod("        var r = y switch\n                {\n                    1 => x = /* c */ 2,\n                    _ => 0,\n                };"), endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var assignment = root.DescendantNodes().OfType<AssignmentExpressionSyntax>().Single();
            var arm = root.DescendantNodes().OfType<SwitchExpressionArmSyntax>().First();

            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(assignment));
            Assert.IsNull(ReihitsuFormatter.FormatNode(assignment, cancellationToken: TestContext.CancellationToken).Unsupported);
            Assert.AreEqual(AssignmentInArmCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(arm)?.Reason);
            Assert.AreEqual(AssignmentInArmCommentReason, ReihitsuFormatter.FindUnsupportedSyntax(arm.Parent)?.Reason);
        }
    }

    /// <summary>
    /// Verifies that the formatter never writes the shape: an assignment inside an arm and a member whose value stands on the next
    /// line is joined, and the output, formatted again, is neither refused nor changed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-019")]
    public void OutputOfAJoinedAssignmentInsideAnArmIsNotUnsupported()
    {
        AssertSupported(WrapInMethod("        var w = new { A = x +=\n            3 }; // c\n        var r = y switch\n                {\n                    1 => x =\n                        2,\n                    _ => 0,\n                };"));
    }

    /// <summary>
    /// Wraps statements in a method of a class, with the parameters and helpers they use
    /// </summary>
    /// <param name="statements">The statements, indented by eight spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInMethod(string statements)
    {
        return $"class C\n{{\n    object M(int x, int y, object o)\n    {{\n{statements}\n\n        return null;\n    }}\n\n    int M2(int v)\n    {{\n        return v;\n    }}\n}}";
    }

    #endregion // Methods
}