using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer"/> and <see cref="RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzerTests : AnalyzerTestsBase<RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer, RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider>
{
    /// <summary>
    /// Verifying static usings are reported and fixed when they appear before regular usings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task StaticUsingsAreReportedAndFixedWhenTheyAppearBeforeRegularUsings()
    {
        const string testCode = """
                                using static {|#0:System.Math|};
                                using Alpha;

                                namespace Alpha
                                {
                                    public class Helper
                                    {
                                    }
                                }

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using Alpha;
                                 using static System.Math;

                                 namespace Alpha
                                 {
                                     public class Helper
                                     {
                                     }
                                 }

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer.DiagnosticId, AnalyzerResources.RH0613MessageFormat));
    }
}