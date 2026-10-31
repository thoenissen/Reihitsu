using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5203MultiLineArgumentsShouldBeAlignedAnalyzer"/> and <see cref="RH5203MultiLineArgumentsShouldBeAlignedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5203MultiLineArgumentsShouldBeAlignedAnalyzerTests : BatchCodeFixTestsBase<RH5203MultiLineArgumentsShouldBeAlignedAnalyzer, RH5203MultiLineArgumentsShouldBeAlignedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that correctly aligned arguments produce no diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    public async Task VerifyNoDiagnosticsForValidCode()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        // All on one line
                                        Console.WriteLine("test1", "test2", "test3");

                                        // Correctly aligned
                                        Console.WriteLine("test1",
                                                          "test2",
                                                          "test3");

                                        // Single argument
                                        Console.WriteLine("test1");

                                        // No arguments
                                        Console.WriteLine();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an argument which follows a leading comma does not start its own line and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    public async Task VerifyArgumentAfterLeadingCommaIsNotReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine("first" // keep
                                                          , "second");
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an argument of an object creation which follows a leading comma is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    public async Task VerifyObjectCreationArgumentAfterLeadingCommaIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass(int first, int second)
                                    {
                                    }

                                    void Method()
                                    {
                                        var value = new TestClass(1 // keep
                                                                  , 2);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an argument which shares a continuation line with a previous argument is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    public async Task VerifyArgumentSharingAContinuationLineIsNotReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine("{0} {1}",
                                                          "second", "third");
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that misaligned arguments are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("RH5203-002")]
    [Requirement("RH5203-003")]
    public async Task VerifyMisalignedArgumentsAreDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine("test1",
                                          {|#0:"test2"|},
                                          {|#1:"test3"|});
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Console.WriteLine("test1",
                                                            "test2",
                                                            "test3");
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that inconsistently aligned arguments are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("RH5203-002")]
    [Requirement("RH5203-003")]
    public async Task VerifyInconsistentAlignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine("test1",
                                                     {|#0:"test2"|},
                                                          "test3");
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Console.WriteLine("test1",
                                                            "test2",
                                                            "test3");
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifying that every argument of a list whose first argument starts below the opening parenthesis is reported
    /// when it is not in the column directly right of the parenthesis
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("RH5203-002")]
    public async Task VerifyArgumentsBelowOpeningParenthesisAreReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(
                                            {|#0:1|},
                                            {|#1:2|});
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that the fix only replaces the indentation in front of the reported argument and leaves the rest of
    /// the list as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("RH5203-003")]
    public async Task VerifyFixChangesOnlyTheIndentationOfTheReportedArgument()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(1,
                                                {|#0:2|},
                                             3+4);
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call(1,
                                               2,
                                               3+4);
                                      }

                                      void Call(params int[] values)
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifying that the fix aligns the argument under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-003")]
    public async Task VerifyMisalignedArgumentIsFixedWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(1,
                                            {|#0:2|},
                                             3);
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call(1,
                                               2,
                                               3);
                                      }

                                      void Call(params int[] values)
                                      {
                                      }
                                  }
                                  """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifies that an argument below a line that ends with a comment is judged by its own column, because the
    /// comment does not start the argument's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("RH5203-003")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyArgumentBelowTrailingCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(1, /* one */
                                                {|#0:2|});
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call(1, /* one */
                                               2);
                                      }

                                      void Call(params int[] values)
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifies that an argument below a line that ends with a comment is judged by its own column under CRLF line
    /// endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("RH5203-003")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyArgumentBelowTrailingCommentIsDetectedAndFixedWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(1, /* one */
                                                {|#0:2|});
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call(1, /* one */
                                               2);
                                      }

                                      void Call(params int[] values)
                                      {
                                      }
                                  }
                                  """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifies that an argument below a comment on a line of its own is judged by its own column, because the comment
    /// does not start the argument's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyArgumentBelowCommentLineIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call(1,
                                             // two
                                                {|#0:2|});
                                    }

                                    void Call(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifies that an argument behind the closing <c>*/</c> of a comment that spans several lines is reported,
    /// because it would start its line without the comment and does not start it behind the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("SHARED-COMMENT-012")]
    public async Task VerifyArgumentBehindEndOfMultiLineCommentIsDetected()
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        P(1,
                                          /* a
                                          b */ {|#0:2|});
                                    }

                                    void P(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifies that an argument behind the closing <c>*/</c> of a comment that spans several lines is reported under
    /// CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("SHARED-COMMENT-012")]
    public async Task VerifyArgumentBehindEndOfMultiLineCommentIsDetectedWithCrLf()
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        P(1,
                                          /* a
                                          b */ {|#0:2|});
                                    }

                                    void P(params int[] values)
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifies that an argument that starts its line behind a documentation comment written behind the comma is
    /// reported outside the alignment column, and the fix moves only the argument into that column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task VerifyArgumentBehindStrayDocumentationCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal int N(int a, int b)
                                    {
                                        return N(1, /// x
                                              {|#0:2|});
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     internal int N(int a, int b)
                                     {
                                         return N(1, /// x
                                                  2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifies the control of the documentation-comment case: an argument that starts its line behind a single-line
    /// comment is reported outside the alignment column and moved into that column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyArgumentBehindSingleLineCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal int N(int a, int b)
                                    {
                                        return N(1, // x
                                              {|#0:2|});
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     internal int N(int a, int b)
                                     {
                                         return N(1, // x
                                                  2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine("test1",
                                          {|#0:"test2"|},
                                          {|#1:"test3"|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         Console.WriteLine("test1",
                                                           "test2",
                                                           "test3");
                                     }
                                 }
                                 """;

        // Each fix replaces only the leading whitespace of its own flagged argument's first token, a span
        // confined to that argument's own line, so the two occurrences here, both misaligned continuations of the
        // same argument list, can never share or abut a text span; this proves the batch fixer merges the two
        // narrow, disjoint edits cleanly within one list
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}