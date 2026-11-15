using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for a documentation-comment syntax (<c>/** */</c>, <c>///</c>) behind a comma that starts a line, in an
/// argument list (FMT-BREAK-012) and in an initializer or a collection expression (FMT-BREAK-038). In the documentation mode
/// <c>None</c> the comment trails the comma and moves with it; in the documentation modes <c>Parse</c> and <c>Diagnose</c> it is
/// a documentation comment, which goes onto a line of its own in front of the element behind the comma, as for the same code
/// written comma-last
/// </summary>
[TestClass]
public class LeadingCommaDocumentationCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a <c>/** */</c> comment behind an argument comma that starts a line goes onto a line of its own in front
    /// of the argument behind the comma, with a blank line above it, in the documentation modes that parse documentation
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void DelimitedDocumentationCommentBehindALeadingArgumentCommaLeadsTheArgument(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , /** c */ 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1,

                                            /** c */ 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that a <c>///</c> comment behind an argument comma that starts a line stays on a line of its own in front of
    /// the argument behind the comma, with a blank line above it, in the documentation modes that parse documentation
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void SingleLineDocumentationCommentBehindALeadingArgumentCommaLeadsTheArgument(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , /// c
                                         2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1,

                                            /// c
                                            2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that an argument list with a <c>/** */</c> comment behind a comma gives the same output written comma-first and
    /// comma-last in the documentation mode <c>Parse</c>
    /// </summary>
    /// <param name="input">The argument list, written comma-first or comma-last</param>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    [DataRow("Foo(1\n            , /** c */ 2,\n            3);", DisplayName = "Comma-first")]
    [DataRow("Foo(1, /** c */ 2,\n            3);", DisplayName = "Comma-last")]
    public void DelimitedDocumentationCommentBehindAnArgumentCommaIsPlacedAsForCommaLastCode(string input)
    {
        // Arrange
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1,

                                            /** c */ 2,
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult($"class C\n{{\n    void M()\n    {{\n        {input}\n    }}\n}}", expected, CreateParseOptions(DocumentationMode.Parse));
    }

    /// <summary>
    /// Verifies that in the documentation mode <c>None</c> a <c>/** */</c> comment behind an argument comma that starts a line
    /// moves with the comma and stays behind it, as every comment the parser lets trail the comma does
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void DelimitedDocumentationCommentSyntaxBehindALeadingArgumentCommaMovesWithTheCommaInDocumentationModeNone()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , /** c */ 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1, /** c */
                                            2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies that in the documentation mode <c>None</c> a <c>///</c> comment behind an argument comma that starts a line
    /// moves with the comma and keeps ending its line, as every comment the parser lets trail the comma does
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void SingleLineDocumentationCommentSyntaxBehindALeadingArgumentCommaMovesWithTheCommaInDocumentationModeNone()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , /// c
                                         2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1, /// c
                                            2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies the example of FMT-BREAK-038 for the documentation modes that parse documentation: a <c>/** */</c> comment
    /// behind an object-initializer comma that starts a line goes onto the line of the member behind the comma, with a blank
    /// line above it
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void DelimitedDocumentationCommentBehindALeadingInitializerCommaLeadsTheMember(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var b = new D { X = 3
                                         , /** c */ Y = 4 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var b = new D
                                                {
                                                    X = 3,

                                                    /** c */ Y = 4
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that a <c>///</c> comment behind an object-initializer comma that starts a line stays on a line of its own in
    /// front of the member behind the comma, with a blank line above it, in the documentation modes that parse documentation
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void SingleLineDocumentationCommentBehindALeadingInitializerCommaLeadsTheMember(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var e = new D { X = 3
                                         , /// c
                                         Y = 4 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var e = new D
                                                {
                                                    X = 3,

                                                    /// c
                                                    Y = 4
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that an object initializer and a collection expression with a <c>/** */</c> comment behind a comma give the
    /// same output written comma-first and comma-last in the documentation mode <c>Parse</c>
    /// </summary>
    /// <param name="initializer">The object initializer, written comma-first or comma-last</param>
    /// <param name="collection">The collection expression, written the same way</param>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    [DataRow("new D { X = 3\n            , /** c */ Y = 4 };", "[1\n            , /** c */ 2];", DisplayName = "Comma-first")]
    [DataRow("new D { X = 3, /** c */ Y = 4 };", "[1, /** c */ 2];", DisplayName = "Comma-last")]
    public void DelimitedDocumentationCommentBehindAnInitializerCommaIsPlacedAsForCommaLastCode(string initializer, string collection)
    {
        // Arrange
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var b = new D
                                                {
                                                    X = 3,

                                                    /** c */ Y = 4
                                                };
                                        int[] d = [
                                                      1,

                                                      /** c */ 2
                                                  ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult($"class C\n{{\n    void M()\n    {{\n        var b = {initializer}\n        int[] d = {collection}\n    }}\n}}", expected, CreateParseOptions(DocumentationMode.Parse));
    }

    /// <summary>
    /// Verifies the example of FMT-BREAK-038 for the documentation mode <c>None</c>: a <c>/** */</c> comment behind an
    /// object-initializer comma that starts a line moves with the comma and stays behind it, and the member behind it starts
    /// its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void DelimitedDocumentationCommentSyntaxBehindALeadingInitializerCommaMovesWithTheCommaInDocumentationModeNone()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var b = new D { X = 3
                                         , /** c */ Y = 4 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var b = new D
                                                {
                                                    X = 3, /** c */
                                                    Y = 4
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies that in the documentation mode <c>None</c> a <c>///</c> comment behind an object-initializer comma that starts
    /// a line moves with the comma and keeps ending its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void SingleLineDocumentationCommentSyntaxBehindALeadingInitializerCommaMovesWithTheCommaInDocumentationModeNone()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var e = new D { X = 3
                                         , /// c
                                         Y = 4 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var e = new D
                                                {
                                                    X = 3, /// c
                                                    Y = 4
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CreateParseOptions(DocumentationMode.None));
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