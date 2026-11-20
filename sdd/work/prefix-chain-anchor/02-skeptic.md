Role: skeptic
Commit: 889975f
Verdict: proceed

The issue is thoenissen/Reihitsu#896 (`agent-filed`). I read it through the GitHub MCP server. My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's own proposal ("last prefix link on the root line", single owner `FluentChain.GetAnchorLink`) was checked against the code and not taken as given.

## Premise

The premise is real and a caller reaches it. Whole-document formatting (`scripts/format.sh`, the CLI) reaches the case, and so does every code fix that formats a statement holding a wrapped chain (RH5201, RH5112, RH5111). The anchor is decided in one place, `FluentChain.GetAnchorLink` (`src/Reihitsu.Core/FluentChain.cs:273`). Two components read it: `MethodChainAlignmentContributor.GetAnchorColumn` (formatter, indentation phase) and `RH5201MethodChainsShouldBeAlignedAnalyzer.GetAnchorColumn`. When no invoked link comes before the first link whose `StartsLine` is true, `GetAnchorLink` returns `FirstLink`. That is the behavior the issue describes.

Observed on `main` (889975f) with `scripts/format.sh <dir>`, fixture `A.cs`. These are the relevant input lines inside `object M(dynamic order, C x, dynamic a, System.Func<int, int, C> get)`, with `C Next { get; set; }`, `C Self() => this;`, and `C[] Arr`:

```csharp
        var p1 = x.Next.Next
            .Self();
        var p4 = get(1,
            2
            ).Next.Next
            .Self();
        var p5 = new C
        {
        }.Next.Next
            .Self();
        var p9 = x!.Next.Next
            .Self();
        var p10 = x?.Next?.Next
            ?.Self();
        var p11 = System.String.Empty
            .Trim();
        var p13 = x.Next.Next.Self().Next
            .Self();
```

Output on `main`:

```csharp
        var p1 = x.Next.Next
                  .Self();
        var p4 = get(1,
                     2
                     ).Next.Next
                      .Self();
        var p5 = new C
                 {
                 }.Next.Next
                  .Self();
        var p9 = x!.Next.Next
                  .Self();
        var p10 = x?.Next?.Next
                   ?.Self();
        var p11 = System.String.Empty
                        .Trim();
        var p13 = x.Next.Next.Self()
                             .Next
                             .Self();
```

In every case without an invoked link in front of the first wrapped link, the wrapped link is under the chain's first link (`p13` aligns under its first call and is unaffected). A second pass of the formatter left the output unchanged.

`scripts/apply-fix.sh RH5201 <dir>` with the change doc's visible change 4 fixture (`D.cs`: `a` at column 17, `b` at column 22) gave `fixed (1 action)` on `b`, which it moved to column 17, under both LF and CRLF, and no diagnostic on `a`. That confirms the change doc's *before* for change 4.

This is a behavior change, not a bug: SHARED-CHAIN-007 on `main` demands today's output. Whether to change it is a matter of taste, so it is the maintainer's call through the spec author's output questions 1 and 2. I agree with recommendation A on both. The one argument against A that the maintainer should see is not new to this change: the column depends on how many prefix links the author leaves on the root line (`x.Items` ⏎ `.Count` ⏎ `.ToString()` aligns under `.Items`; `x.Items.Count` ⏎ `.ToString()` aligns under `.Count`). Calls already behave this way on `main`. Long prefixes drift further right.

Spec conflicts I checked:
- No requirement with a good `Why:` stands against the change except the one being changed.
- SHARED-DIAG-010 / FC-013 hold by construction. The formatter and RH5201 read the same `GetAnchorLink`, and the formatter reads it on the tree after the line-break phase, so formatted output is measured against the anchor it was laid out to. No phase predicts the anchor.
- P11 is not engaged.

## Mechanism and candidates

_N/A_ (behavior change). The dispatch for the anchor is `GetAnchorLink`. Its candidate inputs vary along these axes:

1. Links in front of the first link that starts a line:
   - none (the first link starts a line);
   - one;
   - two or more.
