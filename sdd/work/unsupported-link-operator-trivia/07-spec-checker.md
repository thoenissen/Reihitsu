Role: spec checker
Commit: ca70d84
Verdict: consistent

My prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

_None._

## Checked
1. Change doc: ok. `sdd/changes/2026-11/unsupported-link-operator-trivia.md` exists for a new requirement (FMT-UNSUP-015) and changed ones, has `Visible output changes`, `Special cases` (Deleted / Checked and kept), and `Decisions`, and is `Status: Accepted`. The maintainer's answers (triage; batch 1: 1A, 2A) are recorded under `Decisions`.
2. Specs match the diff: ok. FMT-UNSUP-015 has the normative sentence first, `Why:`, and examples; FMT-BREAK-019, RH2001-007, RH7101-007 and the AGENTS.md precheck row name the shape. The deletions the change doc lists (`AlignRestOfRun`, the trivia test on inner operator gaps) match the code diff. The `?[` gaps stay with FMT-UNSUP-014.
3. IDs: ok. FMT-UNSUP-015 is the next free number and sits at its sorted position in `requirement-ids.txt`; nothing renumbered or removed.
4. Code owners: ok. The spec headers name no new type; the new `FluentChain` helper is a member, not an owner. The `Uses` lists of unsupported-syntax, line-breaks, RH2001 and RH7101 are extended. The architecture run below includes `SpecConsistencyTests`.
5. User documentation: ok. No rule page is affected: no rule is added, and the withheld fixes are covered by `general-notes.md`.
6. Open questions: ok. The two output questions of the spec author have answers under `Maintainer answers` (1A, 2A); the 2A follow-up is under `Follow-up candidates`; none sits in a spec or only in the PR.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198`. FMT-UNSUP-015 is covered by 44 `[Requirement("FMT-UNSUP-015")]` tests; FMT-BREAK-019, RH2001-007 and RH7101-007 are tagged on the tests the diff adds.
8. Tags: ok. The tests the test implementer's table lists carry their requirement (FMT-UNSUP-015, -001, -003, -004, -005, -010, FMT-BREAK-019, RH2001-007, RH7101-007, SHARED-FIX-010), and the spec author's example requirements are tagged on at least one new test.
9. Leftovers: ok. `sdd/work/` holds only this run's directory (the lead deletes it before publishing).

## Notes (not findings)
- Skeptic's wording point on FMT-UNSUP-015's overlap sentence ("the two meet only outside a value"): not vacuous. FMT-UNSUP-004's span includes a field's attributes, and a link operator can stand in an attribute argument of a variable declaration (`[A(nameof(s?.Length))] int f;`) outside any initializer value, which the syntactic precheck sees. The sentence is true as written. The next clause ("a link operator of a value is always inside it") is correct for the value but could be misread as saying no link operator stands elsewhere; this is optional wording, not a mismatch with the diff.
