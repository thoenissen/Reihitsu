using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4116TupleElementCasingAnalyzer"/>
/// </summary>
[TestClass]
public class RH4116TupleElementCasingAnalyzerTests : AnalyzerTestsBase<RH4116TupleElementCasingAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for tuple type elements that are not PascalCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4116-001")]
    public async Task VerifyDiagnosticsForTupleTypeElementWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int {|#0:firstValue|}, int SecondValue) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4116TupleElementCasingAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies multiple tuple type elements can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4116-001")]
    public async Task VerifyDiagnosticsForMultipleTupleTypeElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int {|#0:firstValue|}, int {|#1:secondValue|}) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4116TupleElementCasingAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase tuple type elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4116-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseTupleTypeElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int FirstValue, int SecondValue) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies unnamed tuple elements do not report diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4116-001")]
    public async Task VerifyNoDiagnosticsForUnnamedTupleTypeElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int, int) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies tuple type element names are reported in fields, type arguments, nested tuple types, return values, parameters, and locals, while unnamed and PascalCase elements are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4116-001")]
    public async Task TupleTypeElementNamesAreCheckedWhereverTheTupleTypeIsWritten()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    (int {|RH4116:count|}, int Total) _pair;

                                    List<(string {|RH4116:name|}, int)> _entries;

                                    (int {|RH4116:first|}, (int {|RH4116:inner|}, int Other) {|RH4116:nested|}) M((int {|RH4116:left|}, int Right) input)
                                    {
                                        (int {|RH4116:x|}, int Y) local = (1, 2);

                                        return default;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a tuple type element named with an underscore or a verbatim identifier is reported, with the verbatim prefix inside the location
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4116-001")]
    public async Task TupleTypeElementNamedWithUnderscoreOrVerbatimIdentifierIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    (int {|RH4116:_|}, int {|RH4116:@value|}, int Total) M()
                                    {
                                        return default;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the element names of a tuple type in the signature of an override of a member declared in a referenced
    /// assembly are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesInTheSignatureOfAnOverrideOfAReferencedAssemblyMemberAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public abstract class MetaBase
                                      {
                                          public abstract (int first, int second) pair();
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public class Worker : MetaBase
                                {
                                    public override (int first, int second) pair() => default;
                                }

                                public class Leaf : Worker
                                {
                                    public override (int first, int second) pair() => default;
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of a tuple type in the signature of an explicit implementation of an interface member
    /// declared in a referenced assembly are not reported, while those of an explicit implementation of an interface declared
    /// in source keep their diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesInTheSignatureOfAnExplicitImplementationOfAReferencedAssemblyMemberAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface IPairs
                                      {
                                          (int first, int second) get();
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public interface ILocal
                                {
                                    (int {|RH4116:low|}, int {|RH4116:high|}) range();
                                }

                                public class Pairs : IPairs, ILocal
                                {
                                    (int first, int second) IPairs.get() => default;

                                    (int {|RH4116:low|}, int {|RH4116:high|}) ILocal.range() => default;
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that tuple element names that a type argument written in this compilation supplies are reported where the type
    /// argument is written, and not in the signature of the member that implements a member declared outside the compilation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesSuppliedByATypeArgumentInThisCompilationAreReportedWhereTheTypeArgumentIsWritten()
    {
        const string testCode = """
                                using System.Collections;
                                using System.Collections.Generic;

                                public class Pairs : IEnumerable<(string {|RH4116:key|}, int {|RH4116:value|})>
                                {
                                    public IEnumerator<(string key, int value)> GetEnumerator()
                                    {
                                        yield break;
                                    }

                                    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the element names of tuple types nested in the signature of a dictated member, as a type argument, as the
    /// element type of an array, inside another tuple type, and in a parameter type, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesNestedInTheSignatureOfADictatedMemberAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface IMeta
                                      {
                                          System.Collections.Generic.List<(int first, int second)> all((int low, int high)[] ranges);

                                          (int count, (int x, int y) point) nested { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using System.Collections.Generic;

                                using Meta;

                                public class Worker : IMeta
                                {
                                    public (int count, (int x, int y) point) nested => default;

                                    public List<(int first, int second)> all((int low, int high)[] ranges) => null;
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    #endregion // Tests
}