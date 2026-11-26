# Trailing comma at the end of a conditional branch

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-TRAIL-002, SHARED-TRAIL-004 (new), SHARED-DIAG-008, FMT-STRUCT-019, FMT-STRUCT-020 |

## What

The RH5409, RH5410, and RH5411 code fix, and the formatter's trailing-comma removal, no longer keep every trailing comma that stands in front of a conditional directive. A comma is kept, and its fix withheld, only when an `#if` directive lies between the comma and the closing brace — one the analyzed build reads, or one in code another directive excludes. When the only conditional directives in that gap are `#elif`, `#else`, and `#endif`, the fix is offered and removes the comma, and the formatter removes it too; every directive, its line, and the code the directives exclude stay as written (SHARED-TRAIL-004). The diagnostic itself does not change: every such comma was reported before and is reported now (SHARED-TRAIL-002).

## Why

A comma in front of an `#elif`, `#else`, or `#endif`, with no `#if` behind it, stands at the end of the branch that holds it. A build that compiles the comma takes that branch, skips every branch an `#elif` or `#else` starts behind it, and behind an `#endif` reads on in the branch of the enclosing group that holds the comma, where — as in the analyzed build — nothing but trivia and directives stands in front of the closing brace. So no build that compiles the comma compiles an element behind it, and removing the comma keeps every build intact. The check reads only the kinds of the directives in the gap, never their conditions (SHARED-FIX-009), so it is a cheap syntactic guard (`AGENTS.md`, *Special cases do not grow the source*). Before this change the fix was withheld for these commas although it was safe, and the formatter kept them.

## Visible output changes

The rule pages do not change: a fix that is withheld in a corner case is not listed on them, and `documentation/rules/general-notes.md` already covers diagnostics without a fix.

### RH5409 · Final enum members must not have trailing commas — comma in front of `#endif`

Input, analyzed in a build that defines `DEBUG`:

```csharp
enum E
{
    A,
#if DEBUG
    B,
#endif
}
```

Before: the comma behind `B` is reported, no fix offered.

After: the comma behind `B` is reported, and the fix removes it:

```csharp
enum E
{
    A,
#if DEBUG
    B
#endif
}
```

In a build without `DEBUG`, the comma behind `A` stands in front of the `#if`: it is reported with no fix, before and after.

### RH5409 · Final enum members must not have trailing commas — comma in front of `#else` or `#elif`

Input, analyzed in a build that does not define `X`:

```csharp
enum E
{
    A,
#if !X
    B,
#elif Y
    C,
#endif
}

enum F
{
    A,
#if X
    B,
#else
    C,
#endif
}
```

Before: the commas behind `B` in `E` and behind `C` in `F` are reported, no fix offered.

After: both are reported, the fix removes each, and Fix All removes both:

```csharp
enum E
{
    A,
#if !X
    B
#elif Y
    C,
#endif
}

enum F
{
    A,
#if X
    B,
#else
    C
#endif
}
```

### RH5409 — no change where an `#if` stands behind the comma

Input, analyzed in a build that defines neither `X` nor `Y`:

```csharp
enum E
{
    A,
#if !X
    B,
#endif
#if Y
    C,
#endif
}

enum F
{
    A,
#if !X
    B,
#else
#if Y
    C,
#endif
#endif
}
```

Before and after: the commas behind `B` are reported, no fix offered. In `F`, the `#if Y` stands in code the `#else` excludes and still withholds the fix.

### RH5410 · Final array initializer items must not have trailing commas, and the formatter

Input, analyzed and formatted in a build that does not define `X`:

```csharp
class C
{
    private int[] _numbers = new[]
                             {
                                 1,
#if !X
                                 2,
#endif
                             };
}
```

Before: the comma behind `2` is reported, no fix offered; `reihitsu-format` keeps it.

After: the fix removes it, and so does `reihitsu-format`:

```csharp
class C
{
    private int[] _numbers = new[]
                             {
                                 1,
#if !X
                                 2
#endif
                             };
}
```

### RH5411 · Final collection initializer items must not have trailing commas, and the formatter

Input, analyzed and formatted in a build that does not define `X`:

```csharp
using System.Collections.Generic;

class C
{
    private List<int> _numbers = new List<int>
                                 {
                                     1,
#if X
                                     2,
#else
                                     3,
#endif
                                 };
}
```

Before: the comma behind `3` is reported, no fix offered; `reihitsu-format` keeps it.

After: the fix removes it, and so does `reihitsu-format`; `2,` is code the `#if` excludes and stays as written:

```csharp
using System.Collections.Generic;

class C
{
    private List<int> _numbers = new List<int>
                                 {
                                     1,
#if X
                                     2,
#else
                                     3
#endif
                                 };
}
```

### The formatter on an enum

Input, formatted in a build that does not define `DEBUG` (the build `reihitsu-format` formats, which defines no symbol):

```csharp
enum Level
{
    Low,
#if DEBUG
    Trace,
#else
    High,
#endif
}
```

Before: unchanged.

After:

```csharp
enum Level
{
    Low,
#if DEBUG
    Trace,
#else
    High
#endif
}
```

`reihitsu-format` formats with no symbol defined, so the comma in `#if DEBUG ⏎ B, ⏎ #endif` is code the `#if` excludes there and stays; the formatter removes such a comma only in a build that compiles it, as a code fix in a project that defines `DEBUG` does.

### Code fixes that format an initializer

RH5303 (collection initializer should be formatted correctly) formats the whole object creation through the formatter, and so now also removes such a comma. Input, in a build that does not define `X`:

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        var list = new List<int> { 1,
#if X
            2,
#else
            3,
#endif
        };
    }
}
```

Before:

```csharp
using System.Collections.Generic;

class C
{
    void M()
    {
        var list = new List<int>
                   {
                       1,
#if X
            2,
#else
                       3,
#endif
                   };
    }
}
```

After: the same, with `3` in place of `3,`.

RH5301 and RH5304 format an object creation that can hold a collection or array initializer and change in the same way when they format one with such a comma.

## Decisions

### Only an `#if` in the gap withholds the fix

The fix and the formatter decide from the kinds of the directives between the comma and the closing brace: an `#if` withholds, `#elif`, `#else`, and `#endif` do not. An `#if` that stands in code another directive excludes withholds too, although a build that compiles the comma never reads it: telling it apart would mean pairing it with the directives around it, which is more than a cheap check, for a shape that is rare.

### The diagnostic does not change

Every comma this change makes fixable was already reported (SHARED-TRAIL-002, SHARED-DIAG-008); only the fix and the formatter change.

### Maintainer answer

Pending: issue #1837 asks the maintainer to choose between offering the fix (this change) and leaving the behavior as it is.

## Alternatives considered

- **Leave every comma in front of a conditional directive unfixed** — the behavior before this change, the issue's option B. It costs nothing, but withholds a fix that is safe in every build.
- **Let an `#if` in excluded code not withhold the fix** — rejected: it would need the directives paired across the gap, not a cheap check of their kinds.
- **Evaluate the conditions of the directives** — rejected: SHARED-FIX-009 forbids working out another build configuration, and the kinds alone already decide every case this change makes fixable.
