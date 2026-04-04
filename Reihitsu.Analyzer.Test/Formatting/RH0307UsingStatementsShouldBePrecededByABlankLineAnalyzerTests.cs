using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0307UsingStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer, RH0307UsingStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying that using statements without a preceding blank line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUsingWithoutBlankLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH0307
                                {
                                    public async void Test()
                                    {
                                        using (var resource = new System.IO.MemoryStream())
                                        {
                                        }
                                        {|#0:using|} (var resource = new System.IO.MemoryStream())
                                        {
                                        }

                                        using (var resource = new System.IO.MemoryStream())
                                        {
                                        }
                                        // Test
                                        using (var resource = new System.IO.MemoryStream())
                                        {
                                        }
                                        /* Test */
                                        using (var resource = new System.IO.MemoryStream())
                                        {
                                        }
                                        
                                        await using (var resource = new System.IO.MemoryStream())
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class RH0307
                                  {
                                      public async void Test()
                                      {
                                          using (var resource = new System.IO.MemoryStream())
                                          {
                                          }

                                          using (var resource = new System.IO.MemoryStream())
                                          {
                                          }

                                          using (var resource = new System.IO.MemoryStream())
                                          {
                                          }
                                          // Test
                                          using (var resource = new System.IO.MemoryStream())
                                          {
                                          }
                                          /* Test */
                                          using (var resource = new System.IO.MemoryStream())
                                          {
                                          }
                                          
                                          await using (var resource = new System.IO.MemoryStream())
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0307MessageFormat));
    }
}