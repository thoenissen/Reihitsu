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

    /// <summary>
    /// Verifies that the element names of tuple types in the return type and the parameter types of a unary, a binary, and a
    /// checked operator that implicitly implement static abstract operators declared in a referenced assembly are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesInTheSignatureOfAnOperatorThatImplementsAReferencedAssemblyOperatorAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface IOps<T>
                                          where T : IOps<T>
                                      {
                                          static abstract (int first, int second) operator +(T left, T right);

                                          static abstract (int first, int second) operator checked +(T left, T right);

                                          static abstract T operator -(T value, (int low, int high) range);

                                          static abstract (int count, int total) operator !(T value);
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public struct P : IOps<P>
                                {
                                    public static (int first, int second) operator +(P left, P right) => default;

                                    public static (int first, int second) operator checked +(P left, P right) => default;

                                    public static P operator -(P value, (int low, int high) range) => default;

                                    public static (int count, int total) operator !(P value) => default;
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of tuple types in the signature of an implicit and of an explicit conversion operator
    /// that implicitly implement static abstract conversion operators declared in a referenced assembly are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesInTheSignatureOfAConversionOperatorThatImplementsAReferencedAssemblyOperatorAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface IConversions<T>
                                          where T : IConversions<T>
                                      {
                                          static abstract implicit operator (int low, int high)(T value);

                                          static abstract explicit operator T((int first, int second) value);
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public struct P : IConversions<P>
                                {
                                    public static implicit operator (int low, int high)(P value) => default;

                                    public static explicit operator P((int first, int second) value) => default;
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of tuple types in the signature of explicit interface implementations of an operator and
    /// of a conversion operator declared in a referenced assembly are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesInTheSignatureOfAnExplicitImplementationOfAReferencedAssemblyOperatorAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface IOps<T>
                                          where T : IOps<T>
                                      {
                                          static abstract (int first, int second) operator +(T left, T right);

                                          static abstract implicit operator (int low, int high)(T value);
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public struct P : IOps<P>
                                {
                                    static (int first, int second) IOps<P>.operator +(P left, P right) => default;

                                    static implicit IOps<P>.operator (int low, int high)(P value) => default;
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of a tuple type in the parameter type of an instance compound assignment operator that
    /// implements an operator declared in a referenced assembly are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesInTheSignatureOfACompoundAssignmentOperatorThatImplementsAReferencedAssemblyOperatorAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface IAccumulator
                                      {
                                          void operator +=((int first, int second) value);
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public class Accumulator : IAccumulator
                                {
                                    public void operator +=((int first, int second) value)
                                    {
                                    }
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of tuple types in the signatures of operators and conversion operators that implement
    /// only operators declared in this compilation, or implement nothing, are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesInTheSignatureOfAnOperatorThatIsNotDictatedAreReported()
    {
        const string testCode = """
                                public interface ILocalOps<T>
                                    where T : ILocalOps<T>
                                {
                                    static abstract (int {|RH4116:first|}, int {|RH4116:second|}) operator +(T left, T right);

                                    static abstract implicit operator (int {|RH4116:low|}, int {|RH4116:high|})(T value);
                                }

                                public struct P : ILocalOps<P>
                                {
                                    public static (int {|RH4116:first|}, int {|RH4116:second|}) operator +(P left, P right) => default;

                                    public static implicit operator (int {|RH4116:low|}, int {|RH4116:high|})(P value) => default;
                                }

                                public struct Q : ILocalOps<Q>
                                {
                                    static (int {|RH4116:first|}, int {|RH4116:second|}) ILocalOps<Q>.operator +(Q left, Q right) => default;

                                    static implicit ILocalOps<Q>.operator (int {|RH4116:low|}, int {|RH4116:high|})(Q value) => default;
                                }

                                public struct R
                                {
                                    public static (int {|RH4116:count|}, int {|RH4116:total|}) operator -(R left, R right) => default;

                                    public static explicit operator R((int {|RH4116:first|}, int {|RH4116:second|}) value) => default;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the element names of the tuple type of a field-like event with several declarators are not reported when a
    /// declarator other than the first implements an event declared in a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesOfAnEventFieldAreNotReportedWhenALaterDeclaratorIsDictated()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface IMeta
                                      {
                                          event System.Action<(int first, int second)> changed;
                                      }
                                  }
                                  """;

        const string testCode = """
                                using System;

                                using Meta;

                                public class Second : IMeta
                                {
                                    public event Action<(int first, int second)> local, changed;
                                }

                                public class Middle : IMeta
                                {
                                    public event Action<(int first, int second)> local, changed, other;
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of the tuple type of a field-like event with several declarators are reported when no
    /// declarator implements an event declared outside the compilation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesOfAnEventFieldAreReportedWhenNoDeclaratorIsDictated()
    {
        const string testCode = """
                                using System;

                                public interface ILocal
                                {
                                    event Action<(int {|RH4116:first|}, int {|RH4116:second|})> changed;
                                }

                                public class Second : ILocal
                                {
                                    public event Action<(int {|RH4116:first|}, int {|RH4116:second|})> local, changed;
                                }

                                public class Plain
                                {
                                    public event Action<(int {|RH4116:low|}, int {|RH4116:high|})> local, other;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the element names of the tuple type of a record's positional parameter are not reported when the
    /// property the compiler declares for it implicitly implements an interface property declared in a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesOfARecordPositionalParameterWhosePropertyImplementsAReferencedAssemblyPropertyAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          (int first, int second) Pair { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record R((int first, int second) Pair) : INamed;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of the tuple type of a record struct's positional parameter are not reported when the
    /// property the compiler declares for it implicitly implements an interface property declared in a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesOfARecordStructPositionalParameterWhosePropertyImplementsAReferencedAssemblyPropertyAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          (int first, int second) Pair { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record struct S((int first, int second) Pair) : INamed;

                                public readonly record struct T((int first, int second) Pair) : INamed;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of the tuple type of a record's positional parameter are not reported when the property
    /// the compiler declares for it overrides an abstract property of a record declared in a referenced assembly, directly or
    /// through a record declared in source that overrides it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesOfARecordPositionalParameterWhosePropertyOverridesAReferencedAssemblyPropertyAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public abstract record Base
                                      {
                                          public abstract (int first, int second) Pair { get; init; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record Direct((int first, int second) Pair) : Base;

                                public abstract record Middle : Base
                                {
                                    public abstract override (int first, int second) Pair { get; init; }
                                }

                                public record Leaf((int first, int second) Pair) : Middle;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of tuple types nested in the type of a record's positional parameter, as a type argument,
    /// as the element type of an array, and inside another tuple type, are not reported when the property the compiler declares
    /// for it implements an interface property declared in a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesNestedInTheTypeOfADictatedRecordPositionalParameterAreNotReported()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INested
                                      {
                                          System.Collections.Generic.List<(int first, int second)> All { get; }

                                          (int low, int high)[] Ranges { get; }

                                          (int count, (int x, int y) point) Nested { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using System.Collections.Generic;

                                using Meta;

                                public record W(List<(int first, int second)> All, (int low, int high)[] Ranges, (int count, (int x, int y) point) Nested) : INested;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that tuple element names that a type argument written in this compilation supplies are reported where the type
    /// argument is written, and not in the type of a record's positional parameter whose property implements a member of the
    /// interface declared in a referenced assembly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesSuppliedByATypeArgumentAreReportedWhereWrittenAndNotInARecordPositionalParameter()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface IHolder<T>
                                      {
                                          T Value { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record H((int first, int second) Value) : IHolder<(int {|RH4116:first|}, int {|RH4116:second|})>;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that, in a record whose base list names an interface declared in a referenced assembly, the element names of a
    /// positional parameter whose property implements no member of it are reported, next to a dictated positional parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesOfARecordPositionalParameterThatImplementsNothingAreReportedNextToADictatedOne()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public interface INamed
                                      {
                                          (int first, int second) Pair { get; }
                                      }
                                  }
                                  """;

        const string testCode = """
                                using Meta;

                                public record R((int first, int second) Pair, (int {|RH4116:low|}, int {|RH4116:high|}) Range) : INamed;

                                public record struct S((int {|RH4116:low|}, int {|RH4116:high|}) Range, (int first, int second) Pair) : INamed;
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that the element names of the tuple type of a positional parameter are reported when the property the compiler
    /// declares for it implements or overrides only members declared in source, or nothing, and that those of a class's primary
    /// constructor parameter, for which the compiler declares no property, are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-014")]
    [Requirement("RH4116-001")]
    public async Task TupleElementNamesOfAPositionalParameterThatIsNotDictatedAreReported()
    {
        const string testCode = """
                                public interface ILocal
                                {
                                    (int {|RH4116:first|}, int {|RH4116:second|}) Pair { get; }
                                }

                                public abstract record LocalBase
                                {
                                    public abstract (int {|RH4116:first|}, int {|RH4116:second|}) Pair { get; init; }
                                }

                                public record ImplementsSource((int {|RH4116:first|}, int {|RH4116:second|}) Pair) : ILocal;

                                public record struct ImplementsSourceStruct((int {|RH4116:first|}, int {|RH4116:second|}) Pair) : ILocal;

                                public record OverridesSource((int {|RH4116:first|}, int {|RH4116:second|}) Pair) : LocalBase;

                                public record Plain((int {|RH4116:low|}, int {|RH4116:high|}) Range);

                                public record struct PlainStruct((int {|RH4116:low|}, int {|RH4116:high|}) Range);

                                public class PrimaryConstructor((int {|RH4116:low|}, int {|RH4116:high|}) range)
                                {
                                    public int Low => range.low;
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}