using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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

    #region Documentation holding a parameter element

    /// <summary>
    /// Verifies no diagnostic is reported and no exception is thrown under the default documentation mode for a
    /// private union without a <c>&lt;summary&gt;</c> whose documentation comment holds a <c>&lt;paramref&gt;</c>
    /// element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamrefElement()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <remarks>See <paramref name="x"/></remarks>
                                  private union Inner(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                     });
    }

    /// <summary>
    /// Verifies no diagnostic is reported under the parse documentation mode for a private union without a
    /// <c>&lt;summary&gt;</c> whose documentation comment holds a <c>&lt;paramref&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamrefElementInParseMode()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <remarks>See <paramref name="x"/></remarks>
                                  private union Inner(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                         test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject);
                     });
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private union without a <c>&lt;summary&gt;</c> whose documentation
    /// comment holds a <c>&lt;param&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamElement()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <param name="Circle">Circle</param>
                                  private union Inner(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                     });
    }

    /// <summary>
    /// Verifies a diagnostic is still reported for a private union without a <c>&lt;summary&gt;</c> whose
    /// documentation comment holds other elements but no <c>&lt;param&gt;</c> or <c>&lt;paramref&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyDiagnosticForPrivateUnionWhoseDocumentationHoldsNoParameterElement()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <remarks>See <see cref="Outer"/></remarks>
                                  private union {|RH8036:Inner|}(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies a private union in the body of a union whose documentation holds a <c>&lt;param&gt;</c> element is
    /// judged on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyDiagnosticForUndocumentedUnionInBodyOfUnionWhoseDocumentationHoldsParamElement()
    {
        const string source = """
                              /// <summary>
                              /// Holder
                              /// </summary>
                              /// <param name="x">X</param>
                              public union Holder(int, string)
                              {
                                  union {|RH8036:InUnion|}(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                     });
    }

    /// <summary>
    /// Verifies no diagnostic is reported and no exception is thrown under the default documentation mode for a
    /// private partial union whose second part's documentation comment holds a <c>&lt;paramref&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPrivatePartialUnionWhoseOtherPartHoldsParamrefElement()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public partial class Outer
                              {
                                  /// <remarks>Inner</remarks>
                                  private partial union Inner(int, string);

                                  /// <remarks>See <paramref name="x"/></remarks>
                                  private partial union Inner;
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                     });
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private union without a <c>&lt;summary&gt;</c> whose documentation comment
    /// holds a <c>&lt;ParamRef/&gt;</c> element in mixed case
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsMixedCaseParamrefElement()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <remarks>See <ParamRef name="x"/></remarks>
                                  private union Inner(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                     });
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private union without a <c>&lt;summary&gt;</c> whose documentation comment
    /// holds a <c>&lt;Param&gt;</c> element in another letter case
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamElementInAnotherLetterCase()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <Param name="Circle">Circle</Param>
                                  private union Inner(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                     });
    }

    /// <summary>
    /// Verifies no diagnostic is reported under the parse documentation mode for a private union without a
    /// <c>&lt;summary&gt;</c> whose documentation comment holds a <c>&lt;Param&gt;</c> element in another letter case
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPrivateUnionWhoseDocumentationHoldsParamElementInAnotherLetterCaseInParseMode()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <Param name="Circle">Circle</Param>
                                  private union Inner(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                         test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject);
                     });
    }

    /// <summary>
    /// Verifies no diagnostic is reported and no exception is thrown under the default documentation mode for a private
    /// union declaration without the <c>partial</c> modifier when the documentation comment of its partial declaration
    /// holds a <c>&lt;paramref&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPrivateUnionWithoutPartialModifierWhosePartialDeclarationHoldsParamrefElement()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <remarks>Inner</remarks>
                                  private union Inner;

                                  /// <remarks>See <paramref name="x"/></remarks>
                                  private partial union Inner(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                     });
    }

    /// <summary>
    /// Verifies no diagnostic is reported and no exception is thrown under the documentation mode Parse for a private
    /// union declaration without the <c>partial</c> modifier when the documentation comment of its partial declaration
    /// holds a <c>&lt;paramref&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8036-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPrivateUnionWithoutPartialModifierWhosePartialDeclarationHoldsParamrefElementInParseMode()
    {
        const string source = """
                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  /// <remarks>Inner</remarks>
                                  private union Inner;

                                  /// <remarks>See <paramref name="x"/></remarks>
                                  private partial union Inner(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                         test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject);
                     });
    }

    #endregion // Documentation holding a parameter element
}