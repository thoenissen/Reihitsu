using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing extension declaration type parameter comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("RH8033-002")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyDiagnosticForMissingTypeParameterDocumentationOnExtensionDeclaration()
    {
        const string source = """
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  extension<{|#0:T|}>(IEnumerable<T> values)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8033MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for documented extension declaration type parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyNoDiagnosticsForDocumentedTypeParameterOnExtensionDeclaration()
    {
        const string source = """
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  /// <typeparam name="T">The element type.</typeparam>
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
    [Requirement("RH8033-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  extension<{|#0:T|}>(IEnumerable<T> values)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: a type parameter of a documented generic extension declaration without a matching typeparam element is reported, and a non-generic declaration is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("RH8033-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedExtensionTypeParameter()
    {
        const string source = """
                              using System.Collections.Generic;

                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  /// <summary>
                                  /// Adds helpers to sequences
                                  /// </summary>
                                  /// <param name="values">Values</param>
                                  extension<{|#0:T|}>(IEnumerable<T> values)
                                  {
                                  }

                                  /// <summary>
                                  /// Adds helpers to lists
                                  /// </summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="values">Values</param>
                                  extension<T>(List<T> values)
                                  {
                                  }

                                  /// <summary>
                                  /// Adds helpers to text values
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8033MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a type parameter is documented only by a top-level typeparam element whose name equals the type parameter name without its at sign
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("RH8033-002")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifySpecificationExampleTypeParameterIsDocumentedByTopLevelElementWithItsName()
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
                                  extension(int value)
                                  {
                                  }

                                  /// <summary>
                                  /// Long values
                                  /// </summary>
                                  /// <remarks>
                                  /// <param name="value">Value</param>
                                  /// </remarks>
                                  extension(long value)
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
                                  extension<TKey, {|#0:TValue|}>(Dictionary<TKey, TValue> pairs)
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
                                  extension(char @class)
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
                                  extension<{|#1:@T|}>(HashSet<T> items)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8033MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example: a typeparam element brought in by an include element documents the type parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifySpecificationExampleTypeParameterIsDocumentedByIncludedFile()
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

                                  /// <include file='docs.xml' path='doc/members/member[@name="B"]/*'/>
                                  extension<{|#0:T|}>(List<T> values)
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

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8033MessageFormat));
    }

    /// <summary>
    /// Verifies no type parameter is reported for an undocumented generic extension declaration or one with a top-level inheritdoc in its first, a later, or an included documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("SHARED-DOC-011")]
    public async Task VerifyTypeParametersAreCheckedOnlyWithoutInheritdoc()
    {
        const string source = """
                              using System.Collections.Generic;

                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  extension<T>(T[] values)
                                  {
                                  }

                                  /// <inheritdoc/>
                                  extension<T>(List<T> values)
                                  {
                                  }

                                  /// <remarks>Remarks</remarks>

                                  /// <inheritdoc/>
                                  extension<T>(HashSet<T> values)
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="C"]/*'/>
                                  extension<T>(Stack<T> values)
                                  {
                                  }

                                  /// <summary>Queues</summary>
                                  /// <param name="values">Values</param>
                                  extension<{|#0:T|}>(Queue<T> values)
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

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8033MessageFormat));
    }

    /// <summary>
    /// Verifies every type parameter without a top-level typeparam element of the same name, compared case-sensitively, is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("RH8033-002")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyDiagnosticForEachUndocumentedTypeParameter()
    {
        const string source = """
                              using System.Collections.Generic;

                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Dictionaries</summary>
                                  /// <param name="pairs">Pairs</param>
                                  extension<{|#0:TKey|}, {|#1:TValue|}>(Dictionary<TKey, TValue> pairs)
                                  {
                                  }

                                  /// <summary>Lists</summary>
                                  /// <typeparam name="t">Element</typeparam>
                                  /// <param name="values">Values</param>
                                  extension<{|#2:T|}>(List<T> values)
                                  {
                                  }

                                  /// <summary>Stacks</summary>
                                  /// <remarks>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// </remarks>
                                  /// <param name="values">Values</param>
                                  extension<{|#3:T|}>(Stack<T> values)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8033MessageFormat, 4));
    }

    /// <summary>
    /// Verifies extension declarations merged with reordered or differently qualified constraints share their type parameter documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("SHARED-DOC-013")]
    public async Task VerifyMergedExtensionDeclarationsShareTypeParameterDocumentation()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Arrays</summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="values">Values</param>
                                  extension<T>(T[] values) where T : IComparable, IDisposable
                                  {
                                  }

                                  /// <summary>More arrays</summary>
                                  extension<T>(T[] values) where T : IDisposable, IComparable
                                  {
                                  }

                                  /// <summary>Lists</summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="values">Values</param>
                                  extension<T>(List<T> values) where T : class, IDisposable
                                  {
                                  }

                                  /// <summary>More lists</summary>
                                  extension<T>(List<T> values) where T : class, System.IDisposable
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies extension declarations that differ in their type parameter names, constraints, or type parameter attributes do not share their documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("SHARED-DOC-013")]
    public async Task VerifyExtensionDeclarationsWithDifferentTypeParametersDoNotShareDocumentation()
    {
        const string source = """
                              #nullable enable

                              using System;
                              using System.Collections.Generic;

                              /// <summary>Marker</summary>
                              [AttributeUsage(AttributeTargets.All)]
                              public sealed class MarkAttribute : Attribute
                              {
                              }

                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Arrays</summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="values">Values</param>
                                  extension<T>(T[] values)
                                  {
                                  }

                                  /// <summary>Arrays with another type parameter name</summary>
                                  /// <param name="values">Values</param>
                                  extension<{|#0:U|}>(U[] values)
                                  {
                                  }

                                  /// <summary>Lists</summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="values">Values</param>
                                  extension<T>(List<T> values) where T : class
                                  {
                                  }

                                  /// <summary>Lists with a nullable constraint</summary>
                                  /// <param name="values">Values</param>
                                  extension<{|#1:T|}>(List<T> values) where T : class?
                                  {
                                  }

                                  /// <summary>Sets</summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="values">Values</param>
                                  extension<T>(HashSet<T> values)
                                  {
                                  }

                                  /// <summary>Sets with a marked type parameter</summary>
                                  /// <param name="values">Values</param>
                                  extension<[Mark] {|#2:T|}>(HashSet<T> values)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8033MessageFormat, 3));
    }

    /// <summary>
    /// Verifies an undocumented extension type parameter is reported when the documentation mode is parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyDiagnosticForUndocumentedExtensionTypeParameterInParseMode()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Arrays</summary>
                                  /// <param name="values">Values</param>
                                  extension<{|#0:T|}>(T[] values)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject), Diagnostics(RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8033MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of the merge qualifiers: type parameters of extension declarations the compiler merges share their documentation, and differing type parameter names, constraints, or attributes keep them apart
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8033-001")]
    [Requirement("SHARED-DOC-013")]
    public async Task VerifySpecificationExampleMergeQualifiersOfTypeParameters()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              /// <summary>
                              /// Marks a declaration
                              /// </summary>
                              [AttributeUsage(AttributeTargets.All)]
                              public sealed class MarkAttribute : Attribute
                              {
                                  /// <summary>
                                  /// Creates the attribute
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  public MarkAttribute(int value)
                                  {
                                  }
                              }

                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static partial class Extensions
                              {
                                  /// <summary>
                                  /// Lists
                                  /// </summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="values">Values</param>
                                  extension<T>(List<T> values)
                                  {
                                  }

                                  /// <summary>
                                  /// More lists
                                  /// </summary>
                                  extension<T>(List<T> values)
                                  {
                                  }

                                  /// <summary>
                                  /// Arrays
                                  /// </summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="values">Values</param>
                                  extension<T>(T[] values)
                                  {
                                  }

                                  /// <summary>
                                  /// More arrays
                                  /// </summary>
                                  /// <param name="values">Values</param>
                                  extension<{|#0:U|}>(U[] values)
                                  {
                                  }

                                  /// <summary>
                                  /// Sequences
                                  /// </summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="items">Items</param>
                                  extension<T>(IEnumerable<T> items) where T : IComparable, IDisposable
                                  {
                                  }

                                  /// <summary>
                                  /// More sequences
                                  /// </summary>
                                  extension<T>(IEnumerable<T> items) where T : IDisposable, IComparable
                                  {
                                  }

                                  /// <summary>
                                  /// Sets of references
                                  /// </summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="items">Items</param>
                                  extension<T>(ISet<T> items) where T : class
                                  {
                                  }

                                  /// <summary>
                                  /// Sets of values
                                  /// </summary>
                                  /// <param name="items">Items</param>
                                  extension<{|#1:T|}>(ISet<T> items) where T : struct
                                  {
                                  }

                                  /// <summary>
                                  /// Read-only lists
                                  /// </summary>
                                  /// <typeparam name="T">Element</typeparam>
                                  /// <param name="items">Items</param>
                                  extension<T>(IList<T> items)
                                  {
                                  }

                                  /// <summary>
                                  /// Marked read-only lists
                                  /// </summary>
                                  /// <param name="items">Items</param>
                                  extension<[Mark(0)] {|#2:T|}>(IList<T> items)
                                  {
                                  }

                                  /// <summary>
                                  /// Spans
                                  /// </summary>
                                  /// <param name="span">Span</param>
                                  extension(ref Span<int> span)
                                  {
                                  }

                                  /// <summary>
                                  /// Scoped spans
                                  /// </summary>
                                  extension(scoped ref Span<int> span)
                                  {
                                  }

                                  /// <summary>
                                  /// Read-only references
                                  /// </summary>
                                  /// <param name="number">Number</param>
                                  extension(in decimal number)
                                  {
                                  }

                                  /// <summary>
                                  /// Read-only references, declared otherwise
                                  /// </summary>
                                  extension(ref readonly decimal number)
                                  {
                                  }

                                  /// <summary>
                                  /// Marked longs
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension([Mark(1)] long value)
                                  {
                                  }

                                  /// <summary>
                                  /// Longs marked otherwise
                                  /// </summary>
                                  extension([Mark(2)] long value)
                                  {
                                  }

                                  /// <summary>
                                  /// Marked integers
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension([Mark(1)] int value)
                                  {
                                  }

                                  /// <summary>
                                  /// More marked integers
                                  /// </summary>
                                  extension([Mark(1)] int value)
                                  {
                                  }

                                  /// <summary>
                                  /// Single values
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension(float value)
                                  {
                                  }
                              }

                              /// <summary>
                              /// More extensions
                              /// </summary>
                              public static partial class Extensions
                              {
                                  /// <summary>
                                  /// More single values
                                  /// </summary>
                                  extension(float value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8033MessageFormat, 3));
    }

    #endregion // Tests
}