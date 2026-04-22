using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH0004StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzerTests : AnalyzerTestsBase<RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer, RH0004StatementMustNotUseUnnecessaryParenthesesCodeFixProvider>
{
    /// <summary>
    /// Verifying unnecessary parentheses are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return {|#0:(value)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses."));
    }
}