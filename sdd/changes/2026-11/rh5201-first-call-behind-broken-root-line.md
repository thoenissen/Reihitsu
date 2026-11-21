# RH5201 reports a first call that the root's line does not reach

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-CHAIN-005 (changed: states when the first invoked link must start its own line; example added; its exception for a link behind the closing `*/` of a comment names a comment that stands below the token in front of the link, also with a second comment between the `*/` and the link), FMT-BREAK-019 (clarified: names the first call the formatter breaks), RH5201-006 (added), RH5201-001 and RH5201-002 (clarified: a link behind the closing `*/` of a comment that starts its own line is reported whatever column the comment's last line starts in, also with a second comment between the `*/` and the link, as SHARED-CHAIN-005 and SHARED-COMMENT-012 already require), RH5201 `Uses` (SHARED-COMMENT-012 added), RH5201 non-goals (attached parts and operator tokens behind the first) |

## What

In a wrapped chain, the formatter starts the first invoked link on its own line when the line the root ends on does not reach it: when a link between the chain's first link and the first invoked link starts a line or holds a line break inside its operator, or when a comment, a directive, or disabled text keeps the first link below the root (SHARED-CHAIN-004). No spec stated this, and RH5201 did not report it. SHARED-CHAIN-005 now states it for both surfaces, and RH5201 reports such a first invoked link when it does not start a line (RH5201-006). The RH5201 code fix already lays the chain out through the formatter (RH5201-005), so it gives these chains the formatter's layout.

The same change brings RH5201 back to SHARED-CHAIN-005 and SHARED-COMMENT-012 for a link behind the closing `*/` of a delimited comment that starts its own line: those requirements already ask RH5201 to report such a link whatever column it reaches, but RH5201 judged it by the column of the `*/` line and reported nothing when that line started in the anchor column. RH5201-002 now states the case.

The formatter's output does not change. A line break in front of the chain's first link that nothing holds is not counted: the formatter joins it (SHARED-CHAIN-003), after which the root's line reaches the first call, and RH5112 reports it.

## Why

thoenissen/Reihitsu#893 lists two layouts the formatter changes without a diagnostic. The first is this one:

```csharp
var names = declaration.Declaration
                       .Variables.Select(v => v.Identifier);
```

The formatter writes `.Select(…)` on its own line; on `main`, RH5201 reports nothing. Where the first call stands in a wrapped chain is RH5201's construct, so the missing diagnostic is the rule's gap (P11).

The second case of the issue — the column of an attached part, or of the rest of a link operator, that a comment or a directive keeps on its own line — no longer occurs: the formatter refuses every such input as unsupported (FMT-UNSUP-014, FMT-UNSUP-015) and leaves it unchanged, so it places no such piece in any column, and the RH5201 fix is withheld on it (SHARED-FIX-010). Without a comment, the formatter joins an attached part and a split operator onto the line in front (FMT-BREAK-040, SHARED-CHAIN-006), which RH5115, RH5116, and RH5201-003 report. The issue's own example for it is refused on `main`:

```csharp
var r = a.B() // keep
    [0]
    .C();
```

```text
error: Case2a.cs(5,23): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.
```

## Visible output changes

Every *before* is `main` at `d63eab4`, produced with `scripts/apply-fix.sh RH5201` and `scripts/format.sh`. The formatter's output, and every code fix other than RH5201's, do not change.

**1. A first call behind a wrapped prefix link is reported and fixed.**

```csharp
// input
var a = c.Next
         .Items.Select(x => x);
```

```csharp
// before: not reported, no fix offered
```

```csharp
// after: reported on the `.` of `.Select`; the fix gives
var a = c.Next
         .Items
         .Select(x => x);
```

The issue's example (`declaration.Declaration` ⏎ `.Variables.Select(…)`) is the same case.

**2. A first call behind a first link that a comment keeps below the root is reported and fixed.**

```csharp
// input
var b = c // keep
        .Next.Self();
```

```csharp
// before: not reported, no fix offered
```

```csharp
// after: reported on the `.` of `.Self`; the fix gives
var b = c // keep
        .Next
        .Self();
```

**3. A first call behind a prefix link that a comment wraps is reported and fixed.**

```csharp
// input
var d = c.Next // keep
         .Next.Self();
```

```csharp
// before: not reported, no fix offered
```

```csharp
// after: reported on the `.` of `.Self`; the fix gives
var d = c.Next // keep
         .Next
         .Self();
```

**4. A first call behind a first link that a directive keeps below the root is reported and fixed.**

```csharp
// input
var b = c
#if X
        .Next.Self();
#else
        .Next.Self();
#endif
```

```csharp
// before: not reported, no fix offered
```

```csharp
// after: reported on the `.` of `.Self` in the active branch; the fix gives
var b = c
#if X
        .Next.Self();
#else
        .Next
        .Self();
#endif
```

**5. Inputs already reported for another link get one more diagnostic, on the first call; the fix result does not change.**

```csharp
// input
var a = c.Opt?
         .Next.Self();
```

```csharp
// before: one diagnostic, on the `?` of the split operator; the fix gives
var a = c.Opt
         ?.Next
         .Self();
```

```csharp
// after: two diagnostics, on the `?` and on the `.` of `.Self`; the fix result is unchanged
```

The same holds for a first link that a comment keeps below the root in the wrong column (`c // keep` ⏎ `    .Next.Self()`: before one diagnostic on `.Next`, after also one on `.Self`), and for a prefix link behind the closing `*/` of a comment that starts its own line in a column other than the anchor's (SHARED-CHAIN-005, example `r`).

**6. A link behind the closing `*/` of a comment that starts its own line, with the `*/` in the anchor column, is reported and fixed.** This holds for a later call, for a link of a chain without a call, and for a prefix link; behind a prefix link, the first call is reported as well (RH5201-006). It holds as well when a second comment stands between the `*/` and the link (`f`).

```csharp
// input
var a = c.Self()
         .Self()

         /* x
         */ .Self();
var b = c.Next
         .Next

         /* x
         */ .Next;
var d = c.Next

         /* x
         */ .Next.Self();
var f = c.Self()
         .Self()

         /* x
         */ /* y */ .Self();
```

```csharp
// before: not reported, no fix offered
```

```csharp
// after: reported on the `.` behind each `*/` (in `f`, the `.` behind `/* y */`), and in `d` also on the `.` of `.Self`; the fix gives
var a = c.Self()
         .Self()

         /* x
         */
         .Self();
var b = c.Next
         .Next

         /* x
         */
         .Next;
var d = c.Next

         /* x
         */
         .Next
         .Self();
var f = c.Self()
         .Self()

         /* x
         */ /* y */
         .Self();
```

**7. Not reported, before and after** — a line break in front of the first link that nothing holds (RH5112 reports the first link, and the formatter joins the whole chain onto one line), a first call that already starts its own line, a chain without a call, and a prefix link whose operator a comment holds split (the CLI refuses that file as unsupported, and the RH5201 fix is withheld on it):

```csharp
var e = c
         .Next.Self();
var f = c.Next
         .Next
         .Self();
var h = c.Next
         .Next.Next;
var g = c.Opt? // keep
         .Next.Self();
```

**8. The RH5201 rule page** gains one bullet under *Description* — "The first call starts its own line as well when a member access in front of it starts a line" — and the matching words under *How to fix it*.

## Decisions

### Maintainer answers

The maintainer accepted the visible output changes as recommended (answers 1A, 2A, 3A, 4B, 5A):

- **A first call behind a wrapped prefix link is reported** (changes 1 and 6, the prefix behind an own-line `*/` included), on the first call's `.`; the fix gives the formatter's layout.
- **A first call behind a first link that a comment or a directive holds below the root is reported** (changes 2–4); behind `#if`, only the active branch is reported and fixed.
- **A chain already reported for another link gets one more diagnostic**, on the first call; the fix result does not change (change 5).
- **A line break that a comment, a directive, or disabled text keeps inside a prefix link's operator does not count** (change 7, `g`): it is counted as RH5201-003 counts it, as if it were not there, so the first call behind it is not reported. The same holds for such a line break inside the first link's operator. The formatter refuses every input that holds one (FMT-UNSUP-015), so its output is not involved.
- **The rule page** gains the bullet and the *How to fix it* words of change 8.

In a second batch, the maintainer answered A for a second comment between the closing `*/` of an own-line comment and the link (change 6, `f`): the link is reported, and the fix gives `*/ /* y */` ⏎ `.Self();`, which is the formatter's output for the same input. The comment between does not give the link a line of its own, so it is judged as the link directly behind the `*/`.

### Report a link behind an own-line `*/` in the anchor column

SHARED-CHAIN-005's exception and SHARED-COMMENT-012 already require RH5201 to report a link behind the closing `*/` of a comment that starts its own line, whatever column it reaches, and the formatter breaks the line in front of it. RH5201 compared only the column of the `*/` line, so it missed the link when that line started in the anchor column. The gap lies in the construct this change touches and in the layouts the issue lists as changed without a diagnostic, so it is closed here (change 6); RH5201-002 states the case, and the RH5201 spec now lists SHARED-COMMENT-012 under `Uses`.

### State the rule in the shared chain spec

The formatter already breaks in front of such a first call, and RH5201 is to report it; the rule belongs in SHARED-CHAIN-005, which both surfaces follow, rather than in RH5201 alone. FMT-BREAK-019 names it in its summary of SHARED-CHAIN-005.

### Drop the issue's second case

No caller reaches it (`AGENTS.md`, *Supported inputs*): every input that holds the shape is refused by the precheck and left unchanged. RH5201's non-goals name it, so nobody adds a column check for it.

### One diagnostic per link

The first call is reported as one more misplaced link (RH5201-004). In an input already reported for another link, the count of diagnostics now equals the number of links the fix moves.

## Alternatives considered

- **Keep the case as a documented parity exception** (the issue's alternative) — rejected: the formatter changes a layout inside RH5201's own construct, which is the rule's gap (P11), and the RH5201 fix already produces the formatter's layout, so reporting costs one condition.
- **Change the formatter to keep the first call on the wrapped prefix line** — rejected: it changes formatter output on code the formatter lays out consistently today, and leaves the first step at the end of a wrapped line, which SHARED-CHAIN-005 avoids for every later step.
- **Report the case under RH5112** — rejected: RH5112 is about the chain's first link starting a line; the first call's line is RH5201's (RH5201-001).
