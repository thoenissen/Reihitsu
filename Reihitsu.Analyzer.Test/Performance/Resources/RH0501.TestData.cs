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

internal class RH0501
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