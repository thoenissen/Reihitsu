# RH5307 and RH5401 fixes judge the construct they report

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH5307-001 (clarified), RH5307-004 (changed), RH5401-003 (changed), RH5401-004 (changed) |

## What

**RH5307.** The code fix joins the argument list the diagnostic is located on, whatever the receiver of the element access is and whatever contains it (RH5307-004). Until now the requirement covered only an element access outside every other bracketed argument list and with a receiver that does not start with an element access, and the fix did not read the reported list at all: it took the first bracketed argument list around the diagnostic's first token — the receiver's first token — or, when there was none, the argument list of the innermost element access around that token, and judged and joined that list instead. The class this concerns is every reported element access whose receiver's first token lies inside another bracketed argument list (of an element access, an element binding, or an implicit element access) or inside another element access or element binding than the reported one. For its members the fix was either withheld, because the list it found was not an indexer list or could not be joined, or offered as an action that changed nothing, because the list it found was already on one line; where the receiver's own list was wrapped and reported too, both fixes joined the receiver's list and the reported one was never joined. The members whose receiver's first token lies in no such list or access — an access that is the argument of an invocation, an element of a collection expression, or has a parenthesized or member-binding receiver — were fixed and stay fixed. RH5307-001 now states that the argument list of a null-conditional element access (`c?[…]`) is not reported, which is the analyzer's behavior today, and the rule page says so.

**RH5401.** The code fix is offered for a property with attribute lists as well (RH5401-003). Until now the fix was withheld for every such property whose formatted attribute lists take lines of their own, because it required the whole formatted declaration, attribute lines included, to be one line. The fix now judges only the property's own code, from its first token behind its attribute lists through its `;` (RH5401-004); the attribute lists are laid out with the property as the formatter places them (FMT-BREAK-027), except that the fix inserts no blank line inside the property where the formatter would insert one in front of a comment behind an attribute list (RH5401-003).

## Why

thoenissen/Reihitsu#1277 carries the maintainer's decisions of 2026-10-20 on thoenissen/Reihitsu#1217 (Q1, Q3) and thoenissen/Reihitsu#1219 (Q1). Both fixes refused inputs they can rewrite safely, because their guards looked at more, or at other, code than the diagnostic names (`AGENTS.md`, *Code-fix coverage*: the defect is the guard's scope). Neither change needs more than the syntax tree already gives: the reported list is the one at the diagnostic's location, and the property's own code is the declaration without its attribute lists.

## Visible output changes

**RH5307 · the reported list inside another argument list, or behind a receiver that is or starts with an element access or element binding**

```csharp
// input
using System.Collections.Generic;

class D
{
    public int[][,] P;
}

class C
{
    int[,] this[int i] => null;

    void M(int[] x, int[,] m, int[][,] j, int[][,] c, int[] n, D[] a)
    {
        var va = x[m[1,
                     2]];
        var vb = j[0][1,
                      2];
        var vc = j[0 /* c */][1,
                              2];
        var vd = j[N(1,
                     2)][1,
                         2];
        var vp = x[/* c */ m[1,
                             2]];
        var ve = c?[0][1,
                       2];
        var vh = n?[m[1,
                      2]];
        var vi = a[0].P[0][1,
                           2];
        var vj = this[0][1,
                         2];
        var vm = new Dictionary<int, int>
                 {
                     [m[1,
                        2]] = 1,
                 };
    }

    int N(int a, int b) => a;
}
```

```csharp
// before: RH5307 reports every wrapped list above (va to vj, vm);
// no fix is offered for va, vc, vd, vp, ve, vh, and vm;
// for vb, vi, and vj a fix is offered and changes nothing
```

```csharp
// after: RH5307 reports the same lists; the fix gives
using System.Collections.Generic;

class D
{
    public int[][,] P;
}

class C
{
    int[,] this[int i] => null;

    void M(int[] x, int[,] m, int[][,] j, int[][,] c, int[] n, D[] a)
    {
        var va = x[m[1, 2]];
        var vb = j[0][1, 2];
        var vc = j[0 /* c */][1, 2];
        var vd = j[N(1,
                     2)][1, 2];
        var vp = x[/* c */ m[1, 2]];
        var ve = c?[0][1, 2];
        var vh = n?[m[1, 2]];
        var vi = a[0].P[0][1, 2];
        var vj = this[0][1, 2];
        var vm = new Dictionary<int, int>
                 {
                     [m[1, 2]] = 1,
                 };
    }

    int N(int a, int b) => a;
}
```

**RH5307 · the reported list and its receiver's list both reported**

```csharp
// input
class C
{
    void M(int[,][,] j)
    {
        var n = j[0,
                  1][1,
                     2];
    }
}
```

```csharp
// before: RH5307 reports both lists; both fixes join the receiver's list, one fix at a time and Fix All alike
class C
{
    void M(int[,][,] j)
    {
        var n = j[0, 1][1,
                     2];
    }
}
// the outer list stays reported, and its fix changes nothing
```

```csharp
// after: each fix joins its own list; one fix at a time and Fix All alike give
class C
{
    void M(int[,][,] j)
    {
        var n = j[0, 1][1, 2];
    }
}
```

**RH5307 · members fixed today, unchanged**

```csharp
// input
class E
{
    public int[,] P;
}

class C
{
    void M(int[,] m, int[][,] j, E c)
    {
        N(m[1,
            2]);
        int[] l = [m[1,
                     2]];
        var o = (j[0])[1,
                       2];
        var g = c?.P[1,
                     2];
    }

    void N(int a)
    {
    }
}
```

