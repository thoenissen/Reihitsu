using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline regression tests for XML documentation element content line alignment
/// </summary>
[TestClass]
public class DocumentationElementTextLineAlignmentFullPipelineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a single-content-line documentation element is collapsed to one line
    /// </summary>
    [TestMethod]
    public void CollapsesSingleContentLineElement()
    {
        const string input = """
                             internal class TestClass
                             {
                                 /// <param name="value">The value
                                 /// </param>
                                 void Method(string value)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /// <param name="value">The value</param>
                                    void Method(string value)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multi-line content is moved below the opening tag
    /// </summary>
    [TestMethod]
    public void MovesMultilineContentBelowOpeningTag()
    {
        const string input = """
                             internal class TestClass
                             {
                                 /// <remarks>First line
                                 /// Second line
                                 /// </remarks>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /// <remarks>
                                    /// First line
                                    /// Second line
                                    /// </remarks>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank separator line inside a code element survives line alignment
    /// </summary>
    [TestMethod]
    public void PreservesBlankSeparatorLineInsideCodeElement()
    {
        const string input = """
                             internal class TestClass
                             {
                                 /// <code>var a = 1;
                                 ///
                                 /// var b = 2;
                                 /// </code>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /// <code>
                                    /// var a = 1;
                                    ///
                                    /// var b = 2;
                                    /// </code>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank separator line between paragraphs survives line alignment
    /// </summary>
    [TestMethod]
    public void PreservesBlankSeparatorLineInsideRemarksElement()
    {
        const string input = """
                             internal class TestClass
                             {
                                 /// <remarks>First paragraph.
                                 ///
                                 /// Second paragraph.
                                 /// </remarks>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /// <remarks>
                                    /// First paragraph.
                                    ///
                                    /// Second paragraph.
                                    /// </remarks>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that summary elements remain expanded rather than being collapsed
    /// </summary>
    [TestMethod]
    public void KeepsSummaryElementExpanded()
    {
        const string input = """
                             internal class TestClass
                             {
                                 /// <summary>First line
                                 /// Second line
                                 /// </summary>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// First line
                                    /// Second line
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that leading inline XML content is moved below the opening tag
    /// </summary>
    [TestMethod]
    public void MovesLeadingInlineXmlContentBelowOpeningTag()
    {
        const string input = """
                             internal class TestClass
                             {
                                 /// <summary><see cref="string"/>
                                 /// values</summary>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// <see cref="string"/>
                                    /// values
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}