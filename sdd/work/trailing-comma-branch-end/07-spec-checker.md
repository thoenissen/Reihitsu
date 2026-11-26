Role: spec checker
Commit: 3e033ff
Verdict: findings

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 (minor) | `sdd/specs/analyzer/RH5409.md`, `RH5410.md`, `RH5411.md`: `Uses` row and the intro sentence "offers a code fix that removes it as SHARED-TRAIL-003 specifies" | The offer and removal of the fix for a comma in front of `#elif`/`#else`/`#endif` is now stated by SHARED-TRAIL-004, which the three rule specs neither list under `Uses` nor mention. The shared spec says each rule that uses it follows it, so this is not a contradiction, but the `Uses` rows are now incomplete. | spec author |

## Checked
1. Change doc: ok. A behavior change (fix newly offered) has `sdd/changes/2026-11/trailing-comma-branch-end.md`, Status Accepted, no task list, with `Visible output changes`. Maintainer answer C is recorded under `Decisions`, with options A, B, and C.
2. Specs match the diff: ok apart from #1. SHARED-TRAIL-002 is changed and SHARED-TRAIL-004 is added, in the format (normative sentence, `Why:`, examples with `{|RH5409:,|}` markup). FMT-STRUCT-019 is clarified with behavior as on `main` (answer C), FMT-STRUCT-020 is unchanged, and SHARED-DIAG-008 is clarified. Nothing contradicts the diff: the code withholds only for an `IfDirectiveTrivia`, and the formatter transforms are untouched.
3. IDs: ok. SHARED-TRAIL-004 is the next free number, inserted in sorted position in `requirement-ids.txt`, with no ID reused or removed.
4. Code owners: ok. No owner was renamed, added, or removed; the `sdd/specs/README.md` row for `trailing-commas.md` is unchanged and matches. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198".
5. User documentation: ok. No rule page was touched, and none needs it (the normal case is unchanged; a corner case does not belong on the page). No diagnostic is added.
6. Open questions: ok. The only output question has its answer (C) under `Maintainer answers`. Follow-up candidates: none in any handover.
7. Coverage: ok. SHARED-TRAIL-004 is tagged on 22 tests, SHARED-TRAIL-002 on 4, and the architecture coverage test is green.
8. Tags: ok. Every test in the test implementer's table that I spot-checked (RH5409, RH5410, RH5411, RH5303, formatter tests, `StructuralTransformRequirementTests`) carries its `[Requirement]` tag. Every requirement named under `Examples for the tests` (SHARED-TRAIL-002, SHARED-TRAIL-004, FMT-STRUCT-019, FMT-STRUCT-020) is tagged on at least one added test.
9. Nothing left behind: ok for now. `sdd/work/` holds only this run's directory, which the lead deletes before publishing.
