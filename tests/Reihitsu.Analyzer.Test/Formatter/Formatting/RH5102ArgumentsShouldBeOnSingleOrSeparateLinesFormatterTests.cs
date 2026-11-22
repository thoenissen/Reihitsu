using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5102ArgumentsShouldBeOnSingleOrSeparateLinesFormatterTests : FormatterTestsBase<RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter splits mixed-line argument lists into one argument per line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System;

                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Console.WriteLine("first", "second",
                                                       "third");
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Console.WriteLine("first",
                                                           "second",
                                                           "third");
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, 7, 26, 8, 35, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter joins an argument list wrapped only behind its opening parenthesis, and that the rule reports neither the input nor the result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    public async Task VerifyFormatterJoinsListWrappedOnlyBehindOpeningParenthesisWithoutAReport()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Call(
                                         1, 2);
                                 }

                                 internal void Call(params int[] values)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Call(1, 2);
                                     }

                                     internal void Call(params int[] values)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that an argument list wrapped only in front of its closing parenthesis, which the formatter keeps, is not reported and stays unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyListWrappedOnlyInFrontOfClosingParenthesisIsKeptWithoutAReport()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Call(1, 2
                                          );
                                 }

                                 internal void Call(params int[] values)
                                 {
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that an argument list whose closing parenthesis a comment holds on a later line, which the formatter keeps, is not reported and stays unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5102-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyListWhoseClosingParenthesisACommentHoldsIsKeptWithoutAReport()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Call(1, 2 // c
                                          );
                                 }

                                 internal void Call(params int[] values)
                                 {
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    #endregion // Tests
}