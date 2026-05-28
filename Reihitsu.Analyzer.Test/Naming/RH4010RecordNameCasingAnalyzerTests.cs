using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4010RecordNameCasingAnalyzer"/> and <see cref="RH4010RecordNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4010RecordNameCasingAnalyzerTests : AnalyzerTestsBase<RH4010RecordNameCasingAnalyzer, RH4010RecordNameCasingCodeFixProvider>
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

        await Verify(testCode, fixedCode, Diagnostics(RH4010RecordNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4010MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH4010RecordNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4010MessageFormat));
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