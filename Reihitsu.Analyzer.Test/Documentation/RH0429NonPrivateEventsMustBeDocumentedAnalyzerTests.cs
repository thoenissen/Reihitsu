using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0429NonPrivateEventsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0429NonPrivateEventsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0429NonPrivateEventsMustBeDocumentedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEventWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Raises notifications.</summary>
                              internal class TestClass
                              {
                                  internal event System.Action {|#0:Changed|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0429NonPrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0429MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an event with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForEventWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Notifies when the value changed.</summary>
                                  internal event System.Action Changed;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an explicit event with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForExplicitEventWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class TestClass
                              {
                                  private System.Action _changed;
                                  
                                  /// <summary>Notifies when the value changed.</summary>
                                  internal event System.Action Changed
                                  {
                                      add { _changed += value; }
                                      remove { _changed -= value; }
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Members
}