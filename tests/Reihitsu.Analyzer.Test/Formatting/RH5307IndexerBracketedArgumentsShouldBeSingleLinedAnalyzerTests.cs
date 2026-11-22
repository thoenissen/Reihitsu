using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer"/> and <see cref="RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests : BatchCodeFixTestsBase<RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer, RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider>
{
    #region Methods

    /// <summary>
    /// Verifies the RH5115 diagnostics marked in the source and the result of the RH5115 code fix
    /// </summary>
    /// <param name="source">The source text with markup for every expected RH5115 diagnostic</param>
    /// <param name="fixedSource">The expected fixed source text</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyIndexAccessFix(string source, string fixedSource)
    {
        var test = new CSharpCodeFixVerifierTest<RH5115IndexAccessMustStayOnExpressionLineAnalyzer, RH5115IndexAccessMustStayOnExpressionLineCodeFixProvider>
                   {
                       TestCode = source,
                       FixedCode = fixedSource,
                       ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                   };

        await test.RunAsync(CancellationToken.None);
    }

    #endregion // Methods

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
                     Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, MessageFormat));
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
    /// Verifies that a wrapped element access whose receiver is an element binding (<c>c?[0]</c>) is reported, while the
    /// list of the element binding, on one line, is not, and that the fix joins the reported list
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
    /// Verifies the spec example: the wrapped list of an element access behind a null-forgiving operator and the wrapped list of a
    /// conditional element access are reported alike, the latter on its brackets, also where the conditional access goes on behind
    /// the brackets and where a comment stands in front of the <c>?</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-003")]
    public async Task VerifyConditionalAndNullForgivingElementAccessListsAreReportedAlike()
    {
        const string testCode = """
                                #nullable enable

                                class C
                                {
                                    void M(int[,]? c, int[,]? d)
                                    {
                                        var a = c?{|RH5307:[1,
                                                   2]|};
                                        var b = {|RH5307:d![1,
                                                   2]|};
                                        var f = c?{|RH5307:[
                                            1, 2]|};
                                        var j = c?{|RH5307:[1,
                                                   2]|}.ToString();
                                        var k = c /* c */ ?{|RH5307:[1,
                                                           2]|};
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the spec example: a comment between the brackets of a conditional element access keeps its wrapped list from being
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-002")]
    public async Task VerifyConditionalElementAccessWithCommentBetweenBracketsIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1, // note
                                                   2];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a directive between the brackets of a conditional element access keeps its wrapped list from being reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-002")]
    public async Task VerifyConditionalElementAccessWithDirectiveBetweenBracketsIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1,
                                #if X
                                                   2
                                #else
                                                   3
                                #endif
                                                   ];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an argument that spans several lines keeps the wrapped list of a conditional element access from being
    /// reported, and that the list of a conditional element access on one line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-002")]
    public async Task VerifyConditionalElementAccessTheFormatterKeepsIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1,
                                                   2 +
                                                   3];
                                        var b = c?[1, 2];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the spec example: the fix joins the list of a conditional element access and leaves the <c>?</c>, the code in front
    /// of it, and the code and the comment behind the <c>]</c> as written, and where an element binding is the receiver of an
    /// element access, both lists are reported and fixing all diagnostics joins both
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-003")]
    [Requirement("RH5307-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixJoinsConditionalElementAccessListsAndKeepsTheCodeAround(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c, int[,][,] e)
                                    {
                                        var a = c?{|RH5307:[1,
                                                   2]|} ?? 0;
                                        var n = e?{|RH5307:{|RH5307:[0,
                                                   1]|}[1,
                                                      2]|};
                                        var r = c?{|RH5307:[1,
                                                   2]|}; // note
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] c, int[,][,] e)
                                     {
                                         var a = c?[1, 2] ?? 0;
                                         var n = e?[0, 1][1, 2];
                                         var r = c?[1, 2]; // note
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix joins the wrapped list of a conditional element access
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-003")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsConditionalElementAccessList(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?{|RH5307:[1,
                                                   2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] c)
                                     {
                                         var a = c?[1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins the list of a conditional element access whose opening bracket ends its line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsConditionalElementAccessListWithOpeningBracketAlone(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?{|RH5307:[
                                            1, 2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] c)
                                     {
                                         var a = c?[1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins the list of a conditional element access behind a null-forgiving operator (<c>c!?[</c>)
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsConditionalElementAccessListBehindNullForgivingOperator(string endOfLine)
    {
        const string testCode = """
                                #nullable enable

                                class C
                                {
                                    void M(int[,]? c)
                                    {
                                        var a = c!?{|RH5307:[1,
                                                    2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(int[,]? c)
                                     {
                                         var a = c!?[1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins the list of an element access behind a null-forgiving operator (<c>d![</c>)
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsElementAccessListBehindNullForgivingOperator(string endOfLine)
    {
        const string testCode = """
                                #nullable enable

                                class C
                                {
                                    void M(int[,]? d)
                                    {
                                        var a = {|RH5307:d![1,
                                                   2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 #nullable enable

                                 class C
                                 {
                                     void M(int[,]? d)
                                     {
                                         var a = d![1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins the list of a conditional element access behind a member binding (<c>o?.Q?[</c>)
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsConditionalElementAccessListBehindMemberBinding(string endOfLine)
    {
        const string testCode = """
                                class D
                                {
                                    public int[,] Q;
                                }

                                class C
                                {
                                    void M(D o)
                                    {
                                        var a = o?.Q?{|RH5307:[1,
                                                      2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class D
                                 {
                                     public int[,] Q;
                                 }

                                 class C
                                 {
                                     void M(D o)
                                     {
                                         var a = o?.Q?[1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix joins the list of a conditional element access followed by a member access, and leaves the member
    /// access as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-003")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsConditionalElementAccessListFollowedByMemberAccess(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?{|RH5307:[1,
                                                   2]|}.ToString();
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] c)
                                     {
                                         var a = c?[1, 2].ToString();
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that where two conditional element accesses follow each other, both wrapped lists are reported, each on its
    /// brackets, and fixing all diagnostics joins both
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-003")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixesJoinTheListsOfTwoConditionalElementAccesses(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,][,] e)
                                    {
                                        var a = e?{|RH5307:[0,
                                                   1]|}?{|RH5307:[1,
                                                       2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,][,] e)
                                     {
                                         var a = e?[0, 1]?[1, 2];
                                     }
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that a block comment in front of the <c>?</c> keeps neither the report nor the fix, which joins the list and leaves
    /// the comment where it is
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsConditionalElementAccessListWithCommentInFrontOfQuestionMark(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c /* c */ ?{|RH5307:[1,
                                                           2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] c)
                                     {
                                         var a = c /* c */ ?[1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a block comment between the <c>?</c> and the <c>[</c> keeps neither the report nor the fix, which joins the
    /// list and leaves the comment where it is
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsConditionalElementAccessListWithCommentBehindQuestionMark(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c? /* c */ {|RH5307:[1,
                                                            2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] c)
                                     {
                                         var a = c? /* c */ [1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a comment line between the <c>?</c> and the <c>[</c> keeps neither the report nor the fix, which joins the
    /// list and leaves the lines in front of the <c>[</c> as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixJoinsConditionalElementAccessListBehindCommentLine(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?
                                            // c
                                            {|RH5307:[1,
                                             2]|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[,] c)
                                     {
                                         var a = c?
                                             // c
                                             [1, 2];
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that where an element binding is the receiver of an element access and both lists are wrapped, the fix of the
    /// element access's diagnostic joins only its list and the fix of the element binding's diagnostic only the binding's list, and
    /// that applying the second fix behind the first joins both lists in either order
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-004")]
    public async Task VerifyEachFixJoinsItsOwnListWhenTheReceiverIsAnElementBinding(string endOfLine)
    {
        var source = ConvertEndOfLine("""
                                      class C
                                      {
                                          void M(int[,][,] e)
                                          {
                                              var n = e?[0,
                                                         1][1,
                                                            2];
                                          }
                                      }
                                      """,
                                      endOfLine);
        var outerJoined = ConvertEndOfLine("""
                                           class C
                                           {
                                               void M(int[,][,] e)
                                               {
                                                   var n = e?[0,
                                                              1][1, 2];
                                               }
                                           }
                                           """,
                                           endOfLine);
        var receiverJoined = ConvertEndOfLine("""
                                              class C
                                              {
                                                  void M(int[,][,] e)
                                                  {
                                                      var n = e?[0, 1][1,
                                                                    2];
                                                  }
                                              }
                                              """,
                                              endOfLine);
        var bothJoined = ConvertEndOfLine("""
                                          class C
                                          {
                                              void M(int[,][,] e)
                                              {
                                                  var n = e?[0, 1][1, 2];
                                              }
                                          }
                                          """,
                                          endOfLine);

        var afterOuterFix = await TryApplyCodeFixAtAsync(source,
                                                         RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                         root => root.DescendantNodes()
                                                                     .OfType<ElementAccessExpressionSyntax>()
                                                                     .Single()
                                                                     .GetLocation());

        Assert.AreEqual(outerJoined, afterOuterFix, "The fix of the element access's diagnostic must join only its own list.");
        Assert.AreEqual(bothJoined, await ApplyCodeFixAsync(afterOuterFix), "The fix of the remaining diagnostic must join the element binding's list.");

        var afterReceiverFix = await TryApplyCodeFixAtAsync(source,
                                                            RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                            root => root.DescendantNodes()
                                                                        .OfType<ElementBindingExpressionSyntax>()
                                                                        .Single()
                                                                        .GetLocation());

        Assert.AreEqual(receiverJoined, afterReceiverFix, "The fix of the element binding's diagnostic must join only its own list.");
        Assert.AreEqual(bothJoined, await ApplyCodeFixAsync(afterReceiverFix), "The fix of the remaining diagnostic must join the element access's list.");
    }

    /// <summary>
    /// Verifies that a conditional element access wrapped between the <c>?</c> and the <c>[</c> and inside its list gets an RH5307
    /// diagnostic on the list next to the RH5115 diagnostic on the gap, and that the RH5307 fix and the RH5115 fix, applied in
    /// either order, converge on the access on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5307-001")]
    [Requirement("RH5307-004")]
    public async Task VerifyFixAndIndexAccessFixConvergeInEitherOrder(string endOfLine)
    {
        const string wrappedForList = """
                                      class C
                                      {
                                          void M(int[,] c)
                                          {
                                              var a = c?
                                                  {|RH5307:[1,
                                                   2]|};
                                          }
                                      }
                                      """;
        const string wrappedForGap = """
                                     class C
                                     {
                                         void M(int[,] c)
                                         {
                                             var a = c{|RH5115:?|}
                                                 [1,
                                                  2];
                                         }
                                     }
                                     """;
        const string listJoined = """
                                  class C
                                  {
                                      void M(int[,] c)
                                      {
                                          var a = c{|RH5115:?|}
                                              [1, 2];
                                      }
                                  }
                                  """;
        const string gapJoined = """
                                 class C
                                 {
                                     void M(int[,] c)
                                     {
                                         var a = c?{|RH5307:[1,
                                              2]|};
                                     }
                                 }
                                 """;
        const string bothJoined = """
                                  class C
                                  {
                                      void M(int[,] c)
                                      {
                                          var a = c?[1, 2];
                                      }
                                  }
                                  """;

        await Verify(ConvertEndOfLine(wrappedForList, endOfLine), ConvertEndOfLine(RemoveMarkup(listJoined), endOfLine));
        await VerifyIndexAccessFix(ConvertEndOfLine(listJoined, endOfLine), ConvertEndOfLine(bothJoined, endOfLine));
        await VerifyIndexAccessFix(ConvertEndOfLine(wrappedForGap, endOfLine), ConvertEndOfLine(RemoveMarkup(gapJoined), endOfLine));
        await Verify(ConvertEndOfLine(gapJoined, endOfLine), ConvertEndOfLine(bothJoined, endOfLine));
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
                                  Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}