Role: spec checker
Commit: d8841c3
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/documentation-rules-reported-elements.md` is Accepted, has `Visible output changes`, and records Q1 C, Q2 A, Q4 B under `Decisions`. The other changes (RH8301-RH8303 behind-code, RH8401/RH8402 mode None, RH8032, RH8302/FMT-BLANK-010) are clarifications of unchanged SHARED-DOC-004/-008, SHARED-DIAG-007 and SHARED-BLANK-005 and need none.
2. Specs match the diff: ok. RH8303-006, RH8401-006 and RH8402-007 match the code-fix diffs; RH8307-001/-002 match the nested-element change; RH8307-006 is retired in place; RH8201/RH8203 per-part wording matches SHARED-DIAG-004's second paragraph; the RH8306 and RH8106 per-element counts match.
3. IDs: ok. Added RH8303-006, RH8401-006 and RH8402-007 sit at their sorted positions in `requirement-ids.txt`; no ID was removed, reused or renumbered.
4. Code owners: ok. No owner header changed; `sdd/specs/README.md` rows match; `SpecConsistencyTests` is part of the architecture run below.
5. User documentation: ok. Only `documentation/rules/RH8307.md` changed. It names top-level elements, has no formatter mention, and keeps the footer. The pages for RH8303, RH8106, RH8201, RH8203 and RH8401 state nothing the clarifications contradict.
6. Open questions: ok. The batch 1 output questions (Q1 to Q4) are answered under `Maintainer answers`; the spec author's and skeptic's remaining candidates are under `Follow-up candidates` in `00-lead.md`; none is written into a spec.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 117, Skipped: 0, Total: 117`. RH8303-006, RH8401-006 and RH8402-007 each have covering `[Requirement]` tests.
8. Test tags: ok. The tests added carry the tags in 06's tables (RH8032-001, RH8106-001/-002, RH8201-001/-004/-005, RH8203-001, RH8301 to RH8303, RH8305, RH8306-001/-004, RH8307-001/-002, RH8401-006, RH8402-007, FMT-BLANK-010). The two untagged added tests (RH8002 characterization, RH8203 issue-input characterization) are listed as "none" there and verify no requirement.
9. Nothing left behind: ok. `sdd/work/` holds only `doc-rule-fixes`.
