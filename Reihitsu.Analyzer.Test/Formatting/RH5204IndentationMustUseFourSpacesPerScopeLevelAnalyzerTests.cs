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
public class RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests : AnalyzerTestsBase<RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer, RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean indentation does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// never reset its line-start tracking after documentation comment trivia (issue #429)
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
    /// Verifies that multiple indentation issues are detected and fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleIndentationIssuesAreDetectedAndFixed()
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

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 2,
                     Diagnostics(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH5204MessageFormat, 6));
    }

    /// <summary>
    /// Verifies that fixing one scope does not reformat an unrelated method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that a correctly indented, unbraced <c>while</c> body does not produce a diagnostic (issue #416).
    /// The body is a keyword-led <c>return</c> statement because <c>ShouldAnalyzeToken</c> only analyzes tokens
    /// whose immediate parent is a <see cref="Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax"/>, so an
    /// expression-statement body would not exercise the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that a correctly indented, unbraced <c>for</c> body does not produce a diagnostic (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that a correctly indented, unbraced <c>foreach</c> body does not produce a diagnostic (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that a correctly indented, unbraced <c>using</c> body does not produce a diagnostic (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that a correctly indented, unbraced <c>lock</c> body does not produce a diagnostic (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that a correctly indented, unbraced <c>do</c> body does not produce a diagnostic (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that a correctly indented, unbraced <c>fixed</c> body does not produce a diagnostic (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that correctly indented, unbraced nested <c>if</c> bodies do not produce a diagnostic (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// diagnostic, and that the chained <c>if</c> keywords stay flat rather than accumulating indentation (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// deeper than the <c>while</c> statement (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// deeper than the <c>fixed</c> statement (issue #416)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// deeper than the outer <c>while</c> statement (issue #416). Nested <c>while</c> loops are used instead of
    /// nested <c>if</c> statements because the code fix formats the smallest enclosing multi-line scope, and for
    /// an unbraced <c>if</c> body that scope is the <c>if</c> statement itself, whose braces are then normalized
    /// by the structural-transform phase — a separate, pre-existing concern unrelated to indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}