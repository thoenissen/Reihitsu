using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0225FileScopedNamespaceCasingAnalyzer"/>
/// </summary>
[TestClass]
public class RH0225FileScopedNamespaceCasingAnalyzerTests : AnalyzerTestsBase<RH0225FileScopedNamespaceCasingAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        const string testCode = """
                                using System;

                                namespace {|#0:reihitsu|}.Analyzer.Test.{|#1:naming|}.{|#2:resources|};
                                """;

        await Verify(testCode, Diagnostics(RH0225FileScopedNamespaceCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0225MessageFormat, 3));
    }
}