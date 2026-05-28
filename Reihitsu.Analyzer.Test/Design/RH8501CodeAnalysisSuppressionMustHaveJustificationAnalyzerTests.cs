using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer"/>
/// </summary>
[TestClass]
public class RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzerTests : AnalyzerTestsBase<RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that missing or empty suppression justifications trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        const string testData = """
                                using System.Diagnostics.CodeAnalysis;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    private const string Reason = "Used for testing";

                                    [{|#0:SuppressMessage|}("Microsoft.Performance", "CA1822")]
                                    internal void WithoutJustification()
                                    {
                                    }

                                    [{|#1:SuppressMessage|}("Microsoft.Performance", "CA1822", Justification = "")]
                                    internal void WithEmptyJustification()
                                    {
                                    }

                                    [SuppressMessage("Microsoft.Performance", "CA1822", Justification = Reason)]
                                    internal void WithJustification()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer.DiagnosticId, AnalyzerResources.RH8501MessageFormat, 2));
    }

    #endregion // Tests
}