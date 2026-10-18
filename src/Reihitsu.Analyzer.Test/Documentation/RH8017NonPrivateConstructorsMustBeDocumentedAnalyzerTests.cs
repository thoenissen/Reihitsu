using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8017NonPrivateConstructorsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8017-001")]
    [Requirement("RH8017-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForConstructorWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  internal {|#0:TestClass|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8017MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8017-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { internal TestClass() { } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented public constructor and for a protected internal constructor documented without a summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8017-001")]
    [Requirement("RH8017-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForPublicAndProtectedInternalConstructorsWithoutSummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Service.</summary>
                              public class Service
                              {
                                  public {|#0:Service|}()
                                  {
                                  }

                                  /// <remarks>No summary.</remarks>
                                  protected internal {|#1:Service|}(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8017MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented private protected constructor, which belongs to the non-private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8017-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateProtectedConstructorWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Service.</summary>
                              public class Service
                              {
                                  private protected {|#0:Service|}(long value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8017MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for undocumented constructors declared in a struct, a record, and a record struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8017-001")]
    [Requirement("RH8017-002")]
    public async Task VerifyDiagnosticForConstructorsOfStructRecordAndRecordStruct()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Point.</summary>
                              public struct Point
                              {
                                  public {|#0:Point|}(int value)
                                  {
                                  }
                              }

                              /// <summary>Entity.</summary>
                              public record Entity
                              {
                                  public {|#1:Entity|}(int value)
                                  {
                                  }
                              }

                              /// <summary>Entry.</summary>
                              public record struct Entry
                              {
                                  public {|#2:Entry|}(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8017MessageFormat, 3));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a constructor with a summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8017-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForDocumentedConstructor()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Service.</summary>
                              public class Service
                              {
                                  /// <summary>Creates a service.</summary>
                                  /// <param name="value">Value.</param>
                                  internal Service(short value)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented private constructor and an undocumented static constructor of a class, which belong to the private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8017-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForPrivateAndStaticConstructors()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Service.</summary>
                              public class Service
                              {
                                  private Service(byte value)
                                  {
                                  }

                                  static Service()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for the parameter list of a primary constructor, which is not a constructor declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8017-001")]
    public async Task VerifyNoDiagnosticForPrimaryConstructor()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Point.</summary>
                              /// <param name="left">Left.</param>
                              public class Point(int left)
                              {
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}