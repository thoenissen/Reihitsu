using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0404NonPrivateStructsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0404NonPrivateStructsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0404NonPrivateStructsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an internal struct without a documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal struct {|#0:TestStruct|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0404NonPrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0404MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a public struct without a documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPublicStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              public struct {|#0:Point|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0404NonPrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0404MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an internal struct with only a remarks tag and no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForStructWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>Missing summary.</remarks>
                              internal struct {|#0:ValueHolder|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0404NonPrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0404MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a nested protected internal struct without documentation.
    /// Protected internal is non-private and therefore covered by the non-private rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForNestedProtectedInternalStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  protected internal struct {|#0:ProtectedInternalPoint|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0404NonPrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0404MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for an internal struct with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalStructWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A documented internal struct.</summary>
                              internal struct DocumentedStruct
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an internal struct documented with inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForStructWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <inheritdoc/>
                              internal struct InheritedStruct
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented pure private nested struct.
    /// Pure private declarations are covered by RH0405, not by this analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPurePrivateNestedStruct()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   private struct UndocumentedInner
                                   {
                                   }
                               }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}