using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8005NonPrivateInterfacesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an internal interface without a documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("RH8005-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForInterfaceWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal interface {|#0:ITestContract|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8005MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a public interface without a documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("RH8005-002")]
    public async Task VerifyDiagnosticForPublicInterfaceWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              public interface {|#0:IPublicService|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8005MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an internal interface with only a remarks tag and no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("RH8005-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForInterfaceWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>Missing summary.</remarks>
                              internal interface {|#0:IProcessor|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8005MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a generic internal interface without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("RH8005-002")]
    public async Task VerifyDiagnosticForGenericInterfaceWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal interface {|#0:IRepository|}<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8005MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented generic interface without an access modifier in a namespace, located on its name without the type parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("RH8005-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForGenericInterfaceWithoutModifier()
    {
        const string source = """
                              namespace TestNamespace;

                              interface {|#0:IRepository|}<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8005MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented interface without an access modifier nested in an interface, which belongs to the non-private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("SHARED-DOC-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForInterfaceWithoutModifierNestedInInterface()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Documented.</summary>
                              public interface IDocumented
                              {
                                  interface {|#0:INested|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8005MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for an internal interface with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("SHARED-DOC-005")]
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
    /// Verifies no diagnostic is reported for a public interface documented with inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("SHARED-DOC-005")]
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
    /// Verifies no diagnostic is reported for a generic internal interface with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
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
    /// Pure private declarations are covered by RH8006, not by this analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("SHARED-DOC-002")]
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

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8005-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal interface IContract { }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // No-diagnostic cases
}