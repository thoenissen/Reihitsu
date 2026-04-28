using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer, RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyCodeFixProvider>
{
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
                                    unsafe void Method()
                                    {
                                        int value = 0;
                                        int* pointer = &value;
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
                                    unsafe void Method()
                                    {
                                        int value = 0;
                                        int* pointer = &{|#0: |}value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     unsafe void Method()
                                     {
                                         int value = 0;
                                         int* pointer = &value;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0354MessageFormat));
    }
}