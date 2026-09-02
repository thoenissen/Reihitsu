using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer"/> and <see cref="RH6015NullableTypeSymbolsMustNotBePrecededBySpaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzerTests : BatchCodeFixTestsBase<RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer, RH6015NullableTypeSymbolsMustNotBePrecededBySpaceCodeFixProvider>
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
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
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
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int{|#0: |}? value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int? value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab before a nullable type symbol is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabBeforeNullableTypeSymbolIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int{|#0:	|}? value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int? value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix preserves a comment before a nullable type symbol
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeNullableTypeSymbolIsPreservedByCodeFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int /* Keep. */{|#0: |}? value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int /* Keep. */? value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat));
    }

    /// <summary>
    /// Verifies that a nullable type symbol on a continuation line does not produce a diagnostic with LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineNullableTypeSymbolDoesNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                        ? value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nullable type symbol on a continuation line does not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineNullableTypeSymbolDoesNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                        ? value)
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line nullable type symbol do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineNullableTypeSymbolDoNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                #if false
                                    disabled text
                                #endif
                                        ? value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line nullable type symbol do not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineNullableTypeSymbolDoNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                #if false
                                    disabled text
                                #endif
                                        ? value)
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int{|#0: |}? first, long{|#1:	|}? second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int? first, long? second)
                                     {
                                     }
                                 }
                                 """;

        // Verifies that Fix All removes multiple same-line whitespace runs in one iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}