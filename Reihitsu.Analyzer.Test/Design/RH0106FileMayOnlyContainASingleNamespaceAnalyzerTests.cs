using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0106FileMayOnlyContainASingleNamespaceAnalyzer"/>
/// </summary>
[TestClass]
public class RH0106FileMayOnlyContainASingleNamespaceAnalyzerTests : AnalyzerTestsBase<RH0106FileMayOnlyContainASingleNamespaceAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifying that a second top-level namespace triggers a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostic()
    {
        const string testData = """
                                namespace First
                                {
                                    namespace Nested
                                    {
                                        internal class NestedType
                                        {
                                        }
                                    }
                                }

                                namespace {|#0:Second|}
                                {
                                    internal class SecondType
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0106FileMayOnlyContainASingleNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH0106MessageFormat));
    }

    #endregion // Members
}