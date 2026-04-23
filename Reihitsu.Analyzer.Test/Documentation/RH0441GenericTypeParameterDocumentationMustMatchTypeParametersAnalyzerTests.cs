using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0441GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0441GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzerTests : AnalyzerTestsBase<RH0441GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for type parameter documentation in the wrong order.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForTypeParameterDocumentationInWrongOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a pair.</summary>
                              /// <typeparam name="TFirst">First value.</typeparam>
                              /// {|#0:<typeparam name="TMissing">Missing value.</typeparam>|}
                              internal class Pair<TFirst, TSecond>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0441GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer.DiagnosticId, AnalyzerResources.RH0441MessageFormat));
    }
}