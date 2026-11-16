Role: spec checker
Commit: dadee73
Verdict: consistent

Repair round 1; the diff against `origin/main`, with emphasis on 7eafd21..dadee73. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
_None._

## Checked
1. Change doc: ok. `Status` stays `Accepted`; `Affected requirements` names SHARED-DOC-019 (added); the new *Decisions* section "A union whose documentation holds a parameter element is not judged" states input, before (not reported on `main`) and after (not reported), so no visible output change is added and no maintainer answer is owed; the rejected alternative is listed; no task list.
2. Specs match the diff: ok. SHARED-DOC-019 follows the format (normative sentence, `Why:`, examples, markup) and matches `IsUnionWithParameterElement`: `param`/`paramref` at any depth, every doc comment (SHARED-DOC-004/006), every part of a partial union, nested declarations judged on their own, `include` content not covered. RH8035-001, RH8036-001, RH8108-001, RH8203-001 and their `Uses` rows carry the refusal; RH8030 and RH8109 to RH8111 do not use it, and the code does not call the helper for them, as 09 states.
3. IDs: ok. SHARED-DOC-019 is inserted after SHARED-DOC-018 in `requirement-ids.txt`; nothing reused, renumbered, or removed.
4. Code owners: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118` (SpecConsistencyTests and the `Uses` check included; the earlier Uses failure is fixed).
5. User documentation: ok. No rule page changed in this round, and none needs to: the refusal is a corner case that `general-notes.md` and the pages keep out.
6. Open questions: ok. 09 and 10 have no output question; decisions 4 and 5 are recorded in `00-lead.md`; the `include` route was probed by the test implementer and is no new member of the class; the follow-up candidates are unchanged.
7. Coverage: ok. SHARED-DOC-019 is covered by tagged tests for RH8035, RH8036, RH8108, and RH8203, including the partial-union tests.
8. Tags: ok. 27 added or extended tests carry `[Requirement("SHARED-DOC-019")]` together with their rule's -001 ID; the four RH8030 and RH8109 to RH8111 tests carry only their own -001 ID, as the `Tests` tables in 10 state. Every example under `Examples for the tests` in 09 (incl. the partial-union correction) is tagged on at least one test.
9. Leftovers: ok. `sdd/work/` holds only `union-naming-documentation`.
