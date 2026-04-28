using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0411PrivateRecordStructsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0411PrivateRecordStructsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0411PrivateRecordStructsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested record struct without any documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateRecordStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private record struct {|#0:NestedMeasurement|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0411PrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0411MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested record struct with only a remarks tag but no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateRecordStructWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <remarks>Only remarks, no summary.</remarks>
                                  private record struct {|#0:NestedMeasurement|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0411PrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0411MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested record struct inside another private class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateRecordStructNestedInsidePrivateClass()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Outer container.</summary>
                              internal class Outer
                              {
                                  /// <summary>Inner container.</summary>
                                  private class Inner
                                  {
                                      private record struct {|#0:DeepNestedMeasurement|};
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0411PrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0411MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested record struct with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateRecordStructWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <summary>A documented private inner record struct.</summary>
                                  private record struct DocumentedInnerRecordStruct;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested record struct with an inheritdoc tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateRecordStructWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <inheritdoc/>
                                  private record struct InheritedInnerRecordStruct;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented non-private nested record struct, which is handled by RH0410.
    /// The declaration is intentionally left without documentation to confirm the routing decision
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalNestedRecordStruct()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   internal record struct UndocumentedInternalInnerRecordStruct;
                               }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}