using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0006UseStringEmptyForEmptyStringsAnalyzer"/> and <see cref="RH0006UseStringEmptyForEmptyStringsCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0006UseStringEmptyForEmptyStringsAnalyzerTests : AnalyzerTestsBase<RH0006UseStringEmptyForEmptyStringsAnalyzer, RH0006UseStringEmptyForEmptyStringsCodeFixProvider>
{
    /// <summary>
    /// Verifying empty string literals are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task EmptyStringLiteralIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return {|#0:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return string.Empty;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0006UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }
}