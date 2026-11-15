# RH2001 withholds its fix when the field name is already bound at a reference

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH2001-004 (changed), RH2001-011 (added) |

## What

- **A rewritten reference to another symbol withholds the fix** (RH2001-011). When a symbol other than the property already answers to the derived `_camelCase` field name, such as an inherited field, an extension method, a statically imported member, or a member of an enclosing type, and the rename would rewrite a reference to that symbol to keep its binding (`base._value`, `Extensions._value(this)` for a property whose type is a delegate, `Holder._value`, `Outer._value`), the fix is no longer offered. The diagnostic stays.
- **A reference left as written that would bind to the field withholds the fix** (RH2001-011), as SHARED-FIX-003 already demands: for example `this._value` or `other._value` that reads an inherited field today.
- **A renamed reference to the property that would bind to another symbol withholds the fix** (RH2001-011, under SHARED-FIX-003), where the renamer leaves the reference unqualified: for example a parameter of a lambda or a query range variable in a statement of a block body, a variable declared by `out` in the same expression, or a primary-constructor parameter named like the field.
- **The rename itself is unchanged.** A reference to the property that the renamer qualifies with `this.` or the type next to a local or parameter named like the field, also a lambda parameter in an expression body, an inherited method or an extension method still reached by its call written as before, and an inherited member that no reference names keep the fix (RH2001-011, third paragraph). RH2001-004's coverage now points to RH2001-011.

RH2001 uses the binding paragraphs of SHARED-CASING-009 — the "rewritten" and "left as written" cases for a reference to a symbol outside the rename cascade — and no other paragraph of that requirement.

## Why

The RH2001 fix renames the property with the Roslyn renamer (RH2001-004, SHARED-CASING-007) and applies whatever the renamer returns. Its only check for the field name was RH2001-006: a member the containing type declares itself. A field name that an inherited member, an extension method, a statically imported member, a member of an enclosing type, or a symbol of an inner scope already uses was not checked. The renamer then either rewrites a reference the diagnostic does not name, or leaves a reference that silently reads or writes other state at run time. The casing fixes, such as RH4106 on the same input with a field `Value`, already withhold the fix for both (SHARED-CASING-009); RH2001 now follows the same rule.

The run-time cases are bug fixes against the unchanged SHARED-FIX-003, which RH2001 already uses; RH2001-011 writes them down so that the requirement names the whole class. Only the rewritten cases change intended behavior, and only they are listed below.

## Visible output changes

Code-fix output measured with `scripts/apply-fix.sh RH2001` on `main` at `962d0c4` (`sdd/work/rh2001-rename-safety/05-test-implementer.md`); the inherited-field, statically imported, and enclosing-type rows were first measured at `52ae88c` (`sdd/work/rh2001-rename-safety/01-skeptic.md`).

An inherited field read unqualified:

```csharp
class Base
{
    protected int _value;
}

class Sample : Base
{
    private int Value { get; set; }

    int Get()
    {
        return Value + _value;
    }
}
```

Before:

```csharp
class Sample : Base
{
    private int _value;

    int Get()
    {
        return _value + base._value;
    }
}
```

After: no code fix is offered; RH2001 is still reported on `Value`.

An extension method named like the field of a property whose type is a delegate:

```csharp
using System;

static class Extensions
{
    public static int _value(this Sample sample) => 1;
}

class Sample
{
    private Func<int> Value { get; set; } = () => 2;

    int Get()
    {
        return Value() + this._value();
    }
}
```

Before:

```csharp
class Sample
{
    private Func<int> _value = () => 2;

    int Get()
    {
        return _value() + Extensions._value(this);
    }
}
```

After: no code fix is offered; RH2001 is still reported on `Value`.

With an `int` property, the same call is left as written and still reaches the extension method (`return _value + this._value();`), so that fix stays offered, unchanged from `main`.

A statically imported member named like the field (`using static Holder;`, `Holder` declaring `public static int _value`), read unqualified next to `Value`:

```csharp
return Value + _value;
```

Before:

```csharp
return _value + Holder._value;
```

After: no code fix is offered; RH2001 is still reported on `Value`.

A static member `_value` of the enclosing type `Outer`, read from the nested type that declares `Value`:

```csharp
return Value + _value;
```

Before:

```csharp
return _value + Outer._value;
```

After: no code fix is offered; RH2001 is still reported on `Value`.

## Decisions

### The fix is withheld for a rewritten reference to another symbol

Maintainer's answer to output question 1 of the run: **B** — withhold the fix; the diagnostic stays. The same answer covers the rare rewrite variants: the extension method, the statically imported member, and the member of an enclosing type.

### RH2001 refers to SHARED-CASING-009 for the binding cases only

The two binding cases of SHARED-CASING-009 state exactly the class in question, so RH2001-011 refers to them instead of restating them. The other paragraphs of SHARED-CASING-009 are not adopted: the compile-error paragraph is SHARED-FIX-003, which RH2001 already uses; a private property's cascade holds no member that the renamer rewrites beyond renaming and qualifying; and the interface-member paragraph would withhold the fix for an interface member named `_value`, an output change nobody asked about.

### A reference qualified with `this.` keeps the fix

The renamer qualifies a reference to the property with `this.` where a local, a parameter, or another inner-scope symbol is named like the field. That keeps the reference's binding and changes it only by the rename, as SHARED-CASING-009 allows, so the fix stays offered, as it is on `main`.

## Alternatives considered

- **Withhold the fix for every lambda parameter of an inferred type, or every extension method, named like the field** — rejected: on `main` the renamer qualifies the property reference next to a lambda in an expression body, and leaves an extension call on an `int` property bound as before; both fixes are correct, and withholding them would be a visible output change nobody asked about. The refusal follows the renamer's result for each reference.

- **Keep the fix and ship the renamer's rewrite** (option A of the output question) — rejected by the maintainer: the fix would edit a reference the diagnostic does not name, and it would differ from the casing fixes' rule.
- **Withhold the fix whenever any symbol visible in the property's scope is named like the field** — rejected: it would also withhold the `this.`-qualified cases that are correct today, a visible output change nobody asked about.
- **Repair the renamer's result by adding the missing `this.` or `base.` qualifier** — rejected: machinery for a rare class (`AGENTS.md`, *Special cases do not grow the source*), and a `base.` qualifier is the rewrite the maintainer declined.
