using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8008PrivateRecordsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8008PrivateRecordsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8008PrivateRecordsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested record without any documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    [Requirement("RH8008-002")]
    [Requirement("SHARED-DOC-002")]
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

        await Verify(source, Diagnostics(RH8008PrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8008MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested record that has only a remarks tag but no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    [Requirement("RH8008-002")]
    [Requirement("SHARED-DOC-005")]
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

        await Verify(source, Diagnostics(RH8008PrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8008MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested record inside another private class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    [Requirement("RH8008-002")]
    [Requirement("SHARED-DOC-001")]
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

        await Verify(source, Diagnostics(RH8008PrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8008MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented positional private record, located on its name without the parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    [Requirement("RH8008-002")]
    public async Task VerifyDiagnosticForPositionalPrivateRecordWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private record {|#0:Hidden|}(int Value);
                              }
                              """;

        await Verify(source, Diagnostics(RH8008PrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8008MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented generic record class without an access modifier in a class, located on its name without the type parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    [Requirement("RH8008-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForGenericRecordClassWithoutModifierInClass()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  record class {|#0:Helper|}<T>
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8008PrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8008MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested record with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    [Requirement("SHARED-DOC-005")]
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
    /// Verifies no diagnostic is reported for a private nested record with an inheritdoc tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    [Requirement("SHARED-DOC-005")]
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
    /// Verifies no diagnostic is reported for an undocumented non-private nested record, which is handled by RH8007.
    /// The declaration is intentionally left without documentation to confirm the routing decision
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    [Requirement("SHARED-DOC-002")]
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

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class OuterType { private record NestedRecord; }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented private class and an undocumented private record struct, which are not record declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8008-001")]
    public async Task VerifyNoDiagnosticForPrivateClassAndRecordStructDeclarations()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private class Plain
                                  {
                                  }

                                  private record struct Entry;
                              }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}