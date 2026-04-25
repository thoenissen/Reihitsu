using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0406NonPrivateInterfacesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an internal interface without a documentation comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForInterfaceWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal interface {|#0:ITestContract|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0406MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a public interface without a documentation comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPublicInterfaceWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              public interface {|#0:IPublicService|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0406MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an internal interface with only a remarks tag and no summary.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForInterfaceWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>Missing summary.</remarks>
                              internal interface {|#0:IProcessor|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0406MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a generic internal interface without documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForGenericInterfaceWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal interface {|#0:IRepository|}<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0406MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for an internal interface with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalInterfaceWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A documented internal interface.</summary>
                              internal interface IDocumentedContract
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a public interface documented with inheritdoc.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPublicInterfaceWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <inheritdoc/>
                              public interface IInheritedContract
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a generic internal interface with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForGenericInterfaceWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A generic repository contract.</summary>
                              internal interface IRepository<T>
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented pure private nested interface.
    /// Pure private declarations are covered by RH0407, not by this analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPurePrivateNestedInterface()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   private interface IUndocumentedInner
                                   {
                                   }
                               }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}