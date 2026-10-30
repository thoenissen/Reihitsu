Role: spec checker
Commit: bfa2c19
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-10/line-start-fact.md` is required (SHARED-BLANK-008 added, FMT-SPACE-015 retired, FMT-INDENT-002, FMT-DOC-003 and FMT-SPACE-001 changed). It is `Accepted` and has no task list. The amendment passages are marked and approved.
2. Specs match the diff: ok. SHARED-BLANK-008 is in the requirement format with `Why:`, examples and analyzer markup. FMT-INDENT-015 now states block indentation only for a list that a comment or directive keeps wrapped, matching the join (FMT-BREAK-014). RH5203/RH5204/RH6002/SHARED-CHAIN-002 reference SHARED-BLANK-008. FMT-SPACE-015 is retired in place, in the same wording style as the other retired requirements.
3. IDs: ok. SHARED-BLANK-008 is next free and sorted. The FMT-SPACE-015 ledger line is kept.
4. Code owners: ok. `SyntaxTokenPositionUtilities` is added to the SHARED-BLANK header and to `sdd/specs/README.md`. The `AGENTS.md` IndentationPhase row names `LineStartIndex`. The new Pipeline/Core helper types (`LineStartIndex`, `ByteOrderMarkUtilities`, `TokenContainmentUtilities`, `TriviaListUtilities`) are internal helpers no spec names as owner, and none of the types the diff deleted or renamed is named in a spec. `SpecConsistencyTests` pass.
5. User documentation: ok. No `documentation/rules/` page is affected, and the change doc states why.
6. Open questions: ok. The spec author's handovers 01, 03 and 07 list none, and the escalations are recorded under `squad/decisions/`.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 89, Skipped: 0, Total: 89`. SHARED-BLANK-008 has 19 covering tests, and no test tags the retired FMT-SPACE-015.
8. Tags: ok. Tests in the diff carry the requirements in the test implementer's tables (04, 08). Every requirement under the spec author's `Examples for the tests` (SHARED-BLANK-008, FMT-INDENT-002, FMT-DOC-003, FMT-SPACE-001, FMT-INDENT-015, FMT-INDENT-006, FMT-BREAK-014) is tagged on at least one added or retagged test, as are RH5204-001, RH5203-001, RH6002-001/-002, RH5201-002, RH5048-001 and SHARED-CHAIN-002.
9. Nothing left behind: ok. `sdd/work/` holds only `line-start-fact/`, this run's, and `origin/main` has none.
