# A wrapped link behind a prefix of property reads aligns to the last of them

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-CHAIN-007 (changed: without an invoked link in front of the first wrapped link, the anchor is the last link in front of it, not the chain's first link), FMT-INDENT-029 (example added), RH5201-001 (example changed), RH5201-002 (example added, `Why:` changed), RH5112-003 (changed: the later links align to the chain's anchor, not under the first link; example added), RH5111-006 (example changed) |

## What

SHARED-CHAIN-007 picks the anchor that every wrapped link of a chain aligns to. On `main` it is the first invoked link in front of the first link that starts a line, or the chain's first link when no invoked link comes before it. The second case changes: when no invoked link comes before the first link that starts a line, the anchor is the **last** link in front of it — the link the first wrapped link continues. This covers a chain whose first call is wrapped behind a prefix of property reads (`x.Items.Count` ⏎ `.ToString()` aligns under `.Count`) and a chain without any call (`order.Customer.Address` ⏎ `.City` aligns under `.Address`). The chain's first link stays the anchor when it is the only link in front of the first wrapped link, and when the first link itself starts a line; the root stays the anchor when a comment, a directive, or disabled text keeps the first link on its own line (SHARED-CHAIN-004).

The requirement also states what "starts a line" means for the anchor: a line break that SHARED-BLANK-008 counts stands in the trivia between the first token of the link's operator and the token in front of it — an end-of-line trivia, or the line break that ends a `///` comment, a directive, or disabled text. A line break inside the text of a delimited comment does not count. That is what the shared chain model counts today. A link behind the closing `*/` of a delimited comment that spans lines therefore counts as starting a line for the anchor only when such a line break stands in front of the comment, as when the comment starts its own line (SHARED-CHAIN-005, example `r`); behind a comment that starts on the line of the token in front of it, the link does not start a line and can itself be the anchor (visible change 3, `q`).

Which links start a line does not change, nor does the anchor of a chain whose first call stands in front of the first wrapped link. RH5201 measures against the same anchor (RH5201-002), so it reports what the formatter moves; the RH5201, RH5112, and RH5111 code fixes lay out the chain through the formatter and give it the new columns.

## Why

A wrapped link continues the line above it. Behind a run of property reads, that is the last read on the line, not the first:

```csharp
var s = x.Items.Count
         .ToString();      // main: under .Items
```

When the run ends with a call (`items.Where(…)` ⏎ `.Select(…)`), the wrapped link already aligns under that call, which in a laid-out chain is the last link on the root's line (SHARED-CHAIN-005). The same reading for a prefix of property reads aligns `.ToString()` under `.Count`. thoenissen/Reihitsu#896 proposes this; the change that unified chain layout kept `main`'s anchor and deferred it. One owner keeps the formatter and RH5201 in sync: the shared chain model's anchor, which both read.

## Visible output changes

Every *before* is the output of `main`, produced with `scripts/format.sh` for the formatter and `scripts/apply-fix.sh` for the code fixes; every *after* follows SHARED-CHAIN-007 as changed, with every other line as `main` writes it.

**1. A wrapped call behind a prefix of two or more property reads aligns under the last of them.**

```csharp
// input
var s = x.Items.Length
    .ToString();
```

```csharp
// before
var s = x.Items.Length
         .ToString();
```

```csharp
// after
var s = x.Items.Length
               .ToString();
```

**2. A chain without a call aligns under the last link in front of its first wrapped link.**

```csharp
// input
var c = order.Customer.Address
    .City;
var j = order.Customer.Address
    .City.Street
    .Name;
```

```csharp
// before
var c = order.Customer.Address
             .City;
var j = order.Customer.Address
             .City.Street
             .Name;
```

```csharp
// after
var c = order.Customer.Address
                      .City;
var j = order.Customer.Address
                      .City.Street
                      .Name;
```

**3. Later prefix wraps and conditional links follow the same anchor; a first link joined to the root's line counts as standing on it.**

```csharp
// input
var d = x.Next.Next
    .Next
    .Self();
var g = x.Next?.Next
    ?.Name
    .Trim();
var h = x
    .Next.Next
    .Self();
var q = x.Next /* c
    */ .Next
    .Self();
```

```csharp
// before
var d = x.Next.Next
         .Next
         .Self();
var g = x.Next?.Next
         ?.Name
         .Trim();
var h = x.Next.Next
         .Self();
var q = x.Next /* c
    */.Next
         .Self();
```

```csharp
// after
var d = x.Next.Next
              .Next
              .Self();
var g = x.Next?.Next
              ?.Name
              .Trim();
var h = x.Next.Next
              .Self();
var q = x.Next /* c
    */.Next
      .Self();
```

In `q`, the `.Next` behind `*/` does not start a line, because the only line break in front of it is inside the comment's text, so it is the last link in front of the first wrapped link, `.Self()`, and the anchor. The layout of the `*/` line is `main`'s and does not change.

