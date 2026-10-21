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
    [Requirement("RH7001-001")]
    [Requirement("RH7001-003")]
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

    /// <summary>
    /// Verifies the specification example: every top-level namespace after the first is reported, also when it
    /// declares the same namespace as the first one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7001-001")]
    public async Task EveryTopLevelNamespaceAfterTheFirstIsReported()
    {
        const string source = """
                              namespace A
                              {
                              }

                              namespace {|RH7001:B|}
                              {
                              }

                              namespace {|RH7001:A|}
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: namespaces nested in the single top-level namespace are not counted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7001-002")]
    public async Task NestedNamespacesAreNotCounted()
    {
        const string source = """
                              namespace A
                              {
                                  namespace B
                                  {
                                  }

                                  namespace C
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: the diagnostic covers the whole qualified name of the namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7001-003")]
    public async Task DiagnosticIsLocatedOnTheWholeQualifiedName()
    {
        const string source = """
                              namespace A.B
                              {
                              }

                              namespace {|RH7001:A.C|}
                              {
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}