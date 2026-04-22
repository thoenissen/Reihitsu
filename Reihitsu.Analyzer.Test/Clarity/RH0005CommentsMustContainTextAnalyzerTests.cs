using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0005CommentsMustContainTextAnalyzer"/> and <see cref="RH0005CommentsMustContainTextCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0005CommentsMustContainTextAnalyzerTests : AnalyzerTestsBase<RH0005CommentsMustContainTextAnalyzer, RH0005CommentsMustContainTextCodeFixProvider>
{
    /// <summary>
    /// Verifying empty comments are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task EmptyCommentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0://|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0005CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }
}