using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer"/> and <see cref="RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzerTests : BatchCodeFixTestsBase<RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer, RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that empty attribute constructor parentheses trigger diagnostics and are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3107-001")]
    [Requirement("SHARED-PARENS-001")]
    [Requirement("SHARED-PARENS-002")]
    public async Task VerifyDiagnosticsAndCodeFix()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                [Serializable{|#0:()|}]
                                internal class Sample
                                {
                                    [Obsolete{|#1:()|}]
                                    internal void OldMethod()
                                    {
                                    }

                                    [Obsolete("Still used")]
                                    internal void MessageMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  [Serializable]
                                  internal class Sample
                                  {
                                      [Obsolete]
                                      internal void OldMethod()
                                      {
                                      }

                                      [Obsolete("Still used")]
                                      internal void MessageMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH3107MessageFormat, 2));
    }

    /// <summary>
    /// Verifies empty argument lists of attributes in every attribute list are reported over the parentheses, while
    /// attributes without an argument list or with arguments are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3107-001")]
    [Requirement("SHARED-PARENS-001")]
    public async Task EmptyArgumentListsOfAttributesAreReportedOverTheParentheses()
    {
        const string testCode = """
                                using System;
                                using System.Diagnostics;

                                [Obsolete{|RH3107:()|}]
                                class C
                                {
                                    [Obsolete{|RH3107:()|}, DebuggerStepThrough{|RH3107:()|}]
                                    void M()
                                    {
                                    }

                                    [Obsolete]
                                    void N()
                                    {
                                    }

                                    [Obsolete("x")]
                                    void O()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix removes empty argument lists and keeps a comment in front of the list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    public async Task FixRemovesTheEmptyArgumentListAndKeepsACommentInFrontOfIt()
    {
        const string testCode = """
                                using System;

                                [Obsolete{|RH3107:()|}, Serializable /* a */{|RH3107:()|}]
                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 [Obsolete, Serializable /* a */]
                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a comment inside the empty argument list is reported without a code fix, because the fix would delete
    /// it with the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-003")]
    public async Task CommentInsideTheArgumentListIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                [Obsolete{|RH3107:(/* none */)|}]
                                class C
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies preprocessor directives inside the empty argument list are reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-003")]
    public async Task DirectiveInsideTheArgumentListIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                [Obsolete{|RH3107:(
                                #if DEBUG
                                #endif
                                )|}]
                                class C
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies Fix All leaves an argument list with a comment inside as written and reported, and removes the other
    /// empty argument lists of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-003")]
    public async Task FixAllLeavesAnArgumentListWithACommentInsideReported()
    {
        const string testCode = """
                                using System;

                                [Obsolete{|RH3107:(/* none */)|}]
                                class C
                                {
                                    [Obsolete{|RH3107:()|}]
                                    void M()
                                    {
                                    }

                                    [Obsolete{|RH3107:()|}]
                                    void N()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 [Obsolete{|RH3107:(/* none */)|}]
                                 class C
                                 {
                                     [Obsolete]
                                     void M()
                                     {
                                     }

                                     [Obsolete]
                                     void N()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostics whose fix is withheld stay and are marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies the fix leaves one space between two comments that stood on both sides of the removed argument list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    public async Task FixLeavesOneSpaceBetweenTheCommentsOnBothSidesOfTheArgumentList()
    {
        const string testCode = """
                                using System;

                                [Obsolete /* a */ {|RH3107:( )|} /* b */]
                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 [Obsolete /* a */ /* b */]
                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix leaves no space in front of the <c>]</c> behind the attribute
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    public async Task FixLeavesNoSpaceInFrontOfTheClosingBracket()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    [Obsolete /* a */ {|RH3107:()|}]
                                    void M()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete /* a */]
                                     void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix leaves no space in front of the <c>,</c> behind the attribute
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    public async Task FixLeavesNoSpaceInFrontOfTheComma()
    {
        const string testCode = """
                                using System;

                                [Obsolete {|RH3107:()|}, Serializable]
                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 [Obsolete, Serializable]
                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix leaves no space in front of the <c>]</c> behind the attribute in a document whose line breaks
    /// are CRLF, and keeps those line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    public async Task FixLeavesNoSpaceInFrontOfTheClosingBracketWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         [Obsolete /* a */ ()]
                                                         class C
                                                         {
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             [Obsolete /* a */]
                                                             class C
                                                             {
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies that removing empty attribute parentheses between two comments leaves one space between the comments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixLeavesOneSpaceBetweenTheCommentsAroundTheRemovedParentheses()
    {
        const string testCode = """
                                using System;

                                [Obsolete /* a */ {|RH3107:( )|} /* b */]
                                class C
                                {
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 [Obsolete /* a */ /* b */]
                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                [Serializable{|#0:()|}, Obsolete{|#1:()|}]
                                internal class Sample
                                {
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  [Serializable, Obsolete]
                                  internal class Sample
                                  {
                                  }
                                  """;

        return new FixAllScenario(testData, resultData, Diagnostics(RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH3107MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}