Unchanged: a chain whose first call stands in front of the first wrapped link (`x.Next.Self()` ⏎ `.Next` stays under `.Self`), a chain with one link in front of its first wrapped link (`x.Next` ⏎ `.Next.Next` ⏎ `.Self()` stays under the first `.Next`), and a chain whose first link a comment keeps on its own line (aligned to the root).

**4. RH5201 reports a wrapped link that is not under the last link in front of it, and no longer reports one that is.**

```csharp
class C
{
    C Next { get; set; }
    C Self() => this;

    void M(C c)
    {
        var a = c.Next.Next
                 .Self();
        var b = c.Next.Next
                      .Self();
    }
}
```

Before: `b`'s `.` of `.Self()` is reported; `a` is not reported.
After: `a`'s `.` of `.Self()` is reported (`{|RH5201:.|}Self()`); `b` is not reported. The same holds with `.Next` in place of `.Self()`.

**5. The RH5201, RH5112, and RH5111 code fixes give the chain the new columns.**

RH5201, applied to `a` above:

```csharp
// before (main: no diagnostic on a; the fix moves b)
var b = c.Next.Next
         .Self();
```

```csharp
// after (the fix moves a)
var a = c.Next.Next
              .Self();
```

RH5112, applied to a chain whose first link starts a line:

```csharp
// input
var s = x
    {|RH5112:.|}Items.Length
    .ToString();
```

```csharp
// before
var s = x.Items.Length
         .ToString();
```

```csharp
// after
var s = x.Items.Length
               .ToString();
```

RH5111, applied to an assignment behind a nested conditional access (RH5111-006):

```csharp
// input
a?.B?{|RH5111:.B
    .C =
    5|};
a.B?{|RH5111:.B
   .C =
    5|};
```

```csharp
// before
a?.B?.B
 .C = 5;
a.B?.B
 .C = 5;
```

```csharp
// after
a?.B?.B
    .C = 5;
a.B?.B
   .C = 5;
```

Every other code fix that formats a statement or member holding such a chain gives the chain the same columns, through the formatter.

**6. The RH5201 rule page** says that a continuation line aligns with the first call, or, when no call comes before the first wrapped line, with the last member access in front of that line (before: with the chain's first member access).

**7. Repository source.** Files of this repository that hold such a chain are reformatted, layout only.

## Decisions

### The rule changes only the fallback of the anchor

The first invoked link in front of the first wrapped link stays the anchor when there is one. In a laid-out chain that link is the last link on the root's line anyway (SHARED-CHAIN-005), so the formatter's output reads as one rule — the wrapped links align under the last link in front of them — while RH5201 keeps measuring an unformatted chain such as `items.Where(…).Select(…)` ⏎ `.ToList()` against `.Where`, where the formatter puts `.ToList()`.

### Maintainer answers

The maintainer answered the three output questions of this change, each with the recommended option:

- **A wrapped link behind a prefix of property reads** aligns under the last link in front of the first wrapped link (visible changes 1, 3, 4, 5, 6, and 7): `x.Items.Length` ⏎ `.ToString()` aligns under `.Length`. RH5201 reports and fixes to that column, and the RH5112 and RH5111 fixes write it.
- **A chain without any call** follows the same rule (visible change 2): `order.Customer.Address` ⏎ `.City` aligns under `.Address`.
- **A link behind a comment that spans lines and starts behind code** does not start a line for the anchor: a line break inside the text of a delimited comment does not count (visible change 3, `q`). `.Self()` aligns under the `.` of the `.Next` behind `*/`.

### "Starts a line" is the line break in front of the operator

The anchor is decided by the first link whose operator a line break that SHARED-BLANK-008 counts separates from the token in front of it, the definition the shared chain model already uses; a line break inside the text of a delimited comment does not count. A link behind the closing `*/` of a comment that starts its own line counts as starting a line, so the example of SHARED-CHAIN-005 keeps its output; a link behind the `*/` of a comment that starts behind code does not, so it stays a link in front of the first wrapped link. A second reading, under which any line break the comment's text carries would count, was rejected by the maintainer's answer; it would also need a second "starts a line" predicate next to the shared chain model's.

## Alternatives considered

- **The last link on the line the root ends on** — rejected: the root of `Get(a,` ⏎ `b).Items.Count` is `Get`, whose argument list ends on a later line, so no link would stand on the root's line and the anchor would fall back to `.Items`; the links in front of the first wrapped link name the line the wrapped link continues without depending on where the root ends.
- **Keep the chain's first link as the anchor for chains without a call** — rejected by the maintainer's answer (visible change 2). It would keep two anchor rules for prefix links depending on whether a call follows later in the chain.
- **Align every wrapped link under the last link on the line directly above it** — rejected: it would give each line its own anchor, so the links of one chain would no longer form one column, which SHARED-CHAIN-007 exists for.
