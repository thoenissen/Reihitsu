using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer"/> and <see cref="RH8308NoContentShouldAppearAfterClosingXmlTagsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzerTests : BatchCodeFixTestsBase<RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer, RH8308NoContentShouldAppearAfterClosingXmlTagsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that text on the next documentation line does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8308-002")]
    public async Task VerifyNoDiagnosticsWhenTextStartsOnNextLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Summary.</summary>
                                    /// Additional text
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that another XML element after a closing tag is not reported by this analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenAnotherElementFollowsClosingTag()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Summary.</summary><remarks>More details.</remarks>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that inline nested XML content remains allowed inside a larger element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8308-002")]
    public async Task VerifyNoDiagnosticsForNestedInlineXmlContent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Uses <see cref="string"/> values.</summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that text after a closing tag is detected and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8308-001")]
    [Requirement("RH8308-003")]
    [Requirement("RH8308-004")]
    public async Task VerifyTextAfterClosingTagIsDetectedAndRemoved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Summary.</summary> {|#0:Additional text|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>Summary.</summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId, AnalyzerResources.RH8308MessageFormat));
    }

    /// <summary>
    /// Verifies that text after a self-closing tag is detected and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8308-001")]
    [Requirement("RH8308-003")]
    [Requirement("RH8308-004")]
    public async Task VerifyTextAfterSelfClosingTagIsDetectedAndRemoved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <inheritdoc/> {|#0:Additional text|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <inheritdoc/>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId, AnalyzerResources.RH8308MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8308-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// <summary>Summary.</summary> Additional text
                                  internal void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that text after a top-level element on the line where the element ends is reported from its first
    /// character that is not white space, after an end tag and after an empty element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8308-001")]
    [Requirement("RH8308-003")]
    public async Task VerifyTextAfterTopLevelElementsIsReported()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary> {|#0:Additional text|}
                                    void M()
                                    {
                                    }

                                    /// <summary>Stops</summary>   {|#1:now|}
                                    void N()
                                    {
                                    }

                                    /// <inheritdoc/> {|#2:inherited|}
                                    int A;
                                }
                                """;

        await Verify(testData, Diagnostics(RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId, AnalyzerResources.RH8308MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that text on a later line, text inside an element, and text in a delimited documentation comment are
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8308-002")]
    public async Task VerifyTextOnLaterLineInsideElementOrInDelimitedCommentDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Runs</summary>
                                    /// Text on the next line
                                    void M()
                                    {
                                    }

                                    /// <summary>
                                    /// Stops <c>now</c> quickly
                                    /// </summary>
                                    void N()
                                    {
                                    }

                                    /** <summary>Value</summary> text */
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix removes each reported text with the white space in front of it, also on both sides of a
    /// top-level element that follows the text on the same line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8308-001")]
    [Requirement("RH8308-003")]
    [Requirement("RH8308-004")]
    public async Task VerifyFixRemovesTheText()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary> {|#0:Additional text|}
                                    void M()
                                    {
                                    }

                                    /// <summary>Stops</summary> {|#1:a |}<c>x</c> {|#2:b|}
                                    void N()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     void M()
                                     {
                                     }

                                     /// <summary>Stops</summary><c>x</c>
                                     void N()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId, AnalyzerResources.RH8308MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the fix also removes a tab in front of the reported text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8308-004")]
    public async Task VerifyFixRemovesTabInFrontOfTheText()
    {
        var testData = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   /// <summary>Stops</summary>→{|#0:now|}
                                                   void N()
                                                   {
                                                   }
                                               }
                                               """);
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>Stops</summary>
                                     void N()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId, AnalyzerResources.RH8308MessageFormat));
    }

    /// <summary>
    /// Verifies that a documentation comment that starts behind code on its line is judged as if it started its own line,
    /// and that the fix leaves the comment behind that code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyTextAfterClosingTagInCommentBehindCodeIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    int A; /// <summary>
                                    /// Value
                                    /// </summary> {|#0:trailing|}
                                    int B;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int A; /// <summary>
                                     /// Value
                                     /// </summary>
                                     int B;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId, AnalyzerResources.RH8308MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// <summary>Summary.</summary> {|#0:Additional text|}
                                  /// <remarks>Remarks.</remarks> {|#1:More text|}
                                  void Method()
                                  {
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       /// <summary>Summary.</summary>
                                       /// <remarks>Remarks.</remarks>
                                       void Method()
                                       {
                                       }
                                   }
                                   """;

        // The two trailing-text runs sit on different documentation lines, so the two deletions are disjoint
        // and neither shifts the other's span
        return new FixAllScenario(source, fixedSource, Diagnostics(RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId, AnalyzerResources.RH8308MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}