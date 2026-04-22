using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0013DoNotUseQuerySyntaxAnalyzer"/>
/// </summary>
[TestClass]
public class RH0013DoNotUseQuerySyntaxAnalyzerTests : AnalyzerTestsBase<RH0013DoNotUseQuerySyntaxAnalyzer>
{
    /// <summary>
    /// Verifying query syntax is reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task QuerySyntaxIsReportedAndFixed()
    {
        const string testCode = """
                                using System.Linq;

                                public class Test
                                {
                                    public IQueryable<int> Run(IQueryable<int> values)
                                    {
                                        return {|#0:from|} value in values
                                               where value > 0
                                               select value;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }
}