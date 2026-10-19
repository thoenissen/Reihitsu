using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8021PrivateMethodsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8021PrivateMethodsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8021PrivateMethodsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("RH8021-002")]
    public async Task VerifyDiagnosticForPrivateMethodWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  private void {|#0:Execute|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an implicitly private method without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForImplicitlyPrivateMethodWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  void {|#0:Execute|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { private void Execute() { } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an implicitly private extension block member without documentation, which defaults to private accessibility
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForImplicitlyPrivateExtensionMemberMethod()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text helpers.</summary>
                                  /// <param name="value">The source text.</param>
                                  extension(string value)
                                  {
                                      int {|#0:WordCount|}() => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: undocumented private methods are reported on their names, while documented, non-private, explicitly implemented, and local functions are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("RH8021-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedPrivateMethods()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class Service : IDisposable
                              {
                                  private void {|#0:Run|}()
                                  {
                                  }

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  static int {|#1:Compute|}() => 0;

                                  /// <summary>
                                  /// Stops the service
                                  /// </summary>
                                  private void Stop()
                                  {
                                      void Local()
                                      {
                                      }

                                      Local();
                                  }

                                  private protected void Shared()
                                  {
                                  }

                                  void IDisposable.Dispose()
                                  {
                                  }
                              }

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface IContract
                              {
                                  private void {|#2:Helper|}()
                                  {
                                  }

                                  void Execute();
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the specification example: an explicit interface implementation in a class is not reported although it has no access modifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifySpecificationExampleExplicitInterfaceImplementationInClassIsNotReported()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Resource
                              /// </summary>
                              public class Resource : IDisposable
                              {
                                  void IDisposable.Dispose()
                                  {
                                  }

                                  void {|#0:Release|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat));
    }

    /// <summary>
    /// Verifies an explicit implementation of a generic interface method is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForExplicitGenericMethodImplementation()
    {
        const string source = """
                              /// <summary>Factory contract</summary>
                              public interface IFactory
                              {
                                  /// <summary>Creates a value</summary>
                                  /// <typeparam name="T">Value type</typeparam>
                                  /// <returns>The value</returns>
                                  T Create<T>() where T : new();
                              }

                              /// <summary>Factory</summary>
                              public class Factory : IFactory
                              {
                                  T IFactory.Create<T>() => new T();
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: extension block methods without a modifier or with private are in the private group, public and internal ones are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifySpecificationExamplePrivateGroupInExtensionBlock()
    {
        const string source = """
                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  /// <summary>
                                  /// Text values
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                      int {|#0:Implicit|}() => 0;

                                      private int {|#1:Explicit|}() => 0;

                                      public int Visible() => 0;

                                      internal int Assembly() => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat, 2));
    }

    /// <summary>
    /// Verifies undocumented methods without an access modifier in a struct, a record, and a record struct are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForImplicitlyPrivateMethodsOfStructRecordAndRecordStruct()
    {
        const string source = """
                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  void {|#0:Move|}()
                                  {
                                  }
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  int {|#1:Age|}() => 0;
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  static void {|#2:Swap|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat, 3));
    }

    /// <summary>
    /// Verifies undocumented private protected, protected, internal, public, and interface methods without a modifier are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForNonPrivateMethods()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public class Base
                              {
                                  private protected void Shared()
                                  {
                                  }

                                  protected void Derived()
                                  {
                                  }

                                  internal void Assembly()
                                  {
                                  }

                                  public void Visible()
                                  {
                                  }
                              }

                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  void Run();
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the group of a method is decided by its own modifiers, not by the accessibility of its containing type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateMethodsOfFileLocalAndPublicNestedTypes()
    {
        const string source = """
                              file class Local
                              {
                                  private void {|#0:Run|}()
                                  {
                                  }
                              }

                              /// <summary>Outer</summary>
                              public class Outer
                              {
                                  /// <summary>Inner</summary>
                                  public class Inner
                                  {
                                      void {|#1:Run|}()
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the diagnostic for an undocumented generic private method covers its identifier but not its type parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8021-002")]
    public async Task VerifyDiagnosticForGenericPrivateMethodIsLocatedOnNameWithoutTypeParameterList()
    {
        const string source = """
                              /// <summary>Factory</summary>
                              public class Factory
                              {
                                  private T {|#0:Create|}<T>() where T : new() => new T();
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat));
    }

    #endregion // Tests
}