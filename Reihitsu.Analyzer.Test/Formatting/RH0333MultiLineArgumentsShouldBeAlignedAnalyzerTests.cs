using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0333MultiLineArgumentsShouldBeAlignedAnalyzer"/> and <see cref="RH0333MultiLineArgumentsShouldBeAlignedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0333MultiLineArgumentsShouldBeAlignedAnalyzerTests : AnalyzerTestsBase<RH0333MultiLineArgumentsShouldBeAlignedAnalyzer, RH0333MultiLineArgumentsShouldBeAlignedCodeFixProvider>
{
    /// <summary>
    /// Verifying that correctly aligned arguments produce no diagnostics
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
    /// Verifying that misaligned arguments are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, resultData, Diagnostics(RH0333MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH0333MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that inconsistently aligned arguments are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, resultData, Diagnostics(RH0333MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH0333MessageFormat));
    }
}