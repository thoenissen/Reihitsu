using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0012DoNotPrefixLocalMembersWithThisAnalyzer"/> and <see cref="RH0012DoNotPrefixLocalMembersWithThisCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0012DoNotPrefixLocalMembersWithThisAnalyzerTests : AnalyzerTestsBase<RH0012DoNotPrefixLocalMembersWithThisAnalyzer, RH0012DoNotPrefixLocalMembersWithThisCodeFixProvider>
{
    /// <summary>
    /// Verifying unnecessary this qualifiers are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task UnnecessaryThisQualifierIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _value;

                                    public int Run()
                                    {
                                        return {|#0:this|}._value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _value;

                                     public int Run()
                                     {
                                         return _value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }
}