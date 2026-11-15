Role: spec checker
Commit: 1da79ef
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/fix-layout-scope.md` is `Accepted`, has `Visible output changes`, no task list. Its `Affected requirements` equal the set of requirements the spec diff touches (26, RH5308-006 included). Each change maps to M1–M9 or to an unchanged requirement under `Decisions`.
2. Specs match the diff: ok. Spec diff and code diff agree (kept leading trivia, chain/switch/list-construct narrowing, RH5111/RH5113, RH5308-006 retired). M6 is respected: RH5101/RH5102/RH5417 specs are unchanged from `main`.
3. IDs: ok. No new IDs; `requirement-ids.txt` is unchanged and still holds RH5308-006, which is retired in place.
4. Code owners: ok. `PrecedingTokenFacts` and `RootListPosition` are gone from the pipeline.md owners and `sdd/specs/README.md`. No stale mention in specs or src.
5. User documentation: ok. No rule page is affected; none mentions a statement or member requirement or an attribute withholding.
6. Open questions routed: ok. Output questions from 01–05 are answered as M1–M9; FU1–FU6 sit under `Follow-up candidates`; the RH5308 question was resolved by M9.
7. Coverage: ok. `scripts/test.sh --project architecture` result line: `Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118`. Every named requirement of the "Examples for the tests" has a tagged test in the diff.
8. Test tags: ok. Every added test method carries `[Requirement]` (scripted check; 6 apparent misses were rename-diff artifacts and are tagged). The tab, raw-string and RH2001 tests carry the tags that 06 lists.
9. Nothing left behind: ok. `sdd/work/` holds only `fix-layout-scope`.
