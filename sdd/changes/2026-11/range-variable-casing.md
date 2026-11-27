# RH4105 checks the casing of query range variables, and casing fixes keep anonymous object member names

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH4105-002, RH4105-003, RH4105-005 (added), RH4105-006 (added), SHARED-CASING-015 (added); RH4115-003, RH4117-002, and RH4128-003 refer to SHARED-CASING-015 ([local-casing-rule-split.md](local-casing-rule-split.md)) |

## What

RH4105 reports a range variable of a query expression whose name is not in camelCase, like a lambda parameter (thoenissen/Reihitsu#1978). A range variable is the variable that a `from`, `let`, or `join` clause, or the `into` of a `join` clause or of a query continuation, declares with an identifier of its own. A range variable named `_` is not reported.

The rule's title and message widen from *Method parameter names should be in camelCase* to *Parameter and query range variable names should be in camelCase*.

The rule's code fix renames a reported range variable and every reference to it, as it renames a parameter.

Every casing fix — the fixes that rename a reported name with the shared casing rename, among them RH4105, RH4106, RH4115, RH4117, and the new RH4128 — keeps the name of an anonymous object member that the compiler takes from a renamed reference, by writing the old name explicitly: `values.Select(Value => new { Value })` becomes `values.Select(value => new { Value = value })`, and `from Value in values select new { Value }` becomes `from value in values select new { Value = value }` (SHARED-CASING-015).

## Why

A range variable names the same value that a lambda parameter names in method syntax: `from value in values select value` is `values.Select(value => value)`. RH4105 reported the lambda parameter but not the range variable, and no other casing rule checked it. RH4124 already checks range variables under the parameter rules for single-letter names (RH4124-003). The maintainer chose this option on 2026-11-27 (thoenissen/Reihitsu#1978, option A), with the fix offered only if it is comprehensive and safe for range variables.

The fix is comprehensive: the Roslyn renamer renames a range variable declared by each of the five clauses and every reference to it, also across a query continuation, and the result compiles. Running it on

```csharp
var a = from Value in values
        let Doubled = Value * 2
        join Row in rows on Value equals Row.Id into Matches
        select new { Value, Doubled, Count = Matches.Count() } into Projected
        where Projected.Count > 0
        select Projected;
```

renamed all five range variables and compiled. It also renamed the members `Value` and `Doubled` of the anonymous object to `value` and `doubled`, because the compiler names a member without a name of its own after the expression's identifier; that changes the text `ToString()` returns and the names a serializer or reflection sees, which SHARED-FIX-003 forbids.

The same rename already renamed such members on `main` for every casing fix that shares it: `scripts/apply-fix.sh` gives `new { value }` for a lambda parameter (RH4105), `new { total }` for a local (RH4115, also for a local constant), `new { first, second }` for a deconstruction variable (RH4117), `new { this._value }` for a private field (RH4106), `new { Color.Red }` for an enum member (RH4101), and `new { other.Value }` for a private property (RH4111). Projecting a value into an anonymous object by its bare name is a common shape, so the maintainer chose to keep the member's name rather than withhold the fix (batch 1, answer 3B), and the rule is stated once for every casing fix (SHARED-CASING-015).

## Visible output changes

### 1. RH4105 · Parameter and query range variable names should be in camelCase — range variables

```csharp
using System.Linq;

class C
{
    int[] M(int[] values, int[] others)
    {
        return (from Value in values
                let Doubled = Value * 2
                join int Other in others on Doubled equals Other into Matches
                select Matches.Count() into Count
                select Count).ToArray();
    }
}
```

Before: not reported.

After: RH4105 is reported on `Value`, `Doubled`, `Other`, `Matches`, and `Count`, and the fix gives:

```csharp
using System.Linq;

class C
{
    int[] M(int[] values, int[] others)
    {
        return (from value in values
                let doubled = value * 2
                join int other in others on doubled equals other into matches
                select matches.Count() into count
                select count).ToArray();
    }
}
```

### 2. RH4105 — a range variable that names an anonymous object member

```csharp
using System.Linq;

class C
{
    object M(int[] values)
    {
        return (from Value in values
                let Doubled = Value * 2
                select new { Value, Doubled }).First();
    }
}
```

Before: not reported.

After: RH4105 is reported on `Value` and `Doubled`, and the fix gives:

```csharp
using System.Linq;

class C
{
    object M(int[] values)
    {
        return (from value in values
                let doubled = value * 2
                select new { Value = value, Doubled = doubled }).First();
    }
}
```

### 3. RH4105 — a range variable named `_`

```csharp
using System.Linq;

class C
{
    int M(int[] values)
    {
        return (from _ in values
                select 1).Count();
    }
}
```

Before: not reported. After: not reported.

### 4. Every casing fix — an anonymous object member named after the renamed name

The diagnostics are unchanged; the fix result changes. Each input is reported on `main` as today.

RH4105 (parameter) and RH4115 (local variable):

```csharp
using System.Linq;

class C
{
    object M(int[] values)
    {
        var Total = values.Sum();

        return values.Select(Value => new { Value, Total }).First();
    }
}
```

Before: the fixes give `var total = values.Sum();` and `values.Select(value => new { value, total })`.

After:

```csharp
using System.Linq;

class C
{
    object M(int[] values)
    {
        var total = values.Sum();

        return values.Select(value => new { Value = value, Total = total }).First();
    }
}
```

RH4117 (deconstruction variable):

```csharp
class C
{
    string M((int, int) pair)
    {
        var (First, second) = pair;

        return new { First, second }.ToString();
    }
}
```

Before: `var (first, second) = pair;` and `new { first, second }`. After: `var (first, second) = pair;` and `new { First = first, second }`.

RH4128 (local constant; the rule is new, case 2 of [local-casing-rule-split.md](local-casing-rule-split.md)):

```csharp
class C
{
    string M()
    {
        const int MaxValue = 3;

        return new { MaxValue }.ToString();
    }
}
```

Before: RH4115's fix gives `const int maxValue = 3;` and `new { maxValue }`. After: RH4128's fix gives `const int maxValue = 3;` and `new { MaxValue = maxValue }`.

RH4106 (private field), through a member access, and with the member read by its name:

```csharp
class C
{
    private int Value;

    string M()
    {
        return new { this.Value }.ToString();
    }

    int N()
    {
        var item = new { Value };

        return item.Value;
    }
}
```

Before: in `M` alone, the fix gives `new { this._value }`; with `N`, whose `item.Value` would no longer compile, no code fix is offered.

After: the fix is offered and gives:

```csharp
class C
{
    private int _value;

    string M()
    {
        return new { Value = this._value }.ToString();
    }

    int N()
    {
        var item = new { Value = _value };

        return item.Value;
    }
}
```

The same holds for every other casing fix whose name can stand in an anonymous object member: fields (RH4107 to RH4110), properties (RH4111 to RH4114; `new { other.value }` becomes `new { value = other.Value }` instead of `new { other.Value }`), enum members (RH4101; `new { Color.red }` becomes `new { red = Color.Red }` instead of `new { Color.Red }`), events (RH4102), and record primary-constructor parameters (RH4120). A member with a name of its own (`new { Item = Value }`) and a member named after another identifier (`new { Value.Length }`) are renamed as before.

### 5. Title and message

Before: *Method parameter names should be in camelCase*, as the title and as the message of every RH4105 diagnostic.

After: *Parameter and query range variable names should be in camelCase*.

### 6. Rule page

`documentation/rules/RH4105.md` takes the new title and says that the rule covers the range variables of query expressions.

## Decisions

### Range variables are checked by RH4105

Maintainer, 2026-11-27 (thoenissen/Reihitsu#1978, option A). The clauses are the ones RH4124-003 names for single-letter range variables, so both rules check the same declarations.

### The title and message widen

Maintainer, 2026-11-27: the title names both kinds of declaration the rule checks. The rule keeps its ID, so existing suppressions keep working; the message keeps no placeholder and stays equal to the title.

### The fix is offered for range variables

The maintainer allowed the fix only if it is comprehensive and safe for range variables. It is comprehensive (see *Why*); the one unsafe result found, a renamed anonymous object member, is resolved for every casing fix by the next decision.

### Every casing fix keeps the name of an anonymous object member

Maintainer, 2026-11-27 (batch 1, answer 3B). When the compiler takes an anonymous object member's name from a renamed reference — the member's whole expression, or the name that ends a member access or conditional access that is its whole expression — the fix writes the old name explicitly (`Value = value`). It is stated once in SHARED-CASING-015, implemented in the shared casing fix, and RH4105-003, RH4105-006, RH4115-003, RH4117-002, and RH4128-003 refer to it. It repairs, for the existing casing fixes, a rename that SHARED-FIX-003 already forbade, and it reaches every casing fix because the cause, the compiler's inferred member name, does not depend on the kind of symbol renamed. A member read by its name, which withheld the fix before because the read no longer compiled (SHARED-CASING-009), now keeps the fix.

### The rule holds for every fix that uses the shared casing rename

Maintainer, 2026-11-27 (batch 1b, answer 4A). Answer 3B was asked with the fixes of RH4105, RH4106, RH4115, RH4117, and RH4128 as examples; SHARED-CASING-015 holds for every fix that renames a reported name with the shared casing rename, so the fixes of RH4101, RH4102, RH4107 to RH4114, and RH4120 keep an anonymous object member's name too (case 4): for RH4111, `private int value { get; set; }` with `new { value }` becomes `private int Value { get; set; }` with `new { value = Value }`. The cause, the compiler's inferred member name, does not depend on the kind of symbol renamed, and the shared casing fix implements the rule once.

## Alternatives considered

- **No fix for range variables** (batch 1, option 3C) — rejected by the maintainer: the rename is comprehensive, and the anonymous member keeps its name.
- **Withhold the fix where a renamed reference names an anonymous object member** (batch 1, option 3A) — rejected by the maintainer: projecting a value by its bare name is a common shape, also of query syntax, and keeping the name explicit is a cheap rewrite of the member that the fix already touches.
- **Keep the member's name only in the fixes of RH4105, RH4106, RH4115, RH4117, and RH4128** (batch 1b, the alternative to 4A) — rejected by the maintainer: the other casing fixes would keep renaming the member, which SHARED-FIX-003 forbids, and the requirement would need a list of rules instead of its cause.
- **A rule of its own for range variables** — rejected by the maintainer's choice of option A: a range variable plays the role of a lambda parameter, and RH4124 groups it with lambda parameters already.
- **Leave range variables unchecked and document the gap** (option B of thoenissen/Reihitsu#1978) — rejected by the maintainer.
