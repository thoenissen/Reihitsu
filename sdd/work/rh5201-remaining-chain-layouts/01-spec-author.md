Role: spec author
Commit: d63eab4 (main at claim; branch head 7de2823 adds only run files)
Verdict: change doc written

Change doc: `sdd/changes/2026-11/rh5201-first-call-behind-broken-root-line.md` (Status: Draft). Its *Visible output changes* 1-7 list every change, each with input and `main` output produced by `scripts/apply-fix.sh RH5201` / `scripts/format.sh`; the formatter's output does not change, and no code fix other than RH5201's changes.

## Changed requirements

- SHARED-CHAIN-005 — In a wrapped chain, each later call starts its own line — changed: new paragraph stating when the first invoked link must start its own line (the root's line does not reach it: a line break, counted as SHARED-CHAIN-007 counts one, in front of or inside the operator of a link between the first link and the first invoked link, or SHARED-CHAIN-004 keeps the first link below the root; a line break in front of the first link that SHARED-CHAIN-003 removes, or inside the first link's operator, does not count); `Why:` extended; example added (`a`, `b` broken; `c` the other side). This writes down what `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks` (`isRootLineIntact`) does today.
- FMT-BREAK-019 — Chains follow the shared chain layout — clarified: the summary names the first call the formatter breaks. Formatter behavior unchanged.
- RH5201-006 — A first call the root's line does not reach is reported — added (ID registered in `sdd/specs/requirement-ids.txt`).
- RH5201 non-goals — added: the column of an attached part, or of a link operator's tokens behind its first, that starts a line (refused by FMT-UNSUP-014 / FMT-UNSUP-015, or joined).
- `documentation/rules/RH5201.md` — one bullet under *Description* and matching words under *How to fix it*. This is a judgment call (see Internal choices).

## Examples for the tests

- RH5201-006 — both examples (analyzer markup), and the fix results given under them (fix = formatter layout, RH5201-005).
- SHARED-CHAIN-005 — the new example (`a`, `b`, `c`) is a formatter whole-document example; it already passes on `main` (checked with `scripts/format.sh`), so it is a characterization test for the formatter.
- `scripts/test.sh --project architecture` fails only with `RH5201-006: no test covers it` — expected until the test implementer tags a test.

Checked on `main`: every *before* in the change doc; every *after* fix result equals today's `scripts/format.sh` output for the same input; inputs `A`, `Cc2`, `D2`, `D3` (comment-wrapped prefix) give `no-diagnostic` today; `e` (`c` ⏎ `.Next.Self()`) is joined to one line by the formatter and must stay unreported by RH5201.

## Output questions

_None._ No real output looks wrong. The visible changes 1-7 of the change doc are new diagnostics (and one rule-page edit) for the lead to turn into output questions.

## Internal choices

- The rule is stated in SHARED-CHAIN-005 (both surfaces) rather than only in RH5201, since the formatter already does it and no spec said so. Alternative: an RH5201-only requirement, leaving the formatter behavior unspecified.
- A new requirement RH5201-006 rather than widening RH5201-001, so the existing tests of -001 keep their meaning. Alternative: extend -001.
- The rule page is updated, because the case is new reported behavior in the normal flow of a wrapped chain and the page's first bullet ("every member access after the first call") would not explain a diagnostic on the first call. Alternative: leave the page alone as a rare case — say so if the lead prefers that.
- Issue goal 2 (columns of trivia-kept attached parts and operator rests) is dropped: no caller reaches it. The issue's own example `a.B() // keep` ⏎ `[0]` is refused by the CLI today (`unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator`), as is `a.B()? // keep` ⏎ `.C()` (FMT-UNSUP-015). The issue is `agent-filed`, so this is a goal adjustment to state in the PR; the PR can still `Closes #893` since goal 1 is implemented.
- Issue's "Preferred compliant example" for case 2 (`[0]` aligned under the chain) is not adopted for the same reason.

## Follow-up candidates

_None._ (Checked and not raised: `c.Next /* x` ⏎ `*/ .Self()` keeps the first call behind `*/` — consistent with SHARED-COMMENT-012's rule for a token behind code on the comment's first line.)
