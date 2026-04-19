using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Performance;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Performance;

/// <summary>
/// Test methods for <see cref="RH0502TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer"/>
/// </summary>
[TestClass]
public class RH0502TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzerTests : AnalyzerTestsBase<RH0502TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer>
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

                                    internal class RH0501
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

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifying that struct types used for equality comparison in LINQ methods must implement equality members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStructTypesUsedForEqualityComparisonMustImplementEqualityMembers()
    {
        await Verify(TestData, Diagnostics(RH0502TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.DiagnosticId, AnalyzerResources.RH0502MessageFormat, 13));
    }

    #endregion // Methods
}