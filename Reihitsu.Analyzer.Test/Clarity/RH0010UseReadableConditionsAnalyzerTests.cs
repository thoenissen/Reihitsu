using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0010UseReadableConditionsAnalyzer"/> and <see cref="RH0010UseReadableConditionsCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0010UseReadableConditionsAnalyzerTests : AnalyzerTestsBase<RH0010UseReadableConditionsAnalyzer, RH0010UseReadableConditionsCodeFixProvider>
{
    /// <summary>
    /// Verifying Yoda conditions are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int count)
                                    {
                                        return 0 {|#0:<|} count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(int count)
                                     {
                                         return count > 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }
}