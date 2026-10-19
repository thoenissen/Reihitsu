using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8020NonPrivateMethodsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8020NonPrivateMethodsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8020NonPrivateMethodsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("RH8020-002")]
    public async Task VerifyDiagnosticForMethodWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  internal void {|#0:Execute|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { internal void Execute() { } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an implicitly private method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForImplicitlyPrivateMethod()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies extension block members are still validated by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("RH8020-002")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForUndocumentedExtensionMemberMethod()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text helpers.</summary>
                                  /// <param name="value">The source text.</param>
                                  extension(string value)
                                  {
                                      public int {|#0:WordCount|}() => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an implicitly private extension block member, which defaults to private accessibility
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForImplicitlyPrivateExtensionMemberMethod()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text helpers.</summary>
                                  /// <param name="value">The source text.</param>
                                  extension(string value)
                                  {
                                      int WordCount() => 0;
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: undocumented non-private methods are reported on their names, while documented, private, and local functions are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("RH8020-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedNonPrivateMethods()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public class Service
                              {
                                  public void {|#0:Run|}()
                                  {
                                  }

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  protected virtual int {|#1:Compute|}<T>(T value) => 0;

                                  internal static void {|#2:Reset|}()
                                  {
                                  }

                                  public override string {|#3:ToString|}() => "";

                                  /// <inheritdoc/>
                                  public override int GetHashCode() => 0;

                                  /// <summary>
                                  /// Stops the service
                                  /// </summary>
                                  public void Stop()
                                  {
                                      void Local()
                                      {
                                      }

                                      Local();
                                  }

                                  private void Hidden()
                                  {
                                  }
                              }

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface IContract
                              {
                                  void {|#4:Execute|}();
                              }

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
                                      public int {|#5:WordCount|}() => 0;

                                      int Hidden() => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat, 6));
    }

    /// <summary>
    /// Verifies the diagnostic for an undocumented generic method covers its identifier but not its type parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-002")]
    public async Task VerifyDiagnosticForGenericMethodIsLocatedOnNameWithoutTypeParameterList()
    {
        const string source = """
                              /// <summary>Factory</summary>
                              public class Factory
                              {
                                  public T {|#0:Create|}<T>() where T : new() => new T();
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies the diagnostic for an undocumented method with a verbatim identifier covers the whole identifier, the at sign included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-002")]
    public async Task VerifyDiagnosticForMethodWithVerbatimIdentifierCoversTheAtSign()
    {
        const string source = """
                              /// <summary>Service</summary>
                              public class Service
                              {
                                  public void {|#0:@event|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies undocumented interface methods without an access modifier, static abstract ones included, are reported while a private interface method is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForInterfaceMethodsWithoutAccessModifier()
    {
        const string source = """
                              /// <summary>Contract</summary>
                              /// <typeparam name="TSelf">Self</typeparam>
                              public interface IContract<TSelf> where TSelf : IContract<TSelf>
                              {
                                  void {|#0:Run|}();

                                  static abstract TSelf {|#1:Create|}();

                                  int {|#2:Compute|}() => 0;

                                  private void Helper()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat, 3));
    }

    /// <summary>
    /// Verifies undocumented non-private methods declared in a struct, a record, and a record struct are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForMethodsOfStructRecordAndRecordStruct()
    {
        const string source = """
                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  public void {|#0:Move|}()
                                  {
                                  }
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  internal int {|#1:Age|}() => 0;
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  public static void {|#2:Swap|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat, 3));
    }

    /// <summary>
    /// Verifies undocumented abstract, expression-bodied, block-bodied, and extern methods are all reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    public async Task VerifyDiagnosticForMethodsWhateverTheirBodyForm()
    {
        const string source = """
                              /// <summary>Shape</summary>
                              public abstract class Shape
                              {
                                  public abstract double {|#0:Area|}();

                                  public double {|#1:Half|}() => Area() / 2;

                                  public void {|#2:Draw|}()
                                  {
                                  }

                                  public static extern void {|#3:Native|}();
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat, 4));
    }

    /// <summary>
    /// Verifies undocumented private protected and protected internal methods belong to the non-private group and are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateProtectedAndProtectedInternalMethods()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public class Base
                              {
                                  private protected void {|#0:Shared|}()
                                  {
                                  }

                                  protected internal void {|#1:Visible|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the group of a method is decided by its own modifiers, not by the accessibility of its containing type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForNonPrivateMethodsOfPrivateAndFileLocalTypes()
    {
        const string source = """
                              /// <summary>Outer</summary>
                              public class Outer
                              {
                                  /// <summary>Hidden</summary>
                                  private class Hidden
                                  {
                                      public void {|#0:Run|}()
                                      {
                                      }
                                  }
                              }

                              /// <summary>Local</summary>
                              file class Local
                              {
                                  internal void {|#1:Run|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example: an explicit interface implementation declared in an interface is not reported, while an undocumented interface method is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifySpecificationExampleExplicitInterfaceImplementationInInterfaceIsNotReported()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface IContract
                              {
                                  /// <summary>
                                  /// Runs the contract
                                  /// </summary>
                                  void Run();
                              }

                              /// <summary>
                              /// Derived contract
                              /// </summary>
                              public interface IDerived : IContract
                              {
                                  void IContract.Run()
                                  {
                                  }

                                  void {|#0:Stop|}();
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies a documentation comment between the attribute list and the method does not document it, while one before the attribute list or before a directive does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyDiagnosticForMethodDocumentedOnlyAfterItsAttributeList()
    {
        const string source = """
                              using System;

                              /// <summary>Service</summary>
                              public class Service
                              {
                                  /// <summary>Before the attribute</summary>
                                  [Obsolete]
                                  public void Before()
                                  {
                                  }

                                  [Obsolete]
                                  /// <summary>After the attribute</summary>
                                  public void {|#0:After|}()
                                  {
                                  }

                                  /// <summary>Before a directive</summary>
                              #if DEBUG
                              #endif
                                  public void Directive()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies a method whose summary is nested in remarks is reported, while one with an empty summary or a top-level inheritdoc is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForMethodWithSummaryNestedInRemarks()
    {
        const string source = """
                              /// <summary>Service</summary>
                              public class Service
                              {
                                  /// <remarks>
                                  /// <summary>Nested</summary>
                                  /// </remarks>
                                  public void {|#0:Nested|}()
                                  {
                                  }

                                  /// <summary/>
                                  public void Empty()
                                  {
                                  }

                                  /// <inheritdoc/>
                                  public override string ToString() => "";
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies a summary in a later documentation comment documents a method, while two comments without a summary do not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-006")]
    public async Task VerifyMethodSummaryInLaterDocumentationCommentCounts()
    {
        const string source = """
                              /// <summary>Service</summary>
                              public class Service
                              {
                                  /// <remarks>Remarks</remarks>

                                  /// <summary>Summary</summary>
                                  public void Run()
                                  {
                                  }

                                  /// <remarks>First</remarks>

                                  /// <remarks>Second</remarks>
                                  public void {|#0:Stop|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies an included summary documents a method, while included content without a summary and an unresolved file do not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-007")]
    public async Task VerifyMethodSummaryInIncludedFileCounts()
    {
        const string source = """
                              /// <summary>Service</summary>
                              public class Service
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="A"]/*'/>
                                  public void Run()
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="B"]/*'/>
                                  public void {|#0:Stop|}()
                                  {
                                  }

                                  /// <include file='missing.xml' path='doc/members/member[@name="A"]/*'/>
                                  public void {|#1:Reset|}()
                                  {
                                  }
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>From the file</summary>
                                             </member>
                                             <member name="B">
                                                 <remarks>Only remarks</remarks>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat, 2));
    }

    /// <summary>
    /// Verifies an undocumented non-private method is reported when the documentation mode is parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyDiagnosticForUndocumentedMethodInParseMode()
    {
        const string source = """
                              /// <summary>Service</summary>
                              public class Service
                              {
                                  public void {|#0:Run|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject), Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies an extension block member is decided by its own documentation, whether or not its extension block is documented
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8020-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyExtensionBlockMembersAreDecidedOnTheirOwn()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  extension(string value)
                                  {
                                      /// <summary>Counts the words</summary>
                                      public int WordCount() => 0;
                                  }

                                  /// <summary>Integer values</summary>
                                  /// <param name="number">Number</param>
                                  extension(int number)
                                  {
                                      /// <remarks>No summary</remarks>
                                      public int {|#0:Digits|}() => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    #endregion // Tests
}