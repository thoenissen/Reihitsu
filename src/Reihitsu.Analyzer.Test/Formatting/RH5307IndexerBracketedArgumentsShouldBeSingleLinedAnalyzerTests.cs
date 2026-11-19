using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer"/> and <see cref="RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests : BatchCodeFixTestsBase<RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer, RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that multiline indexer bracketed arguments are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-003")]
    [Requirement("RH5307-004")]
    public async Task VerifyDiagnosticAndCodeFixForMultilineIndexerArguments()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[,] matrix)
                                    {
                                        return {|#0:matrix[
                                            1,
                                            2]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static int Method(int[,] matrix)
                                     {
                                         return matrix[1, 2];
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment written behind the closing bracket does not suppress the diagnostic or
    /// the code fix. The comment sits outside the interior the fix rewrites, so it is not a join
    /// barrier.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-003")]
    [Requirement("RH5307-004")]
    public async Task VerifyDiagnosticAndCodeFixForMultilineIndexerArgumentsWithTrailingComment()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[] values)
                                    {
                                        return {|#0:values[
                                            0]|} /* note */ + 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static int Method(int[] values)
                                     {
                                         return values[0] /* note */ + 2;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment written between the brackets still suppresses the diagnostic, because
    /// the collapse would have to cross it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-002")]
    public async Task VerifyNoDiagnosticForIndexerArgumentsWithInteriorComment()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[] values)
                                    {
                                        return values[ /* note */
                                                      0] + 2;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that single-line indexer bracketed arguments remain valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-001")]
    public async Task VerifyNoDiagnosticForSingleLineIndexerArguments()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[,] matrix)
                                    {
                                        return matrix[1, 2];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiline indexer arguments with comments are ignored because they are not safely fixable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-002")]
    public async Task VerifyNoDiagnosticForMultilineIndexerArgumentsWithComments()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[,] matrix)
                                    {
                                        return matrix[
                                            1, // keep
                                            2
                                        ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiline indexer arguments with directives are ignored because they are not safely fixable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-002")]
    public async Task VerifyNoDiagnosticForMultilineIndexerArgumentsWithDirectives()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[,] matrix)
                                    {
                                        return matrix[
                                                       #if DEBUG
                                                           1,
                                                       #else
                                                           2,
                                                       #endif
                                                           3
                                                   ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that list patterns are not analyzed by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForListPattern()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is [
                                            1,
                                            2
                                        ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiline indexer arguments carrying a documentation comment are ignored, because the
    /// formatter refuses to collapse across it and the fix would otherwise never converge.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-002")]
    public async Task VerifyNoDiagnosticForMultilineIndexerArgumentsWithDocumentationComment()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[] values)
                                    {
                                        return values[
                                            /// <summary>
                                            /// index
                                            /// </summary>
                                            0];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix joins the indexer arguments when the formatter also splits an argument call
    /// that contains an attributed lambda
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-003")]
    [Requirement("RH5307-004")]
    public async Task VerifyDiagnosticAndCodeFixForIndexerArgumentCallWithAttributedLambda()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    private int this[int first, int second] => first + second;

                                    private static int Apply(Func<int, int> function, int value) => function(value);

                                    private int Method()
                                    {
                                        return {|#0:this[Apply([Obsolete] (int x) => x, 1),
                                            2]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class Example
                                 {
                                     private int this[int first, int second] => first + second;

                                     private static int Apply(Func<int, int> function, int value) => function(value);

                                     private int Method()
                                     {
                                         return this[Apply([Obsolete]
                                                           (int x) => x,
                                                           1), 2];
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped argument list of an element access is reported on the element access, also with a
    /// comment behind its closing bracket, that a wrapped implicit element access is reported on its argument list, and
    /// that an argument list on one line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-003")]
    public async Task VerifyWrappedElementAccessesAreReported()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(int[,] m)
                                    {
                                        var a = {|RH5307:m[1,
                                                  2]|};
                                        var b = {|RH5307:m[1,
                                                  2]|}; // note
                                        var c = m[1, 2];
                                        var d = new Dictionary<int, int>
                                                {
                                                    {|RH5307:[1
                                                    ]|} = 2
                                                };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a wrapped argument list with a comment between its brackets, or with an argument that spans
    /// several lines, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-002")]
    public async Task VerifyWrappedElementAccessTheFormatterCannotJoinIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] m)
                                    {
                                        var a = m[1, // note
                                                  2];
                                        var b = m[N(1,
                                                    2), 3];
                                    }

                                    int N(int a, int b) => a;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix joins the argument list onto the line of the opening bracket with a comma and a space
    /// between the arguments, and that a comment behind the closing bracket stays behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsArgumentListAndKeepsTrailingComment()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] m)
                                    {
                                        var a = {|RH5307:m[
                                            1, 2]|};
                                        var b = {|RH5307:m[1,
                                                  2]|}; // note
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] m)
                                     {
                                         var a = m[1, 2];
                                         var b = m[1, 2]; // note
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix joins the argument list onto the line of the opening bracket with a comma and a space
    /// between the arguments, and that a comment behind the closing bracket stays behind it, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsArgumentListAndKeepsTrailingCommentWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] m)
                                    {
                                        var a = {|RH5307:m[
                                            1, 2]|};
                                        var b = {|RH5307:m[1,
                                                  2]|}; // note
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] m)
                                     {
                                         var a = m[1, 2];
                                         var b = m[1, 2]; // note
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix joins the argument list of an implicit element access inside an object initializer onto
    /// one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsImplicitElementAccessArgumentList()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    {|RH5307:[1
                                                    ]|} = 2
                                                };
                                    }
                                }
                                """;
        const string fixedCode = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = new Dictionary<int, int>
                                                 {
                                                     [1] = 2
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix joins the argument list of an implicit element access inside an object initializer onto
    /// one line, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsImplicitElementAccessArgumentListWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    {|RH5307:[1
                                                    ]|} = 2
                                                };
                                    }
                                }
                                """;
        const string fixedCode = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = new Dictionary<int, int>
                                                 {
                                                     [1] = 2
                                                 };
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access that stands inside the bracketed argument list of another
    /// element access, and leaves the other list as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsElementAccessInsideAnotherArgumentList(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] x, int[,] m)
                                    {
                                        var a = x[{|RH5307:m[1,
                                                    2]|}];
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[] x, int[,] m)
                                     {
                                         var a = x[m[1, 2]];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access whose receiver is an element access with its list on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListBehindAReceiverElementAccess(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[][,] j)
                                    {
                                        var b = {|RH5307:j[0][1,
                                                     2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[][,] j)
                                     {
                                         var b = j[0][1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a comment in the list of the receiver, which is an element access, does not keep the fix from
    /// joining the reported list, and that the receiver stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListBehindAReceiverElementAccessWithAComment(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[][,] j)
                                    {
                                        var c = {|RH5307:j[0 /* c */][1,
                                                             2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[][,] j)
                                     {
                                         var c = j[0 /* c */][1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that an argument spanning several lines in the list of the receiver does not keep the fix from joining
    /// the reported list, and that the receiver's list, which is not reported, stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListBehindAReceiverElementAccessWithAMultiLineArgument(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[][,] j)
                                    {
                                        var d = {|RH5307:j[N(1,
                                                    2)][1,
                                                        2]|};
                                    }

                                    int N(int a, int b) => a;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[][,] j)
                                     {
                                         var d = j[N(1,
                                                     2)][1, 2];
                                     }

                                     int N(int a, int b) => a;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a comment in front of the reported element access inside another argument list does not keep the
    /// fix from joining the reported list, and that the space behind the comment stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixJoinsListBehindACommentInAnotherArgumentList(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] x, int[,] m)
                                    {
                                        var p = x[/* c */ {|RH5307:m[1,
                                                            2]|}];
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[] x, int[,] m)
                                     {
                                         var p = x[/* c */ m[1, 2]];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a wrapped element access whose receiver is an element binding (<c>c?[0]</c>) is reported, although
    /// the list of the element binding is not, and that the fix joins the reported list
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListBehindAnElementBinding(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[][,] c)
                                    {
                                        var e = c?{|RH5307:[0][1,
                                                      2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[][,] c)
                                     {
                                         var e = c?[0][1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access that stands inside the argument list of an element binding
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListInsideAnElementBinding(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] n, int[,] m)
                                    {
                                        var h = n?[{|RH5307:m[1,
                                                     2]|}];
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[] n, int[,] m)
                                     {
                                         var h = n?[m[1, 2]];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access whose receiver is a member access that starts with an
    /// element access
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListBehindAMemberAccessThatStartsWithAnElementAccess(string endOfLine)
    {
        const string testCode = """
                                class D
                                {
                                    public int[][,] P;
                                }

                                class C
                                {
                                    void M(D[] a)
                                    {
                                        var i = {|RH5307:a[0].P[0][1,
                                                          2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class D
                                 {
                                     public int[][,] P;
                                 }

                                 class C
                                 {
                                     void M(D[] a)
                                     {
                                         var i = a[0].P[0][1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access whose receiver is an access of the containing type's indexer
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListBehindAThisIndexerAccess(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    int[,] this[int i] => null;

                                    void M()
                                    {
                                        var j = {|RH5307:this[0][1,
                                                        2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int[,] this[int i] => null;

                                     void M()
                                     {
                                         var j = this[0][1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access that stands inside the argument list of an implicit element
    /// access in an object initializer, and leaves the initializer as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListInsideAnImplicitElementAccess(string endOfLine)
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(int[,] m)
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    [{|RH5307:m[1,
                                                       2]|}] = 1,
                                                };
                                    }
                                }
                                """;
        const string fixedCode = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M(int[,] m)
                                     {
                                         var d = new Dictionary<int, int>
                                                 {
                                                     [m[1, 2]] = 1,
                                                 };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that where both the reported list and the list of its receiver are reported, fixing one diagnostic after
    /// the other and fixing all diagnostics at once join both lists
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixesJoinBothListsWhenTheReceiverListIsReportedToo(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,][,] j)
                                    {
                                        var n = {|RH5307:{|RH5307:j[0,
                                                  1]|}[1,
                                                     2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,][,] j)
                                     {
                                         var n = j[0, 1][1, 2];
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that where both the reported list and the list of its receiver are reported, the fix of the outer
    /// diagnostic joins only the outer list and the fix of the receiver's diagnostic only the receiver's list, the rest of
    /// the access staying as written, and that applying the second fix behind the first joins both lists in either order
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyEachFixJoinsItsOwnListWhenTheReceiverListIsReportedToo(string endOfLine)
    {
        var source = ConvertEndOfLine("""
                                      class C
                                      {
                                          void M(int[,][,] j)
                                          {
                                              var n = j[0,
                                                        1][1,
                                                           2];
                                          }
                                      }
                                      """,
                                      endOfLine);
        var outerJoined = ConvertEndOfLine("""
                                           class C
                                           {
                                               void M(int[,][,] j)
                                               {
                                                   var n = j[0,
                                                             1][1, 2];
                                               }
                                           }
                                           """,
                                           endOfLine);
        var receiverJoined = ConvertEndOfLine("""
                                              class C
                                              {
                                                  void M(int[,][,] j)
                                                  {
                                                      var n = j[0, 1][1,
                                                                   2];
                                                  }
                                              }
                                              """,
                                              endOfLine);
        var bothJoined = ConvertEndOfLine("""
                                          class C
                                          {
                                              void M(int[,][,] j)
                                              {
                                                  var n = j[0, 1][1, 2];
                                              }
                                          }
                                          """,
                                          endOfLine);

        var afterOuterFix = await TryApplyCodeFixAtAsync(source,
                                                         RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                         root => root.DescendantNodes()
                                                                     .OfType<ElementAccessExpressionSyntax>()
                                                                     .Single(access => access.Expression is ElementAccessExpressionSyntax)
                                                                     .GetLocation());

        Assert.AreEqual(outerJoined, afterOuterFix, "The fix of the outer diagnostic must join only the outer list.");
        Assert.AreEqual(bothJoined, await ApplyCodeFixAsync(afterOuterFix), "The fix of the remaining diagnostic must join the receiver's list.");

        var afterReceiverFix = await TryApplyCodeFixAtAsync(source,
                                                            RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                            root => root.DescendantNodes()
                                                                        .OfType<ElementAccessExpressionSyntax>()
                                                                        .Single(access => access.Expression is IdentifierNameSyntax)
                                                                        .GetLocation());

        Assert.AreEqual(receiverJoined, afterReceiverFix, "The fix of the receiver's diagnostic must join only the receiver's list.");
        Assert.AreEqual(bothJoined, await ApplyCodeFixAsync(afterReceiverFix), "The fix of the remaining diagnostic must join the outer list.");
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access that is the argument of an invocation
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListOfAnInvocationArgument(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] m)
                                    {
                                        N({|RH5307:m[1,
                                            2]|});
                                    }

                                    void N(int a)
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] m)
                                     {
                                         N(m[1, 2]);
                                     }

                                     void N(int a)
                                     {
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access that is an element of a collection expression, and leaves
    /// the collection expression as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixJoinsListOfACollectionExpressionElement(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] m)
                                    {
                                        int[] l = [{|RH5307:m[1,
                                                     2]|}];
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] m)
                                     {
                                         int[] l = [m[1, 2]];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access whose receiver is a parenthesized element access
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListBehindAParenthesizedReceiver(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[][,] j)
                                    {
                                        var o = {|RH5307:(j[0])[1,
                                                       2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[][,] j)
                                     {
                                         var o = (j[0])[1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins a wrapped element access whose receiver is a member binding
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsListBehindAMemberBinding(string endOfLine)
    {
        const string testCode = """
                                class E
                                {
                                    public int[,] P;
                                }

                                class C
                                {
                                    void M(E c)
                                    {
                                        var g = c?{|RH5307:.P[1,
                                                     2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class E
                                 {
                                     public int[,] P;
                                 }

                                 class C
                                 {
                                     void M(E c)
                                     {
                                         var g = c?.P[1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that fixing one diagnostic after the other and fixing all diagnostics at once join every reported list of a
    /// document that holds every receiver and container of the reported access the requirement names
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixesJoinEveryReportedListWhateverItsReceiverOrContainer(string endOfLine)
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class D
                                {
                                    public int[][,] P;
                                }

                                class C
                                {
                                    int[,] this[int i] => null;

                                    void M(int[] x, int[,] m, int[][,] j, int[][,] c, int[] n, D[] a)
                                    {
                                        var va = x[{|RH5307:m[1,
                                                     2]|}];
                                        var vb = {|RH5307:j[0][1,
                                                      2]|};
                                        var vc = {|RH5307:j[0 /* c */][1,
                                                              2]|};
                                        var vd = {|RH5307:j[N(1,
                                                     2)][1,
                                                         2]|};
                                        var vp = x[/* c */ {|RH5307:m[1,
                                                             2]|}];
                                        var ve = c?{|RH5307:[0][1,
                                                       2]|};
                                        var vh = n?[{|RH5307:m[1,
                                                      2]|}];
                                        var vi = {|RH5307:a[0].P[0][1,
                                                           2]|};
                                        var vj = {|RH5307:this[0][1,
                                                         2]|};
                                        var vm = new Dictionary<int, int>
                                                 {
                                                     [{|RH5307:m[1,
                                                        2]|}] = 1,
                                                 };
                                    }

                                    int N(int a, int b) => a;
                                }
                                """;
        const string fixedCode = """
                                 using System.Collections.Generic;

                                 class D
                                 {
                                     public int[][,] P;
                                 }

                                 class C
                                 {
                                     int[,] this[int i] => null;

                                     void M(int[] x, int[,] m, int[][,] j, int[][,] c, int[] n, D[] a)
                                     {
                                         var va = x[m[1, 2]];
                                         var vb = j[0][1, 2];
                                         var vc = j[0 /* c */][1, 2];
                                         var vd = j[N(1,
                                                      2)][1, 2];
                                         var vp = x[/* c */ m[1, 2]];
                                         var ve = c?[0][1, 2];
                                         var vh = n?[m[1, 2]];
                                         var vi = a[0].P[0][1, 2];
                                         var vj = this[0][1, 2];
                                         var vm = new Dictionary<int, int>
                                                  {
                                                      [m[1, 2]] = 1,
                                                  };
                                     }

                                     int N(int a, int b) => a;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that the wrapped argument list of a null-conditional element access is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-001")]
    public async Task VerifyNullConditionalElementAccessIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1,
                                                   2];
                                    }
                                }
                                """;

        await Verify(testCode);
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
                              internal class Example
                              {
                                  private static int Method(int[] values1, int[] values2)
                                  {
                                      return values1[
                                          0] + values2[
                                          1];
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that the fix joins only the bracketed argument list, while the misspaced declaration in front of the
    /// element access and the code behind it stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixLeavesTheCodeAroundTheElementAccessAsWritten(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int[,] m)
                                    {
                                        var  a={|RH5307:m[
                                            1, 2]|}+1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int[,] m)
                                     {
                                         var  a=m[1, 2]+1;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    private static int Method(int[] values1, int[] values2)
                                    {
                                        return {|#0:values1[
                                            0]|} + {|#1:values2[
                                            1]|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     private static int Method(int[] values1, int[] values2)
                                     {
                                         return values1[0] + values2[1];
                                     }
                                 }
                                 """;

        // The two indexer accesses sit side by side in one return statement, so the fixes of the two
        // diagnostics edit neighboring spans of the same statement
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}