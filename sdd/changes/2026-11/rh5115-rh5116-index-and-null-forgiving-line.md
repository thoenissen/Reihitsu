# RH5115 and RH5116 report an index access or a null-forgiving operator below its expression

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | the specs `analyzer/RH5115.md` (RH5115-001 to RH5115-006) and `analyzer/RH5116.md` (RH5116-001 to RH5116-006) this change creates; FMT-BREAK-040 (Why reworded, behavior unchanged); the non-goals and the introduction of `shared/fluent-chain-layout.md` (reworded, no requirement changed) |

## What

Two new Layout rules report the parts whose gap FMT-BREAK-040 joins when a line break separates them from the code in front of them, wherever the part stands, in a member-access chain with links or not:

- RH5115 "Index access must stay on the line of its expression" reports an element access (`arr⏎[0]`) and a conditional element access (`n⏎?[0]`, `n?⏎[0]`);
- RH5116 "Null-forgiving operator must stay on the line of its expression" reports a null-forgiving operator not followed by `.` or `?.` (`s⏎! + ""`).

Both are warnings, enabled by default, with a code fix and Fix All, and both report nothing in a file with a syntax error. Their fixes work alike:

- outside a chain with links, the fix deletes the gap (SHARED-WS-003), as the RH5105 fix does, and changes nothing else — a later line aligned to code behind the part keeps its column, and another rule reports it;
- in a chain with links, the fix lays out the whole chain (SHARED-CHAIN-008, SHARED-CHAIN-009), as the RH5112 and RH5201 fixes do;
- a comment, a directive, or disabled text in the gap does not prevent the report, and withholds the fix.

The opening parenthesis of an argument list (`x.B⏎(1)`, `Foo⏎(4)`) is part of neither rule.

The analyzers, their fixes, and the formatter's precheck and line-break phase decide which token starts such a part, and which trivia form its gap, by one predicate, which moves from the formatter into `Reihitsu.Core`; the formatter's output does not change.

New rule pages, `documentation/rules/RH5115.md` and `documentation/rules/RH5116.md`, describe them.

## Why

thoenissen/Reihitsu#892 asked for a rule that reports what the formatter joins in front of an attached part, so that a user who runs only the analyzers learns about the layout. The issue is agent-filed; three of its statements no longer hold:

