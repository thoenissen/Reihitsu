using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an internal record struct without a documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForRecordStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal record struct {|#0:Measurement|};
                              """;

        await Verify(source, Diagnostics(RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8009MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a public record struct without a documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPublicRecordStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              public record struct {|#0:Vector2D|}(double X, double Y);
                              """;

        await Verify(source, Diagnostics(RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8009MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an internal record struct with only a remarks tag and no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForRecordStructWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>Missing summary.</remarks>
                              internal record struct {|#0:SensorReading|};
                              """;

        await Verify(source, Diagnostics(RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8009MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a nested protected internal record struct without documentation.
    /// Protected internal is non-private and therefore covered by the non-private rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForNestedProtectedInternalRecordStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  protected internal record struct {|#0:ProtectedInternalMeasurement|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8009MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a generic internal record struct without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForGenericRecordStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal record struct {|#0:Pair|}<T>(T First, T Second);
                              """;

        await Verify(source, Diagnostics(RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8009MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for an internal record struct with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalRecordStructWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A documented internal record struct.</summary>
                              internal record struct DocumentedRecordStruct;
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an internal record struct documented with inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForRecordStructWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <inheritdoc/>
                              internal record struct InheritedRecordStruct;
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented pure private nested record struct.
    /// Pure private declarations are covered by RH8010, not by this analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPurePrivateNestedRecordStruct()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   private record struct UndocumentedPrivateRecordStruct;
                               }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a generic internal record struct with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForGenericRecordStructWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A typed pair record struct.</summary>
                              internal record struct Pair<T>(T First, T Second);
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal record struct TestRecordStruct;
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // No-diagnostic cases
}