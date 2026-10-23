using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer"/> and <see cref="RH8304XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzerTests : BatchCodeFixTestsBase<RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer, RH8304XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that direct XML documentation elements on separate lines do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-001")]
    public async Task VerifyNoDiagnosticsWhenElementsAreAlreadySeparated()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Summary.
                                    /// </summary>
                                    /// <param name="value">Value.</param>
                                    void Method(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that direct XML documentation elements on the same line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-001")]
    [Requirement("RH8304-003")]
    [Requirement("RH8304-004")]
    public async Task VerifyElementsOnSameLineAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Summary.
                                    /// </summary>{|#0:<param name="value">Value.</param>|}
                                    void Method(string value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Summary.
                                     /// </summary>
                                     /// <param name="value">Value.</param>
                                     void Method(string value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8304MessageFormat));
    }

    /// <summary>
    /// Verifies that moving an element to its own line uses only the documentation exterior as the prefix and does not duplicate the leading sentence text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-001")]
    [Requirement("RH8304-003")]
    [Requirement("RH8304-004")]
    public async Task VerifyElementMoveDoesNotDuplicateLeadingSentenceText()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// Intro <summary>Summary.</summary> {|#0:<remarks>Remarks.</remarks>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// Intro <summary>Summary.</summary>
                                     /// <remarks>Remarks.</remarks>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8304MessageFormat));
    }

    /// <summary>
    /// Verifies that nested inline XML elements do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-002")]
    public async Task VerifyNestedInlineElementsDoNotProduceDiagnostics()
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
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Summary.
                                  /// </summary><param name="value">Value.</param>
                                  internal void Execute(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that elements starting on the line where the previous top-level element ends are reported, after
    /// nothing, after spaces, after a character reference that stands for white space, and as the second and the third
    /// element of one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-001")]
    [Requirement("RH8304-003")]
    public async Task VerifyElementsOnTheLineOfThePreviousElementAreReported()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Runs</summary>{|#0:<remarks>Remarks</remarks>|}
                                    void M()
                                    {
                                    }

                                    /// <summary>Stops</summary> {|#1:<remarks>Remarks</remarks>|} {|#2:<example>Example</example>|}
                                    void N()
                                    {
                                    }

                                    /// <summary>
                                    /// Value
                                    /// </summary>{|#3:<inheritdoc/>|}
                                    int A;

                                    /// <summary>Count</summary>&#32;{|#4:<remarks>Remarks</remarks>|}
                                    int B;
                                }
                                """;

        await Verify(testData, Diagnostics(RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8304MessageFormat, 5));
    }

    /// <summary>
    /// Verifies that elements separated by text, inline elements, an XML comment, and the elements of a delimited
    /// documentation comment are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-002")]
    public async Task VerifySeparatedNestedAndDelimitedElementsDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Runs</summary> and <remarks>Remarks</remarks>
                                    void M()
                                    {
                                    }

                                    /// <summary>Stops <see cref="C"/> <c>now</c></summary>
                                    void N()
                                    {
                                    }

                                    /// <summary>Value</summary><!-- Note --><remarks>Remarks</remarks>
                                    int A;

                                    /** <summary>Count</summary><remarks>Remarks</remarks> */
                                    int B;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a character reference that does not stand for white space separates two elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-002")]
    public async Task VerifyElementsSeparatedByNonWhitespaceCharacterReferenceDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Runs</summary>&amp;<remarks>Remarks</remarks>
                                    void M()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix moves each reported element onto a new documentation line with the indentation of its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-004")]
    public async Task VerifyFixMovesEachElementOntoANewDocumentationLine()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Stops</summary> {|#0:<remarks>Remarks</remarks>|} {|#1:<example>Example</example>|}
                                    void N()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>Stops</summary>
                                     /// <remarks>Remarks</remarks>
                                     /// <example>Example</example>
                                     void N()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8304MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix inserts the CRLF line break of the element's line in a CRLF document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8304-004")]
    public async Task VerifyFixInsertsTheLineBreakOfTheLineWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Stops</summary> <remarks>Remarks</remarks>
                                    void N()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>Stops</summary>
                                     /// <remarks>Remarks</remarks>
                                     void N()
                                     {
                                     }
                                 }
                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), actual);
    }

    /// <summary>
    /// Verifies that a documentation comment that starts behind code on its line is judged as if it started its own line,
    /// and that the fix leaves the comment behind that code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyElementsOnSameLineInCommentBehindCodeAreDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    int A; /// <summary>
                                    /// Value
                                    /// </summary> {|#0:<remarks>x</remarks>|}
                                    int B;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int A; /// <summary>
                                     /// Value
                                     /// </summary>
                                     /// <remarks>x</remarks>
                                     int B;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8304MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Summary.
                                  /// </summary>{|#0:<param name="first">First.</param>|}{|#1:<param name="second">Second.</param>|}
                                  void Method(string first, string second)
                                  {
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       /// <summary>
                                       /// Summary.
                                       /// </summary>
                                       /// <param name="first">First.</param>
                                       /// <param name="second">Second.</param>
                                       void Method(string first, string second)
                                       {
                                       }
                                   }
                                   """;

        // Both elements are inserted onto their own line from the same original line, so the incremental and
        // batch prefixes agree even though the second insertion point shifts once the first fix is applied
        return new FixAllScenario(source, fixedSource, Diagnostics(RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8304MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}