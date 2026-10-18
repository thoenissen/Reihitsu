using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing extension declaration parameter comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("RH8032-002")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyDiagnosticForMissingExtensionDeclarationParameterDocumentation()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text extensions.</summary>
                                  extension(string {|#0:value|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a fully documented generic extension declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyNoDiagnosticsForDocumentedGenericExtensionDeclaration()
    {
        const string source = """
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  /// <param name="values">The values to inspect.</param>
                                  extension<T>(IEnumerable<T> values)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text extensions.</summary>
                                  extension(string {|#0:value|})
                                  {
                                  }
                              }
                              """;

        await Verify(source,
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject);
                     });
    }

    /// <summary>
    /// Verifies the specification example: the parameter of a documented extension declaration without a matching param element is reported on its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("RH8032-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedExtensionParameter()
    {
        const string source = """
                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  /// <summary>
                                  /// Adds helpers to text values
                                  /// </summary>
                                  extension(string {|#0:value|})
                                  {
                                  }

                                  /// <summary>
                                  /// Adds helpers to integer values
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: no parameter is reported for an undocumented extension declaration or one with a top-level inheritdoc in its first or a later documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-011")]
    public async Task VerifySpecificationExampleParametersAreCheckedOnlyWithoutInheritdoc()
    {
        const string source = """
                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  extension(string value)
                                  {
                                  }

                                  /// <inheritdoc/>
                                  extension(int value)
                                  {
                                  }

                                  /// <remarks>
                                  /// Remarks
                                  /// </remarks>

                                  /// <inheritdoc/>
                                  extension(byte value)
                                  {
                                  }

                                  /// <summary>
                                  /// Long values
                                  /// </summary>
                                  extension(long {|#0:value|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat));
    }

    /// <summary>
    /// Verifies an inheritdoc element brought in by an include element exempts the extension parameter, while included content with only a summary does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-011")]
    public async Task VerifyInheritdocInIncludedFileExemptsParameter()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="C"]/*'/>
                                  extension(string value)
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="B"]/*'/>
                                  extension(int {|#0:value|})
                                  {
                                  }
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>From the file</summary>
                                                 <typeparam name="T">Element</typeparam>
                                                 <param name="values">Values</param>
                                             </member>
                                             <member name="B">
                                                 <summary>Only a summary</summary>
                                             </member>
                                             <member name="C">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a parameter is documented only by a top-level param element whose name equals the parameter name without its at sign
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("RH8032-002")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifySpecificationExampleParameterIsDocumentedByTopLevelElementWithItsName()
    {
        const string source = """
                              using System.Collections.Generic;

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
                                  }

                                  /// <summary>
                                  /// Integer values
                                  /// </summary>
                                  /// <param name="Value">Value</param>
                                  extension(int {|#0:value|})
                                  {
                                  }

                                  /// <summary>
                                  /// Long values
                                  /// </summary>
                                  /// <remarks>
                                  /// <param name="value">Value</param>
                                  /// </remarks>
                                  extension(long {|#1:value|})
                                  {
                                  }

                                  /// <remarks>
                                  /// Remarks
                                  /// </remarks>

                                  /// <summary>
                                  /// Lists
                                  /// </summary>
                                  /// <typeparam name="T"></typeparam>
                                  /// <param name="values">Values</param>
                                  extension<T>(List<T> values)
                                  {
                                  }

                                  /// <summary>
                                  /// Pairs
                                  /// </summary>
                                  /// <typeparam name="TKey">Key</typeparam>
                                  /// <param name="pairs">Pairs</param>
                                  extension<TKey, TValue>(Dictionary<TKey, TValue> pairs)
                                  {
                                  }

                                  /// <summary>
                                  /// Bytes
                                  /// </summary>
                                  /// <param name="class">Value</param>
                                  extension(byte @class)
                                  {
                                  }

                                  /// <summary>
                                  /// Characters
                                  /// </summary>
                                  /// <param name="@class">Value</param>
                                  extension(char {|#2:@class|})
                                  {
                                  }

                                  /// <summary>
                                  /// Arrays
                                  /// </summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="items">Items</param>
                                  extension<@T>(T[] items)
                                  {
                                  }

                                  /// <summary>
                                  /// Sets
                                  /// </summary>
                                  /// <typeparam name="@T">Element</typeparam>
                                  /// <param name="items">Items</param>
                                  extension<@T>(HashSet<T> items)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the specification example: a param element brought in by an include element documents the parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifySpecificationExampleParameterIsDocumentedByIncludedFile()
    {
        const string source = """
                              using System.Collections.Generic;

                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="A"]/*'/>
                                  extension<T>(IEnumerable<T> values)
                                  {
                                  }
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>From the file</summary>
                                                 <typeparam name="T">Element</typeparam>
                                                 <param name="values">Values</param>
                                             </member>
                                             <member name="B">
                                                 <summary>Only a summary</summary>
                                             </member>
                                             <member name="C">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)));
    }

    /// <summary>
    /// Verifies a param element in a later documentation comment, and one without text, documents the parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyParameterDocumentedInLaterCommentOrWithoutTextCounts()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Text values</summary>

                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                  }

                                  /// <summary>Integer values</summary>
                                  /// <param name="value"></param>
                                  extension(int value)
                                  {
                                  }

                                  /// <summary>Long values</summary>
                                  /// <param name="value"/>
                                  extension(long value)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: extension declarations the compiler merges share their documentation, and those it keeps apart do not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-011")]
    [Requirement("SHARED-DOC-012")]
    [Requirement("SHARED-DOC-013")]
    public async Task VerifySpecificationExampleMergedExtensionDeclarationsShareDocumentation()
    {
        const string source = """
                              #nullable enable

                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  /// <summary>
                                  /// Text values
                                  /// </summary>
                                  extension(string value)
                                  {
                                  }

                                  /// <summary>
                                  /// More text values
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                  }

                                  /// <summary>
                                  /// Nullable text values
                                  /// </summary>
                                  extension(string? {|#0:value|})
                                  {
                                  }

                                  /// <summary>
                                  /// Integer values
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension(int value)
                                  {
                                  }

                                  /// <summary>
                                  /// More integer values
                                  /// </summary>
                                  extension(int {|#1:other|})
                                  {
                                  }

                                  /// <summary>
                                  /// Long values
                                  /// </summary>
                                  extension(long value)
                                  {
                                  }

                                  /// <inheritdoc/>
                                  extension(long value)
                                  {
                                  }

                                  /// <summary>
                                  /// Characters
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension(char value)
                                  {
                                  }

                                  /// <summary>
                                  /// More characters
                                  /// </summary>
                                  extension(System.Char value)
                                  {
                                  }

                                  /// <summary>
                                  /// Pairs
                                  /// </summary>
                                  /// <param name="pair">Pair</param>
                                  extension((int A, int B) pair)
                                  {
                                  }

                                  /// <summary>
                                  /// Other pairs
                                  /// </summary>
                                  extension((int C, int D) {|#2:pair|})
                                  {
                                  }

                                  /// <summary>
                                  /// Read-only references
                                  /// </summary>
                                  /// <param name="number">Number</param>
                                  extension(in double number)
                                  {
                                  }

                                  /// <summary>
                                  /// Writable references
                                  /// </summary>
                                  extension(ref double {|#3:number|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat, 4));
    }

    /// <summary>
    /// Verifies extension declarations merged across the files of a partial static class share their documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-013")]
    public async Task VerifyMergedExtensionDeclarationsAcrossFilesOfPartialClassShareDocumentation()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static partial class Extensions
                              {
                                  /// <summary>Text values</summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                  }
                              }
                              """;

        const string secondSource = """
                                    public static partial class Extensions
                                    {
                                        /// <summary>More text values</summary>
                                        extension(string value)
                                        {
                                        }

                                        /// <summary>Integer values</summary>
                                        extension(int {|#0:value|})
                                        {
                                        }
                                    }
                                    """;

        await Verify(source, test => test.TestState.Sources.Add(("/0/Test1.cs", secondSource)), Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat));
    }

    /// <summary>
    /// Verifies a merged extension declaration without a documentation comment is not reported, while its documented sibling without a param element is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-011")]
    [Requirement("SHARED-DOC-013")]
    public async Task VerifyUndocumentedMergedExtensionDeclarationIsNotReported()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Text values</summary>
                                  extension(string {|#0:value|})
                                  {
                                  }

                                  extension(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat));
    }

    /// <summary>
    /// Verifies extension declarations that differ in in, ref readonly, or scoped do not share their documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-013")]
    public async Task VerifyExtensionDeclarationsWithDifferentReferenceModifiersDoNotShareDocumentation()
    {
        const string source = """
                              using System;

                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Read-only references</summary>
                                  /// <param name="number">Number</param>
                                  extension(in int number)
                                  {
                                  }

                                  /// <summary>Read-only references again</summary>
                                  extension(ref readonly int {|#0:number|})
                                  {
                                  }

                                  /// <summary>Spans</summary>
                                  /// <param name="span">Span</param>
                                  extension(ref Span<int> span)
                                  {
                                  }

                                  /// <summary>Scoped spans</summary>
                                  extension(scoped ref Span<int> {|#1:span|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat, 2));
    }

    /// <summary>
    /// Verifies extension declarations whose parameters differ in their attributes or attribute arguments do not share their documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-013")]
    public async Task VerifyExtensionDeclarationsWithDifferentParameterAttributesDoNotShareDocumentation()
    {
        const string source = """
                              using System;

                              /// <summary>Marker</summary>
                              [AttributeUsage(AttributeTargets.All)]
                              public sealed class MarkAttribute : Attribute
                              {
                                  /// <summary>Creates the marker</summary>
                                  /// <param name="value">Value</param>
                                  public MarkAttribute(int value)
                                  {
                                  }
                              }

                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Marked</summary>
                                  /// <param name="value">Value</param>
                                  extension([Mark(1)] long value)
                                  {
                                  }

                                  /// <summary>Marked differently</summary>
                                  extension([Mark(2)] long {|#0:value|})
                                  {
                                  }

                                  /// <summary>Unmarked</summary>
                                  extension(long {|#1:value|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat, 2));
    }

    /// <summary>
    /// Verifies extension declarations whose receiver type is nullable-annotated in one and nullable-oblivious in the other do not share their documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-013")]
    public async Task VerifyAnnotatedAndObliviousReceiverTypesDoNotShareDocumentation()
    {
        const string source = """
                              #nullable enable

                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Annotated</summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                  }

                              #nullable disable
                                  /// <summary>Oblivious</summary>
                                  extension(string {|#0:value|})
                                  {
                                  }
                              #nullable enable
                              }
                              """;

        await Verify(source, Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat));
    }

    /// <summary>
    /// Verifies an undocumented extension parameter is reported when the documentation mode is parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8032-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyDiagnosticForUndocumentedExtensionParameterInParseMode()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Text values</summary>
                                  extension(string {|#0:value|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject), Diagnostics(RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8032MessageFormat));
    }

    #endregion // Tests
}