using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer"/> and <see cref="RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzerTests : AnalyzerTestsBase<RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer, RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying static usings are reported and fixed when they appear before regular usings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer.DiagnosticId, AnalyzerResources.RH7205MessageFormat));
    }

    #endregion // Tests
}