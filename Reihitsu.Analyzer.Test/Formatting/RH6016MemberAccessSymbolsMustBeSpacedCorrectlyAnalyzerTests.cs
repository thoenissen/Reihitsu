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
/// Test methods for <see cref="RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer, RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider>
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

    #endregion // Tests
}