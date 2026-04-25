using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0405PrivateStructsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0405PrivateStructsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0405PrivateStructsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested struct without any documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private struct {|#0:NestedStruct|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0405PrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0405MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested struct that has only a remarks tag but no summary.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateStructWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <remarks>Only remarks, no summary.</remarks>
                                  private struct {|#0:NestedStruct|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0405PrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0405MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested struct with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateStructWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <summary>A documented private inner struct.</summary>
                                  private struct DocumentedInner
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested struct with an inheritdoc tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateStructWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <inheritdoc/>
                                  private struct InheritedInner
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented non-private nested struct, which is handled by RH0404.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalNestedStruct()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   internal struct UndocumentedInternalInner
                                   {
                                   }
                               }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}