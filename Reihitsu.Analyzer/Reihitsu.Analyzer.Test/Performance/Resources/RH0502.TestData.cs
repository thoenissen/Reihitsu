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