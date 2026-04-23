using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0443GenericTypeParameterDocumentationMustHaveTextAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0443GenericTypeParameterDocumentationMustHaveTextAnalyzerTests : AnalyzerTestsBase<RH0443GenericTypeParameterDocumentationMustHaveTextAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for an empty type parameter tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEmptyTypeParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a generic type.</summary>
                              /// {|#0:<typeparam name="T"></typeparam>|}
                              internal class Repository<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0443GenericTypeParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH0443MessageFormat));
    }
}