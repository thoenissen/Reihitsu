Role: spec author
Commit: a62125c
Verdict: change doc written

Change doc: `sdd/changes/2026-11/rh5102-arguments-on-one-line.md` (Status: Draft). It records the maintainer's answer A on #1755 and mirrors `rh5109-parameters-on-one-line.md`.

## Changed requirements

- RH5102-001 — Arguments sharing a line in a multi-line list are reported — changed: the arguments alone decide whether the list is multi-line (two arguments start on different lines, or an argument spans lines); a list whose arguments all start on one line, none spanning lines, is not reported, whatever line that is, with `)` on a later line (also behind a comment), or with a comment or directive holding the first argument off the `(` line. No new requirement ID; `requirement-ids.txt` unchanged.
- FMT-BREAK-012 — A list is on one line or one element per line — Why reworded only: it justified the held-first-element layout by "RH5102 reports [the other layout]", which is no longer true. Normative text and behavior unchanged.
- RH5102-002, -003, -004 — unchanged; every example in them is still reported under the new reading (checked).
- Rule page `documentation/rules/RH5102.md` — left unchanged: it states the rule by the arguments ("all on a single line or each on its own line") and never by the parentheses, so it reads correctly under the new reading. Unlike RH5109's page, it has no sentence to correct.

## Examples for the tests

- RH5102-001: the reported block (new: `M(⏎1, 2,⏎3)` and `M(⏎1, () =>⏎{⏎})` stay reported) and the not-reported block (`M(⏎1, 2)`, `M(1, 2⏎)`, `M( // c⏎1, 2)`, `M(⏎#pragma …⏎1, 2)`, `M(1, 2 // c⏎)`).
- The issue's "Done when" (and `00-lead.md`) calls `M(1,⏎2)` reported. It is not: every argument starts its own line, so it is not reported today and after (RH5102-001); the reported counterpart is `M(1, 2,⏎3)` / `M(⏎1, 2,⏎3)`. Change-doc entry 5 shows both. Cover `M(1,⏎2)` as not reported and `M(⏎1, 2,⏎3)` as reported.
- Existing tests that today expect a report for a list whose arguments all start on one line must flip to not reported; none was found among the RH5102 fix tests (`FixMovesLeadingCommaOfListWrappedBehindOpeningParenthesis` has arguments on two lines and stays reported), but the test implementer should grep for any.

## Output questions

The change doc's visible output changes 1 to 4 are for the lead to turn into output questions (or to record as covered by the answer). Entry 1 is exactly the maintainer's answer A. Entries 2-4 are the further shapes whose arguments all start on one line; the answer's wording ("arguments all stand on one line") covers them and RH5109 decided the same four shapes as A, so I wrote the spec on that reading. Entry 4 is the one where rule and formatter then differ (formatter splits `M( // c⏎1, 2)` one per line; RH5102 no longer reports it; P11). If the lead judges entries 2-4 not covered by the answer, they go to the maintainer with recommendation A for each.

Entry 2's *before* output (fix result for `M(1, 2⏎)`, with `)` kept on its own line) is derived from RH5102-003 and FMT-BREAK-012's non-goal, not run; the test implementer should confirm it on `main` before the question is asked.

## Internal choices

- Changed RH5102-001 in place rather than retiring it and adding a new ID, as the RH5109 change did. Alternative: retire and add RH5102-005.
- Reworded FMT-BREAK-012's Why so it no longer cites an RH5102 report. Alternative: leave the stale sentence (rejected: a spec would state something false).
- Rule page unchanged (see above). Alternative: add RH5109's sentence "A line break directly after the opening parenthesis or before the closing parenthesis alone does not make the list multi-line." Not needed for the normal case.

## Follow-up candidates

_None._

## Amendment

On the skeptic's must-corrections (`02-skeptic.md`, *Internal choices* 1) and `00-lead.md` decisions 4–7, the change doc `sdd/changes/2026-11/rh5102-arguments-on-one-line.md` now records `main` as it is. No visible output in the spec or the change doc's *after* blocks changed.

- *Why*: the claim that the fix splits `M(⏎1, 2)` and leaves two stable results is replaced by the real defects: `M(1, 2⏎)` and `M(⏎1, 2⏎)` are reported with a fix that changes nothing (SHARED-DIAG-010, SHARED-FIX-005); `M(1, 2 // c⏎)` and `M( /* a⏎b */ 1, 2)` are reported without a fix for a shape the formatter keeps (SHARED-DIAG-010); `M(⏎1, 2)` duplicates RH5101's report.
- Entry 1 *before*: the fix gives `M(1, 2);`.
- Entry 2 *before*: reported, a fix is offered that changes nothing. The "derived, not run" note is removed; the skeptic settled the entry on `main`.
- Entry 2 also names `M(⏎1, 2⏎)` (the skeptic's optional suggestion, taken).
- *Alternatives considered*, option B: rejected for the duplicate with RH5101 and the rule's title, not for "two stable results".
- RH5102-001's *Why* gains the skeptic's optional clause: reporting a line break in front of `)` left a fix that changes nothing (SHARED-DIAG-010). Taken because it names the defect the change removes.
- Not taken: nothing else. Skeptic *Internal choices* 3 (keep the P11 wording for entry 4) and 4 (line counting unstated) agree with the spec as written.
- For the tests: the skeptic's *Breaking inputs* matrix supersedes my *Examples for the tests* where it is wider (`M(⏎1, 2⏎)`, `M( /* a⏎b */ 1, 2)`, a non-invocation parent, CRLF, the spanning-argument variants behind `(⏎`).

## Amendment 2

On `00-lead.md` decision 9 (`UsesListsExactlyTheReferencedRequirements` failed): SHARED-DIAG-010 added to the `Uses` row of `sdd/specs/analyzer/RH5102.md`, which now cites it in RH5102-001's Why, and removed from the `Uses` row of `sdd/specs/formatter/line-breaks.md`, whose FMT-BREAK-012 Why no longer cites it. Metadata only, no wording changed. `scripts/test.sh --project architecture` passes.

## Amendment 3

On `06-spec-checker.md` check 1 and `00-lead.md` decision 3: `Status` of `sdd/changes/2026-11/rh5102-arguments-on-one-line.md` set from `Draft` to `Accepted`, in the form the other accepted change docs under `sdd/changes/2026-11/` use (`| Status | Accepted |`).

## Amendment 4

On `07-auditor.md` finding #1: the last sentence of *What* in `sdd/changes/2026-11/rh5102-arguments-on-one-line.md` no longer says the old fix split the list. It now says the fix, which lays the list out as the formatter does, joined it where the formatter joins it and changed nothing where the formatter keeps it, in agreement with *Why*. No other text changed.
