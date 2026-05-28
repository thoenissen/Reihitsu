using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7303DoNotPlaceRegionsWithinElementsAnalyzer"/> and <see cref="RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7303DoNotPlaceRegionsWithinElementsAnalyzerTests : AnalyzerTestsBase<RH7303DoNotPlaceRegionsWithinElementsAnalyzer, RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider>
{
    #region Tests

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

        await Verify(testData, fixedData, Diagnostics(RH7303DoNotPlaceRegionsWithinElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7303MessageFormat, 2));
    }

    #endregion // Tests
}