2. Kind of the last such link:
   - `.`;
   - `?.` (member binding of a conditional access, including a nested conditional access);
   - `!.`;
   - a link behind an attached part (`x.Arr[0].Next`, `x?[0, 1].Next`).
3. Root shape:
   - an identifier;
   - `this`;
   - a namespace-qualified name (`System.String.Empty`);
   - a multi-line invocation (`get(1,` ⏎ `2` ⏎ `)`);
   - an object creation with an initializer whose `}` starts a line (the rebase path in `MethodChainAlignmentContributor.GetChainAnchorColumn`, `:73`);
   - a multi-line raw string literal.
4. Whether a later call exists (output question 2).
5. Trivia in front of the first wrapped link:
   - none;
   - an end-of-line comment;
   - a directive;
   - an own-line `/* */` spanning lines;
   - a `/* */` that starts behind code and spans lines (see *Output questions*);
   - a tab in front of a mid-line `/** */`.
6. Entry point:
   - a whole document;
   - a node-level code fix (RH5201, RH5112, RH5111);
   - a chain that starts mid-line (`var r=a…`, SHARED-CHAIN-009).

## Breaking inputs

These failure classes reach the change:

| Class | What to check |
|---|---|
| FC-013 | RH5201 must be silent on formatter output for every candidate below. |
| FC-010 / FC-017 | Two formatting passes, LF and CRLF. |
| FC-007 | The RH5201, RH5112, and RH5111 fixes must converge in one iteration with the new columns. |
| FC-028 | `FormatNode` and the document-scoped entry points agree. This includes the tab-matrix row in `failure-classes.md` with a prefix anchor; see candidate 9. |
| FC-021 | A `/* */` that ends mid-line in front of a prefix link. |
| FC-020 | A directive between prefix links and the first wrapped link. |
| FC-022 | A raw-string root. |
| FC-027 | A chain inside a lambda body, an argument, or an initializer. |
| FC-003 | The boundary between "one link in front" (unchanged) and "two links in front" (changed). Test both sides. |
| FC-031 | The XML summary of `GetAnchorLink` (`FluentChain.cs:267-272`) states the old rule and must change with it. |

I ran these candidates on `main` (fixtures above, plus the ones quoted here). Expected outputs under the change follow SHARED-CHAIN-007 as written; the test implementer derives the exact columns.

| # | Input | `main` | After the change |
|---|---|---|---|
| 1 | `p1` (two `.` links) | col 18 | under the second `.Next` |
| 2 | `p4` (multi-line invocation root) | col 22 | under the second `.Next` on the `).Next.Next` line |
| 3 | `p5` (initializer root) | col 18 | under the second `.Next`, which exercises the brace-offset rebase with an anchor that is not the first dot |
| 4 | `p9` | anchor `!` of `!.` | under the second `.Next` |
| 5 | `p10` | anchor first `?` | under the second `?` |
| 6 | `p11` | under `.String` | under `.Empty` |
| 7 | `p13` | | must stay under the first call `.Self` (unchanged) |
| 8 | `x?[0, 1].Next` ⏎ `.Self()` | `.Self()` under `.Next` at col 25 | unchanged: one link |
| 9 | Tab before a mid-line doc comment in front of a prefix (see below) | `.G()` at col 29 under the first `.B` | under the second `.B` (col 31), through whole-document formatting and the node-level entry points |
| 10 | The own-line `/* */` case `p7`, which is SHARED-CHAIN-005 example `r` with a prefix | `.Next` is broken behind `*/`, everything under the first `.Next` (col 18) | must stay unchanged: the link behind `*/` has an end-of-line in front of the comment, so it counts as wrapped, and the anchor stays the first `.Next` |

Fixture for candidate 9 (`F.cs`; the input holds a real tab, written with `printf`):

```
        var x = a<TAB>/** c */.B.B
            .G();
```

On `main` the tab became four spaces.

Also cover a chain in an expression-bodied lambda: `System.Action a = () => x.Next.Next` ⏎ `.Self();`. On `main`, `.Self()` is at col 33, under the first `.Next`.

