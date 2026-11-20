# A wrapped link behind a prefix of property reads aligns to the last of them

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | SHARED-CHAIN-007 (changed: without an invoked link in front of the first wrapped link, the anchor is the last link in front of it, not the chain's first link), FMT-INDENT-029 (example added), RH5201-001 (example changed), RH5201-002 (example added), RH5111-006 (example changed) |

## What

SHARED-CHAIN-007 picks the anchor that every wrapped link of a chain aligns to. On `main` it is the first invoked link in front of the first link that starts a line, or the chain's first link when no invoked link comes before it. The second case changes: when no invoked link comes before the first link that starts a line, the anchor is the **last** link in front of it — the link the first wrapped link continues. This covers a chain whose first call is wrapped behind a prefix of property reads (`x.Items.Count` ⏎ `.ToString()` aligns under `.Count`) and a chain without any call (`order.Customer.Address` ⏎ `.City` aligns under `.Address`). The chain's first link stays the anchor when it is the only link in front of the first wrapped link, and when the first link itself starts a line; the root stays the anchor when a comment, a directive, or disabled text keeps the first link on its own line (SHARED-CHAIN-004).

The requirement also states what "starts a line" means for the anchor: a line break separates the first token of the link's operator from the token in front of it, whatever trivia carries it. That is what the shared chain model counts today; a link behind the closing `*/` of a multi-line comment therefore counts as starting a line for the anchor, and the links in front of it decide the anchor, as the formatter sees them after it breaks the line in front of that link (SHARED-CHAIN-005).

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
```

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

### "Starts a line" is the line break in front of the operator

The anchor is decided by the first link whose operator a line break separates from the token in front of it, the definition the shared chain model already uses. A link behind the closing `*/` of a multi-line comment counts as starting a line here, so the analyzer measures such a chain against the link the formatter's layout makes the anchor, and the example of SHARED-CHAIN-005 keeps its output.

### Call-less chains

Pending the maintainer's answer: whether a chain without any call follows the same rule (visible change 2).

## Alternatives considered

- **The last link on the line the root ends on** — rejected: the root of `Get(a,` ⏎ `b).Items.Count` is `Get`, whose argument list ends on a later line, so no link would stand on the root's line and the anchor would fall back to `.Items`; the links in front of the first wrapped link name the line the wrapped link continues without depending on where the root ends.
- **Keep the chain's first link as the anchor for chains without a call** — not chosen; it is asked as an output question (visible change 2). It would keep two anchor rules for prefix links depending on whether a call follows later in the chain.
- **Align every wrapped link under the last link on the line directly above it** — rejected: it would give each line its own anchor, so the links of one chain would no longer form one column, which SHARED-CHAIN-007 exists for.
