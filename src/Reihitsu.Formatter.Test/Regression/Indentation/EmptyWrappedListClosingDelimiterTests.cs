using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the closing delimiter of an empty list that a comment or a directive spreads over several lines:
/// it starts in the column directly after the opening delimiter, as in a list with elements (FMT-INDENT-012)
/// </summary>
[TestClass]
public class EmptyWrappedListClosingDelimiterTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Formats the input under LF and CRLF, verifies that the first line of the output whose text starts with
    /// <paramref name="lineStart"/> starts in <paramref name="column"/>, and verifies that a second pass is stable
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="lineStart">The start of the line to check, without its leading whitespace</param>
    /// <param name="column">The expected zero-based column of the line's first character</param>
    private static void AssertLineColumn(string input, string lineStart, int column)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var source = NormalizeLineEndings(input, endOfLine);
            var output = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(source), default).Output.ToString();
            var line = Array.Find(output.Split(endOfLine), candidate => candidate.TrimStart().StartsWith(lineStart, StringComparison.Ordinal));

            Assert.IsNotNull(line, $"The output under {DescribeLineEnding(endOfLine)} line endings has no line starting with '{lineStart}':{Environment.NewLine}{output}");
            Assert.AreEqual(column, line.Length - line.TrimStart().Length, $"Unexpected column of '{lineStart}' under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
        }

        AssertSecondPassIsStable(input);
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the reported example: the closing parenthesis of an empty argument list behind a comment starts in the
    /// column after the opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyArgumentListAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     M( // c
                                                   );
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        M( // c
                                          );
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an empty parameter list behind a comment starts in the column after the
    /// opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyParameterListAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 void M( // c
                                     )
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M( // c
                                           )
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an empty object-creation argument list starts in the column after the
    /// opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyObjectCreationAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 object M()
                                 {
                                     return new object( // c
                                         );
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M()
                                    {
                                        return new object( // c
                                                          );
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an empty constructor-initializer argument list starts in the column after
    /// the opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyConstructorInitializerAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 public C()
                                 {
                                 }

                                 public C(int x)
                                     : this( // c
                                     )
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public C()
                                    {
                                    }

                                    public C(int x)
                                        : this( // c
                                               )
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an empty primary-constructor base argument list starts in the column after
    /// the opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyPrimaryConstructorBaseAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class Base;

                             class P() : Base( // c
                                 );
                             """;
        const string expected = """
                                class Base;

                                class P() : Base( // c
                                                 );
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an empty attribute argument list starts in the column after the opening
    /// parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyAttributeArgumentListAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 [System.Obsolete( // c
                                     )]
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    [System.Obsolete( // c
                                                     )]
                                    void M()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an empty local-function parameter list starts in the column after the
    /// opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyLocalFunctionParameterListAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     void L( // c
                                         )
                                     {
                                     }

                                     L();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        void L( // c
                                               )
                                        {
                                        }

                                        L();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an empty delegate parameter list starts in the column after the opening
    /// parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyDelegateParameterListAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 delegate void Del( // c
                                     );
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    delegate void Del( // c
                                                      );
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an empty parenthesized-lambda parameter list starts in the column after the
    /// opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyLambdaParameterListAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Action a = ( // c
                                         ) => M();
                                 }
                             }
                             """;

        AssertLineColumn(input, ") => M();", 27);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an empty anonymous-method parameter list starts in the column after the
    /// opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfEmptyAnonymousMethodParameterListAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Action a = delegate( // c
                                         )
                                     {
                                     };
                                 }
                             }
                             """;

        AssertLineColumn(input, ")", 35);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of an argument list that holds only a disabled branch starts in the column after
    /// the opening parenthesis, and the directives stay at column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisBehindDisabledBranchAlignsAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 void M(params int[] v)
                                 {
                                     M(
                             #if X
                                         1
                             #endif
                                         );
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(params int[] v)
                                    {
                                        M(
                                #if X
                                            1
                                #endif
                                          );
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of a list with an element, behind a comment, keeps its alignment after the
    /// opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOfNonEmptyListStaysAlignedAfterOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 void M(int v)
                                 {
                                     M(1 // c
                                       );
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Tests
}