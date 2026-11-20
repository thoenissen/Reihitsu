# RH5115 reports an index or a null-forgiving operator below its expression

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | the spec `analyzer/RH5115.md` this change creates (RH5115-001 to RH5115-006); FMT-BREAK-040 (Why reworded, behavior unchanged); the non-goals and the introduction of `shared/fluent-chain-layout.md` (reworded, no requirement changed) |

## What

A new Layout rule, RH5115 "Index and null-forgiving operator must stay on the line of their expression", reports an element access (`arr⏎[0]`), a conditional element access (`n⏎?[0]`, `n?⏎[0]`), and a null-forgiving operator not followed by `.` or `?.` (`s⏎! + ""`) that a line break separates from the code in front of it — the parts whose gap FMT-BREAK-040 joins — wherever the part stands, in a member-access chain with links or not. It is a warning, enabled by default, with a code fix and Fix All:

- outside a chain with links, the fix deletes the gap (SHARED-WS-003), as the RH5105 fix does;
- in a chain with links, the fix lays out the whole chain (SHARED-CHAIN-008, SHARED-CHAIN-009), as the RH5112 and RH5201 fixes do;
- a comment, a directive, or disabled text in the gap does not prevent the report, and withholds the fix.

The opening parenthesis of an argument list (`x.B⏎(1)`, `Foo⏎(4)`) is not part of the rule.

A new rule page, `documentation/rules/RH5115.md`, describes it.

## Why

thoenissen/Reihitsu#892 asked for a rule that reports what the formatter joins in front of an attached part, so that a user who runs only the analyzers learns about the layout. The issue is agent-filed; three of its statements no longer hold:

- **Scope.** The issue limited the report to chains with a link, except `?[`, because the formatter joined only there. Since FMT-BREAK-040, the formatter joins `[`, `?[`, and a trailing `!` everywhere. An element access below its expression is the same construct inside and outside a chain, so a rule limited to chains would have its own gap ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)).
- **Argument lists.** The issue included `x.B⏎(1)`. The argument list's `(` is joined by a requirement of its own (FMT-BREAK-039), and the open candidate thoenissen/Reihitsu#1827, which the maintainer scheduled separately, proposes a rule for exactly that parenthesis. A rule that reported it only in chains with a link would report `this.Foo⏎(4)` but not `Foo⏎(4)`; leaving all argument lists to that candidate gives each rule one formatter requirement.
- **Comments.** The issue asked not to report a gap that holds a comment or a directive, because the formatter kept such a gap. Since FMT-UNSUP-014, the formatter refuses such an input as unsupported, which SHARED-DIAG-010 says a layout rule reports, with the fix withheld (SHARED-FIX-010).

RH5201 is not extended: its subject is the links of a chain, and an attached part is never a link (SHARED-CHAIN-001).

## Visible output changes

**1. A new diagnostic: an index or a null-forgiving operator below its expression, also outside a chain**

```csharp
// input
#nullable enable
using System.Linq;

class C
{
    void M(int[] arr, int[]? n, string? s, int[] items)
    {
        var a = arr
            [0];
        var b = n
            ?[0];
        var c = n?
            [0];
        var d = s
            ! + "";
        var f = items.Where(x => x > 0)
                     .ToArray()
                     [0];
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: reported on the [ of a, the ? of b and c, the ! of d, and the [ of f
        var a = arr
            {|RH5115:[|}0];
        var b = n
            {|RH5115:?|}[0];
        var c = n{|RH5115:?|}
            [0];
        var d = s
            {|RH5115:!|} + "";
        var f = items.Where(x => x > 0)
                     .ToArray()
                     {|RH5115:[|}0];
```

**2. The fix outside a chain with links joins the part**

```csharp
// input: a, b, c, d of change 1
```

```csharp
// before: no fix
```

```csharp
// after
        var a = arr[0];
        var b = n?[0];
        var c = n?[0];
        var d = s! + "";
```

**3. The fix in a chain with links lays out the chain**

```csharp
// input
class C
{
    void M(dynamic x)
    {
        var a = x.B()
            [0]
            .C();
        var e = x
            [0].B()
            .C();
    }
}
```

```csharp
// before: not reported, no fix
```

```csharp
// after
class C
{
    void M(dynamic x)
    {
        var a = x.B()[0]
                 .C();
        var e = x[0].B()
                    .C();
    }
}
```

**4. A comment or a directive in the gap: reported, no fix**

```csharp
// input
class C
{
    void M(int[] arr)
    {
        var a = arr // first
            [0];
    }
}
```

```csharp
// before: not reported
```

```csharp
// after: reported on [, no fix offered
        var a = arr // first
            {|RH5115:[|}0];
```

**5. Not reported: an argument list below its name, a link operator, other brackets**

```csharp
// input
#nullable enable

class C
{
    void M(dynamic x, string? s)
    {
        var a = x.B
            (1);
        var b = s
            !.Length;
    }
}
```

```csharp
// before: not reported by RH5115 (b is RH5112's)
```

```csharp
// after: unchanged — not reported by RH5115
```

**6. A new rule page** `documentation/rules/RH5115.md`, with the violation `return values⏎    [0];` and the correction `return values[0];`.

## Decisions

### A rule of its own, not RH5201

RH5201 reports links; an attached part is never a link (SHARED-CHAIN-001), and RH5201's message is about alignment. A new ID in the line-break range RH51xx, next to RH5105 and RH5113, which report a token that belongs on the line of the token in front of it.

### One rule for the parts of FMT-BREAK-040, everywhere

The rule's construct is the part and its gap; whether a chain surrounds it does not change the construct, and the formatter joins it in both places. The argument list's opening parenthesis is FMT-BREAK-039's, and is left to a rule of its own.

### The fix by whether the part's chain has a link

Without a link, no column depends on the part, and deleting the gap is the formatter's output. With a link, joining the part can move the chain's first link and the anchor of every wrapped link, so the fix takes the formatted layout of the whole chain, as RH5112's does.

## Alternatives considered

- **Extend RH5201** — rejected: a different construct from RH5201's links, and a message about alignment.
- **Only chains with a link, except `?[` (the issue's scope)** — rejected: since FMT-BREAK-040 the formatter joins the parts everywhere, and the rule would have a gap inside its own construct.
- **Include the argument list's `(` in chains with a link** — rejected: reports `this.Foo⏎(4)` but not `Foo⏎(4)`, and overlaps the separate argument-list candidate.
- **Include every argument list's `(`, as FMT-BREAK-039 joins it** — rejected for this change: it decides a candidate the maintainer scheduled separately; the maintainer may choose it instead.
- **Do not report a gap that holds a comment or a directive** — rejected: SHARED-DIAG-010 reports a shape the formatter refuses.
- **Join only, also in a chain with links** — rejected: `x⏎[0].B()⏎.C()` would become `x[0].B()⏎    .C()` with `.C` no longer under `.B`, a layout RH5201 then reports.
