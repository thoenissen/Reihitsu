using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8019DestructorsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8019DestructorsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8019DestructorsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8019-001")]
    [Requirement("RH8019-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForDestructorWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Releases resources.</summary>
                              internal class TestClass
                              {
                                  ~{|#0:TestClass|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8019DestructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8019MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8019-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { ~TestClass() { } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a destructor documented without a summary in a private nested class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8019-001")]
    [Requirement("RH8019-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForDestructorWithoutSummaryInPrivateNestedClass()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Outer.</summary>
                              public class Outer
                              {
                                  /// <summary>Hidden.</summary>
                                  private class Hidden
                                  {
                                      /// <remarks>No summary.</remarks>
                                      ~{|#0:Hidden|}()
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8019DestructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8019MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a destructor with a summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8019-001")]
    [Requirement("SHARED-DOC-004")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForDocumentedDestructor()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Resource.</summary>
                              public class Resource
                              {
                                  /// <summary>Releases the resources.</summary>
                                  ~Resource()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}