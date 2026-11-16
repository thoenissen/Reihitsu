using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8035NonPrivateUnionsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8035NonPrivateUnionsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8035NonPrivateUnionsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for a public union without a documentation comment, on its identifier and
    /// with the rule's message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("RH8035-002")]
    public async Task VerifyDiagnosticForPublicUnionWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              public union {|#0:Shape|}(int, string);
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport), Diagnostics(RH8035NonPrivateUnionsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8035MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: undocumented unions of the non-private group are reported, whatever their
    /// modifiers, type parameters, body, or container, and documented unions, unions of the private group, and
    /// structs are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("RH8035-002")]
    [Requirement("SHARED-DOC-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticsForUndocumentedNonPrivateUnions()
    {
        const string source = """
                              public union {|RH8035:Shape|}(int, string);

                              /// <remarks>
                              /// No summary
                              /// </remarks>
                              internal union {|RH8035:Result|}<T>(T, string);

                              union {|RH8035:Implicit|}(int, long);

                              /// <summary>
                              /// Documented
                              /// </summary>
                              public union Documented(int, string)
                              {
                                  private union Hidden(int, string);
                              }

                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  protected union {|RH8035:Visible|}(int, string);

                                  private union Inner(int, string);
                              }

                              public struct Point
                              {
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for undocumented unions that are in the non-private group although they are
    /// nested or declared without a modifier: a <c>file</c> union, a <c>protected internal</c> and a
    /// <c>private protected</c> union in a class, a union without a modifier in an interface, and a <c>readonly</c>
    /// union
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticsForUndocumentedUnionsOfEveryNonPrivateKind()
    {
        const string source = """
                              file union {|RH8035:Local|}(int, string);

                              internal readonly union {|RH8035:Money|}(int, long);

                              /// <summary>
                              /// Outer
                              /// </summary>
                              public class Outer
                              {
                                  protected internal union {|RH8035:Shared|}(int, string);

                                  private protected union {|RH8035:Derived|}(int, string);
                              }

                              /// <summary>
                              /// Container
                              /// </summary>
                              public interface IContainer
                              {
                                  union {|RH8035:Contained|}(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented non-private union nested in an undocumented class: the
    /// container's documentation does not decide the nested union
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForUndocumentedUnionNestedInUndocumentedClass()
    {
        const string source = """
                              public class Outer
                              {
                                  public union {|RH8035:Nested|}(int, string);
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented union with a body whose members are declared in it, and
    /// none for a documented one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForUndocumentedUnionWithBody()
    {
        const string source = """
                              /// <summary>
                              /// Holder
                              /// </summary>
                              public union Holder(int, string)
                              {
                                  public int Count() => 0;
                              }

                              public union {|RH8035:Shape|}(int, string)
                              {
                                  public int Sides() => 0;
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for non-private unions documented with a summary or with
    /// <c>&lt;inheritdoc/&gt;</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForDocumentedUnions()
    {
        const string source = """
                              /// <summary>
                              /// Shape
                              /// </summary>
                              public union Shape(int, string);

                              /// <inheritdoc/>
                              public union Inherited(int, string);
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for undocumented unions of the private group: a union without a modifier
    /// nested in a struct, a record, and a union body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForUndocumentedPrivateUnions()
    {
        const string source = """
                              /// <summary>
                              /// Point
                              /// </summary>
                              public struct Point
                              {
                                  union InStruct(int, string);
                              }

                              /// <summary>
                              /// Entry
                              /// </summary>
                              public record Entry
                              {
                                  union InRecord(int, string);
                              }

                              /// <summary>
                              /// Holder
                              /// </summary>
                              public union Holder(int, string)
                              {
                                  union InUnion(int, string);
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
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              public union Shape(int, string);
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
    /// Verifies no diagnostic is reported for undocumented structs and record structs, which are not union
    /// declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    public async Task VerifyNoDiagnosticForStructDeclarations()
    {
        const string source = """
                              public struct Point
                              {
                              }

                              public record struct Entry;
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases

    #region Documentation holding a parameter element

    /// <summary>
    /// Verifies no diagnostic is reported and no exception is thrown under the default documentation mode for a union
    /// without a <c>&lt;summary&gt;</c> whose documentation comment holds a <c>&lt;param&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForUnionWhoseDocumentationHoldsParamElement()
    {
        const string source = """
                              /// <param name="Circle">Circle</param>
                              public union Shape(int, string);
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
    /// Verifies no diagnostic is reported under the parse documentation mode for a union without a
    /// <c>&lt;summary&gt;</c> whose documentation comment holds a <c>&lt;param&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForUnionWhoseDocumentationHoldsParamElementInParseMode()
    {
        const string source = """
                              /// <param name="Circle">Circle</param>
                              public union Shape(int, string);
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
    /// Verifies no diagnostic is reported for a union without a <c>&lt;summary&gt;</c> whose documentation comment
    /// holds a <c>&lt;paramref&gt;</c> element nested in another element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForUnionWhoseDocumentationHoldsNestedParamrefElement()
    {
        const string source = """
                              /// <remarks>
                              /// <para>See <paramref name="x"/></para>
                              /// </remarks>
                              public union Shape(int, string);
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
    /// Verifies no diagnostic is reported for a union without a <c>&lt;summary&gt;</c> whose documentation comment
    /// holds a <c>&lt;param&gt;</c> element without a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForUnionWhoseDocumentationHoldsParamElementWithoutName()
    {
        const string source = """
                              /// <param>Circle</param>
                              public union Shape(int, string);
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
    /// Verifies no diagnostic is reported for a union without a <c>&lt;summary&gt;</c> when a later documentation
    /// comment holds the <c>&lt;param&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForUnionWhoseLaterDocumentationCommentHoldsParamElement()
    {
        const string source = """
                              /// <remarks>
                              /// First
                              /// </remarks>

                              /// <param name="Circle">Circle</param>
                              public union Shape(int, string);
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
    /// Verifies a diagnostic is still reported for a union without a <c>&lt;summary&gt;</c> whose documentation
    /// comment holds other elements but no <c>&lt;param&gt;</c> or <c>&lt;paramref&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyDiagnosticForUnionWhoseDocumentationHoldsNoParameterElement()
    {
        const string source = """
                              /// <remarks>
                              /// <para>See <typeparamref name="T"/></para>
                              /// </remarks>
                              /// <typeparam name="T">Value</typeparam>
                              public union {|RH8035:Shape|}<T>(T, string);
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies a declaration in the body of a union whose documentation holds a <c>&lt;param&gt;</c> element is
    /// judged on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyDiagnosticForUndocumentedUnionInBodyOfUnionWhoseDocumentationHoldsParamElement()
    {
        const string source = """
                              /// <param name="Circle">Circle</param>
                              public union Shape(int, string)
                              {
                                  public union {|RH8035:Inner|}(int, string);
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
    /// partial union whose second part's documentation comment holds a <c>&lt;param&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPartialUnionWhoseOtherPartHoldsParamElement()
    {
        const string source = """
                              /// <inheritdoc/>
                              public partial union Shape(int, string);

                              /// <param name="Circle">Circle</param>
                              public partial union Shape;
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
    /// Verifies no diagnostic is reported for a partial union without a <c>&lt;summary&gt;</c> whose first part's
    /// documentation comment holds a <c>&lt;paramref&gt;</c> element and whose second part has a remark only
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPartialUnionWhoseFirstPartHoldsParamrefElement()
    {
        const string source = """
                              /// <remarks>See <paramref name="x"/></remarks>
                              public partial union Shape(int, string);

                              /// <remarks>Shape</remarks>
                              public partial union Shape;
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
    /// Verifies no diagnostic is reported for a partial union declared in two files when the documentation comment of
    /// the part in the other file holds a <c>&lt;param&gt;</c> element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8035-001")]
    [Requirement("SHARED-DOC-019")]
    public async Task VerifyNoDiagnosticForPartialUnionWhosePartInAnotherFileHoldsParamElement()
    {
        const string source = """
                              /// <remarks>Shape</remarks>
                              public partial union Shape(int, string);
                              """;

        const string otherPart = """
                                 /// <param name="Circle">Circle</param>
                                 public partial union Shape;
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPart));
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                     });
    }

    #endregion // Documentation holding a parameter element
}