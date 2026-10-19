using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer"/> and <see cref="RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests : BatchCodeFixTestsBase<RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer, RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that empty anonymous method parentheses trigger a diagnostic and are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-002")]
    public async Task VerifyDiagnosticAndCodeFix()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    internal void Verify()
                                    {
                                        Action action = delegate{|#0:()|} { };
                                        Action<int> other = delegate(int value) { };
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Sample
                                  {
                                      internal void Verify()
                                      {
                                          Action action = delegate { };
                                          Action<int> other = delegate(int value) { };
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH3106MessageFormat));
    }

    /// <summary>
    /// Verifies empty parameter lists of plain and <c>async</c> anonymous methods that initialize delegate-typed
    /// variables are reported over the parentheses, while anonymous methods without a parameter list or with
    /// parameters are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("SHARED-EMPTY-001")]
    public async Task EmptyParameterListsOfAnonymousMethodsAreReportedOverTheParentheses()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate{|RH3106:()|} { };
                                        Func<Task> b = async delegate {|RH3106:()|} { await Task.Yield(); };
                                        Action c = delegate { };
                                        Action<int> d = delegate (int x) { };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix removes an empty parameter list that directly follows <c>delegate</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-002")]
    public async Task FixRemovesTheEmptyParameterList()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate{|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a = delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Characterizes that an empty parameter list of an anonymous method passed to overloads with different delegate
    /// types is reported and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfAnonymousMethodPassedToOverloadsRemovesTheParameterList()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a) { }
                                    void Run(Action<int> a) { }
                                    void M() { Run(delegate () { }); }
                                }
                                """;

        const string expectedCode = """
                                    using System;

                                    class C
                                    {
                                        void Run(Action a) { }
                                        void Run(Action<int> a) { }
                                        void M() { Run(delegate  { }); }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that an empty parameter list of an anonymous method assigned to an implicitly typed variable is
    /// reported and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfAnonymousMethodInImplicitlyTypedVariableRemovesTheParameterList()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = delegate () { };
                                        a();
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M()
                                        {
                                            var a = delegate  { };
                                            a();
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix removes a comment inside the empty parameter list together with the list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixRemovesACommentInsideTheParameterList()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate ( /* none */ ) { };
                                    }
                                }
                                """;

        const string expectedCode = """
                                    using System;

                                    class C
                                    {
                                        void M()
                                        {
                                            Action a = delegate  { };
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix keeps the whitespace on both sides of the removed parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixKeepsTheWhitespaceOnBothSidesOfTheParameterList()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate () { };
                                    }
                                }
                                """;

        const string expectedCode = """
                                    using System;

                                    class C
                                    {
                                        void M()
                                        {
                                            Action a = delegate  { };
                                        }
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

                                internal class Sample
                                {
                                    internal void Verify()
                                    {
                                        Action outer = delegate{|#0:()|}
                                        {
                                            Action inner = delegate{|#1:()|} { };
                                        };
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Sample
                                  {
                                      internal void Verify()
                                      {
                                          Action outer = delegate
                                          {
                                              Action inner = delegate { };
                                          };
                                      }
                                  }
                                  """;

        return new FixAllScenario(testData, resultData, Diagnostics(RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH3106MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}