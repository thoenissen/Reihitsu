using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer"/>
/// </summary>
[TestClass]
public class RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzerTests : AnalyzerTestsBase<RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that each additional top-level type in a file-scoped namespace triggers a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-001")]
    [Requirement("RH7002-003")]
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

        await Verify(source, Diagnostics(RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer.DiagnosticId, AnalyzerResources.RH7002MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that each additional top-level type in nested namespaces triggers a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-001")]
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

        await Verify(source, Diagnostics(RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer.DiagnosticId, AnalyzerResources.RH7002MessageFormat));
    }

    /// <summary>
    /// Verifying that nested types do not trigger a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-002")]
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

    /// <summary>
    /// Verifies the specification example: every class, struct, interface, enum, and record after the first
    /// top-level type of a file is reported on its identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-001")]
    [Requirement("RH7002-003")]
    public async Task EveryTopLevelTypeAfterTheFirstIsReportedOnItsName()
    {
        const string source = """
                              namespace N;

                              public class A
                              {
                              }

                              internal record {|RH7002:B|}(string Name);

                              internal struct {|RH7002:S|}
                              {
                              }

                              internal interface {|RH7002:I|}
                              {
                              }

                              internal enum {|RH7002:E|}
                              {
                                  One,
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: a type without <c>partial</c> that follows a <c>partial</c> declaration
    /// is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-001")]
    [Requirement("RH7002-003")]
    public async Task TypeAfterAPartialDeclarationIsReported()
    {
        const string source = """
                              partial class A
                              {
                              }

                              class {|RH7002:B|}
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: the types of all namespaces of one file are counted together, at any
    /// depth of nested namespaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-001")]
    public async Task TypesOfSeveralNamespacesAreCountedTogether()
    {
        const string source = """
                              namespace N1
                              {
                                  class A
                                  {
                                  }
                              }

                              namespace N2
                              {
                                  namespace Inner
                                  {
                                      class {|RH7002:B|}
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: nested types and delegates are not counted as top-level types
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-002")]
    public async Task NestedTypesAndDelegatesAreNotCounted()
    {
        const string source = """
                              delegate void Handler();

                              class C
                              {
                                  private record Nested(string Name);

                                  private enum Kind
                                  {
                                      One,
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example of two parts of one partial type in one file: neither is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-004")]
    public async Task FurtherPartOfAPartialTypeIsNotReported()
    {
        const string source = """
                              partial class C
                              {
                              }

                              partial class C
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example of the same name in another namespace and the same name with a type
    /// parameter: they are other types, so their first parts are reported, and the further part of <c>C&lt;T&gt;</c>
    /// is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-004")]
    [Requirement("RH7002-001")]
    public async Task PartialTypesOfOtherNamespaceOrArityAreReportedOnTheirFirstPart()
    {
        const string source = """
                              namespace N1
                              {
                                  partial class C
                                  {
                                  }
                              }

                              namespace N2
                              {
                                  partial class {|RH7002:C|}
                                  {
                                  }

                                  partial class {|RH7002:C|}<T>
                                  {
                                  }

                                  partial class C<T>
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that a <see langword="partial"/> declaration of a new type that follows another type is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-001")]
    public async Task PartialDeclarationOfANewTypeAfterAnotherTypeIsReported()
    {
        const string source = """
                              class A
                              {
                              }

                              partial class {|RH7002:B|}
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that a further part of a partial type that follows another type is not reported, while that other
    /// type is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-004")]
    [Requirement("RH7002-001")]
    public async Task FurtherPartAfterAnotherTypeIsNotReported()
    {
        const string source = """
                              partial class C
                              {
                              }

                              class {|RH7002:D|}
                              {
                              }

                              partial class C
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that parts of one partial type in nested namespace declarations and in a qualified namespace
    /// declaration of the same name are parts of one type: the further part is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-004")]
    public async Task FurtherPartInNestedAndQualifiedNamespaceOfTheSameNameIsNotReported()
    {
        const string source = """
                              namespace A
                              {
                                  namespace B
                                  {
                                      partial class C
                                      {
                                      }
                                  }
                              }

                              namespace A.B
                              {
                                  partial class C
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that parts of one partial type in two separate declarations of the same namespace are parts of one
    /// type: the further part is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7002-004")]
    public async Task FurtherPartInAnotherDeclarationOfTheSameNamespaceIsNotReported()
    {
        const string source = """
                              namespace N
                              {
                                  partial class C
                                  {
                                  }
                              }

                              namespace N
                              {
                                  partial class C
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}