A directive between the prefix and the wrapped link (`p12`: `x.Next.Next` ⏎ `#if DEBUG` ⏎ `.Self()` ⏎ `#endif` ⏎ `.Self()`) gives the same anchor whether or not `DEBUG` is defined, because both builds see `.Next.Next` on the root line.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `FluentChain.GetAnchorLink` | Must change, together with its XML summary. It is the only owner of the decision. |
| Formatter `MethodChainAlignmentContributor` | Must stay. It reads `GetAnchorLink`, and its initializer rebase already handles an anchor that is a later link on the first dot's line (its summary says so). Its summary needs only an FC-031 check. |
| Formatter line-break phase (`ChainLineBreakRewriter`) | Must stay. Which links start a line does not change. |
| RH5201 analyzer | Must stay. It reads `GetAnchorLink`. |
| RH5201, RH5112, RH5111 code fixes and Fix All | Must stay. They delegate to the formatter. Their tests' expected columns change. |
| RH5048 | Not involved. |
| Spec SHARED-CHAIN-007 | Changed. Needs a wording fix; see *Output questions* question 1. |
| Spec RH5112-003 | **Must change; the spec author missed it.** Its normative text says the fix "joins the first link to the root's line and aligns the later wrapped links under it" (`sdd/specs/analyzer/RH5112.md:74`). After the change, the fix of `x` ⏎ `.Items.Length` ⏎ `.ToString()` (change doc, visible change 5) aligns `.ToString()` under `.Length`, not under the first link. The text should point to SHARED-CHAIN-007 instead. |
| Spec RH5201-002 `Why:` | Should change. It reads "a column of steps under the first one (SHARED-CHAIN-007)" (`sdd/specs/analyzer/RH5201.md:56`), which is stale after the change. |
| Spec SHARED-CHAIN-009 example caption ("aligns the later link under the first one") | Stays. Its chain's first link `.B()` is invoked. |
| Rule page RH5201 | Changed. The other chain rule pages (RH5112, RH5111, RH5048) do not mention alignment; I checked them with `rg`. |
| Tests | The spec author's list of tests that pin today's anchor holds. |

## Refuse option

Keep `main`'s anchor (option B of output questions 1 and 2): no production change, and the issue is linked with `Refs #896`.

The requested change is small: a few lines in `GetAnchorLink` and its summary, with no new type and no new mechanism, plus the reformatting of the repository's own sources that the spec author lists. That is cheap enough that refusing is only a matter of taste, which the output questions already decide. The input class is not rare: a call wrapped behind two or more property reads on the root line, such as `this.Settings.Items` ⏎ `.Where(…)`.

Declaring a shape unsupported is not an option here. Under the recommended reading below, no shape costs the change a special case: `GetAnchorLink` keeps using `FluentChainLink.StartsLine`.

There is no simpler option.

## Output questions

**1 · Formatter and RH5201 · Method chains should be aligned — a property prefix whose last link stands behind a comment that spans lines**

This is a rare case inside #896. The spec text as written reads two ways. SHARED-CHAIN-007 now says a link starts a line "when a line break separates the first token of its operator from the token in front of it, whatever trivia carries that line break".

- **Code reading.** `FluentChainLink.StartsLine` → `SyntaxTokenPositionUtilities.IsFirstOnLine` → `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`/`EndsLine`. A line break inside the text of a `/* */` comment does not count.
- **Literal reading.** The `/* */` comment "carries" a line break, so the link behind its `*/` would count as starting a line.

The two readings give different output when the comment starts behind code.

The change doc's sentence "a link behind the closing `*/` of a multi-line comment therefore counts as starting a line for the anchor" is true only when an end-of-line stands in front of the comment (SHARED-CHAIN-005 example `r`, candidate 10).

Input:
```csharp
var q2 = x.Next /* c
    */ .Next
          .Self();
```

Before (`main`, `scripts/format.sh`; observed inside a method body, shown here without its indentation):
```csharp
var q2 = x.Next /* c
    */.Next
          .Self();
```

