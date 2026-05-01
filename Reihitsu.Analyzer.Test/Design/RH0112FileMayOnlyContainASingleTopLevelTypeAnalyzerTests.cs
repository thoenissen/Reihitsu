using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0112FileMayOnlyContainASingleTopLevelTypeAnalyzer"/>
/// </summary>
[TestClass]
public class RH0112FileMayOnlyContainASingleTopLevelTypeAnalyzerTests : AnalyzerTestsBase<RH0112FileMayOnlyContainASingleTopLevelTypeAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifying that each additional top-level type in a file-scoped namespace triggers a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForAdditionalTopLevelTypesInFileScopedNamespace()
    {
        const string source = """
                              namespace Example;

                              public class First
                              {
                                  private record Nested(string Name);
                              }

                              internal record {|#0:Second|}(string Name);

                              internal struct {|#1:Third|}
                              {
                              }

                              internal interface {|#2:IFourth|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0112FileMayOnlyContainASingleTopLevelTypeAnalyzer.DiagnosticId, AnalyzerResources.RH0112MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that each additional top-level type in nested namespaces triggers a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForAdditionalTopLevelTypesInNestedNamespaces()
    {
        const string source = """
                              namespace Example
                              {
                                  namespace Inner
                                  {
                                      internal enum First
                                      {
                                          One,
                                      }

                                      internal class {|#0:Second|}
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0112FileMayOnlyContainASingleTopLevelTypeAnalyzer.DiagnosticId, AnalyzerResources.RH0112MessageFormat));
    }

    /// <summary>
    /// Verifying that nested types do not trigger a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedTypesAreIgnored()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private record NestedRecord(string Name);

                                  private struct NestedStruct
                                  {
                                  }

                                  private interface INestedInterface
                                  {
                                  }

                                  private enum NestedEnum
                                  {
                                      One,
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Members
}