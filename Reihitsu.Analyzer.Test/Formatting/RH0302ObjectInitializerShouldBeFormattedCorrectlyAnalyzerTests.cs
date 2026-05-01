using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer, RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifying diagnostics for case 1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase1()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0301
                                    {
                                        public RH0301()
                                        {
                                            var tmp1 = {|#0:new RH0301
                                            {
                                                Test1 = "123"
                                            }|};
                                        }

                                        public string Test1 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH0301
                                      {
                                          public RH0301()
                                          {
                                              var tmp1 = new RH0301
                                                         {
                                                             Test1 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0302MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for case 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase2()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0301
                                    {
                                        public RH0301()
                                        {
                                            var tmp2 = {|#0:new RH0301
                                                       {
                                                Test1 = "123"
                                                       }|};
                                        }

                                        public string Test1 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH0301
                                      {
                                          public RH0301()
                                          {
                                              var tmp2 = new RH0301
                                                         {
                                                             Test1 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0302MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for case 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase3()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0301
                                    {
                                        public RH0301()
                                        {
                                            var tmp3 = {|#0:new RH0301
                                                                        {
                                                                            Test1 = "123"
                                                                        }|};
                                        }

                                        public string Test1 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH0301
                                      {
                                          public RH0301()
                                          {
                                              var tmp3 = new RH0301
                                                         {
                                                             Test1 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0302MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for case 4
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase4()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0301
                                    {
                                        public RH0301()
                                        {
                                            var tmp4 = {|#0:new RH0301
                                                {
                                                Test1 = "123",
                                                   Test2 = "123",
                                                   Test3 = "123",
                                                   Test4 = "123"
                                                }|};
                                        }

                                        public string Test1 { get; set; }
                                        public string Test2 { get; set; }
                                        public string Test3 { get; set; }
                                        public string Test4 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH0301
                                      {
                                          public RH0301()
                                          {
                                              var tmp4 = new RH0301
                                                         {
                                                             Test1 = "123",
                                                             Test2 = "123",
                                                             Test3 = "123",
                                                             Test4 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                          public string Test2 { get; set; }
                                          public string Test3 { get; set; }
                                          public string Test4 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0302MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for case 5
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsCase5()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0301
                                    {
                                        public RH0301()
                                        {
                                            var list = new List<string>();

                                            var tmp4 = {|#0:new RH0301
                                                {
                                                Test1 = "123",
                                                   Test2 = "123",
                                                   Test3 = list.Where(s => s == "123")
                                                            .FirstOrDefault(),
                                                   Test4 = "123"
                                                }|};
                                        }

                                        public string Test1 { get; set; }
                                        public string Test2 { get; set; }
                                        public string Test3 { get; set; }
                                        public string Test4 { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Linq;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH0301
                                      {
                                          public RH0301()
                                          {
                                              var list = new List<string>();

                                              var tmp4 = new RH0301
                                                         {
                                                             Test1 = "123",
                                                             Test2 = "123",
                                                             Test3 = list.Where(s => s == "123")
                                                                         .FirstOrDefault(),
                                                             Test4 = "123"
                                                         };
                                          }

                                          public string Test1 { get; set; }
                                          public string Test2 { get; set; }
                                          public string Test3 { get; set; }
                                          public string Test4 { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0302MessageFormat));
    }

    #endregion // Members
}