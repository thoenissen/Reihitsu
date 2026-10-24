using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8202ValueTagMustNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8202ValueTagMustNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH8202ValueTagMustNotBeUsedAnalyzer, RH8202ValueTagMustNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported and fixed for a value tag on a property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    [Requirement("RH8202-002")]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyDiagnosticForValueTagOnProperty()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>
                                  /// {|#0:<value>The current value.</value>|}
                                  internal int Value { get; }
                              }
                              """;
        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>Gets the current value.</summary>
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps a summary tag which shares the line with the value tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyCodeFixKeepsSummaryTagSharingTheLine()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>{|#0:<value>The current value.</value>|}
                                  internal int Value { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>Gets the current value.</summary>
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps the end tag of a preceding element which shares the line with the value tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyCodeFixKeepsPrecedingEndTagSharingTheLine()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Gets the current value.
                                  /// </summary>{|#0:<value>The current value.</value>|}
                                  internal int Value { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>
                                       /// Gets the current value.
                                       /// </summary>
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps the following tag which starts on the last line of the value tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-002")]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyCodeFixKeepsFollowingTagSharingTheLastLine()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>
                                  /// {|#0:<value>
                                  /// The current value.
                                  /// </value>|}<remarks>Never negative.</remarks>
                                  internal int Value { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>Gets the current value.</summary>
                                       /// <remarks>Never negative.</remarks>
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix removes every line of a multi line value tag which owns its lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-002")]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyCodeFixRemovesMultiLineValueTagCompletely()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>
                                  /// {|#0:<value>
                                  /// The current value.
                                  /// </value>|}
                                  internal int Value { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>Gets the current value.</summary>
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps the tags on both sides of the value tag on a shared line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyCodeFixKeepsSurroundingTagsSharingTheLine()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>{|#0:<value>The current value.</value>|}<remarks>Never negative.</remarks>
                                  internal int Value { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>Gets the current value.</summary><remarks>Never negative.</remarks>
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix removes the value tag from a delimited documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyCodeFixRemovesValueTagFromDelimitedDocumentationComment()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /**
                                   * <summary>Gets the current value.</summary>
                                   * {|#0:<value>The current value.</value>|}
                                   */
                                  internal int Value { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /**
                                        * <summary>Gets the current value.</summary>
                                        */
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies no code fix is offered when the value tag has no end tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// Roslyn extends the element span past the trailing line break when the end tag is missing, so the following
    /// declaration would be pulled into the documentation comment
    /// </remarks>
    [TestMethod]
    [Requirement("RH8202-002")]
    [Requirement("RH8202-004")]
    [Requirement("SHARED-DOC-017")]
    public async Task VerifyNoCodeFixWhenEndTagIsMissing()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>
                                  /// <value>The current value.
                                  internal int Value { get; }
                              }
                              """;

        await Verify(source,
                     source,
                     Diagnostic(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId).WithSpan(7, 9, 8, 1)
                                                                                 .WithMessage(AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies no code fix is offered when a value tag without an end tag swallows a following tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-002")]
    [Requirement("RH8202-004")]
    public async Task VerifyNoCodeFixWhenEndTagIsMissingAndFollowingTagIsSwallowed()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <value>The current value.
                                  /// <remarks>Never negative.</remarks>
                                  internal int Value { get; }
                              }
                              """;

        await Verify(source,
                     source,
                     Diagnostic(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId).WithSpan(6, 9, 8, 1)
                                                                                 .WithMessage(AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>
                                  /// {|#0:<value>The current value.</value>|}
                                  internal int Value { get; }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a value tag nested in a remarks element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyDiagnosticForValueTagNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>
                                  /// <remarks>
                                  /// {|#0:<value>The current value.</value>|}
                                  /// </remarks>
                                  internal int Value { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>Gets the current value.</summary>
                                       /// <remarks>
                                       /// </remarks>
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a value tag nested in summary text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyDiagnosticForValueTagNestedInSummaryText()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the {|#0:<value>current</value>|} value.</summary>
                                  internal int Value { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>Gets the value.</summary>
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a value tag which only appears inside a code sample
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    public async Task VerifyNoDiagnosticForValueTagInsideCodeSample()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Gets the current value.
                                  /// <code>
                                  /// <value>Sample tag inside a code block.</value>
                                  /// </code>
                                  /// </summary>
                                  internal int Value { get; }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a value tag which only appears inside an example sample
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    public async Task VerifyNoDiagnosticForValueTagInsideExampleSample()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>
                                  /// <example>
                                  /// <value>Sample tag inside an example.</value>
                                  /// </example>
                                  internal int Value { get; }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a top level value tag is still detected when a code sample also contains one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyTopLevelValueTagIsDetectedBesideCodeSample()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Gets the current value.
                                  /// <code>
                                  /// <value>Sample tag inside a code block.</value>
                                  /// </code>
                                  /// </summary>
                                  /// {|#0:<value>The current value.</value>|}
                                  internal int Value { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>
                                       /// Gets the current value.
                                       /// <code>
                                       /// <value>Sample tag inside a code block.</value>
                                       /// </code>
                                       /// </summary>
                                       internal int Value { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a top-level or nested value element on a property or indexer is reported on the element, while one inside a sample is neither reported nor hides a nested one after it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    [Requirement("RH8202-002")]
    public async Task VerifySpecificationExampleReportsValueElementsOnPropertiesAndIndexers()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C
                              {
                                  /// <summary>
                                  /// Gets the size
                                  /// </summary>
                                  /// {|#0:<value>The size</value>|}
                                  public int Size { get; }

                                  /// <summary>
                                  /// Gets the item
                                  /// </summary>
                                  /// <remarks>{|#1:<value>The item</value>|}</remarks>
                                  private int this[int index] => 0;

                                  /// <summary>
                                  /// Gets the name
                                  /// </summary>
                                  /// <example><code><value>Sample</value></code></example>
                                  public string Name => "";

                                  /// <summary>
                                  /// Gets the weight
                                  /// </summary>
                                  /// <example><code><value>Sample</value></code></example>
                                  /// <remarks>{|#2:<value>Nested</value>|}</remarks>
                                  public int Weight { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the specification example: the fix removes a value element with the lines it owns, with the white space before it on a shared line, and from the remarks element that contains it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-002")]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifySpecificationExampleRemovesValueElement()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C
                              {
                                  /// <summary>
                                  /// Gets the size
                                  /// </summary>
                                  /// {|#0:<value>
                                  /// The size
                                  /// </value>|}
                                  public int Size { get; }

                                  /// <summary>Gets the count</summary> {|#1:<value>The count</value>|}
                                  public int Count { get; }

                                  /// <summary>
                                  /// Gets the item
                                  /// </summary>
                                  /// <remarks>{|#2:<value>The item</value>|}</remarks>
                                  public int this[int index] => 0;
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>
                                   /// Service
                                   /// </summary>
                                   public class C
                                   {
                                       /// <summary>
                                       /// Gets the size
                                       /// </summary>
                                       public int Size { get; }

                                       /// <summary>Gets the count</summary>
                                       public int Count { get; }

                                       /// <summary>
                                       /// Gets the item
                                       /// </summary>
                                       /// <remarks></remarks>
                                       public int this[int index] => 0;
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the specification example: a value element without an end tag is reported through the line break at the end of its line, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-002")]
    [Requirement("RH8202-004")]
    public async Task VerifySpecificationExampleWithholdsFixForValueElementWithoutEndTag()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C
                              {
                                  /// <summary>
                                  /// Gets the size
                                  /// </summary>
                                  /// {|RH8202:<value>The size
                              |}    public int Size { get; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies a value element on an indexer with a set accessor and on a private property is reported, whatever the accessibility
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-001")]
    [Requirement("RH8202-002")]
    public async Task VerifyDiagnosticForValueElementWhateverTheAccessibilityAndAccessors()
    {
        const string source = """
                              internal class C
                              {
                                  /// <summary>Gets or sets the item</summary>
                                  /// {|#0:<value>The item</value>|}
                                  public int this[int index]
                                  {
                                      get => 0;
                                      set
                                      {
                                      }
                                  }

                                  /// <summary>Gets the size</summary>
                                  /// {|#1:<value/>|}
                                  private int Size { get; set; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example of the fix in a document whose line breaks are CRLF: the removed lines and the kept content leave every remaining line break as CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifySpecificationExampleRemovesValueElementInCarriageReturnLineFeedDocument()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       /// <summary>
                                                       /// Service
                                                       /// </summary>
                                                       public class C
                                                       {
                                                           /// <summary>
                                                           /// Gets the size
                                                           /// </summary>
                                                           /// {|#0:<value>
                                                           /// The size
                                                           /// </value>|}
                                                           public int Size { get; }

                                                           /// <summary>Gets the count</summary> {|#1:<value>The count</value>|}
                                                           public int Count { get; }

                                                           /// <summary>
                                                           /// Gets the item
                                                           /// </summary>
                                                           /// <remarks>{|#2:<value>The item</value>|}</remarks>
                                                           public int this[int index] => 0;
                                                       }
                                                       """);

        var fixedSource = NormalizeToCarriageReturnLineFeed("""
                                                            /// <summary>
                                                            /// Service
                                                            /// </summary>
                                                            public class C
                                                            {
                                                                /// <summary>
                                                                /// Gets the size
                                                                /// </summary>
                                                                public int Size { get; }

                                                                /// <summary>Gets the count</summary>
                                                                public int Count { get; }

                                                                /// <summary>
                                                                /// Gets the item
                                                                /// </summary>
                                                                /// <remarks></remarks>
                                                                public int this[int index] => 0;
                                                            }
                                                            """);

        await Verify(source, fixedSource, Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat, 3));
    }

    /// <summary>
    /// Verifies an empty <c>value</c> element needs no end tag, so the code fix is offered and removes its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8202-003")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyCodeFixRemovesEmptyValueElement()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Gets the size
                                  /// </summary>
                                  /// {|#0:<value/>|}
                                  public int Size { get; }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       /// <summary>
                                       /// Gets the size
                                       /// </summary>
                                       public int Size { get; }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a <c>value</c> element without an end tag: it has no end tag of its own, so
    /// it is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8202-004")]
    public async Task VerifyNoCodeFixForValueElementWithoutEndTagOfItsOwn()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Gets the size
                                  /// </summary>
                                  /// <value>The size
                                  public int Size { get; }
                              }
                              """;

        await Verify(source,
                     source,
                     Diagnostic(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId).WithSpan(6, 9, 7, 1)
                                                                                 .WithMessage(AnalyzerResources.RH8202MessageFormat));
    }

    /// <summary>
    /// Verifies that no code fix is offered for a <c>value</c> element that the end tag of the enclosing
    /// <c>remarks</c> element closes, so that end tag is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8202-004")]
    public async Task VerifyNoCodeFixForValueElementClosedByEndTagOfTheEnclosingElement()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Gets the size
                                  /// </summary>
                                  /// <remarks>{|RH8202:<value>The size</remarks>|}
                                  public int Size { get; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that end-tag names are compared exactly, so no code fix is offered for a <c>Value</c> element that
    /// <c>&lt;/value&gt;</c> closes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8202-004")]
    public async Task VerifyNoCodeFixForValueElementClosedByEndTagDifferingOnlyInCase()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Gets the size
                                  /// </summary>
                                  /// {|RH8202:<Value>The size</value>|}
                                  public int Size { get; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>
                                  /// {|#0:<value>The current value.</value>|}
                                  internal int Value { get; }

                                  /// <summary>Gets the previous value.</summary>{|#1:<value>The previous value.</value>|}
                                  internal int PreviousValue { get; }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>Stores the current value.</summary>
                                   internal class TestClass
                                   {
                                       /// <summary>Gets the current value.</summary>
                                       internal int Value { get; }

                                       /// <summary>Gets the previous value.</summary>
                                       internal int PreviousValue { get; }
                                   }
                                   """;

        // Verifies multiple value tags are detected and fixed together
        return new FixAllScenario(source,
                                  fixedSource,
                                  Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}