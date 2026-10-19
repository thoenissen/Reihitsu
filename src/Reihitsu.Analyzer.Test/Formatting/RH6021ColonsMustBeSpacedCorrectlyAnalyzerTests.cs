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
public class RH6021ColonsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6021ColonsMustBeSpacedCorrectlyAnalyzer, RH6021ColonsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6021-002")]
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
    [Requirement("RH6021-001")]
    [Requirement("RH6021-003")]
    [Requirement("RH6021-004")]
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
    [Requirement("RH6021-001")]
    [Requirement("RH6021-004")]
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
    [Requirement("RH6021-002")]
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
    [Requirement("RH6021-002")]
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
    [Requirement("RH6021-001")]
    [Requirement("RH6021-004")]
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
    [Requirement("RH6021-001")]
    [Requirement("RH6021-004")]
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
    /// instead of being deleted along with the joined line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6021-004")]
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
    [Requirement("RH6021-004")]
    [Requirement("RH6021-005")]
    [Requirement("SHARED-GAP-001")]
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

    /// <summary>
    /// Verifies that a base-list colon of a class, enum, record with a primary constructor, or struct, and a
    /// constructor-initializer colon, is reported once when a side has no whitespace, a tab next to the colon, or a
    /// comment that ends directly in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6021-001")]
    [Requirement("RH6021-003")]
    public async Task VerifyUnspacedBaseListAndConstructorInitializerColonsAreReported()
    {
        const string testData = """
                                interface I
                                {
                                }

                                class B
                                {
                                    public B()
                                    {
                                    }

                                    public B(int a)
                                    {
                                    }
                                }

                                class C{|RH6021::|}B
                                {
                                    public C(){|RH6021::|}base(1)
                                    {
                                    }

                                    public C(int a) {|RH6021::|}this()
                                    {
                                    }

                                    public C(long a){|RH6021::|} base()
                                    {
                                    }
                                }

                                class D→{|RH6021::|} B, I
                                {
                                }

                                class E /* note */{|RH6021::|} B
                                {
                                }

                                enum F{|RH6021::|}byte
                                {
                                }

                                record R(int A){|RH6021::|}I;

                                struct S{|RH6021::|}I
                                {
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that a colon with one space on each side, or a line break on one side and one space on the other, is
    /// not reported, and neither are the colons of a constraint, a conditional expression, a case label, or a named
    /// argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6021-002")]
    public async Task VerifySpacedOrLineBrokenColonsAndOtherColonsAreNotReported()
    {
        const string testData = """
                                class B
                                {
                                }

                                class C : B
                                {
                                }

                                class D
                                    : B
                                {
                                }

                                class E :
                                    B
                                {
                                }

                                class F<T> : B where T:class
                                {
                                    void M(int a)
                                    {
                                        _ = a > 0 ? a:1;

                                        switch (a)
                                        {
                                            case 1:
                                                break;
                                        }

                                        M(a:1);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix puts one space on each reported side of the colon and leaves a line-broken side unchanged,
    /// including the comment on it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6021-001")]
    [Requirement("RH6021-004")]
    [Requirement("RH6021-005")]
    [Requirement("SHARED-GAP-001")]
    public async Task VerifyFixPutsOneSpaceOnEachReportedSide()
    {
        const string testData = """
                                class B
                                {
                                    public B()
                                    {
                                    }
                                }

                                class C{|RH6021::|}B
                                {
                                    public C(){|RH6021::|}base()
                                    {
                                    }
                                }

                                class D→{|RH6021::|} B
                                {
                                }

                                class E
                                    {|RH6021::|}B
                                {
                                }

                                class F{|RH6021::|}
                                    B
                                {
                                }

                                class G // note
                                    {|RH6021::|}B
                                {
                                }
                                """;
        const string fixedData = """
                                 class B
                                 {
                                     public B()
                                     {
                                     }
                                 }

                                 class C : B
                                 {
                                     public C() : base()
                                     {
                                     }
                                 }

                                 class D : B
                                 {
                                 }

                                 class E
                                     : B
                                 {
                                 }

                                 class F :
                                     B
                                 {
                                 }

                                 class G // note
                                     : B
                                 {
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), fixedData);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixPutsOneSpaceOnEachReportedSide"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6021-001")]
    [Requirement("RH6021-004")]
    [Requirement("RH6021-005")]
    [Requirement("SHARED-GAP-001")]
    public async Task VerifyFixPutsOneSpaceOnEachReportedSideCarriageReturnLineFeed()
    {
        const string testData = """
                                class B
                                {
                                    public B()
                                    {
                                    }
                                }

                                class C{|RH6021::|}B
                                {
                                    public C(){|RH6021::|}base()
                                    {
                                    }
                                }

                                class D→{|RH6021::|} B
                                {
                                }

                                class E
                                    {|RH6021::|}B
                                {
                                }

                                class F{|RH6021::|}
                                    B
                                {
                                }

                                class G // note
                                    {|RH6021::|}B
                                {
                                }
                                """;
        const string fixedData = """
                                 class B
                                 {
                                     public B()
                                     {
                                     }
                                 }

                                 class C : B
                                 {
                                     public C() : base()
                                     {
                                     }
                                 }

                                 class D : B
                                 {
                                 }

                                 class E
                                     : B
                                 {
                                 }

                                 class F :
                                     B
                                 {
                                 }

                                 class G // note
                                     : B
                                 {
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), NormalizeToCarriageReturnLineFeed(fixedData));
    }

    /// <summary>
    /// Verifies that a comment that ends directly in front of the colon is reported and withholds the fix, because the
    /// fix would rewrite the gap the comment is in
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6021-001")]
    [Requirement("RH6021-005")]
    [Requirement("SHARED-GAP-001")]
    public async Task VerifyFixIsNotOfferedWhenCommentIsInRewrittenGap()
    {
        const string testData = """
                                class B
                                {
                                }

                                class C /* note */{|RH6021::|} B
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass{|#0::|}System.IDisposable
                                {
                                    public TestClass()
                                    {
                                    }

                                    public TestClass(int value){|#1::|}this()
                                    {
                                    }

                                    public void Dispose()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass : System.IDisposable
                                 {
                                     public TestClass()
                                     {
                                     }

                                     public TestClass(int value) : this()
                                     {
                                     }

                                     public void Dispose()
                                     {
                                     }
                                 }
                                 """;

        // The base-list colon and the constructor-initializer colon each carry their own unspaced sides in the
        // same document; the fixes only rewrite their own colon's gaps, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6021MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}