- **Scope.** The issue limited the report to chains with a link, except `?[`, because the formatter joined only there. Since FMT-BREAK-040, the formatter joins `[`, `?[`, and a trailing `!` everywhere. An element access below its expression is the same construct inside and outside a chain, so a rule limited to chains would have its own gap ([P11](../../README.md#the-formatter-may-do-more-than-the-analyzers-p11)).
- **Argument lists.** The issue included `x.B⏎(1)`. The argument list's `(` is joined by a requirement of its own (FMT-BREAK-039), and the open candidate thoenissen/Reihitsu#1827, which the maintainer scheduled separately, proposes a rule for exactly that parenthesis. A rule that reported it only in chains with a link would report `this.Foo⏎(4)` but not `Foo⏎(4)`; leaving all argument lists to that candidate gives each rule one construct.
- **Comments.** The issue asked not to report a gap that holds a comment or a directive, because the formatter kept such a gap. Since FMT-UNSUP-014, the formatter refuses such an input as unsupported, which SHARED-DIAG-010 says a layout rule reports, with the fix withheld (SHARED-FIX-010).

RH5201 is not extended: its subject is the links of a chain, and an attached part is never a link (SHARED-CHAIN-001).

## Visible output changes

**1. New diagnostics: an index access (RH5115) or a null-forgiving operator (RH5116) below its expression, also outside a chain**

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
// after: RH5115 on the [ of a and f and the ? of b and c; RH5116 on the ! of d
        var a = arr
            {|RH5115:[|}0];
        var b = n
            {|RH5115:?|}[0];
        var c = n{|RH5115:?|}
            [0];
        var d = s
            {|RH5116:!|} + "";
        var f = items.Where(x => x > 0)
                     .ToArray()
                     {|RH5115:[|}0];
```

**2. The fix outside a chain with links joins the part, and nothing else**

```csharp
// input: a, b, c, d of change 1, and
#nullable enable

class D
{
    void M(int[] arr, string? s)
    {
        var g = s
            ! + Foo(1,
                    2);
        var h = Bar(arr
                [0]).ToString()
                     .Trim();
    }

    string Foo(int x, int y)
    {
        return "";
    }

    int Bar(int x)
    {
        return x;
    }
}
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

        var g = s! + Foo(1,
                    2);
        var h = Bar(arr[0]).ToString()
                     .Trim();
```

`2` and `.Trim` keep their columns; RH5203 and RH5201 then report them, and their fixes realign them.

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
// after: RH5115 on [, no fix offered
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
// before: not reported by RH5115 or RH5116 (b is RH5112's)
```

```csharp
// after: unchanged — not reported by RH5115 or RH5116
```

**6. Two new rule pages**: `documentation/rules/RH5115.md`, with the violation `return values⏎    [0];` and the correction `return values[0];`, and `documentation/rules/RH5116.md`, with the violation `return value⏎    !;` and the correction `return value!;`.

## Decisions

### The maintainer's answers

- **Scope per part (question 1: A).** `[`, `?[`, and `!` are reported everywhere; no argument list's `(` is reported, in a chain or not. Those stay with thoenissen/Reihitsu#1827.
- **Comment or directive in the gap (question 2: A).** Reported, no fix offered.
- **The fix in a chain with links (question 3: A).** The fix lays out the whole chain.
- **Rule identity (question 4: A, with a change).** Two rules instead of one: RH5115 "Index access must stay on the line of its expression" for `[` and `?[`, both gaps of `?[` included, and RH5116 "Null-forgiving operator must stay on the line of its expression" for `!`. Both are Warning, enabled by default, with Fix All, report nothing in a file with a syntax error, and fix alike.
- **The fix outside a chain, with later lines aligned to code behind the part (question 5: A).** The fix deletes only the gap; an aligned later line keeps its column, also when the part stands inside a chain with links it is not attached to, and the rule that reports the misalignment fixes it.

### Rules of their own, not RH5201

RH5201 reports links; an attached part is never a link (SHARED-CHAIN-001), and RH5201's message is about alignment. New IDs in the line-break range RH51xx, next to RH5105 and RH5113, which report a token that belongs on the line of the token in front of it.

### One construct per rule, everywhere

Each rule's construct is its part and its gap; whether a chain surrounds it does not change the construct, and the formatter joins it in both places. An index and a null-forgiving operator are different constructs with different messages, so each gets a rule a user can configure on its own. A `!` in front of an index, as in `s⏎!⏎?[0]`, is two parts: RH5116 reports the `!`, RH5115 the `?`. The argument list's opening parenthesis is FMT-BREAK-039's, and is left to a rule of its own.

### The fix by whether the part's chain has a link

Without a link, no column of the part's chain depends on the part, and deleting the gap is the formatter's output for that gap. With a link, joining the part can move the chain's first link and the anchor of every wrapped link, so the fix takes the formatted layout of the whole chain, as RH5112's does.

### One predicate for the gap

The formatter's precheck (FMT-UNSUP-014) and line-break phase (FMT-BREAK-040) already decide the gaps by one predicate; the analyzers need the same decision, and the analyzer assembly does not reference the formatter. The predicate moves to `Reihitsu.Core` and keeps its name, so the formatter, both analyzers, and both fixes use one copy and cannot disagree on which token starts a part, such as a `!` in front of `?[` versus one in front of `?.`.

## Alternatives considered

- **Extend RH5201** — rejected: a different construct from RH5201's links, and a message about alignment.
- **One rule for `[`, `?[`, and `!`** — rejected by the maintainer: two constructs, one rule each.
- **Only chains with a link, except `?[` (the issue's scope)** — rejected: since FMT-BREAK-040 the formatter joins the parts everywhere, and the rule would have a gap inside its own construct.
- **Include the argument list's `(` in chains with a link** — rejected: reports `this.Foo⏎(4)` but not `Foo⏎(4)`, and overlaps the separate argument-list candidate.
- **Include every argument list's `(`, as FMT-BREAK-039 joins it** — rejected: it decides thoenissen/Reihitsu#1827, which the maintainer keeps separate.
- **Do not report a gap that holds a comment or a directive** — rejected: SHARED-DIAG-010 reports a shape the formatter refuses.
- **Join only, also in a chain with links** — rejected: `x⏎[0].B()⏎.C()` would become `x[0].B()⏎    .C()` with `.C` no longer under `.B`, a layout RH5201 then reports.
- **Lay out the outermost chain when the part stands inside a chain with links it is not attached to** — rejected: it fixes only the chain half of the misalignment class, adds a second chain condition to the fix, and changes code outside the reported gap.
- **A private copy of the gap predicate in the analyzer** — rejected: a second statement of the same rule, which can drift from the formatter's.
