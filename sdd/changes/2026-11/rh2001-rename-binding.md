# RH2001 withholds its fix when the field name is already bound at a reference

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH2001-004 (changed), RH2001-011 (added) |

## What

- **A rewritten reference to another symbol withholds the fix** (RH2001-011). When a symbol other than the property already answers to the derived `_camelCase` field name, such as an inherited field, an extension method, a statically imported member, or a member of an enclosing type, and the rename would rewrite a reference to that symbol to keep its binding (`base._value`, `Extensions._value(this)` for a property whose type is a delegate, `Holder._value`, `Outer._value`), the fix is no longer offered. The diagnostic stays.
- **A reference left as written that would bind to the field withholds the fix** (RH2001-011), as SHARED-FIX-003 already demands: for example `this._value` or `other._value` that reads an inherited field today, or a `<see cref="_value"/>` in a documentation comment that names it.
- **A renamed reference to the property that would bind to another symbol withholds the fix** (RH2001-011, under SHARED-FIX-003), where the renamer leaves the reference unqualified: for example a parameter of a lambda or a query range variable in a statement of a block body, a variable declared by `out` in the same expression, or a primary-constructor parameter named like the field.
- **A reference to the property that gives an anonymous-object member or a tuple element an inferred name that is part of a type withholds the fix** (RH2001-011, under SHARED-FIX-003): for example `new { Value }`, `new { this.Value }`, or `var pair = (Value, 1);`. The rename would rename the member or element, so `pair.Value` no longer compiles, and `new { Value }.ToString()` returns `{ _value = 0 }` instead of `{ Value = 0 }`. A tuple drops its inferred names as the target or the source of a deconstruction, such as the deconstructing constructor `(Value, Name) = (value, name);` or `(value, count) = (Value, 1);`, and where it is converted to a type without the name, such as `return (Value, 1);` from a method that returns `(int, int)`; there the fix stays offered, unchanged from `main`. A member or element named explicitly, such as `new { Value = Value }`, keeps the fix.
- **Two rare shapes withhold the fix** (RH2001-011), although the renamer would keep every binding: a reference to the property through another construction of its generic containing type, such as `other.Value` with `Sample<int> other` inside `Sample<T>`, and a member named like the field reached through the property's own value whose receiver the renamer also qualifies, such as `Value._value` next to a local `_value`, which `main` rewrites to `this._value._value`. This is a refusal of a rare class (`AGENTS.md`, *Code-fix coverage*), not a fix against SHARED-FIX-003.
- **The rename itself is unchanged.** A reference to the property that the renamer qualifies with `this.` or the type next to a local or parameter named like the field, also a lambda parameter in an expression body, an inherited method or an extension method still reached by its call written as before, an inherited member that no reference names, and a member named like the field reached through the property's own value (`Value._value` becomes `_value._value`, where no symbol of an inner scope is named like the field) keep the fix (RH2001-011, fourth paragraph). RH2001-004's coverage now points to RH2001-011.

RH2001 uses the binding paragraphs of SHARED-CASING-009 — the "rewritten" and "left as written" cases for a reference to a symbol outside the rename cascade — and no other paragraph of that requirement.

## Why

The RH2001 fix renames the property with the Roslyn renamer (RH2001-004, SHARED-CASING-007) and applies whatever the renamer returns. Its only check for the field name was RH2001-006: a member the containing type declares itself. A field name that an inherited member, an extension method, a statically imported member, a member of an enclosing type, or a symbol of an inner scope already uses was not checked. The renamer then either rewrites a reference the diagnostic does not name, or leaves a reference that silently reads or writes other state at run time. The casing fixes, such as RH4106 on the same input with a field `Value`, already withhold the fix for both (SHARED-CASING-009); RH2001 now follows the same rule.

The run-time cases are bug fixes against the unchanged SHARED-FIX-003, which RH2001 already uses; RH2001-011 writes them down so that the requirement names the whole class. The inferred-name case is one of them: on `main` the fix renames an anonymous-object member, or an element of a tuple used in its natural type, named after the property, which ships code that does not compile or changes what `ToString()` returns, so withholding it is a fix against SHARED-FIX-003 and not a new decision about visible output. A tuple that is deconstructed, or converted to a type without the name, drops its inferred names; renaming its element changes no type, so SHARED-FIX-003 does not demand withholding there, and the fix stays offered as on `main`.

