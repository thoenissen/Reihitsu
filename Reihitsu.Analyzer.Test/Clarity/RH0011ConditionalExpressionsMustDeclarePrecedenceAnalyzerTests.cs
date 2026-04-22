using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer"/> and <see cref="RH0011ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzerTests : AnalyzerTestsBase<RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer, RH0011ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider>
{
    /// <summary>
    /// Verifying mixed logical operators are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task MixedLogicalOperatorsAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool left, bool middle, bool right)
                                    {
                                        return left || {|#0:middle && right|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(bool left, bool middle, bool right)
                                     {
                                         return left || (middle && right);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence."));
    }
}