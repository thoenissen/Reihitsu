using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6001KeywordsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6001KeywordsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6001KeywordsMustBeSpacedCorrectlyAnalyzer, RH6001KeywordsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-002")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-001")]
    [Requirement("RH6001-003")]
    [Requirement("RH6001-004")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        {|#0:if|}(true)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6001MessageFormat));
    }

    /// <summary>
    /// Verifies that operator-like keywords do not require a separating space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-002")]
    public async Task VerifyTypeofDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    System.Type Method()
                                    {
                                        return typeof(object);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that every listed statement keyword glued to its opening parenthesis is reported on the keyword and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-001")]
    [Requirement("RH6001-003")]
    [Requirement("RH6001-004")]
    public async Task VerifyEveryListedStatementKeywordIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    int Method(int a, object o, IDisposable d, int[] arr)
                                    {
                                        {|#0:if|}(a > 0)
                                        {
                                        }

                                        {|#1:while|}(a > 0)
                                        {
                                        }

                                        do
                                        {
                                        }
                                        {|#2:while|}(a > 0);

                                        {|#3:for|}(var i = 0; i < a; i++)
                                        {
                                        }

                                        {|#4:foreach|}(var x in arr)
                                        {
                                        }

                                        {|#5:switch|}(a)
                                        {
                                            default:
                                                break;
                                        }

                                        try
                                        {
                                        }
                                        {|#6:catch|}(Exception)
                                        {
                                        }

                                        {|#7:lock|}(o)
                                        {
                                        }

                                        {|#8:using|}(d)
                                        {
                                        }

                                        if (a > 1)
                                        {
                                            {|#9:throw|}(new Exception());
                                        }

                                        {|#10:return|}(a);
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class TestClass
                                 {
                                     int Method(int a, object o, IDisposable d, int[] arr)
                                     {
                                         if (a > 0)
                                         {
                                         }

                                         while (a > 0)
                                         {
                                         }

                                         do
                                         {
                                         }
                                         while (a > 0);

                                         for (var i = 0; i < a; i++)
                                         {
                                         }

                                         foreach (var x in arr)
                                         {
                                         }

                                         switch (a)
                                         {
                                             default:
                                                 break;
                                         }

                                         try
                                         {
                                         }
                                         catch (Exception)
                                         {
                                         }

                                         lock (o)
                                         {
                                         }

                                         using (d)
                                         {
                                         }

                                         if (a > 1)
                                         {
                                             throw (new Exception());
                                         }

                                         return (a);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6001MessageFormat, 11));
    }

    /// <summary>
    /// Verifies that a comment alone between the keyword and its parenthesis does not separate them, and that the
    /// fix inserts the space directly after the keyword, in front of the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-001")]
    [Requirement("RH6001-004")]
    public async Task VerifyCommentAloneDoesNotSeparateKeywordAndFixInsertsSpaceBeforeComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int a)
                                    {
                                        {|#0:if|}/* note */(a > 1)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int a)
                                     {
                                         if /* note */(a > 1)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6001MessageFormat));
    }

    /// <summary>
    /// Verifies that a keyword separated from its parenthesis by several spaces or by a line break is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-002")]
    public async Task VerifyKeywordSeparatedBySeveralSpacesOrALineBreakIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a)
                                    {
                                        while  (a > 0)
                                        {
                                        }

                                        if
                                            (a > 1)
                                        {
                                        }

                                        return (a);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the keywords that take their parenthesis like a call are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-002")]
    public async Task VerifyCallLikeKeywordsAreNotChecked()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int a)
                                    {
                                        var n = nameof(a);
                                        var s = sizeof(int);
                                        var c = checked(a + 1);
                                        var u = unchecked(a + 1);
                                        var d = default(int);
                                        object w = new();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        {|#0:if|}(true)
                                        {
                                        }
                                        {|#1:while|}(true)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                         }
                                         while (true)
                                         {
                                         }
                                     }
                                 }
                                 """;

        // The two keyword violations sit on adjacent lines with no blank line between them; each fix only
        // inserts a single space at its own keyword's boundary, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6001MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}