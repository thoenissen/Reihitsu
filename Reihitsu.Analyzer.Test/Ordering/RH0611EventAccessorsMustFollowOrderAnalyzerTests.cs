using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0611EventAccessorsMustFollowOrderAnalyzer"/> and <see cref="RH0611EventAccessorsMustFollowOrderCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0611EventAccessorsMustFollowOrderAnalyzerTests : AnalyzerTestsBase<RH0611EventAccessorsMustFollowOrderAnalyzer, RH0611EventAccessorsMustFollowOrderCodeFixProvider>
{
    /// <summary>
    /// Verifying event accessors are reported and fixed when add appears after remove.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task EventAccessorsAreReportedAndFixedWhenAddAppearsAfterRemove()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    public event EventHandler Changed
                                    {
                                        remove
                                        {
                                            _changed -= value;
                                        }

                                        {|#0:add|}
                                        {
                                            _changed += value;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class TestClass
                                 {
                                     private EventHandler _changed;

                                     public event EventHandler Changed
                                     {
                                         add
                                         {
                                             _changed += value;
                                         }
                                         remove
                                         {
                                             _changed -= value;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0611EventAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH0611MessageFormat));
    }
}