using System.Threading.Tasks;

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
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-002")]
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
    [Requirement("SHARED-EMPTY-001")]
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
    [Requirement("SHARED-EMPTY-002")]
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
    /// Characterizes that the fix removes a comment inside the empty argument list together with the list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixRemovesACommentInsideTheArgumentList()
    {
        const string testCode = """
                                using System;

                                [Obsolete(/* none */)]
                                class C
                                {
                                }
                                """;

        const string expectedCode = """
                                    using System;

                                    [Obsolete]
                                    class C
                                    {
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix keeps the whitespace on both sides of the removed argument list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixKeepsTheWhitespaceOnBothSidesOfTheArgumentList()
    {
        const string testCode = """
                                using System;

                                [Obsolete /* a */ ( ) /* b */]
                                class C
                                {
                                }
                                """;

        const string expectedCode = """
                                    using System;

                                    [Obsolete /* a */  /* b */]
                                    class C
                                    {
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
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