using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5101FirstArgumentShouldBeOnSameLineAnalyzer"/> and <see cref="RH5101FirstArgumentShouldBeOnSameLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests : AnalyzerTestsBase<RH5101FirstArgumentShouldBeOnSameLineAnalyzer, RH5101FirstArgumentShouldBeOnSameLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that valid argument placements produce no diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

                                        // First arg on same line, rest on subsequent lines
                                        Console.WriteLine("test1",
                                                          "test2",
                                                          "test3");

                                        // Single argument
                                        Console.WriteLine("test1");

                                        // No arguments
                                        Console.WriteLine();

                                        // Constructor call
                                        var obj = new System.Text.StringBuilder("initial",
                                                                                16);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that first argument on a new line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFirstArgumentOnNewLineIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine(
                                            {|#0:"test1"|},
                                            "test2");
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
                                                            "test2");
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that first argument on a new line in a constructor call is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFirstArgumentOnNewLineInConstructorCallIsDetectedAndFixed()
    {
        const string testData = """
                                using System;
                                using System.Text;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var sb = new StringBuilder(
                                            {|#0:"initial"|},
                                            16);
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Text;

                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          var sb = new StringBuilder("initial",
                                                                     16);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH5101MessageFormat));
    }

    /// <summary>
    /// Verifying that an argument list carrying a comment in the join gap is not flagged, because the formatter
    /// refuses to collapse the first argument across that comment (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentedArgumentListIsNotFlagged()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine( // note
                                            "test1",
                                            "test2");
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that no code fix action is registered for an argument list carrying a comment in the join gap,
    /// so the code fix does not offer a no-op action (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentedArgumentListIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   using System;

                                   internal class TestClass
                                   {
                                       void Method()
                                       {
                                           Console.WriteLine( // note
                                               "test1",
                                               "test2");
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .Arguments
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a documentation comment in the join gap is gated like other comments, so the gate and the
    /// formatter agree on what counts as a comment (issue #226)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentedArgumentListIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   using System;

                                   internal class TestClass
                                   {
                                       void Method()
                                       {
                                           Console.WriteLine(
                                               /// note
                                               "test1",
                                               "test2");
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ArgumentListSyntax>()
                                                               .First()
                                                               .Arguments
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}