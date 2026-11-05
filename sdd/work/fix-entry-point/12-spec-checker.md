Role: spec checker
Commit: b30a2c2e
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

Note for the lead (not a finding): `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md` is still `Status: Draft`. It has to be `Accepted` before the PR is marked ready for review; its `Decisions` section already records every maintainer answer.

## Checked
1. Change doc: ok. Required (requirements FMT-PIPE-005/006/009 changed, FMT-PIPE-029/030/031 added, RH7101-002 changed); no task list; has `Visible output changes`; every visible change has its answer under `Decisions`, matching `00-lead.md`. Status Draft (see note).
2. Specs match the diff: ok. RH7101, FMT-PIPE, FMT-STRUCT-026, FMT-USING scope and non-goal state the changed behavior; format (one heading, `Why:`, examples, markup) followed. No other spec still says the fix runs the split or the phase directly (grep).
3. IDs: ok. FMT-PIPE-029/030/031 are the next free numbers, inserted in sorted position in `requirement-ids.txt`; none reused or removed. No RH7101 ID added.
4. Code owners: ok. No owners renamed, added or removed; `sdd/specs/README.md` unchanged and consistent. `scripts/test.sh --project architecture`: Passed! Failed 0, Passed 100, Total 100 (includes SpecConsistencyTests).
5. User documentation: ok. `documentation/rules/RH7101.md` is unchanged; its Correction equals the fix output (plain case unchanged); no formatter mention, no corner-case note.
6. Open questions: ok. Every output question has its answer under `Maintainer answers`; follow-up candidates are listed under `Follow-up candidates`; none written only into the spec or the PR.
7. Coverage: ok. Tests in the diff carry `[Requirement]` for FMT-PIPE-005, -006, -009, -029, -030, -031, FMT-STRUCT-026 (and -022), RH7101-001, -002, -004, RH7203-004, SHARED-JOIN-002; RH7101-005 stays covered by existing tagged tests. Every new requirement (FMT-PIPE-029/030/031) has covering tests.
8. Tags: ok. The tables of 06/07/09 match the tags in the diff; the spec author's "Examples for the tests" requirements (FMT-PIPE-029, -030, -009) are tagged on tests the diff adds.
9. Leftovers: ok. Only `sdd/work/fix-entry-point/` exists under `sdd/work/`; the lead deletes it before publishing.
