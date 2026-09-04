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

        await Verify(testData, resultData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat, 2));
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

        await Verify(testData, resultData, Diagnostics(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5203MessageFormat));
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