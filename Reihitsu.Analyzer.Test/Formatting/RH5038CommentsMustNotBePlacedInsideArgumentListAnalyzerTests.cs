using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer"/>
/// </summary>
[TestClass]
public class RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzerTests : AnalyzerTestsBase<RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment before the first argument is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeFirstArgumentIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var result = Math.Max(
                                            {|#0:// first value|}
                                            1,
                                            2);
                                        _ = result;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside an attribute argument list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInAttributeArgumentListIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete(
                                        {|#0:// reason|}
                                        "Use V2")]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside an object creation argument list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInObjectCreationArgumentListIsReported()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var list = new List<int>(
                                            {|#0:// capacity|}
                                            4);
                                        _ = list;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside an element access is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInElementAccessIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        _ = items[
                                            {|#0:// first|}
                                            0];
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the closing parenthesis is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeClosingParenthesisIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(1
                                            {|#0:// last|}
                                            );
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment sharing the line with an argument is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameLineCommentAfterCommaIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(1, {|#0:// first|}
                                            2);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside the expression body of a lambda argument is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInLambdaExpressionBodyIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run((Func<int, int>)(value =>
                                            {|#0:// projection|}
                                            value));
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside nested argument lists is reported once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInNestedArgumentListIsReportedOnce()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(Run(
                                            {|#0:// inner|}
                                            1));
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a binary operator inside an argument list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterBinaryOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(a &&
                                            {|#0:// why b|}
                                            b);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the first call of a chain inside an argument list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeFirstChainCallIsReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(items
                                            {|#0:// only positive|}
                                            .Where(value => value > 0)
                                            .ToList());
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a member access that is not a call is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeMemberAccessThatIsNotCalledIsReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(items.Where(value => value > 0).ToList()
                                            {|#0:// size|}
                                            .Count);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before an element access in a chain is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeElementAccessInChainIsReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(items.Where(value => value > 0).ToArray()
                                            {|#0:// first|}
                                            [0]);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a null-forgiving operator and the following call is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeNullForgivingOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(o.ToString()!
                                            {|#0:// text|}
                                            .Trim());
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a binary operator inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeBinaryOperator()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(a
                                            // why b
                                            && b);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a null-coalescing operator inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeCoalesceOperator()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(o
                                            // fallback
                                            ?? items);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a later call of a chain inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeLaterChainCall()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(items.Where(value => value > 0)

                                                     // materialize
                                                     .ToList());
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a later conditional call of a chain inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeLaterConditionalChainCall()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(items?.Where(value => value > 0)

                                                      // materialize
                                                      ?.ToList());
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a call that follows an element access in a chain is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeCallAfterElementAccess()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(items.Where(value => value > 0).ToArray()[0]

                                                     // text
                                                     .ToString());
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a null-forgiving call of a chain is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeNullForgivingCall()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(o.ToString()

                                              // text
                                              !.Trim());
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment between the invoked method and its argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeOpeningParenthesis()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run
                                            // arguments
                                            (1);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a statement inside a lambda block passed as an argument is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForStatementInLambdaBlock()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run((Func<int>)(() =>
                                        {
                                            // explanation
                                            return 1;
                                        }));
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before an initializer element inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInitializerElement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(new[]
                                            {
                                                // first
                                                1,
                                                2
                                            });
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a collection expression element inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCollectionExpressionElement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run((int[])[
                                                // first
                                                1,
                                                2
                                            ]);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before an anonymous object member inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForAnonymousObjectMember()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(new
                                            {
                                                // first
                                                First = 1
                                            });
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a switch expression arm inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSwitchExpressionArm()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(o switch
                                            {
                                                // text
                                                string => 1,
                                                _ => 2
                                            });
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a query clause inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForQueryClause()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(from item in items
                                            // only positive
                                            where item > 0
                                            select item);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside an interpolation hole inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInterpolationHole()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run($"{a /* flag */}");
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in disabled code inside an argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentInDisabledCode()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(1,
                                        #if UNDEFINED_SYMBOL
                                            // second
                                        #endif
                                            2);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in active conditional code inside an argument list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInActiveConditionalCodeIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(1,
                                        #if !UNDEFINED_SYMBOL
                                            {|#0:// second|}
                                        #endif
                                            2);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer.DiagnosticId, AnalyzerResources.RH5038MessageFormat));
    }

    #endregion // Tests
}