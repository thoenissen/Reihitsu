Role: spec checker
Commit: 85aae50
Verdict: consistent

Repair round 2, delta against f179726 (13-spec-checker.md). The delta outside `sdd/work` is the FMT-UNSUP-009 first tie bullet and FMT-UNSUP-010 tie paragraph in `sdd/specs/formatter/unsupported-syntax.md` (clarifications) and `UnsupportedFieldDeclarationDirectiveTests.cs` (tests only). Checks 1 to 6 and 9 for everything older stand as in 13.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | None | - |

## Checked
- 1 Change doc: ok. `field-declaration-directives.md` is Accepted; the repair adds no behavior (decision 4: the code and `main` already give FMT-UNSUP-009's reason), so no change doc edit is needed.
- 2 Specs match the diff: ok. Both edits write down existing behavior, agree with the code per 14 and 15, and the new example is the pragma row of test (5). Format unchanged.
- 3 IDs: ok. No ID added or removed; `requirement-ids.txt` untouched since 13.
- 4 Code owners: ok. Headers untouched; `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 153, Total: 153 (includes SpecConsistencyTests).
- 5 User documentation: ok. No rule page or `AGENTS.md` change in the delta.
- 6 Open questions: ok. 15 to 17 list none; decision 4 is recorded in `00-lead.md`.
- 7 Coverage: ok. No new requirement; architecture result above.
- 8 Tags: ok. The four new or extended tests carry `[Requirement("FMT-UNSUP-010")]` with `-008` or `-009` as the Tests table in 15 names; the spec author names no new test example beyond test (5), which is tagged.
- 9 Leftovers: ok. Only `sdd/work/field-split-directives/` exists.

## thoenissen/Reihitsu#1728
consistent. Delta is the two spec clarifications and the tie tests; spec, tests, and 14's CLI evidence agree.

## thoenissen/Reihitsu#1586
consistent. The delta touches nothing of the split side.
