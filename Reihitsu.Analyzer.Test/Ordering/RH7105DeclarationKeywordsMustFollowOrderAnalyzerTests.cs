using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7105DeclarationKeywordsMustFollowOrderAnalyzer"/> and <see cref="RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests : AnalyzerTestsBase<RH7105DeclarationKeywordsMustFollowOrderAnalyzer, RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider>
{
    #region Tests

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

        await Verify(testCode, fixedCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7105MessageFormat));
    }

    #endregion // Tests
}