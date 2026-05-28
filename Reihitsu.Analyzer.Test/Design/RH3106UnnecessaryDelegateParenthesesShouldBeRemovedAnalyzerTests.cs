using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer"/> and <see cref="RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests : AnalyzerTestsBase<RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer, RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that empty anonymous method parentheses trigger a diagnostic and are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}