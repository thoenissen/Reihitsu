using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for a later link of a wrapped chain whose operator stands behind the closing
/// <c>*/</c> of a comment that spans several lines: the link does not start its line, so the formatter
/// breaks the line in front of its operator
/// </summary>
[TestClass]
public class ChainLinkBehindMultiLineCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the reported shape: the line breaks in front of <c>.D</c>, which aligns with the other
    /// links, and the comment gets its blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("SHARED-COMMENT-012")]
    public void ReportedLinkBehindMultiLineCommentTailStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()
                                              /* x
                                     */ .D();

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /* x
                                        */
                                                 .D();

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specified example: the line breaks in front of <c>.D</c> and <c>.D</c> aligns with the
    /// other links
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    public void LinkBehindMultiLineCommentTailStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* x
                                     */ .D();

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /* x
                                        */
                                                 .D();

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the break does not depend on the column of the comment's last line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("SHARED-COMMENT-012")]
    public void LinkBehindIndentedMultiLineCommentTailStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* x
                                                 y */ .D();

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /* x
                                                    y */
                                                 .D();

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a later link that is a property read behind a multi-line comment tail starts its own
    /// line too
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("SHARED-COMMENT-012")]
    public void PropertyLinkBehindMultiLineCommentTailStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* x
                                     */ .E;

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /* x
                                        */
                                                 .E;

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional-access link behind a multi-line comment tail starts its own line too
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("SHARED-COMMENT-012")]
    public void ConditionalAccessLinkBehindMultiLineCommentTailStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* x
                                     */ ?.D();

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /* x
                                        */
                                                 ?.D();

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>!.</c> link behind a multi-line comment tail starts its own line too
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("SHARED-COMMENT-012")]
    public void NullForgivingLinkBehindMultiLineCommentTailStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* x
                                     */ !.D();

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /* x
                                        */
                                                 !.D();

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a link behind the tail of a <c>/** */</c> comment that spans several lines starts its own line
    /// too
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("SHARED-COMMENT-012")]
    public void LinkBehindMultiLineDocumentationStyleCommentTailStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /** x
                                     */ .D();

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /** x
                                        */
                                                 .D();

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line comment that starts on the previous link's line already gets the break
    /// in front of the next link
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    public void LinkBehindCommentStartingOnPreviousLinkLineStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C() /* x
                                     */ .D();

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C() /* x
                                        */
                                                 .D();

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a link behind a single-line comment on a line of its own already starts its line and
    /// is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    public void LinkBehindSingleLineCommentOnItsOwnLineIsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* x */ .D();

                                     return r;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Tests
}