```csharp
// before and after: RH5307 reports the four lists; the fix gives
class E
{
    public int[,] P;
}

class C
{
    void M(int[,] m, int[][,] j, E c)
    {
        N(m[1, 2]);
        int[] l = [m[1, 2]];
        var o = (j[0])[1, 2];
        var g = c?.P[1, 2];
    }

    void N(int a)
    {
    }
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

    [Obsolete]
    // note
    int D
        => 1;
}
```

```csharp
// before: RH5401 reports A, B, and D; no fix is offered
```

```csharp
// after: RH5401 reports A, B, and D; the fix gives
using System;

class C
{
    [Obsolete]
    [CLSCompliant(false)]
    public int A => 1;

    [Obsolete]
    int B => 1;

    [Obsolete]
    // note
    int D => 1;
}
```

Formatting the document would insert a blank line in front of `// note`; the fix inserts none.

A property with attribute lists whose expression the formatter keeps wrapped (`[Obsolete]⏎int E =>⏎    1⏎    + 2;`) is still reported without a fix. A property whose attribute list keeps the property's line because it holds a comment between its brackets (`[Obsolete /* c */] int E⏎    => 1;`) was fixed before and is fixed the same way after: `[Obsolete /* c */] int E => 1;`.

**RH5401 · rule page**

In `documentation/rules/RH5401.md`, "The code fix is offered when the whole declaration fits on one line." becomes "The code fix is offered when the property, apart from its attributes, fits on one line."

## Decisions

### RH5307 does not cover `?[ ]`

The maintainer decided on thoenissen/Reihitsu#1217 Q1 (2026-10-20): a null-conditional element access is not part of RH5307; the spec and the rule page say so.

### The RH5307 fix resolves the reported list by the diagnostic's location

The maintainer decided on thoenissen/Reihitsu#1217 Q3 (2026-10-20), under the guard-scope rule of `AGENTS.md`, *Code-fix coverage*: the fix judges and joins the list the diagnostic is located on. A list in the receiver or around the element access is a finding of its own, or none.

### The RH5307 fix covers the whole class, not only the issue's instances

The issue names two instances (`x[m[1,⏎2]]`, `j[0 /* c */][1,⏎2]`). The cause — the target resolved from the receiver's first token — holds for every member listed under *Visible output changes*, including those where the fix was offered and changed nothing, so the change covers all of them, and keeps the members fixed today fixed.

### The RH5401 fix judges the property's own lines

The maintainer decided on thoenissen/Reihitsu#1219 Q1 (2026-10-20), under the same rule: attribute lines do not keep the fix from being offered. The attribute lists are part of the reported declaration (RH5401-002), so the fix lays them out as the formatter does (FMT-BREAK-027); one that shares the property's line moves onto a line of its own, unless FMT-BREAK-027 leaves its placement unspecified, as for a list with a comment between its brackets. A comment between the attribute lists and the property's own code is not part of that code.

### The RH5401 fix is offered for an attribute list on the property's line

The maintainer decided in the run for thoenissen/Reihitsu#1277, 2026-11 (question 1, answer A): `[Obsolete] int E⏎    => 1;` gets the fix, and the attribute list moves onto a line of its own, `[Obsolete]⏎int E => 1;`.

### The RH5401 fix inserts no blank line inside the property

The maintainer decided in the run for thoenissen/Reihitsu#1277, 2026-11 (question 3): the fix offered for `[Obsolete]⏎// note⏎int E⏎    => 1;` must not insert a blank line, because blank lines are not part of what the fix is for; it gives `[Obsolete]⏎// note⏎int E => 1;`. Neither option asked — the formatter's blank line in front of the comment, or no fix — was chosen. The formatter inserts that blank line under FMT-BLANK-011, and the fix formats the property as the formatter does (RH5401-003), so RH5401-003 gains the exception; the same holds for a comment between two attribute lists.

### The RH5401 rule page states the condition without an attribute layout

The maintainer decided in the run for thoenissen/Reihitsu#1277, 2026-11 (question 2, answer B): the page says "The code fix is offered when the property, apart from its attributes, fits on one line.", which holds also for an attribute list that keeps the property's line.

### The RH5307 fix joins its own list for every further receiver and container

The maintainer decided in the run for thoenissen/Reihitsu#1277, 2026-11 (question 4, answer A): the members thoenissen/Reihitsu#1217 Q3 did not name — a receiver that is or starts with an element binding, an element access in a member-access chain, `this[…]`, an implicit element access in an initializer, a comment in front of the reported access inside another list, and a reported receiver list — each get the fix that joins the list the diagnostic reports.

## Alternatives considered

- **Keep the RH5307 fix's scope and record the withheld inputs in the spec** — rejected by the maintainer: the inputs are safe to rewrite, and withholding the fix gives that ground away.
- **Offer the RH5401 fix for an attributed property only when its attribute lists already stand on lines of their own** — rejected: the attribute lists are part of the reported construct, and the formatter's layout for them is fixed (FMT-BREAK-027); a second guard for their placement would refuse safe ground again.
- **Let the RH5401 fix insert the formatter's blank line in front of a comment behind an attribute list** — rejected by the maintainer: blank lines are not part of what the fix is for.
- **Withhold the RH5401 fix for a comment between the attribute lists and the property** — rejected by the maintainer: the fix is offered, without the blank line.
