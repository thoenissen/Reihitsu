using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4007FileScopedNamespaceCasingAnalyzer"/>
/// </summary>
[TestClass]
public class RH4007FileScopedNamespaceCasingAnalyzerTests : AnalyzerTestsBase<RH4007FileScopedNamespaceCasingAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for lowercase file-scoped namespace segments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4007-001")]
    [Requirement("SHARED-CASING-005")]
    public async Task VerifyDiagnosticsForLowercaseFileScopedNamespaceSegments()
    {
        const string testCode = """
                                namespace {|#0:reihitsu|}.Analyzer.{|#1:naming|}.Resources;
                                """;

        await Verify(testCode, Diagnostics(RH4007FileScopedNamespaceCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4007MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase file-scoped namespace segments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4007-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseFileScopedNamespace()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Naming.Resources;
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies mixed file-scoped namespace segments only report diagnostics for invalid parts
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4007-001")]
    [Requirement("SHARED-CASING-005")]
    public async Task VerifyDiagnosticsForMixedFileScopedNamespaceSegments()
    {
        const string testCode = """
                                namespace Reihitsu.{|#0:analyzer|}.Naming.{|#1:resources|};
                                """;

        await Verify(testCode, Diagnostics(RH4007FileScopedNamespaceCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4007MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a lowercase segment and a segment with a leading underscore are each reported on their own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4007-001")]
    [Requirement("SHARED-CASING-005")]
    public async Task EachFileScopedNamespaceSegmentThatIsNotPascalCaseIsReported()
    {
        const string testCode = """
                                namespace Company.{|RH4007:product|}.{|RH4007:_Internal|};
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a file-scoped namespace whose segments are all PascalCase is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4007-001")]
    public async Task PascalCaseFileScopedNamespaceIsNotReported()
    {
        const string testCode = """
                                namespace Company.Product.Internal;
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}