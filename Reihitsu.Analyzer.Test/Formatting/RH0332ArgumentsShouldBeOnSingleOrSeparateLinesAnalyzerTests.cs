using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer"/> and <see cref="RH0332ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests : AnalyzerTestsBase<RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer, RH0332ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider>
{
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

                                        // Each on its own line
                                        Console.WriteLine("test1",
                                                          "test2",
                                                          "test3");

                                        // Single argument
                                        Console.WriteLine("test1");

                                        // No arguments
                                        Console.WriteLine();

                                        // Multi-line argument (lambda) is ok
                                        Call("test1",
                                             Nested()
                                                 .Chain(),
                                             "test3");
                                    }

                                    string Call(string a, string b, string c) => a;
                                    string Nested() => "";
                                }

                                internal static class Extensions
                                {
                                    public static string Chain(this string s) => s;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that mixed argument placement is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedArgumentPlacementIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Console.WriteLine{|#0:("test1", "test2",
                                                          "test3")|};
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

        await Verify(testData, resultData, Diagnostics(RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH0332MessageFormat));
    }

    /// <summary>
    /// Verifying that mixed argument placement with multiple groups is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedArgumentPlacementWithMultipleGroupsIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Call{|#0:("test1", "test2",
                                             "test3", "test4")|};
                                    }

                                    void Call(string a, string b, string c, string d) { }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestClass
                                  {
                                      void Method()
                                      {
                                          Call("test1",
                                               "test2",
                                               "test3",
                                               "test4");
                                      }

                                      void Call(string a, string b, string c, string d) { }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH0332MessageFormat));
    }
}