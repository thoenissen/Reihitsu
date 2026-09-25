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
    /// Test data for verifying that wrapped <see langword="null"/> comparer arguments are treated like omitted
    /// comparers and do not exempt diagnostics
    /// </summary>
    private const string WrappedNullComparerTestData = """
                                                       using System.Collections.Generic;
                                                       using System.Linq;

                                                       namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                       internal struct NotImplementedStruct;

                                                       internal class RH1002
                                                       {
                                                           private IEnumerable<NotImplementedStruct> _enumerable;

                                                           public void Test()
                                                           {
                                                               _enumerable.{|#0:Distinct|}((IEqualityComparer<NotImplementedStruct>)null);
                                                               _enumerable.{|#1:Distinct|}(((IEqualityComparer<NotImplementedStruct>)null)!);
                                                               _enumerable.{|#2:Distinct|}((IEqualityComparer<NotImplementedStruct>)(object)null);
                                                           }
                                                       }
                                                       """;

    /// <summary>
    /// Test data for verifying that <c>default(T)</c> supplies a non-null comparer for a non-nullable value-type
    /// comparer, while a nullable value-type comparer still produces <see langword="null"/>
    /// </summary>
    private const string ValueTypeDefaultComparerTestData = """
                                                            using System.Collections.Generic;
                                                            using System.Linq;

                                                            namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                            internal struct NotImplementedStruct;

                                                            internal readonly struct NotImplementedStructComparer : IEqualityComparer<NotImplementedStruct>
                                                            {
                                                                public bool Equals(NotImplementedStruct x, NotImplementedStruct y) => true;
                                                                public int GetHashCode(NotImplementedStruct obj) => 0;
                                                            }

                                                            internal class RH1002
                                                            {
                                                                private IEnumerable<NotImplementedStruct> _enumerable;

                                                                public void Test()
                                                                {
                                                                    _enumerable.Distinct(default(NotImplementedStructComparer));
                                                                    _enumerable.{|#0:Distinct|}(default(NotImplementedStructComparer?));
                                                                    _enumerable.Distinct((IEqualityComparer<NotImplementedStruct>)default(NotImplementedStructComparer));
                                                                    _enumerable.{|#1:Distinct|}((IEqualityComparer<NotImplementedStruct>)default(NotImplementedStructComparer?));
                                                                }
                                                            }
                                                            """;

    /// <summary>
    /// Test data for verifying that <c>EqualityComparer&lt;T&gt;.Default</c> is not treated as a custom comparer
    /// </summary>
    private const string FrameworkDefaultComparerTestData = """
                                                            using System.Collections.Generic;
                                                            using System.Linq;

                                                            namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                            internal struct NotImplementedStruct;

                                                            internal class RH1002
                                                            {
                                                                private IEnumerable<NotImplementedStruct> _enumerable;

                                                                public void Test()
                                                                {
                                                                    _enumerable.{|#0:Distinct|}(EqualityComparer<NotImplementedStruct>.Default);
                                                                    _enumerable.{|#1:Distinct|}(((IEqualityComparer<NotImplementedStruct>)EqualityComparer<NotImplementedStruct>.Default)!);
                                                                }
                                                            }
                                                            """;

    /// <summary>
    /// Test data for verifying that composite comparer expressions which necessarily produce
    /// <see langword="null"/> do not exempt diagnostics
    /// </summary>
    private const string CompositeNullComparerTestData = """
                                                         using System.Collections.Generic;
                                                         using System.Linq;

                                                         namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                         internal struct NotImplementedStruct;

                                                         internal class RH1002
                                                         {
                                                             private IEnumerable<NotImplementedStruct> _enumerable;
                                                             private bool _condition;
                                                             private int _value;

                                                             private sealed class ComparerHolder
                                                             {
                                                                 internal IEqualityComparer<NotImplementedStruct> Comparer { get; set; }
                                                             }

                                                             public void Test()
                                                             {
                                                                 _enumerable.{|#0:Distinct|}(
                                                                     (IEqualityComparer<NotImplementedStruct>)null
                                                                     ?? default(IEqualityComparer<NotImplementedStruct>));

                                                                 _enumerable.{|#1:Distinct|}(
                                                                     _condition
                                                                         ? null
                                                                         : default(IEqualityComparer<NotImplementedStruct>));

                                                                 _enumerable.{|#2:Distinct|}(
                                                                     _value switch
                                                                     {
                                                                         _ => default(IEqualityComparer<NotImplementedStruct>)
                                                                     });

                                                                 _enumerable.{|#3:Distinct|}(
                                                                     _condition
                                                                         ? null
                                                                         : EqualityComparer<NotImplementedStruct>.Default);

                                                                 _enumerable.{|#4:Distinct|}(
                                                                     _value switch
                                                                     {
                                                                         0 => null,
                                                                         _ => EqualityComparer<NotImplementedStruct>.Default
                                                                     });

                                                                 _enumerable.{|#5:Distinct|}(
                                                                     ((ComparerHolder)null)?.Comparer);
                                                             }
                                                         }
                                                         """;

    /// <summary>
    /// Test data for verifying that a named <c>keySelector</c> argument in its natural position does not desync
    /// positional matching for a subsequent, unnamed comparer argument
    /// </summary>
    private const string NamedKeySelectorArgumentTestData = """
                                                            using System.Collections.Frozen;
                                                            using System.Collections.Generic;
                                                            using System.Linq;

                                                            namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                            internal struct NotImplementedStruct;

                                                            internal class RH1002
                                                            {
                                                                internal class NamedKeySelectorArgumentTest
                                                                {
                                                                    private IEnumerable<NotImplementedStruct> _enumerable;

                                                                    public void Test()
                                                                    {
                                                                        _enumerable.{|#0:ToFrozenDictionary|}(keySelector: x => x, EqualityComparer<NotImplementedStruct>.Default);
                                                                    }
                                                                }
                                                            }
                                                            """;

    /// <summary>
    /// Test data for verifying that a <see langword="default"/> or <c>default(T)</c> comparer argument is treated
    /// like an omitted comparer, and does not exempt the diagnostic
    /// </summary>
    private const string DefaultComparerArgumentTestData = """
                                                           using System.Collections.Frozen;
                                                           using System.Collections.Generic;
                                                           using System.Linq;

                                                           namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                           internal struct NotImplementedStruct;

                                                           internal class RH1002
                                                           {
                                                               internal class DefaultComparerArgumentTest
                                                               {
                                                                   private IEnumerable<NotImplementedStruct> _enumerable;

                                                                   public void Test()
                                                                   {
                                                                       _enumerable.{|#0:ToFrozenDictionary|}(x => x, default);
                                                                       _enumerable.{|#1:ToFrozenDictionary|}(x => x, default(IEqualityComparer<NotImplementedStruct>));
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

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> is checked
    /// </summary>
    private const string ContainsTestData = """
                                            using System;
                                            using System.Collections.Concurrent;
                                            using System.Collections.Generic;
                                            using System.Collections.Immutable;
                                            using System.Collections.ObjectModel;
                                            using System.Linq;

                                            namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                            internal struct NotImplementedStruct;

                                            internal class RH1002
                                            {
                                                internal class ContainsTest
                                                {
                                                    private IEnumerable<NotImplementedStruct> _enumerable;
                                                    private NotImplementedStruct _value;

                                                    public void Test()
                                                    {
                                                        _enumerable.{|#0:Contains|}(_value);
                                                    }
                                                }
                                            }
                                            """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.SequenceEqual</c> is checked
    /// </summary>
    private const string SequenceEqualTestData = """
                                                 using System;
                                                 using System.Collections.Concurrent;
                                                 using System.Collections.Generic;
                                                 using System.Collections.Immutable;
                                                 using System.Collections.ObjectModel;
                                                 using System.Linq;

                                                 namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                 internal struct NotImplementedStruct;

                                                 internal class RH1002
                                                 {
                                                     internal class SequenceEqualTest
                                                     {
                                                         private IEnumerable<NotImplementedStruct> _enumerable;

                                                         public void Test()
                                                         {
                                                             _enumerable.{|#0:SequenceEqual|}(_enumerable);
                                                         }
                                                     }
                                                 }
                                                 """;

    /// <summary>
    /// Test data for verifying that the static invocation forms of <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> are checked
    /// </summary>
    private const string StaticContainsAndSequenceEqualTestData = """
                                                                  using System;
                                                                  using System.Collections.Concurrent;
                                                                  using System.Collections.Generic;
                                                                  using System.Collections.Immutable;
                                                                  using System.Collections.ObjectModel;
                                                                  using System.Linq;

                                                                  namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                  internal struct NotImplementedStruct;

                                                                  internal class RH1002
                                                                  {
                                                                      internal class StaticInvocationTest
                                                                      {
                                                                          private IEnumerable<NotImplementedStruct> _enumerable;
                                                                          private NotImplementedStruct _value;

                                                                          public void Test()
                                                                          {
                                                                              Enumerable.{|#0:Contains|}(_enumerable, _value);
                                                                              Enumerable.{|#1:Contains<NotImplementedStruct>|}(_enumerable, _value);
                                                                              Enumerable.{|#2:SequenceEqual|}(_enumerable, _enumerable);
                                                                              Enumerable.{|#3:Contains|}(value: _value, source: _enumerable);
                                                                          }
                                                                      }
                                                                  }
                                                                  """;

    /// <summary>
    /// Test data for verifying that conditional-access invocations of <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> are checked
    /// </summary>
    private const string ConditionalAccessContainsAndSequenceEqualTestData = """
                                                                             using System;
                                                                             using System.Collections.Concurrent;
                                                                             using System.Collections.Generic;
                                                                             using System.Collections.Immutable;
                                                                             using System.Collections.ObjectModel;
                                                                             using System.Linq;

                                                                             namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                             internal struct NotImplementedStruct;

                                                                             internal class RH1002
                                                                             {
                                                                                 internal class ConditionalAccessTest
                                                                                 {
                                                                                     private IEnumerable<NotImplementedStruct> _enumerable;
                                                                                     private NotImplementedStruct _value;

                                                                                     public void Test()
                                                                                     {
                                                                                         _ = _enumerable?{|#0:.Contains(_value)|};
                                                                                         _ = _enumerable?{|#1:.SequenceEqual(_enumerable)|};
                                                                                     }
                                                                                 }
                                                                             }
                                                                             """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> with a custom comparer are exempt
    /// </summary>
    private const string ContainsAndSequenceEqualCustomComparerTestData = """
                                                                          using System;
                                                                          using System.Collections.Concurrent;
                                                                          using System.Collections.Generic;
                                                                          using System.Collections.Immutable;
                                                                          using System.Collections.ObjectModel;
                                                                          using System.Linq;

                                                                          namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                          internal struct NotImplementedStruct;

                                                                          internal class RH1002
                                                                          {
                                                                              internal class CustomComparerTest
                                                                              {
                                                                                  private IEnumerable<NotImplementedStruct> _enumerable;
                                                                                  private NotImplementedStruct _value;
                                                                                  private IEqualityComparer<NotImplementedStruct> _comparer;

                                                                                  public void Test()
                                                                                  {
                                                                                      _enumerable.Contains(_value, _comparer);
                                                                                      _enumerable.Contains(_value, comparer: _comparer);
                                                                                      _enumerable.SequenceEqual(_enumerable, _comparer);
                                                                                      Enumerable.Contains(comparer: _comparer, value: _value, source: _enumerable);
                                                                                  }
                                                                              }
                                                                          }
                                                                          """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> with a comparer argument that is not custom are checked
    /// </summary>
    private const string ContainsAndSequenceEqualNonCustomComparerTestData = """
                                                                             using System;
                                                                             using System.Collections.Concurrent;
                                                                             using System.Collections.Generic;
                                                                             using System.Collections.Immutable;
                                                                             using System.Collections.ObjectModel;
                                                                             using System.Linq;

                                                                             namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                             internal struct NotImplementedStruct;

                                                                             internal class RH1002
                                                                             {
                                                                                 internal class NonCustomComparerTest
                                                                                 {
                                                                                     private IEnumerable<NotImplementedStruct> _enumerable;
                                                                                     private NotImplementedStruct _value;

                                                                                     public void Test()
                                                                                     {
                                                                                         _enumerable.{|#0:Contains|}(_value, null);
                                                                                         _enumerable.{|#1:Contains|}(_value, default);
                                                                                         _enumerable.{|#2:Contains|}(_value, EqualityComparer<NotImplementedStruct>.Default);
                                                                                         _enumerable.{|#3:SequenceEqual|}(_enumerable, null);
                                                                                         _enumerable.{|#4:SequenceEqual|}(_enumerable, EqualityComparer<NotImplementedStruct>.Default);
                                                                                     }
                                                                                 }
                                                                             }
                                                                             """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> are not flagged for element types with equality members or without struct semantics
    /// </summary>
    private const string ContainsAndSequenceEqualNotFlaggedElementTypesTestData = """
                                                                                  using System;
                                                                                  using System.Collections.Concurrent;
                                                                                  using System.Collections.Generic;
                                                                                  using System.Collections.Immutable;
                                                                                  using System.Collections.ObjectModel;
                                                                                  using System.Linq;

                                                                                  namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                                  internal class Class;
                                                                                  internal struct NotImplementedStruct;
                                                                                  internal struct OverrideStruct
                                                                                  {
                                                                                      public override bool Equals(object obj) => true;
                                                                                      public override int GetHashCode() => 0;
                                                                                  }
                                                                                  internal struct EquatableStruct : IEquatable<EquatableStruct>
                                                                                  {
                                                                                      public bool Equals(EquatableStruct other) => true;
                                                                                  }

                                                                                  internal class RH1002
                                                                                  {
                                                                                      internal class NotFlaggedElementTypesTest
                                                                                      {
                                                                                          private IEnumerable<Class> _classes;
                                                                                          private IEnumerable<OverrideStruct> _overrideStructs;
                                                                                          private IEnumerable<EquatableStruct> _equatableStructs;
                                                                                          private IEnumerable<int> _integers;
                                                                                          private IEnumerable<object> _objects;

                                                                                          public void Test()
                                                                                          {
                                                                                              _classes.Contains(null);
                                                                                              _classes.SequenceEqual(_classes);
                                                                                              _overrideStructs.Contains(default);
                                                                                              _overrideStructs.SequenceEqual(_overrideStructs);
                                                                                              _equatableStructs.Contains(default);
                                                                                              _equatableStructs.SequenceEqual(_equatableStructs);
                                                                                              _integers.Contains(1);
                                                                                              _integers.SequenceEqual(_integers);
                                                                                              _objects.Contains(default(NotImplementedStruct));
                                                                                          }
                                                                                      }
                                                                                  }
                                                                                  """;

    /// <summary>
    /// Test data for verifying that <c>Contains</c> and <c>SequenceEqual</c> methods not declared on <c>Enumerable</c> are not flagged
    /// </summary>
    private const string NonEnumerableContainsAndSequenceEqualTestData = """
                                                                         using System;
                                                                         using System.Collections.Concurrent;
                                                                         using System.Collections.Generic;
                                                                         using System.Collections.Immutable;
                                                                         using System.Collections.ObjectModel;
                                                                         using System.Linq;

                                                                         namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                         internal struct NotImplementedStruct;

                                                                         internal class RH1002
                                                                         {
                                                                             internal class NonEnumerableMethodTest
                                                                             {
                                                                                 private List<NotImplementedStruct> _list;
                                                                                 private HashSet<NotImplementedStruct> _hashSet;
                                                                                 private ICollection<NotImplementedStruct> _collection;
                                                                                 private IQueryable<NotImplementedStruct> _queryable;
                                                                                 private ImmutableArray<NotImplementedStruct> _immutableArray;
                                                                                 private IImmutableDictionary<NotImplementedStruct, int> _immutableDictionary;
                                                                                 private IDictionary<string, int> _dictionaryInterface;
                                                                                 private KeyValuePair<string, int> _pair;
                                                                                 private NotImplementedStruct _value;

                                                                                 public void Test()
                                                                                 {
                                                                                     _list.Contains(_value);
                                                                                     _hashSet.Contains(_value);
                                                                                     _collection.Contains(_value);
                                                                                     _queryable.Contains(_value);
                                                                                     _immutableArray.SequenceEqual(_immutableArray);
                                                                                     "text".Contains("t");
                                                                                     _immutableDictionary.Contains(_value, 1);
                                                                                     ImmutableDictionary.Contains(_immutableDictionary, _value, 1);
                                                                                     _dictionaryInterface.Contains(_pair);
                                                                                 }
                                                                             }
                                                                         }
                                                                         """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> is checked on receivers that bind it without dictionary semantics
    /// </summary>
    private const string ReadOnlyCollectionReceiverContainsTestData = """
                                                                      using System;
                                                                      using System.Collections.Concurrent;
                                                                      using System.Collections.Generic;
                                                                      using System.Collections.Immutable;
                                                                      using System.Collections.ObjectModel;
                                                                      using System.Linq;

                                                                      namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                      internal struct NotImplementedStruct;

                                                                      internal class RH1002
                                                                      {
                                                                          internal class ReadOnlyCollectionReceiverTest
                                                                          {
                                                                              private IReadOnlyList<NotImplementedStruct> _readOnlyList;
                                                                              private IReadOnlyCollection<NotImplementedStruct> _readOnlyCollection;
                                                                              private Dictionary<string, NotImplementedStruct> _dictionary;
                                                                              private NotImplementedStruct _value;

                                                                              public void Test()
                                                                              {
                                                                                  _readOnlyList.{|#0:Contains|}(_value);
                                                                                  _readOnlyCollection.{|#1:Contains|}(_value);
                                                                                  _dictionary.Values.{|#2:Contains|}(_value);
                                                                              }
                                                                          }
                                                                      }
                                                                      """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a dictionary receiver, because it delegates to the dictionary's key lookup
    /// </summary>
    private const string DictionaryReceiverContainsTestData = """
                                                              using System;
                                                              using System.Collections.Concurrent;
                                                              using System.Collections.Generic;
                                                              using System.Collections.Immutable;
                                                              using System.Collections.ObjectModel;
                                                              using System.Linq;

                                                              namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                              internal class RH1002
                                                              {
                                                                  internal class DictionaryReceiverTest
                                                                  {
                                                                      private Dictionary<string, int> _dictionary;
                                                                      private IDictionary<string, int> _dictionaryInterface;
                                                                      private SortedDictionary<string, int> _sortedDictionary;
                                                                      private ConcurrentDictionary<string, int> _concurrentDictionary;
                                                                      private ReadOnlyDictionary<string, int> _readOnlyDictionary;
                                                                      private KeyValuePair<string, int> _value;

                                                                      public void Test()
                                                                      {
                                                                          _dictionary.Contains(_value);
                                                                          Enumerable.Contains(_dictionaryInterface, _value);
                                                                          _sortedDictionary.Contains(_value);
                                                                          _concurrentDictionary.Contains(_value);
                                                                          _readOnlyDictionary.Contains(_value);
                                                                          Enumerable.Contains(_dictionary, _value);
                                                                          Enumerable.Contains(value: _value, source: _dictionary);
                                                                          _ = _dictionary?.Contains(_value);
                                                                      }
                                                                  }
                                                              }
                                                              """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a receiver typed as <c>IReadOnlyDictionary&lt;TKey, TValue&gt;</c>
    /// </summary>
    private const string ReadOnlyDictionaryReceiverContainsTestData = """
                                                                      using System;
                                                                      using System.Collections.Concurrent;
                                                                      using System.Collections.Generic;
                                                                      using System.Collections.Immutable;
                                                                      using System.Collections.ObjectModel;
                                                                      using System.Linq;

                                                                      namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                      internal class RH1002
                                                                      {
                                                                          internal class ReadOnlyDictionaryReceiverTest
                                                                          {
                                                                              private IReadOnlyDictionary<string, int> _readOnlyDictionary;
                                                                              private KeyValuePair<string, int> _value;

                                                                              public void Test()
                                                                              {
                                                                                  _readOnlyDictionary.Contains(_value);
                                                                              }
                                                                          }
                                                                      }
                                                                      """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> with a comparer argument that is not custom is checked on a dictionary receiver, because that overload compares the key/value pairs itself
    /// </summary>
    private const string DictionaryReceiverContainsWithComparerTestData = """
                                                                          using System;
                                                                          using System.Collections.Concurrent;
                                                                          using System.Collections.Generic;
                                                                          using System.Collections.Immutable;
                                                                          using System.Collections.ObjectModel;
                                                                          using System.Linq;

                                                                          namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                          internal class RH1002
                                                                          {
                                                                              internal class DictionaryReceiverWithComparerTest
                                                                              {
                                                                                  private Dictionary<string, int> _dictionary;
                                                                                  private KeyValuePair<string, int> _value;

                                                                                  public void Test()
                                                                                  {
                                                                                      _dictionary.{|#0:Contains|}(_value, null);
                                                                                      _dictionary.{|#1:Contains|}(_value, EqualityComparer<KeyValuePair<string, int>>.Default);
                                                                                  }
                                                                              }
                                                                          }
                                                                          """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> is checked on a key/value pair sequence whose static type carries no dictionary semantics
    /// </summary>
    private const string KeyValuePairSequenceContainsTestData = """
                                                                using System;
                                                                using System.Collections.Concurrent;
                                                                using System.Collections.Generic;
                                                                using System.Collections.Immutable;
                                                                using System.Collections.ObjectModel;
                                                                using System.Linq;

                                                                namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                internal class RH1002
                                                                {
                                                                    internal class KeyValuePairSequenceTest
                                                                    {
                                                                        private IEnumerable<KeyValuePair<string, int>> _enumerable;
                                                                        private Dictionary<string, int> _dictionary;
                                                                        private KeyValuePair<string, int> _value;

                                                                        public void Test()
                                                                        {
                                                                            _enumerable.{|#0:Contains|}(_value);
                                                                            ((IEnumerable<KeyValuePair<string, int>>)_dictionary).{|#1:Contains|}(_value);
                                                                        }
                                                                    }
                                                                }
                                                                """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.SequenceEqual</c> is checked on a dictionary receiver, because it compares the key/value pairs itself
    /// </summary>
    private const string DictionaryReceiverSequenceEqualTestData = """
                                                                   using System;
                                                                   using System.Collections.Concurrent;
                                                                   using System.Collections.Generic;
                                                                   using System.Collections.Immutable;
                                                                   using System.Collections.ObjectModel;
                                                                   using System.Linq;

                                                                   namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                   internal class RH1002
                                                                   {
                                                                       internal class DictionaryReceiverSequenceEqualTest
                                                                       {
                                                                           private Dictionary<string, int> _dictionary;

                                                                           public void Test()
                                                                           {
                                                                               _dictionary.{|#0:SequenceEqual|}(_dictionary);
                                                                           }
                                                                       }
                                                                   }
                                                                   """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a receiver typed as a type parameter constrained to a dictionary
    /// </summary>
    private const string TypeParameterDictionaryReceiverContainsTestData = """
                                                                           using System.Collections.Generic;
                                                                           using System.Linq;

                                                                           namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                           internal class RH1002
                                                                           {
                                                                               internal class TypeParameterDictionaryReceiverTest
                                                                               {
                                                                                   private KeyValuePair<string, int> _value;

                                                                                   public void Test<TDictionary, TReadOnlyDictionary, TConcreteDictionary, TNestedReadOnlyDictionary>(TDictionary dictionary, TReadOnlyDictionary readOnlyDictionary, TConcreteDictionary concreteDictionary, TNestedReadOnlyDictionary nestedReadOnlyDictionary)
                                                                                       where TDictionary : IDictionary<string, int>
                                                                                       where TReadOnlyDictionary : IReadOnlyDictionary<string, int>
                                                                                       where TConcreteDictionary : Dictionary<string, int>
                                                                                       where TNestedReadOnlyDictionary : TReadOnlyDictionary
                                                                                   {
                                                                                       Enumerable.Contains(dictionary, _value);
                                                                                       readOnlyDictionary.Contains(_value);
                                                                                       concreteDictionary.Contains(_value);
                                                                                       nestedReadOnlyDictionary.Contains(_value);
                                                                                   }
                                                                               }
                                                                           }
                                                                           """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a dictionary whose tuple key type differs from the searched value's only in element names
    /// </summary>
    private const string TupleKeyDictionaryReceiverContainsTestData = """
                                                                      using System.Collections.Generic;
                                                                      using System.Linq;

                                                                      namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                      internal class RH1002
                                                                      {
                                                                          internal class TupleKeyDictionaryReceiverTest
                                                                          {
                                                                              private Dictionary<(int First, int Second), int> _dictionary;

                                                                              public void Test()
                                                                              {
                                                                                  _dictionary.Contains(new KeyValuePair<(int, int), int>((1, 2), 3));
                                                                                  _dictionary.Contains(KeyValuePair.Create((1, 2), 3));
                                                                              }
                                                                          }
                                                                      }
                                                                      """;

    /// <summary>
    /// Test data for verifying that <c>Enumerable.Contains</c> is checked on a concrete type that implements only the read-only dictionary interface, directly or through a type parameter constrained to it, because it is not a collection that the call delegates to
    /// </summary>
    private const string ReadOnlyOnlyDictionaryReceiverContainsTestData = """
                                                                          using System.Collections;
                                                                          using System.Collections.Generic;
                                                                          using System.Linq;

                                                                          namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                                                          internal abstract class ReadOnlyOnlyDictionary : IReadOnlyDictionary<string, int>
                                                                          {
                                                                              public abstract int this[string key] { get; }
                                                                              public abstract IEnumerable<string> Keys { get; }
                                                                              public abstract IEnumerable<int> Values { get; }
                                                                              public abstract int Count { get; }
                                                                              public abstract bool ContainsKey(string key);
                                                                              public abstract bool TryGetValue(string key, out int value);
                                                                              public abstract IEnumerator<KeyValuePair<string, int>> GetEnumerator();
                                                                              IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                                                                          }

                                                                          internal class RH1002
                                                                          {
                                                                              internal class ReadOnlyOnlyDictionaryReceiverTest
                                                                              {
                                                                                  private ReadOnlyOnlyDictionary _dictionary;
                                                                                  private KeyValuePair<string, int> _value;

                                                                                  public void Test()
                                                                                  {
                                                                                      _dictionary.{|#0:Contains|}(_value);
                                                                                  }

                                                                                  public void Test<TReadOnlyOnlyDictionary>(TReadOnlyOnlyDictionary dictionary)
                                                                                      where TReadOnlyOnlyDictionary : ReadOnlyOnlyDictionary
                                                                                  {
                                                                                      dictionary.{|#1:Contains|}(_value);
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
    /// Verifying that wrapped <see langword="null"/> comparer arguments do not exempt diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedNullComparerArgumentsDoNotExempt()
    {
        await Verify(WrappedNullComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that only a nullable value-type default comparer is null-like
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyValueTypeDefaultComparerNullabilityIsRespected()
    {
        await Verify(ValueTypeDefaultComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that <c>EqualityComparer&lt;T&gt;.Default</c> does not exempt diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFrameworkDefaultComparerDoesNotExempt()
    {
        await Verify(FrameworkDefaultComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that composite comparer expressions which necessarily produce <see langword="null"/> do not
    /// exempt diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCompositeNullComparerDoesNotExempt()
    {
        await Verify(CompositeNullComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 6));
    }

    /// <summary>
    /// Verifying that a named <c>keySelector</c> argument in its natural position does not desync positional
    /// matching for a subsequent, unnamed comparer argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNamedKeySelectorArgumentDoesNotDesyncComparerDetection()
    {
        await Verify(NamedKeySelectorArgumentTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that a <see langword="default"/> or <c>default(T)</c> comparer argument does not exempt the
    /// diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDefaultComparerArgumentDoesNotExempt()
    {
        await Verify(DefaultComparerArgumentTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
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

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsIsChecked()
    {
        await Verify(ContainsTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.SequenceEqual</c> is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySequenceEqualIsChecked()
    {
        await Verify(SequenceEqualTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that the static invocation forms of <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStaticContainsAndSequenceEqualFormsAreChecked()
    {
        await Verify(StaticContainsAndSequenceEqualTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that conditional-access invocations of <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConditionalAccessContainsAndSequenceEqualAreChecked()
    {
        await Verify(ConditionalAccessContainsAndSequenceEqualTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> with a custom comparer are exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsAndSequenceEqualWithCustomComparerAreExempt()
    {
        await Verify(ContainsAndSequenceEqualCustomComparerTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> with a comparer argument that is not custom are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsAndSequenceEqualWithNonCustomComparerAreChecked()
    {
        await Verify(ContainsAndSequenceEqualNonCustomComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 5));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> are not flagged for element types with equality members or without struct semantics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsAndSequenceEqualAreNotFlaggedForEqualityCapableElementTypes()
    {
        await Verify(ContainsAndSequenceEqualNotFlaggedElementTypesTestData);
    }

    /// <summary>
    /// Verifying that <c>Contains</c> and <c>SequenceEqual</c> methods not declared on <c>Enumerable</c> are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsAndSequenceEqualNotDeclaredOnEnumerableAreNotFlagged()
    {
        await Verify(NonEnumerableContainsAndSequenceEqualTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> is checked on receivers that bind it without dictionary semantics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsOnReceiversWithoutDictionarySemanticsIsChecked()
    {
        await Verify(ReadOnlyCollectionReceiverContainsTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a dictionary receiver, because it delegates to the dictionary's key lookup
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsWithoutComparerOnDictionaryReceiverIsNotFlagged()
    {
        await Verify(DictionaryReceiverContainsTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a receiver typed as <c>IReadOnlyDictionary&lt;TKey, TValue&gt;</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsWithoutComparerOnReadOnlyDictionaryReceiverIsNotFlagged()
    {
        await Verify(ReadOnlyDictionaryReceiverContainsTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> with a comparer argument that is not custom is checked on a dictionary receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsWithNonCustomComparerOnDictionaryReceiverIsChecked()
    {
        await Verify(DictionaryReceiverContainsWithComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> is checked on a key/value pair sequence whose static type carries no dictionary semantics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsOnKeyValuePairSequenceWithoutDictionaryTypeIsChecked()
    {
        await Verify(KeyValuePairSequenceContainsTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.SequenceEqual</c> is checked on a dictionary receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySequenceEqualOnDictionaryReceiverIsChecked()
    {
        await Verify(DictionaryReceiverSequenceEqualTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a receiver typed as a type parameter constrained to a dictionary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsWithoutComparerOnTypeParameterDictionaryReceiverIsNotFlagged()
    {
        await Verify(TypeParameterDictionaryReceiverContainsTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a dictionary whose tuple key type differs from the searched value's only in element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsWithoutComparerOnDictionaryWithNamedTupleKeyIsNotFlagged()
    {
        await Verify(TupleKeyDictionaryReceiverContainsTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> is checked on a concrete type that implements only the read-only dictionary interface, directly or through a type parameter constrained to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContainsOnConcreteReadOnlyOnlyDictionaryIsChecked()
    {
        await Verify(ReadOnlyOnlyDictionaryReceiverContainsTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    #endregion // Methods
}