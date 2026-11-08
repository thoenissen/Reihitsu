using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Methods

    /// <summary>
    /// Replaces the placeholders of a fixture with the whitespace characters they stand for: <c>·</c> with a no-break space
    /// (U+00A0) and <c>→</c> with a tab
    /// </summary>
    /// <param name="text">The fixture text</param>
    /// <returns>The text with the whitespace characters</returns>
    private static string WithWhitespace(string text)
    {
        return text.Replace("·", ((char)0x00A0).ToString())
                   .Replace("→", "\t");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that the formatter realigns multi-line raw string literals
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        var input = """"
                    using System;

                    internal class Example
                    {
                        internal void Method()
                        {
                            var value = """
                        Text
                        """;
                        }
                    }
                    """";
        var fixedData = """"
                        using System;

                        internal class Example
                        {
                            internal void Method()
                            {
                                var value = """
                                            Text
                                            """;
                            }
                        }
                        """";

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 7, 21, 9, 8, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns a literal with a multi-line hole so that the hole lines move only with it, clears the diagnostic, and stays stable on a second pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public async Task VerifyFormatterAlignsMultiLineHoleAndClearsTheDiagnostic()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                 Hello {
                                     name
                                 }
                                 """;
                                 }
                             }
                             """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M()
                                     {
                                         var name = "x";
                                         var a = $"""
                                                  Hello {
                                                      name
                                                  }
                                                  """;
                                     }
                                 }
                                 """";

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 6, 17, 10, 8, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns a raw string nested in a hole together with the literal around it, clears both diagnostics, and stays stable on a second pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-RAW-008")]
    public async Task VerifyFormatterAlignsNestedRawStringInOnePassAndClearsTheDiagnostics()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Outer {"""
                                     inner
                                     """}
                                 """;
                                 }
                             }
                             """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M()
                                     {
                                         var a = $"""
                                                  Outer {"""
                                                         inner
                                                         """}
                                                  """;
                                     }
                                 }
                                 """";

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 5, 17, 9, 8, AnalyzerResources.RH5202MessageFormat),
                              ExpectedDiagnostic(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 6, 12, 8, 12, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns a literal on a line that a comment leads to its opening quotes behind the comment,
    /// clears the diagnostic, and stays stable on a second pass, while the chain link behind the closing marker aligns to its
    /// anchor where the literal moved
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-RAW-005")]
    public async Task VerifyFormatterAlignsLiteralOnCommentLedLineAndClearsTheDiagnostic()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     /* c */ var s = """
                                 a
                                 """.Trim()
                                 .ToUpper();
                                 }
                             }
                             """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M()
                                     {
                                         /* c */ var s = """
                                                         a
                                                         """.Trim()
                                                            .ToUpper();
                                     }
                                 }
                                 """";

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 5, 25, 7, 8, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns a literal whose closing line starts with four no-break spaces by the analyzer's
    /// measure, so the analyzer reports nothing on the formatted code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    [Requirement("FMT-RAW-002")]
    public async Task VerifyFormatterAlignsLiteralBehindNoBreakSpacesAndClearsTheDiagnostic()
    {
        var input = WithWhitespace("""""
                                   class C
                                   {
                                       void M()
                                       {
                                           var a = {|#0:"""
                                   ····x
                                   ····"""|};
                                       }
                                   }
                                   """"");
        var fixedData = WithWhitespace(""""
                                       class C
                                       {
                                           void M()
                                           {
                                               var a = """
                                                   ····x
                                                   ····""";
                                           }
                                       }
                                       """");

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns a literal whose closing line starts with four tabs by the analyzer's measure, one
    /// column per tab, so the analyzer reports nothing on the formatted code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public async Task VerifyFormatterAlignsLiteralBehindTabsAndClearsTheDiagnostic()
    {
        var input = WithWhitespace("""""
                                   class C
                                   {
                                       void M()
                                       {
                                           var a = {|#0:"""
                                   →→→→x
                                   →→→→"""|};
                                       }
                                   }
                                   """"");
        var fixedData = WithWhitespace(""""
                                       class C
                                       {
                                           void M()
                                           {
                                               var a = """
                                                   →→→→x
                                                   →→→→""";
                                           }
                                       }
                                       """");

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter moves a literal behind twenty no-break spaces to the left by removing four of them, so the
    /// analyzer reports nothing on the formatted code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-RAW-002")]
    public async Task VerifyFormatterMovesLiteralBehindNoBreakSpacesLeftAndClearsTheDiagnostic()
    {
        var input = WithWhitespace("""""
                                   class C
                                   {
                                       void M()
                                       {
                                           var a = {|#0:"""
                                   ····················x
                                   ····················"""|};
                                       }
                                   }
                                   """"");
        var fixedData = WithWhitespace(""""
                                       class C
                                       {
                                           void M()
                                           {
                                               var a = """
                                       ················x
                                       ················""";
                                           }
                                       }
                                       """");

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves a literal the analyzer finds aligned, 12 spaces and four no-break spaces, as the
    /// analyzer's fix writes it, untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public async Task VerifyFormatterKeepsLiteralAlignedByTheAnalyzersMeasureBehindNoBreakSpaces()
    {
        var input = WithWhitespace(""""
                                   class C
                                   {
                                       void M()
                                       {
                                           var a = """
                                               ····x
                                               ····""";
                                       }
                                   }
                                   """");

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter leaves a literal the analyzer finds aligned, 12 spaces and four tabs, as the analyzer's
    /// fix writes it, untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public async Task VerifyFormatterKeepsLiteralAlignedByTheAnalyzersMeasureBehindTabs()
    {
        var input = WithWhitespace(""""
                                   class C
                                   {
                                       void M()
                                       {
                                           var a = """
                                               →→→→x
                                               →→→→""";
                                       }
                                   }
                                   """");

        await VerifyFormatter(input);
    }

    #endregion // Tests
}