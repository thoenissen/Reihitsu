using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer, RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider>
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
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = string.Empty;
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
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = string {|#0:.|}Empty;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         _ = string.Empty;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
    }

    /// <summary>
    /// Verifies that wrapped method chains do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMethodChainsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass Foo1()
                                    {
                                        return this;
                                    }

                                    TestClass Foo2()
                                    {
                                        return this;
                                    }

                                    void Method(TestClass value)
                                    {
                                        _ = value.Foo1()
                                                 .Foo2();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that only the flagged same-line trailing space is removed when the dot starts a continuation
    /// line, so the line break on the leading side is not joined onto one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOnlyExtraneousTrailingSpaceIsRemovedWhenLeadingSideIsLineBroken()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass Foo1()
                                    {
                                        return this;
                                    }

                                    void Method(TestClass value)
                                    {
                                        _ = value
                                            {|#0:.|} Foo1();
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     TestClass Foo1()
                                     {
                                         return this;
                                     }

                                     void Method(TestClass value)
                                     {
                                         _ = value
                                             .Foo1();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
    }

    /// <summary>
    /// Verifies that only the flagged same-line side is fixed when the other side of the dot starts a
    /// continuation line, so the line break after the dot is not joined onto one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOnlyExtraneousLeadingSpaceIsRemovedWhenTrailingSideIsLineBroken()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass Foo1()
                                    {
                                        return this;
                                    }

                                    void Method(TestClass value)
                                    {
                                        _ = value {|#0:.|}
                                            Foo1();
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     TestClass Foo1()
                                     {
                                         return this;
                                     }

                                     void Method(TestClass value)
                                     {
                                         _ = value.
                                             Foo1();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
    }

    /// <summary>
    /// Verifies that no fix is offered when neither side of the dot carries an extraneous space, exercising
    /// the defensive branch that guards against an otherwise unreachable no-op edit
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoActionIsOfferedWhenNeitherSideHasExtraSpace()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = string.Empty;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root => root.DescendantTokens().Single(token => token.IsKind(SyntaxKind.DotToken)).GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a preprocessor directive sitting on the line-broken side of the dot survives the fix,
    /// instead of being deleted along with the joined line break (issue #408)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveOnLineBrokenSideSurvivesFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass Foo1()
                                    {
                                        return this;
                                    }

                                    void Method(TestClass value)
                                    {
                                        _ = value
                                #if FEATURE
                                            . Foo1();
                                #endif
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     TestClass Foo1()
                                     {
                                         return this;
                                     }

                                     void Method(TestClass value)
                                     {
                                         _ = value
                                 #if FEATURE
                                             .Foo1();
                                 #endif
                                     }
                                 }
                                 """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData), "FEATURE");

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), fixedSource);
    }

    /// <summary>
    /// Verifies that a comment sitting on the untouched line-broken side of the dot survives the fix,
    /// because the replacement span never reaches across the line break to the leading side
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentOnLineBrokenSideSurvivesFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass Foo1()
                                    {
                                        return this;
                                    }

                                    void Method(TestClass value)
                                    {
                                        _ = value // keep this comment
                                            {|#0:.|} Foo1();
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     TestClass Foo1()
                                     {
                                         return this;
                                     }

                                     void Method(TestClass value)
                                     {
                                         _ = value // keep this comment
                                             .Foo1();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
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
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = string. /* comment */Empty;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root => root.DescendantTokens().Single(token => token.IsKind(SyntaxKind.DotToken)).GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a space in front of a conditional access operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConditionalAccessLeadingSpaceIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(string value)
                                    {
                                        _ = value {|#0:?|}.Length;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(string value)
                                     {
                                         _ = value?.Length;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
    }

    /// <summary>
    /// Verifies that a spaced conditional access reports its question mark and its dot separately, and that
    /// applying both fixes tightens the whole operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySpacedConditionalAccessReportsBothSymbolsAndIsFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(string value)
                                    {
                                        _ = value {|#0:?|}{|#1:.|} Length;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(string value)
                                     {
                                         _ = value?.Length;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a space in front of a conditional element access is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConditionalElementAccessLeadingSpaceIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int[] values)
                                    {
                                        _ = values {|#0:?|}[0];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int[] values)
                                     {
                                         _ = values?[0];
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
    }

    /// <summary>
    /// Verifies that spaces around a pointer member access are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPointerMemberAccessSpacesAreDetectedAndFixed()
    {
        const string testData = """
                                internal unsafe class TestClass
                                {
                                    private struct Data
                                    {
                                        public int Value;
                                    }

                                    private int Method(Data* pointer)
                                    {
                                        return pointer {|#0:->|} Value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal unsafe class TestClass
                                 {
                                     private struct Data
                                     {
                                         public int Value;
                                     }

                                     private int Method(Data* pointer)
                                     {
                                         return pointer->Value;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, AllowUnsafe, Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
    }

    /// <summary>
    /// Verifies that a tight conditional access does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTightConditionalAccessDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(string value, int[] values)
                                    {
                                        _ = value?.Length;
                                        _ = values?[0];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the question mark of a conditional expression does not produce diagnostics, although it
    /// carries the same token kind as the conditional access operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTernaryQuestionMarkDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool flag)
                                    {
                                        _ = flag ? 1 : 2;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the question mark of a nullable type does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNullableTypeQuestionMarkDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int? value = null;

                                        _ = value;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a conditional access chain broken across lines does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineBrokenConditionalAccessDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(string value)
                                    {
                                        _ = value
                                            ?.Trim()
                                            ?.Length;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Enables unsafe code for the verifier so sources using pointer types compile
    /// </summary>
    /// <param name="test">Test</param>
    private static void AllowUnsafe(CSharpCodeFixVerifierTest<RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer, RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider> test)
    {
        test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(string value)
                                    {
                                        _ = value {|#0:?|}{|#1:.|} Length;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(string value)
                                     {
                                         _ = value?.Length;
                                     }
                                 }
                                 """;

        // The conditional-access question mark and its immediately following dot are directly adjacent tokens
        // whose fixes both touch the trivia run around the same operator: the question mark's fix removes the
        // leading space and the dot's fix removes the trailing space, an adjacent-trivia interference the batch
        // fixer must converge in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}