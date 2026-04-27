using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0384XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer"/> and <see cref="RH0384XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0384XmlDocumentationElementsMustBeOnSeparateLinesAnalyzerTests : AnalyzerTestsBase<RH0384XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer, RH0384XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider>
{
    /// <summary>
    /// Verifies that direct XML documentation elements on separate lines do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
    /// Verifies that direct XML documentation elements on the same line are detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        await Verify(testData, fixedData, Diagnostics(RH0384XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
    }

    /// <summary>
    /// Verifies that nested inline XML elements do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
}