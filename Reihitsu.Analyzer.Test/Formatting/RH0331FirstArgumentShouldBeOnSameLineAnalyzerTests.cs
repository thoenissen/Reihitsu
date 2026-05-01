using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0331FirstArgumentShouldBeOnSameLineAnalyzer"/> and <see cref="RH0331FirstArgumentShouldBeOnSameLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0331FirstArgumentShouldBeOnSameLineAnalyzerTests : AnalyzerTestsBase<RH0331FirstArgumentShouldBeOnSameLineAnalyzer, RH0331FirstArgumentShouldBeOnSameLineCodeFixProvider>
{
    #region Members

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

        await Verify(testData, resultData, Diagnostics(RH0331FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH0331MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH0331FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, AnalyzerResources.RH0331MessageFormat));
    }

    #endregion // Members
}