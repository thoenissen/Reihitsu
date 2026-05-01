using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0226NamespaceCasingAnalyzer"/>
/// </summary>
[TestClass]
public class RH0226NamespaceCasingAnalyzerTests : AnalyzerTestsBase<RH0226NamespaceCasingAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported for lowercase namespace segments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForLowercaseNamespaceSegments()
    {
        const string testCode = """
                                namespace {|#0:reihitsu|}.Analyzer.{|#1:naming|}.Resources
                                {
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0226NamespaceCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0226MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase namespace segments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseNamespace()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Naming.Resources
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies mixed namespace segments only report diagnostics for invalid parts
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMixedNamespaceSegments()
    {
        const string testCode = """
                                namespace Reihitsu.{|#0:analyzer|}.Naming.{|#1:resources|}
                                {
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0226NamespaceCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0226MessageFormat, 2));
    }

    #endregion // Members
}