# Shared — Grouping members by region

| | |
|---|---|
| ID prefix | SHARED-REGGROUP |
| Status | Active |
| Code owners | `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`, `OverrideMemberUtilities`, `InterfaceImplementationUtilities` |
| Uses | SHARED-DIAG-006, SHARED-DIAG-009, SHARED-REGDESC-001, SHARED-TYPEREGION-006 |

This spec covers what the rules that group members by the type they come from share: which type declarations they check, how a region's name is matched against a type, which type an override comes from (RH7401–RH7408), which interface an implementation comes from (RH7409–RH7412), when a region is an override region (RH7405–RH7408), and where the diagnostic is. Which members a rule checks, and whether it asks for a region or forbids one, is each rule's own spec. Which top-level regions a type declaration has, when a member lies in one, and which region judges a member that lies in a region nested in one, is SHARED-TYPEREGION-006.

## Non-goals

- Whether a type has regions at all — RH7305, RH7305A — and which member kinds a region named after a member kind may hold — RH7308.
- The order of the regions — RH7309 — and whether two regions share a name — RH7311.
- A code fix. Where a member should move, and whether the region it needs exists, is the user's decision, so none of these rules offers one.

## Requirements

### SHARED-REGGROUP-001 — Class, struct, and record declarations are checked

A rule MUST check every class, struct, record, and record struct declaration, nested ones included, and each declaration of a `partial` type on its own, against that declaration's own top-level regions and the regions nested in them (SHARED-TYPEREGION-006). A rule MUST judge a member that lies in one of them by the innermost region that encloses it, as SHARED-TYPEREGION-006 states: the top-level region, or the innermost region nested in it that encloses the member. It MUST NOT check an interface declaration.

Why: Overrides and interface implementations are members of the types that inherit them, and a nested type has an outline of its own, so a region around the nested type groups the nested type, not its members. Each declaration of a partial type is what the user reads in one file. An interface declares a contract; the outline of its body is not grouped by where its members come from.

Example — RH7401, where the nested type `N` has no region of its own and the second declaration of `D` has none either:

```csharp
class B
{
    public virtual void M()
    {
    }

    public virtual void N()
    {
    }
}

class O
{
    #region B

    class N : B
    {
        public override void {|RH7401:M|}()
        {
        }
    }

    #endregion
}

partial class D : B
{
    #region B

    public override void M()
    {
    }

    #endregion
}

partial class D
{
    public override void {|RH7401:N|}()
    {
    }
}
```

and an interface declaration is not checked, so RH7409 does not report the explicit implementation in `IB`:

```csharp
interface IA
{
    void M();
}

interface IB : IA
{
    void IA.M()
    {
    }
}
```

is not reported.

Example — RH7401 judges a member in a nested region by the innermost region, so `M`, which lies in `Methods` inside `B`, is reported, and `N`, which lies in `B` inside `Overrides`, is not:

```csharp
class B
{
    public virtual void M()
    {
    }

    public virtual void N()
    {
    }
}

class D : B
{
    #region B

    #region Methods

    public override void {|RH7401:M|}()
    {
    }

    #endregion

    #endregion

    #region Overrides

    #region B

    public override void N()
    {
    }

    #endregion

    #endregion
}
```

### SHARED-REGGROUP-002 — A region name matches the type's simple name

For a type whose simple name does not start with a character that changes under invariant upper-casing, a region MUST match the type exactly when the region's name — its description (SHARED-REGDESC-001) — equals the type's simple name, compared ordinally: the name's value without a verbatim `@`, without its namespace or containing type, and without a type parameter or type argument list. A name with different casing or with any further text MUST NOT match.

Why: One spelling per type keeps the regions of every type in a code base alike, and the simple name is the same for every construction of a generic type, so the members of `B<int>` and `B<string>` group under the same name. The `@` is spelling, not part of the name. Comparing exactly keeps the region's name the type's name instead of a text that only resembles it.

Example — RH7401, with the base type `B<T>`:

