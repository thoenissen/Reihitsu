using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Performance;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Performance;

/// <summary>
/// Test methods for <see cref="RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzerTests"/>
/// </summary>
[TestClass]
public class RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzerTests : AnalyzerTestsBase<RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer>
{
    #region Constants

    /// <summary>
    /// Test data for verifying that struct types used as dictionary/set keys must implement equality members
    /// </summary>
    private const string TestData = """
                                    using System;
                                    using System.Collections.Concurrent;
                                    using System.Collections.Frozen;
                                    using System.Collections.Generic;
                                    using System.Collections.Immutable;
                                    using System.Diagnostics.CodeAnalysis;

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
                                            private Dictionary<Class, string> _dictionary = new Dictionary<Class, string>();
                                            private HashSet<Class> _hashSet = new HashSet<Class>();
                                            private ConcurrentDictionary<Class, string> _concurrentDictionary = new ConcurrentDictionary<Class, string>();
                                            private ImmutableDictionary<Class, string> _immutableDictionary;
                                            private ImmutableHashSet<Class> _immutableHashSet;
                                            private FrozenDictionary<Class, string> _frozenDictionary;
                                            private FrozenSet<Class> _frozenSet;
                                        }

                                        internal class NotImplementedStructTest
                                        {
                                            private Dictionary<{|#0:NotImplementedStruct|}, string> _dictionary = new Dictionary<{|#1:NotImplementedStruct|}, string>();
                                            private HashSet<{|#2:NotImplementedStruct|}> _hashSet = new HashSet<{|#3:NotImplementedStruct|}>();
                                            private ConcurrentDictionary<{|#4:NotImplementedStruct|}, string> _concurrentDictionary = new ConcurrentDictionary<{|#5:NotImplementedStruct|}, string>();
                                            private ImmutableDictionary<{|#6:NotImplementedStruct|}, string> _immutableDictionary;
                                            private ImmutableHashSet<{|#7:NotImplementedStruct|}> _immutableHashSet;
                                            private FrozenDictionary<{|#8:NotImplementedStruct|}, string> _frozenDictionary;
                                            private FrozenSet<{|#9:NotImplementedStruct|}> _frozenSet;
                                        }

                                        internal class OverrideStructTest
                                        {
                                            private Dictionary<OverrideStruct, string> _dictionary = new Dictionary<OverrideStruct, string>();
                                            private HashSet<OverrideStruct> _hashSet = new HashSet<OverrideStruct>();
                                            private ConcurrentDictionary<OverrideStruct, string> _concurrentDictionary = new ConcurrentDictionary<OverrideStruct, string>();
                                            private ImmutableDictionary<OverrideStruct, string> _immutableDictionary;
                                            private ImmutableHashSet<OverrideStruct> _immutableHashSet;
                                            private FrozenDictionary<OverrideStruct, string> _frozenDictionary;
                                            private FrozenSet<OverrideStruct> _frozenSet;
                                        }
                                        internal class EquatableStructTest
                                        {
                                            private Dictionary<EquatableStruct, string> _dictionary = new Dictionary<EquatableStruct, string>();
                                            private HashSet<EquatableStruct> _hashSet = new HashSet<EquatableStruct>();
                                            private ConcurrentDictionary<EquatableStruct, string> _concurrentDictionary = new ConcurrentDictionary<EquatableStruct, string>();
                                            private ImmutableDictionary<EquatableStruct, string> _immutableDictionary;
                                            private ImmutableHashSet<EquatableStruct> _immutableHashSet;
                                            private FrozenDictionary<EquatableStruct, string> _frozenDictionary;
                                            private FrozenSet<EquatableStruct> _frozenSet;
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

                                                       namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                       internal interface IEquatableViaInterface : IEquatable<TransitiveEquatableStruct>;

                                                       internal struct TransitiveEquatableStruct : IEquatableViaInterface
                                                       {
                                                           public bool Equals(TransitiveEquatableStruct other) => true;
                                                           public override int GetHashCode() => 0;
                                                       }

                                                       internal class RH1001
                                                       {
                                                           internal class TransitiveEquatableStructTest
                                                           {
                                                               private Dictionary<TransitiveEquatableStruct, string> _dictionary = new Dictionary<TransitiveEquatableStruct, string>();
                                                           }
                                                       }
                                                       """;

