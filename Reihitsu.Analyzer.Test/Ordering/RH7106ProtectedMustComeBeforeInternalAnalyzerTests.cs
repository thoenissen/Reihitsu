using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7106ProtectedMustComeBeforeInternalAnalyzer"/> and <see cref="RH7106ProtectedMustComeBeforeInternalCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7106ProtectedMustComeBeforeInternalAnalyzerTests : AnalyzerTestsBase<RH7106ProtectedMustComeBeforeInternalAnalyzer, RH7106ProtectedMustComeBeforeInternalCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying compound access modifiers are reported and fixed when the keyword order is wrong
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH7106ProtectedMustComeBeforeInternalAnalyzer.DiagnosticId, AnalyzerResources.RH7106MessageFormat));
    }

    #endregion // Tests
}