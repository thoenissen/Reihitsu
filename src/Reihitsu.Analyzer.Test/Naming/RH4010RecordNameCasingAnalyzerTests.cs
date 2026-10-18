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
public class RH4010RecordNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4010RecordNameCasingAnalyzer, RH4010RecordNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for record declarations with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4010-001")]
    [Requirement("RH4010-002")]
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
    [Requirement("RH4010-001")]
    [Requirement("RH4010-002")]
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
    [Requirement("RH4010-001")]
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
    [Requirement("RH4010-001")]
    public async Task VerifyNoDiagnosticsForRecordStruct()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record struct orderrecord(int Value);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that records declared as record or record class are reported, and a record struct is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4010-001")]
    public async Task RecordClassesAreCheckedAndRecordStructsAreNot()
    {
        const string testCode = """
                                public record OrderRecord(int Value);

                                public record {|RH4010:orderLine|}(int Value);

                                public record class {|RH4010:customerAggregate|}(int Value);

                                public record struct pointValue(int X);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames each record to PascalCase together with its use as a parameter type, and treats a name without separator or uppercase letter as one word
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4010-002")]
    public async Task FixRenamesRecordAndEveryReference()
    {
        const string testCode = """
                                public record {|RH4010:orderRecord|}(int Id);

                                public record {|RH4010:customerRecord|}(orderRecord Order);

                                public record {|RH4010:orderrecord2|}(int Value);
                                """;

        const string fixedCode = """
                                 public record OrderRecord(int Id);

                                 public record CustomerRecord(OrderRecord Order);

                                 public record Orderrecord2(int Value);
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public record {|#0:orderRecord|}(int Id);

                                    public record {|#1:customerRecord|}(orderRecord Order);
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public record OrderRecord(int Id);

                                     public record CustomerRecord(OrderRecord Order);
                                 }
                                 """;

        // The second record takes the first as a primary constructor parameter type, so the batch rewrites
        // that reference as well as both declarations
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4010RecordNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4010MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}