using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer>
{
    /// <summary>
    /// Verifying that break statements without a following blank line are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBreakWithoutFollowingBlankLineIsDetected()
    {
        const string testData = """
                                internal class RH0313
                                {
                                    public RH0313()
                                    {
                                        while(true)
                                        {
                                            break;
                                            
                                            {|#0:break|};
                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0313MessageFormat));
    }
}