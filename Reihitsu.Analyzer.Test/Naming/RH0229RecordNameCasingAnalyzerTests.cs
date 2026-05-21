using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0229RecordNameCasingAnalyzer"/> and <see cref="RH0229RecordNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0229RecordNameCasingAnalyzerTests : AnalyzerTestsBase<RH0229RecordNameCasingAnalyzer, RH0229RecordNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for record declarations with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForRecordWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record {|#0:orderrecord|}(int Value);
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record Orderrecord(int Value);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0229RecordNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0229MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for record class declarations with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForRecordClassWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record class {|#0:customeraggregate|}(int Value);
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record class Customeraggregate(int Value);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0229RecordNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0229MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for PascalCase record declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseRecord()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record OrderRecord(int Value);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for record struct declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForRecordStruct()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record struct orderrecord(int Value);
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}