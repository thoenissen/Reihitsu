using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer"/> and <see cref="RH6004PreprocessorKeywordsMustNotBePrecededBySpaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzerTests : BatchCodeFixTestsBase<RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer, RH6004PreprocessorKeywordsMustNotBePrecededBySpaceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                #pragma warning disable CS0168
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6004-001")]
    [Requirement("RH6004-003")]
    [Requirement("RH6004-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                {|#0:    |}#pragma warning disable CS0168
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 #pragma warning disable CS0168
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6004MessageFormat));
    }

    /// <summary>
    /// Verifies that region directives do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6004-002")]
    public async Task VerifyRegionsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    void Method()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that raw-string content does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6004-002")]
    public async Task VerifyRawStringContentDoesNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    private const string Value = """
                                                                 #pragma warning disable CS0168
                                                                 #endregion
                                                                 """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that hash-prefixed content in a multi-line comment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6004-002")]
    public async Task VerifyMultiLineCommentContentDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /* Notes:
                                       # Heading
                                    */
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a directive in an inactive preprocessor branch does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6004-002")]
    public async Task VerifyInactiveDirectiveDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                #if false
                                    #pragma warning disable CS0168
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that indented conditional directives inside a method, including the active <c>#else</c> after an
    /// inactive branch, and a directive indented with a tab are each reported once and moved to column 0
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6004-001")]
    [Requirement("RH6004-003")]
    [Requirement("RH6004-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIndentedConditionalDirectivesAndTabIndentationAreDetectedAndFixed()
    {
        const string testData = "internal class TestClass\n"
                                + "{\n"
                                + "    void Method(int x)\n"
                                + "    {\n"
                                + "{|#0:        |}#if FEATURE\n"
                                + "        x++;\n"
                                + "{|#1:        |}#else\n"
                                + "        x--;\n"
                                + "{|#2:        |}#endif\n"
                                + "{|#3:\t|}#pragma warning disable CS0168\n"
                                + "    }\n"
                                + "}";
        const string fixedData = "internal class TestClass\n"
                                 + "{\n"
                                 + "    void Method(int x)\n"
                                 + "    {\n"
                                 + "#if FEATURE\n"
                                 + "        x++;\n"
                                 + "#else\n"
                                 + "        x--;\n"
                                 + "#endif\n"
                                 + "#pragma warning disable CS0168\n"
                                 + "    }\n"
                                 + "}";

        await Verify(testData, fixedData, Diagnostics(RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6004MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that an indented <c>#define</c> directive is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6004-001")]
    [Requirement("RH6004-004")]
    public async Task VerifyIndentedDefineDirectiveIsDetectedAndFixed()
    {
        const string testData = """
                                {|#0:  |}#define FEATURE
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 #define FEATURE
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6004MessageFormat));
    }

    /// <summary>
    /// Verifies that directive-like text inside a verbatim string literal and nested directives inside an inactive
    /// branch are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6004-002")]
    public async Task VerifyVerbatimStringContentAndNestedInactiveDirectivesDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string _value = @"
                                        #if FEATURE";

                                #if NEVER
                                    #if NESTED
                                    #endif
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                {|#0:    |}#pragma warning disable CS0168
                                {|#1:    |}#pragma warning restore CS0168
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 #pragma warning disable CS0168
                                 #pragma warning restore CS0168
                                 internal class TestClass
                                 {
                                 }
                                 """;

        // Two indented preprocessor directives sit on adjacent lines; each fix only removes its own line's
        // leading whitespace run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6004MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}