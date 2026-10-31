using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the line that starts behind a documentation comment written behind code on the line above.
/// The comment ends its line, so the first token behind it starts its line and gets the column the same line gets
/// behind a single-line comment, while the comment itself stays where it was written
/// </summary>
[TestClass]
public class StrandedDocumentationLineStartTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: the closing brace of a type on the line behind a documentation comment
    /// written behind its last field takes the column of its type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    [Requirement("SHARED-BLANK-008")]
    public void ClosingBraceOfTypeBehindStrayDocumentationCommentTakesTypeColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /// <summary>end</summary>
                                       }
                             """;
        const string expected = """
                                class C
                                {
                                    int A; /// <summary>end</summary>
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the continuation line of a documentation comment written behind code is the
    /// comment's text and keeps its column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ContinuationLineOfStrayDocumentationCommentKeepsItsColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; ///<remarks>a
                                     /// b</remarks>
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the closing brace on the line behind a two-line documentation comment written behind code takes
    /// the column of its type, while the comment's continuation line keeps its column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingBraceBehindTwoLineStrayDocumentationCommentTakesTypeColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; ///<remarks>a
                                     /// b</remarks>
                                   }
                             """;
        const string expected = """
                                class C
                                {
                                    int A; ///<remarks>a
                                        /// b</remarks>
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a closing brace behind the last line of a multi-line comment does not start
    /// its line, so it is not placed
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void ClosingBraceBehindLastLineOfMultiLineCommentIsNotPlaced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /* a
                                 b */ }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a documentation comment written behind code in front of a chain link stays
    /// behind code, and the links behind it are aligned
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-INDENT-002")]
    public void StrayDocumentationCommentBeforeChainLinkStaysAndLinksAreAligned()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 object M(int[] items)
                                 {
                                     return items.Where(x => x > 0) /// x
                                              .Select(x => x)
                                                  .ToList();
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    object M(int[] items)
                                    {
                                        return items.Where(x => x > 0) /// x
                                                    .Select(x => x)
                                                    .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of a method body on the line behind a documentation comment written behind a
    /// statement takes the column of its method, as it does behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingBraceOfBlockBehindStrayDocumentationCommentTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1; /// x
                                         }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1; /// x
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of an accessor list on the line behind a documentation comment written behind an
    /// accessor takes the column of its property
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingBraceOfAccessorListBehindStrayDocumentationCommentTakesPropertyColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P
                                 {
                                     get; /// x
                                         }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P
                                    {
                                        get; /// x
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of a block-scoped namespace on the line behind a documentation comment written
    /// behind the closing brace of a type takes the column of its namespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingBraceOfNamespaceBehindStrayDocumentationCommentTakesNamespaceColumn()
    {
        // Arrange
        const string input = """
                             namespace N
                             {
                                 class C
                                 {
                                     int A;
                                 } /// x
                                     }
                             """;
        const string expected = """
                                namespace N
                                {
                                    class C
                                    {
                                        int A;
                                    } /// x
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of a switch statement on the line behind a documentation comment written behind
    /// the last statement of a section takes the column of its switch statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingBraceOfSwitchStatementBehindStrayDocumentationCommentTakesSwitchColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     switch (a)
                                     {
                                         case 1:
                                             break; /// x
                                             }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                break; /// x
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of an object initializer on the line behind a documentation comment written
    /// behind its last assignment is aligned with its opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingBraceOfObjectInitializerBehindStrayDocumentationCommentIsAlignedWithItsOpeningBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A;

                                 object M()
                                 {
                                     return new C { A = 1 /// x
                                                };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    object M()
                                    {
                                        return new C
                                               {
                                                   A = 1 /// x
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of a switch expression on the line behind a documentation comment written behind
    /// its last arm is aligned with its opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingBraceOfSwitchExpressionBehindStrayDocumentationCommentIsAlignedWithItsOpeningBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int a)
                                 {
                                     return a switch { 1 => 2, _ => 3 /// x
                                              };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return a switch
                                               {
                                                   1 => 2, _ => 3 /// x
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an over-indented terminator of a return statement on the line behind a documentation comment
    /// written behind the expression takes the column of its statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ReturnTerminatorBehindStrayDocumentationCommentTakesStatementColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return 1 /// x
                                             ;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return 1 /// x
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an argument list on the line behind a documentation comment written
    /// behind the last argument gets the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingParenthesisBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int a, int b)
                                 {
                                     return System.Math.Max(a, b /// x
                                         );
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        return System.Math.Max(a, b /// x
                                                               );
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing bracket of an element access on the line behind a documentation comment written behind
    /// the index gets the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingBracketBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int[] items)
                                 {
                                     return items[0 /// x
                                             ];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int[] items)
                                    {
                                        return items[0 /// x
                                                     ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing angle bracket of a type argument list on the line behind a documentation comment
    /// written behind the type argument gets the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingAngleBracketBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 System.Collections.Generic.List<int /// x
                                         > _l;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    System.Collections.Generic.List<int /// x
                                                                    > _l;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma on the line behind a documentation comment written behind an argument gets the
    /// column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void LeadingCommaBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int a, int b)
                                 {
                                     return System.Math.Max(a /// x
                                         , b);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        return System.Math.Max(a /// x
                                                               , b);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>+</c> on the line behind a documentation comment written behind the left operand gets
    /// the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void WrappedAdditionOperatorBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int a)
                                 {
                                     var x = a /// x
                                 + 1;

                                     return x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        var x = a /// x
                                                + 1;

                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>&amp;&amp;</c> on the line behind a documentation comment written behind the left
    /// operand gets the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void WrappedLogicalAndOperatorBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(bool a, bool c)
                                 {
                                     var x = a /// x
                                 && c;

                                     return x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(bool a, bool c)
                                    {
                                        var x = a /// x
                                                && c;

                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>??</c> on the line behind a documentation comment written behind the left operand gets
    /// the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void WrappedCoalesceOperatorBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 string M(string t)
                                 {
                                     var x = t /// x
                                 ?? string.Empty;

                                     return x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    string M(string t)
                                    {
                                        var x = t /// x
                                                    ?? string.Empty;

                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>is</c> on the line behind a documentation comment written behind the operand gets the
    /// column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void WrappedIsOperatorBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     var x = o /// x
                                 is int;

                                     return x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o)
                                    {
                                        var x = o /// x
                                                is int;

                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped lambda arrow on the line behind a documentation comment written behind the parameter gets
    /// the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void WrappedLambdaArrowBehindStrayDocumentationCommentIsPlacedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Func<int, int> f = x /// x
                                 => x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Func<int, int> f = x /// x
                                        => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>=</c> on the line behind a documentation comment written behind the assigned field
    /// gets the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void WrappedAssignmentOperatorBehindStrayDocumentationCommentIsPlacedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _x;

                                 void M()
                                 {
                                     _x /// x
                                 = 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int _x;

                                    void M()
                                    {
                                        _x /// x
                                        = 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single wrapped <c>?.</c> on the line behind a documentation comment written behind its receiver
    /// gets the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void SingleConditionalAccessBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int? M(string s)
                                 {
                                     return s /// x
                                 ?.Length;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int? M(string s)
                                    {
                                        return s /// x
                                               ?.Length;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the dot of a member access on the line behind a documentation comment written behind its receiver
    /// gets the column it gets behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void MemberAccessDotBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public string M()
                                 {
                                     return this /// x
                                         .ToString();
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public string M()
                                    {
                                        return this /// x
                                               .ToString();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain whose first link starts its line behind a documentation comment written behind the root
    /// keeps the comment and aligns its links as behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void FirstChainLinkBehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 object M(int[] items)
                                 {
                                     return items /// x
                                         .Where(x => x > 0)
                                         .ToList();
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    object M(int[] items)
                                    {
                                        return items /// x
                                               .Where(x => x > 0)
                                               .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written behind code in front of a conditional-access link stays behind
    /// code, and the links behind it are aligned as behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-INDENT-002")]
    public void StrayDocumentationCommentBeforeConditionalAccessLinkStaysAndLinksAreAligned()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int? M(System.Collections.Generic.List<object> items)
                                 {
                                     return items.Find(x => x != null) /// x
                                              ?.ToString()
                                                  ?.Trim()
                                                  .Length;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int? M(System.Collections.Generic.List<object> items)
                                    {
                                        return items.Find(x => x != null) /// x
                                                    ?.ToString()
                                                    ?.Trim()
                                                    .Length;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written behind the condition of a conditional expression in front of the
    /// <c>?</c> stays behind code, and the branches are aligned as behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-INDENT-002")]
    public void StrayDocumentationCommentBeforeConditionalQuestionMarkStaysAndBranchesAreAligned()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     return a /// x
                                 ? 1
                                 : 2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool a)
                                    {
                                        return a /// x
                                                   ? 1
                                                   : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written behind the first branch of a conditional expression in front of
    /// the <c>:</c> stays behind code, and the conditional is laid out as behind a single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-INDENT-002")]
    public void StrayDocumentationCommentBeforeConditionalColonStaysAndBranchesAreAligned()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     return a ? 1 /// x
                                 : 2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool a)
                                    {
                                        return a
                                                   ? 1 /// x
                                                   : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written behind the body of a do-while statement in front of <c>while</c>
    /// stays behind the closing brace, and <c>while</c> takes the column of its statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-INDENT-002")]
    public void StrayDocumentationCommentBeforeDoWhileConditionStaysAndWhileTakesStatementColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     do
                                     {
                                     } /// x
                                 while (a);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        do
                                        {
                                        } /// x
                                        while (a);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a closing brace separated from a documentation comment written behind code by a blank line takes
    /// the column of its type once the blank line is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void ClosingBraceBehindStrayDocumentationCommentAndBlankLineTakesTypeColumn()
    {
        // Arrange
        const string input = """
                             class D
                             {
                                 int A; /// x

                                       }
                             """;
        const string expected = """
                                class D
                                {
                                    int A; /// x
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a terminator separated from a documentation comment written behind code by a conditional directive
    /// takes the column of its statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void TerminatorBehindStrayDocumentationCommentAndDirectiveTakesStatementColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return 1 /// x
                             #if DEBUG
                             #endif
                                         ;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return 1 /// x
                                #if DEBUG
                                #endif
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a terminator separated from a documentation comment written behind code by disabled text takes the
    /// column of its statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void TerminatorBehindStrayDocumentationCommentAndDisabledTextTakesStatementColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return 1 /// x
                             #if NEVER
                                     + 2
                             #endif
                                         ;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return 1 /// x
                                #if NEVER
                                        + 2
                                #endif
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment and a terminator behind a documentation comment written behind code both take
    /// the column of their statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void OwnLineCommentAndTerminatorBehindStrayDocumentationCommentTakeStatementColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return 1 /// x
                                 // own
                                             ;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return 1 /// x
                                        // own
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written behind the closing brace of an <c>if</c> block in front of
    /// <c>else</c> is still moved onto its own line in front of <c>else</c>
    /// </summary>
    [TestMethod]
    public void StrayDocumentationCommentBeforeElseIsStillRelocated()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                     } /// x
                                     else
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                        }

                                        /// x
                                        else
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written behind the closing brace of a <c>try</c> block in front of
    /// <c>catch</c> is still moved onto its own line in front of <c>catch</c>
    /// </summary>
    [TestMethod]
    public void StrayDocumentationCommentBeforeCatchIsStillRelocated()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     try
                                     {
                                     } /// x
                                     catch
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        try
                                        {
                                        }

                                        /// x
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written behind a comma in front of the next argument is still moved onto
    /// its own line in front of that argument
    /// </summary>
    [TestMethod]
    public void StrayDocumentationCommentBeforeArgumentIsStillRelocated()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int N(int a, int b)
                                 {
                                     return N(1, /// x
                                              2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int N(int a, int b)
                                    {
                                        return N(1,

                                                 /// x
                                                 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written behind the last statement of a switch section in front of the
    /// next switch label is still moved onto its own line in front of that label
    /// </summary>
    [TestMethod]
    public void StrayDocumentationCommentBeforeSwitchLabelIsStillRelocated()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     switch (a)
                                     {
                                         case 1:
                                             break; /// x
                                         case 2:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                break;

                                            /// x
                                            case 2:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written behind a statement in front of a misindented statement is still
    /// moved onto its own line, and the statement takes the column of its block
    /// </summary>
    [TestMethod]
    public void StrayDocumentationCommentBeforeStatementIsStillRelocated()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1; /// stray
                                         return;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        /// stray
                                        return;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}