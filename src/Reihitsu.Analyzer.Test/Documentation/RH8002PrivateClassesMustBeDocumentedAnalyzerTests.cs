using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8002PrivateClassesMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8002PrivateClassesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8002PrivateClassesMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested class without any documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("RH8002-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private class {|#0:NestedClass|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8002PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8002MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested class that has only a remarks tag but no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("RH8002-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForPrivateClassWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <remarks>Only remarks, no summary.</remarks>
                                  private class {|#0:NestedClass|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8002PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8002MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested class inside another private class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("RH8002-002")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForPrivateClassNestedInsidePrivateClass()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Outer container.</summary>
                              internal class Outer
                              {
                                  /// <summary>Inner container.</summary>
                                  private class Inner
                                  {
                                      private class {|#0:DeepNested|}
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8002PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8002MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for undocumented private static, abstract, and sealed classes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("RH8002-002")]
    public async Task VerifyDiagnosticForPrivateStaticAbstractAndSealedClassesWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private static class {|#0:Helpers|}
                                  {
                                  }

                                  private abstract class {|#1:Shape|}
                                  {
                                  }

                                  private sealed class {|#2:Circle|} : Shape
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8002PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8002MessageFormat, 3));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented generic class without an access modifier in a class, located on its name without the type parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("RH8002-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForGenericClassWithoutModifierInClass()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  class {|#0:Cache|}<T>
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8002PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8002MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented class without an access modifier in a struct, which belongs to the private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForClassWithoutModifierInStruct()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Value.</summary>
                              public struct Value
                              {
                                  class {|#0:InStruct|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8002PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8002MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for undocumented classes without an access modifier in a record and a record struct, which belong to the private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForClassesWithoutModifierInRecordAndRecordStruct()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Entity.</summary>
                              public record Entity
                              {
                                  class {|#0:InRecord|}
                                  {
                                  }
                              }

                              /// <summary>Entry.</summary>
                              public record struct Entry
                              {
                                  class {|#1:InRecordStruct|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8002PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8002MessageFormat, 2));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested class with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForPrivateClassWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <summary>A documented private inner class.</summary>
                                  private class DocumentedInner
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested class with an inheritdoc tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForPrivateClassWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <inheritdoc/>
                                  private class InheritedInner
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented non-private nested class, which is handled by RH8001
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForInternalNestedClass()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   internal class UndocumentedInternalInner
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
    [Requirement("RH8002-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class OuterType { private class NestedType { } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented private protected class, which belongs to the non-private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForPrivateProtectedNestedClass()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  private protected class Shared
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for undocumented private records declared with record or record class, which are not class declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8002-001")]
    public async Task VerifyNoDiagnosticForPrivateRecordDeclarations()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private record Entity;

                                  private record class Order;
                              }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}