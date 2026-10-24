using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    [Requirement("SHARED-EQUAL-001")]
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
    [Requirement("RH1001-001")]
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
    [Requirement("RH1001-003")]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyCollectionConstructionsWithExplicitCustomComparerAreNotFlagged()
    {
        await Verify(CustomComparerConstructionTestData);
    }

    /// <summary>
    /// Verifying that a <see langword="null"/> or default comparer argument does not exempt the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    [Requirement("SHARED-EQUAL-002")]
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
    [Requirement("RH1001-002")]
    public async Task VerifyNestedCollectionTypeArgumentIsStillFlagged()
    {
        await Verify(NestedCollectionTypeArgumentTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that an identical collection type referenced inside a constructor argument is not exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-002")]
    [Requirement("RH1001-003")]
    public async Task VerifyRepeatedCollectionTypeInsideConstructorArgumentIsStillFlagged()
    {
        await Verify(RepeatedCollectionTypeArgumentTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that wrapped <see langword="null"/> comparer arguments do not exempt collection constructions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyCollectionConstructionsWithWrappedNullComparerAreFlagged()
    {
        await Verify(WrappedNullComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that only a nullable value-type default comparer is null-like
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyValueTypeDefaultComparerNullabilityIsRespected()
    {
        await Verify(ValueTypeDefaultComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that target-typed collection constructions use their bound constructor's comparer semantics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    public async Task VerifyTargetTypedCollectionConstructionsUseBoundComparer()
    {
        await Verify(TargetTypedComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that aliased collection constructions are analyzed at their construction sites
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    [Requirement("RH1001-004")]
    [Requirement("RH1001-005")]
    public async Task VerifyAliasedCollectionConstructionsUseBoundComparer()
    {
        await Verify(AliasedComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 7));
    }

    /// <summary>
    /// Verifying that <c>EqualityComparer&lt;T&gt;.Default</c> does not exempt collection constructions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
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
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyCompositeNullComparerDoesNotExemptCollectionConstructions()
    {
        await Verify(CompositeNullComparerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 6));
    }

    /// <summary>
    /// Verifying that explicit object creations without parentheses are still analyzed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-002")]
    public async Task VerifyObjectInitializerConstructionWithoutParenthesesIsFlagged()
    {
        await Verify(ObjectInitializerConstructionTestData, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 1));
    }

    /// <summary>
    /// Verifying which types lack equality members: a struct without equality members, a struct overriding only
    /// <see cref="object.Equals(object)"/>, and a constructed generic struct without them are reported, while structs
    /// with both overrides or with <see cref="System.IEquatable{T}"/> and <see cref="object.GetHashCode"/>, record
    /// structs, enums, type parameters, primitives, and classes are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-001")]
    [Requirement("RH1001-001")]
    [Requirement("RH1001-005")]
    public async Task VerifyOnlyStructsWithoutTheirOwnEqualityMembersAreReported()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              struct Plain
                              {
                              }

                              struct EqualsAndHash
                              {
                                  public override bool Equals(object obj) => true;

                                  public override int GetHashCode() => 0;
                              }

                              struct EqualsOnly
                              {
                                  public override bool Equals(object obj) => true;
                              }

                              interface IEquatableThroughInterface : IEquatable<Equatable>
                              {
                              }

                              struct Equatable : IEquatableThroughInterface
                              {
                                  public bool Equals(Equatable other) => true;

                                  public override int GetHashCode() => 0;
                              }

                              struct DirectlyEquatable : IEquatable<DirectlyEquatable>
                              {
                                  public bool Equals(DirectlyEquatable other) => true;

                                  public override int GetHashCode() => 0;
                              }

                              record struct Point(int X);

                              enum Color
                              {
                              }

                              class C<T>
                                  where T : struct
                              {
                                  HashSet<{|#0:Plain|}> a;
                                  HashSet<EqualsAndHash> b;
                                  HashSet<{|#1:EqualsOnly|}> c;
                                  HashSet<Equatable> d;
                                  HashSet<Point> e;
                                  HashSet<Color> f;
                                  HashSet<T> g;
                                  HashSet<int> h;
                                  HashSet<string> i;
                                  HashSet<{|#2:KeyValuePair<int, int>|}> j;
                                  HashSet<DirectlyEquatable> k;
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that every listed hash-based collection type is reported on its key or element type argument, and
    /// that the value type of a dictionary and collections that do not hash are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-001")]
    [Requirement("RH1001-005")]
    public async Task VerifyEveryHashBasedCollectionTypeIsReportedOnItsKey()
    {
        const string source = """
                              using System.Collections.Concurrent;
                              using System.Collections.Frozen;
                              using System.Collections.Generic;
                              using System.Collections.Immutable;

                              struct S
                              {
                              }

                              class C
                              {
                                  Dictionary<{|#0:S|}, int> a;
                                  HashSet<{|#1:S|}> b;
                                  ConcurrentDictionary<{|#2:S|}, int> c;
                                  ImmutableDictionary<{|#3:S|}, int> d;
                                  ImmutableHashSet<{|#4:S|}> e;
                                  FrozenDictionary<{|#5:S|}, int> f;
                                  FrozenSet<{|#6:S|}> g;
                                  Dictionary<int, S> h;
                                  SortedDictionary<S, int> i;
                                  List<S> j;
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 7));
    }

    /// <summary>
    /// Verifying that every place naming the collection type is reported, together with explicit creations, a
    /// target-typed creation passed as an argument, assigned, or returned from an expression body, while a target-typed
    /// creation that initializes a declaration or is the operand of a cast is covered by the named type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-002")]
    [Requirement("RH1001-005")]
    public async Task VerifyEveryUseOfTheCollectionTypeIsReported()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  Dictionary<{|#0:S|}, int> _field;

                                  Dictionary<{|#1:S|}, int> M(Dictionary<{|#2:S|}, int> values, List<HashSet<{|#3:S|}>> sets)
                                  {
                                      var created = new Dictionary<{|#4:S|}, int>();
                                      Dictionary<{|#5:S|}, int> declared = new();
                                      Dictionary<{|#6:S|}, int> both = new Dictionary<{|#7:S|}, int>();
                                      object cast = (Dictionary<{|#8:S|}, int>)(values == null ? new() : new());
                                      declared = {|#9:new|}();
                                      var type = typeof(HashSet<{|#10:S|}>);
                                      Take({|#11:new|}());

                                      return created;
                                  }

                                  void Take(HashSet<{|#12:S|}> set)
                                  {
                                  }

                                  HashSet<{|#13:S|}> Create() => {|#14:new|}();
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 15));
    }

    /// <summary>
    /// Verifying that a target-typed creation initializing a field or a property, or wrapped as the operand of a cast in
    /// parentheses, behind <c>!</c>, or in a <see langword="switch"/> arm, is covered by the declared or cast type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-002")]
    [Requirement("RH1001-005")]
    public async Task VerifyTargetTypedCreationInFieldPropertyAndWrappedCastIsCoveredByNamedType()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  Dictionary<{|#0:S|}, int> _field = new();

                                  HashSet<{|#1:S|}> Property { get; } = new();

                                  object M(int mode)
                                  {
                                      object a = (HashSet<{|#2:S|}>)(new())!;
                                      object b = (HashSet<{|#3:S|}>)(mode switch { 0 => new(), _ => new() });

                                      return a ?? b;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that creations with a custom comparer are not reported, nor declarations and casts every creation of
    /// whose initial value passes one, through conditional branches, <see langword="switch"/> arms, parentheses,
    /// <c>!</c>, and casts, and that a declaration with one creation lacking a custom comparer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyCreationsAndDeclarationsWithCustomComparerAreNotReported()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  IEqualityComparer<S> _comparer;
                                  bool _flag;
                                  int _mode;

                                  void M()
                                  {
                                      var a = new Dictionary<S, int>(_comparer);
                                      Dictionary<S, int> b = new(_comparer);
                                      Dictionary<S, int> c = _flag ? new(_comparer) : new Dictionary<S, int>(_comparer);
                                      Dictionary<S, int> d = _mode switch
                                      {
                                          0 => new(_comparer),
                                          _ => new(_comparer),
                                      };
                                      Dictionary<{|#0:S|}, int> e = _flag ? new(_comparer) : new();
                                      Dictionary<S, int> f = true ? new(_comparer) : new();
                                      Dictionary<S, int> l = new(_comparer), m = new(_comparer);
                                      Dictionary<{|#1:S|}, int> n = new(_comparer), o = new();
                                      Dictionary<S, int> h = (new(_comparer))!;
                                      object i = (Dictionary<S, int>)new Dictionary<S, int>(_comparer);
                                      object j = (Dictionary<{|#2:S|}, int>)new Dictionary<{|#3:S|}, int>();
                                      var k = new Dictionary<{|#4:S|}, int>(EqualityComparer<S>.Default);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 5));
    }

    /// <summary>
    /// Verifying that a property initialized with a creation that passes a custom comparer is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    public async Task VerifyPropertyInitializedWithCustomComparerIsNotReported()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  static IEqualityComparer<S> Comparer;

                                  HashSet<S> First { get; } = new(Comparer);

                                  HashSet<S> Second { get; } = new HashSet<S>(Comparer);

                                  HashSet<{|#0:S|}> Third { get; } = new();
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat));
    }

    /// <summary>
    /// Verifying that alias directives are not reported and each use of an alias whose target is, contains as a type
    /// argument, or has as array element such a collection type is reported on the alias name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-004")]
    [Requirement("RH1001-005")]
    public async Task VerifyUsesOfAliasesAreReportedOnTheAliasName()
    {
        const string source = """
                              using Map = System.Collections.Generic.Dictionary<S, int>;
                              using Maps = System.Collections.Generic.List<System.Collections.Generic.Dictionary<S, int>>;
                              using Table = System.Collections.Generic.Dictionary<S, int>[];
                              using Names = System.Collections.Generic.Dictionary<string, S>;

                              struct S
                              {
                              }

                              class C
                              {
                                  {|#0:Map|} _map;

                                  {|#1:Maps|} _maps;

                                  {|#2:Table|} _table;

                                  Names _names;

                                  object Create() => new {|#3:Map|}();
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 4));
    }

    /// <summary>
    /// Characterizes a struct that implements <see cref="System.IEquatable{T}"/> of itself without overriding <see
    /// cref="object.GetHashCode"/>: it counts as having equality members and is not reported. This test pins today's
    /// behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStructImplementingIEquatableWithoutGetHashCodeIsNotReported()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              struct S : IEquatable<S>
                              {
                                  public int Value;

                                  public bool Equals(S other) => Value == other.Value;
                              }

                              class C
                              {
                                  HashSet<S> _set = new HashSet<S>();
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Characterizes generic structs with their own equality members constructed with a struct that lacks them:
    /// <c>S?</c> and <c>(int, S)</c> are not reported, while <c>S</c> is. This test pins today's behavior
    /// without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyGenericStructsWithOwnEqualityMembersOverStructWithoutThemAreNotReported()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                                  public int Value;
                              }

                              class C
                              {
                                  Dictionary<S?, int> a;
                                  Dictionary<(int, S), int> b;
                                  Dictionary<{|#0:S|}, int> c;
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat));
    }

    /// <summary>
    /// Verifying which initial values exempt the declared or cast type: creations that pass a custom comparer, inside
    /// parentheses, behind <c>!</c>, converted by a cast that is not user-defined, in both conditional branches, or in
    /// every <see langword="switch"/> arm, while a creation without a custom comparer, a cast with a user-defined
    /// conversion, a <c>??</c> expression, a variable, a <see langword="throw"/> next to a creation without one,
    /// <see langword="default"/>, and a collection expression leave the type reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    public async Task VerifyOnlyInitialValuesOfRecognizedCreationFormsExemptTheDeclaredType()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class Wrapper
                              {
                                  public static implicit operator Wrapper(Dictionary<{|#0:S|}, int> map) => new Wrapper();

                                  public static explicit operator Dictionary<{|#1:S|}, int>(Wrapper wrapper) => null;
                              }

                              class C
                              {
                                  IEqualityComparer<S> _comparer;
                                  bool _flag;
                                  int _mode;
                                  Dictionary<{|#2:S|}, int> _other;

                                  void M()
                                  {
                                      var a = new Dictionary<S, int>(_comparer);
                                      Dictionary<S, int> b = new(_comparer);
                                      Dictionary<S, int> c = _flag ? new(_comparer) : new Dictionary<S, int>(_comparer);
                                      Dictionary<S, int> d = _mode switch
                                      {
                                          0 => new(_comparer),
                                          _ => new(_comparer),
                                      };
                                      Dictionary<{|#3:S|}, int> e = _flag ? new(_comparer) : new();
                                      Dictionary<S, int> f = true ? new(_comparer) : new();
                                      Dictionary<S, int> l = new(_comparer), m = new(_comparer);
                                      Dictionary<{|#4:S|}, int> n = new(_comparer), o = new();
                                      Dictionary<S, int> h = (new(_comparer))!;
                                      object i = (Dictionary<S, int>)new Dictionary<S, int>(_comparer);
                                      object j = (Dictionary<{|#5:S|}, int>)new Dictionary<{|#6:S|}, int>();
                                      var k = new Dictionary<{|#7:S|}, int>(EqualityComparer<S>.Default);
                                      Dictionary<S, int> p = (Dictionary<S, int>)new(_comparer);
                                      Dictionary<{|#8:S|}, int> q = (Dictionary<{|#9:S|}, int>)(Wrapper)new Dictionary<S, int>(_comparer);
                                      Dictionary<{|#10:S|}, int> r = _other ?? new(_comparer);
                                      Dictionary<{|#11:S|}, int> t = _other;
                                      Dictionary<{|#12:S|}, int> u = _flag ? new() : throw new InvalidOperationException();
                                      Dictionary<{|#13:S|}, int> w = default;
                                      HashSet<{|#14:S|}> v = [];
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 15));
    }

    /// <summary>
    /// Characterizes a creation whose comparer argument can only produce <see langword="null"/> or throw: it counts as
    /// passing a custom comparer and is not reported, while the creation whose argument can only produce
    /// <see langword="null"/> or <c>EqualityComparer&lt;T&gt;.Default</c> is. This test pins today's behavior; the decided
    /// behavior is stated by SHARED-DIAG-005, under which a <see langword="throw"/> branch produces no value and the other
    /// branch alone decides, and its change belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCreationWithComparerArgumentThatCanOnlyProduceNullOrThrowIsNotReported()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(bool flag)
                                  {
                                      Dictionary<S, int> map = new(comparer: flag ? null : throw new InvalidOperationException());
                                      Dictionary<{|#0:S|}, int> other = new(comparer: flag ? null : EqualityComparer<S>.Default);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat));
    }

    /// <summary>
    /// Characterizes declarations whose initial value picks between creations with a custom comparer and a
    /// <see langword="throw"/>: the declared types of <c>a</c> and <c>b</c> are reported, while <c>c</c>, which picks
    /// only between such creations, is not. This test pins today's behavior; the decided behavior is stated by
    /// SHARED-DIAG-005, under which a <see langword="throw"/> branch or arm produces no value and the other branches or
    /// arms alone decide, and its change belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDeclarationPickingBetweenCustomComparerCreationAndThrowIsReported()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  IEqualityComparer<S> _comparer;
                                  bool _flag;
                                  int _mode;

                                  void M()
                                  {
                                      Dictionary<{|#0:S|}, int> a = _mode switch { 0 => new(_comparer), _ => throw new InvalidOperationException() };
                                      Dictionary<{|#1:S|}, int> b = _flag ? new(_comparer) : throw new InvalidOperationException();
                                      Dictionary<S, int> c = _flag ? new(_comparer) : new(_comparer);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 2));
    }

    /// <summary>
    /// Characterizes a collection expression that passes a custom comparer through <c>with(…)</c>: the declared type of
    /// <c>set</c> is reported, while <c>other</c>, created with <see langword="new"/> and the same comparer, is not.
    /// This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionExpressionPassingCustomComparerThroughWithIsReported()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  IEqualityComparer<S> _comparer;
                                  S _item;

                                  void M()
                                  {
                                      HashSet<{|#0:S|}> set = [with(_comparer), _item];
                                      HashSet<S> other = new(_comparer) { _item };
                                  }
                              }
                              """;

        await Verify(source,
                     test => test.SolutionTransforms.Add(ApplyPreviewLanguageVersionToTestProject),
                     Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat));

        static Solution ApplyPreviewLanguageVersionToTestProject(Solution solution, ProjectId projectId)
        {
            var project = solution.GetProject(projectId);

            if (project?.ParseOptions is CSharpParseOptions parseOptions)
            {
                solution = solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion.Preview));
            }

            return solution;
        }
    }

    /// <summary>
    /// Characterizes a collection expression that creates the collection as an argument: the parameter type and the
    /// target-typed <c>new()</c> of <c>Take(new())</c> are reported, while <c>[_item]</c> is not. This test pins
    /// today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionExpressionArgumentIsNotReportedWhereTargetTypedCreationIs()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  S _item;

                                  void Take(HashSet<{|#0:S|}> set)
                                  {
                                  }

                                  void M()
                                  {
                                      Take([_item]);
                                      Take({|#1:new|}());
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 2));
    }

    /// <summary>
    /// Characterizes a creation whose comparer argument is an <see langword="as"/> expression that can only produce
    /// <see langword="null"/>: the declared type of <c>a</c> is reported, while <c>b</c>, whose argument is the same
    /// expression wrapped in parentheses, counts as passing a custom comparer and is not. This test pins today's behavior;
    /// the decided behavior is stated by SHARED-DIAG-005, which judges an expression in parentheses as the expression
    /// inside them, and its change belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCreationWithWrappedAsExpressionComparerArgumentIsNotReported()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M()
                                  {
                                      Dictionary<{|#0:S|}, int> a = new(null as IEqualityComparer<S>);
                                      Dictionary<S, int> b = new((null as IEqualityComparer<S>));
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat));
    }

    /// <summary>
    /// Verifying that a creation passing a constant with the value <see langword="null"/> as comparer, through a
    /// <see langword="const"/> field or a <see langword="const"/> local, is not exempt, while a creation passing a local
    /// that is not a <see langword="const"/> and is initialized with <see langword="null"/> is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyCreationWithConstantNullComparerIsNotExempt()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  const IEqualityComparer<S> NoComparer = null;

                                  void M()
                                  {
                                      const IEqualityComparer<S> none = null;
                                      IEqualityComparer<S> notConst = null;

                                      var a = new Dictionary<{|#0:S|}, int>(NoComparer);
                                      Dictionary<{|#1:S|}, int> b = new(none);
                                      Dictionary<S, int> c = new(notConst);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 2));
    }

    /// <summary>
    /// Characterizes a creation whose comparer argument is <see langword="default"/> of a type parameter constrained to
    /// the comparer interface: the declared type of <c>map</c>, which passes <c>default(TC)</c>, is reported, while
    /// <c>other</c>, which passes <c>default(TV)</c> of a type parameter constrained to a value type, counts as passing
    /// a custom comparer and is not. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCreationWithDefaultOfTypeParameterNotConstrainedToValueTypeIsReported()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M<TC, TN, TR, TV>(IEnumerable<S> items)
                                      where TC : IEqualityComparer<S>
                                      where TN : notnull, IEqualityComparer<S>
                                      where TR : class, IEqualityComparer<S>
                                      where TV : struct, IEqualityComparer<S>
                                  {
                                      items.Distinct(default(TC));
                                      items.Distinct(default(TN));
                                      items.Distinct(default(TR));
                                      items.Distinct(default(TV));
                                  }

                                  void N<TC, TV>()
                                      where TC : IEqualityComparer<S>
                                      where TV : struct, IEqualityComparer<S>
                                  {
                                      Dictionary<{|#0:S|}, int> map = new(default(TC));
                                      Dictionary<S, int> other = new(default(TV));
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat));
    }

    /// <summary>
    /// Verifying that a variable as initial value inside parentheses or behind the null-forgiving operator <c>!</c>
    /// leaves the declared type reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    public async Task VerifyWrappedVariableInitialValueLeavesDeclaredTypeReported()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  Dictionary<{|#0:S|}, int> _other;

                                  void M()
                                  {
                                      Dictionary<{|#1:S|}, int> a = (_other);
                                      Dictionary<{|#2:S|}, int> b = _other!;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that a conditional expression with a constant condition leaves the declared type reported when its
    /// chosen branch passes no comparer, whichever branch that is, and exempts it when its chosen branch passes a custom
    /// comparer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    public async Task VerifyConstantConditionChoosingCreationWithoutComparerLeavesDeclaredTypeReported()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  IEqualityComparer<S> _comparer;

                                  void M()
                                  {
                                      Dictionary<{|#0:S|}, int> a = true ? new() : new(_comparer);
                                      Dictionary<{|#1:S|}, int> b = false ? new(_comparer) : new();
                                      Dictionary<S, int> c = false ? new() : new(_comparer);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a <see langword="switch"/> expression leaves the declared type reported when one of its arms
    /// passes no comparer or is a variable while another arm passes a custom comparer, and exempts it when every arm
    /// passes a custom comparer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1001-003")]
    public async Task VerifySwitchWithArmWithoutCustomComparerLeavesDeclaredTypeReported()
    {
        const string source = """
                              using System.Collections.Generic;

                              struct S
                              {
                              }

                              class C
                              {
                                  IEqualityComparer<S> _comparer;
                                  int _mode;
                                  Dictionary<{|#0:S|}, int> _other;

                                  void M()
                                  {
                                      Dictionary<{|#1:S|}, int> a = _mode switch { 0 => new(_comparer), _ => new() };
                                      Dictionary<{|#2:S|}, int> b = _mode switch { 0 => _other, _ => new(_comparer) };
                                      Dictionary<S, int> c = _mode switch { 0 => new(_comparer), _ => new(_comparer) };
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1001MessageFormat, 3));
    }

    #endregion // Methods
}