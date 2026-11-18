# RH5307 and RH5401 fixes judge the construct they report

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH5307-001 (clarified), RH5307-004 (changed), RH5401-003 (changed), RH5401-004 (changed) |

## What

**RH5307.** The code fix joins the argument list the diagnostic is located on, whatever the receiver of the element access is and whatever contains it (RH5307-004). Until now the requirement covered only an element access outside every other bracketed argument list and with a receiver that does not start with an element access, and the fix took the first bracketed argument list around the diagnostic's first token — the receiver's first token — and judged that list instead of the reported one. Inside another argument list, and behind a receiver with a comment in its own brackets, the fix was withheld; behind a receiver that is an element access with a list on one line, the fix was offered and changed nothing. RH5307-001 now states that the argument list of a null-conditional element access (`c?[…]`) is not reported, which is the analyzer's behavior today, and the rule page says so.

**RH5401.** The code fix is offered for a property with attribute lists as well (RH5401-003). Until now the fix was withheld for every such property, because it required the whole formatted declaration, attribute lines included, to be one line. The fix now judges only the property's own code, from its first token behind its attribute lists through its `;` (RH5401-004); attribute lists stand on lines of their own above it, as the formatter places them (FMT-BREAK-027).

## Why

thoenissen/Reihitsu#1277 carries the maintainer's decisions of 2026-10-20 on thoenissen/Reihitsu#1217 (Q1, Q3) and thoenissen/Reihitsu#1219 (Q1). Both fixes refused inputs they can rewrite safely, because their guards looked at more, or at other, code than the diagnostic names (`AGENTS.md`, *Code-fix coverage*: the defect is the guard's scope). Neither change needs more than the syntax tree already gives: the reported list is the one at the diagnostic's location, and the property's own code is the declaration without its attribute lists.

## Visible output changes

**RH5307 · the reported list inside another argument list, or behind an element-access receiver**

```csharp
// input
class C
{
    void M(int[] x, int[,] m, int[][,] j)
    {
        var a = x[m[1,
                    2]];
        var b = j[0][1,
                     2];
        var c = j[0 /* c */][1,
                             2];
        var d = j[N(1,
                    2)][1,
                        2];
    }

    int N(int a, int b) => a;
}
```

```csharp
// before: RH5307 reports a, b, c, and d; no fix is offered for a, c, and d; for b a fix is offered and changes nothing
```

```csharp
// after: RH5307 reports a, b, c, and d; the fix gives
class C
{
    void M(int[] x, int[,] m, int[][,] j)
    {
        var a = x[m[1, 2]];
        var b = j[0][1, 2];
        var c = j[0 /* c */][1, 2];
        var d = j[N(1,
                    2)][1, 2];
    }

    int N(int a, int b) => a;
}
```

**RH5307 · rule page**

The description of `documentation/rules/RH5307.md` gains the sentence "A null-conditional index access (`target?[...]`) is not checked." The diagnostic does not change: `c?[1,⏎ 2]` was not reported before and is not reported after.

**RH5401 · a property with attribute lists**

```csharp
// input
using System;

class C
{
    [Obsolete]
    [CLSCompliant(false)]
    public int A
        => 1;

    [Obsolete] int B
        => 1;
}
```

```csharp
// before: RH5401 reports A and B; no fix is offered
```

```csharp
// after: RH5401 reports A and B; the fix gives
using System;

class C
{
    [Obsolete]
    [CLSCompliant(false)]
    public int A => 1;

    [Obsolete]
    int B => 1;
}
```

A property with attribute lists whose expression the formatter keeps wrapped (`[Obsolete]⏎int E =>⏎    1⏎    + 2;`) is still reported without a fix.

**RH5401 · rule page**

In `documentation/rules/RH5401.md`, "The code fix is offered when the whole declaration fits on one line." becomes "The code fix is offered when the property fits on one line; attributes keep lines of their own above it."

## Decisions

### RH5307 does not cover `?[ ]`

The maintainer decided on thoenissen/Reihitsu#1217 Q1 (2026-10-20): a null-conditional element access is not part of RH5307; the spec and the rule page say so.

### The RH5307 fix resolves the reported list by the diagnostic's location

The maintainer decided on thoenissen/Reihitsu#1217 Q3 (2026-10-20), under the guard-scope rule of `AGENTS.md`, *Code-fix coverage*: the fix judges and joins the list the diagnostic is located on. A list in the receiver or around the element access is a finding of its own, or none.

### The RH5401 fix judges the property's own lines

The maintainer decided on thoenissen/Reihitsu#1219 Q1 (2026-10-20), under the same rule: attribute lines do not keep the fix from being offered. The attribute lists are part of the reported declaration (RH5401-002), so the fix lays them out as the formatter does, and one that shares the property's line moves onto a line of its own.

## Alternatives considered

- **Keep the RH5307 fix's scope and record the withheld inputs in the spec** — rejected by the maintainer: the inputs are safe to rewrite, and withholding the fix gives that ground away.
- **Offer the RH5401 fix for an attributed property only when its attribute lists already stand on lines of their own** — rejected: the attribute lists are part of the reported construct, and the formatter's layout for them is fixed (FMT-BREAK-027); a second guard for their placement would refuse safe ground again.
