using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0432ValueTagMustNotBeUsedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0432ValueTagMustNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0432ValueTagMustNotBeUsedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a value tag on a property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForValueTagOnProperty()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Stores the current value.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Gets the current value.</summary>
                                  /// {|#0:<value>The current value.</value>|}
                                  internal int Value { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH0432ValueTagMustNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0432MessageFormat));
    }
}