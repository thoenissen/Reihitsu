using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH7001FileMayOnlyContainASingleNamespaceAnalyzer"/>
/// </summary>
[TestClass]
public class RH7001FileMayOnlyContainASingleNamespaceAnalyzerTests : AnalyzerTestsBase<RH7001FileMayOnlyContainASingleNamespaceAnalyzer>
{
    #region Tests

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

        await Verify(testData, Diagnostics(RH7001FileMayOnlyContainASingleNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7001MessageFormat));
    }

    #endregion // Tests
}