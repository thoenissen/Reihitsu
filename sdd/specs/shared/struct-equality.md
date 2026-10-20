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

This requirement covers a struct that implements `IEquatable<T>` of itself only when the struct also overrides `GetHashCode()`. For a generic struct that has its own equality members, it covers only one none of whose type arguments is a struct that lacks equality members.

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

A rule MUST treat an operation as not using the compared type's equality members when one of its arguments, positional or named, is passed to a parameter of type `IEqualityComparer<T>` and is a custom comparer. An argument MUST count as not custom exactly when its expression has one of these forms, each of which can only produce `null` or `EqualityComparer<T>.Default`:

- `null`, or `default` of a reference type or of a nullable value type;
- `EqualityComparer<T>.Default`;
- one of these forms inside parentheses, behind the null-forgiving operator `!`, or converted by a cast that is not a user-defined conversion;
- a `??` expression whose two operands have one of these forms, or whose left operand has one of these forms and always produces `EqualityComparer<T>.Default`, so that its right operand is never used;
- a conditional expression whose two branches have one of these forms, or, when its condition is a constant, whose chosen branch has one;
- a `switch` expression whose arms all have one of these forms;
- a null-conditional access whose receiver is `null`.

Every other argument MUST count as custom. The rule decides by the form of the expression alone: a variable, a field, a property, or a method call counts as custom whatever value it has at run time, also a local variable initialized with `null`, and so does a cast with a user-defined conversion, whatever its operand. `default` of a non-nullable value type that implements `IEqualityComparer<T>` is a comparer and MUST count as a custom one.

This requirement covers a parameter of type `IEqualityComparer<T>` only when `T` is the type the operation compares. It covers a conditional expression with a `throw` branch, and a `switch` expression with a `throw` arm, only when another branch or arm counts as custom.

Why: A comparer passed to the operation does the comparing and hashing itself, so the type's own members are never called. `null` and `EqualityComparer<T>.Default` both fall back to the type's own equality behavior, so passing them changes nothing, however the expression is written. The rule does not follow a variable or a call to the value it produces, and a conversion operator can return any comparer, so such an argument may be a real comparer. A value-type comparer is boxed into a real instance and is used.

Example — RH1002:

```csharp
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
        items.{|RH1002:Distinct|}(null);
        items.{|RH1002:Distinct|}(default);
        items.{|RH1002:Distinct|}(EqualityComparer<S>.Default);
        items.{|RH1002:Distinct|}(((IEqualityComparer<S>)null)!);
        items.{|RH1002:Distinct|}(flag ? null : EqualityComparer<S>.Default);
        items.{|RH1002:Distinct|}(true ? null : comparer);
        items.{|RH1002:Distinct|}((IEqualityComparer<S>)null ?? EqualityComparer<S>.Default);
        items.{|RH1002:Distinct|}(EqualityComparer<S>.Default ?? comparer);
        items.{|RH1002:Distinct|}(mode switch { 0 => null, _ => EqualityComparer<S>.Default });
        items.{|RH1002:Distinct|}(((Holder)null)?.Comparer);
        items.{|RH1002:Distinct|}(default(ValueComparer?));
    }
}
```
