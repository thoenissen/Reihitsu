using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8012PrivateEnumsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8012PrivateEnumsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8012PrivateEnumsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested enum without any documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private enum {|#0:NestedStatus|}
                                  {
                                      Active
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8012PrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8012MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested enum that has only a remarks tag but no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateEnumWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <remarks>Only remarks, no summary.</remarks>
                                  private enum {|#0:NestedPriority|}
                                  {
                                      Low
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8012PrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8012MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested enum inside another private class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateEnumNestedInsidePrivateClass()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Outer container.</summary>
                              internal class Outer
                              {
                                  /// <summary>Inner container.</summary>
                                  private class Inner
                                  {
                                      private enum {|#0:DeepNestedStatus|}
                                      {
                                          On
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8012PrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8012MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested enum with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateEnumWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <summary>A documented private inner enum.</summary>
                                  private enum DocumentedInnerEnum
                                  {
                                      Active
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested enum with an inheritdoc tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrivateEnumWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <inheritdoc/>
                                  private enum InheritedInnerEnum
                                  {
                                      Active
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented non-private nested enum, which is handled by RH8011.
    /// The declaration is intentionally left without documentation to confirm the routing decision
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalNestedEnum()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   internal enum UndocumentedInternalInnerEnum
                                   {
                                       ValueA
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
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class OuterType { private enum NestedEnum { Value } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // No-diagnostic cases
}