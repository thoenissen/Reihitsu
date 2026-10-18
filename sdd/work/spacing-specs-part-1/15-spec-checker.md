Role: spec checker
Commit: 708dc1b
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing had to be ignored. Nothing but this handover was edited. GitHub was not touched. This checks the change `f2df5ae..708dc1b` against finding 1 of `13-spec-checker.md`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None. | — |

## Checked
1. Change doc: ok. The change narrows two requirements of specs of existing behavior; no intended behavior changes, no change doc is needed.
2. Specs match the diff: ok. Finding 1 of `13-spec-checker.md` is resolved by the text, not only by the handover:
   - RH6001-001 and RH6005-001 now decide by what stands between the keyword and the next token: nothing, or only `/* */` comments that are not documentation comments, with no whitespace and no line break. A `/** */` comment between them satisfies neither alternative, so `if/** c */(…)`, `if/** c */ (…)`, and `operator/** c */ -` are no longer claimed as reported. They stay with thoenissen/Reihitsu#1052 Q1 and RH6005 Q2.
   - RH6005-002 says "directly followed by exactly one space", so `operator/** c */ -` is not claimed as unreported either. RH6001-002 (whitespace or a line break directly after the keyword or after a non-documentation `/* */` comment) is unchanged and does not claim a `/** */` case.
   - The Whys stay true for the claimed inputs: glued keywords get the space from FMT-SPACE-012, and a keyword followed only by `/* */` comments gets it after the last comment (FMT-SPACE-013); the spec author's check of `if/*a*//*b*/(…)` and `operator/*a*//*b*/-(…)` is in `14-spec-author.md`. No example, heading, ID, or `Uses` row changed.
3. IDs: ok. No ID added, retired, or renumbered; ledger and index untouched in this change.
4. Code owners: ok. No owner changed; `SpecConsistencyTests` and `SpecStructureTests` pass.
5. User documentation: ok. No file under `documentation/rules/` changed.
6. Open questions: ok. The change adds none; the left-out inputs are already filed (#1052 Q1, #1044 Q2).
7. Coverage and consistency: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`. `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6001Key|FullyQualifiedName~RH6005Op"`: `Passed!  - Failed:     0, Passed:    18, Skipped:     0, Total:    18`. No C# file changed in this change, and the tests of RH6001-001/-002 and RH6005-001/-002 use no `/** */` comment (`14-spec-author.md`, accepted without re-deriving).
8. Nothing left behind: ok for now. `sdd/work/` holds only `spacing-specs-part-1/`, which the lead deletes before publishing. `scripts/lane-check.sh tooling`: "confirmed — claimed tooling, diff needs tooling". The working tree is clean.
