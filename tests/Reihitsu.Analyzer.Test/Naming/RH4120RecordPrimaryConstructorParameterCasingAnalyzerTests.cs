using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4120RecordPrimaryConstructorParameterCasingAnalyzer"/> and <see cref="RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests : BatchCodeFixTestsBase<RH4120RecordPrimaryConstructorParameterCasingAnalyzer, RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for camelCase record primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    [Requirement("RH4120-003")]
    public async Task VerifyDiagnosticsForRecordPrimaryConstructorParameterWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record Product(string {|#0:productCode|});
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record Product(string ProductCode);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix renames record primary constructor parameter references
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    [Requirement("RH4120-003")]
    public async Task VerifyCodeFixRenamesRecordPrimaryConstructorParameterReferences()
    {
        const string testCode = """
                                public record R(string {|#0:badName|});

                                public class C
                                {
                                    void M()
                                    {
                                        var r = new R(badName: "x");
                                        var v = r.badName;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public record R(string BadName);

                                 public class C
                                 {
                                     void M()
                                     {
                                         var r = new R(BadName: "x");
                                         var v = r.BadName;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for camelCase record struct primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    [Requirement("RH4120-003")]
    public async Task VerifyDiagnosticsForRecordStructPrimaryConstructorParameterWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record struct ProductId(string {|#0:valueCode|});
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record struct ProductId(string ValueCode);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for PascalCase record primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseRecordPrimaryConstructorParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record Product(string ProductCode);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for method parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-002")]
    public async Task VerifyNoDiagnosticsForMethodParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(string productCode)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the primary constructor parameters of a record, record class, record struct, and readonly record struct are reported, while PascalCase names are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    public async Task PrimaryConstructorParametersOfEveryRecordKindAreChecked()
    {
        const string testCode = """
                                public record R(int {|RH4120:firstName|}, int LastName);

                                public record class C(int {|RH4120:last_name|});

                                public record struct S(int {|RH4120:count|});

                                public readonly record struct T(int {|RH4120:size|});
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a record primary constructor parameter named with an underscore, an explicit record constructor, a record method, and class and struct primary constructors are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-002")]
    public async Task DiscardsAndParametersOfOtherConstructorsAndMembersAreNotChecked()
    {
        const string testCode = """
                                public record R(int _, int Id)
                                {
                                    public R(string text)
                                        : this(0, text.Length)
                                    {
                                    }

                                    public void M(int input)
                                    {
                                    }
                                }

                                public class C(int value);

                                public struct S(int value);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix renames the record parameter, the property it declares, and every reference to them to the derived PascalCase name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-003")]
    public async Task FixRenamesParameterItsPropertyAndEveryReferenceToPascalCase()
    {
        const string testCode = """
                                public record R(int {|RH4120:firstName|})
                                {
                                    public int Twice => firstName * 2;
                                }

                                public class U
                                {
                                    public int M(R r) => r.firstName;
                                }
                                """;

        const string fixedCode = """
                                 public record R(int FirstName)
                                 {
                                     public int Twice => FirstName * 2;
                                 }

                                 public class U
                                 {
                                     public int M(R r) => r.FirstName;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a positional parameter keeps its diagnostic but gets no fix when its new name is the name of a property its record inherits, because the record's own public property would disappear
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-003")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameWouldMatchAnInheritedProperty()
    {
        const string testCode = """
                                public record B(int Count);

                                public record D(int {|RH4120:count|}) : B(count);
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a positional parameter of a base record keeps its diagnostic but gets no fix when a derived record has a positional parameter of the new name, because the derived record's own public property would disappear
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-003")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameWouldGiveADerivedRecordAnInheritedPropertyOfItsParameterName()
    {
        const string testCode = """
                                public record B(int {|RH4120:count|});

                                public record D(int Count) : B(Count);
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that matching positional parameters of a base and a derived record keep their diagnostics but get no fix, because renaming either alone gives the derived record a public property it did not declare before, and Fix All renames neither
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    [Requirement("RH4120-003")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedForEitherOfTwoMatchingPositionalParametersOfABaseAndADerivedRecord()
    {
        const string testCode = """
                                public record B(int {|RH4120:count|});

                                public record D(int {|RH4120:count|}) : B(count);
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a positional parameter keeps its diagnostic but gets no fix when the record declares a property of its name, because after the rename the compiler would declare a second public property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameWouldSplitTheParameterFromItsDeclaredProperty()
    {
        const string testCode = """
                                public record R(int {|RH4120:count|})
                                {
                                    public int count { get; init; } = count;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a positional parameter of a record struct keeps its diagnostic but gets no fix when the record struct declares a property of its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameWouldSplitARecordStructParameterFromItsDeclaredProperty()
    {
        const string testCode = """
                                public record struct R(int {|RH4120:count|})
                                {
                                    public int count { get; init; } = count;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a record parameter named by the string literal of a caller-argument expression is still reported
    /// but not fixed, because after the rename the attribute would name no parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4120-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionLiteralNamesTheOldName()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public record R(bool {|RH4120:flag|}, [CallerArgumentExpression("flag")] string Expression = null);
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a record parameter whose new name is the string literal of a caller-argument expression is still
    /// reported but not fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4120-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionLiteralNamesTheNewName()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public record R(bool {|RH4120:flag|}, [CallerArgumentExpression("Flag")] string Expression = null);
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a record parameter named by a <c>nameof</c> caller-argument expression is renamed together with the
    /// <c>nameof</c> argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4120-003")]
    public async Task FixIsOfferedWhenACallerArgumentExpressionNamesTheParameterWithNameof()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public record R(bool {|RH4120:flag|}, [CallerArgumentExpression(nameof(flag))] string Expression = null);
                                """;

        const string fixedCode = """
                                 using System.Runtime.CompilerServices;

                                 public record R(bool Flag, [CallerArgumentExpression(nameof(Flag))] string Expression = null);
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that Fix All leaves a record parameter named by a caller-argument expression, and renames the parameter
    /// of the other record of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4120-003")]
    public async Task FixAllSkipsTheParameterNamedByACallerArgumentExpression()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public record R(bool {|RH4120:flag|}, [CallerArgumentExpression("flag")] string Expression = null);

                                public record S(int {|RH4120:count|});
                                """;

        const string fixedCode = """
                                 using System.Runtime.CompilerServices;

                                 public record R(bool {|RH4120:flag|}, [CallerArgumentExpression("flag")] string Expression = null);

                                 public record S(int Count);
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostics that keep no fix are marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that a record primary constructor parameter keeps its diagnostic but gets no fix when its new name would hide, inside the
    /// record, a constant of the enclosing class that a reference there binds to today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldHideAConstantOfTheEnclosingClass()
    {
        const string testCode = """
                                public class Outer
                                {
                                    public const string Name = "outer";

                                    public record Item(string {|RH4120:name|})
                                    {
                                        public string Label => Name;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a positional record parameter keeps its diagnostic but gets no fix when the property the compiler declares for it would implement a member of an interface the record implements, because that interface member would return another value at run time
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheParametersPropertyWouldImplementAnInterfaceProperty()
    {
        const string testCode = """
                                public interface INamed
                                {
                                    string Name => "default";
                                }

                                public record Person(string {|RH4120:name|}) : INamed;
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a positional record parameter is renamed when the interface its record implements has no member of the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesTheParameterWhenTheInterfaceHasNoMemberOfTheNewName()
    {
        const string testCode = """
                                public interface INamed
                                {
                                    string Other => "default";
                                }

                                public record Person(string {|RH4120:name|}) : INamed;
                                """;

        const string fixedCode = """
                                 public interface INamed
                                 {
                                     string Other => "default";
                                 }

                                 public record Person(string Name) : INamed;
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a positional record parameter is not reported when the property the compiler declares for it implements an interface property declared in a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterWhosePropertyImplementsAReferencedAssemblyInterfacePropertyIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Person(string name) : INamed;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional parameter of a record class, a record struct, and a readonly record struct is not reported when the property the compiler declares for it implements an interface property declared in a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterOfEveryRecordKindWhosePropertyImplementsAReferencedAssemblyInterfacePropertyIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record class Person(string name) : INamed;

                                public record struct Point(string name) : INamed;

                                public readonly record struct Size(string name) : INamed;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional record parameter is not reported when the property the compiler declares for it overrides an abstract property of a record declared in a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterWhosePropertyOverridesAReferencedAssemblyAbstractPropertyIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public abstract record MetaRecord
                                      {
                                          public abstract string title { get; init; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Item(string title) : MetaRecord;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional record parameter is not reported when the property the compiler declares for it overrides a property of a record declared in source, which in turn overrides an abstract property of a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterWhosePropertyOverridesAReferencedAssemblyPropertyThroughASourceRecordIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public abstract record MetaRecord
                                      {
                                          public abstract string title { get; init; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public abstract record Middle : MetaRecord
                                {
                                    public abstract override string title { get; init; }
                                }

                                public record Leaf(string title) : Middle;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional record parameter is not reported when its record implements a source interface that extends an interface of a referenced assembly, whose property the parameter's property implements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterWhosePropertyImplementsAReferencedAssemblyPropertyThroughASourceInterfaceIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public interface ILocal : INamed
                                {
                                }

                                public record ViaLocal(string name) : ILocal;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional record parameter is not reported when the property the compiler declares for it implements a property of a generic interface declared in a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterWhosePropertyImplementsAReferencedAssemblyGenericInterfacePropertyIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface IGen<T>
                                      {
                                          T name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Gen(string name) : IGen<string>;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional record parameter is not reported when the property the compiler declares for it implements an interface property of a referenced assembly that has a default implementation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterWhosePropertyImplementsAReferencedAssemblyDefaultInterfacePropertyIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name => "default";
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Person(string name) : INamed;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional parameter of a partial record is not reported when another part of the record, in another document, names the referenced-assembly interface whose property the parameter's property implements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterOfAPartialRecordWhoseOtherPartImplementsAReferencedAssemblyInterfaceIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                public partial record Part(string name);
                                """;

        const string otherPart = """
                                 using Meta;

                                 public partial record Part : INamed;
                                 """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPart));
                         test.TestState.AdditionalReferences.Add(reference);
                     });
    }

    /// <summary>
    /// Verifies that a positional record parameter is not reported when the interface or abstract record whose property its property implements or overrides is declared in another project of the solution
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterWhosePropertyImplementsOrOverridesAPropertyOfAnotherProjectIsNotReported()
    {
        const string contractCode = """
                                    public interface INamed
                                    {
                                        string name { get; }
                                    }

                                    public abstract record MetaRecord
                                    {
                                        public abstract string title { get; init; }
                                    }
                                    """;

        const string testCode = """
                                public record Person(string name) : INamed;

                                public record Item(string title) : MetaRecord;
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.AdditionalProjects["Contracts"].Sources.Add(("/Contracts/Contracts.cs", contractCode));
                         test.TestState.AdditionalProjectReferences.Add("Contracts");
                     });
    }

    /// <summary>
    /// Verifies that only the parameter whose property implements a referenced-assembly interface property is exempted, and that the fix and Fix All rename the other parameters of the same record alone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    [Requirement("RH4120-003")]
    public async Task OtherParametersOfARecordWithADictatedParameterAreReportedAndFixed()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Person(string name, string {|RH4120:other|}, int {|RH4120:age|}) : INamed
                                {
                                    public string Describe() => name + other + age;
                                }
                                """;

        const string fixedCode = """
                                 using Meta;

                                 public record Person(string name, string Other, int Age) : INamed
                                 {
                                     public string Describe() => name + Other + Age;
                                 }
                                 """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional record parameter is still reported when the property the compiler declares for it implements a property of an interface declared in source
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterWhosePropertyImplementsASourceInterfacePropertyIsReported()
    {
        const string testCode = """
                                public interface ILocal
                                {
                                    string code { get; }
                                }

                                public record Local(string {|RH4120:code|}) : ILocal;
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that positional record parameters are still reported when their record does not implement the referenced-assembly interface itself, and only a derived record names it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterWhosePropertyImplementsAReferencedAssemblyPropertyOnlyForADerivedRecordIsReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Holder(string {|RH4120:name|});

                                public record Sub(string {|RH4120:other|}) : Holder(other), INamed;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional record parameter is still reported when its record implements the referenced-assembly interface property explicitly, so the property the compiler declares for the parameter implements nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterOfARecordThatImplementsTheReferencedAssemblyPropertyExplicitlyIsReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Explicit(string {|RH4120:name|}) : INamed
                                {
                                    string INamed.name => "explicit";
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional record parameter is not reported when the compiler declares no property for it because the record inherits a property of its name, at one or two levels, that implements an interface property of a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterMatchingAnInheritedDictatedPropertyIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Person(string name) : INamed;

                                public record Derived(string name) : Person(name);

                                public record Leaf(string name) : Derived(name);
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional parameter of a record and of a record struct is not reported when the compiler declares no property for it because the body declares a property of its name that implements an interface property of a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterMatchingADeclaredDictatedPropertyIsNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Annotated(string name) : INamed
                                {
                                    public string name { get; init; } = name;
                                }

                                public record struct AnnotatedPoint(string name) : INamed
                                {
                                    public string name { get; init; } = name;
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a positional parameter matching an inherited property of a referenced-assembly record is still reported when that property overrides and implements nothing outside its own assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4120-001")]
    public async Task ParameterMatchingAnInheritedPropertyOfAReferencedAssemblyThatIsNotDictatedIsReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          string name { get; }
                                      }

                                      public record MetaPerson(string name);

                                      public record MetaVirtual
                                      {
                                          public virtual string title { get; init; }
                                      }

                                      public record MetaNamed(string name) : INamed;
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Inherited(string {|RH4120:name|}) : MetaPerson(name);

                                public record Virtual(string {|RH4120:title|}) : MetaVirtual;

                                public record NamedInMeta(string {|RH4120:name|}) : MetaNamed(name);
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public record Order(int {|#0:orderId|}, string {|#1:customerName|});

                                    public class OrderFactory
                                    {
                                        public string Describe()
                                        {
                                            var order = new Order(orderId: 1, customerName: "Ada");

                                            return order.customerName + order.orderId;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public record Order(int OrderId, string CustomerName);

                                     public class OrderFactory
                                     {
                                         public string Describe()
                                         {
                                             var order = new Order(OrderId: 1, CustomerName: "Ada");

                                             return order.CustomerName + order.OrderId;
                                         }
                                     }
                                 }
                                 """;

        // Both parameters belong to the same primary constructor, so each rename also rewrites the
        // synthesized property and the named argument that reads it
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}