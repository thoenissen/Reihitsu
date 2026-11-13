using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer"/> and <see cref="RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests : BatchCodeFixTestsBase<RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer, RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// An unbraced embedded <c>if</c> inside <c>while (a)</c> that starts behind the last line of a multi-line comment, with
    /// misindented braces and body
    /// </summary>
    private const string EmbeddedIfBehindMultiLineCommentTestData = """
                                                                    class C
                                                                    {
                                                                        void M(bool a, bool b)
                                                                        {
                                                                            while (a)
                                                                                /* note
                                                                                */ if (b)
                                                                            {|#0:{|}
                                                                            X();
                                                                            {|#1:}|}
                                                                        }

                                                                        void X()
                                                                        {
                                                                        }
                                                                    }
                                                                    """;

    /// <summary>
    /// Result of the fix on <see cref="EmbeddedIfBehindMultiLineCommentTestData"/>: the reported braces stand one level deeper
    /// than the <c>while</c>; the body, which is not reported, keeps its column
    /// </summary>
    private const string EmbeddedIfBehindMultiLineCommentFixedData = """
                                                                     class C
                                                                     {
                                                                         void M(bool a, bool b)
                                                                         {
                                                                             while (a)
                                                                                 /* note
                                                                                 */ if (b)
                                                                                 {
                                                                             X();
                                                                                 }
                                                                         }

                                                                         void X()
                                                                         {
                                                                         }
                                                                     }
                                                                     """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that clean indentation does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-003")]
    public async Task VerifyNoDiagnosticsWhenIndentationIsConsistent()
    {
        const string testData = """
                                internal class Example
                                {
                                    #region Members

                                    /// <summary>
                                    /// Gets a value.
                                    /// </summary>
                                    internal bool Value
                                    {
                                        get
                                        {
                                            // Comment
                                            return true;
                                        }
                                    }

                                    #endregion // Members
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that lines within multi-line strings are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-004")]
    public async Task VerifyNoDiagnosticsForMultiLineStrings()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        var text = @"first
                                  second";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that member indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyMemberIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                  {|#0:internal|} bool Value { get; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Value { get; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a member preceded by a documentation comment is detected and fixed. Previously the
    /// code fix registered but produced no change for documented members because the underlying formatter
    /// never reset its line-start tracking after documentation comment trivia.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentedMemberIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    /// <summary>
                                    /// Gets a value.
                                    /// </summary>
                                  {|#0:internal|} bool Value { get; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /// <summary>
                                     /// Gets a value.
                                     /// </summary>
                                     internal bool Value { get; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that nested statement indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyNestedStatementIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal bool Method()
                                    {
                                         {|#0:return|} false;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Method()
                                     {
                                         return false;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that accessor indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyAccessorIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal bool Value
                                    {
                                      {|#0:get|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Value
                                     {
                                         get;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that region indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-003")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyRegionIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                  {|#0:#region Members|}

                                    internal bool Value => true;

                                    #endregion // Members
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Members

                                     internal bool Value => true;

                                     #endregion // Members
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that comment indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-002")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyCommentIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method()
                                    {
                                      {|#0:// Comment|}
                                        return;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         // Comment
                                         return;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written behind an opening brace reports no diagnostic, because a
    /// comment that does not start its line does not decide that line's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDelimitedDocumentationBehindOpeningBrace()
    {
        const string testData = """
                                internal class Example
                                { /** Doc */
                                    internal bool Value { get; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a misindented block comment on its own line below an opening brace is still detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-002")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyBlockCommentOnOwnLineBelowOpeningBraceIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                {|#0:/* Comment */|}
                                    internal bool Value { get; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /* Comment */
                                     internal bool Value { get; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented delimited documentation comment on its own line below an opening brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDelimitedDocumentationCommentOnOwnLineBelowOpeningBraceIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                /**{|#0: Doc */|}
                                    internal bool Value { get; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /** Doc */
                                     internal bool Value { get; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented delimited documentation comment on its own line is fixed under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDelimitedDocumentationCommentOnOwnLineIsDetectedAndFixedWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class Example
                                {
                                /**{|#0: Doc */|}
                                    internal bool Value { get; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /** Doc */
                                     internal bool Value { get; }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that an over-indented delimited documentation comment on its own line is moved left to the column of
    /// the member it documents
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOverIndentedDelimitedDocumentationCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                            /**{|#0: Doc */|}
                                    internal bool Value { get; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /** Doc */
                                     internal bool Value { get; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented single-line documentation comment on its own line is indented to the column of the
    /// member it documents
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineDocumentationCommentOnOwnLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                ///{|#0: Doc
                                |}    internal bool Value { get; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /// Doc
                                     internal bool Value { get; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that every line of a misindented multi-line single-line documentation comment is indented to the column
    /// of the member it documents, not only the reported first line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineSingleLineDocumentationCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                ///{|#0: <summary>
                                /// Gets a value.
                                /// </summary>
                                |}    internal bool Value { get; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /// <summary>
                                     /// Gets a value.
                                     /// </summary>
                                     internal bool Value { get; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented documentation comment before a statement is indented to the statement's column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDelimitedDocumentationCommentBeforeStatementIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal bool Method()
                                    {
                                    /**{|#0: Doc */|}
                                        return true;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Method()
                                     {
                                         /** Doc */
                                         return true;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing a misindented documentation comment formats only the documented member and leaves an
    /// unrelated method untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentCodeFixDoesNotFormatIndependentMethod()
    {
        const string testData = """
                                internal class Example
                                {
                                /**{|#0: Doc */|}
                                    internal bool First()
                                    {
                                        return true;
                                    }

                                    internal bool Second()
                                    {
                                        var value=false;return value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /** Doc */
                                     internal bool First()
                                     {
                                         return true;
                                     }

                                     internal bool Second()
                                     {
                                         var value=false;return value;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that misindented delimited, single-line documentation and block comments in one document are all fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedMisindentedCommentKindsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                /**{|#0: First */|}
                                    internal bool First { get; }

                                ///{|#1: Second
                                |}    internal bool Second { get; }

                                {|#2:/* Third */|}
                                    internal bool Third { get; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /** First */
                                     internal bool First { get; }

                                     /// Second
                                     internal bool Second { get; }

                                     /* Third */
                                     internal bool Third { get; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that fixing one scope does not reformat an unrelated method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task VerifyCodeFixDoesNotFormatIndependentMethod()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal bool First()
                                    {
                                         {|#0:return|} true;
                                    }

                                    internal bool Second()
                                    {
                                        var value=false;return value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool First()
                                     {
                                         return true;
                                     }

                                     internal bool Second()
                                     {
                                         var value=false;return value;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that namespace-level region indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-003")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyNamespaceLevelRegionIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                namespace Example
                                {
                                  {|#0:#region Members|}

                                    internal class Type
                                    {
                                    }

                                    #endregion // Members
                                }
                                """;
        const string fixedData = """
                                 namespace Example
                                 {
                                     #region Members

                                     internal class Type
                                     {
                                     }

                                     #endregion // Members
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that enum-level region indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-003")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyEnumLevelRegionIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum Example
                                {
                                  {|#0:#region Values|}
                                    One,
                                    Two,
                                    #endregion // Values
                                }
                                """;
        const string fixedData = """
                                 internal enum Example
                                 {
                                     #region Values
                                     One,
                                     Two,
                                     #endregion // Values
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a region directive inside a switch section is re-indented to the column the analyzer expects.
    /// The code fix reuses the analyzer indentation policy, so the extra switch-section scope level is honored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-003")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyRegionIndentationInsideSwitchSectionIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                            {|#0:#region Inner|}
                                                value++;
                                                break;
                                            #endregion // Inner
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 #region Inner
                                                 value++;
                                                 break;
                                             #endregion // Inner
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a correctly indented, unbraced <c>while</c> body does not produce a diagnostic.
    /// The body is a keyword-led <c>return</c> statement because <c>ShouldAnalyzeToken</c> only analyzes tokens
    /// whose immediate parent is a <see cref="Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax"/>, so an
    /// expression-statement body would not exercise the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    public async Task VerifyNoDiagnosticsForUnbracedWhileBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method(bool value)
                                    {
                                        while (value)
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly indented, unbraced <c>for</c> body does not produce a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    public async Task VerifyNoDiagnosticsForUnbracedForBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        for (var index = 0; index < 1; index++)
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly indented, unbraced <c>foreach</c> body does not produce a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    public async Task VerifyNoDiagnosticsForUnbracedForEachBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method(int[] values)
                                    {
                                        foreach (var value in values)
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly indented, unbraced <c>using</c> body does not produce a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    public async Task VerifyNoDiagnosticsForUnbracedUsingBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        using (var stream = new System.IO.MemoryStream())
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly indented, unbraced <c>lock</c> body does not produce a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    public async Task VerifyNoDiagnosticsForUnbracedLockBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    private readonly object _sync = new object();

                                    internal void Method()
                                    {
                                        lock (_sync)
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly indented, unbraced <c>do</c> body does not produce a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    public async Task VerifyNoDiagnosticsForUnbracedDoWhileBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        do
                                            return;
                                        while (true);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly indented, unbraced <c>fixed</c> body does not produce a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    public async Task VerifyNoDiagnosticsForUnbracedFixedBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal unsafe void Method(byte[] buffer)
                                    {
                                        fixed (byte* pointer = buffer)
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that correctly indented, unbraced nested <c>if</c> bodies do not produce a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    public async Task VerifyNoDiagnosticsForUnbracedNestedIfBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method(bool a, bool b)
                                    {
                                        if (a)
                                            if (b)
                                                return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an <c>else if</c> chain with correctly indented, unbraced bodies does not produce a
    /// diagnostic, and that the chained <c>if</c> keywords stay flat rather than accumulating indentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    public async Task VerifyNoDiagnosticsForElseIfChainWithUnbracedBodies()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method(int value)
                                    {
                                        if (value == 1)
                                            return;
                                        else if (value == 2)
                                            return;
                                        else
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a wrongly indented, unbraced <c>while</c> body is detected and fixed to one level
    /// deeper than the <c>while</c> statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyUnbracedWhileBodyIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method(bool value)
                                    {
                                        while (value)
                                        {|#0:return|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(bool value)
                                     {
                                         while (value)
                                             return;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrongly indented, unbraced <c>fixed</c> body is detected and fixed to one level
    /// deeper than the <c>fixed</c> statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyUnbracedFixedBodyIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal unsafe void Method(byte[] buffer)
                                    {
                                        fixed (byte* pointer = buffer)
                                        {|#0:return|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal unsafe void Method(byte[] buffer)
                                     {
                                         fixed (byte* pointer = buffer)
                                             return;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrongly indented, unbraced nested <c>while</c> body is detected and fixed to two levels
    /// deeper than the outer <c>while</c> statement. Nested <c>while</c> loops are used instead of
    /// nested <c>if</c> statements because the code fix formats the smallest enclosing multi-line scope, and for
    /// an unbraced <c>if</c> body that scope is the <c>if</c> statement itself, whose braces are then normalized
    /// by the structural-transform phase — a separate, pre-existing concern unrelated to indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyUnbracedNestedWhileBodyIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method(bool a, bool b)
                                    {
                                        while (a)
                                            while (b)
                                            {|#0:return|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(bool a, bool b)
                                     {
                                         while (a)
                                             while (b)
                                                 return;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies a statement moved off an inline switch-section block line is correctly indented for its syntax depth
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSyntaxDerivedIndentationInsideInlineSwitchBlock()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1: { Consume();

                                                    break; }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region directives inside a branch the compiler excluded are left alone. Their surroundings are
    /// untouched disabled text, so re-indenting the directive alone would half-format a region nobody compiles. The
    /// formatter already declines this in <c>LayoutComputer</c>, and the analyzer has to agree or the CLI and the IDE
    /// fix write different text for the same input
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-003")]
    public async Task VerifyNoDiagnosticsForRegionDirectivesInsideInactiveBranch()
    {
        const string testData = """
                                internal class Example
                                {
                                #if false
                                #region Disabled
                                #endregion
                                #endif

                                    internal bool Value => true;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region directives inside a branch the compiler kept are still detected and fixed. This is the
    /// active side of the boundary that <see cref="VerifyNoDiagnosticsForRegionDirectivesInsideInactiveBranch"/> owns
    /// the inactive side of
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-003")]
    [Requirement("RH5204-005")]
    [Requirement("RH5204-006")]
    public async Task VerifyRegionIndentationInsideActiveBranchIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                #if true
                                {|#0:#region Enabled|}

                                    internal bool Value => true;

                                    #endregion // Enabled
                                #endif
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                 #if true
                                     #region Enabled

                                     internal bool Value => true;

                                     #endregion // Enabled
                                 #endif
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented switch label is reported on its keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    public async Task VerifySwitchLabelIndentationIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal void Method(int value)
                                    {
                                        switch (value)
                                        {
                                              {|#0:case|} 1:
                                                break;
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented enum member is reported on its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    public async Task VerifyEnumMemberIndentationIsDetected()
    {
        const string testData = """
                                internal enum Values
                                {
                                      {|#0:First|},
                                    Second,
                                }
                                """;

        await Verify(testData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented labeled statement is reported on its label
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    public async Task VerifyLabeledStatementIndentationIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal void Method()
                                    {
                                          {|#0:Label|}: return;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented else clause and a misindented closing brace are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-005")]
    public async Task VerifyElseClauseAndClosingBraceIndentationAreDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal void Method(bool value)
                                    {
                                        if (value)
                                        {
                                            return;
                                        }
                                          {|#0:else|}
                                        {
                                            return;
                                        }
                                      {|#1:}|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a comment in front of a closing brace is expected one level deeper than the brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-002")]
    [Requirement("RH5204-005")]
    public async Task VerifyCommentInFrontOfClosingBraceIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal void Method()
                                    {
                                        return;
                                    {|#0:// end|}
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a line whose indentation contains a tab character is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-004")]
    public async Task VerifyNoDiagnosticsForTabIndentedLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal void Method()
                                    {
                                <TAB>       return;
                                    }
                                }
                                """;

        await Verify(testData.Replace("<TAB>", "\t"));
    }

    /// <summary>
    /// Verifies that a statement inside the block body of a lambda is not reported, because the body aligns to the
    /// lambda rather than to the scope depth
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-004")]
    public async Task VerifyNoDiagnosticsInsideLambdaBlockBody()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    internal Action Method()
                                    {
                                        return () =>
                                        {
                                                    return;
                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix re-indents a misindented comment in a method body under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-002")]
    [Requirement("RH5204-006")]
    public async Task VerifyCommentIndentationIsFixedWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _value;

                                    public void Method()
                                    {
                                          {|#0:// first|}
                                        return;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int _value;

                                     public void Method()
                                     {
                                         // first
                                         return;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix re-indents a misindented member by formatting the type around it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-006")]
    public async Task VerifyFieldIndentationIsFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                  {|#0:private|} int _value;

                                    public void Method()
                                    {
                                        return;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int _value;

                                     public void Method()
                                     {
                                         return;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a line which starts with a block comment in the scope's column is not reported for the column of
    /// the member behind the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForMemberBehindBlockCommentStartingTheLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /* a */   private int _a;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a line which starts with a block comment in the scope's column is not reported for the column of
    /// the member behind the comment when documentation comments are not parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForMemberBehindBlockCommentStartingTheLineWithoutDocumentationParsing()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /* a */   private int _a;
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that a line which starts with a block comment in the scope's column is not reported for the column of
    /// the member behind the comment under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForMemberBehindBlockCommentStartingTheLineWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /* a */   private int _a;
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that a line which starts with a delimited documentation comment in the scope's column is not reported
    /// for the column of the member behind the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForMemberBehindDelimitedDocumentationCommentStartingTheLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /** <summary>B</summary> */   private int _b;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a line which starts with a delimited documentation comment in the scope's column is not reported
    /// for the column of the member behind the comment when documentation comments are not parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForMemberBehindDelimitedDocumentationCommentStartingTheLineWithoutDocumentationParsing()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /** <summary>B</summary> */   private int _b;
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that a member on the line below a comment line is judged by its own column, because the comment does
    /// not start the member's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-006")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyMemberBelowBlockCommentLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /* a */
                                      {|#0:private|} int _a;
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /* a */
                                     private int _a;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a line which starts with a block comment outside the scope's column is reported on the member
    /// behind the comment, whatever column that member reaches
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyMemberBehindMisplacedBlockCommentStartingTheLineIsDetected()
    {
        const string testData = """
                                class C
                                {
                                      /* a */ {|#0:private|} int _a;
                                /**/{|#1:private|} int _b;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a line which starts with a block comment in column zero is reported on the member behind the
    /// comment, although that member stands in the scope's column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyMemberInScopeColumnBehindMisplacedBlockCommentIsDetected()
    {
        const string testData = """
                                class C
                                {
                                /**/{|#0:private|} int _b;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a line which starts with a block comment in column zero is reported on the member behind the
    /// comment, although that member stands in the scope's column, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyMemberInScopeColumnBehindMisplacedBlockCommentIsDetectedWithCrLf()
    {
        const string testData = """
                                class C
                                {
                                /**/{|#0:private|} int _b;
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a closing brace that starts its line behind a documentation comment written
    /// behind the last field is reported and moved to the column of its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task VerifyClosingBraceBehindStrayDocumentationCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class C
                                {
                                    internal int A; /// <summary>end</summary>
                                          {|#0:}|}
                                """;
        const string fixedData = """
                                 internal class C
                                 {
                                     internal int A; /// <summary>end</summary>
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies the control of the documentation-comment case: a closing brace that starts its line behind a single-line
    /// comment is reported and moved to the column of its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyClosingBraceBehindSingleLineCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class C
                                {
                                    internal int A; // end
                                          {|#0:}|}
                                """;
        const string fixedData = """
                                 internal class C
                                 {
                                     internal int A; // end
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a closing brace behind the last line of a multi-line comment does not start
    /// its line and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public async Task VerifyClosingBraceBehindLastLineOfMultiLineCommentIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    internal int A; /* a
                                    b */ }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example: at the start of a file, the first token behind a documentation comment starts
    /// its line and is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task VerifyFirstTokenOfFileBehindDocumentationCommentIsDetected()
    {
        const string testData = """
                                /// <summary>C</summary>
                                    {|#0:public|} class C
                                {
                                    private int _a;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a closing brace separated from a documentation comment written behind code by a blank line is
    /// reported, and the fix moves it to the column of its type and keeps the blank line in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-006")]
    public async Task VerifyClosingBraceBehindStrayDocumentationCommentAndBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class C
                                {
                                    internal int A; /// x

                                          {|#0:}|}
                                """;
        const string fixedData = """
                                 internal class C
                                 {
                                     internal int A; /// x

                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented statement on the line behind a documentation comment written behind the previous
    /// statement is reported, and that the fix moves only the reported line and leaves the comment where it stands, in one
    /// pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-006")]
    public async Task VerifyStatementBehindStrayDocumentationCommentIsDetectedAndFixedInOnePass()
    {
        const string testData = """
                                internal class C
                                {
                                    internal void M()
                                    {
                                        var x = 1; /// stray
                                            {|#0:return|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class C
                                 {
                                     internal void M()
                                     {
                                         var x = 1; /// stray
                                         return;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that a misindented <c>if</c> statement on the line behind a documentation comment written behind the
    /// previous statement is reported together with its braces, and that the fix moves only the reported lines and leaves
    /// the comment where it stands
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("RH5204-006")]
    public async Task VerifyIfStatementBehindStrayDocumentationCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class C
                                {
                                    internal void M()
                                    {
                                        var x = 1; /// stray
                                            {|#0:if|} (x > 0)
                                            {|#1:{|}
                                            {|#2:}|}
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class C
                                 {
                                     internal void M()
                                     {
                                         var x = 1; /// stray
                                         if (x > 0)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the fix on the misindented braces and body of an unbraced embedded <c>if</c> that starts behind the last
    /// line of a multi-line comment places them in the columns of the embedded statement, with one application for each
    /// diagnostic and one Fix All application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    [Requirement("SHARED-FIX-001")]
    public async Task VerifyEmbeddedIfBehindMultiLineCommentIsFixedWithOneApplicationPerDiagnostic()
    {
        await Verify(EmbeddedIfBehindMultiLineCommentTestData,
                     EmbeddedIfBehindMultiLineCommentFixedData,
                     static test =>
                            {
                                test.NumberOfIncrementalIterations = 2;
                                test.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix on the misindented braces and body of an unbraced embedded <c>if</c> that starts behind the last
    /// line of a multi-line comment places them in the columns of the embedded statement, with one application for each
    /// diagnostic and one Fix All application, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    [Requirement("SHARED-FIX-001")]
    public async Task VerifyEmbeddedIfBehindMultiLineCommentIsFixedWithOneApplicationPerDiagnosticWithCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(EmbeddedIfBehindMultiLineCommentTestData),
                     NormalizeToCarriageReturnLineFeed(EmbeddedIfBehindMultiLineCommentFixedData),
                     static test =>
                            {
                                test.NumberOfIncrementalIterations = 2;
                                test.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example: the fix moves only the reported line and leaves the rest of the body, its
    /// spacing, its unbraced body and its blank lines as written, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesOnlyTheReportedLineAndLeavesTheRestOfTheBodyAsWritten()
    {
        const string testCode = """
                                class C
                                {
                                    int M(bool a)
                                    {
                                        if (a) M(a);
                                        int  y = 1;
                                          {|RH5204:return|} y;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int M(bool a)
                                     {
                                         if (a) M(a);
                                         int  y = 1;
                                         return y;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix on a misindented else keyword moves only the else line and leaves the unbraced bodies of
    /// the if statement as written, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesOnlyTheMisindentedElseAndLeavesTheUnbracedBodiesAsWritten()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                            M(a);
                                          {|RH5204:else|}
                                            M(!a);
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a)
                                     {
                                         if (a)
                                             M(a);
                                         else
                                             M(!a);
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example: a wrapped argument below the reported line keeps its column, under LF and
    /// CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixKeepsTheColumnOfAWrappedArgumentBelowTheReportedLine()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                      {|RH5204:return|} M(1,
                                               2);
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int M(int a, int b)
                                     {
                                         return M(1,
                                                2);
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix on a misindented member leaves the unformatted spacing of the member's own line and of the
    /// type around it as written, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesOnlyTheReportedMemberLineAndKeepsTheSpacingBehindItsFirstToken()
    {
        const string testCode = """
                                class C
                                {
                                    int  _a;
                                      {|RH5204:private|}  int  _b;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int  _a;
                                     private  int  _b;
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example: the first type of a file behind a documentation comment is moved to column
    /// zero in one application, and its empty body is not rewritten, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesTheFirstTypeOfAFileBehindADocumentationComment()
    {
        const string testCode = """
                                /// <summary>x</summary>
                                    {|RH5204:public|} class C
                                {
                                }
                                """;
        const string fixedCode = """
                                 /// <summary>x</summary>
                                 public class C
                                 {
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the first type of a file behind a directive is moved to column zero in one application, under LF and
    /// CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesTheFirstTypeOfAFileBehindADirective()
    {
        const string testCode = """
                                #nullable enable
                                    {|RH5204:public|} class C
                                {
                                }
                                """;
        const string fixedCode = """
                                 #nullable enable
                                 public class C
                                 {
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the first type of a file without leading trivia other than its indentation is moved to column zero in
    /// one application, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesTheFirstTypeOfAFileWithoutLeadingTrivia()
    {
        const string testCode = """
                                    {|RH5204:public|} class C
                                {
                                }
                                """;
        const string fixedCode = """
                                 public class C
                                 {
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the first type of a file behind a line comment is moved to column zero and its one-line body is not
    /// re-indented, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesTheFirstTypeOfAFileBehindALineCommentAndKeepsItsBody()
    {
        const string testCode = """
                                // header
                                    {|RH5204:class|} C
                                { int _a; }
                                """;
        const string fixedCode = """
                                 // header
                                 class C
                                 { int _a; }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a namespace that starts a file is moved to column zero and its body is not reformatted, under LF and
    /// CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesANamespaceThatStartsTheFileAndKeepsItsBody()
    {
        const string testCode = """
                                    {|RH5204:namespace|} N
                                { class C { } }
                                """;
        const string fixedCode = """
                                 namespace N
                                 { class C { } }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a misindented comment that starts a file is moved to column zero and the type below it is not
    /// rewritten, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesAMisindentedCommentThatStartsTheFile()
    {
        const string testCode = """
                                    {|RH5204:// header|}
                                class C
                                {
                                }
                                """;
        const string fixedCode = """
                                 // header
                                 class C
                                 {
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix on a later top-level type moves only its line and leaves the type in front of it as written,
    /// under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesALaterTopLevelTypeAndLeavesTheTypeInFrontOfItAsWritten()
    {
        const string testCode = """
                                class A { int  _a; }

                                  {|RH5204:class|} B
                                {
                                }
                                """;
        const string fixedCode = """
                                 class A { int  _a; }

                                 class B
                                 {
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix on a type in a file-scoped namespace moves only its line, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixMovesATypeInAFileScopedNamespaceAndKeepsItsEmptyBody()
    {
        const string testCode = """
                                namespace N;

                                  {|RH5204:class|} B
                                {
                                }
                                """;
        const string fixedCode = """
                                 namespace N;

                                 class B
                                 {
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a refused comment elsewhere in the block does not withhold the fix, because the fix replaces only the
    /// whitespace in front of the reported line and formats nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-006")]
    public async Task FixIsOfferedWhenAnotherStatementOfTheBlockHoldsACommentTheFormatterRefuses()
    {
        const string testCode = """
                                internal class C
                                {
                                    private void M()
                                    {
                                        int /* c */ y = 1;
                                         {|RH5204:return|};
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class C
                                 {
                                     private void M()
                                     {
                                         int /* c */ y = 1;
                                         return;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a case label behind the closing line of a multi-line documentation comment that starts its line does
    /// not start its line and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task CaseLabelBehindTheClosingLineOfAMultiLineDocumentationCommentIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                break;
                                            /** a
                                b */ case 2:
                                                break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that a case label behind the closing line of a multi-line block comment that starts its line does not
    /// start its line and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task CaseLabelBehindTheClosingLineOfAMultiLineBlockCommentIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                break;
                                            /* a
                                b */ case 2:
                                                break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that a closing brace behind the closing line of a multi-line block comment that starts its line is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task ClosingBraceBehindTheClosingLineOfAMultiLineCommentOnItsOwnLineIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        M();
                                        /* a
                                b */ }
                                }
                                """;

        await Verify(testCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that an opening brace behind the closing line of a multi-line block comment that starts its line is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task OpeningBraceBehindTheClosingLineOfAMultiLineCommentIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        if (x > 0)
                                        /* a
                                b */ {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that an else keyword behind the closing line of a multi-line block comment that starts its line is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task ElseBehindTheClosingLineOfAMultiLineCommentIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        if (x > 0)
                                        {
                                        }
                                        /* a
                                b */ else
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that a catch keyword behind the closing line of a multi-line block comment that starts its line is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task CatchBehindTheClosingLineOfAMultiLineCommentIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        try
                                        {
                                        }
                                        /* a
                                b */ catch
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that a return statement behind the closing line of a multi-line block comment that starts its line is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task ReturnBehindTheClosingLineOfAMultiLineCommentIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        /* a
                                b */ return;
                                    }
                                }
                                """;

        await Verify(testCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that a member modifier behind the closing line of a multi-line block comment that starts its line is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task MemberBehindTheClosingLineOfAMultiLineCommentIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    private int _a;
                                    /* a
                                b */ private int _b;
                                }
                                """;

        await Verify(testCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that a misindented multi-line comment in front of a statement that stands behind its closing line is
    /// reported on the comment, and that the fix moves only the comment's first line, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-002")]
    [Requirement("RH5204-006")]
    public async Task MisindentedMultiLineCommentInFrontOfAStatementBehindItsClosingLineIsReportedAndFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                    {|RH5204:/* a
                                b */|} return;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                         /* a
                                 b */ return;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the diagnostics marked in the source and the fix result under LF and under CRLF
    /// </summary>
    /// <param name="testCode">The source with markup and LF line endings</param>
    /// <param name="fixedCode">The fixed source with LF line endings</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyUnderBothLineEndings(string testCode, string fixedCode)
    {
        await Verify(testCode, fixedCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class Example
                                {
                                  {|#0:#region Members|}

                                  {|#1:internal|} bool Method()
                                  {|#2:{|}
                                     {|#3:// Comment|}
                                     {|#4:return|} false;
                                  {|#5:}|}

                                    #endregion // Members
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Members

                                     internal bool Method()
                                     {
                                         // Comment
                                         return false;
                                     }

                                     #endregion // Members
                                 }
                                 """;

        // Verifies that multiple indentation issues are detected and fixed together
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat, 6),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}