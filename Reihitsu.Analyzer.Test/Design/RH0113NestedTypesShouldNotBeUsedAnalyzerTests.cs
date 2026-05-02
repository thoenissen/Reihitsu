using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0113NestedTypesShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0113NestedTypesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0113NestedTypesShouldNotBeUsedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifying that nested classes trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedClassesTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private class {|#0:Nested|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0113NestedTypesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0113MessageFormat));
    }

    /// <summary>
    /// Verifying that nested structs trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedStructsTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal struct Container
                              {
                                  private struct {|#0:Nested|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0113NestedTypesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0113MessageFormat));
    }

    /// <summary>
    /// Verifying that nested interfaces trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedInterfacesTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal interface IContainer
                              {
                                  interface {|#0:INested|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0113NestedTypesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0113MessageFormat));
    }

    /// <summary>
    /// Verifying that nested records trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedRecordsTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal record Container(string Name)
                              {
                                  private record {|#0:Nested|}(int Id);
                              }
                              """;

        await Verify(source, Diagnostics(RH0113NestedTypesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0113MessageFormat));
    }

    /// <summary>
    /// Verifying that nested enums trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedEnumsTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private enum {|#0:NestedEnum|}
                                  {
                                      One,
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0113NestedTypesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0113MessageFormat));
    }

    /// <summary>
    /// Verifying that nested delegates trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedDelegatesTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private delegate void {|#0:NestedDelegate|}(int value);
                              }
                              """;

        await Verify(source, Diagnostics(RH0113NestedTypesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0113MessageFormat));
    }

    /// <summary>
    /// Verifying that generic nested types trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyGenericNestedTypesTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal readonly record struct Container<T>(T Value)
                              {
                                  private class {|#0:NestedClass|}<TItem>
                                  {
                                  }

                                  private readonly record struct {|#1:NestedRecord|}<TItem>(TItem Value);
                              }
                              """;

        await Verify(source, Diagnostics(RH0113NestedTypesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0113MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that top-level types do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTopLevelTypesDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class ClassType
                              {
                              }

                              internal struct StructType
                              {
                              }

                              internal interface IInterfaceType
                              {
                              }

                              internal record RecordType(string Name);

                              internal enum EnumType
                              {
                                  One,
                              }

                              internal delegate void DelegateType(int value);
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that static nested types do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStaticNestedTypesDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  public static class Helper
                                  {
                                  }

                                  internal static class AnotherHelper
                                  {
                                  }

                                  private static class PrivateHelper
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Members
}