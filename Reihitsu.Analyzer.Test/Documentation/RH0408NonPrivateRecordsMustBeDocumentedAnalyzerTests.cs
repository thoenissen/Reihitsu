using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0408NonPrivateRecordsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0408NonPrivateRecordsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0408NonPrivateRecordsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an internal record without a documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForRecordWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal record {|#0:CustomerRecord|};
                              """;

        await Verify(source, Diagnostics(RH0408NonPrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0408MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a public record without a documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPublicRecordWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              public record {|#0:OrderRecord|};
                              """;

        await Verify(source, Diagnostics(RH0408NonPrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0408MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an internal record that has a remarks tag but no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForRecordWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>Missing summary.</remarks>
                              internal record {|#0:EventRecord|};
                              """;

        await Verify(source, Diagnostics(RH0408NonPrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0408MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a top-level record with no explicit access modifier and no documentation.
    /// A record declared at namespace scope without a modifier defaults to internal accessibility
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForTopLevelRecordWithNoModifierAndNoDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              record {|#0:ImplicitInternalRecord|};
                              """;

        await Verify(source, Diagnostics(RH0408NonPrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0408MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a generic internal record without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForGenericRecordWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal record {|#0:ResultRecord|}<T>(T Value);
                              """;

        await Verify(source, Diagnostics(RH0408NonPrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0408MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a nested private protected record without documentation.
    /// The private protected modifier includes protected, making the declaration non-pure-private and
    /// therefore subject to this non-private documentation rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForNestedPrivateProtectedRecordWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  private protected record {|#0:PrivateProtectedRecord|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0408NonPrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0408MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a positional internal record without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPositionalRecordWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal record {|#0:CoordinateRecord|}(double Latitude, double Longitude);
                              """;

        await Verify(source, Diagnostics(RH0408NonPrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0408MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for an internal record with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalRecordWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A documented internal record.</summary>
                              internal record DocumentedRecord;
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a public record documented with inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPublicRecordWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Base record.</summary>
                              public record BaseRecord;
                              
                              /// <inheritdoc/>
                              public record DerivedRecord : BaseRecord;
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented pure private nested record.
    /// Pure private declarations are covered by RH0409, not by this analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPurePrivateNestedRecord()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   private record UndocumentedPrivateRecord;
                               }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a generic internal record with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForGenericRecordWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A typed result record.</summary>
                              internal record ResultRecord<T>(T Value);
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a partial internal record with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPartialRecordWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A partial record.</summary>
                              internal partial record PartialRecord;
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}