using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0445InheritdocMustBeUsedWithInheritingClassAnalyzer"/>
/// </summary>
[TestClass]
public class RH0445InheritdocMustBeUsedWithInheritingClassAnalyzerTests : AnalyzerTestsBase<RH0445InheritdocMustBeUsedWithInheritingClassAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for inheritdoc on a standalone class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForStandaloneInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;

                              /// {|#0:<inheritdoc/>|}
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0445InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH0445MessageFormat));
    }

    /// <summary>
    /// Verifies that implicit interface implementations may use inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForImplicitInterfaceImplementation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal interface ITest
                              {
                                  /// <summary>
                                  /// Does work.
                                  /// </summary>
                                  void Execute();
                              }

                              internal class TestClass : ITest
                              {
                                  /// <inheritdoc/>
                                  public void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }
}