After, A (recommended, the code reading). `.Self()` goes under the `.` of the `.Next` behind `*/`:
```csharp
var q2 = x.Next /* c
    */.Next
      .Self();
```

After, B (the literal reading): unchanged from `main`, with `.Self()` under the first `.Next`.

Why A: it is how `main` already lays out the same shape with a call in place of the second `.Next`. With `q1 = x.Next /* c` ⏎ `*/ .Self()` ⏎ `.Self()`, `main` puts the second `.Self()` under the `.` of `*/.Self()`; I observed this with both `scripts/format.sh` and `scripts/apply-fix.sh RH5201`. B would need a second "starts a line" predicate next to `StartsLine`, which is FC-015.

Either way, the spec text and the change doc must say which reading holds. For A, the wording would be "an end-of-line in the trivia between them; a line break inside the text of a delimited comment does not count".

## Internal choices

- **Anchor statement.** I agree with the spec author: "the last link in front of the first link that starts a line" rather than the issue's "the last prefix link on the root line". The two agree on every formatted shape. The spec author's statement is local to the links and does not depend on where the root ends.
- **Precedence of the first invoked link.** I agree to keep it. Without it, RH5201 would measure an unformatted `items.Where(…).Select(…)` ⏎ `.ToList()` against `.Select` while the formatter writes `.ToList()` under `.Where`. RH5201 still reports `.Select` for sharing a line, but it would also report the `.ToList()` column against an anchor the formatter never uses.
- **RH5112-003 wording.** Delegate the column to SHARED-CHAIN-007 instead of restating it ("under it"). This avoids a second statement of the rule.

## Issue goals

- "Only the anchor selection changes; which links start a line is unchanged": keep. `ChainLineBreakRewriter` does not read the anchor.
- "Chains whose first call is on the root line keep their current anchor": keep. This is the first-invoked-link precedence.
- "One pass; LF and CRLF identical": keep. It is a test obligation.
- "Anchor under the operator of the last prefix link on the root line": change the statement to the spec author's "last link in front of the first link that starts a line". It gives the same output for every formatted chain, so no visible output changes.
- "Single owner `FluentChain.GetAnchorLink`": a prescribed solution, but it states today's code accurately. Keep it.
- "The decision on call-less chains belongs to this issue": asked as the spec author's output question 2.

## Architecture findings

_None._ The decision has one owner, read by both surfaces, and nothing predicts it.

## Follow-up candidates

- **Pre-existing deviation from FMT-BREAK-019 and the SHARED-CHAIN-005 exception (SHARED-COMMENT-012).** This is outside the run's issue in its own right, although this change extends its reach to prefix links (output question 1).
  - What happens: the formatter does not break the line in front of a prefix link or a first invoked link whose operator stands behind the closing `*/` of a comment that spans lines, when that comment starts behind code on the root's line.
  - Where: `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks` decides `isWrappedByUser` from `link.StartsLine`, which is false there (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs:151`).
  - Observed on `main` with `scripts/format.sh`: input `var q1 = x.Next /* c` ⏎ `    */ .Self()` ⏎ `    .Self();` gives `*/.Self()` with the second `.Self()` under it. FMT-BREAK-019 asks for a line break behind `*/` and `.Self()` under `.Next`. `q2` shows the same for a prefix link.
  - The shape is rare.

  As an output question for the follow-up issue:

  **Formatter · chain link behind a comment that spans lines and starts behind code**

  Input:
  ```csharp
  var q1 = x.Next /* c
      */ .Self()
      .Self();
  ```

  Before (`main`):
  ```csharp
  var q1 = x.Next /* c
      */.Self()
        .Self();
  ```

  After, A (recommended, as FMT-BREAK-019 states):
  ```csharp
  var q1 = x.Next /* c
      */
            .Self()
            .Self();
  ```

  After, B: unchanged, and FMT-BREAK-019 and SHARED-CHAIN-005 are narrowed to a comment that starts its own line.

  Why A: it is what the specs already demand.
