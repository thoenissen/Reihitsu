using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0605ProtectedMustComeBeforeInternalAnalyzer"/> and <see cref="RH0605ProtectedMustComeBeforeInternalCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0605ProtectedMustComeBeforeInternalAnalyzerTests : AnalyzerTestsBase<RH0605ProtectedMustComeBeforeInternalAnalyzer, RH0605ProtectedMustComeBeforeInternalCodeFixProvider>
{
    /// <summary>
    /// Verifying compound access modifiers are reported and fixed when the keyword order is wrong.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task MisorderedCompoundAccessModifiersAreReportedAndFixed()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    internal {|#0:protected|} void Execute()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     protected internal void Execute()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0605ProtectedMustComeBeforeInternalAnalyzer.DiagnosticId, AnalyzerResources.RH0605MessageFormat));
    }
}