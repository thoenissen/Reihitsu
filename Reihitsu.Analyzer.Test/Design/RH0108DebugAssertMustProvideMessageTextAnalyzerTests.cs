using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0108DebugAssertMustProvideMessageTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH0108DebugAssertMustProvideMessageTextAnalyzerTests : AnalyzerTestsBase<RH0108DebugAssertMustProvideMessageTextAnalyzer>
{
    /// <summary>
    /// Verifying that Debug.Assert calls without message text trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        const string testData = """
                                using System.Diagnostics;
                                using static System.Diagnostics.Debug;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal static class Sample
                                {
                                    internal static void Verify(object value)
                                    {
                                        {|#0:Debug.Assert(value != null)|};
                                        {|#1:Assert(value != null)|};

                                        Debug.Assert(value != null, "Value must not be null.");
                                        Assert(value != null, "Value must not be null.");
                                        CustomDebug.Assert(value != null);
                                    }

                                    private static class CustomDebug
                                    {
                                        public static void Assert(bool condition)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0108DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH0108MessageFormat, 2));
    }
}