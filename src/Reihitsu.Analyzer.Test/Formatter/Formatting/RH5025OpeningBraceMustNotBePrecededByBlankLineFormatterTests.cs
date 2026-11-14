using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5025OpeningBraceMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines before opening braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                             {|#0:
                             |}    {
                                     int value = 0;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         int value = 0;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5025MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter keeps the blank line above a block statement that follows a statement not ending with a
    /// closing brace, and that the result is analyzer-clean
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-005")]
    [Requirement("FMT-BLANK-017")]
    public async Task VerifyFormatterKeepsBlankLineAboveBlockStatementAfterOtherStatement()
    {
        const string source = """
                              internal class Example
                              {
                                  void Method()
                                  {
                                      int value = 0;

                                      {
                                          value++;
                                      }
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter keeps the blank line above a block statement that follows a statement ending with a
    /// closing brace, and that the result is analyzer-clean
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-005")]
    [Requirement("FMT-BLANK-017")]
    public async Task VerifyFormatterKeepsBlankLineAboveBlockStatementAfterClosingBrace()
    {
        const string source = """
                              internal class Example
                              {
                                  void Method(bool flag)
                                  {
                                      if (flag)
                                      {
                                      }

                                      {
                                      }
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies code with a blank line directly below a region directive is left unchanged by the formatter and not
    /// reported, so the formatter and the analyzer agree
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-007")]
    public async Task VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var d = new System.Collections.Generic.Dictionary<string, int>
                                             {
                                                 { "a", 1 },

                                     #region A

                                                 { "b", 2 },

                                     #endregion // A

                                                 { "c", 3 }
                                             };
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between an own-line comment and the opening brace of an array
    /// initializer, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindCommentBeforeOpeningBraceOfArrayInitializer()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[] g;
                                     g = new int[]

                                         // c
                             {|#0:
                             |}            {
                                             1,
                                             2

                                             // d

                                         };
                                 }
                             }
                             """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         int[] g;
                                         g = new int[]

                                             // c
                                             {
                                                 1,
                                                 2

                                                 // d
                                             };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5025MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between an own-line comment and the opening brace of a nested
    /// element of a multi-dimensional array initializer, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindCommentBeforeOpeningBraceOfNestedArrayElement()
    {
        const string input = """
                             class C
                             {
                                 int[] G = {
                                               1

                                               // d

                                           };

                                 int[,] H = {
                                                { 1 },

                                                // e
                             {|#0:
                             |}                   { 2 }
                                            };
                             }
                             """;
        const string fixedData = """
                                 class C
                                 {
                                     int[] G = {
                                                   1

                                                   // d
                                               };

                                     int[,] H = {
                                                    { 1 },

                                                    // e
                                                    { 2 }
                                                };
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5025MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between an own-line comment and the opening brace of an extension
    /// block, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindCommentBeforeOpeningBraceOfExtensionBlock()
    {
        const string input = """
                             static class E
                             {
                                 extension(int i)

                                 // g
                             {|#0:
                             |}    {
                                     public int D => i;

                                     // h

                                 }
                             }
                             """;
        const string fixedData = """
                                 static class E
                                 {
                                     extension(int i)

                                     // g
                                     {
                                         public int D => i;

                                         // h
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5025MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between an own-line comment and the opening brace of a property
    /// pattern, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindCommentBeforeOpeningBraceOfPropertyPattern()
    {
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     if (o is C

                                     // c
                             {|#0:
                             |}        { })
                                     {
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(object o)
                                     {
                                         if (o is C

                                         // c
                                         { })
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5025MessageFormat));
    }

    #endregion // Tests
}