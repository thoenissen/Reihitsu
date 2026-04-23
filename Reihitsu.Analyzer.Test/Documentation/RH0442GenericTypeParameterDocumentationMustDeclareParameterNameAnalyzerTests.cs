using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0442GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0442GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzerTests : AnalyzerTestsBase<RH0442GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a missing type parameter name attribute.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingTypeParameterName()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a generic type.</summary>
                              /// {|#0:<typeparam>Value.</typeparam>|}
                              internal class Repository<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0442GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH0442MessageFormat));
    }
}