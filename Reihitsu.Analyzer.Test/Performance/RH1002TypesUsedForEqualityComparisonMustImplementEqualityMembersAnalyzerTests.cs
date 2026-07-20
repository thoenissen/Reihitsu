using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Performance;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Performance;

/// <summary>
/// Test methods for <see cref="RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer"/>
/// </summary>
[TestClass]
public class RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzerTests : AnalyzerTestsBase<RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer>
{
    #region Constants

    /// <summary>
    /// Test data for verifying that struct types used for equality comparison must implement equality members
    /// </summary>
    private const string TestData = """
                                    using System;
                                    using System.Collections.Frozen;
                                    using System.Collections.Generic;
                                    using System.Collections.Immutable;
                                    using System.Diagnostics.CodeAnalysis;
                                    using System.Linq;

                                    namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                    internal class Class;
                                    internal struct NotImplementedStruct;
                                    internal struct OverrideStruct
                                    {
                                        public override bool Equals([NotNullWhen(true)] object obj) => true;
                                        public override int GetHashCode() => 0;
                                    }
                                    internal struct EquatableStruct : IEquatable<EquatableStruct>
                                    {
                                        public bool Equals(EquatableStruct other) => true;
                                    }

                                    internal class RH1001
                                    {
                                        internal class ClassTest
                                        {
                                            private IEnumerable<Class> _enumerable;

                                            public void Test()
                                            {
                                                _enumerable.Distinct()
                                                           .Union(_enumerable)
                                                           .Intersect(_enumerable)
                                                           .Except(_enumerable);

                                                _enumerable.ToLookup(k => k, v => v);
                                                _enumerable.ToDictionary(k => k, v => v);
                                                _enumerable.GroupBy(k => k);
                                                _enumerable.Join(_enumerable, k => k, k => k, (k1, k2) => k1);
                                                _enumerable.GroupJoin(_enumerable, k => k, k => k, (k1, k2) => k1);

                                                _enumerable.ToFrozenDictionary(k => k, v => v);
                                                _enumerable.ToFrozenSet();

                                                _enumerable.ToImmutableDictionary(k => k, v => v);
                                                _enumerable.ToImmutableHashSet();
                                            }
                                        }

                                        internal class NotImplementedStructTestTest
                                        {
                                            private IEnumerable<NotImplementedStruct> _enumerable;

                                            public void Test()
                                            {
                                                _enumerable.{|#0:Distinct|}()
                                                           .{|#1:Union|}(_enumerable)
                                                           .{|#2:Intersect|}(_enumerable)
                                                           .{|#3:Except|}(_enumerable);

                                                _enumerable.{|#4:ToLookup|}(k => k, v => v);
                                                _enumerable.{|#5:ToDictionary|}(k => k, v => v);
                                                _enumerable.{|#6:GroupBy|}(k => k);
                                                _enumerable.{|#7:Join|}(_enumerable, k => k, k => k, (k1, k2) => k1);
                                                _enumerable.{|#8:GroupJoin|}(_enumerable, k => k, k => k, (k1, k2) => k1);

                                                _enumerable.{|#9:ToFrozenDictionary|}(k => k, v => v);
                                                _enumerable.{|#10:ToFrozenSet|}();

                                                _enumerable.{|#11:ToImmutableDictionary|}(k => k, v => v);
                                                _enumerable.{|#12:ToImmutableHashSet|}();
                                            }
                                        }

                                        internal class OverrideStructTest
                                        {
                                            private IEnumerable<OverrideStruct> _enumerable;

                                            public void Test()
                                            {
                                                _enumerable.Distinct()
                                                           .Union(_enumerable)
                                                           .Intersect(_enumerable)
                                                           .Except(_enumerable);

                                                _enumerable.ToLookup(k => k, v => v);
                                                _enumerable.ToDictionary(k => k, v => v);
                                                _enumerable.GroupBy(k => k);
                                                _enumerable.Join(_enumerable, k => k, k => k, (k1, k2) => k1);
                                                _enumerable.GroupJoin(_enumerable, k => k, k => k, (k1, k2) => k1);

                                                _enumerable.ToFrozenDictionary(k => k, v => v);
                                                _enumerable.ToFrozenSet();

                                                _enumerable.ToImmutableDictionary(k => k, v => v);
                                                _enumerable.ToImmutableHashSet();
                                            }
                                        }

                                        internal class EquatableStruct
                                            {
                                            private IEnumerable<EquatableStruct> _enumerable;

                                            public void Test()
                                            {
                                                _enumerable.Distinct()
                                                           .Union(_enumerable)
                                                           .Intersect(_enumerable)
                                                           .Except(_enumerable);

                                                _enumerable.ToLookup(k => k, v => v);
                                                _enumerable.ToDictionary(k => k, v => v);
                                                _enumerable.GroupBy(k => k);
                                                _enumerable.Join(_enumerable, k => k, k => k, (k1, k2) => k1);
                                                _enumerable.GroupJoin(_enumerable, k => k, k => k, (k1, k2) => k1);

                                                _enumerable.ToFrozenDictionary(k => k, v => v);
                                                _enumerable.ToFrozenSet();

                                                _enumerable.ToImmutableDictionary(k => k, v => v);
                                                _enumerable.ToImmutableHashSet();
                                            }
                                        }
                                    }
                                    """;

