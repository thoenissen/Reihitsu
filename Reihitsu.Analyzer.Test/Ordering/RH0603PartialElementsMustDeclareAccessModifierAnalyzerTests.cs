using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0603PartialElementsMustDeclareAccessModifierAnalyzer"/> and <see cref="RH0603PartialElementsMustDeclareAccessModifierCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0603PartialElementsMustDeclareAccessModifierAnalyzerTests : AnalyzerTestsBase<RH0603PartialElementsMustDeclareAccessModifierAnalyzer, RH0603PartialElementsMustDeclareAccessModifierCodeFixProvider>
{
    /// <summary>
    /// Verifying partial types without an access modifier are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PartialTypesWithoutAccessModifierAreReportedAndFixed()
    {
        const string testCode = """
                                partial class {|#0:TestClass|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal partial class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0603PartialElementsMustDeclareAccessModifierAnalyzer.DiagnosticId, AnalyzerResources.RH0603MessageFormat));
    }
}