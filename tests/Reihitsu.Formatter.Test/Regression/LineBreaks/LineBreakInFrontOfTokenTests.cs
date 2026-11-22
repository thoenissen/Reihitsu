using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the line break the line-break phase writes in front of a token that has to start its line, keeping a line break that is already there
/// </summary>
[TestClass]
public class LineBreakInFrontOfTokenTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a later call standing behind the closing line of a multi-line comment starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("SHARED-COMMENT-012")]
    public void LaterCallBehindMultiLineCommentTailStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var x = a.B()
                                 /* one
                                    two */ .C()
                                 .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var x = a.B()

                                                 /* one
                                       two */
                                                 .C()
                                                 .D();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the line break inserted behind a comma that ends in whitespace in a wrapped argument list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void ArgumentBehindWhitespaceStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void N(int a, int b, int c) { }

                                 void M()
                                 {
                                     N(1,
                               2,   3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N(int a, int b, int c)
                                    {
                                    }

                                    void M()
                                    {
                                        N(1,
                                          2,
                                          3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the line break inserted behind a block comment that follows a comma in a wrapped argument list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void ArgumentBehindBlockCommentStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void N(int a, int b, int c) { }

                                 void M()
                                 {
                                     N(1,
                               2, /* c */ 3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N(int a, int b, int c)
                                    {
                                    }

                                    void M()
                                    {
                                        N(1,
                                          2, /* c */
                                          3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument that already starts its line keeps its single line break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    [Requirement("FMT-BREAK-030")]
    public void ArgumentAlreadyStartingItsLineKeepsItsLineBreak()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void N(int a, int b, int c) { }

                                 void M()
                                 {
                                     N(1,
                               2,
                             3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N(int a, int b, int c)
                                    {
                                    }

                                    void M()
                                    {
                                        N(1,
                                          2,
                                          3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the line break inserted behind a comma that ends in whitespace in a wrapped constraint clause
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ConstraintBehindWhitespaceStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class,
                                 System.IDisposable,   new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class,
                                                  System.IDisposable,
                                                  new()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the line break inserted behind a block comment that follows a comma in a wrapped constraint clause
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ConstraintBehindBlockCommentStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class,
                                 System.IDisposable, /* c */ new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class,
                                                  System.IDisposable, /* c */
                                                  new()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a constraint that already starts its line keeps its single line break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-BREAK-030")]
    public void ConstraintAlreadyStartingItsLineKeepsItsLineBreak()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class,
                                 System.IDisposable,
                             new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class,
                                                  System.IDisposable,
                                                  new()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}