```csharp
class B<T>
{
    public virtual void M()
    {
    }

    public virtual void N()
    {
    }

    public virtual void O()
    {
    }

    public virtual void P()
    {
    }
}

class C : B<int>
{
    #region   B

    public override void M()
    {
    }

    #endregion

    #region B<T>

    public override void {|RH7401:N|}()
    {
    }

    #endregion

    #region b

    public override void {|RH7401:O|}()
    {
    }

    #endregion

    #region B // base

    public override void {|RH7401:P|}()
    {
    }

    #endregion
}
```

The description of the first region is `B`, whatever whitespace surrounds it; `B<T>`, `b`, and `B // base` do not match. A type declared with a verbatim identifier matches its value — RH7401:

```csharp
class @Base
{
    public virtual void M()
    {
    }

    public virtual void N()
    {
    }
}

class C : @Base
{
    #region Base

    public override void M()
    {
    }

    #endregion

    #region @Base

    public override void {|RH7401:N|}()
    {
    }

    #endregion
}
```

The same holds for an interface — RH7409:

```csharp
using System;

class C : IComparable<C>
{
    #region IComparable<C>

    int IComparable<C>.{|RH7409:CompareTo|}(C other) => 0;

    #endregion
}
```

### SHARED-REGGROUP-003 — An override comes from the type that introduced the member

The type an override comes from MUST be the type that declares the member its override chain starts from — the `virtual` or `abstract` member that the first override in the chain overrides — and not the direct base type that overrode it last. This holds for methods, properties, indexers, and events, and for `override`, `sealed override`, and `abstract override` alike. An override of a member that `object` declares MUST come from `Object`, also in a struct, whose `ToString`, `Equals`, and `GetHashCode` override members that `ValueType` overrides in turn.

Why: The type that introduced a member defines its contract. Grouping by that type keeps every override of one member under one name across a hierarchy, however many levels override it again.

Example — RH7401:

```csharp
class B
{
    public virtual void M()
    {
    }
}

class D : B
{
    #region B

    public override void M()
    {
    }

    #endregion

    public virtual void N()
    {
    }
}

class E : D
{
    #region D

    public override void {|RH7401:M|}()
    {
    }

    public override void N()
    {
    }

    #endregion
}

struct S
{
    #region ValueType

    public override string {|RH7401:ToString|}() => "";

    #endregion
}

class C
{
    #region Object

    public override string ToString() => "";

    #endregion
}
```

`E.M` comes from `B`, because `D.M` only overrides it again; `E.N` comes from `D`, which introduced `N`; `S.ToString` and `C.ToString` come from `Object`.

### SHARED-REGGROUP-004 — An implementation comes from the interface that declares the member

The interface an explicit interface implementation comes from MUST be the interface its name is qualified with. The interface an implicit implementation comes from MUST be the interface that declares the member it implements, also when the type lists only an interface derived from it. An implicit implementation that implements members of several interfaces MUST come from each of them, so a region whose name matches any one of them is a region of the member's interface (SHARED-DIAG-009), whatever the order of the interfaces in the base list. A member that is an override MUST NOT come from an interface, even when it implements an interface member; it comes from its base type (SHARED-REGGROUP-003).

Why: The interface that declares a member is the contract the member fulfils, and it is the name the member is implemented under, whichever derived interface the type lists. A member that fulfils the contracts of two interfaces is in its place under either name; the order of the base list is incidental. An override that also implements an interface member is grouped once, under the type it overrides, so that one member is never asked to lie in two regions.

Example — RH7409:

```csharp
interface IA
{
    void M();
}

interface IB : IA
{
    void N();
}

interface IC
{
    void O();
}

class C : IB, IC
{
    #region IB

    public void {|RH7409:M|}()
    {
    }

    public void N()
    {
    }

    #endregion

    #region IC

    void IC.O()
    {
    }

    #endregion
}
```

`M` implements a member that `IA` declares, so it comes from `IA`, although `C` lists `IB`. An override that implements an interface member comes from its base type:

