using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests covering documentation comments whose normalization candidates are nested inside one
/// another, and the <c>///</c> exterior of a comment written at a member's own indentation. Both
/// used to need a second pipeline run to reach their fixed point
/// </summary>
[TestClass]
public class DocumentationCommentNestedNormalizationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a code element nested inside a remarks element is normalized in the same run
    /// as the element that contains it
    /// </summary>
    [TestMethod]
    public void NestedCodeElementIsNormalizedInTheSameRun()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Text
                                 /// </summary>
                                 /// <remarks>Lead
                                 /// <code>first
                                 /// second</code>
                                 /// </remarks>
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Text
                                    /// </summary>
                                    /// <remarks>
                                    /// Lead
                                    /// <code>
                                    /// first
                                    /// second
                                    /// </code>
                                    /// </remarks>
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a candidate nested two levels deep is normalized in the same run
    /// </summary>
    [TestMethod]
    public void TwiceNestedElementIsNormalizedInTheSameRun()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Text
                                 /// </summary>
                                 /// <remarks>Outer
                                 /// <example>Middle
                                 /// <code>first
                                 /// second</code>
                                 /// </example>
                                 /// </remarks>
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Text
                                    /// </summary>
                                    /// <remarks>
                                    /// Outer
                                    /// <example>
                                    /// Middle
                                    /// <code>
                                    /// first
                                    /// second
                                    /// </code>
                                    /// </example>
                                    /// </remarks>
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single, unnested candidate is still normalized — the other side of the
    /// boundary the nesting filter is taken on
    /// </summary>
    [TestMethod]
    public void UnnestedElementIsNormalized()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Text
                                 /// </summary>
                                 /// <remarks>Lead
                                 /// more
                                 /// </remarks>
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Text
                                    /// </summary>
                                    /// <remarks>
                                    /// Lead
                                    /// more
                                    /// </remarks>
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation exterior written at a member's own indentation gains its
    /// separating space. The prefix pattern used to anchor at the line start, so only a comment at
    /// column zero was ever normalized
    /// </summary>
    [TestMethod]
    public void IndentedDocumentationExteriorGainsItsSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 ///<summary>
                                 ///Text
                                 ///</summary>
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Text
                                    /// </summary>
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation exterior at column zero gains its separating space
    /// </summary>
    [TestMethod]
    public void ColumnZeroDocumentationExteriorGainsItsSpace()
    {
        // Arrange
        const string input = """
                             ///<summary>
                             ///Text
                             ///</summary>
                             class C
                             {
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                /// <summary>
                                /// Text
                                /// </summary>
                                class C
                                {
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}