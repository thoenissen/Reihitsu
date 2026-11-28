using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Full-pipeline tests that the removal of a trailing comma keeps a comment written on the last element's line in front of
/// the comma behind the last element, whichever token's trivia carries it (FMT-STRUCT-019, FMT-STRUCT-020): in the
/// documentation modes <c>Parse</c> and <c>Diagnose</c> a <c>/** */</c> comment there is the comma's leading trivia, in the
/// documentation mode <c>None</c> the last element's trailing trivia
/// </summary>
[TestClass]
public class TrailingCommaDocumentationCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the specification example of FMT-STRUCT-019: a documentation comment and a block comment in front of an
    /// enum's trailing comma stay behind the last member in every documentation mode
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-019")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    [DataRow(DocumentationMode.None, DisplayName = "None")]
    public void SpecificationExampleCommentsInFrontOfTheEnumTrailingCommaStayBehindTheLastMember(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             enum Size
                             {
                                 Small,
                                 Large /** largest */,
                             }

                             enum Mode
                             {
                                 A,
                                 B /* last */,
                             }
                             """;
        const string expected = """
                                enum Size
                                {
                                    Small,
                                    Large /** largest */
                                }

                                enum Mode
                                {
                                    A,
                                    B /* last */
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that a documentation comment in front of an enum's trailing comma stays behind the last member and in front
    /// of the line comment that stood behind the comma, in every documentation mode
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-019")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    [DataRow(DocumentationMode.None, DisplayName = "None")]
    public void DocumentationCommentInFrontOfTheEnumTrailingCommaStaysInFrontOfTheCommasLineComment(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             enum E
                             {
                                 A,
                                 B /** doc B */, // after
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A,
                                    B /** doc B */ // after
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that a documentation comment in front of an array initializer's trailing comma stays behind the last element,
    /// in an initializer on one line and in one whose last element starts a line of its own, in every documentation mode
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-020")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    [DataRow(DocumentationMode.None, DisplayName = "None")]
    public void DocumentationCommentInFrontOfTheArrayInitializerTrailingCommaStaysBehindTheLastElement(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int[] _a = { 1, 2 /** two */, };

                                 void M()
                                 {
                                     var m = new[] { 1,
                                         2 /** two */, };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] _a = { 1, 2 /** two */ };

                                    void M()
                                    {
                                        var m = new[]
                                                {
                                                    1,
                                                    2 /** two */
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that a documentation comment in front of a collection initializer's trailing comma stays behind the last
    /// element, in every documentation mode
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-020")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    [DataRow(DocumentationMode.None, DisplayName = "None")]
    public void DocumentationCommentInFrontOfTheCollectionInitializerTrailingCommaStaysBehindTheLastElement(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var l = new List<int> { 1 /** one */, };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var l = new List<int>
                                                {
                                                    1 /** one */
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that removing an enum's trailing comma with a documentation comment in front of it and a comment line between
    /// the comma's line and the closing brace gives the comma-less text's output in one pass, without a blank line in front of
    /// the comment line, in the documentation modes that make the documentation comment the comma's leading trivia
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-019")]
    [Requirement("FMT-PIPE-004")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void DocumentationCommentInFrontOfTheEnumTrailingCommaStaysBehindTheLastMemberInFrontOfACommentLine(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             enum E
                             {
                                 A,
                                 B /** doc B */,
                                 // note
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A,
                                    B /** doc B */
                                    // note
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that removing an array initializer's trailing comma with a documentation comment in front of it and a comment
    /// line between the comma's line and the closing brace gives the comma-less text's output in one pass, without a blank line
    /// in front of the comment line, in the documentation modes that make the documentation comment the comma's leading trivia
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-020")]
    [Requirement("FMT-PIPE-004")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void DocumentationCommentInFrontOfTheArrayInitializerTrailingCommaStaysBehindTheLastElementInFrontOfACommentLine(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int[] _c = {
                                                1,
                                                2 /** two */,
                                                // note
                                            };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] _c = {
                                                   1,
                                                   2 /** two */
                                                   // note
                                               };
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that removing a collection initializer's trailing comma with a documentation comment in front of it and a
    /// comment line between the comma's line and the closing brace gives the comma-less text's output in one pass, without a
    /// blank line in front of the comment line, in the documentation modes that make the documentation comment the comma's
    /// leading trivia
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-020")]
    [Requirement("FMT-PIPE-004")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void DocumentationCommentInFrontOfTheCollectionInitializerTrailingCommaStaysBehindTheLastElementInFrontOfACommentLine(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var l = new List<int>
                                             {
                                                 1,
                                                 2 /** two */,
                                                 // note
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var l = new List<int>
                                                {
                                                    1,
                                                    2 /** two */
                                                    // note
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the parse options of a fixture with the given documentation mode
    /// </summary>
    /// <param name="documentationMode">The documentation mode</param>
    /// <returns>The parse options</returns>
    private static CSharpParseOptions CreateParseOptions(DocumentationMode documentationMode)
    {
        return CSharpParseOptions.Default.WithDocumentationMode(documentationMode);
    }

    #endregion // Methods
}