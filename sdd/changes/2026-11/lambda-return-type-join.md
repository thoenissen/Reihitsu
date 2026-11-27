# A lambda's explicit return type joins its parameter list

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-BREAK-011 (changed), FMT-BREAK-015 (changed), FMT-BREAK-045 (added) |

## What

The line-break phase joins the `(` of a parenthesized lambda's parameter list onto the line of the lambda's explicit return type, and removes the blank lines between them, wherever the lambda stands. A comment, a directive, or disabled text in that gap keeps its line breaks (FMT-BREAK-001). FMT-BREAK-011 names the requirements that own the gaps in front of a lambda's opening parenthesis.

## Why

The return type belongs to the lambda as a method's return type belongs to its declaration. A line break between it and the parameter list structures nothing, and the line it leaves, `(int x) => x`, reads like a new expression and needs a continuation column of its own. The gap behind a lambda's modifier is already joined (FMT-BREAK-041), and the same gap behind a return type is already joined inside a list element (FMT-BREAK-015); only the return type of a lambda that is not a list element kept its line break.

The issue's examples behind `static` and `async`, as a declaration's value and in a conditional branch, no longer reproduce: FMT-BREAK-041 joins a lambda's modifier with the token behind it, and on this branch's base (fe7b8f8) `Func<int, int> f = static⏎    (int x) => x;` already becomes `Func<int, int> f = static (int x) => x;`. What remains of the issue is the explicit return type, which the issue names as applying the same way.

## Visible output changes

Each "before" is the output of `scripts/format.sh` on fe7b8f8. Each "after" is the layout this change specifies; it is not built yet.

### 1. Return type on the line above the parameter list

```csharp
using System;

class C
{
    void M()
    {
        Func<int, int> i = int
            (int x) => x;
        Func<int, int> h = static int

            (int x) => x;
    }

    Func<int, int> P() => int
        (int x) => x;
}
```

Before — the line break and the blank line stay, and the `(` is a continuation line one level deeper than the statement (FMT-INDENT-034); the expression-bodied method gets a block body from the structural transforms, before and after:

```csharp
        Func<int, int> i = int
            (int x) => x;
        Func<int, int> h = static int

            (int x) => x;
    }

    Func<int, int> P()
    {
        return int
            (int x) => x;
    }
```

After:

```csharp
        Func<int, int> i = int (int x) => x;
        Func<int, int> h = static int (int x) => x;
    }

    Func<int, int> P()
    {
        return int (int x) => x;
    }
```

### 2. In a conditional branch, and with a block body

```csharp
using System;

class C
{
    void M(bool b)
    {
        Func<int, int> g = b
            ? int
                (int x) => x
            : static int (int x) => x;
        Func<int, int> m = int
            (int x) =>
            {
                return x;
            };
    }
}
```

Before:

```csharp
        Func<int, int> g = b
                               ? int
                                   (int x) => x
                               : static int (int x) => x;
        Func<int, int> m = int
            (int x) =>
            {
                return x;
            };
```

After — the block aligns under the joined `(` (FMT-INDENT-028):

```csharp
        Func<int, int> g = b
                               ? int (int x) => x
                               : static int (int x) => x;
        Func<int, int> m = int (int x) =>
                               {
                                   return x;
                               };
```

### 3. Not changed: a comment in the gap

```csharp
using System;

class C
{
    void M()
    {
        Func<int, int> i = int // c
            (int x) => x;
    }
}
```

is left unchanged, before and after.

### Code-fix results

Every code fix that formats a node holding such a lambda gives the joined layout. Checked with `scripts/apply-fix.sh`: RH5111 on `f =⏎    int⏎    (int x) => x;` gives `f = int⏎    (int x) => x;` today, and `f = int (int x) => x;` after this change. A lambda inside an argument list is joined today already (FMT-BREAK-015), so RH5101 and RH5102 do not change for it.

No rule page changes: no rule reports the line break in front of a lambda's `(`; RH5105 leaves a lambda's opening parenthesis out.

## Decisions

### Maintainer answer (2026-11-27)

Asked as an output question with the input of entries 1 and 2 and the argument form `N(int⏎(int x) => x)`, which is joined already: **A** join the `(` onto the line of the return type, **B** keep the line break. The maintainer chose **A**, as recommended.

### A requirement of its own, not an entry of FMT-BREAK-041

FMT-BREAK-041's gaps are the ones FMT-UNSUP-017 refuses a comment in, so that list never meets a comment. The gap behind a lambda's return type is not declared unsupported: a comment there keeps the line break under the guard every other join of the phase has (FMT-BREAK-001), and declaring it would delete no special case. The join is therefore a requirement of its own.

### The gap in front of the return type is not part of it

The gap between a lambda's modifier and its return type is FMT-BREAK-041's, and the gap in front of the lambda's first token belongs to the code around the lambda (FMT-BREAK-011). This change joins only the gap between the return type and the `(`.

### One owner for the gap, also inside a list element

Inside a list element the gap between a lambda's return type and its `(` was joined as a wrap inside the element (FMT-BREAK-015). FMT-BREAK-045 now owns that gap wherever the lambda stands, and FMT-BREAK-015 leaves it to it, as it leaves the opening parenthesis of an argument list to FMT-BREAK-039. Both joins refuse the same trivia and write the same minimal separator, so the output inside a list element does not change; FMT-BREAK-015 keeps the `(` of a parenthesized lambda without a return type.

## Alternatives considered

- **Keep the line break and the continuation indent** (the issue's options B and C, which are today's output on fe7b8f8) — rejected by the maintainer's answer: the line break structures nothing, and the same gap is joined inside a list element and behind a modifier.
- **Add the gap to FMT-BREAK-041 and declare a comment in it unsupported** — rejected: the shape deletes no special case, so it is not declared (`AGENTS.md`, *Special cases do not grow the source*).
