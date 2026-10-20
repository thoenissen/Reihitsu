using System.Linq;
using System.Threading.Tasks;

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

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5302MessageFormat, 4));
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

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5302MessageFormat, 2));
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

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5302MessageFormat, 3));
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

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5302MessageFormat, 2));
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

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5302MessageFormat));
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
    /// Verifying that an own-line comment above an operand does not block the fix for a sibling operator whose
    /// own gap is clean, while an operator whose own gap genuinely contains that comment is correctly left
    /// reported — moving its right operand up would relocate the comment, which the formatting pipeline's own
    /// join-safety check already refuses regardless of this fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-005")]
    public async Task VerifyOwnLineCommentAboveOperandLeavesOnlyItsOwnGapUnfixed()
    {
        const string testData = """
                                internal class RH5302
                                {
                                    void Run(bool a, bool b, bool c)
                                    {
                                        if (a &&
                                            // note
                                            b ||
                                            c)
                                        {
                                        }
                                    }
                                }
                                """;
        const string expectedFixedText = """
                                         internal class RH5302
                                         {
                                             void Run(bool a, bool b, bool c)
                                             {
                                                 if (a &&

                                                     // note
                                                     b
                                                     || c)
                                                 {
                                                 }
                                             }
                                         }
                                         """;

        var fixedText = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(expectedFixedText, fixedText, "The outer || operator should move while the inner && stays, since its own gap to \"b\" genuinely contains the comment.");
        await Verify(fixedText, Diagnostic(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId).WithSpan(5, 15, 5, 17).WithMessage(AnalyzerResources.RH5302MessageFormat));
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
                                  Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5302MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}