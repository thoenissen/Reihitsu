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
public class RH8202ValueTagMustNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH8202ValueTagMustNotBeUsedAnalyzer, RH8202ValueTagMustNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported and fixed for a value tag on a property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies multiple value tags are detected and fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyValueTagsAreFixedTogether()
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

        await Verify(source,
                     fixedSource,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8202MessageFormat, 2));
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

    #endregion // Tests
}