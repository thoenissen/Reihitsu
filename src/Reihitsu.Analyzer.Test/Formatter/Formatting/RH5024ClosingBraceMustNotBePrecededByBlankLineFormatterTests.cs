using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5024ClosingBraceMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines before closing braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                                 {
                                     int value = 0;
                             {|#0:
                             |}    }
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
                              Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between a line that holds two delimited comments and a closing
    /// brace, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindLineOfTwoDelimitedComments()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                                 {
                                     int value = 0;

                                     /* a */ /* b */
                             {|#0:
                             |}    }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         int value = 0;

                                         /* a */ /* b */
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies code with a blank line directly below a region directive is left unchanged by the formatter and not
    /// reported, so the formatter and the analyzer agree
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-001")]
    public async Task VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     #region R

                                 }

                                 #endregion // R
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between an own-line comment and the closing brace of an array
    /// initializer, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindCommentBeforeClosingBraceOfArrayInitializer()
    {
        const string input = """
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
                             {|#0:
                             |}            };
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
                              Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between an own-line comment and the closing brace of an extension
    /// block, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindCommentBeforeClosingBraceOfExtensionBlock()
    {
        const string input = """
                             static class E
                             {
                                 extension(int i)

                                 // g

                                 {
                                     public int D => i;

                                     // h
                             {|#0:
                             |}    }
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
                              Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between an own-line comment and the closing brace of a collection
    /// initializer assigned to a member, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindCommentBeforeClosingBraceOfAssignedCollectionInitializer()
    {
        const string input = """
                             class P
                             {
                                 public System.Collections.Generic.List<int> L { get; } = new();
                             }

                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L = {
                                                         1

                                                         // d
                             {|#0:
                             |}                        }
                                             };
                                 }
                             }
                             """;
        const string fixedData = """
                                 class P
                                 {
                                     public System.Collections.Generic.List<int> L { get; } = new();
                                 }

                                 class C
                                 {
                                     void M()
                                     {
                                         var p = new P
                                                 {
                                                     L = {
                                                             1

                                                             // d
                                                         }
                                                 };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between an own-line comment and the closing brace of an
    /// interpolation of a raw interpolated string, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindCommentBeforeClosingBraceOfRawStringInterpolation()
    {
        const string input = """"
                             class C
                             {
                                 void M(int a)
                                 {
                                     var s = $"""
                                              {
                                                  a

                                                  // f
                             {|#0:
                             |}                 }
                                              """;
                                 }
                             }
                             """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         var s = $"""
                                                  {
                                                      a

                                                      // f
                                                  }
                                                  """;
                                     }
                                 }
                                 """";

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    #endregion // Tests
}