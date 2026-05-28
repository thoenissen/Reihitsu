using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Tests for <see cref="RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer, RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that correctly aligned raw string literals do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyValidRawStringLiteralsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void ValidSingleLine()
                                    {
                                        var a = """Test""";
                                    }

                                    void ValidMultiLineAligned()
                                    {
                                        var a = """
                                                Test
                                                """;
                                    }

                                    void ValidInterpolatedAligned()
                                    {
                                        var x = 1;

                                        var a = $"""
                                                 {x} Test
                                                 """;
                                    }

                                    void ValidDoubleInterpolatedAligned()
                                    {
                                        var x = 1;

                                        var a = $$"""
                                                  {{x}} Test
                                                  """;
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that correctly aligned raw string literals with four quotes do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyValidFourQuoteRawStringLiteralsDoNotProduceDiagnostics()
    {
        const string testData = """""
                                using System;

                                internal class RH5202
                                {
                                    void ValidFourQuotes()
                                    {
                                        var a = """"
                                                Test """
                                                """";
                                    }
                                }
                                """"";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that misaligned raw string literals are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedRawStringLiteralsAreDetectedAndFixed()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedClosingLess()
                                    {
                                        var a = {|#0:"""
                                    Test
                                    """|};
                                    }

                                    void MisalignedClosingMore()
                                    {
                                        var b = {|#1:"""
                                                        Test
                                                        """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedClosingLess()
                                      {
                                          var a = """
                                                  Test
                                                  """;
                                      }

                                      void MisalignedClosingMore()
                                      {
                                          var b = """
                                                  Test
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that misaligned raw string literals with four quotes are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedFourQuoteRawStringLiteralsAreDetectedAndFixed()
    {
        const string testData = """""
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedFourQuotes()
                                    {
                                        var a = {|#0:""""
                                    Test """
                                    """"|};
                                    }
                                }
                                """"";

        const string resultData = """""
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedFourQuotes()
                                      {
                                          var a = """"
                                                  Test """
                                                  """";
                                      }
                                  }
                                  """"";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that misaligned interpolated raw string literals are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedInterpolatedRawStringLiteralsAreDetectedAndFixed()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedInterpolated()
                                    {
                                        var x = 1;

                                        var a = {|#0:$"""
                                    {x} Test
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedInterpolated()
                                      {
                                          var x = 1;

                                          var a = $"""
                                                   {x} Test
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that misaligned double-dollar interpolated raw string literals are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedDoubleInterpolatedRawStringLiteralsAreDetectedAndFixed()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedDoubleInterpolated()
                                    {
                                        var x = 1;

                                        var a = $$"""
                                    {{x}} Test
                                    """;
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedDoubleInterpolated()
                                      {
                                          var x = 1;

                                          var a = $$"""
                                                    {{x}} Test
                                                    """;
                                      }
                                  }
                                  """";

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         test.TestState.MarkupHandling = MarkupMode.None;
                         test.FixedState.MarkupHandling = MarkupMode.None;
                     },
                     Diagnostic(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId).WithSpan(9, 17, 11, 8)
                                                                                                       .WithMessage(AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that a raw string literal with multiple content lines is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedMultiLineContentRawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void MultiLineContent()
                                    {
                                        var a = {|#0:"""
                                    Line 1
                                    Line 2
                                    Line 3
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MultiLineContent()
                                      {
                                          var a = """
                                                  Line 1
                                                  Line 2
                                                  Line 3
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 1));
    }

    #endregion // Tests
}