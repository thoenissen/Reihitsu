using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0439VoidReturnValueMustNotBeDocumentedAnalyzer"/> and
/// <see cref="RH0439VoidReturnValueMustNotBeDocumentedCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0439VoidReturnValueMustNotBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0439VoidReturnValueMustNotBeDocumentedAnalyzer, RH0439VoidReturnValueMustNotBeDocumentedCodeFixProvider>
{
    /// <summary>
    /// Verifies a diagnostic and code fix for a void member with a returns tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForVoidReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<returns>Nothing.</returns>|}
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       /// <summary>Runs the method.</summary>
                                       internal void TestMethod()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH0439VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0439MessageFormat));
    }
}