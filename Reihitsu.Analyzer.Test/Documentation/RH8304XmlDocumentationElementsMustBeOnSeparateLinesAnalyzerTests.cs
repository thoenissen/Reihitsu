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
public class RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzerTests : AnalyzerTestsBase<RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer, RH8304XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that direct XML documentation elements on separate lines do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}