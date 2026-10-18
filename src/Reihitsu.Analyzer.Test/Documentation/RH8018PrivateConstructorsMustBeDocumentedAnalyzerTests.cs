using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8018PrivateConstructorsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8018PrivateConstructorsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8018PrivateConstructorsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8018-001")]
    [Requirement("RH8018-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateConstructorWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  private {|#0:TestClass|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8018PrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8018MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8018-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { private TestClass() { } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a constructor without an access modifier that is documented without a summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8018-001")]
    [Requirement("RH8018-002")]
    [Requirement("SHARED-DOC-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForConstructorWithoutModifierWithoutSummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Service.</summary>
                              public class Service
                              {
                                  /// <remarks>No summary.</remarks>
                                  {|#0:Service|}(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8018PrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8018MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for undocumented static constructors of a class, a struct, a record, and a record struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8018-001")]
    [Requirement("RH8018-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForStaticConstructorsOfClassStructRecordAndRecordStruct()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Service.</summary>
                              public class Service
                              {
                                  static {|#0:Service|}()
                                  {
                                  }
                              }

                              /// <summary>Point.</summary>
                              public struct Point
                              {
                                  static {|#1:Point|}()
                                  {
                                  }
                              }

                              /// <summary>Entity.</summary>
                              public record Entity
                              {
                                  static {|#2:Entity|}()
                                  {
                                  }
                              }

                              /// <summary>Entry.</summary>
                              public record struct Entry
                              {
                                  static {|#3:Entry|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8018PrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8018MessageFormat, 4));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented constructor without an access modifier in a struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8018-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForConstructorWithoutModifierInStruct()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Point.</summary>
                              public struct Point
                              {
                                  {|#0:Point|}(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8018PrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8018MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private constructor with a summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8018-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForDocumentedPrivateConstructor()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Service.</summary>
                              public class Service
                              {
                                  /// <summary>Creates a service.</summary>
                                  /// <param name="value">Value.</param>
                                  private Service(long value)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented public constructor, which belongs to the non-private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8018-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForPublicConstructor()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Service.</summary>
                              public class Service
                              {
                                  public Service(short value)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}