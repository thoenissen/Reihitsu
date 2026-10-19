using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2101NestedClassesShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2101NestedClassesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2101NestedClassesShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that nested classes trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2101-001")]
    [Requirement("SHARED-NESTED-002")]
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

        await Verify(source, Diagnostics(RH2101NestedClassesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2101MessageFormat));
    }

    /// <summary>
    /// Verifying that static nested classes do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2101-002")]
    public async Task VerifyStaticNestedClassesDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  public static class Helper
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that top-level classes do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-NESTED-001")]
    public async Task VerifyTopLevelClassesDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that other nested type kinds do not trigger RH2101 diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2101-001")]
    public async Task VerifyOtherNestedTypeKindsDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private struct NestedStruct
                                  {
                                  }

                                  private interface INested
                                  {
                                  }

                                  private record NestedRecord(int Id);

                                  private enum NestedEnum
                                  {
                                      One,
                                  }

                                  private delegate void NestedDelegate();
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that a class declared directly in a class, struct, interface, record, or record struct is reported
    /// at any depth, and that classes declared in a namespace or at file level are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-NESTED-001")]
    [Requirement("RH2101-001")]
    public async Task VerifyClassesNestedInEveryKindOfTypeAtAnyDepthAreReported()
    {
        const string source = """
                              namespace N
                              {
                                  class TopLevel
                                  {
                                      class {|#0:Inner|}
                                      {
                                          class {|#1:Deeper|}
                                          {
                                          }
                                      }
                                  }

                                  interface I
                                  {
                                      class {|#2:InInterface|}
                                      {
                                      }
                                  }

                                  record R
                                  {
                                      class {|#3:InRecord|}
                                      {
                                      }
                                  }

                                  struct S
                                  {
                                      class {|#4:InStruct|}
                                      {
                                      }
                                  }

                                  record struct RS
                                  {
                                      class {|#5:InRecordStruct|}
                                      {
                                      }
                                  }

                                  static class Holder
                                  {
                                      class {|#6:InStaticClass|}
                                      {
                                      }
                                  }
                              }

                              class AtFileLevel
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH2101NestedClassesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2101MessageFormat, 7));
    }

    /// <summary>
    /// Verifying that each part of a nested partial class is reported on its own identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-NESTED-002")]
    [Requirement("RH2101-001")]
    public async Task VerifyEachPartOfANestedPartialClassIsReported()
    {
        const string source = """
                              class Outer
                              {
                                  partial class {|#0:Part|}
                                  {
                                  }

                                  partial class {|#1:Part|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH2101NestedClassesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2101MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a nested class is reported and a nested record class is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2101-001")]
    public async Task VerifyNestedClassIsReportedAndNestedRecordIsNot()
    {
        const string source = """
                              class Outer
                              {
                                  class {|#0:Helper|}
                                  {
                                  }

                                  record Point(int X);

                                  record class Line(int Length);
                              }
                              """;

        await Verify(source, Diagnostics(RH2101NestedClassesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2101MessageFormat));
    }

    /// <summary>
    /// Verifying that a class nested in a static class is reported when it is not static itself
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2101-002")]
    public async Task VerifyNonStaticClassNestedInStaticClassIsReported()
    {
        const string source = """
                              class Outer
                              {
                                  static class Helpers
                                  {
                                      class {|#0:State|}
                                      {
                                      }

                                      static class Inner
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH2101NestedClassesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2101MessageFormat));
    }

    /// <summary>
    /// Verifying that a nested static partial class whose parts all carry the static modifier is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2101-002")]
    public async Task VerifyNestedStaticPartialClassWithStaticOnEveryPartIsNotReported()
    {
        const string source = """
                              class Outer
                              {
                                  static partial class Helpers
                                  {
                                  }

                                  static partial class Helpers
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Characterizes a nested static partial class whose second part omits the static modifier: the class is static,
    /// but only the part without the modifier is reported. The case is an open question about intended behavior, so
    /// this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPartWithoutStaticOfNestedStaticPartialClassIsReported()
    {
        const string source = """
                              class Outer
                              {
                                  static partial class Helpers
                                  {
                                  }

                                  partial class {|#0:Helpers|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH2101NestedClassesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2101MessageFormat));
    }

    #endregion // Tests
}