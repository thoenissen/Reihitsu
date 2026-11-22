using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer, RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that misaligned logical operators are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-001")]
    [Requirement("RH5302-002")]
    [Requirement("RH5302-003")]
    [Requirement("RH5302-004")]
    public async Task VerifyMisalignedLogicalOperatorsAreDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class RH5302
                                {
                                    // Valid: operator on same line
                                    void ValidSameLine()
                                    {
                                        var a = true && false;
                                        var b = true || false;
                                        var c = true && false || true;
                                    }

                                    // Valid: operator aligned with first expression on next line
                                    void ValidMultiLine()
                                    {
                                        var a = true
                                                && false;

                                        var b = true
                                                || false;

                                        var c = true
                                                && false
                                                && true;

                                        var d = true
                                                || false
                                                || true;
                                    }

                                    // Invalid: operator not aligned with first expression
                                    void InvalidMultiLine()
                                    {
                                        var a = true
                                            {|#0:&&|} false;

                                        var b = true
                                                    {|#1:|||} false;

                                        var c = true
                                        {|#2:&&|} false
                                        {|#3:&&|} true;
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class RH5302
                                  {
                                      // Valid: operator on same line
                                      void ValidSameLine()
                                      {
                                          var a = true && false;
                                          var b = true || false;
                                          var c = true && false || true;
                                      }

                                      // Valid: operator aligned with first expression on next line
                                      void ValidMultiLine()
                                      {
                                          var a = true
                                                  && false;

                                          var b = true
                                                  || false;

                                          var c = true
                                                  && false
                                                  && true;

                                          var d = true
                                                  || false
                                                  || true;
                                      }

                                      // Invalid: operator not aligned with first expression
                                      void InvalidMultiLine()
                                      {
                                          var a = true
                                                  && false;

                                          var b = true
                                                  || false;

                                          var c = true
                                                  && false
                                                  && true;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that a chain with more than one trailing operator converges to the leading-operator form in a
    /// single fix application.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-001")]
    [Requirement("RH5302-003")]
    [Requirement("RH5302-004")]
    public async Task VerifyChainWithMultipleTrailingOperatorsConverges()
    {
        const string testData = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2, bool condition3)
                                    {
                                        if (condition1 {|#0:&&|}
                                            condition2 {|#1:|||}
                                            condition3)
                                        {
                                        }
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class RH5302
                                  {
                                      void Run(bool condition1, bool condition2, bool condition3)
                                      {
                                          if (condition1
                                              && condition2
                                              || condition3)
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a uniform three-operator trailing chain converges in a single fix application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-001")]
    [Requirement("RH5302-003")]
    [Requirement("RH5302-004")]
    public async Task VerifyThreeOperatorTrailingChainConverges()
    {
        const string testData = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2, bool condition3, bool condition4)
                                    {
                                        if (condition1 {|#0:&&|}
                                            condition2 {|#1:&&|}
                                            condition3 {|#2:&&|}
                                            condition4)
                                        {
                                        }
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class RH5302
                                  {
                                      void Run(bool condition1, bool condition2, bool condition3, bool condition4)
                                      {
                                          if (condition1
                                              && condition2
                                              && condition3
                                              && condition4)
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that a right-nested chain (<c>||</c> binding looser than <c>&amp;&amp;</c>) converges, with the
    /// inner operator aligned to its own left operand rather than to the outer one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-001")]
    [Requirement("RH5302-002")]
    [Requirement("RH5302-004")]
    public async Task VerifyRightNestedChainConverges()
    {
        const string testData = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2, bool condition3)
                                    {
                                        if (condition1 {|#0:|||}
                                            condition2 {|#1:&&|}
                                            condition3)
                                        {
                                        }
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class RH5302
                                  {
                                      void Run(bool condition1, bool condition2, bool condition3)
                                      {
                                          if (condition1
                                              || condition2
                                                 && condition3)
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that no code action is registered when a comment sits between the operator and its right
    /// operand, since relocating the right operand would relocate the comment too
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    public async Task VerifyOperatorFollowedByLineCommentIsNotFixed()
    {
        const string testData = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2)
                                    {
                                        if (condition1 && // note
                                            condition2)
                                        {
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<BinaryExpressionSyntax>()
                                                               .Single()
                                                               .OperatorToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment placed before the operator, on the left operand's own line, does not block the
    /// fix — only the gap between the operator and the right operand matters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-004")]
    [Requirement("RH5302-005")]
    public async Task VerifyCommentBeforeOperatorIsStillFixed()
    {
        const string testData = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2)
                                    {
                                        if (condition1 /* note */ {|#0:&&|}
                                            condition2)
                                        {
                                        }
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class RH5302
                                  {
                                      void Run(bool condition1, bool condition2)
                                      {
                                          if (condition1 /* note */
                                              && condition2)
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that an own-line comment above a misaligned operator does not block the fix. The formatting
    /// pipeline correctly precedes the comment with the blank line RH5020 requires as part of the same
    /// reformat — that is desired behavior, not a hazard this fix needs to avoid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOperatorWithCommentDirectlyAboveIsFixed()
    {
        const string testData = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2)
                                    {
                                        var a = condition1
                                            // comment
                                            && condition2;
                                    }
                                }
                                """;
        const string expectedFixedText = """
                                         internal class RH5302
                                         {
                                             void Run(bool condition1, bool condition2)
                                             {
                                                 var a = condition1

                                                         // comment
                                                         && condition2;
                                             }
                                         }
                                         """;

        var fixedText = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(expectedFixedText, fixedText);
        await Verify(fixedText);
    }

    /// <summary>
    /// Verifies that an own-line comment in front of the right operand of one operator of a chain withholds the fix for every
    /// operator of the chain: the formatter refuses the chain the fix formats
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyOwnLineCommentAboveOperandWithholdsTheFixForTheWholeChain(string endOfLine)
    {
        const string testData = """
                                internal class RH5302
                                {
                                    void Run(bool a, bool b, bool c)
                                    {
                                        if (a {|RH5302:&&|}
                                            // note
                                            b {|RH5302:|||}
                                            c)
                                        {
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifying that no code action is registered when a preprocessor directive sits between the operator and
    /// its right operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    public async Task VerifyOperatorFollowedByDirectiveIsNotFixed()
    {
        const string testData = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2)
                                    {
                                        if (condition1 &&
                                #region Note
                                            condition2)
                                #endregion
                                        {
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<BinaryExpressionSyntax>()
                                                               .Single()
                                                               .OperatorToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a misaligned operator preceded by a conditional directive is still fixed, and the active
    /// branch's operator moves to the target column while the inactive branch's disabled text stays untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOperatorPrecededByConditionalDirectiveIsStillFixed()
    {
        const string source = """
                              internal class RH5302
                              {
                                  void Run(bool condition1, bool condition2)
                                  {
                                      var a = condition1
                              #if DEBUG
                                          && condition2;
                              #else
                                          && condition2;
                              #endif
                                  }
                              }
                              """;
        const string expected = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2)
                                    {
                                        var a = condition1
                                #if DEBUG
                                            && condition2;
                                #else
                                                && condition2;
                                #endif
                                    }
                                }
                                """;

        var fixedText = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, fixedText);
    }

    /// <summary>
    /// Verifying that the fix preserves the document's own line-ending style
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixPreservesCarriageReturnLineFeedLineEndings()
    {
        const string source = """
                              internal class RH5302
                              {
                                  void Run(bool condition1, bool condition2)
                                  {
                                      if (condition1 &&
                                          condition2)
                                      {
                                      }
                                  }
                              }
                              """;

        var fixedText = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(source));

        Assert.IsFalse(fixedText.Replace("\r\n", string.Empty).Contains('\n'), "The fixed source must not contain a lone line feed.");
        Assert.Contains("\r\n", fixedText);
    }

    /// <summary>
    /// Verifies that an operator on the line its left operand ends on, with its right operand on a later line, is
    /// reported on the operator, while a logical expression on one line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-001")]
    [Requirement("RH5302-003")]
    public async Task VerifyOperatorAtEndOfLineIsReportedOnOperator()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        var x = a {|RH5302:&&|}
                                                b;
                                        var y = a && b || c;

                                        return x;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an operator which starts a line outside the column of its own left operand is reported, while
    /// operators at that column are not reported, also when a tighter-binding operator aligns with its own left operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-002")]
    [Requirement("RH5302-003")]
    public async Task VerifyOperatorStartingLineOutsideColumnOfLeftOperandIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        var x = a
                                            {|RH5302:|||} b;
                                        var y = a
                                                && b
                                                && c;
                                        var z = a
                                                || b
                                                   && c;

                                        return x;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an operator which starts a line one column to the right of its left operand is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-002")]
    public async Task VerifyOperatorOneColumnRightOfLeftOperandIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        var x = a
                                                 {|RH5302:&&|} b;

                                        return x;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix lays out the whole chain of logical operators, so that one application corrects every
    /// operator of the chain
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-004")]
    public async Task VerifyFixLaysOutWholeChainInOneApplication()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b, bool c)
                                    {
                                        if (a {|RH5302:&&|}
                                            b {|RH5302:|||}
                                            c)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b, bool c)
                                     {
                                         if (a
                                             && b
                                             || c)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix lays out the whole chain of logical operators, so that one application corrects every
    /// operator of the chain, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-004")]
    public async Task VerifyFixLaysOutWholeChainInOneApplicationWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b, bool c)
                                    {
                                        if (a {|RH5302:&&|}
                                            b {|RH5302:|||}
                                            c)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b, bool c)
                                     {
                                         if (a
                                             && b
                                             || c)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that a comment behind an operator at the end of a line keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    public async Task VerifyCommentBehindOperatorAtEndOfLineWithholdsFix()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        var x = a {|RH5302:&&|} // note
                                                b;

                                        return x;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that a comment in front of an operator at the end of a line does not withhold the fix and stays where
    /// it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-004")]
    [Requirement("RH5302-005")]
    public async Task VerifyCommentInFrontOfOperatorDoesNotWithholdFix()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        var x = a /* note */ {|RH5302:&&|}
                                                b;

                                        return x;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b)
                                     {
                                         var x = a /* note */
                                                 && b;

                                         return x;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a comment in front of an operator at the end of a line does not withhold the fix and stays where
    /// it is, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-004")]
    [Requirement("RH5302-005")]
    public async Task VerifyCommentInFrontOfOperatorDoesNotWithholdFixWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        var x = a /* note */ {|RH5302:&&|}
                                                b;

                                        return x;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b)
                                     {
                                         var x = a /* note */
                                                 && b;

                                         return x;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies the specification example: each operator is reported, and no code fix is offered for any of them, because each
    /// chain holds a comment behind an operator — a reported operator at the end of its line, a misaligned operator at the start
    /// of its line, another operator of the chain, and an arithmetic operator inside an operand of the chain
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifySpecExampleCommentBehindAnOperatorOfTheChainWithholdsTheFix(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c, int n)
                                    {
                                        var x = a {|RH5302:&&|} // note
                                                b;
                                        var y = a
                                            {|RH5302:&&|} /* note */ b;

                                        if (a {|RH5302:&&|}
                                            b {|RH5302:&&|} // note
                                            c)
                                        {
                                        }

                                        if (n + /* note */ 1 > 2 {|RH5302:&&|}
                                            b)
                                        {
                                        }

                                        return x && y;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment behind a misaligned operator at the start of its line withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyCommentBehindMisalignedLeadingOperatorWithholdsFix(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        var y = a
                                            {|RH5302:&&|} /* note */ b;

                                        return y;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment behind another operator of the chain, which is not the reported one, withholds the fix for the
    /// reported operator too
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyCommentBehindAnotherOperatorOfTheChainWithholdsFix(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b, bool c)
                                    {
                                        if (a {|RH5302:&&|}
                                            b {|RH5302:&&|} // note
                                            c)
                                        {
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment behind an operator at the start of its line, which is aligned and not reported, withholds the fix
    /// for a reported operator of the same chain
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    public async Task VerifyCommentBehindAnAlignedLeadingOperatorOfTheChainWithholdsFix()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        var x = a
                                                && /* note */ b {|RH5302:&&|}
                                                c;

                                        return x;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that a comment behind an arithmetic operator inside an operand of the chain withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyCommentBehindAnArithmeticOperatorInsideAnOperandWithholdsFix(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b, int n)
                                    {
                                        if (n + /* note */ 1 > 2 {|RH5302:&&|}
                                            b)
                                        {
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional operand behind a reported operator withholds the fix in both builds: in the build that defines
    /// <c>DEBUG</c> the directive stands behind the first operator, and in the other the directive and the disabled text do
    /// </summary>
    /// <param name="defineDebug">Whether the build defines <c>DEBUG</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    [DataRow(false)]
    [DataRow(true)]
    public async Task VerifyDisabledTextBehindAnOperatorWithholdsFixInEveryBuild(bool defineDebug)
    {
        const string withoutDebug = """
                                    class C
                                    {
                                        bool M(bool a, bool b, bool c)
                                        {
                                            return a {|RH5302:&&|}
                                    #if DEBUG
                                                   b &&
                                    #endif
                                                   c;
                                        }
                                    }
                                    """;
        const string withDebug = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c)
                                     {
                                         return a {|RH5302:&&|}
                                 #if DEBUG
                                                b {|RH5302:&&|}
                                 #endif
                                                c;
                                     }
                                 }
                                 """;

        if (defineDebug)
        {
            await VerifyNoCodeFixIsOffered(withDebug, DocumentationMode.Parse, "DEBUG");
        }
        else
        {
            await VerifyNoCodeFixIsOffered(withoutDebug, DocumentationMode.Parse);
        }
    }

    /// <summary>
    /// Verifies the specification example: a comment behind an operator outside the chain the fix formats, in another argument,
    /// does not withhold the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifySpecExampleCommentBehindAnOperatorOutsideTheChainDoesNotWithholdFix(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b, int n)
                                    {
                                        N(a {|RH5302:&&|}
                                          b, n + /* note */ 1);
                                    }

                                    void N(bool v, int w)
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b, int n)
                                     {
                                         N(a
                                           && b, n + /* note */ 1);
                                     }

                                     void N(bool v, int w)
                                     {
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              internal class RH5302
                              {
                                  void Run(bool condition1, bool condition2, bool condition3)
                                  {
                                      if (condition1 ||
                                          condition2 &&
                                          condition3)
                                      {
                                      }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that an operator behind a comment that starts its line outside the operator column is reported, although the operator behind the comment reaches that column, and that the code fix is withheld or lays the line out in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-002")]
    [Requirement("SHARED-COMMENT-011")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyOperatorBehindMisplacedLeadingCommentIsReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        return a
                                       /* x */ {|RH5302:&&|} b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     bool M(bool a, bool b)
                                     {
                                         return a
                                                /* x */ && b;
                                     }
                                 }
                                 """;

        await VerifyCodeFixIsWithheldOrProducesOneOf(ConvertEndOfLine(testData, endOfLine), true, ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix of a logical expression whose right operand's argument list stands on the line below the invoked name
    /// joins the argument list onto the name
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-BREAK-039")]
    public async Task VerifyFixJoinsOperandArgumentListBelowItsName(string endOfLine)
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(bool a)
                                    {
                                        return a {|RH5302:&&|}
                                            Has
                                            (1);
                                    }

                                    private static bool Has(int value)
                                    {
                                        return value > 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static bool Method(bool a)
                                     {
                                         return a
                                                && Has(1);
                                     }

                                     private static bool Has(int value)
                                     {
                                         return value > 0;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a logical expression whose right operand's argument list holds, inside an interpolation hole, an argument
    /// list below its name joins that argument list onto the name and lays out the operand's argument list as the one line it becomes
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-BREAK-039")]
    public async Task VerifyFixJoinsArgumentListInInterpolationHoleAndKeepsOperandListOnOneLine(string endOfLine)
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(bool a, object x)
                                    {
                                        return a {|RH5302:&&|}
                                            Has($"{x.ToString
                                            ()}", 2);
                                    }

                                    private static bool Has(string text, int value)
                                    {
                                        return value > text.Length;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static bool Method(bool a, object x)
                                     {
                                         return a
                                                && Has($"{x.ToString()}", 2);
                                     }

                                     private static bool Has(string text, int value)
                                     {
                                         return value > text.Length;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the change example: the fix of a logical expression whose right operand holds an element access below its element
    /// joins the element access onto the element
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-BREAK-040")]
    public async Task VerifyFixJoinsOperandElementAccessBelowItsElement(string endOfLine)
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(bool a, int[] arr)
                                    {
                                        return a {|RH5302:&&|}
                                            arr
                                            [0] > 1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static bool Method(bool a, int[] arr)
                                     {
                                         return a
                                                && arr[0] > 1;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the change example: the fix of a logical expression whose right operand's argument list holds a null-forgiving
    /// operator below its element joins the operator onto the element and lays out the argument list as the one line it becomes
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-BREAK-040")]
    public async Task VerifyFixJoinsNullForgivingOperatorAndKeepsOperandListOnOneLine(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                internal class Example
                                {
                                    private static bool Method(bool a, string? s)
                                    {
                                        return a {|RH5302:&&|}
                                            Has(s
                                            !, 2);
                                    }

                                    private static bool Has(string text, int value)
                                    {
                                        return value > text.Length;
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 internal class Example
                                 {
                                     private static bool Method(bool a, string? s)
                                     {
                                         return a
                                                && Has(s!, 2);
                                     }

                                     private static bool Has(string text, int value)
                                     {
                                         return value > text.Length;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix, which formats the logical expression, joins the wrapped list of a conditional element access in an
    /// operand, as it joins the list of an element access there
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-BREAK-014")]
    public async Task VerifyFixJoinsConditionalElementAccessListInAnOperand(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool x, bool y, bool[,] c)
                                    {
                                        var a = x {|RH5302:&&|}
                                                (c?[1,
                                                    2] ?? false) && y;
                                        return a;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool x, bool y, bool[,] c)
                                     {
                                         var a = x
                                                 && (c?[1, 2] ?? false) && y;
                                         return a;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the specification example: the fix formats the chain as a root of its own that starts behind <c>var r = </c>, and
    /// the continuation of the compound assignment, whose construct starts on the root's first line, is indented four columns
    /// right of where the document begins that line, as whole-document formatting indents it, not four columns right of the
    /// root's first token
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    [Requirement("FMT-PIPE-024")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixIndentsAContinuationOnTheRootsFirstLineFromTheDocumentsLineStart(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    bool M(int x, bool y)
                                    {
                                        var r = (x += // c
                                        3) > 0 &&
                                        y;

                                        return r;
                                    }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      bool M(int x, bool y)
                                      {
                                          var r = (x += // c
                                              3) > 0
                                                  && y;

                                          return r;
                                      }
                                  }
                                  """;

        var actual = await ApplyCodeFixAsync(ConvertEndOfLine(testData, endOfLine));

        Assert.AreEqual(ConvertEndOfLine(resultData, endOfLine), actual);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5302
                                {
                                    void Run(bool condition1, bool condition2, bool condition3)
                                    {
                                        if (condition1 {|#0:|||}
                                            condition2 {|#1:&&|}
                                            condition3)
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5302
                                 {
                                     void Run(bool condition1, bool condition2, bool condition3)
                                     {
                                         if (condition1
                                             || condition2
                                                && condition3)
                                         {
                                         }
                                     }
                                 }
                                 """;

        // Both operators belong to the same logical chain, so walking up through their enclosing &&/|| parents
        // reaches the identical outermost expression for each diagnostic. The batch fixer discards the second,
        // overlapping action, and the surviving single fix already re-lays out every operator in the chain,
        // including the right-nested && whose own anchor is the || expression's right operand rather than the
        // chain's start
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}