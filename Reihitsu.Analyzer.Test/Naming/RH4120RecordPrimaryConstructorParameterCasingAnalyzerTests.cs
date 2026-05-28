using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4120RecordPrimaryConstructorParameterCasingAnalyzer"/> and <see cref="RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests : AnalyzerTestsBase<RH4120RecordPrimaryConstructorParameterCasingAnalyzer, RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for camelCase record primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForRecordPrimaryConstructorParameterWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record Product(string {|#0:productCode|});
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record Product(string ProductCode);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4120MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for camelCase record struct primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForRecordStructPrimaryConstructorParameterWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record struct ProductId(string {|#0:valueCode|});
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record struct ProductId(string ValueCode);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4120MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for PascalCase record primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseRecordPrimaryConstructorParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record Product(string ProductCode);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for method parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForMethodParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(string productCode)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}