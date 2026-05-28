using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer, RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that misaligned logical operators are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedLogicalOperatorsAreDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class RH5302
                                {
                                    // Valid: operator on same line
                                    void ValidSameLine()
                                    {
                                        var a = true && false;
                                        var b = true || false;
                                        var c = true && false || true;
                                    }

                                    // Valid: operator aligned with first expression on next line
                                    void ValidMultiLine()
                                    {
                                        var a = true
                                                && false;

                                        var b = true
                                                || false;

                                        var c = true
                                                && false
                                                && true;

                                        var d = true
                                                || false
                                                || true;
                                    }

                                    // Invalid: operator not aligned with first expression
                                    void InvalidMultiLine()
                                    {
                                        var a = true
                                            {|#0:&&|} false;

                                        var b = true
                                                    {|#1:|||} false;

                                        var c = true
                                        {|#2:&&|} false
                                        {|#3:&&|} true;
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class RH5302
                                  {
                                      // Valid: operator on same line
                                      void ValidSameLine()
                                      {
                                          var a = true && false;
                                          var b = true || false;
                                          var c = true && false || true;
                                      }

                                      // Valid: operator aligned with first expression on next line
                                      void ValidMultiLine()
                                      {
                                          var a = true
                                                  && false;

                                          var b = true
                                                  || false;

                                          var c = true
                                                  && false
                                                  && true;

                                          var d = true
                                                  || false
                                                  || true;
                                      }

                                      // Invalid: operator not aligned with first expression
                                      void InvalidMultiLine()
                                      {
                                          var a = true
                                                  && false;

                                          var b = true
                                                  || false;

                                          var c = true
                                                  && false
                                                  && true;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5302MessageFormat, 4));
    }

    #endregion // Tests
}