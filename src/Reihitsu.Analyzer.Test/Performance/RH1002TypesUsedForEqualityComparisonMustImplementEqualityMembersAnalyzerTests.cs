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
    [Requirement("SHARED-EQUAL-001")]
    public async Task VerifyStructImplementingIEquatableTransitivelyIsNotFlagged()
    {
        await Verify(TransitiveEquatableTestData);
    }

    /// <summary>
    /// Verifying that a key-selector overload checks the projected key type instead of the source element type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-002")]
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
    [Requirement("RH1002-002")]
    public async Task VerifyKeyValuePairSourceOverloadsCheckKeyTypeNotValueType()
    {
        await Verify(KeyValuePairSourceTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that overloads receiving an explicit custom <c>IEqualityComparer&lt;T&gt;</c> are exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    [Requirement("RH1002-001")]
    public async Task VerifyComparerOverloadsAreExempt()
    {
        await Verify(ComparerOverloadTestData);
    }

    /// <summary>
    /// Verifying that an explicit <see langword="null"/> comparer argument does not exempt the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyExplicitNullComparerArgumentDoesNotExempt()
    {
        await Verify(ExplicitNullComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that wrapped <see langword="null"/> comparer arguments do not exempt diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyWrappedNullComparerArgumentsDoNotExempt()
    {
        await Verify(WrappedNullComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that only a nullable value-type default comparer is null-like
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyValueTypeDefaultComparerNullabilityIsRespected()
    {
        await Verify(ValueTypeDefaultComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that <c>EqualityComparer&lt;T&gt;.Default</c> does not exempt diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
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
    [Requirement("SHARED-EQUAL-002")]
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
    [Requirement("SHARED-EQUAL-002")]
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
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyDefaultComparerArgumentDoesNotExempt()
    {
        await Verify(DefaultComparerArgumentTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that the <c>*By</c> family of methods are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-001")]
    public async Task VerifyByFamilyMethodsAreChecked()
    {
        await Verify(ByFamilyTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.ToHashSet</c> is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-001")]
    public async Task VerifyToHashSetIsChecked()
    {
        await Verify(ToHashSetTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-001")]
    public async Task VerifyContainsIsChecked()
    {
        await Verify(ContainsTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.SequenceEqual</c> is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-001")]
    public async Task VerifySequenceEqualIsChecked()
    {
        await Verify(SequenceEqualTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that the static invocation forms of <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-001")]
    [Requirement("RH1002-005")]
    public async Task VerifyStaticContainsAndSequenceEqualFormsAreChecked()
    {
        await Verify(StaticContainsAndSequenceEqualTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that conditional-access invocations of <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-005")]
    public async Task VerifyConditionalAccessContainsAndSequenceEqualAreChecked()
    {
        await Verify(ConditionalAccessContainsAndSequenceEqualTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> with a custom comparer are exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyContainsAndSequenceEqualWithCustomComparerAreExempt()
    {
        await Verify(ContainsAndSequenceEqualCustomComparerTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> and <c>Enumerable.SequenceEqual</c> with a comparer argument that is not custom are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
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
    [Requirement("RH1002-003")]
    public async Task VerifyContainsAndSequenceEqualNotDeclaredOnEnumerableAreNotFlagged()
    {
        await Verify(NonEnumerableContainsAndSequenceEqualTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> is checked on receivers that bind it without dictionary semantics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifyContainsOnReceiversWithoutDictionarySemanticsIsChecked()
    {
        await Verify(ReadOnlyCollectionReceiverContainsTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a dictionary receiver, because it delegates to the dictionary's key lookup
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifyContainsWithoutComparerOnDictionaryReceiverIsNotFlagged()
    {
        await Verify(DictionaryReceiverContainsTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a receiver typed as <c>IReadOnlyDictionary&lt;TKey, TValue&gt;</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifyContainsWithoutComparerOnReadOnlyDictionaryReceiverIsNotFlagged()
    {
        await Verify(ReadOnlyDictionaryReceiverContainsTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> with a comparer argument that is not custom is checked on a dictionary receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifyContainsWithNonCustomComparerOnDictionaryReceiverIsChecked()
    {
        await Verify(DictionaryReceiverContainsWithComparerTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> is checked on a key/value pair sequence whose static type carries no dictionary semantics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifyContainsOnKeyValuePairSequenceWithoutDictionaryTypeIsChecked()
    {
        await Verify(KeyValuePairSequenceContainsTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.SequenceEqual</c> is checked on a dictionary receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifySequenceEqualOnDictionaryReceiverIsChecked()
    {
        await Verify(DictionaryReceiverSequenceEqualTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a receiver typed as a type parameter constrained to a dictionary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifyContainsWithoutComparerOnTypeParameterDictionaryReceiverIsNotFlagged()
    {
        await Verify(TypeParameterDictionaryReceiverContainsTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> without a comparer is not flagged on a dictionary whose tuple key type differs from the searched value's only in element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifyContainsWithoutComparerOnDictionaryWithNamedTupleKeyIsNotFlagged()
    {
        await Verify(TupleKeyDictionaryReceiverContainsTestData);
    }

    /// <summary>
    /// Verifying that <c>Enumerable.Contains</c> is checked on a concrete type that implements only the read-only dictionary interface, directly or through a type parameter constrained to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifyContainsOnConcreteReadOnlyOnlyDictionaryIsChecked()
    {
        await Verify(ReadOnlyOnlyDictionaryReceiverContainsTestData, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a call of every listed method is reported on the method name when it compares a struct without
    /// equality members and passes no custom comparer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-001")]
    [Requirement("RH1002-005")]
    public async Task VerifyEveryListedMethodIsReported()
    {
        const string source = """
                              using System.Collections.Frozen;
                              using System.Collections.Generic;
                              using System.Collections.Immutable;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items, S item)
                                  {
                                      items.{|#0:Distinct|}();
                                      items.{|#1:DistinctBy|}(x => x);
                                      items.{|#2:Union|}(items);
                                      items.{|#3:UnionBy|}(items, x => x);
                                      items.{|#4:Intersect|}(items);
                                      items.{|#5:IntersectBy|}(items, x => x);
                                      items.{|#6:Except|}(items);
                                      items.{|#7:ExceptBy|}(items, x => x);
                                      items.{|#8:ToHashSet|}();
                                      items.{|#9:ToLookup|}(x => x);
                                      items.{|#10:ToDictionary|}(x => x);
                                      items.{|#11:GroupBy|}(x => x);
                                      items.{|#12:Join|}(items, x => x, x => x, (a, b) => a);
                                      items.{|#13:GroupJoin|}(items, x => x, x => x, (a, b) => a);
                                      items.{|#14:Contains|}(item);
                                      items.{|#15:SequenceEqual|}(items);
                                      items.{|#16:ToImmutableHashSet|}();
                                      items.{|#17:ToImmutableDictionary|}(x => x, x => 0);
                                      items.{|#18:ToFrozenSet|}();
                                      items.{|#19:ToFrozenDictionary|}(x => x);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 20));
    }

    /// <summary>
    /// Verifying that the listed methods are not reported for an element type that has its own equality members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-001")]
    [Requirement("SHARED-EQUAL-001")]
    public async Task VerifyListedMethodsAreNotReportedForTypesWithEqualityMembers()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              using System.Linq;

                              struct WithMembers
                              {
                                  public override bool Equals(object obj) => true;

                                  public override int GetHashCode() => 0;
                              }

                              struct Equatable : IEquatable<Equatable>
                              {
                                  public bool Equals(Equatable other) => true;

                                  public override int GetHashCode() => 0;
                              }

                              record struct Point(int X);

                              class C
                              {
                                  void M(IEnumerable<WithMembers> a, IEnumerable<Equatable> b, IEnumerable<Point> c, IEnumerable<string> d, IEnumerable<int> e)
                                  {
                                      a.Distinct();
                                      b.ToHashSet();
                                      c.GroupBy(x => x);
                                      d.Contains("");
                                      e.SequenceEqual(e);
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that the selected key type is checked for an overload with a key selector, the first type argument
    /// otherwise, and the key type for an overload built from key/value pairs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-002")]
    public async Task VerifyComparedTypeIsTheSelectedOrPairKey()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                                  public int Value;
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items, IEnumerable<int> numbers, IEnumerable<KeyValuePair<S, int>> byKey, IEnumerable<KeyValuePair<int, S>> byValue)
                                  {
                                      items.GroupBy(x => x.Value);
                                      items.ToDictionary(x => x.Value);
                                      items.DistinctBy(x => x.Value);
                                      numbers.{|#0:GroupBy|}(x => new S());
                                      byKey.{|#1:ToDictionary|}();
                                      byValue.ToDictionary();
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that methods with a listed name that bind to other types are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-003")]
    [Requirement("RH1002-001")]
    public async Task VerifyMethodsOfOtherTypesAreNotReported()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Collections.Immutable;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(List<S> list, IQueryable<S> query, ImmutableDictionary<S, int> map, S item)
                                  {
                                      list.Contains(item);
                                      query.Distinct();
                                      map.Contains(item, 0);
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that <c>Contains</c> without a comparer is not reported on a dictionary, a read-only dictionary
    /// interface, or a type parameter constrained to it, and that a class implementing only the read-only interface,
    /// a plain pair sequence, a call with a comparer that is not custom, and <c>SequenceEqual</c> are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-004")]
    public async Task VerifyContainsOnDictionaryIsNotReported()
    {
        const string source = """
                              using System.Collections;
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              abstract class ReadOnlyMap : IReadOnlyDictionary<S, int>
                              {
                                  public abstract int this[S key] { get; }
                                  public abstract IEnumerable<S> Keys { get; }
                                  public abstract IEnumerable<int> Values { get; }
                                  public abstract int Count { get; }
                                  public abstract bool ContainsKey(S key);
                                  public abstract bool TryGetValue(S key, out int value);
                                  public abstract IEnumerator<KeyValuePair<S, int>> GetEnumerator();
                                  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                              }

                              class C
                              {
                                  void M(Dictionary<S, int> dictionary, IReadOnlyDictionary<S, int> readOnly, ReadOnlyMap map, IEnumerable<KeyValuePair<S, int>> pairs, KeyValuePair<S, int> pair)
                                  {
                                      dictionary.Contains(pair);
                                      readOnly.Contains(pair);
                                      map.{|#0:Contains|}(pair);
                                      pairs.{|#1:Contains|}(pair);
                                      dictionary.{|#2:Contains|}(pair, null);
                                      dictionary.{|#3:SequenceEqual|}(pairs);
                                  }

                                  bool N<T>(T source, KeyValuePair<S, int> pair)
                                      where T : IReadOnlyDictionary<S, int>
                                  {
                                      return source.Contains(pair);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that the diagnostic is on the method name with its type argument list, on the method name of a static
    /// call, and on the whole call from its dot on for a null-conditional call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH1002-005")]
    public async Task VerifyDiagnosticLocationOnNameTypeArgumentsAndNullConditionalCall()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items)
                                  {
                                      items.{|#0:Distinct<S>|}();
                                      Enumerable.{|#1:Distinct|}(items);
                                      items?{|#2:.Distinct()|};
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that the form of a comparer argument decides whether it counts as custom: an object creation, a
    /// variable passed as a named argument or positionally after a non-trailing named argument, a conditional or
    /// <see langword="switch"/> with a variable branch or arm, and <see langword="default"/> of a non-nullable value-type
    /// comparer count as custom, while <see langword="null"/>, <see langword="default"/>,
    /// <c>EqualityComparer&lt;T&gt;.Default</c>, a cast of <see langword="null"/> inside parentheses behind <c>!</c>, a
    /// conditional or <see langword="switch"/> whose branches or arms all have such a form, a constant condition whose
    /// chosen branch has one, a <c>??</c> of two such forms, a null-conditional access on a cast of
    /// <see langword="null"/>, and <see langword="default"/> of a nullable value-type comparer do not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    [Requirement("RH1002-001")]
    public async Task VerifyComparerArgumentCountsAsCustomByItsForm()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class SComparer : IEqualityComparer<S>
                              {
                                  public bool Equals(S x, S y) => true;

                                  public int GetHashCode(S obj) => 0;
                              }

                              struct ValueComparer : IEqualityComparer<S>
                              {
                                  public bool Equals(S x, S y) => true;

                                  public int GetHashCode(S obj) => 0;
                              }

                              class Holder
                              {
                                  public IEqualityComparer<S> Comparer { get; set; }
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items, IEqualityComparer<S> comparer, bool flag, int mode)
                                  {
                                      items.Distinct(new SComparer());
                                      items.Distinct(comparer: comparer);
                                      Enumerable.Distinct(source: items, comparer);
                                      items.Distinct(flag ? comparer : null);
                                      items.Distinct(mode switch { 0 => comparer, _ => null });
                                      items.Distinct(default(ValueComparer));
                                      items.{|#0:Distinct|}(null);
                                      items.{|#1:Distinct|}(default);
                                      items.{|#2:Distinct|}(EqualityComparer<S>.Default);
                                      items.{|#3:Distinct|}(((IEqualityComparer<S>)null)!);
                                      items.{|#4:Distinct|}(flag ? null : EqualityComparer<S>.Default);
                                      items.{|#5:Distinct|}(true ? null : comparer);
                                      items.{|#6:Distinct|}((IEqualityComparer<S>)null ?? EqualityComparer<S>.Default);
                                      items.{|#7:Distinct|}(mode switch { 0 => null, _ => EqualityComparer<S>.Default });
                                      items.{|#8:Distinct|}(((Holder)null)?.Comparer);
                                      items.{|#9:Distinct|}(default(ValueComparer?));
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 10));
    }

    /// <summary>
    /// Verifying that a conditional expression with a constant condition counts only its chosen branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyConstantConditionCountsOnlyChosenBranch()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items, IEqualityComparer<S> comparer)
                                  {
                                      items.Distinct(false ? null : comparer);
                                      items.{|#0:Distinct|}(false ? comparer : null);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat));
    }

    /// <summary>
    /// Characterizes a comparer passed for the value type of <c>ToImmutableDictionary</c>: it counts as a custom
    /// comparer for the struct key, so the call is not reported, while the call without comparers is. The case is an
    /// open question about intended behavior, so this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyValueComparerCountsAsCustomComparerForKey()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Collections.Immutable;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items, IEqualityComparer<int> valueComparer)
                                  {
                                      items.ToImmutableDictionary(x => x, x => 0, null, valueComparer);
                                      items.{|#0:ToImmutableDictionary|}(x => x, x => 0);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat));
    }

    /// <summary>
    /// Characterizes equality-based methods and factories outside the rule's fixed list of names: <c>CountBy</c>,
    /// <c>AggregateBy</c>, <c>ImmutableHashSet.Create</c>, <c>ImmutableHashSet.CreateRange</c>, and
    /// <c>ImmutableDictionary.CreateRange</c> are not reported, while <c>GroupBy</c> is. These cases are an open
    /// question about intended behavior, so this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEqualityBasedMethodsOutsideTheListAreNotReported()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Collections.Immutable;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items, IEnumerable<KeyValuePair<S, int>> pairs)
                                  {
                                      items.CountBy(x => x);
                                      items.AggregateBy(x => x, 0, (count, x) => count + 1);
                                      var set = ImmutableHashSet.Create<S>();
                                      var range = ImmutableHashSet.CreateRange(items);
                                      var map = ImmutableDictionary.CreateRange(pairs);
                                      items.{|#0:GroupBy|}(x => x);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat));
    }

    /// <summary>
    /// Verifying that the form of the argument decides whether a comparer is custom: parentheses, <c>!</c>, a <c>??</c>,
    /// conditional, or <see langword="switch"/> with a custom operand, a <see langword="throw"/> branch next to a custom
    /// one, a null-conditional access on a receiver that is not <see langword="null"/>, a cast with a user-defined
    /// conversion, and a local variable initialized with <see langword="null"/> count as custom, while forms that can only
    /// produce <see langword="null"/> or <c>EqualityComparer&lt;T&gt;.Default</c>, including
    /// <c>EqualityComparer&lt;T&gt;.Default ?? comparer</c>, do not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyFormOfArgumentDecidesWhetherComparerIsCustom()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class Token
                              {
                              }

                              class SComparer : IEqualityComparer<S>
                              {
                                  public bool Equals(S x, S y) => true;

                                  public int GetHashCode(S obj) => 0;

                                  public static implicit operator SComparer(Token token) => new SComparer();
                              }

                              struct ValueComparer : IEqualityComparer<S>
                              {
                                  public bool Equals(S x, S y) => true;

                                  public int GetHashCode(S obj) => 0;
                              }

                              class Holder
                              {
                                  public IEqualityComparer<S> Comparer { get; set; }
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items, IEqualityComparer<S> comparer, Holder holder, bool flag, int mode)
                                  {
                                      IEqualityComparer<S> none = null;

                                      items.Distinct(new SComparer());
                                      items.Distinct(comparer: comparer);
                                      Enumerable.Distinct(source: items, comparer);
                                      items.Distinct((comparer));
                                      items.Distinct(comparer!);
                                      items.Distinct(comparer ?? null);
                                      items.Distinct(flag ? comparer : null);
                                      items.Distinct(flag ? comparer : throw new InvalidOperationException());
                                      items.Distinct(mode switch { 0 => comparer, _ => null });
                                      items.Distinct(holder?.Comparer);
                                      items.Distinct((SComparer)(Token)null);
                                      items.Distinct(none);
                                      items.Distinct(default(ValueComparer));
                                      items.{|#0:Distinct|}(null);
                                      items.{|#1:Distinct|}(default);
                                      items.{|#2:Distinct|}(EqualityComparer<S>.Default);
                                      items.{|#3:Distinct|}(((IEqualityComparer<S>)null)!);
                                      items.{|#4:Distinct|}(flag ? null : EqualityComparer<S>.Default);
                                      items.{|#5:Distinct|}(true ? null : comparer);
                                      items.{|#6:Distinct|}((IEqualityComparer<S>)null ?? EqualityComparer<S>.Default);
                                      items.{|#7:Distinct|}(EqualityComparer<S>.Default ?? comparer);
                                      items.{|#8:Distinct|}(mode switch { 0 => null, _ => EqualityComparer<S>.Default });
                                      items.{|#9:Distinct|}(((Holder)null)?.Comparer);
                                      items.{|#10:Distinct|}(default(ValueComparer?));
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 11));
    }

    /// <summary>
    /// Characterizes a comparer argument that can only produce <see langword="null"/> or throw: a conditional with a
    /// <see langword="throw"/> branch and a <see langword="switch"/> with a <see langword="throw"/> arm count as custom
    /// and are not reported, while the conditional that can only produce <see langword="null"/> or
    /// <c>EqualityComparer&lt;T&gt;.Default</c> is. The case is an open question about intended behavior, so this test
    /// pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyArgumentThatCanOnlyProduceNullOrThrowCountsAsCustom()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items, bool flag, int mode)
                                  {
                                      items.Distinct(flag ? null : throw new InvalidOperationException());
                                      items.Distinct(mode switch { 0 => null, _ => throw new InvalidOperationException() });
                                      items.{|#0:Distinct|}(flag ? null : EqualityComparer<S>.Default);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat));
    }

    /// <summary>
    /// Characterizes the <c>group … by</c>, <c>join</c>, and <c>join … into</c> clauses of a query expression: they are not
    /// reported, while the <c>GroupBy</c> call is. The case is an open question about intended behavior, so this test
    /// pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyQueryExpressionClausesAreNotReported()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items)
                                  {
                                      var groups = from x in items group x by x;
                                      var joined = from a in items join b in items on a equals b select a;
                                      var grouped = from a in items join b in items on a equals b into g select g;
                                      var method = items.{|#0:GroupBy|}(x => x);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat));
    }

    /// <summary>
    /// Characterizes a comparer argument that can only produce <see langword="null"/> or
    /// <c>EqualityComparer&lt;T&gt;.Default</c> and is wrapped as a whole in parentheses or behind the null-forgiving
    /// operator <c>!</c>: the unwrapped conditional and the unwrapped <see langword="as"/> expression are reported, while
    /// the wrapped ones count as custom and are not. The case is an open question about intended behavior, so this test
    /// pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedArgumentThatCanOnlyProduceNullOrDefaultComparerIsNotReported()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  void M(IEnumerable<S> items, bool flag)
                                  {
                                      items.{|#0:Distinct|}(flag ? null : EqualityComparer<S>.Default);
                                      items.Distinct((flag ? null : EqualityComparer<S>.Default));
                                      items.Distinct((flag ? null : EqualityComparer<S>.Default)!);
                                      items.{|#1:Distinct|}(null as IEqualityComparer<S>);
                                      items.Distinct((null as IEqualityComparer<S>));
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a constant with the value <see langword="null"/>, a <see langword="const"/> field, a
    /// <see langword="const"/> local, or a conditional choosing between <see langword="null"/> and such a constant, is not
    /// a custom comparer, while a local that is not a <see langword="const"/> and is initialized with
    /// <see langword="null"/> is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyConstantWithValueNullIsNotCustomComparer()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  const IEqualityComparer<S> NoComparer = null;

                                  void M(IEnumerable<S> items, bool flag)
                                  {
                                      const IEqualityComparer<S> none = null;
                                      IEqualityComparer<S> notConst = null;

                                      items.{|#0:Distinct|}(NoComparer);
                                      items.{|#1:Distinct|}(none);
                                      items.{|#2:Distinct|}(flag ? null : NoComparer);
                                      items.Distinct(notConst);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 3));
    }

    /// <summary>
    /// Characterizes <see langword="default"/> of a type parameter constrained to the comparer interface as a comparer
    /// argument: the calls with <c>default(TC)</c>, <c>default(TN)</c>, and <c>default(TR)</c> are reported, while the
    /// call with <c>default(TV)</c>, whose type parameter is constrained to a value type, counts as custom and is not.
    /// The case is an open question about intended behavior, so this test pins today's behavior without stating it as
    /// intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDefaultOfTypeParameterNotConstrainedToValueTypeCountsAsNoComparer()
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
                                      items.{|#0:Distinct|}(default(TC));
                                      items.{|#1:Distinct|}(default(TN));
                                      items.{|#2:Distinct|}(default(TR));
                                      items.Distinct(default(TV));
                                  }

                                  void N<TC, TV>()
                                      where TC : IEqualityComparer<S>
                                      where TV : struct, IEqualityComparer<S>
                                  {
                                      Dictionary<S, int> map = new(default(TC));
                                      Dictionary<S, int> other = new(default(TV));
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that a constant with the value <see langword="null"/> converted by the <see langword="as"/> operator,
    /// whether it is a <see langword="const"/> field or the literal, is not a custom comparer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyConstantNullConvertedByAsIsNotCustomComparer()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  const IEqualityComparer<S> NoComparer = null;

                                  void M(IEnumerable<S> items)
                                  {
                                      items.{|#0:Distinct|}(NoComparer as IEqualityComparer<S>);
                                      items.{|#1:Distinct|}(null as IEqualityComparer<S>);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a property other than <c>EqualityComparer&lt;T&gt;.Default</c> and a method call are custom
    /// comparers, also when the call returns <see langword="null"/>, while <c>EqualityComparer&lt;T&gt;.Default</c> is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyPropertyOrMethodCallArgumentIsCustomComparer()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class C
                              {
                                  IEqualityComparer<S> Prop { get; set; }

                                  IEqualityComparer<S> Make() => null;

                                  void M(IEnumerable<S> items)
                                  {
                                      items.Distinct(Prop);
                                      items.Distinct(Make());
                                      items.{|#0:Distinct|}(EqualityComparer<S>.Default);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat));
    }

    /// <summary>
    /// Verifying that a field that is not a constant or an object creation converted by the <see langword="as"/>
    /// operator is a custom comparer, while the literal <see langword="null"/> converted by it is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyCustomFormConvertedByAsIsCustomComparer()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class SComparer : IEqualityComparer<S>
                              {
                                  public bool Equals(S x, S y) => true;

                                  public int GetHashCode(S obj) => 0;
                              }

                              class C
                              {
                                  IEqualityComparer<S> _field;
                                  object _boxed;

                                  void M(IEnumerable<S> items)
                                  {
                                      items.Distinct(_field as IEqualityComparer<S>);
                                      items.Distinct(_boxed as IEqualityComparer<S>);
                                      items.Distinct(new SComparer() as IEqualityComparer<S>);
                                      items.{|#0:Distinct|}(null as IEqualityComparer<S>);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat));
    }

    /// <summary>
    /// Verifying that a null-conditional access whose receiver is a field that is not a constant, a property, or a
    /// method call is a custom comparer, while one whose receiver is a cast of <see langword="null"/> is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyNullConditionalAccessOnFieldPropertyOrCallIsCustomComparer()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class Holder
                              {
                                  public IEqualityComparer<S> Comparer { get; set; }
                              }

                              class C
                              {
                                  Holder _holder;

                                  Holder HolderProp { get; set; }

                                  Holder MakeHolder() => null;

                                  void M(IEnumerable<S> items)
                                  {
                                      items.Distinct(_holder?.Comparer);
                                      items.Distinct(HolderProp?.Comparer);
                                      items.Distinct(MakeHolder()?.Comparer);
                                      items.{|#0:Distinct|}(((Holder)null)?.Comparer);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat));
    }

    /// <summary>
    /// Verifying that a null-conditional access whose receiver is a constant with the value <see langword="null"/> or
    /// <see langword="default"/> of a reference type or of a nullable value type — a <see langword="const"/> field, a
    /// <see langword="const"/> local, <c>default(Holder)</c>, or <c>default(VH?)</c> — is not a custom comparer, while one
    /// whose receiver is a local that is not a <see langword="const"/> and is initialized with <see langword="null"/> is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EQUAL-002")]
    public async Task VerifyNullConditionalAccessOnNullConstantReceiverIsNotCustomComparer()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              struct S
                              {
                              }

                              class Holder
                              {
                                  public IEqualityComparer<S> Comparer { get; set; }
                              }

                              struct VH
                              {
                                  public IEqualityComparer<S> Comparer { get; set; }
                              }

                              class C
                              {
                                  const Holder NullHolder = null;

                                  void M(IEnumerable<S> items)
                                  {
                                      const Holder isConst = null;
                                      Holder notConst = null;

                                      items.{|#0:Distinct|}(NullHolder?.Comparer);
                                      items.{|#1:Distinct|}(isConst?.Comparer);
                                      items.{|#2:Distinct|}(default(Holder)?.Comparer);
                                      items.{|#3:Distinct|}(default(VH?)?.Comparer);
                                      items.Distinct(notConst?.Comparer);
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH1002MessageFormat, 4));
    }

    #endregion // Methods
}