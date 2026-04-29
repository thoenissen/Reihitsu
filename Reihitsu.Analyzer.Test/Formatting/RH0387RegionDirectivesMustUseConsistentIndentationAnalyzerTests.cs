using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0387RegionDirectivesMustUseConsistentIndentationAnalyzer"/>
/// </summary>
[TestClass]
public class RH0387RegionDirectivesMustUseConsistentIndentationAnalyzerTests : AnalyzerTestsBase<RH0387RegionDirectivesMustUseConsistentIndentationAnalyzer>
{
    /// <summary>
    /// Verifies that correctly indented regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForCorrectlyIndentedRegions()
    {
        const string testData = """
                                namespace Sample
                                {
                                    internal class TestClass
                                    {
                                        #region Fields

                                        private readonly int _value;

                                        #endregion // Fields
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that misindented type-level regions are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisindentedTypeLevelRegionsAreDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                {|#0:#region Fields|}

                                    private readonly int _value;

                                {|#1:#endregion // Fields|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH0387RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH0387MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that misindented namespace-level regions are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisindentedNamespaceLevelRegionsAreDetected()
    {
        const string testData = """
                                namespace Sample
                                {
                                {|#0:#region Types|}

                                    internal class TestClass
                                    {
                                    }

                                {|#1:#endregion // Types|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH0387RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH0387MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that file-scoped namespace regions can stay at file indentation level
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForFileScopedNamespaceRegions()
    {
        const string testData = """
                                namespace Sample;

                                #region Types

                                internal class TestClass
                                {
                                }

                                #endregion // Types
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that regions within method bodies are ignored by this analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionsWithinMethodBodiesAreIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                #region Helper
                                        var value = 1;
                                #endregion // Helper
                                    }
                                }
                                """;

        await Verify(testData);
    }
}