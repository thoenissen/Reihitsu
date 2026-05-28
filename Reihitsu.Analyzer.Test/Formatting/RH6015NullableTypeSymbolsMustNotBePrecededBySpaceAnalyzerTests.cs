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
public class RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzerTests : AnalyzerTestsBase<RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer, RH6015NullableTypeSymbolsMustNotBePrecededBySpaceCodeFixProvider>
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

    #endregion // Tests
}