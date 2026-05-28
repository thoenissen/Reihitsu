using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer"/> and <see cref="RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzerTests : AnalyzerTestsBase<RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer, RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that empty attribute constructor parentheses trigger diagnostics and are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}