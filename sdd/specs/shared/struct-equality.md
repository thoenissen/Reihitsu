# Shared — Struct equality members

| | |
|---|---|
| ID prefix | SHARED-EQUAL |
| Status | Active |
| Code owners | `StructEqualityPerformanceAnalyzerBase`, `EqualityComparerArgumentUtilities` |
| Uses | — |

This spec covers the rules that report a struct that is compared for equality, or hashed, through its default equality behavior. It defines when a type has its own equality members and when an `IEqualityComparer<T>` argument replaces them. Each rule that uses this spec states which operations it checks, which type of the operation it checks, and where its diagnostic is located.

## Non-goals

- Which operations a rule checks and which of their types is compared — each rule's spec.
- Whether the equality members a struct declares are correct or fast. Only their presence is checked.
- A code fix: no rule of this kind offers one, because the equality of a struct depends on what its fields mean.

## Requirements

### SHARED-EQUAL-001 — A struct without its own equality members

A rule MUST treat a type as lacking equality members exactly when it is a struct, including a generic struct constructed with type arguments, that neither implements `IEquatable<T>` of itself, directly or through another interface, nor declares overrides of both `Equals(object)` and `GetHashCode()`. A type that is not a struct — a class, an interface, an enum, or a type parameter — MUST NOT be treated as lacking equality members, and neither MUST a record struct, whose compiler-generated members implement `IEquatable<T>`.

This requirement covers a struct that implements `IEquatable<T>` of itself only when the struct also overrides `GetHashCode()`. For a generic struct, it covers only one whose type arguments are not structs that lack equality members themselves.

Why: A struct that declares no equality members falls back to `ValueType.Equals` and `ValueType.GetHashCode`, which box the value and may inspect its fields through reflection on every call. Equality-based operations call them many times, so the cost adds up. Overriding only `Equals(object)` still leaves the slow `GetHashCode`, so both overrides are needed. An enum, a class, and a type parameter do not use `ValueType`'s members this way, or their equality is not known at the place of use.

Example — RH1001:

```csharp
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

record struct Point(int X);

enum Color
{
}

class C<T>
    where T : struct
{
    HashSet<{|RH1001:Plain|}> a;
    HashSet<EqualsAndHash> b;
    HashSet<{|RH1001:EqualsOnly|}> c;
    HashSet<Equatable> d;
    HashSet<Point> e;
    HashSet<Color> f;
    HashSet<T> g;
    HashSet<int> h;
    HashSet<string> i;
    HashSet<{|RH1001:KeyValuePair<int, int>|}> j;
}
```

### SHARED-EQUAL-002 — A custom comparer argument replaces the equality members

A rule MUST treat an operation as not using the compared type's equality members when one of its arguments, positional or named, is passed to a parameter of type `IEqualityComparer<T>` and can produce a value other than `null` and `EqualityComparer<T>.Default`. An argument MUST count as producing only those two values when it is:

- `null`, or `default` of a reference type or of a nullable value type;
- `EqualityComparer<T>.Default`;
- one of these values inside parentheses, behind the null-forgiving operator `!`, or converted by a cast that is not a user-defined conversion;
- a `??` expression whose two operands count as such;
- a conditional expression whose two branches count as such, or, when its condition is a constant, whose chosen branch counts as such;
- a `switch` expression whose arms all count as such;
- a null-conditional access whose receiver is `null`.

`default` of a non-nullable value type that implements `IEqualityComparer<T>` is a comparer and MUST count as a custom one.

Why: A comparer passed to the operation does the comparing and hashing itself, so the type's own members are never called. `null` and `EqualityComparer<T>.Default` both fall back to the type's own equality behavior, so passing them changes nothing, however the expression is written. A value-type comparer is boxed into a real instance and is used.

Example — RH1002:

```csharp
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

class C
{
    void M(IEnumerable<S> items, IEqualityComparer<S> comparer, bool flag)
    {
        items.Distinct(new SComparer());
        items.Distinct(comparer: comparer);
        items.Distinct(flag ? comparer : null);
        items.Distinct(default(ValueComparer));
        items.{|RH1002:Distinct|}(null);
        items.{|RH1002:Distinct|}(default);
        items.{|RH1002:Distinct|}(EqualityComparer<S>.Default);
        items.{|RH1002:Distinct|}(((IEqualityComparer<S>)null)!);
        items.{|RH1002:Distinct|}(flag ? null : EqualityComparer<S>.Default);
        items.{|RH1002:Distinct|}(true ? null : comparer);
        items.{|RH1002:Distinct|}((IEqualityComparer<S>)null ?? EqualityComparer<S>.Default);
        items.{|RH1002:Distinct|}(default(ValueComparer?));
    }
}
```