    /// <summary>
    /// Test data for verifying that a struct implementing <c>IEquatable&lt;T&gt;</c> transitively, through an
    /// intermediate interface, is not flagged
    /// </summary>
    private const string TransitiveEquatableTestData = """
                                                       using System;
                                                       using System.Collections.Generic;
                                                       using System.Linq;

                                                       namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                       internal interface IEquatableViaInterface : IEquatable<TransitiveEquatableStruct>;

                                                       internal struct TransitiveEquatableStruct : IEquatableViaInterface
                                                       {
                                                           public bool Equals(TransitiveEquatableStruct other) => true;
                                                           public override int GetHashCode() => 0;
                                                       }

                                                       internal class RH1002
                                                       {
                                                           internal class TransitiveEquatableStructTest
                                                           {
                                                               private IEnumerable<TransitiveEquatableStruct> _enumerable;

                                                               public void Test()
                                                               {
                                                                   _enumerable.Distinct();
                                                               }
                                                           }
                                                       }
                                                       """;

    /// <summary>
    /// Test data for verifying that a key-selector overload checks the projected key type instead of the source
    /// element type
    /// </summary>
    private const string SelectorOverloadTestData = """
                                                    using System.Collections.Frozen;
                                                    using System.Collections.Generic;
                                                    using System.Linq;

                                                    namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                    internal struct NotImplementedStruct
                                                    {
                                                        public int Value;
                                                    }

                                                    internal class RH1002
                                                    {
                                                        internal class SelectorOverloadTest
                                                        {
                                                            private IEnumerable<NotImplementedStruct> _enumerable;

                                                            public void Test()
                                                            {
                                                                // The projected key is int, which already implements equality members; the
                                                                // struct element itself must not be checked
                                                                _enumerable.ToFrozenDictionary(x => x.Value);
                                                                _enumerable.ToFrozenDictionary(x => x.Value, x => x);

                                                                // The projected key is the struct itself
                                                                _enumerable.{|#0:ToFrozenDictionary|}(x => x);
                                                                _enumerable.{|#1:ToFrozenDictionary|}(x => x, x => x.Value);
                                                            }
                                                        }
                                                    }
                                                    """;

    /// <summary>
    /// Test data for verifying that the <c>KeyValuePair&lt;TKey,TValue&gt;</c>-sourced overloads check the key
    /// type, never the value type
    /// </summary>
    private const string KeyValuePairSourceTestData = """
                                                      using System.Collections.Frozen;
                                                      using System.Collections.Generic;
                                                      using System.Collections.Immutable;
                                                      using System.Linq;

                                                      namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                      internal struct NotImplementedStruct;

                                                      internal class RH1002
                                                      {
                                                          internal class KeyValuePairSourceTest
                                                          {
                                                              private IEnumerable<KeyValuePair<NotImplementedStruct, int>> _structKeyed;
                                                              private IEnumerable<KeyValuePair<int, NotImplementedStruct>> _structValued;

                                                              public void Test()
                                                              {
                                                                  // The struct is the key: must be flagged
                                                                  _structKeyed.{|#0:ToDictionary|}();
                                                                  _structKeyed.{|#1:ToImmutableDictionary|}();
                                                                  _structKeyed.{|#2:ToFrozenDictionary|}();

                                                                  // The struct is only the value: must not be flagged
                                                                  _structValued.ToDictionary();
                                                                  _structValued.ToImmutableDictionary();
                                                                  _structValued.ToFrozenDictionary();
                                                              }
                                                          }
                                                      }
                                                      """;

