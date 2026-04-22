using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0003CodeMustNotContainEmptyStatementsAnalyzer"/> and <see cref="RH0003CodeMustNotContainEmptyStatementsCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0003CodeMustNotContainEmptyStatementsAnalyzerTests : AnalyzerTestsBase<RH0003CodeMustNotContainEmptyStatementsAnalyzer, RH0003CodeMustNotContainEmptyStatementsCodeFixProvider>
{
    /// <summary>
    /// Verifying empty statements are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task EmptyStatementIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:;|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0003CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }
}