The two rare shapes are neither: `main`'s fix is correct for them. The fix tells the property's own rename apart from a change of another reference by the references' text and symbols, and in these shapes the property's own rename changes more than its name: a constructed symbol, or a receiver the renamer qualifies. Both are rare — a private property read through another construction of its own generic type, and a local named like a field, which RH4115 reports — so the fix is withheld for them rather than the comparison extended, and the diagnostic stays (`AGENTS.md`, *Code-fix coverage*).

Only the rewritten cases change intended behavior by a maintainer's answer; they are listed below, followed by the rare shapes.

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

A reference to the property through another construction of its generic containing type (measured at `0696846` and on `main` at `4838e9a`, `sdd/work/rh2001-rename-safety/16-auditor.md`):

```csharp
class Sample<T>
{
    private int Value { get; set; }

    int Get(Sample<int> other)
    {
        return other.Value + Value;
    }
}
```

Before:

```csharp
class Sample<T>
{
    private int _value;

    int Get(Sample<int> other)
    {
        return other._value + _value;
    }
}
```

After: no code fix is offered; RH2001 is still reported on `Value`. The same holds for `new Sample<TOther> { Value = Value }`, which `main` turns into `new Sample<TOther> { _value = _value }`.

A member named like the field reached through the property's own value, next to a local named like the field, in a type `Inner` nested in `Outer`, which declares `private int _value`:

```csharp
var _value = 1;

return Value._value + _value;
```

Before:

```csharp
var _value = 1;

return this._value._value + _value;
```

After: no code fix is offered; RH2001 is still reported on `Value`.

## Decisions

### The fix is withheld for a rewritten reference to another symbol

Maintainer's answer to output question 1 of the run: **B** — withhold the fix; the diagnostic stays. The same answer covers the rare rewrite variants: the extension method, the statically imported member, and the member of an enclosing type.

### RH2001 refers to SHARED-CASING-009 for the binding cases only

The two binding cases of SHARED-CASING-009 state exactly the class in question, so RH2001-011 refers to them instead of restating them. The other paragraphs of SHARED-CASING-009 are not adopted: the compile-error paragraph is SHARED-FIX-003, which RH2001 already uses; a private property's cascade holds no member that the renamer rewrites beyond renaming and qualifying; and the interface-member paragraph would withhold the fix for an interface member named `_value`, an output change nobody asked about.

### A reference qualified with `this.` keeps the fix

The renamer qualifies a reference to the property with `this.` where a local, a parameter, or another inner-scope symbol is named like the field. That keeps the reference's binding and changes it only by the rename, as SHARED-CASING-009 allows, so the fix stays offered, as it is on `main`.

### The rare shapes are refused, not compared more precisely

A reference through another construction of the generic type, and a receiver the renamer qualifies, are each a way in which the property's own rename changes more than its name. Earlier repairs of this run enumerated such ways one at a time; the remaining ones are rare, so the fix is withheld for them (`sdd/work/rh2001-rename-safety/00-lead.md`, decision 19).

## Alternatives considered

- **Withhold the fix for every lambda parameter of an inferred type, or every extension method, named like the field** — rejected: on `main` the renamer qualifies the property reference next to a lambda in an expression body, and leaves an extension call on an `int` property bound as before; both fixes are correct, and withholding them would be a visible output change nobody asked about. The refusal follows the renamer's result for each reference.

- **Keep the fix and ship the renamer's rewrite** (option A of the output question) — rejected by the maintainer: the fix would edit a reference the diagnostic does not name, and it would differ from the casing fixes' rule.
- **Withhold the fix whenever any symbol visible in the property's scope is named like the field** — rejected: it would also withhold the `this.`-qualified cases that are correct today, a visible output change nobody asked about.
- **Compare symbols by their original definition, as the speculative rebinding helper does** — rejected: it keeps the fix for the generic construction but leaves the qualified receiver, and the next way the property's own rename changes more than its name would need another repair; both shapes are rare.
- **Withhold the fix for every tuple element whose name is inferred from the property** — rejected: a deconstruction, such as the commonly written deconstructing constructor, and a tuple converted to a type without the name drop the inferred name, so `main`'s fix is correct there.
- **Repair the renamer's result by adding the missing `this.` or `base.` qualifier** — rejected: machinery for a rare class (`AGENTS.md`, *Special cases do not grow the source*), and a `base.` qualifier is the rewrite the maintainer declined.
