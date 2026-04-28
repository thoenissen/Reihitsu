using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0417PrivateDelegatesMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0417PrivateDelegatesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0417PrivateDelegatesMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateDelegateWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private delegate void {|#0:NestedHandler|}();
                              }
                              """;

        await Verify(source, Diagnostics(RH0417PrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0417MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private delegate that has a remarks tag but no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateDelegateWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <remarks>A delegate, but no summary.</remarks>
                                  private delegate void {|#0:NestedHandler|}();
                              }
                              """;

        await Verify(source, Diagnostics(RH0417PrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0417MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private generic delegate without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateGenericDelegateWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private delegate TResult {|#0:Transformer|}<TResult>(TResult input);
                              }
                              """;

        await Verify(source, Diagnostics(RH0417PrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0417MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private delegate with parameters without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateDelegateWithParametersWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private delegate bool {|#0:Predicate|}(string value, int index);
                              }
                              """;

        await Verify(source, Diagnostics(RH0417PrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0417MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private delegate with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateDelegateWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <summary>Handles internal events.</summary>
                                  private delegate void NestedHandler();
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private delegate with an inheritdoc tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateDelegateWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <inheritdoc/>
                                  private delegate void NestedHandler();
                              }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases

    #region Access-boundary cases

    /// <summary>
    /// Verifies no diagnostic is reported by this analyzer for an undocumented internal delegate.
    /// Non-private delegates are covered by RH0416, not by this analyzer.
    /// The declaration is intentionally left undocumented to confirm the routing decision
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUndocumentedInternalDelegate()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal delegate void InternalHandler();
                              """;

        await Verify(source);
    }

    #endregion // Access-boundary cases
}