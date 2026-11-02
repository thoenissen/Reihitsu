Role: spec checker
Commit: 9a29c8d
Verdict: findings

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/changes/2026-11/phase-order-line-breaks.md:5` | `Status` is still `Draft`. Lead decision 16 accepts the change doc after the skeptic's amendment, and it must be `Accepted` before the PR is marked ready. The `Decisions` section already records maintainer answer 1 for visible output changes 1 to 5, so nothing else blocks acceptance. | spec author |

## Checked
1. Change doc: finding 1. It is required (phase order and visible comment placement change), has no task list, has `Visible output changes`, and records answer 1 in `Decisions` for changes 1 to 5.
2. Specs match the diff: ok. FMT-PIPE-001/024/025/026, FMT-BREAK-005 (retired in place), 006, 030, 033, 036, FMT-BLANK-004/005/018, and FMT-SWITCH-007/015/016 agree with the code and test changes. FMT-BLANK-021 does not appear anywhere (dropped, decision 12). `AGENTS.md` order sentence and the ownership rows match the specs and decisions 18 and 21.
3. IDs: ok. FMT-BREAK-036, FMT-PIPE-025/026, and FMT-SWITCH-015/016 are the next free numbers and sit in sorted position in `requirement-ids.txt`. The diff removes no ledger line.
4. Code owners: ok. `scripts/test.sh --project architecture` gives `Passed!  - Failed: 0, Passed: 92, Skipped: 0, Total: 92`, which includes `SpecConsistencyTests` and `RequirementCoverageTests`. No type moved. `BlankLineSpacingPolicy` stays with SHARED-BLANK (decision 25).
5. User documentation: ok. No file under `documentation/` is in the diff, and no analyzer report or fix changes. RH5104 is unchanged (decision 17).
6. Open questions: ok. No output question is open in any spec author or skeptic handover. The three follow-up candidates are in `00-lead.md` under `Follow-up candidates`, and none is written into a spec.
7. Coverage: ok. Every added active requirement (FMT-BREAK-036, FMT-PIPE-025, FMT-PIPE-026, FMT-SWITCH-015, FMT-SWITCH-016) has a covering `[Requirement]` test in the diff, and the architecture run above passes.
8. Test tags: ok. The tags in the diff match the `Tests` tables of 04, 06, and 09, and the FMT-BREAK-006, FMT-BLANK-018, FMT-PIPE-001, FMT-SWITCH-015, and FMT-SWITCH-016 tests carry their IDs. Every requirement under `Examples for the tests` in 01, 03, 05, and 08 is tagged on at least one added or newly tagged test. This includes FMT-SWITCH-016's second example (09-test-implementer).
9. Leftovers: ok. The only `sdd/work/` directory is this run's `phase-order-line-breaks`, which the lead deletes before publishing.
