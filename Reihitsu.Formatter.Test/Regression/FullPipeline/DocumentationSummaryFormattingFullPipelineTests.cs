using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline regression tests for XML documentation summary formatting
/// </summary>
[TestClass]
public class DocumentationSummaryFormattingFullPipelineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that single-line summary elements are expanded to three lines
    /// </summary>
    [TestMethod]
    public void ExpandsSingleLineSummaryElement()
    {
        const string input = """
                             internal class TestClass
                             {
                                 /// <summary>Summary text</summary>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Summary text
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that inline XML content remains on the same content line when the summary is expanded
    /// </summary>
    [TestMethod]
    public void ExpandsSingleLineSummaryElementWithInlineXmlContent()
    {
        const string input = """
                             internal class TestClass
                             {
                                 /// <summary>Uses <see cref="string"/> values</summary>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Uses <see cref="string"/> values
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that indentation used for nested list items inside a summary is preserved
    /// </summary>
    [TestMethod]
    public void PreservesNestedListIndentation()
    {
        const string input = """
                             /// <summary>
                             /// Supported modes:
                             /// - Standard
                             ///   - Fast
                             ///   - Safe
                             /// - Advanced
                             /// </summary>
                             public sealed class Options
                             {
                                 public int Count { get; } = 2;
                             }
                             """;

        AssertRuleResult(input, input);
    }

    #endregion // Methods
}