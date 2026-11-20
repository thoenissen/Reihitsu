Role: spec author
Commit: f4ca0cf
Verdict: spec written

This is an amendment. It acts on the skeptic's findings (`02-skeptic.md`) and on the maintainer's answers 1A, 2A and 3A (`00-lead.md`, *Maintainer answers*). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Files changed:

- `/home/user/Reihitsu/sdd/specs/shared/fluent-chain-layout.md` (SHARED-CHAIN-007)
- `/home/user/Reihitsu/sdd/specs/analyzer/RH5112.md` (RH5112-003, `Uses` row)
- `/home/user/Reihitsu/sdd/specs/analyzer/RH5201.md` (RH5201-002 `Why:`)
- `/home/user/Reihitsu/sdd/changes/2026-11/prefix-chain-anchor.md`: `Status: Accepted`. The *Affected requirements* row, the *What* sentence on "starts a line", and visible change 3 (case `q`) were updated. *Decisions* now records the three answers, and the "Starts a line" decision was rewritten.

No requirement ID was added, so `requirement-ids.txt` is unchanged. `scripts/test.sh --project architecture`: 198 passed.

## Changed requirements

- **SHARED-CHAIN-007 — Wrapped links align to the anchor link — clarified (answer 3A).**
  - "Starts a line" now means a line break that SHARED-BLANK-008 counts, standing in the trivia between the first token of the link's operator and the token in front of it: an end-of-line trivia, or the line break that ends a `///` comment, a directive, or disabled text.
  - A line break inside the text of a delimited comment does not count.
  - Both sides of the qualifier are covered:
    - A comment that starts its own line: the link behind `*/` starts a line (SHARED-CHAIN-005 example `r`, referenced).
    - A comment that starts behind code: the link behind `*/` does not start a line and can be the anchor (inline example `x.Next /* c` ⏎ `*/ .Next` ⏎ `.Self()`).
  - This matches `FluentChainLink.StartsLine` → `SyntaxTokenPositionUtilities.IsFirstOnLine` → `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`/`EndsLine`.
  - `Why:` gained one sentence on the counting.
- **RH5112-003 — The fix lays out the chain — changed (skeptic's finding).**
  - The normative text no longer says "aligns the later wrapped links under it". It now says they align "to the chain's anchor (SHARED-CHAIN-007)".
  - A second example was added: the change doc's visible change 5 RH5112 case, `x` ⏎ `.Items.Length` ⏎ `.ToString()`, with `.ToString()` under `.Length`.
  - `SHARED-CHAIN-007` was added to the spec's `Uses` row.
- **RH5201-002 — A misaligned wrapped link is reported — `Why:` changed (skeptic's finding).** "under the first one" became "under the one they continue". The normative text is unchanged.

## Examples for the tests

- **RH5112-003, second example.** I checked it on `main` with `scripts/apply-fix.sh RH5112`: it compiles, is fixed in one iteration under LF and CRLF, and the result differs from the example only in the column of `.ToString()`. `main` puts it at column 17, under `.Items`; the example puts it at column 23, under `.Length`.
- **SHARED-CHAIN-007 inline example (`x.Next /* c` ⏎ `*/ .Next` ⏎ `.Self()`) and change doc visible change 3, case `q`.**
  - The *before* was checked with `scripts/format.sh` on a fixture inside a method: `main` writes `*/.Next` and puts `.Self()` under the first `.Next`, at column 17.
  - The *after* puts `.Self()` under the `.` of `*/.Next`, at column 14 in the method fixture and column 6 in the change doc's snippet.
  - On that *after* layout, `scripts/apply-fix.sh RH5201` on `main` reports `.Self()` and moves it back to column 17. After the change it must not report it.
- The seeds from `01-spec-author.md` are unchanged.

## Output questions

_None._

## Internal choices

- **Where the q2 shape stands.**
  - The task asked for the skeptic's `q2` shape as an example. I put it in the normative paragraph of SHARED-CHAIN-007 as an inline anchor example, and in the change doc as visible change 3, case `q`, with real *before* and *after* layouts.
  - I did not put it in SHARED-CHAIN-007's formatted example block. That block would assert `*/.Next` as intended formatter output. SHARED-CHAIN-005 (second paragraph) and FMT-BREAK-019, both unchanged, demand a line break in front of that `.Next` instead.
  - The formatter's `*/.Next` join is the pre-existing deviation the lead dropped (`00-lead.md`, decision 1). The role file forbids specifying it as intended.
  - The inline example states only the anchor, not the line layout.
  - The alternative was to add case `h` to the formatted block, which would write that deviation into the spec.
  - The tests the test implementer derives from case `q` will pin `main`'s `*/.Next` line together with the new column. The lead decides whether that is acceptable, given decision 1. The maintainer's answer 3A showed exactly that output.
- **RH5112-003 wording.** It delegates the column to SHARED-CHAIN-007 instead of restating the rule, as the skeptic recommended. The alternative was "under the last link in front of the first wrapped link", which would be a second statement of the rule.
- **RH5112-003 example.** I added a second example so the test implementer has a spec seed for the new column. The alternative was to leave the example only in the change doc.
- **Rule pages.** Unchanged in this amendment. RH5112's page does not mention alignment.

## Follow-up candidates

_None._ The FMT-BREAK-019 / SHARED-CHAIN-005 deviation for a link behind the `*/` of a comment that starts behind code is already decided as not filed (`00-lead.md`, decision 1).
