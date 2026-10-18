using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8001NonPrivateClassesMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8001NonPrivateClassesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8001NonPrivateClassesMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an internal class that has a remarks tag but no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("RH8001-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForClassWithoutSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>Missing summary.</remarks>
                              internal class {|#0:TestClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a public class without any documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("RH8001-002")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForPublicClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              public class {|#0:PublicService|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a top-level class without an explicit access modifier and without documentation.
    /// A class declared at namespace scope without a modifier defaults to internal accessibility
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("RH8001-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForTopLevelClassWithNoModifierAndNoDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              class {|#0:ImplicitInternalClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a nested protected class without documentation.
    /// Protected is a non-private modifier and falls under the non-private accessibility group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("RH8001-002")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForNestedProtectedClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  protected class {|#0:ProtectedInner|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a nested private protected class without documentation.
    /// The private protected modifier includes a protected qualifier, which makes the declaration
    /// non-pure-private and therefore subject to the non-private documentation rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("RH8001-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForNestedPrivateProtectedClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  private protected class {|#0:PrivateProtectedInner|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a generic internal class without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("RH8001-002")]
    public async Task VerifyDiagnosticForGenericClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class {|#0:Repository|}<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for undocumented static, abstract, and sealed classes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("RH8001-002")]
    public async Task VerifyDiagnosticForStaticAbstractAndSealedClassesWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              public static class {|#0:Helpers|}
                              {
                              }

                              public abstract class {|#1:Shape|}
                              {
                              }

                              public sealed class {|#2:Circle|} : Shape
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the diagnostic for a class with a verbatim identifier is located on the whole identifier, the at sign included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-002")]
    public async Task VerifyDiagnosticForClassWithVerbatimIdentifierCoversTheAtSign()
    {
        const string source = """
                              namespace TestNamespace;

                              public class {|#0:@event|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented class declared at file level, outside any namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForClassAtFileLevelWithoutNamespace()
    {
        const string source = """
                              public class {|#0:TopLevel|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented file-local class, which belongs to the non-private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForFileLocalClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              file class {|#0:FileLocal|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented class and for the undocumented class nested in it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("RH8001-002")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForUndocumentedClassNestedInUndocumentedClass()
    {
        const string source = """
                              namespace TestNamespace;

                              public class {|#0:Outer|}
                              {
                                  public class {|#1:Inner|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented public class nested in a private class, because the accessibility of the containing class does not change the group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPublicClassNestedInPrivateClass()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Outer.</summary>
                              public class Outer
                              {
                                  /// <summary>Hidden.</summary>
                                  private class Hidden
                                  {
                                      public class {|#0:Visible|}
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented class without an access modifier nested in an interface, which belongs to the non-private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForClassWithoutModifierNestedInInterface()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Contract.</summary>
                              public interface IContract
                              {
                                  class {|#0:Nested|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for undocumented classes nested in a struct, a record, and a record struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForClassesNestedInStructRecordAndRecordStruct()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Value.</summary>
                              public struct Value
                              {
                                  public class {|#0:InStruct|}
                                  {
                                  }
                              }

                              /// <summary>Entity.</summary>
                              public record Entity
                              {
                                  public class {|#1:InRecord|}
                                  {
                                  }
                              }

                              /// <summary>Entry.</summary>
                              public record struct Entry
                              {
                                  public class {|#2:InRecordStruct|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat, 3));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class whose documentation comment follows its attribute list, because that comment does not belong to the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyDiagnosticForClassDocumentedAfterAttributeList()
    {
        const string source = """
                              using System;

                              namespace TestNamespace;

                              [Obsolete]
                              /// <summary>After the attribute.</summary>
                              public class {|#0:TestClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class whose summary is nested in a remarks element instead of at the top level
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForClassWithSummaryNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <remarks>
                              /// <summary>Nested.</summary>
                              /// </remarks>
                              public class {|#0:TestClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class whose inheritdoc element is nested in a remarks element instead of at the top level
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForClassWithInheritdocNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <remarks><inheritdoc/></remarks>
                              public class {|#0:TestClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class whose documentation comment holds plain text without any element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyDiagnosticForClassWithPlainTextDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// Plain text
                              public class {|#0:TestClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class with two documentation comments, separated by a blank line, of which neither has a summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-006")]
    public async Task VerifyDiagnosticForClassWhenNoDocumentationCommentHasSummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <remarks>First.</remarks>

                              /// <remarks>Second.</remarks>
                              public class {|#0:TestClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class whose included documentation has no summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-007")]
    public async Task VerifyDiagnosticForClassWithIncludedFileWithoutSummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <include file='docs.xml' path='doc/members/member[@name="B"]/*'/>
                              public class {|#0:TestClass|}
                              {
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>From the file.</summary>
                                             </member>
                                             <member name="B">
                                                 <remarks>Only remarks.</remarks>
                                             </member>
                                             <member name="C">
                                                 <remarks>
                                                     <summary>Nested.</summary>
                                                 </remarks>
                                             </member>
                                             <member name="D">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class whose included documentation has its summary nested in a remarks element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-007")]
    public async Task VerifyDiagnosticForClassWithIncludedSummaryNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <include file='docs.xml' path='doc/members/member[@name="C"]/*'/>
                              public class {|#0:TestClass|}
                              {
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>From the file.</summary>
                                             </member>
                                             <member name="B">
                                                 <remarks>Only remarks.</remarks>
                                             </member>
                                             <member name="C">
                                                 <remarks>
                                                     <summary>Nested.</summary>
                                                 </remarks>
                                             </member>
                                             <member name="D">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class whose included documentation file cannot be resolved
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-007")]
    public async Task VerifyDiagnosticForClassWithUnresolvedIncludedFile()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <include file='missing.xml' path='doc/members/member[@name="A"]/*'/>
                              public class {|#0:TestClass|}
                              {
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>From the file.</summary>
                                             </member>
                                             <member name="B">
                                                 <remarks>Only remarks.</remarks>
                                             </member>
                                             <member name="C">
                                                 <remarks>
                                                     <summary>Nested.</summary>
                                                 </remarks>
                                             </member>
                                             <member name="D">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented class when the documentation mode is parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyDiagnosticForUndocumentedClassInParseMode()
    {
        const string source = """
                              namespace TestNamespace;

                              public class {|#0:TestClass|}
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject), Diagnostics(RH8001NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8001MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for an internal class with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForInternalClassWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A documented internal class.</summary>
                              internal class DocumentedClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a public class documented with inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForPublicClassWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Base class.</summary>
                              public class BaseEntity
                              {
                              }
                              
                              /// <inheritdoc/>
                              public class DerivedEntity : BaseEntity
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented pure private nested class.
    /// Pure private declarations are covered by RH8002, not by this analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForPurePrivateNestedClass()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   private class UndocumentedInner
                                   {
                                   }
                               }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a partial internal class with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForPartialClassWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A partial class.</summary>
                              internal partial class PartialHandler
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for undocumented records declared with record or record class, which are not class declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    public async Task VerifyNoDiagnosticForRecordDeclarations()
    {
        const string source = """
                              namespace TestNamespace;

                              public record Entity;

                              public record class Order;
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose documentation comment precedes its attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyNoDiagnosticForClassDocumentedBeforeAttributeList()
    {
        const string source = """
                              using System;

                              namespace TestNamespace;

                              /// <summary>Before the attribute.</summary>
                              [Obsolete]
                              public class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose documentation comment is followed by an ordinary comment before the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyNoDiagnosticForDocumentationFollowedByOrdinaryComment()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Documented.</summary>
                              // Ordinary comment
                              public class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose documentation comment is followed by a block comment before the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyNoDiagnosticForDocumentationFollowedByBlockComment()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Documented.</summary>
                              /* Block comment */
                              public class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose documentation comment is followed by a conditional directive before the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyNoDiagnosticForDocumentationFollowedByConditionalDirective()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Documented.</summary>
                              #if DEBUG
                              #endif
                              public class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose documentation comment is followed by a region directive before the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyNoDiagnosticForDocumentationFollowedByRegionDirective()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Documented.</summary>
                              #region Types
                              public class TestClass
                              {
                              }
                              #endregion
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose documentation comment is followed by a pragma directive before the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyNoDiagnosticForDocumentationFollowedByPragmaDirective()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Documented.</summary>
                              #pragma warning disable CS0618
                              public class TestClass
                              {
                              }
                              #pragma warning restore CS0618
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose documentation comment is followed by inactive code before the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyNoDiagnosticForDocumentationFollowedByDisabledText()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Documented.</summary>
                              #if NEVER
                              Disabled text
                              #endif
                              public class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class documented with an empty summary element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForClassWithEmptySummaryElement()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary/>
                              public class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class documented with a block documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-004")]
    [Requirement("SHARED-DOC-005")]
    public async Task VerifyNoDiagnosticForClassWithBlockDocumentationComment()
    {
        const string source = """
                              namespace TestNamespace;

                              /** <summary>Block comment.</summary> */
                              public class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose summary is in the second of two documentation comments separated by a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-006")]
    public async Task VerifyNoDiagnosticForClassWithSummaryInLaterDocumentationComment()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <remarks>Remarks.</remarks>

                              /// <summary>Summary.</summary>
                              public class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose summary is in the second of two documentation comments when the documentation mode is parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-006")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticForClassWithSummaryInLaterDocumentationCommentInParseMode()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <remarks>Remarks.</remarks>

                              /// <summary>Summary.</summary>
                              public class TestClass
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose included documentation has a top-level summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-007")]
    public async Task VerifyNoDiagnosticForClassWithSummaryFromIncludedFile()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <include file='docs.xml' path='doc/members/member[@name="A"]/*'/>
                              public class TestClass
                              {
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>From the file.</summary>
                                             </member>
                                             <member name="B">
                                                 <remarks>Only remarks.</remarks>
                                             </member>
                                             <member name="C">
                                                 <remarks>
                                                     <summary>Nested.</summary>
                                                 </remarks>
                                             </member>
                                             <member name="D">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose included documentation has a top-level inheritdoc element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-007")]
    public async Task VerifyNoDiagnosticForClassWithInheritdocFromIncludedFile()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Base.</summary>
                              public class BaseClass
                              {
                              }

                              /// <include file='docs.xml' path='doc/members/member[@name="D"]/*'/>
                              public class TestClass : BaseClass
                              {
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>From the file.</summary>
                                             </member>
                                             <member name="B">
                                                 <remarks>Only remarks.</remarks>
                                             </member>
                                             <member name="C">
                                                 <remarks>
                                                     <summary>Nested.</summary>
                                                 </remarks>
                                             </member>
                                             <member name="D">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class whose included documentation has a top-level summary when the documentation mode is parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-007")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticForClassWithSummaryFromIncludedFileInParseMode()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <include file='docs.xml' path='doc/members/member[@name="A"]/*'/>
                              public class TestClass
                              {
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>From the file.</summary>
                                             </member>
                                             <member name="B">
                                                 <remarks>Only remarks.</remarks>
                                             </member>
                                             <member name="C">
                                                 <remarks>
                                                     <summary>Nested.</summary>
                                                 </remarks>
                                             </member>
                                             <member name="D">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source,
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject);
                         test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation));
                     });
    }

    #endregion // No-diagnostic cases

    #region Malformed XML cases

    /// <summary>
    /// Verifies that a class with an unescaped ampersand in its XML documentation does not crash the analyzer.
    /// Malformed XML must be handled gracefully and degrade to a no-expanded-documentation result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForClassWithMalformedAmpersandInDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A & B</summary>
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source,
                     test => test.SolutionTransforms.Add(ApplyParseDocumentationMode));

        static Solution ApplyParseDocumentationMode(Solution solution, ProjectId projectId)
        {
            var project = solution.GetProject(projectId);

            if (project?.ParseOptions is CSharpParseOptions parseOptions)
            {
                solution = solution.WithProjectParseOptions(projectId, parseOptions.WithDocumentationMode(DocumentationMode.Parse));
            }

            return solution;
        }
    }

    /// <summary>
    /// Verifies that a class with an unclosed XML tag in its documentation does not crash the analyzer.
    /// Malformed XML must be handled gracefully and degrade to a no-expanded-documentation result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForClassWithUnclosedTagInDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Text <em
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source,
                     test => test.SolutionTransforms.Add(ApplyParseDocumentationMode));

        static Solution ApplyParseDocumentationMode(Solution solution, ProjectId projectId)
        {
            var project = solution.GetProject(projectId);

            if (project?.ParseOptions is CSharpParseOptions parseOptions)
            {
                solution = solution.WithProjectParseOptions(projectId, parseOptions.WithDocumentationMode(DocumentationMode.Parse));
            }

            return solution;
        }
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8001-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Malformed XML cases
}