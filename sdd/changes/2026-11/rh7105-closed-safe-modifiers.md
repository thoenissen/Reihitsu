# RH7105 orders the C# 15 `closed` and the preview `safe` modifiers

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH7105-001, RH7105-002, RH7105-004 |

## What

RH7105-001's canonical order names two new modifiers. `closed` (C# 15, closed class hierarchies) stands in the position of `sealed`, between `virtual` and `override`, so it stands behind the accessibility and `static` and in front of `new`, `unsafe`, and `partial`. `safe` (preview, the updated memory-safety rules) stands in the position of `unsafe`, behind `extern` and in front of `required`, `ref`, and `partial`. Before this change both keywords counted as "every other modifier" and belonged behind `partial`.

## Why

The order list put every modifier it did not name behind `ref` and `partial`. For `closed` and `safe`, which C# allows together with `partial` (and `safe` with `ref`), that order cannot be written: `public partial closed class`, `public partial safe void M();`, and `public ref safe struct` do not compile. RH7105 therefore reported `public closed partial class`, which SHARED-DIAG-011 forbids, and its fix produced code that does not compile, which SHARED-FIX-003 forbids. Its own `Why:`, the rule page, and the remark on the rank table all state that `ref` and `partial` come last because C# requires it; only the order list disagreed.

`closed` is an inheritance modifier that C# does not allow together with `abstract`, `sealed`, or `static`; it replaces them on a type, so it stands where they stand, as `public sealed new class` and `public abstract unsafe class` read today. `safe` and `unsafe` are alternatives that C# does not allow on one declaration; a declaration that switches from one to the other keeps its order.

## Visible output changes

Diagnostics and code-fix results of RH7105. Every other order is unchanged.

1 · `closed`

Input:

```csharp
public closed partial class A
{
    public new closed partial class N
    {
    }
}

internal closed unsafe partial class U
{
}
```

Before: `closed` is reported on `A`, `N`, and `U`, and the fix writes the following, none of which compiles:

```csharp
public partial closed class A
{
    public new partial closed class N
    {
    }
}

internal unsafe partial closed class U
{
}
```

After: `A` and `U` are not reported; `N` is reported on `new`, and the fix writes:

```csharp
public closed partial class A
{
    public closed new partial class N
    {
    }
}

internal closed unsafe partial class U
{
}
```

2 · `safe` (compiled with `LangVersion preview`)

Input:

```csharp
public partial class C
{
    public static safe extern void M();

    public safe partial void P();

    public safe partial void P()
    {
    }

    public safe required int R { get; set; }
}

public safe ref struct S
{
}
```

Before: `safe` is reported on `M`, on both parts of `P`, on `R`, and on `S`, and the fix writes:

```csharp
public partial class C
{
    public static extern safe void M();

    public partial safe void P();

    public partial safe void P()
    {
    }

    public required safe int R { get; set; }
}

public ref safe struct S
{
}
```

The two parts of `P` (CS0267) and `S` (CS1585) do not compile.

After: `P`, `R`, and `S` are not reported; `M` is reported on `safe`, and the fix writes:

```csharp
public partial class C
{
    public static extern safe void M();

    public safe partial void P();

    public safe partial void P()
    {
    }

    public safe required int R { get; set; }
}

public safe ref struct S
{
}
```

3 · Rule page `documentation/rules/RH7105.md`: the order under *How to fix it* names `closed` next to `sealed` and `safe` next to `unsafe`.

Before:

`file` / `public` / `private` / `protected` / `internal` → `static` → `abstract` → `virtual` → `sealed` → `override` → `new` → `readonly` → `volatile` → `async` → `extern` → `unsafe` → `required` → `ref` → `partial`

After:

`file` / `public` / `private` / `protected` / `internal` → `static` → `abstract` → `virtual` → `sealed` or `closed` → `override` → `new` → `readonly` → `volatile` → `async` → `extern` → `unsafe` or `safe` → `required` → `ref` → `partial`

## Decisions

### `closed` stands in the position of `sealed`

Maintainer answer to output question 1 of the run (option B): `closed` takes the place of `abstract`/`sealed`. `public closed partial class A` and `internal closed unsafe partial class U` are not reported; `public new closed partial class N` is reported on `new` and fixed to `public closed new partial class N`.

### `safe` stands in the position of `unsafe`

Maintainer answer to output question 2 of the run (option B): `safe` takes the place of `unsafe`, behind `extern` and in front of `required`. `public safe partial void P();`, `public safe required int R`, and `public safe ref struct S` are not reported; `public static safe extern void M();` is reported and fixed to `public static extern safe void M();`.

### The order of each pair among itself stays undecided

C# allows neither `closed` with `sealed` nor `safe` with `unsafe` on one declaration, so the spec does not decide their order relative to each other; such code does not compile whatever the order.

## Alternatives considered

- **Reported as today, no fix offered** — rejected: SHARED-DIAG-011 forbids reporting an order that cannot be written at the position.
- **Every modifier the order does not name stands directly in front of `ref` and `partial`** (`internal unsafe closed partial class U`; `public required safe int R` stays reported) — rejected by the maintainer's answers in favour of a place that matches each keyword's kind; a later unnamed modifier that combines with `ref` or `partial` needs its own decision.
- **`safe` directly in front of `extern`** (`public static safe extern void M();` not reported) — rejected by the maintainer's answer to question 2.
