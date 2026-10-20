using System.Threading.Tasks;

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
    /// Characterizes the current behavior that no fix is offered for a wrapped element access inside another bracketed
    /// argument list, while the diagnostic is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForElementAccessInsideAnotherArgumentList()
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

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes the current behavior that no fix is offered for a wrapped element access whose receiver starts
    /// with an element access that holds a comment, while the diagnostic is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenReceiverStartsWithElementAccess()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[][,] j)
                                    {
                                        var a = {|RH5307:j[0 /* c */][1,
                                                             2]|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
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

        // The two indexer accesses sit side by side in one return statement, so both resolve to the identical
        // fix target: the enclosing return statement. The batch fixer discards the second, overlapping action,
        // and the surviving single fix already collapses both bracketed argument lists to a single line
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}