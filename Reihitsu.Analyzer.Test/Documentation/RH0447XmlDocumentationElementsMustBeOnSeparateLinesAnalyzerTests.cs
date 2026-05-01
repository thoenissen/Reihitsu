using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH0447XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer"/> and <see cref="RH0447XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0447XmlDocumentationElementsMustBeOnSeparateLinesAnalyzerTests : AnalyzerTestsBase<RH0447XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer, RH0447XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider>
{
    #region Members

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

        await Verify(testData, fixedData, Diagnostics(RH0447XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH0447MessageFormat));
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

    #endregion // Members
}