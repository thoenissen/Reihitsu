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

    /// <summary>
    /// Test data for verifying that an identical collection type referenced inside a constructor argument is not
    /// mistaken for the collection type being constructed
    /// </summary>
    private const string RepeatedCollectionTypeArgumentTestData = """
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
                                                                      private object CreateDictionary() => new Dictionary<NotImplementedStruct, int>(
                                                                          typeof(Dictionary<{|#0:NotImplementedStruct|}, int>).Name.Length,
                                                                          new NotImplementedStructComparer());
                                                                  }
                                                                  """;

    /// <summary>
    /// Test data for verifying that wrapped <see langword="null"/> comparer arguments do not exempt collection
    /// constructions
    /// </summary>
    private const string WrappedNullComparerConstructionTestData = """
                                                                   using System.Collections.Concurrent;
                                                                   using System.Collections.Generic;

                                                                   namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                   internal struct NotImplementedStruct;

                                                                   internal class RH1001
                                                                   {
                                                                       private object CreateDictionaryWithCast() => new Dictionary<{|#0:NotImplementedStruct|}, int>(
                                                                           comparer: (IEqualityComparer<NotImplementedStruct>)null);

                                                                       private object CreateHashSetWithParentheses() => new HashSet<{|#1:NotImplementedStruct|}>(
                                                                           comparer: ((IEqualityComparer<NotImplementedStruct>)null));

                                                                       private object CreateConcurrentDictionaryWithNullForgiving() => new ConcurrentDictionary<{|#2:NotImplementedStruct|}, int>(
                                                                           comparer: ((IEqualityComparer<NotImplementedStruct>)null)!);

                                                                       private object CreateDictionaryWithConversions() => new Dictionary<{|#3:NotImplementedStruct|}, int>(
                                                                           comparer: (IEqualityComparer<NotImplementedStruct>)(object)null);
                                                                   }
                                                                   """;

    /// <summary>
    /// Test data for verifying that <c>default(T)</c> supplies a non-null comparer for a non-nullable value-type
    /// comparer, while a nullable value-type comparer still produces <see langword="null"/>
    /// </summary>
    private const string ValueTypeDefaultComparerConstructionTestData = """
                                                                        using System.Collections.Generic;

                                                                          namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                          internal struct NotImplementedStruct;

                                                                          internal readonly struct NotImplementedStructComparer : IEqualityComparer<NotImplementedStruct>
                                                                          {
                                                                              public bool Equals(NotImplementedStruct x, NotImplementedStruct y) => true;
                                                                              public int GetHashCode(NotImplementedStruct obj) => 0;
                                                                          }

                                                                          internal class RH1001
                                                                          {
                                                                              private object CreateHashSet() => new HashSet<NotImplementedStruct>(
                                                                                  comparer: default(NotImplementedStructComparer));

                                                                             private object CreateDictionaryWithNullableComparer() => new Dictionary<{|#0:NotImplementedStruct|}, int>(
                                                                                 comparer: default(NotImplementedStructComparer?));

                                                                             private object CreateHashSetWithConvertedComparer() => new HashSet<NotImplementedStruct>(
                                                                                 comparer: (IEqualityComparer<NotImplementedStruct>)default(NotImplementedStructComparer));

                                                                             private object CreateDictionaryWithConvertedNullableComparer() => new Dictionary<{|#1:NotImplementedStruct|}, int>(
                                                                                 comparer: (IEqualityComparer<NotImplementedStruct>)default(NotImplementedStructComparer?));
                                                                        }
                                                                        """;

    /// <summary>
    /// Test data for verifying that target-typed collection constructions use their bound constructor's comparer
    /// semantics
    /// </summary>
    private const string TargetTypedComparerConstructionTestData = """
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
                                                                       private bool _condition;
                                                                       private int _value;

                                                                       private object CreateWithCustomComparer()
                                                                       {
                                                                           Dictionary<NotImplementedStruct, int> values = new(new NotImplementedStructComparer());
                                                                           return values;
                                                                       }

                                                                       private object CreateExplicitWithCustomComparer()
                                                                       {
                                                                           Dictionary<NotImplementedStruct, int> values =
                                                                               new Dictionary<NotImplementedStruct, int>(new NotImplementedStructComparer());
                                                                           return values;
                                                                       }

                                                                       private object CreateWithDefaultComparer()
                                                                       {
                                                                           Dictionary<{|#0:NotImplementedStruct|}, int> values = new();
                                                                           return values;
                                                                       }

                                                                       private object CreateMultipleWithCustomComparers()
                                                                       {
                                                                           Dictionary<NotImplementedStruct, int> first = new(new NotImplementedStructComparer()),
                                                                                                                 second = new(new NotImplementedStructComparer());
                                                                           return first;
                                                                       }

                                                                       private object CreateMultipleWithMixedComparers()
                                                                       {
                                                                           Dictionary<{|#1:NotImplementedStruct|}, int> first = new(new NotImplementedStructComparer()),
                                                                                                                        second = new();
                                                                           return first;
                                                                       }

                                                                       private object CreateParenthesizedWithCustomComparer()
                                                                       {
                                                                           Dictionary<NotImplementedStruct, int> values = (new(new NotImplementedStructComparer()));
                                                                           return values;
                                                                       }

                                                                       private object CreateConditionalWithCustomComparers()
                                                                       {
                                                                           Dictionary<NotImplementedStruct, int> values = _condition
                                                                               ? new(new NotImplementedStructComparer())
                                                                               : new(new NotImplementedStructComparer());
                                                                           return values;
                                                                       }

                                                                       private object CreateSwitchWithCustomComparers()
                                                                       {
                                                                           Dictionary<NotImplementedStruct, int> values = _value switch
                                                                           {
                                                                               0 => new(new NotImplementedStructComparer()),
                                                                               _ => new(new NotImplementedStructComparer())
                                                                           };
                                                                           return values;
                                                                       }

                                                                       private object CreateConditionalWithMixedComparers()
                                                                       {
                                                                           Dictionary<{|#2:NotImplementedStruct|}, int> values = _condition
                                                                               ? new(new NotImplementedStructComparer())
                                                                               : new();
                                                                           return values;
                                                                       }
                                                                   }
                                                                   """;

    /// <summary>
    /// Test data for verifying that aliased collection constructions are analyzed at their construction sites
    /// </summary>
    private const string AliasedComparerConstructionTestData = """
                                                               using System.Collections.Generic;
                                                               using NotImplementedStructDictionary = System.Collections.Generic.Dictionary<Reihitsu.Analyzer.Test.Performance.Resources.NotImplementedStruct, int>;
                                                               using NestedNotImplementedStructDictionaries = System.Collections.Generic.List<System.Collections.Generic.Dictionary<Reihitsu.Analyzer.Test.Performance.Resources.NotImplementedStruct, int>>;

                                                               namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                               internal struct NotImplementedStruct;

                                                               internal class NotImplementedStructComparer : IEqualityComparer<NotImplementedStruct>
                                                               {
                                                                   public bool Equals(NotImplementedStruct x, NotImplementedStruct y) => true;
                                                                   public int GetHashCode(NotImplementedStruct obj) => 0;
                                                               }

                                                               internal class RH1001
                                                               {
                                                                   private {|#2:NotImplementedStructDictionary|} _field;

                                                                   private object CreateExplicitWithCustomComparer() => new NotImplementedStructDictionary(new NotImplementedStructComparer());
                                                                   private object CreateExplicitWithDefaultComparer() => new {|#0:NotImplementedStructDictionary|}();

                                                                   private object CreateTargetTypedWithCustomComparer()
                                                                   {
                                                                       NotImplementedStructDictionary values = new(new NotImplementedStructComparer());
                                                                       return values;
                                                                   }

                                                                   private object CreateTargetTypedWithDefaultComparer()
                                                                   {
                                                                       {|#1:NotImplementedStructDictionary|} values = new();
                                                                       return values;
                                                                   }

                                                                   private {|#3:NotImplementedStructDictionary|} ReturnWithCustomComparer() =>
                                                                       new NotImplementedStructDictionary(new NotImplementedStructComparer());

                                                                   private void Accept({|#4:NotImplementedStructDictionary|} values)
                                                                   {
                                                                   }

                                                                   private List<{|#5:NotImplementedStructDictionary|}> GetNestedAliases() => [];
                                                                   private {|#6:NestedNotImplementedStructDictionaries|} GetAliasWithNestedCollection() => [];
                                                               }
                                                               """;

    /// <summary>
    /// Test data for verifying that <c>EqualityComparer&lt;T&gt;.Default</c> is not treated as a custom comparer
    /// </summary>
    private const string FrameworkDefaultComparerConstructionTestData = """
                                                                        using System.Collections.Generic;

                                                                        namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                        internal struct NotImplementedStruct;

                                                                        internal class RH1001
                                                                        {
                                                                            private object CreateDictionary() => new Dictionary<{|#0:NotImplementedStruct|}, int>(
                                                                                comparer: EqualityComparer<NotImplementedStruct>.Default);

                                                                            private object CreateHashSet() => new HashSet<{|#1:NotImplementedStruct|}>(
                                                                                comparer: ((IEqualityComparer<NotImplementedStruct>)EqualityComparer<NotImplementedStruct>.Default)!);
                                                                        }
                                                                        """;

    /// <summary>
    /// Test data for verifying that composite comparer expressions which necessarily produce
    /// <see langword="null"/> do not exempt collection constructions
    /// </summary>
    private const string CompositeNullComparerConstructionTestData = """
                                                                     using System.Collections.Concurrent;
                                                                     using System.Collections.Generic;

                                                                     namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                     internal struct NotImplementedStruct;

                                                                     internal class RH1001
                                                                     {
                                                                         private bool _condition;
                                                                         private int _value;

                                                                         private sealed class ComparerHolder
                                                                         {
                                                                             internal IEqualityComparer<NotImplementedStruct> Comparer { get; set; }
                                                                         }

                                                                         private object CreateWithCoalescingComparer() => new Dictionary<{|#0:NotImplementedStruct|}, int>(
                                                                             comparer: (IEqualityComparer<NotImplementedStruct>)null
                                                                                       ?? default(IEqualityComparer<NotImplementedStruct>));

                                                                         private object CreateWithConditionalComparer() => new HashSet<{|#1:NotImplementedStruct|}>(
                                                                             comparer: _condition
                                                                                           ? null
                                                                                           : default(IEqualityComparer<NotImplementedStruct>));

                                                                         private object CreateWithSwitchComparer() => new ConcurrentDictionary<{|#2:NotImplementedStruct|}, int>(
                                                                             comparer: _value switch
                                                                                       {
                                                                                           _ => default(IEqualityComparer<NotImplementedStruct>)
                                                                                       });

                                                                         private object CreateWithMixedConditionalComparer() => new Dictionary<{|#3:NotImplementedStruct|}, int>(
                                                                             comparer: _condition
                                                                                           ? null
                                                                                           : EqualityComparer<NotImplementedStruct>.Default);

                                                                         private object CreateWithMixedSwitchComparer() => new HashSet<{|#4:NotImplementedStruct|}>(
                                                                             comparer: _value switch
                                                                                       {
                                                                                           0 => null,
                                                                                           _ => EqualityComparer<NotImplementedStruct>.Default
                                                                                       });

                                                                         private object CreateWithNullConditionalAccessComparer() => new ConcurrentDictionary<{|#5:NotImplementedStruct|}, int>(
                                                                             comparer: ((ComparerHolder)null)?.Comparer);
                                                                     }
                                                                     """;

    /// <summary>
    /// Test data for verifying that explicit object creations without parentheses are still analyzed
    /// </summary>
    private const string ObjectInitializerConstructionTestData = """
                                                                 using System.Collections.Generic;

                                                                 namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                 internal struct NotImplementedStruct;

                                                                 internal class RH1001
                                                                 {
                                                                     private object CreateDictionary() => new Dictionary<{|#0:NotImplementedStruct|}, int>
                                                                     {
                                                                     };
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

    /// <summary>
    /// Verifying that an identical collection type referenced inside a constructor argument is not exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRepeatedCollectionTypeInsideConstructorArgumentIsStillFlagged()
    {
        await Verify(RepeatedCollectionTypeArgumentTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that wrapped <see langword="null"/> comparer arguments do not exempt collection constructions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionConstructionsWithWrappedNullComparerAreFlagged()
    {
        await Verify(WrappedNullComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that only a nullable value-type default comparer is null-like
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyValueTypeDefaultComparerNullabilityIsRespected()
    {
        await Verify(ValueTypeDefaultComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that target-typed collection constructions use their bound constructor's comparer semantics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTargetTypedCollectionConstructionsUseBoundComparer()
    {
        await Verify(TargetTypedComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that aliased collection constructions are analyzed at their construction sites
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAliasedCollectionConstructionsUseBoundComparer()
    {
        await Verify(AliasedComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 7));
    }

    /// <summary>
    /// Verifying that <c>EqualityComparer&lt;T&gt;.Default</c> does not exempt collection constructions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFrameworkDefaultComparerDoesNotExemptCollectionConstructions()
    {
        await Verify(FrameworkDefaultComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that composite comparer expressions which necessarily produce <see langword="null"/> do not
    /// exempt collection constructions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCompositeNullComparerDoesNotExemptCollectionConstructions()
    {
        await Verify(CompositeNullComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 6));
    }

    /// <summary>
    /// Verifying that explicit object creations without parentheses are still analyzed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyObjectInitializerConstructionWithoutParenthesesIsFlagged()
    {
        await Verify(ObjectInitializerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 1));
    }

    #endregion // Methods
}