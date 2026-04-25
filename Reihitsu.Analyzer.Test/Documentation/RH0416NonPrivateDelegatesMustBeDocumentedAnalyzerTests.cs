using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0416NonPrivateDelegatesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForDelegateWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal delegate void {|#0:Handler|}();
                              """;

        await Verify(source, Diagnostics(RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0416MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a public delegate without documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPublicDelegateWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              public delegate void {|#0:Callback|}();
                              """;

        await Verify(source, Diagnostics(RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0416MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a protected nested delegate without documentation.
    /// Protected is a non-private modifier and therefore falls under the non-private group.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForProtectedDelegateWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  protected delegate void {|#0:ProtectedHandler|}();
                              }
                              """;

        await Verify(source, Diagnostics(RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0416MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a delegate at namespace scope with no explicit access modifier.
    /// A top-level delegate with no modifier defaults to internal, which is non-private.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitInternalDelegateWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              delegate void {|#0:ImplicitInternalHandler|}();
                              """;

        await Verify(source, Diagnostics(RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0416MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a delegate that has a remarks tag but no summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForDelegateWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>A delegate, but no summary.</remarks>
                              internal delegate void {|#0:Handler|}();
                              """;

        await Verify(source, Diagnostics(RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0416MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a generic delegate without documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForGenericDelegateWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal delegate TResult {|#0:Transformer|}<TResult>(TResult input);
                              """;

        await Verify(source, Diagnostics(RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0416MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a delegate with parameters without documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForDelegateWithParametersWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal delegate bool {|#0:Predicate|}(string value, int index);
                              """;

        await Verify(source, Diagnostics(RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0416MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a delegate with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDelegateWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Handles events.</summary>
                              internal delegate void Handler();
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a delegate with an inheritdoc tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDelegateWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <inheritdoc/>
                              public delegate void Callback();
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases

    #region Access-boundary cases

    /// <summary>
    /// Verifies no diagnostic is reported by this analyzer for an undocumented private delegate.
    /// Private delegates are covered by RH0417, not by this analyzer.
    /// The declaration is intentionally left undocumented to confirm the routing decision.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUndocumentedPrivateDelegate()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private delegate void PrivateHandler();
                              }
                              """;

        await Verify(source);
    }

    #endregion // Access-boundary cases
}