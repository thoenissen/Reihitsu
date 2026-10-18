using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8004PrivateStructsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8004PrivateStructsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8004PrivateStructsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested struct without any documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8004-001")]
    [Requirement("RH8004-002")]
    [Requirement("SHARED-DOC-002")]
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

        await Verify(source, Diagnostics(RH8004PrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8004MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested struct that has only a remarks tag but no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8004-001")]
    [Requirement("RH8004-002")]
    [Requirement("SHARED-DOC-005")]
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

        await Verify(source, Diagnostics(RH8004PrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8004MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented generic readonly struct without an access modifier in a class, located on its name without the type parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8004-001")]
    [Requirement("RH8004-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForGenericReadonlyStructWithoutModifierInClass()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  readonly struct {|#0:Pair|}<T>
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8004PrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8004MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested struct with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8004-001")]
    [Requirement("SHARED-DOC-005")]
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
    /// Verifies no diagnostic is reported for a private nested struct with an inheritdoc tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8004-001")]
    [Requirement("SHARED-DOC-005")]
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
    /// Verifies no diagnostic is reported for an undocumented non-private nested struct, which is handled by RH8003
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8004-001")]
    [Requirement("SHARED-DOC-002")]
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

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8004-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class OuterType { private struct NestedStruct { } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented private record struct, which is not a struct declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8004-001")]
    public async Task VerifyNoDiagnosticForPrivateRecordStructDeclaration()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private record struct Entry;
                              }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}