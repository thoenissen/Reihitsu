using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0230RecordStructNameCasingAnalyzer"/> and <see cref="RH0230RecordStructNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0230RecordStructNameCasingAnalyzerTests : AnalyzerTestsBase<RH0230RecordStructNameCasingAnalyzer, RH0230RecordStructNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for record struct declarations with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForRecordStructWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record struct {|#0:currencyvalue|}(decimal Amount);
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record struct Currencyvalue(decimal Amount);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0230RecordStructNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0230MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for PascalCase record struct declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseRecordStruct()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record struct CurrencyValue(decimal Amount);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for record declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForRecord()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record currencyvalue(decimal Amount);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for record class declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForRecordClass()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record class currencyvalue(decimal Amount);
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}