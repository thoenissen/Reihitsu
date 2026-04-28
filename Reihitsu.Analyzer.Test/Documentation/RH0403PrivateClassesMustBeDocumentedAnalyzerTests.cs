using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0403PrivateClassesMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0403PrivateClassesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0403PrivateClassesMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested class without any documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(source, Diagnostics(RH0403PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0403MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested class that has only a remarks tag but no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(source, Diagnostics(RH0403PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0403MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private nested class inside another private class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(source, Diagnostics(RH0403PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0403MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private nested class with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies no diagnostic is reported for an undocumented non-private nested class, which is handled by RH0402
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // No-diagnostic cases
}