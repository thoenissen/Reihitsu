using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4011RecordStructNameCasingAnalyzer"/> and <see cref="RH4011RecordStructNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4011RecordStructNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4011RecordStructNameCasingAnalyzer, RH4011RecordStructNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for record struct declarations with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4011-001")]
    [Requirement("RH4011-002")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4011RecordStructNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4011MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for PascalCase record struct declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4011-001")]
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
    [Requirement("RH4011-001")]
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
    [Requirement("RH4011-001")]
    public async Task VerifyNoDiagnosticsForRecordClass()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record class currencyvalue(decimal Amount);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a record struct is reported, and records declared as record or record class are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4011-001")]
    public async Task RecordStructsAreCheckedAndRecordClassesAreNot()
    {
        const string testCode = """
                                public record struct CurrencyValue(decimal Amount);

                                public record struct {|RH4011:currencyvalue|}(decimal Amount);

                                public record orderLine(int Value);

                                public record class customerAggregate(int Value);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames each record struct to PascalCase together with its use as a property type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4011-002")]
    public async Task FixRenamesRecordStructAndEveryReference()
    {
        const string testCode = """
                                public record struct {|RH4011:pointRecord|}(int X, int Y);

                                public record struct {|RH4011:sizeRecord|}(int Width, int Height);

                                public class C
                                {
                                    public pointRecord Location { get; set; }

                                    public sizeRecord Size { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 public record struct PointRecord(int X, int Y);

                                 public record struct SizeRecord(int Width, int Height);

                                 public class C
                                 {
                                     public PointRecord Location { get; set; }

                                     public SizeRecord Size { get; set; }
                                 }
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
                                    public record struct {|#0:pointRecord|}(int X, int Y);

                                    public record struct {|#1:sizeRecord|}(int Width, int Height);

                                    public class Layout
                                    {
                                        public pointRecord Position { get; set; }

                                        public sizeRecord Size { get; set; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public record struct PointRecord(int X, int Y);

                                     public record struct SizeRecord(int Width, int Height);

                                     public class Layout
                                     {
                                         public PointRecord Position { get; set; }

                                         public SizeRecord Size { get; set; }
                                     }
                                 }
                                 """;

        // Both record structs are referenced by the same class, so each rename reaches a property type
        // outside its own declaration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4011RecordStructNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4011MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}