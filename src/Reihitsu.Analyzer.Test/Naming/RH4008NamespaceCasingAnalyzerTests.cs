using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4008NamespaceCasingAnalyzer"/>
/// </summary>
[TestClass]
public class RH4008NamespaceCasingAnalyzerTests : AnalyzerTestsBase<RH4008NamespaceCasingAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for lowercase namespace segments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4008-001")]
    [Requirement("SHARED-CASING-005")]
    public async Task VerifyDiagnosticsForLowercaseNamespaceSegments()
    {
        const string testCode = """
                                namespace {|#0:reihitsu|}.Analyzer.{|#1:naming|}.Resources
                                {
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4008NamespaceCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4008MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase namespace segments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4008-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseNamespace()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Naming.Resources
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies mixed namespace segments only report diagnostics for invalid parts
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4008-001")]
    [Requirement("SHARED-CASING-005")]
    public async Task VerifyDiagnosticsForMixedNamespaceSegments()
    {
        const string testCode = """
                                namespace Reihitsu.{|#0:analyzer|}.Naming.{|#1:resources|}
                                {
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4008NamespaceCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4008MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that every segment of a block-scoped namespace that is not PascalCase is reported, and a PascalCase namespace is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4008-001")]
    public async Task EachBlockScopedNamespaceSegmentThatIsNotPascalCaseIsReported()
    {
        const string testCode = """
                                namespace Reihitsu.{|RH4008:analyzer|}.Naming.{|RH4008:resources|}
                                {
                                }

                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a lowercase segment and a segment with a separator are reported on their own identifiers, while the PascalCase segment between them is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-005")]
    [Requirement("RH4008-001")]
    public async Task NamespaceSegmentsAreReportedOnTheirOwnIdentifier()
    {
        const string testCode = """
                                namespace {|RH4008:company|}.Product.{|RH4008:data_access|}
                                {
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}