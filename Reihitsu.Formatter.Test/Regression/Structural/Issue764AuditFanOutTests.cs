using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Escalated-confirmation audit fan-out for issue #764. Each fixture asserts only the issue's own
/// expected-versus-actual difference — the <c>else</c> keyword sharing a line with the closing brace
/// of the inserted if-block — so an unrelated layout guess cannot produce a false failure
/// </summary>
[TestClass]
public class Issue764AuditFanOutTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Formats the given source with the requested line ending
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="endOfLine">The end-of-line sequence</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext(endOfLine);

        return FormattingPipeline.Execute(tree.GetRoot(), context, CancellationToken.None).ToFullString();
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Runs every audit variant under LF and CRLF and asserts that no closing brace ever shares its
    /// line with a following <c>else</c> keyword
    /// </summary>
    [TestMethod]
    public void AuditFanOutNeverGluesElseOntoClosingBrace()
    {
        var fixtures = new Dictionary<string, string>
                       {
                           ["A_LiteralExample"] = """
                                                  class C
                                                  {
                                                      void M(bool value)
                                                      {
                                                          if (value)
                                                              DoSomething();
                                                          else
                                                              Thread.Sleep(100);
                                                      }
                                                  }
                                                  """,
                           ["B_OwnLineCommentBeforeElse"] = """
                                                            class C
                                                            {
                                                                void M(bool value)
                                                                {
                                                                    if (value)
                                                                        DoSomething();
                                                                    // otherwise wait
                                                                    else
                                                                        Thread.Sleep(100);
                                                                }
                                                            }
                                                            """,
                           ["C_TrailingCommentOnThenBranch"] = """
                                                               class C
                                                               {
                                                                   void M(bool value)
                                                                   {
                                                                       if (value)
                                                                           DoSomething(); // note
                                                                       else
                                                                           Thread.Sleep(100);
                                                                   }
                                                               }
                                                               """,
                           ["D_DanglingNestedIfElse"] = """
                                                        class C
                                                        {
                                                            void M(bool a, bool b)
                                                            {
                                                                if (a)
                                                                    if (b)
                                                                        DoSomething();
                                                                    else
                                                                        Thread.Sleep(100);
                                                            }
                                                        }
                                                        """,
                           ["E_MultiLineThenBranchStatement"] = """
                                                                class C
                                                                {
                                                                    void M(bool value)
                                                                    {
                                                                        if (value)
                                                                            DoSomething(firstArgument,
                                                                                        secondArgument);
                                                                        else
                                                                            Thread.Sleep(100);
                                                                    }
                                                                }
                                                                """,
                           ["F_LeadingCommentOnElseBody"] = """
                                                            class C
                                                            {
                                                                void M(bool value)
                                                                {
                                                                    if (value)
                                                                        DoSomething();
                                                                    else
                                                                        // wait a bit
                                                                        Thread.Sleep(100);
                                                                }
                                                            }
                                                            """,
                           ["G_BlankLineBeforeElse"] = """
                                                       class C
                                                       {
                                                           void M(bool value)
                                                           {
                                                               if (value)
                                                                   DoSomething();

                                                               else
                                                                   Thread.Sleep(100);
                                                           }
                                                       }
                                                       """,
                           ["H_NestedInsideLoop"] = """
                                                    class C
                                                    {
                                                        void M(bool value)
                                                        {
                                                            foreach (var item in items)
                                                            {
                                                                if (value)
                                                                    DoSomething(item);
                                                                else
                                                                    Thread.Sleep(100);
                                                            }
                                                        }
                                                    }
                                                    """,
                           ["I_EmbeddedWhileInThenBranch"] = """
                                                             class C
                                                             {
                                                                 void M(bool a, bool b)
                                                                 {
                                                                     if (a)
                                                                         while (b)
                                                                             DoSomething();
                                                                     else
                                                                         Thread.Sleep(100);
                                                                 }
                                                             }
                                                             """,
                           ["J_ElseIfChainWithCommentBeforeFinalElse"] = """
                                                                         class C
                                                                         {
                                                                             void M(int value)
                                                                             {
                                                                                 if (value == 1)
                                                                                     First();
                                                                                 else if (value == 2)
                                                                                     Second();
                                                                                 // fallback
                                                                                 else
                                                                                     Third();
                                                                             }
                                                                         }
                                                                         """,
                           ["K_IfElseInsideSwitchSection"] = """
                                                             class C
                                                             {
                                                                 void M(int value, bool flag)
                                                                 {
                                                                     switch (value)
                                                                     {
                                                                         case 1:
                                                                             if (flag)
                                                                                 DoSomething();
                                                                             else
                                                                                 Thread.Sleep(100);
                                                                             break;
                                                                     }
                                                                 }
                                                             }
                                                             """,
                       };

        var glue = new Regex(@"\}[^\r\n]*\belse\b");
        var failures = new List<string>();

        foreach (var fixture in fixtures)
        {
            foreach (var endOfLine in _lineEndings)
            {
                var endingName = DescribeLineEnding(endOfLine);
                var input = NormalizeLineEndings(fixture.Value, endOfLine);
                var formatted = Format(input, endOfLine);
                var secondPass = Format(formatted, endOfLine);

                if (glue.IsMatch(formatted))
                {
                    failures.Add($"{fixture.Key} [{endingName}] glued 'else' onto a closing-brace line:{Environment.NewLine}{formatted}");
                }

                Assert.AreEqual(formatted, secondPass, $"{fixture.Key} [{endingName}] was not idempotent on a second formatter pass");
            }
        }

        Assert.IsEmpty(failures, string.Join(Environment.NewLine + Environment.NewLine, failures));
    }

    #endregion // Tests
}