using System;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6007OpeningSquareBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer, RH6007OpeningSquareBracketsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6007-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = [0];
                                        _ = values[0];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6007-001")]
    [Requirement("RH6007-002")]
    [Requirement("RH6007-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = [0];
                                        _ = values[{|#0: |}0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] values = [0];
                                         _ = values[0];
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6007MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace after the opening bracket of an array type, a rank specifier, an array creation, a
    /// collection expression, and a list pattern, and at the end of the line, is reported once per bracket and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6007-001")]
    [Requirement("RH6007-002")]
    [Requirement("RH6007-003")]
    public async Task VerifyWhitespaceAfterOtherOpeningSquareBracketsIsDetectedAndFixed()
    {
        const string testData = "internal class TestClass\n"
                                + "{\n"
                                + "    void Method(int[] arr)\n"
                                + "    {\n"
                                + "        int[{|#0: |}] e = [{|#1:\t|}1, 2];\n"
                                + "        int[{|#2: |},] g = null;\n"
                                + "        var y = new int[{|#3: |}2];\n"
                                + "        var p = arr is [{|#4: |}1, ..];\n"
                                + "        var v = arr[{|#5:  |}\n"
                                + "            0];\n"
                                + "    }\n"
                                + "}";
        const string fixedData = "internal class TestClass\n"
                                 + "{\n"
                                 + "    void Method(int[] arr)\n"
                                 + "    {\n"
                                 + "        int[] e = [1, 2];\n"
                                 + "        int[,] g = null;\n"
                                 + "        var y = new int[2];\n"
                                 + "        var p = arr is [1, ..];\n"
                                 + "        var v = arr[\n"
                                 + "            0];\n"
                                 + "    }\n"
                                 + "}";

        await Verify(testData, fixedData, Diagnostics(RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6007MessageFormat, 6));
    }

    /// <summary>
    /// Verifies that the opening bracket of an attribute list and an opening bracket directly followed by a line
    /// break are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6007-001")]
    public async Task VerifyAttributeListAndLineBreakAfterOpeningBracketAreNotReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [ Obsolete]
                                    void Method(int[] arr)
                                    {
                                        var u = arr[
                                            0];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment on a later line of the token gap withholds the fix while the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6007-001")]
    [Requirement("RH6007-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenACommentIsOnALaterLineOfTheGap()
    {
        const string testData = "internal class TestClass\n"
                                + "{\n"
                                + "    void Method(int[] arr)\n"
                                + "    {\n"
                                + "        var v = arr[{|#0:  |}\n"
                                + "            // first index\n"
                                + "            0];\n"
                                + "    }\n"
                                + "}";

        await Verify(testData, Diagnostics(RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6007MessageFormat));

        var spanStart = testData.IndexOf("{|#0:", StringComparison.Ordinal);
        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:  |}", "  "),
                                                   RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root => Location.Create(root.SyntaxTree, new TextSpan(spanStart, 2)));

        Assert.IsEmpty(actions, "The comment sits in the token gap of the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a directive in the token gap withholds the fix while the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6007-001")]
    [Requirement("RH6007-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenADirectiveIsInTheGap()
    {
        const string testData = "internal class TestClass\n"
                                + "{\n"
                                + "    void Method(int[] arr)\n"
                                + "    {\n"
                                + "        var v = arr[{|#0:  |}\n"
                                + "#if FEATURE\n"
                                + "#endif\n"
                                + "            0];\n"
                                + "    }\n"
                                + "}";

        await Verify(testData, Diagnostics(RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6007MessageFormat));

        var spanStart = testData.IndexOf("{|#0:", StringComparison.Ordinal);
        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:  |}", "  "),
                                                   RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root => Location.Create(root.SyntaxTree, new TextSpan(spanStart, 2)));

        Assert.IsEmpty(actions, "The directive sits in the token gap of the reported whitespace run; the fix must be withheld.");
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] first = [0];
                                        int[] second = [0];
                                        _ = first[{|#0: |}0];
                                        _ = second[{|#1: |}0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] first = [0];
                                         int[] second = [0];
                                         _ = first[0];
                                         _ = second[0];
                                     }
                                 }
                                 """;

        // Two element-access brackets sit on adjacent lines with no blank line between them; each fix only
        // removes its own trailing whitespace run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6007MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}