using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6012ClosingGenericBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer, RH6012ClosingGenericBracketsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List<int>();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List<int{|#0: |}>();
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         _ = new List<int>();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));
    }

    /// <summary>
    /// Verifies that a space before the closing generic bracket of a type parameter list is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTypeParameterListIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method<T{|#0: |}>(T value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method<T>(T value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab before a closing generic bracket is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabBeforeClosingGenericBracketIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int{|#0:	|}> Method() => new();
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     List<int> Method() => new();
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix preserves a comment before a closing generic bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeClosingGenericBracketIsPreservedByCodeFix()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int /* Keep. */{|#0: |}> Method() => new();
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     List<int /* Keep. */> Method() => new();
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));
    }

    /// <summary>
    /// Verifies that a closing generic bracket on a continuation line does not produce a diagnostic with LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingGenericBracketDoesNotProduceDiagnostic()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                        > Method() => new();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a closing generic bracket on a continuation line does not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingGenericBracketDoesNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                        > Method() => new();
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line closing generic bracket do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineClosingGenericBracketDoNotProduceDiagnostic()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                #if false
                                    disabled text
                                #endif
                                        > Method() => new();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line closing generic bracket do not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineClosingGenericBracketDoNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                #if false
                                    disabled text
                                #endif
                                        > Method() => new();
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int{|#0: |}> First() => new();
                                    List<string{|#1:	|}> Second() => new();
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     List<int> First() => new();
                                     List<string> Second() => new();
                                 }
                                 """;

        // Verifies that Fix All removes multiple same-line whitespace runs in one iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}