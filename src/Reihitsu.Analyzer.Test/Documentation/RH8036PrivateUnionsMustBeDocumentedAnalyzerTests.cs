using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8036PrivateUnionsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8036PrivateUnionsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8036PrivateUnionsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a private union without a documentation comment, on its identifier and
    /// with the rule's message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("RH8036-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateUnionWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  private union {|#0:Inner|}(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport), Diagnostics(RH8036PrivateUnionsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8036MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: undocumented unions of the private group are reported, whatever their type
    /// parameters or container, and documented unions, unions of the non-private group, and structs are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("RH8036-002")]
    [Requirement("SHARED-DOC-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticsForUndocumentedPrivateUnions()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  private union {|RH8036:Hidden|}(int, string);

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  union {|RH8036:Implicit|}<T>(T, string);

                                  /// <summary>
                                  /// Documented
                                  /// </summary>
                                  private union Documented(int, string);

                                  private protected union Shared(int, string);

                                  internal union Visible(int, string);

                                  private struct Point
                                  {
                                  }
                              }

                              /// <summary>
                              /// Holder
                              /// </summary>
                              public union Holder(int, string)
                              {
                                  private union {|RH8036:Nested|}(int, long);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for undocumented unions without a modifier nested in a struct, a record, and
    /// the body of a union, and for a private <c>readonly</c> union
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticsForUndocumentedUnionsOfEveryPrivateKind()
    {
        const string source = """
                              /// <summary>
                              /// Point
                              /// </summary>
                              public struct Point
                              {
                                  union {|RH8036:InStruct|}(int, string);

                                  private readonly union {|RH8036:Money|}(int, long);
                              }

                              /// <summary>
                              /// Entry
                              /// </summary>
                              public record Entry
                              {
                                  union {|RH8036:InRecord|}(int, string);
                              }

                              /// <summary>
                              /// Holder
                              /// </summary>
                              public union Holder(int, string)
                              {
                                  union {|RH8036:InUnion|}(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented private union nested in an undocumented union: the
    /// container's documentation does not decide the nested union
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForUndocumentedUnionNestedInUndocumentedUnion()
    {
        const string source = """
                              public union Holder(int, string)
                              {
                                  union {|RH8036:Nested|}(int, long);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for private unions documented with a summary or with
    /// <c>&lt;inheritdoc/&gt;</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForDocumentedPrivateUnions()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <summary>
                                  /// Shape
                                  /// </summary>
                                  private union Shape(int, string);

                                  /// <inheritdoc/>
                                  private union Inherited(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for undocumented unions of the non-private group: a top-level union with and
    /// without a modifier, a <c>file</c> union, and a union without a modifier nested in an interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForUndocumentedNonPrivateUnions()
    {
        const string source = """
                              public union Shape(int, string);

                              union Implicit(int, long);

                              file union Local(int, string);

                              /// <summary>
                              /// Container
                              /// </summary>
                              public interface IContainer
                              {
                                  union Contained(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              public class Outer
                              {
                                  private union Inner(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject);
                     });
    }

    /// <summary>
    /// Verifies no diagnostic is reported for undocumented private structs and record structs, which are not union
    /// declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    public async Task VerifyNoDiagnosticForPrivateStructDeclarations()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  private struct Point
                                  {
                                  }

                                  private record struct Entry;
                              }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}