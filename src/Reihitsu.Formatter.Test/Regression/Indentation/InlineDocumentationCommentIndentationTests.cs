using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Regression tests verifying that a documentation comment which shares its line with preceding code does not change the
/// indentation of that line, while a comment that starts its own line is still aligned to the code it precedes
/// </summary>
[TestClass]
public class InlineDocumentationCommentIndentationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a delimited documentation comment behind the opening brace of a type leaves the brace at its column
    /// </summary>
    [TestMethod]
    public void KeepsOpeningBraceColumnWithDelimitedDocumentationBehindIt()
    {
        // Arrange
        const string input = """
                             public class TestClass
                             { /** Doc */
                                 public string Description { get; set; }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a switch label leaves the label at its column
    /// </summary>
    [TestMethod]
    public void KeepsCaseLabelColumnWithDelimitedDocumentationBehindIt()
    {
        // Arrange
        const string input = """
                             public class TestClass
                             {
                                 public void M(int v)
                                 {
                                     switch (v)
                                     {
                                         case 1: /** Doc */
                                             M(2);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment stranded behind a wrapped argument before a closing brace leaves
    /// the argument aligned
    /// </summary>
    [TestMethod]
    public void KeepsWrappedArgumentAlignedWithStrandedDelimitedDocumentationBehindIt()
    {
        // Arrange
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     Call(1,
                                          2); /** Doc */
                                 }

                                 public void Call(int a, int b)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment on its own line below an opening brace is indented to the column
    /// of the member it documents
    /// </summary>
    [TestMethod]
    public void IndentsDelimitedDocumentationOnOwnLineBelowOpeningBrace()
    {
        // Arrange
        const string input = """
                             public class TestClass
                             {
                             /** Doc */
                                 public string Description { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    /** Doc */
                                    public string Description { get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}