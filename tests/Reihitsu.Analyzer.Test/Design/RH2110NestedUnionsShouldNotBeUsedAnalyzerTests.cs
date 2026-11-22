using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2110NestedUnionsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2110NestedUnionsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2110NestedUnionsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies the example of RH2110-001: unions nested in a union, a class, and an interface are reported on their
    /// names with the rule's message, and a top-level union, a union in a namespace, and a nested struct are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2110-001")]
    public async Task NestedUnionsAreReportedAsSpecified()
    {
        const string testCode = """
                                union Top(int, string)
                                {
                                    union {|#0:Inner|}(long, string);
                                }

                                class Outer
                                {
                                    union {|#1:Choice|}(int, string);

                                    public union {|#2:Result|}(int, string)
                                    {
                                        public int Get() => 1;
                                    }

                                    struct Entry
                                    {
                                    }
                                }

                                interface I
                                {
                                    union {|#3:Shape|}(int, string);
                                }

                                namespace N
                                {
                                    union Free(int, string);
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(unionSupport), Diagnostics(RH2110NestedUnionsShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that a union nested in a struct, a record, a record struct, and a static class is reported, also at a
    /// depth of two types
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2110-001")]
    [Requirement("SHARED-NESTED-001")]
    public async Task UnionNestedInEveryKindOfTypeIsReported()
    {
        const string testCode = """
                                struct S
                                {
                                    union {|RH2110:InStruct|}(int, string);
                                }

                                record R
                                {
                                    union {|RH2110:InRecord|}(int, string);
                                }

                                record struct RS
                                {
                                    union {|RH2110:InRecordStruct|}(int, string);
                                }

                                static class Holder
                                {
                                    union {|RH2110:InStaticClass|}(int, string);
                                }

                                class A
                                {
                                    class B
                                    {
                                        union {|RH2110:Deep|}(int, string);
                                    }
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union in a block namespace and at file level is not reported, and that nested structs and record
    /// structs are not reported by this rule although the compiler emits a union as a struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2110-001")]
    [Requirement("SHARED-NESTED-001")]
    public async Task TopLevelUnionsAndNestedStructsAreNotReported()
    {
        const string testCode = """
                                union AtFileLevel(int, string);

                                namespace N
                                {
                                    union InNamespace(int, string);

                                    class C
                                    {
                                        struct S
                                        {
                                        }

                                        record struct R(int X);
                                    }
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union in a file-scoped namespace is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2110-001")]
    [Requirement("SHARED-NESTED-001")]
    public async Task UnionInFileScopedNamespaceIsNotReported()
    {
        const string testCode = """
                                namespace N;

                                public union U(int, string);
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that each part of a nested partial union is reported on its own name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2110-001")]
    [Requirement("SHARED-NESTED-002")]
    public async Task EachPartOfNestedPartialUnionIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    partial union {|RH2110:P|}(int, string);

                                    partial union {|RH2110:P|}
                                    {
                                        public int M() => 1;
                                    }
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests
}