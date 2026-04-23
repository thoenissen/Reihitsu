using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0440GenericTypeParametersMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0440GenericTypeParametersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0440GenericTypeParametersMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a missing type parameter comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingTypeParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Creates a value.</summary>
                                  internal T Create<{|#0:T|}>()
                                  {
                                      return default;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0440GenericTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0440MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a missing type parameter comment on a partial type.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingTypeParameterDocumentationOnPartialType()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a generic value.</summary>
                              internal partial class TestClass<{|#0:T|}>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0440GenericTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0440MessageFormat));
    }
}