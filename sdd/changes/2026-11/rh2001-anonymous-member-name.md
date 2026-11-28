# RH2001 keeps the name of an anonymous-object member

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH2001-004, RH2001-011, RH2001-012 (added); RH2001 now uses SHARED-CASING-015 |
| Depends on | [RH4105 checks the casing of query range variables, and casing fixes keep anonymous object member names](range-variable-casing.md) |

## What

The RH2001 code fix is offered where a reference to the property gives an anonymous-object member its inferred name, such as `new { Value }`, `new { this.Value }`, or `new { other?.Value }`. It keeps the member's name by writing it explicitly, as every casing fix does (SHARED-CASING-015): `new { Value }` becomes `new { Value = _value }` while the property becomes the field `_value` (RH2001-012). RH2001-011 no longer withholds the fix for such a member; its rule for a tuple element is unchanged. RH2001-004 points at RH2001-012 for the member.

Every other reason that withholds the fix still does, also in the same rename: a tuple element in a withheld position next to the member keeps the fix withheld.

## Why

The maintainer decided the issue's output question for option B: the fix is offered and the member keeps its name.

It also resolves a conflict between two specs. SHARED-CASING-015 states that every code fix that renames a reported name as SHARED-CASING-007 states writes such a member's name explicitly. RH2001-004 renames the property as SHARED-CASING-007 states, so SHARED-CASING-015, read literally, covered RH2001, while RH2001-011 withheld the fix for the same member. RH2001 now uses SHARED-CASING-015, so both specs say the same thing and the shared text needs no exception.

Withholding was needed only because the rename alone would rename the member, its reads, and the text `ToString()` returns (SHARED-FIX-003). Writing the old name out keeps the member's name and type, so the rewrite is safe, and projecting a property into an anonymous object by its bare name is a common shape.

## Visible output changes

**1. A member named by the bare reference.**

```csharp
// input
class C
{
    private int Value { get; set; }

    object M() => new { Value };
}
```

```csharp
// before: RH2001 is reported on Value; no fix offered.
```

```csharp
// after
class C
{
    private int _value;

    object M() => new { Value = _value };
}
```

**2. A member access and a conditional access, read by their names.**

```csharp
// input
class Sample
{
    private int Value { get; set; }

    int Get(Sample other)
    {
        var item = new { this.Value, Other = other?.Value };
        var copy = new { other?.Value };

        return item.Value + (item.Other ?? 0) + (copy.Value ?? 0);
    }
}
```

```csharp
// before: RH2001 is reported on Value; no fix offered.
```

```csharp
// after
class Sample
{
    private int _value;

    int Get(Sample other)
    {
        var item = new { Value = this._value, Other = other?._value };
        var copy = new { Value = other?._value };

        return item.Value + (item.Other ?? 0) + (copy.Value ?? 0);
    }
}
```

**3. Fix All over two properties whose names `ToString()` prints.**

```csharp
// input
class Sample
{
    private int Value { get; set; }

    private string Name { get; }

    string Describe() => new { Value, Name }.ToString();
}
```

```csharp
// before: RH2001 is reported on Value and Name; no fix offered.
```

```csharp
// after
class Sample
{
    private int _value;

    private readonly string _name;

    string Describe() => new { Value = _value, Name = _name }.ToString();
}
```

**4. A reference the renamer qualifies with `this.`, next to a local named like the field.**

```csharp
// input
class Sample
{
    private int Value { get; set; }

    int Get()
    {
        var _value = 1;
        var item = new { Value };

        return item.Value + _value;
    }
}
```

```csharp
// before: RH2001 is reported on Value; no fix offered.
```

```csharp
// after
class Sample
{
    private int _value;

    int Get()
    {
        var _value = 1;
        var item = new { Value = this._value };

        return item.Value + _value;
    }
}
```

Unchanged: a member whose name the compiler takes from another identifier, such as `new { Value.ToString().Length }`, is only renamed, as on `main`; a tuple element that a reference names keeps or withholds the fix as RH2001-011 states, also next to an anonymous-object member in the same rename. The rule page `documentation/rules/RH2001.md` does not change: what is reported and how to fix it stay the same in the normal case.

No formatter output changes. RH2001's is the only code fix whose result changes.

## Decisions

### Offer the fix and write the member's name out

Maintainer, 2026-11-28 (issue comment, option B). The fix is offered, and the member keeps its name explicitly, the same output SHARED-CASING-015 gives every casing fix.

### Reference SHARED-CASING-015 instead of restating it

RH2001-012 states only what is specific to RH2001 — that the member no longer withholds the fix, that the other withholding reasons still do, and Fix All — and takes which members get their name from a reference, and how the name is written, from SHARED-CASING-015. One statement of the rule keeps RH2001 and the casing fixes from drifting apart again. `CasingAnonymousMemberUtilities`, which implements SHARED-CASING-015, is listed as a code owner of the casing spec.

### Tuple elements stay as they are

A tuple element that a reference to the property names keeps the positions RH2001-011 states; writing its name out, such as `(Value: _value, 1)`, is not part of this change (RH2001 non-goal).

## Alternatives considered

- **Keep withholding the fix (option A)** — rejected by the maintainer: it leaves a diagnostic without an action for an input the fix can rewrite safely, and keeps RH2001 at odds with SHARED-CASING-015.
- **Exempt RH2001 in SHARED-CASING-015's scope sentence** — rejected: it would keep the conflict as a documented exception for no reason other than today's code.
- **Write tuple element names out as well** — rejected: outside the issue, and RH2001-011's tuple positions are a separate decision.
