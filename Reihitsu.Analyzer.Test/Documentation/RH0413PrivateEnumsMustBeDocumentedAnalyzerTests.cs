using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0413PrivateEnumsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0413PrivateEnumsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0413PrivateEnumsMustBeDocumentedAnalyzer>
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

        await Verify(source, Diagnostics(RH0413PrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0413MessageFormat));
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

        await Verify(source, Diagnostics(RH0413PrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0413MessageFormat));
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

        await Verify(source, Diagnostics(RH0413PrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0413MessageFormat));
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
    /// Verifies no diagnostic is reported for an undocumented non-private nested enum, which is handled by RH0412.
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

    #endregion // No-diagnostic cases
}