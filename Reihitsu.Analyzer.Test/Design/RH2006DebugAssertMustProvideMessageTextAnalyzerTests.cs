using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2006DebugAssertMustProvideMessageTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH2006DebugAssertMustProvideMessageTextAnalyzerTests : AnalyzerTestsBase<RH2006DebugAssertMustProvideMessageTextAnalyzer>
{
    #region Tests

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

        await Verify(testData, Diagnostics(RH2006DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2006MessageFormat, 2));
    }

    #endregion // Tests
}