    /// <summary>
    /// Test data for verifying that overloads receiving an explicit custom <c>IEqualityComparer&lt;T&gt;</c> are
    /// exempt, since the comparer bypasses the type's own equality members
    /// </summary>
    private const string ComparerOverloadTestData = """
                                                    using System.Collections.Frozen;
                                                    using System.Collections.Generic;
                                                    using System.Linq;

                                                    namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                    internal struct NotImplementedStruct;

                                                    internal class NotImplementedStructComparer : IEqualityComparer<NotImplementedStruct>
                                                    {
                                                        public bool Equals(NotImplementedStruct a, NotImplementedStruct b) => true;
                                                        public int GetHashCode(NotImplementedStruct s) => 0;
                                                    }

                                                    internal class RH1002
                                                    {
                                                        internal class ComparerOverloadTest
                                                        {
                                                            private IEnumerable<NotImplementedStruct> _enumerable;
                                                            private NotImplementedStructComparer _comparer = new NotImplementedStructComparer();

                                                            public void Test()
                                                            {
                                                                _enumerable.Distinct(_comparer);
                                                                _enumerable.ToDictionary(x => x, x => x, _comparer);
                                                                _enumerable.ToFrozenDictionary(x => x, _comparer);
                                                                _enumerable.GroupBy(x => x, _comparer);
                                                            }
                                                        }
                                                    }
                                                    """;

    /// <summary>
    /// Test data for verifying that an explicit <see langword="null"/> comparer argument is treated like an
    /// omitted comparer, and does not exempt the diagnostic
    /// </summary>
    private const string ExplicitNullComparerTestData = """
                                                        using System.Collections.Frozen;
                                                        using System.Collections.Generic;
                                                        using System.Linq;

                                                        namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                        internal struct NotImplementedStruct;

                                                        internal class RH1002
                                                        {
                                                            internal class ExplicitNullComparerTest
                                                            {
                                                                private IEnumerable<NotImplementedStruct> _enumerable;

                                                                public void Test()
                                                                {
                                                                    _enumerable.{|#0:ToFrozenDictionary|}(x => x, null);
                                                                }
                                                            }
                                                        }
                                                        """;

    /// <summary>
    /// Test data for verifying that the <c>*By</c> family of methods, previously absent from the relevant method
    /// names, are now checked
    /// </summary>
    private const string ByFamilyTestData = """
                                            using System.Collections.Generic;
                                            using System.Linq;

                                            namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                            internal struct NotImplementedStruct;

                                            internal class RH1002
                                            {
                                                internal class ByFamilyTest
                                                {
                                                    private IEnumerable<NotImplementedStruct> _enumerable;

                                                    public void Test()
                                                    {
                                                        _enumerable.{|#0:DistinctBy|}(x => x);
                                                        _enumerable.{|#1:UnionBy|}(_enumerable, x => x);
                                                        _enumerable.{|#2:IntersectBy|}(_enumerable, x => x);
                                                        _enumerable.{|#3:ExceptBy|}(_enumerable, x => x);
                                                    }
                                                }
                                            }
                                            """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.ToHashSet</c>, previously absent from the relevant method
    /// names, is now checked
    /// </summary>
    private const string ToHashSetTestData = """
                                             using System.Collections.Generic;
                                             using System.Linq;

                                             namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                             internal struct NotImplementedStruct;

                                             internal class RH1002
                                             {
                                                 internal class ToHashSetTest
                                                 {
                                                     private IEnumerable<NotImplementedStruct> _enumerable;

                                                     public void Test()
                                                     {
                                                         _enumerable.{|#0:ToHashSet|}();
                                                     }
                                                 }
                                             }
                                             """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifying that struct types used for equality comparison in LINQ methods must implement equality members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStructTypesUsedForEqualityComparisonMustImplementEqualityMembers()
    {
        await Verify(TestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 13));
    }

    /// <summary>
    /// Verifying that a struct implementing <c>IEquatable&lt;T&gt;</c> transitively, through an intermediate
    /// interface, is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStructImplementingIEquatableTransitivelyIsNotFlagged()
    {
        await Verify(TransitiveEquatableTestData);
    }

    /// <summary>
    /// Verifying that a key-selector overload checks the projected key type instead of the source element type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyKeySelectorOverloadChecksProjectedKeyTypeNotElementType()
    {
        await Verify(SelectorOverloadTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that the <c>KeyValuePair&lt;TKey,TValue&gt;</c>-sourced overloads check the key type, never the
    /// value type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyKeyValuePairSourceOverloadsCheckKeyTypeNotValueType()
    {
        await Verify(KeyValuePairSourceTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that overloads receiving an explicit custom <c>IEqualityComparer&lt;T&gt;</c> are exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyComparerOverloadsAreExempt()
    {
        await Verify(ComparerOverloadTestData);
    }

    /// <summary>
    /// Verifying that an explicit <see langword="null"/> comparer argument does not exempt the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExplicitNullComparerArgumentDoesNotExempt()
    {
        await Verify(ExplicitNullComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that the <c>*By</c> family of methods are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyByFamilyMethodsAreChecked()
    {
        await Verify(ByFamilyTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.ToHashSet</c> is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyToHashSetIsChecked()
    {
        await Verify(ToHashSetTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    #endregion // Methods
}