```csharp
interface IA
{
    void M();
}

abstract class B
{
    public abstract void M();
}

class D : B, IA
{
    #region B

    public override void M()
    {
    }

    #endregion
}
```

is not reported by RH7409. A member that implements a member of each of two interfaces comes from both; the example of SHARED-DIAG-009 shows it.

### SHARED-REGGROUP-005 — The diagnostic is on the member's name

A rule MUST report one diagnostic per reported member declaration, located on its identifier: for an explicit interface implementation the identifier after the interface name, and for an indexer the `this` keyword. A field-like event declaration declares one event per declarator: a rule MUST judge each declarator on its own and MUST report one diagnostic per reported declarator, located on that declarator's identifier (SHARED-DIAG-006).

Why: The name is what a reader scans for, and one diagnostic per member lets the user move members one at a time. Each declarator of a field-like event is an event of its own, which can override or implement another member than its neighbours, so its position among the declarators decides nothing.

Example — RH7410, RH7411, and RH7412:

```csharp
using System;

interface I
{
    int P { get; }

    int this[int i] { get; }

    event EventHandler E;
}

class C : I
{
    int I.{|RH7410:P|} => 0;

    public int {|RH7412:this|}[int i] => 0;

    public event EventHandler {|RH7411:E|};
}
```

Example — RH7411, where each declarator of one field-like event declaration is judged and reported on its own:

```csharp
using System;

interface IA
{
    event EventHandler A;
}

interface IB
{
    event EventHandler X;

    event EventHandler Y;

    event EventHandler Z;
}

class C : IA, IB
{
    #region IA

    public event EventHandler A, {|RH7411:X|};

    #endregion

    public event EventHandler {|RH7411:Y|}, {|RH7411:Z|};
}
```

`A` lies in the region of its interface and `X` does not, although both share one declaration; `Y` and `Z` lie in no region, so each has a diagnostic of its own.

### SHARED-REGGROUP-006 — An override region

A region of a type declaration — a top-level region or a region nested in one (SHARED-TYPEREGION-006) — MUST count as an override region when its name matches (SHARED-REGGROUP-002) the type that an override comes from (SHARED-REGGROUP-003), for an override of any member kind — a method, a property, an indexer, or an event, each declarator of a field-like event on its own — that any declaration of the type declares, wherever that override lies — in the region, in another region, in none, or in another declaration of a `partial` type. A region whose name matches no type that such an override comes from MUST NOT count as an override region. Which member kind a rule reports, and that the override making a region an override region is of another kind or in another part, MUST NOT change the verdict (SHARED-DIAG-009).

Why: A region named after a base type of the type's overrides is where those overrides belong, whether or not they have been moved there yet; a member that overrides nothing does not belong in it. A region that only happens to carry a base type's name, in a type that overrides nothing of it, is an ordinary region. Whether the override that makes it an override region is a method or a property, and which part of a `partial` type declares it, is incidental: the region groups the same base type's members either way.

Example — RH7405, where the override lies outside the region named after its base type:

```csharp
class B
{
    public virtual void M()
    {
    }
}

class C : B
{
    public override void M()
    {
    }

    #region B

    public void {|RH7405:N|}()
    {
    }

    #endregion
}
```

while a region named after a base type that the type overrides nothing of is not an override region:

```csharp
class B
{
}

class C : B
{
    #region B

    public void N()
    {
    }

    #endregion
}
```

is not reported.

Example — RH7405, where the override that makes `B` an override region is a property:

```csharp
class B
{
    public virtual int P => 0;
}

class C : B
{
    #region B

    public override int P => 1;

    public void {|RH7405:N|}()
    {
    }

    #endregion
}
```

Example — RH7405, where the override lies in another declaration of the `partial` type:

```csharp
class B
{
    public virtual void M()
    {
    }
}

partial class C : B
{
    public override void M()
    {
    }
}

partial class C
{
    #region B

    public void {|RH7405:N|}()
    {
    }

    #endregion
}
```