    /// <summary>
    /// Test data for verifying that a struct without equality members used only as a dictionary <em>value</em>
    /// (with a <c>string</c> key) is not flagged, because only key positions are hashed
    /// </summary>
    private const string StructValueTestData = """
                                               using System.Collections.Concurrent;
                                               using System.Collections.Frozen;
                                               using System.Collections.Generic;
                                               using System.Collections.Immutable;

                                               namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                               internal struct NotImplementedStruct;

                                               internal class RH1001
                                               {
                                                   internal class StructValueTest
                                                   {
                                                       private Dictionary<string, NotImplementedStruct> _dictionary = new Dictionary<string, NotImplementedStruct>();
                                                       private ConcurrentDictionary<string, NotImplementedStruct> _concurrentDictionary = new ConcurrentDictionary<string, NotImplementedStruct>();
                                                       private ImmutableDictionary<string, NotImplementedStruct> _immutableDictionary;
                                                       private FrozenDictionary<string, NotImplementedStruct> _frozenDictionary;
                                                   }
                                               }
                                               """;

    /// <summary>
    /// Test data for verifying that collection constructions receiving an explicit custom
    /// <c>IEqualityComparer&lt;T&gt;</c> are exempt, since the comparer bypasses the key type's own equality members
    /// </summary>
    private const string CustomComparerConstructionTestData = """
                                                              using System.Collections.Concurrent;
                                                              using System.Collections.Generic;

                                                              namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                              internal struct NotImplementedStruct;

                                                              internal class NotImplementedStructComparer : IEqualityComparer<NotImplementedStruct>
                                                              {
                                                                  public bool Equals(NotImplementedStruct x, NotImplementedStruct y) => true;
                                                                  public int GetHashCode(NotImplementedStruct obj) => 0;
                                                              }

                                                              internal class RH1001
                                                              {
                                                                  private static readonly IEqualityComparer<NotImplementedStruct> _comparer = new NotImplementedStructComparer();

                                                                  private object CreateDictionary() => new System.Collections.Generic.Dictionary<NotImplementedStruct, string>(_comparer);
                                                                  private object CreateHashSet() => new HashSet<NotImplementedStruct>(comparer: _comparer);
                                                                  private object CreateConcurrentDictionary() => new ConcurrentDictionary<NotImplementedStruct, string>(
                                                                      capacity: 1,
                                                                      concurrencyLevel: 1,
                                                                      comparer: _comparer);
                                                              }
                                                              """;

    /// <summary>
    /// Test data for verifying that a <see langword="null"/> or default comparer argument is treated like an
    /// omitted comparer and does not exempt the diagnostic
    /// </summary>
    private const string NullLikeComparerConstructionTestData = """
                                                                using System.Collections.Concurrent;
                                                                using System.Collections.Generic;

                                                                namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                internal struct NotImplementedStruct;

                                                                internal class RH1001
                                                                {
                                                                    private object CreateDictionary() => new Dictionary<{|#0:NotImplementedStruct|}, string>(comparer: null);
                                                                    private object CreateHashSet() => new HashSet<{|#1:NotImplementedStruct|}>(comparer: default);
                                                                    private object CreateConcurrentDictionary() => new ConcurrentDictionary<{|#2:NotImplementedStruct|}, string>(
                                                                        default(IEqualityComparer<NotImplementedStruct>));
                                                                }
                                                                """;

    /// <summary>
    /// Test data for verifying that a collection used as a nested type argument is still flagged when the outer
    /// object creation happens to receive an equality comparer
    /// </summary>
    private const string NestedCollectionTypeArgumentTestData = """
                                                                using System.Collections.Generic;

                                                                namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                internal struct NotImplementedStruct;

                                                                internal class Wrapper<T>
                                                                {
                                                                    internal Wrapper(IEqualityComparer<NotImplementedStruct> comparer)
                                                                    {
                                                                    }
                                                                }

                                                                internal class RH1001
                                                                {
                                                                    private static readonly IEqualityComparer<NotImplementedStruct> _comparer;

                                                                    private object CreateWrapper() => new Wrapper<Dictionary<{|#0:NotImplementedStruct|}, string>>(_comparer);
                                                                }
                                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifying that struct types used as keys in dictionaries and sets must implement equality members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStructTypesUsedAsKeysMustImplementEqualityMembers()
    {
        await Verify(TestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 10));
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
    /// Verifying that a struct without equality members used only as a dictionary <em>value</em> is not flagged,
    /// because only key positions are hashed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStructTypesUsedAsDictionaryValuesAreNotFlagged()
    {
        await Verify(StructValueTestData);
    }

    /// <summary>
    /// Verifying that collection constructions receiving an explicit custom <c>IEqualityComparer&lt;T&gt;</c>
    /// are exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionConstructionsWithExplicitCustomComparerAreNotFlagged()
    {
        await Verify(CustomComparerConstructionTestData);
    }

    /// <summary>
    /// Verifying that a <see langword="null"/> or default comparer argument does not exempt the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionConstructionsWithNullLikeComparerAreFlagged()
    {
        await Verify(NullLikeComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that an equality comparer on an outer object creation does not exempt a nested collection type
    /// argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedCollectionTypeArgumentIsStillFlagged()
    {
        await Verify(NestedCollectionTypeArgumentTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 1));
    }

    #endregion // Methods
}