# Trailing comma at the end of a conditional branch

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-TRAIL-002, SHARED-TRAIL-004 (new), SHARED-DIAG-008, FMT-STRUCT-019 (clarified; behavior unchanged) |

## What

The RH5409, RH5410, and RH5411 code fix no longer withholds itself for every trailing comma that stands in front of a conditional directive. The fix is withheld only when an `#if` directive lies between the comma and the closing brace — one the analyzed build reads, or one in code another directive excludes. When the only conditional directives in that gap are `#elif`, `#else`, and `#endif`, the fix is offered and removes the comma; every directive, its line, and the code the directives exclude stay as written (SHARED-TRAIL-004). The diagnostic does not change: every such comma was reported before and is reported now (SHARED-TRAIL-002).

The formatter does not change: `reihitsu-format` and every code fix that formats a node through the formatter, such as RH5303's, keep a trailing comma in front of every conditional directive, as before (FMT-STRUCT-019, FMT-STRUCT-020). Formatting therefore does not clear the diagnostic for such a comma; applying the trailing-comma fix does.

## Why

A comma in front of an `#elif`, `#else`, or `#endif`, with no `#if` behind it, stands at the end of the branch that holds it. A build that compiles the comma takes that branch, skips every branch an `#elif` or `#else` starts behind it, and behind an `#endif` reads on in the branch of the enclosing group that holds the comma, where — as in the analyzed build — nothing but trivia and directives stands in front of the closing brace. So no build that compiles the comma compiles an element behind it, and removing the comma keeps every build intact. The check reads only the kinds of the directives in the gap, never their conditions (SHARED-FIX-009), so it is a cheap syntactic guard (`AGENTS.md`, *Special cases do not grow the source*). Before this change the fix was withheld for these commas although it was safe; `AGENTS.md`, *Code-fix coverage*, calls a guard that refuses safe input worth narrowing when the narrowing stays a cheap check.

## Visible output changes

The rule pages do not change: a fix that is withheld in a corner case is not listed on them, and `documentation/rules/general-notes.md` already covers diagnostics without a fix. The formatter's output does not change: `reihitsu-format` keeps every comma below, before and after, so each of these diagnostics stays after formatting until the fix is applied.

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

### RH5410 · Final array initializer items must not have trailing commas

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

After: the fix removes it; `reihitsu-format` still keeps it, so the diagnostic stays after formatting until the fix is applied. The fix's result:

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

### RH5411 · Final collection initializer items must not have trailing commas

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

After: the fix removes it, and `2,`, code the `#if` excludes, stays as written; `reihitsu-format` still keeps the comma behind `3`. The fix's result:

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

### The formatter — no change

Input, formatted with `reihitsu-format`, which defines no symbol:

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

Before and after: unchanged. The comma behind `High` stands in front of an `#endif` and stays (FMT-STRUCT-019); RH5409 reports it, and its fix removes it, as in the `#else` example above.

### Code fixes that format the list through the formatter — no change

A code fix that formats, through the formatter, a node holding an enum or an array or collection initializer whose last comma stands in front of an `#elif`, `#else`, or `#endif` keeps that comma, before and after. Confirmed on `main` for RH3202, RH3208, RH5110 (an enum inside the formatted declaration), RH5301, RH5303, and RH5304. Example — RH5303 (collection initializer should be formatted correctly), in a build that does not define `X`:

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

Before and after:

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

The comma behind `3` stays reported by RH5411, whose fix removes it.

## Decisions

### Maintainer answer

The maintainer was asked whether the trailing-comma fix and the formatter should remove a comma in front of `#else`, `#elif`, or `#endif` when no `#if` stands between the comma and the closing brace. Options: A, the fix and the formatter both remove it, which also changes the result of every code fix that formats such a list through the formatter; B, nothing changes; C, only the RH5409, RH5410, and RH5411 code fix removes it, and the formatter and the formatter-delegating fixes keep it, so the diagnostic stays after formatting until the fix is applied. The maintainer answered **C**. Their concern was that the formatter cannot know the directives; after the clarification that the check reads only the kinds of the directives, they kept C.

### The formatter keeps the comma; only the rule's fix removes it

The formatter keeps the comma in front of every conditional directive (FMT-STRUCT-019, FMT-STRUCT-020), and its requirement says so explicitly, so that the fix and the formatter deciding differently here reads as a decision and not as a disagreement to repair. The relationship needs no exception elsewhere:

- *The formatter may do more than the analyzers* (`sdd/README.md`, P11) covers the other direction, a formatter rewriting what no rule reports. Here the rule's fix does more than the formatter; that disagreement is the one the maintainer decided above.
- SHARED-DIAG-010 does not cover the trailing-comma rules: their fix removes the comma from the text itself and does not lay the code out as the formatter does. That the rules report a comma the formatter keeps already holds on `main` for a comma in front of an `#if`.

### Only an `#if` in the gap withholds the fix

The fix decides from the kinds of the directives between the comma and the closing brace: an `#if` withholds, `#elif`, `#else`, and `#endif` do not. An `#if` that stands in code another directive excludes withholds too, although a build that compiles the comma never reads it: the shape is rare, and its fix stays withheld rather than the check growing for it (`AGENTS.md`, *Code-fix coverage*).

### The diagnostic does not change

Every comma this change makes fixable was already reported (SHARED-TRAIL-002, SHARED-DIAG-008); only the fix changes.

### A build that does not compile the comma

Such a build reads the comma as excluded code, which nothing compiles, except where a string or a comment that only an excluded branch opens holds it. That is SHARED-FIX-012's decided gap; every trailing comma the fix removes already shares it, with or without a directive behind it, so this change adds nothing to it.

## Alternatives considered

- **A: the formatter removes the comma too** — rejected by the maintainer. It would also have changed the result of RH3202, RH3208, RH5110, RH5301, RH5303, and RH5304 wherever they format such a list.
- **B: leave every comma in front of a conditional directive unfixed** — the behavior before this change. Rejected by the maintainer; it withholds a fix that is safe in every build.
- **Let an `#if` in excluded code not withhold the fix** — rejected: the shape is rare, and withholding is the default for a rare class.
- **Evaluate the conditions of the directives** — rejected: SHARED-FIX-009 forbids working out another build configuration, and the kinds alone already decide every case this change makes fixable.
- **Declare a conditional directive between the last comma and the closing brace unsupported** — not asked again: the maintainer rejected that shape in `sdd/changes/2026-11/report-across-conditional-directives.md`, and under C the formatter's keep-the-comma check stays, so declaring it would delete nothing.
