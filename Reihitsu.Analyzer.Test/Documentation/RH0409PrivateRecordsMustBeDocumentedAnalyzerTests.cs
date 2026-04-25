using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0409PrivateRecordsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0409PrivateRecordsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0409PrivateRecordsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested record without any documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateRecordWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private record {|#0:NestedRecord|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0409PrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0409MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested record that has only a remarks tag but no summary.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateRecordWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <remarks>Only remarks, no summary.</remarks>
                                  private record {|#0:NestedRecord|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0409PrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0409MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested record inside another private class.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateRecordNestedInsidePrivateClass()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Outer container.</summary>
                              internal class Outer
                              {
                                  /// <summary>Inner container.</summary>
                                  private class Inner
                                  {
                                      private record {|#0:DeepNestedRecord|};
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0409PrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0409MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested record with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateRecordWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <summary>A documented private inner record.</summary>
                                  private record DocumentedInnerRecord;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested record with an inheritdoc tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateRecordWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <inheritdoc/>
                                  private record InheritedInnerRecord;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented non-private nested record, which is handled by RH0408.
    /// The declaration is intentionally left without documentation to confirm the routing decision.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalNestedRecord()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   internal record UndocumentedInternalInnerRecord;
                               }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}