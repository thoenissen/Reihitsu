using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0604DeclarationKeywordsMustFollowOrderAnalyzer"/> and <see cref="RH0604DeclarationKeywordsMustFollowOrderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0604DeclarationKeywordsMustFollowOrderAnalyzerTests : AnalyzerTestsBase<RH0604DeclarationKeywordsMustFollowOrderAnalyzer, RH0604DeclarationKeywordsMustFollowOrderCodeFixProvider>
{
    /// <summary>
    /// Verifying misordered declaration keywords are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MisorderedDeclarationKeywordsAreReportedAndFixed()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    {|#0:static|} public int Value { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     public static int Value { get; set; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0604DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH0604MessageFormat));
    }
}