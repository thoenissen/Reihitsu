using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0226NamespaceCasingAnalyzerTests"/>
/// </summary>
[TestClass]
public class RH0226NamespaceCasingAnalyzerTests : AnalyzerTestsBase<RH0226NamespaceCasingAnalyzer>
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

            namespace {|#0:reihitsu|}.Analyzer.Test.{|#1:naming|}.{|#2:resources|}
            {
            }
            """;

        await Verify(testCode, Diagnostics(RH0226NamespaceCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0226MessageFormat, 3));
    }
}