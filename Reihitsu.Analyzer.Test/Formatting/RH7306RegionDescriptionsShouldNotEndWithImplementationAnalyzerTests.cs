using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer"/>
/// </summary>
[TestClass]
public class RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzerTests : AnalyzerTestsBase<RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer, RH7306RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that concise region descriptions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForConciseDescriptions()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    public void DoWork()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region descriptions ending with implementation are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionDescriptionsEndingWithImplementationAreDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Methods implementation|}

                                    public void DoWork()
                                    {
                                    }

                                    {|#1:#endregion // Methods implementation|}
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     #region Methods

                                     public void DoWork()
                                     {
                                     }

                                     #endregion // Methods
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.DiagnosticId, AnalyzerResources.RH7306MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that case-insensitive implementation suffixes are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyImplementationSuffixIsDetectedCaseInsensitively()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region IDisposable IMPLEMENTATION|}

                                    public void Dispose()
                                    {
                                    }

                                    {|#1:#endregion // IDisposable IMPLEMENTATION|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.DiagnosticId, AnalyzerResources.RH7306MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that words merely containing implementation are not detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDescriptionsContainingImplementationAsPartOfAnotherWordAreIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region ImplementationDetails

                                    public void DoWork()
                                    {
                                    }

                                    #endregion // ImplementationDetails
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}