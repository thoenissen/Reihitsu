using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer"/> and <see cref="RH8301DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzerTests : BatchCodeFixTestsBase<RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer, RH8301DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// Summary.
                                    void Method()
                                    {
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
    [Requirement("RH8301-001")]
    [Requirement("RH8301-003")]
    [Requirement("RH8301-004")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:///|}Summary.
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// Summary.
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that indentation used for nested list content does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    public async Task VerifyNestedListIndentationDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Supported modes:
                                    /// - Standard
                                    ///   - Fast
                                    ///   - Safe
                                    /// - Advanced
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that raw strings containing documentation-like text do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-002")]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       ///Not documentation
                                                       """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a tab after the required space is treated as content indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    public async Task VerifyTabContentIndentationDoesNotProduceDiagnostics()
    {
        const string testDataWithTabMarker = """
                                             internal class TestClass
                                             {
                                                 /// <summary>
                                                 /// {TAB}Summary.
                                                 /// </summary>
                                                 void Method()
                                                 {
                                                 }
                                             }
                                             """;
        var testData = testDataWithTabMarker.Replace("{TAB}", "\t");

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that fixing an invalid separator preserves the indentation which follows it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    [Requirement("RH8301-003")]
    [Requirement("RH8301-004")]
    public async Task VerifyInvalidSeparatorFixPreservesContentIndentation()
    {
        const string testDataWithTabMarker = """
                                             internal class TestClass
                                             {
                                                 /// <summary>
                                                 {|#0:///|}{TAB}  - Fast
                                                 /// </summary>
                                                 void Method()
                                                 {
                                                 }
                                             }
                                             """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     ///   - Fast
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;
        var testData = testDataWithTabMarker.Replace("{TAB}", "\t");

        await Verify(testData, fixedData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that extra whitespace without content is detected and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    [Requirement("RH8301-003")]
    [Requirement("RH8301-004")]
    public async Task VerifyWhitespaceOnlySuffixIsDetectedAndFixed()
    {
        const string testDataWithWhitespaceMarker = """
                                                    internal class TestClass
                                                    {
                                                        {|#0:///|}{WHITESPACE}
                                                        void Method()
                                                        {
                                                        }
                                                    }
                                                    """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     ///
                                     void Method()
                                     {
                                     }
                                 }
                                 """;
        var testData = testDataWithWhitespaceMarker.Replace("{WHITESPACE}", "  ");

        await Verify(testData, fixedData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that documentation-like text in multi-line comments does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-002")]
    public async Task VerifyMultiLineCommentsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /*
                                    ///Not documentation
                                    */
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that documentation-like text in disabled regions does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-002")]
    public async Task VerifyDisabledTextDoesNotProduceDiagnostics()
    {
        const string testData = """
                                #if false
                                ///Not documentation
                                #endif
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that documentation lines are ignored when documentation mode is disabled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    ///Summary.
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that content directly after the slashes and content after a tab are reported on the slashes, while
    /// indentation after the single space and a line that holds only the slashes are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    [Requirement("RH8301-003")]
    public async Task VerifyMissingAndTabSeparatorsAreReportedOnTheSlashes()
    {
        var testData = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   /// <summary>
                                                   {|#0:///|}Runs
                                                   {|#1:///|}→the service
                                                   ///   - nested item
                                                   ///
                                                   /// </summary>
                                                   void M()
                                                   {
                                                   }
                                               }
                                               """);

        await Verify(testData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a line holding only a tab after the slashes is reported and keeps nothing after the slashes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    [Requirement("RH8301-004")]
    public async Task VerifyTabOnlySuffixIsDetectedAndRemoved()
    {
        var testData = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   /// <summary>
                                                   /// Runs
                                                   {|#0:///|}→
                                                   /// </summary>
                                                   void M()
                                                   {
                                                   }
                                               }
                                               """);
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Runs
                                     ///
                                     /// </summary>
                                     void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that a line holding only a single space after the slashes is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    public async Task VerifySingleSpaceOnlySuffixDoesNotProduceDiagnostics()
    {
        var testData = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   /// <summary>
                                                   /// Runs
                                                   ///·
                                                   /// </summary>
                                                   void M()
                                                   {
                                                   }
                                               }
                                               """);

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a no-break space used as the separator is reported and replaced by a space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    [Requirement("RH8301-004")]
    public async Task VerifyNoBreakSpaceSeparatorIsDetectedAndReplaced()
    {
        const string testDataWithMarker = """
                                          class C
                                          {
                                              /// <summary>
                                              {|#0:///|}{NBSP}Runs
                                              /// </summary>
                                              void M()
                                              {
                                              }
                                          }
                                          """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     void M()
                                     {
                                     }
                                 }
                                 """;
        var testData = testDataWithMarker.Replace("{NBSP}", " ");

        await Verify(testData, fixedData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that four-slash comments and lines inside block comments, a verbatim string, a delimited
    /// documentation comment, and excluded code are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-002")]
    public async Task VerifyLinesOutsideDocumentationCommentsDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    ////Ordinary
                                    int a;

                                    /*
                                    ///Block
                                    */
                                    string s = @"
                                ///Literal
                                ";

                                    /**
                                    ///Delimited
                                    */
                                    int b;
                                #if NEVER
                                    ///Excluded
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix writes one space after the slashes, replaces a tab separator by it, and keeps the
    /// indentation that follows the separator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-001")]
    [Requirement("RH8301-003")]
    [Requirement("RH8301-004")]
    public async Task VerifyFixWritesTheSeparatorAndKeepsTheContent()
    {
        var testData = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   /// <summary>
                                                   {|#0:///|}Runs
                                                   {|#1:///|}→  - nested item
                                                   /// </summary>
                                                   void M()
                                                   {
                                                   }
                                               }
                                               """);
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Runs
                                     ///   - nested item
                                     /// </summary>
                                     void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example: lines of a comment that holds an element without an end tag, and of a
    /// comment kept behind code, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-002")]
    public async Task VerifySpecificationExampleCommentsLeftAsWrittenDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    ///<summary>S
                                    int A;

                                    int B; ///<remarks>a
                                    ///b</remarks>
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the line of a summary without an end tag is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-002")]
    public async Task VerifyCommentWithElementWithoutEndTagDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    ///<summary>S
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the lines of a comment whose element is closed by an end tag of another name are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-002")]
    public async Task VerifyCommentWithElementClosedByEndTagOfAnotherNameDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    ///<summary>
                                    ///S</returns>
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the lines of a comment kept behind the last field of a type are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8301-002")]
    public async Task VerifyCommentKeptBehindCodeDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    int B; ///<remarks>a
                                    ///b</remarks>
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    {|#0:///|}Summary.
                                    {|#1:///|}</summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Summary.
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        // Verifies that continuation lines are detected and fixed together
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}