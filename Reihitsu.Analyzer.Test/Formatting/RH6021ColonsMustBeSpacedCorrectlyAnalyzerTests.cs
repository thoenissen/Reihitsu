using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6021ColonsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6021ColonsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6021ColonsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6021ColonsMustBeSpacedCorrectlyAnalyzer, RH6021ColonsMustBeSpacedCorrectlyCodeFixProvider>
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
                                internal class TestClass : System.IDisposable
                                {
                                    public void Dispose()
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
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass{|#0::|}System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass : System.IDisposable
                                 {
                                     public void Dispose()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6021MessageFormat));
    }

    /// <summary>
    /// Verifies that a constructor initializer colon without spaces is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConstructorInitializerColonIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public TestClass()
                                    {
                                    }

                                    public TestClass(int value){|#0::|}this()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public TestClass()
                                     {
                                     }

                                     public TestClass(int value) : this()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6021MessageFormat));
    }

    /// <summary>
    /// Verifies that a base list colon that starts a continuation line is not flagged, because the formatter leaves
    /// the layout of the line boundary to the indentation handling
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBaseListColonOnContinuationLineIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                    : System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a constructor initializer colon that starts a continuation line is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConstructorInitializerColonOnContinuationLineIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public TestClass()
                                    {
                                    }

                                    public TestClass(int value)
                                        : this()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that only the flagged same-line side is fixed when the other side of the colon starts a
    /// continuation line, so the line break is not joined onto one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOnlyMissingTrailingSpaceIsFixedWhenLeadingSideIsLineBroken()
    {
        const string testData = """
                                internal class TestClass
                                    {|#0::|}System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                     : System.IDisposable
                                 {
                                     public void Dispose()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6021MessageFormat));
    }

    /// <summary>
    /// Verifies that only the flagged same-line side is fixed when the other side of the colon ends a
    /// continuation line, so the line break after the colon is not joined onto one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOnlyMissingLeadingSpaceIsFixedWhenTrailingSideIsLineBroken()
    {
        const string testData = """
                                internal class TestClass{|#0::|}
                                    System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass :
                                     System.IDisposable
                                 {
                                     public void Dispose()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6021MessageFormat));
    }

    /// <summary>
    /// Verifies that no fix is offered when both sides of the colon are already spaced, exercising the
    /// defensive branch that guards against an otherwise unreachable no-op edit
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoActionIsOfferedWhenBothSidesAreAlreadySpaced()
    {
        const string testData = """
                                internal class TestClass : System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root => root.DescendantTokens().Single(token => token.IsKind(SyntaxKind.ColonToken)).GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a preprocessor directive sitting on the line-broken side of the colon survives the fix,
    /// instead of being deleted along with the joined line break (issue #408)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveOnLineBrokenSideSurvivesFix()
    {
        const string testData = """
                                internal class TestClass
                                #if NET5_0
                                    :System.IDisposable
                                #endif
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 #if NET5_0
                                     : System.IDisposable
                                 #endif
                                 {
                                     public void Dispose()
                                     {
                                     }
                                 }
                                 """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData), "NET5_0");

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), fixedSource);
    }

    /// <summary>
    /// Verifies that a comment sitting on the untouched line-broken side of the colon survives the fix,
    /// because the replacement span never reaches across the line break to the leading side
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentOnLineBrokenSideSurvivesFix()
    {
        const string testData = """
                                internal class TestClass // keep this comment
                                    {|#0::|}System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass // keep this comment
                                     : System.IDisposable
                                 {
                                     public void Dispose()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6021MessageFormat));
    }

    /// <summary>
    /// Verifies that no fix is offered when a comment sits inside the flagged same-line gap, since applying
    /// the fix would otherwise delete or glue the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoActionIsOfferedWhenCommentIsInFlaggedGap()
    {
        const string testData = """
                                internal class TestClass :/* comment */System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root => root.DescendantTokens().Single(token => token.IsKind(SyntaxKind.ColonToken)).GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}