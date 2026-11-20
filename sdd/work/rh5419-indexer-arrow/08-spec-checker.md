Role: spec checker
Commit: cf7479c
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 5 / 2 | `Release.ruleset` | Every shipped rule is listed there (`RH5418` at line 278, `RH5401` at line 262); `RH5419` is missing. The diff does not touch the file. No architecture test enforces it, so it passes silently. | implementer |
| 2 | 1 / 6 | change doc item 5 and Decisions "A block comment inside a parameter list on one line does not withhold the fix" | The change doc is `Accepted` and states this decision, but the maintainer has not answered it. The answer is not under `Maintainer answers` in `00-lead.md`, where it is still the open batch-2 question from `03-spec-author.md`. It is correctly routed (open for the lead's batch before the auditor), but the Decisions entry must not stay as decided before the answer. Not blocking now; after the answer, the spec author records it, or flips RH5419-004 and the tests on B. | lead / spec author |

## Checked
1. Change doc: required (new rule and behavior), `Accepted`, no task list, has `Visible output changes`; Q1 A and Q2 follow-up D are recorded in Decisions. Item 5 is open, see finding 2.
2. Specs match the diff: RH5419-001 to -004 follow the format (`Why:`, examples, `{|RH5419:…|}` markup). The family pointers (RH5107, RH5108, RH5109, RH5401, SHARED-GETONLY, FMT-BREAK-026 `Why:`) and the SHARED-DIAG-010 exception are consistent with the code. The ruleset omission is finding 1.
3. IDs: RH5419-001 to -004 follow RH5418-001, sorted, none reused or removed.
4. Code owners: `scripts/test.sh --project architecture` gives `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198`. This covers `SpecConsistencyTests`. `sdd/specs/README.md` has the RH5419 row with matching owners.
5. User documentation: `documentation/rules/RH5419.md` is short, has no formatter mention, no corner-case notes, has the footer, and its Correction equals the fix's output for the Violation. Category Layout fits RH54##. The package README row is present. Resx title and message are present. Finding 1 (`Release.ruleset`).
6. Open questions: the block-comment question is open for the lead's batch 2 (finding 2). The formatter follow-up is under `Follow-up candidates` in `00-lead.md`. Nothing is recorded only in the PR.
7. Requirements covered: the architecture run above includes `EveryRequirementOfAnEnforcedSpecIsCovered`, which passes. RH5419-001 to -004 each have tagged tests in the diff.
8. Tags: the analyzer tests carry `RH5419-001` (17), `-002` (2), `-003` (19), `-004` (17), plus SHARED-FIX-006, -007, -010 and -011. The formatter tests carry `RH5419-001` and `SHARED-DIAG-010`. Every requirement the spec author named under `Examples for the tests` (RH5419-001 to -004) is tagged on a test the diff adds. No mismatch found.
9. Nothing left behind: `sdd/work/` holds only `rh5419-indexer-arrow`.
