Role: spec checker
Commit: 5df96d7
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked
1. ok: no change doc needed. RH3004-011 and the RH3004-003/008 additions write down today's output (clarification, lane tests, no production diff); no visible output change.
2. ok: RH3004-011 has the normative sentence, `Why:`, and examples with `{|RH3004:...|}` markup; RH3004-003 names SHARED-FIX-010 and `Uses` gains FMT-INDENT-007, FMT-UNSUP-009, SHARED-FIX-010.
3. ok: RH3004-011 is the next free ID, inserted in sorted position after RH3004-010; no ID removed.
4. ok: owners unchanged and exist; `sdd/specs/README.md` row for RH3004 matches. `scripts/test.sh --project architecture`: Passed 198, Failed 0.
5. ok: rule page `documentation/rules/RH3004.md` untouched, correct as the amendments cover a withheld fix and a corner case.
6. ok: no output questions in 01, 02, 03; no open questions; follow-up candidates none.
7. ok: RH3004-011 is covered by `[Requirement("RH3004-011")]` tests (EveryRequirementOfAnEnforcedSpecIsCovered passes).
8. ok: all 14 RH3004-011 tests carry the tag (two also SHARED-FIX-010); the RH3004-008 example test carries RH3004-008; every example under `Examples for the tests` is tagged on a test the diff adds.
9. ok: the only `sdd/work/` is this run's (lead deletes before publishing).
