using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0109DebugFailMustProvideMessageTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH0109DebugFailMustProvideMessageTextAnalyzerTests : AnalyzerTestsBase<RH0109DebugFailMustProvideMessageTextAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifying that Debug.Fail calls with null or whitespace message text trigger diagnostics
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
                                    internal static void Verify()
                                    {
                                        {|#0:Debug.Fail(null)|};
                                        {|#1:Fail("")|};
                                        {|#2:Debug.Fail("   ", "detail")|};

                                        Debug.Fail("Do not get here.");
                                        Fail("Do not get here.");
                                        Debug.Fail("Do not get here.", "detail");
                                        CustomDebug.Fail(null);
                                    }

                                    private static class CustomDebug
                                    {
                                        public static void Fail(string message)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0109DebugFailMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH0109MessageFormat, 3));
    }

    #endregion // Members
}