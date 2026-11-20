using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4012UnionNameCasingAnalyzer"/> and <see cref="RH4012UnionNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4012UnionNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4012UnionNameCasingAnalyzer, RH4012UnionNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a union whose name is not in PascalCase is reported on its identifier with the rule's message,
    /// and that the fix renames it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4012-001")]
    [Requirement("RH4012-002")]
    public async Task VerifyDiagnosticForUnionWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public union {|#0:pet|}(int, string);
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public union Pet(int, string);
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport), Diagnostics(RH4012UnionNameCasingAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported for unions whose names are in PascalCase, generic or nested, with or
    /// without a body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4012-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseUnions()
    {
        const string testCode = """
                                public union Shape(int, string);

                                public union Result<T>(T, string);

                                public union Holder(int, string)
                                {
                                    private union Nested(int, long);
                                }

                                public class Outer
                                {
                                    private union InnerValue(int, string);
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that unions are reported whatever their type parameters or nesting, located on the identifier
    /// without the type parameter list and the case types, and that a struct is not reported; the fix renames each
    /// union
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4012-001")]
    [Requirement("RH4012-002")]
    public async Task UnionsAreCheckedAndStructsAreNot()
    {
        const string testCode = """
                                public union Shape(int, string);

                                public union {|RH4012:pet|}(int, string);

                                public union {|RH4012:result|}<T>(T, string);

                                public class Outer
                                {
                                    private union {|RH4012:inner_value|}(int, string);
                                }

                                public struct point
                                {
                                }
                                """;

        const string fixedCode = """
                                 public union Shape(int, string);

                                 public union Pet(int, string);

                                 public union Result<T>(T, string);

                                 public class Outer
                                 {
                                     private union InnerValue(int, string);
                                 }

                                 public struct point
                                 {
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that unions are reported whatever their modifiers, and that a union nested in the body of another
    /// union is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4012-001")]
    [Requirement("RH4012-002")]
    public async Task UnionsAreCheckedWhateverTheirModifiersAndNesting()
    {
        const string testCode = """
                                internal readonly union {|RH4012:money_value|}(int, long);

                                file union {|RH4012:local_value|}(int, string);

                                public union Holder(int, string)
                                {
                                    union {|RH4012:nested_value|}(int, long);
                                }

                                public class Outer
                                {
                                    protected internal union {|RH4012:shared_value|}(int, string);

                                    private protected union {|RH4012:derived_value|}(int, string);
                                }

                                public interface IContainer
                                {
                                    union {|RH4012:contained_value|}(int, string);
                                }
                                """;

        const string fixedCode = """
                                 internal readonly union MoneyValue(int, long);

                                 file union LocalValue(int, string);

                                 public union Holder(int, string)
                                 {
                                     union NestedValue(int, long);
                                 }

                                 public class Outer
                                 {
                                     protected internal union SharedValue(int, string);

                                     private protected union DerivedValue(int, string);
                                 }

                                 public interface IContainer
                                 {
                                     union ContainedValue(int, string);
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that the fix renames each union and every reference to it, including its use as a case type of
    /// another union and as a field type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4012-002")]
    public async Task FixRenamesUnionAndEveryReference()
    {
        const string testCode = """
                                public union {|RH4012:pet|}(int, string);

                                public union {|RH4012:shape_kind|}(pet, long);

                                public class C
                                {
                                    private pet _value;
                                }
                                """;

        const string fixedCode = """
                                 public union Pet(int, string);

                                 public union ShapeKind(Pet, long);

                                 public class C
                                 {
                                     private Pet _value;
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union named with the verbatim prefix is reported including the prefix, and renamed to the
    /// PascalCase name without it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4012-001")]
    [Requirement("RH4012-002")]
    public async Task VerbatimUnionNameIsReportedAndRenamed()
    {
        const string testCode = """
                                public union {|RH4012:@pet|}(int, string);

                                public class C
                                {
                                    private @pet _value;
                                }
                                """;

        const string fixedCode = """
                                 public union Pet(int, string);

                                 public class C
                                 {
                                     private Pet _value;
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that both parts of a partial union are reported and renamed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4012-001")]
    [Requirement("RH4012-002")]
    public async Task FixRenamesEveryPartOfAPartialUnion()
    {
        const string testCode = """
                                public partial union {|RH4012:pet|}(int, string);

                                public partial union {|RH4012:pet|}
                                {
                                    public int Count() => 0;
                                }
                                """;

        const string fixedCode = """
                                 public partial union Pet(int, string);

                                 public partial union Pet
                                 {
                                     public int Count() => 0;
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union keeps its diagnostic but gets no fix when its new name is the name of the member the
    /// compiler declares for every union, which would be named like its union
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameIsTheNameOfTheGeneratedValueMember()
    {
        const string testCode = """
                                public union {|RH4012:value|}(int, string);
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a nested union keeps its diagnostic but gets no fix when its new name is the name of a type it
    /// uses as a case type, which would then name the union itself
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldRebindACaseType()
    {
        const string testCode = """
                                public class Cat
                                {
                                }

                                public class Dog
                                {
                                }

                                public class Outer
                                {
                                    private union {|RH4012:cat|}(Cat, Dog);
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public union {|#0:pet|}(int, string);

                                    public union {|#1:shape_kind|}(pet, long);

                                    public class Registry
                                    {
                                        public pet Current { get; set; }

                                        public shape_kind Kind { get; set; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public union Pet(int, string);

                                     public union ShapeKind(Pet, long);

                                     public class Registry
                                     {
                                         public Pet Current { get; set; }

                                         public ShapeKind Kind { get; set; }
                                     }
                                 }
                                 """;

        // The second union names the first as a case type, so the first rename reaches a declaration that the second
        // rename changes as well
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4012UnionNameCasingAnalyzer.DiagnosticId, MessageFormat, 2),
                                  test => test.TestState.AdditionalReferences.Add(UnionSupport.GetReference()));
    }

    #endregion // BatchCodeFixTestsBase
}