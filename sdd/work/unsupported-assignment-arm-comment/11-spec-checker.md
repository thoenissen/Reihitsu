Role: spec checker
Commit: 368c272
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

## Checked
1. Change doc: ok. `sdd/changes/2026-11/assignment-and-arm-comment-unsupported.md` exists, is Accepted, has `Visible output changes`, and every visible change is covered by maintainer answers in `00-lead.md` (escalation 10, batch 1, batch 2, decision 7). It has no task list.
2. Specs match the diff: ok. The diff touches only `PrecheckPhase` and `LineBreakAssignmentRewriter` in `src`. FMT-UNSUP-020/021/022 state the new shapes; FMT-UNSUP-019 is retired in place; the FMT-BREAK, FMT-INDENT, FMT-BLANK, FMT-PIPE, RH2001, RH7101 and SHARED-FIX texts follow the new behavior. No stale reference to 019 remains outside the change docs and the retired entry.
3. IDs: ok. `requirement-ids.txt` adds FMT-UNSUP-020, 021, 022 at their sorted positions; 019 stays in the ledger.
4. Code owners: ok. Architecture project: `Passed! Failed: 0, Passed: 198, Skipped: 0, Total: 198` (includes SpecConsistencyTests). The diff changes no spec header owner.
5. User documentation: ok. No rule page is affected; the diff touches none, and `AGENTS.md`'s PrecheckPhase row lists 020, 021, and 022.
6. Open questions: ok. No output question is open. The batch 1 and batch 2 answers and decision 7 are in `00-lead.md`, and the `int P { get; } /** c */ = 1;` candidate is under Follow-up candidates.
7. Coverage: ok. The architecture run is quoted above. FMT-UNSUP-020 (29 tags in `UnsupportedAssignmentCommentTests`, plus others), FMT-UNSUP-021 (12 in `UnsupportedSwitchArmExpressionCommentTests`), and FMT-UNSUP-022 (13 in `UnsupportedNameEqualsCommentTests`) each have covering `[Requirement]` tests.
8. Tags: ok. Spot checks against the `Tests` table of `06-test-implementer.md` and `09-test-implementer.md` show matching tags. The spec author's examples-for-tests items, the other-side tests, the FMT-UNSUP-003 clause tests, and the FMT-BREAK-030 test (moved, tagged) are all present.
9. Nothing left behind: ok for now. `sdd/work/` holds only this run's directory, which the lead deletes before publishing. This is not a finding.
