Role: spec checker
Commit: f47b1d8
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/changes/2026-11/attribute-list-structural.md:5` | Status is `Draft`, but lead decision 12 records the change doc as accepted at `bfa84b7`. Every visible output change has its answer (change 1 is demanded by FMT-PIPE-004/FMT-SPACE-004, change 2 carries the maintainer's answer 1 in the doc), so it is ready to be set to `Accepted` (sdd/README.md: the lead sets it, before the PR is marked ready). | spec author (the lead sets it in a squad run) |

## Checked
1. Change doc: required (requirements added, FMT-BREAK-027/031 changed), has `Visible output changes` and no task list; status finding 1.
2. Specs match the diff: ok (FMT-STRUCT-028/029, FMT-BREAK-032 added, FMT-BREAK-027/031 and SHARED-ATTR-013 updated, with Why and examples).
3. IDs: ok (FMT-BREAK-032, FMT-STRUCT-028, FMT-STRUCT-029 inserted sorted; no ledger line removed; file sorted).
4. Code owners: ok; sdd/specs/README.md matches the spec headers (AttributeListSplitTransform, AttributeListMergeTransform, AttributeListShapeUtilities, AttributeListMergeSeparators). `scripts/test.sh --project architecture`: Passed 92, Failed 0.
5. User documentation: ok; no rule page touched, none affected (code-fix output unchanged).
6. Open questions: ok; the one output question has maintainer answer 1; follow-ups 1-7 are under Follow-up candidates.
7. Coverage: ok; architecture result above includes EveryRequirementOfAnEnforcedSpecIsCovered; each new requirement has tagged tests.
8. Tags: ok; every test in the table carries its requirement. Untagged characterization tests verify no requirement.
9. Leftovers: ok; the only sdd/work directory is this run's (lead deletes it before publishing).
