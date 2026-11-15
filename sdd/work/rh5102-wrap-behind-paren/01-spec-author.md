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
- The issue's "Done when": `M(⏎1, 2)` not reported (new), `M(1,⏎2)` reported... note: as written in the issue, `M(1,⏎2)` has every argument on its own line and is NOT reported, today and after; the reported counterpart is `M(1, 2,⏎3)` / `M(⏎1, 2,⏎3)`. Change-doc entry 5 shows both. Cover `M(1,⏎2)` as not reported and `M(⏎1, 2,⏎3)` as reported.
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
