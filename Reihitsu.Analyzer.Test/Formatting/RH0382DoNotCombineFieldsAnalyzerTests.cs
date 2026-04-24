using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0382DoNotCombineFieldsAnalyzer"/> and <see cref="RH0382DoNotCombineFieldsCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0382DoNotCombineFieldsAnalyzerTests : AnalyzerTestsBase<RH0382DoNotCombineFieldsAnalyzer, RH0382DoNotCombineFieldsCodeFixProvider>
{
    /// <summary>
    /// Verifies that single field declarations do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForSingleFieldDeclarations()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField;
                                    private int secondField;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that combined field declarations are detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyCombinedFieldDeclarationsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0382DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH0382MessageFormat));
    }
}