using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0383DoNotPlaceRegionsWithinElementsAnalyzer"/> and <see cref="RH0383DoNotPlaceRegionsWithinElementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0383DoNotPlaceRegionsWithinElementsAnalyzerTests : AnalyzerTestsBase<RH0383DoNotPlaceRegionsWithinElementsAnalyzer, RH0383DoNotPlaceRegionsWithinElementsCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies that type-level regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTypeLevelRegions()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields
                                    private int field;
                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that regions within elements are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionsWithinMethodBodiesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        {|#0:#region Helper|}
                                        var value = 1;
                                        {|#1:#endregion|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0383DoNotPlaceRegionsWithinElementsAnalyzer.DiagnosticId, AnalyzerResources.RH0383MessageFormat, 2));
    }

    #endregion // Members
}