using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests that the formatter never writes a shape it refuses as unsupported: inputs with comments next to variable declarations
/// that the formatter formats are formatted, and their output, formatted again, is neither refused nor changed
/// </summary>
[TestClass]
public class UnsupportedSyntaxClosureTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: the field split keeps a comment inside an initializer value inside that value, where
    /// no requirement refuses it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    public void SpecExampleFieldSplitKeepsACommentInsideAValueInsideThatValue()
    {
        const string input = """
                             class C
                             {
                                 private int _a = 1 + /* c */ 2, _b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a = 1 + /* c */ 2;
                                    private int _b;
                                }
                                """;

        AssertFormattedOutputIsNotUnsupported(input);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the output of an input whose comments stand in the declarations' excepted places is not refused when it is
    /// formatted again
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [DataRow("class C\n{\n    private int _r = Compute(1) /* r */ + 2, _s;\n\n    static int Compute(int v) => v;\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        int a = 1; /* c */ int b = 2;\n    }\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        int a = 1; // c\n        int b = 2;\n    }\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        int a =\n            Compute(1, // c\n                    2);\n    }\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        System.Func<int> f = () =>\n        {\n            // c\n            int x = 1;\n            return x;\n        };\n    }\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        using (var s = Open()) // c\n        {\n        }\n\n        for (int i = 0; /* c */ i < 1; i++)\n        {\n        }\n    }\n}")]
    [DataRow("class C\n{\n    private int _p; /** The second field. */ private int _q;\n}")]
    [DataRow("class C\n{\n    // c\n    [System.Obsolete]\n    private int _a = 1, _b; // d\n}")]
    [DataRow("class C{void M(){int a=1;// c\nint b=2;}}")]
    public void OutputOfAFormattedInputIsNotUnsupported(string input)
    {
        AssertFormattedOutputIsNotUnsupported(input);
    }

    /// <summary>
    /// Formats the input with <see cref="ReihitsuFormatter.FormatSyntaxTree"/> under LF and CRLF line endings, verifies that it is
    /// not refused, then formats its output again and verifies that the output is neither refused nor changed
    /// </summary>
    /// <param name="input">The input source text</param>
    private void AssertFormattedOutputIsNotUnsupported(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var first = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(first.Unsupported, $"The input must be formatted under {endingName} line endings.");

            var output = first.Output.ToString();
            var second = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(output, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(second.Unsupported, $"The formatted output must not be refused under {endingName} line endings:{Environment.NewLine}{output}");
            Assert.AreEqual(output, second.Output.ToString(), $"The formatted output must be stable under {endingName} line endings.");
        }
    }

    #endregion // Methods
}